registerOutputEvent("Player","WeaponFire","string 200 100" TAB "vector" TAB "string 200 100" TAB "string 200 100",true);
registerOutputEvent("GameConnection","WeaponCooldown","string 200 100");
registerOutputEvent("GameConnection","WeaponDamage","string 200 100");
registerOutputEvent("GameConnection","WeaponProjectile","Datablock ProjectileData");

$Server::Moba::WeaponFireAngle = 30;
$Server::Moba::LockonRange = 20;
//event that fires the weapon projectile only if it is off cooldown
function Player::WeaponFire(%player,%speed,%variable,%scale,%lockon,%client)
{
    %scale += 0;
	%speed += 0;
	%lockon += 0;
    if(%client.activeWeaponCooldown <= getSimTime() && %client.weaponProjectile)
    {
		%target = getTarget(%player,$Server::Moba::LockonRange,$Server::Moba::WeaponFireAngle,false);
		if(%lockon && %target)
		{
			%proj = %player.spawnHomingWeaponProjectile(%speed,%client.weaponProjectile,%variable,%scale,%target);
		}
		else
		{
			%proj = %player.SpawnWeaponProjectile(%speed,%client.weaponProjectile,%variable,%scale,%client);
		}

        
        %client.activeWeaponCooldown = getSimTime() + %client.weaponCooldown;
        %proj.isweaponProjectile = true;
    }

}
//event that sets the cooldown
function GameConnection::WeaponCooldown(%client,%cooldown)
{
    %cooldown = %cooldown + 0;
    %oldCooldown = %client.weaponCooldown;
    %client.activeWeaponCooldown += %cooldown - %oldCooldown;
    %client.weaponCooldown = %cooldown;
}
//event that sets the damage
function GameConnection::WeaponDamage(%client,%damage)
{
    %damage = %damage + 0;
    %client.weaponDamage = %damage;
}
//event that sets the projectile
function GameConnection::WeaponProjectile(%client,%projectile)
{
    %client.weaponProjectile = %projectile;
}

function Player::SpawnWeaponProjectile(%player, %speed, %projectileData, %variance, %scale, %client)
{
    if (!isObject (%projectileData))
	{
		return;
	}
	%velocity = VectorScale (%player.getEyeVector (), %speed);
	%pos = %player.getEyePoint ();
	%velx = getWord (%velocity, 0);
	%vely = getWord (%velocity, 1);
	%velz = getWord (%velocity, 2);
	%varx = getWord (%variance, 0);
	%vary = getWord (%variance, 1);
	%varz = getWord (%variance, 2);
	%x = (%velx + (%varx * getRandom ())) - %varx / 2;
	%y = (%vely + (%vary * getRandom ())) - %vary / 2;
	%z = (%velz + (%varz * getRandom ())) - %varz / 2;
	%muzzleVelocity = %x SPC %y SPC %z;
	%p = new Projectile ("")
	{
		dataBlock = %projectileData;
		initialVelocity = %muzzleVelocity;
		initialPosition = %pos;
		sourceClient = %client;
		sourceObject = %player;
		client = %client;
	};
	if (%p)
	{
		MissionCleanup.add (%p);
		%p.setScale (%scale SPC %scale SPC %scale);
	}
    return %p;
}

function Player::spawnHomingWeaponProjectile(%this,%speed,%projectile,%spread,%scaleFactor,%target)
{

	if(!isObject(%projectile) || %projectile.getClassName() !$= "ProjectileData")
		return;

	%obj = -1;
	if(!isObject(%target))
    {
        return;
    }
    %obj = %target;
	if(%obj.getType() & $TypeMasks::PlayerObjectType)
	{
		%pos = %obj.getHackPosition();
	}
	else
	{
		%pos = %obj.getPosition();
	}

	%start = %this.getHackPosition();
	
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
	
	%vel = vectorAdd(vectorScale(%vec,%speed),%random);
	
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

	return(%p);
}

package mobaWeapon
{
    function ShapeBase::Damage(%this, %sourceObject, %position, %damage, %damageType)
    {
        %isWeaponProjectile = %sourceObject.isweaponProjectile;
        %client = findClientByBL_ID(getBL_IDFromObject(%sourceObject));

        if(%isWeaponProjectile)
        {
            %damage = %client.weaponDamage;
        }

        parent::Damage(%this, %sourceObject, %position, %damage, %damageType);
    }
};
deactivatePackage("mobaWeapon");
activatePackage("mobaWeapon");