registerOutputEvent("fxDTSBrick","TowerStart","string 200 100" TAB "int 0 9999999 10" TAB "int 33 9999999 1000" TAB "datablock ProjectileData");
registerOutputEvent("fxDTSBrick","TowerStop");

function fxDTSBrick::TowerStart(%brick, %teamOwner, %radius, %delay, %projectile)
{
    if(%brick.TowerGoing)
    {
        %brick.towerStop();
    }

    %brick.TowerGoing = true;
    %brick.TowerTeamOwner = %teamOwner;
    %brick.towerDelay = %delay;
    %brick.towerRadius = %radius;
    %brick.towerProjectile = %projectile;
    %brick.TowerSchedule();   
}
function fxDTSBrick::TowerStop(%brick)
{
    if(%brick.TowerGoing)
    {
        %brick.TowerGoing = false;
        cancel(%brick.TowerSchedule);
    }
}

function fxDTSBrick::TowerSchedule(%brick)
{
    cancel(%brick.TowerSchedule);

    %minigame = getMinigameFromObject(%brick);
    %owningTeam = %brick.TowerTeamOwner;
    %radius = %brick.towerRadius;

    %targetDistance = %radius + 1;

    if(%minigame > 0)
    {
        %towerTarget = %brick.towerTarget;
        if(%towerTarget $= "")
        {
            %towerPos = %brick.getPosition();
            InitContainerRadiusSearch( %towerPos,%radius,$TypeMasks::PlayerObjectType);
            
            while(%next = ContainerSearchNext())
            {
                if(%next.getClassName() $= "Player")
                {
                    %teamName = %next.client.getTeam().name;
                }
                else
                {
                    %teamName = %next.getTeam().name;
                }
                
                if(%owningTeam !$= %teamName)
                {
                    if(%next)
                    {
                        %playerPos = %next.getHackPosition();
                        %distance = ContainerSearchCurrDist();
                        if(%targetDistance > %distance)
                        {   
                            %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::PlayerObjectType | $TypeMasks::FxBrickObjectType;
                            %hit = ContainerRayCast(%towerPos,%playerPos,%mask,%brick);
                            %hit = getWord(%hit,0);

                            if(!(%hit.getType() & $TypeMasks::PlayerObjectType))
                            {
                                continue;
                            }

                            %towerTarget = %next;
                            %targetDistance = %distance;
                        }
                    }
                }   

            }

            %brick.towerTarget = %towerTarget;
        }
        else
        {
            %player = %towerTarget;
            if(%player)
            {
                %playerPos = %player.getHackPosition();
                %towerPos = %brick.getPosition();

                %mask = $TypeMasks::StaticShapeObjectType | $TypeMasks::PlayerObjectType | $TypeMasks::FxBrickObjectType;
                %hit = ContainerRayCast(%towerPos,%playerPos,%mask,%brick);
                %hit = getWord(%hit,0);

                if(!(%hit.getType() & $TypeMasks::PlayerObjectType))
                {
                    %brick.towerTarget = "";
                }
                else
                {
                    %distance = vectorDist(%playerPos,%towerPos);
                    if(%distance <= %radius)
                    {
                        %brick.spawnTowerHomingProjectile(%brick.towerProjectile,"1 1 1","1",%towerTarget);
                    }
                    else
                    {
                        %brick.towerTarget = "";
                    }
                }
            }   
            else
            {
                %brick.towerTarget = "";
            }
        }
    }

    if(%brick.TowerGoing)
    {
        %brick.TowerSchedule = %brick.schedule(%brick.TowerDelay,"TowerSchedule");
    }
}

function fxDTSBrick::spawnTowerHomingProjectile(%this,%projectile,%spread,%scaleFactor,%target)
{
	if(%this.isDead() || %this.isFakeDead() || !%this.isRaycasting())
		return;
	
	if(!isObject(%projectile) || %projectile.getClassName() !$= "ProjectileData")
		return;
	
	%center = %this.getWorldBoxCenter();
	%x = (%this.dataBlock.brickSizeX * 0.5 / 2) - 0.5;
	%y = (%this.dataBlock.brickSizeY * 0.5 / 2) - 0.5;
	%z = (%this.dataBlock.brickSizeZ * 0.2 / 2) - 0.5;
	
	%x = (%x > 0 ? %x : 0);
	%y = (%y > 0 ? %y : 0);
	%z = (%z > 0 ? %z : 0);
	
	%x = %x - (%x * getRandom() * 2);
	%y = %y - (%y * getRandom() * 2);
	%z = %z - (%z * getRandom() * 2);
	
	%v = %x SPC %y SPC %z;
	
	%start = vectorAdd(%center,%v);
	
	%obj = -1;

    if(!isObject(%target))
    {
        return;
    }
    %obj = %target;
    %pos = %obj.getHackPosition();

	
	%x = getWord(%spread,0);
	%y = getWord(%spread,1);
	%z = getWord(%spread,2);
	
	%x = %x - (%x * getRandom() * 2);
	%y = %y - (%y * getRandom() * 2);
	%z = %z - (%z * getRandom() * 2);
	
	%random = %x SPC %y SPC %z;
	
	if(%this.lastFireHomingObject == %obj && (getSimTime() - %this.lastFireHomingTime) < 1000)
	{
		%delta = vectorSub(%obj.getVelocity(),%this.lastFireHomingVel);
		%accl = vectorScale(%delta,100/(getSimTime() - %this.lastFireHomingTime));
	}
	else
	{
		%accl = 0;
	}
	
	%end = %pos;
	%vec = getProjectileAimVector(%start,%end,%projectile);
	
	//Approximate the travel time. This assumes that the time to the adjusted point is the same as the time to the object position
	//... which it isn't. If you're moving slowly it shouldn't make much of a difference.
	for(%i=0;%i<5;%i++)
	{
		%t = vectorDist(%start,%end) / vectorLen(vectorScale(getWord(%vec,0) SPC getWord(%vec,1),%projectile.muzzleVelocity));
		%velaccl = vectorScale(%accl,%t);
		
		%x = getword(%velaccl,0);
		%y = getword(%velaccl,1);
		%z = getWord(%velaccl,2);
		
		%x = (%x < 0 ? 0 : %x);
		%y = (%y < 0 ? 0 : %y);
		%z = (%z < 0 ? 0 : %z);
		
		%vel = vectorAdd(vectorScale(%obj.getVelocity(),%t),%x SPC %y SPC %z);
		%end = vectorAdd(%pos,%vel);
		%vec = getProjectileAimVector(%start,%end,%projectile);
	}	
	
	%vel = vectorAdd(vectorScale(%vec,%projectile.muzzleVelocity),%random);
	
	%this.lastFireHomingObject  = %obj;
	%this.lastFireHomingTime    = getSimTime();
	%this.lastFireHomingVel     = %obj.getVelocity();
	
	//I can't use ::spawnProjectile to create it because the start place might be different on a non-1x1 brick
	
	%p = new Projectile()
	{
		dataBlock = %projectile;
		initialPosition = %start;
		initialVelocity = %vel;
		sourceObject = %this;
		client = %client;
		sourceClient = %client;
		sourceSlot = 0;
		originPoint = %start;
	};
	
	//Projectile quota
	if(!isObject(%p))
		return;
	
	MissionCleanup.add(%p);
	%p.setScale(%scaleFactor SPC %scaleFactor SPC %scaleFactor);
}