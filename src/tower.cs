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
                        if(%distance <= %radius && %targetDistance > %distance)
                        {   
                            %mask = $TypeMasks::StaticObjectType | $TypeMasks::PlayerObjectType | $TypeMasks::FxBrickObjectType;
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

                %mask = $TypeMasks::StaticObjectType | $TypeMasks::PlayerObjectType | $TypeMasks::FxBrickObjectType;
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
                        %brick.spawnHomingProjectile(0,%brick.towerProjectile,"1 1 1","1",%towerTarget);
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