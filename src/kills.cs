$Server::Moba::BotDenyThreshold = 0.85;
$Server::Moba::GoldPerHealthPoint = 0.25;
$Server::Moba::ExperienceArea = 100;

function Slayer_Moba::minigameCanDamage(%minigame,%objA, %objB)
{   
    %client = %objA;
    %bot = %objB;
    if(%client.getClassName() $= "GameConnection" && %bot.getClassName() $= "AIPlayer")
    {
        %clientTeam = %client.getTeam().name;
        %botTeam = %bot.hType;

        if(%clientTeam $= %botTeam)
        {
            %percent = %bot.getDamagePercent();

            //are we within threshold?
            if($Server::Moba::BotDenyThreshold <= %percent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }
    return true;
    
}

package mobaKills
{
    function giveAreaXp(%team,%epicenter,%ammount)
    {
        %poolC = 0;
        %minigame = %team.minigame;
        %numMembers = %minigame.numMembers;
        for(%i = 0; %i < %numMembers; %i++)
        {
            %client = %minigame.member[%i];
            if(%client.getTeam() $= %team)
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

    function getHudElement(%client,%name)
    {
        %minigame = getMinigameFromObject(%client);
        if(%minigame <= 0)
        {
            %client.BottomPrint("",0,true);
            return;
        }

        %varGroup = nameToId("VariableGroup_" @ %minigame .creatorBLID);

        %current = %varGroup.getVariable("Client",%name,%client);

        return %current;
    }

    function setHudElement(%client,%name,%ammount)
    {
        %minigame = getMinigameFromObject(%client);
        if(%minigame <= 0)
        {
            %client.BottomPrint("",0,true);
            return;
        }

        %varGroup = nameToId("VariableGroup_" @ %minigame .creatorBLID);

        %varGroup.setVariable("Client",%name,%ammount,%client);

        %client.DisplayMobaHud();
    }

    function gainHudElement(%client,%name,%ammount)
    {
        %current = getHudElement(%client,%name);

        setHudElement(%client,%name,%current + %ammount);
    }

    function creepKilled(%sourceClient,%creep)
    {  
        %clientTeam = %sourceClient.getTeam().name;
        %creepTeam = %creep.getTeam().name;

        %creepHealth = %creep.getmaxHealth();

        if(%clientTeam $= %creepTeam)
        {
            gainHudElement(%sourceClient,"denies",1);
        }
        else
        {
            %goldGain = %creepHealth * $Server::Moba::GoldPerHealthPoint;
            gainHudElement(%sourceClient,"gold",%goldGain);
            giveAreaXp(%sourceClient.getTeam(),%creep.getPosition(),100);
            gainHudElement(%sourceClient,"lastHits",1);
        }
    }

    function playerKilled(%sourceClient,%client)
    {
        gainHudElement(%sourceClient,"lasthits",1);
        //TODO: add gold and xp gain
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

    function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
    {
        if(%client != %sourceClient && %sourceClient.getClassName() $= "GameConnection")
        {
            playerKilled(%sourceClient,%client);
        }

        parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);
    }
};
deactivatePackage("mobaKills");
activatePackage("mobaKills");