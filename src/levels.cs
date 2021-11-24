$Server::Moba::StartingThreshold = 230;
$Server::Moba::ThresholdAddition = (3500 - $Server::Moba::StartingThreshold) / $Server::Moba::MaxLevel;
$Server::Moba::MaxLevel = 25;
$Server::Moba::HealthBase = 100;
$Server::Moba::ManaBase = 100;
$Server::Moba::StartingCoins = 200;

function Slayer_Moba::onMinigameReset(%this, %client)
{
    %minigame = getMinigameFromObject(%client);
    %numMembers = %minigame.numMembers;

    for(%i = 0; %i < %numMembers; %i++)
    {
        %client = %minigame.member[%i];
        schedule(100,%client,"resetPlayer",%client);
    }
}


package MobaLevels
{
    function resetPlayer(%client)
    {
        setHudElement(%client,"level",1);
        setHudElement(%client,"exp",0);
        setHudElement(%client,"denies",0);
        setHudElement(%client,"lasthits",0);
        setHudElement(%client,"healthLevel",1);
        setHudElement(%client,"manaLevel",1);
        setHudElement(%client,"healthLevelIncrease",1);
        setHudElement(%client,"manaLevelIncrease",1);
        setHudElement(%client,"gold",200);
        setHudElement(%client,"expthreshold",$Server::Moba::StartingThreshold);

        updateStats(%client);
    }

    function checkLevelUp(%client)
    {
        %level = getHudElement(%client,"level");
        %exp = getHudElement(%client,"exp");
        %expthreshold = getHudElement(%client,"expthreshold");

        while(%exp >= %expThreshold)
        {
            %expThresholdGain = %level * $Server::Moba::ThresholdAddition;
            
            gainHudElement(%client,"expThreshold",%expThresholdGain);
            %expThreshold += %expThresholdGain;

            levelUp(%client);
        }
    }   

    function levelUp(%client)
    {
        %level = getHudElement(%client,"level");

        if(%level < 25)
        {
            gainHudElement(%client,"level",1);
            gainHudElement(%client,"healthLevel",1);
            gainHudElement(%client,"manaLevel",1);

            updateStats(%client);
        }
    }

    function updateStats(%client)
    {
        %healthLevel = getHudElement(%client,"healthLevel");
        %manaLevel = getHudElement(%client,"manaLevel");

        %player = %client.player;

        if(!isObject(%player))
        {
            return;
        }

        %player.setMaxHealth($Server::Moba::HealthBase * %healthLevel * getHudElement(%client,"healthLevelIncrease"));
        %player.SetMaxEnergyLevel($Server::Moba::ManaBase * %manaLevel * getHudElement(%client,"manaLevelIncrease"));
        %client.DisplayMobaHud();
    }

    function ShapeBase::setMaxHealth(%this, %maxHealth)
    {
        if(!(%this.getType() & $TypeMasks::PlayerObjectType))
        {
            return parent::setMaxHealth(%this, %maxHealth);
        }

        if(!isObject(%this))
            return -1;

        if(%maxHealth <= 0)
            return false;

        %this.maxHealth = mClampF(%maxHealth, 1, 999999);

        %oldHealth = %this.oldHealth;

        if(!%oldHealth)
        {
            %oldHealth = %this.maxHealth;
        }

        if(!%this.health)
        {
            %this.health = %this.maxHealth;
        }

        %this.setHealth(%this.health + %this.maxHealth - %oldHealth);
        %this.oldMaxHealth = %this.maxHealth;
        %this.oldHealth = %this.health;
        

        return true;
    }

    function Armor::onAdd(%this, %obj)
    {
        %client = %Obj.client;        

        if(%client)
        {
            schedule(100,0,"updateStats",%client);
        }

        parent::onAdd(%this, %obj);
    }
};
deactivatePackage("MobaLevels");
activatePackage("MobaLevels");