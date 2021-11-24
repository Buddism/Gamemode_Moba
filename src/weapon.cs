registerOutputEvent("Player","WeaponFire","string 200 100" TAB "vector" TAB "string 200 100" TAB "string 200 100",true);
registerOutputEvent("GameConnection","WeaponCooldown","string 200 100");
registerOutputEvent("GameConnection","WeaponDamage","string 200 100");
registerOutputEvent("GameConnection","WeaponProjectile","Datablock ProjectileData");

$Server::Moba::WeaponFireAngle = 30;
$Server::Moba::WeaponFireRange = 30;
//event that fires the weapon projectile only if it is off cooldown
function Player::WeaponFire(%player,%speed,%variable,%scale,%lockon,%client)
{
    %scale += 0;
	%speed += 0;
	%lockon += 0;
    if(%client.activeWeaponCooldown <= getSimTime() && %client.weaponProjectile)
    {
		if(%lockon)
		{
			
		}

        %proj = %player.SpawnWeaponProjectile(%speed,%client.weaponProjectile,%variable,%scale,%client);
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