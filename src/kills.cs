$Server::Moba::DenyThreshold = 0.85;
$Server::Moba::GoldPerHealthPoint = 0.25;
$Server::Moba::ExperienceArea = 100;
$Server::Moba::CreepHealthXP = 0.35;
$Server::Moba::HeroBaseXP = 100;
$Server::Moba::HeroXPPerXP = 7.69;

function Slayer_Moba::minigameCanDamage(%minigame,%objA, %objB)
{   
    %client = %objA;
    %other = %objB;
    if(%client.getClassName() $= "GameConnection" && (%other.getType() & ($TypeMasks::PlayerObjectType | $TypeMasks::StaticShapeObjectType)))
    {
        %clientTeam = %client.getTeam().name;
        %otherTeam = %other.getTeam().name;
        

        if(%clientTeam $= %otherTeam && %other.getType() & $TypeMasks::PlayerObjectType && %client.player)
        {
            %percent = %other.getDamagePercent();

            //are we within threshold?
            if($Server::Moba::DenyThreshold <= %percent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if(%clientTeam !$= %otherTeam)
        {
            return true;
        }
    }
    return true;
    
}

package mobaKills
{
    function giveAreaXp(%team,%epicenter,%ammount,%inverse)
    {
        %poolC = 0;
        %minigame = %team.minigame;
        %numMembers = %minigame.numMembers;
        for(%i = 0; %i < %numMembers; %i++)
        {
            %client = %minigame.member[%i];
            if((!%inverse && (%client.getTeam() $= %team)) || (%inverse && (%client.getTeam() $= %team)))
            {
                %player = %client.player;

                if(!isObject(%player))
                {
                    continue;
                }

                %distance = vectorDist(%epicenter, %player.getPosition());
                if(%distance <= $Server::Moba::ExperienceArea)
                {
                    %pool[%poolC] = %client;
                    %poolC++;
                }
            }
        }

        for(%i = 0; %i < %poolC; %i++)
        {
            %client = %pool[%i];
            gainHudElement(%client,"exp",mFloor(%ammount / %poolC));
            checkLevelUp(%client);
        }
    }

    function creepKilled(%sourceClient,%creep)
    {  
        %clientTeam = %sourceClient.getTeam().name;
        %creepTeam = %creep.getTeam().name;

        %creepHealth = %creep.getmaxHealth();

        if(%clientTeam $= %creepTeam)
        {   
            %expGain = (%creepHealth * $Server::Moba::CreepHealthXP) / 2;

            giveAreaXp(%sourceClient.getTeam(),%creep.getPosition(), %expGain,true);
            gainHudElement(%sourceClient,"denies",1);
        }
        else
        {
            %goldGain = %creepHealth * $Server::Moba::GoldPerHealthPoint;
            %expGain = %creepHealth * $Server::Moba::CreepHealthXP;
            gainHudElement(%sourceClient,"gold",%goldGain);
            giveAreaXp(%sourceClient.getTeam(),%creep.getPosition(),%expGain);
            gainHudElement(%sourceClient,"lastHits",1);
        }
    }

    function playerKilled(%sourceClient,%client,%player)
    {
        %clientTeam = %sourceClient.getTeam().name;
        %otherTeam = %client.getTeam().name;

        if(%clientTeam $= %otherTeam)
        {
            gainHudElement(%sourceClient,"denies",1);
        }
        else
        {
            %position = %player.getPosition();

            %maxHealth = %player.getMaxHealth();
            %exp = getHudElement(%client,"exp");

            giveAreaXp(%sourceClient.getTeam(),%position,%exp / $Server::Moba::HeroXPPerXP + $Server::Moba::HeroBaseXP);
        }
        
    }

    function fxDTSBrick::onBotDeath(%obj)
    {
        
        %bot = %obj.hBot;
        %sourceClient = %bot.hKiller;
        if(%sourceClient.getClassName() $= "GameConnection")
        {
            creepKilled(%sourceClient,%bot);
        }

        parent::onBotDeath(%obj,%sourceClient);
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

				if(%this.health <= 0)
					%this.health = %this.maxHealth;
				else
        	%this.addHealth(%this.maxHealth - %this.oldMaxHealth);

        %this.oldMaxHealth = %this.maxHealth;
        %this.oldHealth = %this.health;
        

        return true;
    }

    function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
    {
        if(isObject(%sourceClient))
        {
            if(%client != %sourceClient && %sourceClient.getClassName() $= "GameConnection")
            {
                playerKilled(%sourceClient,%client,%client.player);
            }
        }

        parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);
    }
};
deactivatePackage("mobaKills");
activatePackage("mobaKills");