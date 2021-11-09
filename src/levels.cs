$Server::Moba::StartingThreshold = 100;
$Server::Moba::ThresholdMultiplier = 1.5;
$Server::Moba::HealthMultiplier = 1.5;
$Server::Moba::ManaMultiplier = 1.5;
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
        setHudElement(%client,"gold",0);
        setHudElement(%client,"expthreshold",$Server::Moba::StartingThreshold);

        updateStats(%client);
    }

    function checkLevelUp(%client)
    {
        %exp = getHudElement(%client,"exp");
        %expthreshold = getHudElement(%client,"expthreshold");

        while(%exp >= %expThreshold)
        {
            gainHudElement(%client,"exp",-%expThreshold);
            %exp -= %expthreshold;

            %expThresholdGain = mFloor(%expThreshold * $Server::Moba::ThresholdMultiplier);
            
            setHudElement(%client,"expThreshold",%expThresholdGain);
            %expThresholdGain += %expThreshold;

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

        %player.setMaxHealth(100 * %healthLevel * $Server::Moba::HealthMultiplier);
        %client.DisplayMobaHud();
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