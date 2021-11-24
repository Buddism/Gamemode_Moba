registerOutputEvent("GameConnection", "setAbilityCooldown", "string 100 140" TAB "int 0 999999999 5000");

//display
function GameConnection::DisplayMobaHud(%client)
{
    %minigame = getMinigameFromObject(%client);
    if(!isObject(%minigame) || %client.mobaHudEnabled)
    {
        %client.BottomPrint("",0,true);
        return;
    }

    %varGroup = nameToId("VariableGroup_" @ %minigame .creatorBLID);

    %player = %client.player;
    if (!isObject(%player))
    {
        return;
    }

    //center print
    %abilityCount = %client.abilityCount;
    %product = "";
    for(%i = 0; %i < %abilityCount; %i++)
    {  
        %ability = %client.ability[%i,N];
        if(%ability !$= "Ultimate")
        {
            %cooldown = %client.ability[%i,C];

            %product = %product @ "\c7" @ mFloor((%cooldown - getSimTime()) / 1000) SPC ":" SPC %ability @ "<br>";
        }
    }

    %client.centerprint("<just:right>" @ %product,0);
    
    //bottom print
    %maxHealth = mFloor(%player.getMaxHealth());
    %curhealth = mFloor(%player.getHealth());
    talk(%maxHealth SPC %curhealth SPC "Hud");
    %healthText = "\c2H " @ makeMobaValueBar(%curhealth,%maxHealth,"|||||||||||||||||||");

    %maxMana = mFloor(%client.leveling_maxenergy);
    %curMana = mFloor(%player.GetEnergyLevel());
    %manaText = "\c4M " @ makeMobaValueBar(%curMana,%maxMana,"|||||||||||||||||||");

    %level = getHudElement(%client,"level");

    if(%level >= 6)
    {
        %ultimateCooldown = %client.ability["Ultimate",N];
        if(%ultimateCooldown)
        {
            %ultimate = "\c7" @ mFloor((%ultimateCooldown - getSimTime()) / 1000) @ " : Ultimate";
        }
        else
        {
            %ultimate = "\c3Ultimate Ready";
        }
        %ulitmateText = %ultimate;
    }
    

    
    %levelText = "\c5Level:" SPC %level;

    %exp = mFloor(getHudElement(%client,"exp"));
    %expThreshold = mFloor(getHudElement(%client,"expThreshold"));
    %expText = "\c1Exp:" SPC %exp @ "/" @ %expThreshold;

    %lastHits = getHudElement(%client,"lastHits");
    %denies = getHudElement(%client,"denies");
    %ldText = "\c2" @ %lastHits @ "\c6/\c0" @ %denies;

    %coins =  getHudElement(%client,"gold");
    %coinsText = "\c3Coins:" SPC %coins;

    %client.bottomPrint(%healthText @ "<just:center>" @ %ulitmateText @ "<just:right>" @ %levelText @ "<br><just:left>" @ %manaText @ "<just:right>" @
												%expText @ "<br><just:left>" @ %ldText @ "<just:right>" @ %coinsText,-1,true);
}

function Player::UpdateMobaShapeName(%player)
{
    if(getMinigameFromObject(%player) <= 0)
    {
        return;
    }

    //bot or player?
    %isBot = %player.isHoleBot;

    %maxHealth = mFloor(%player.getMaxHealth());
    %currenthealth = mFloor(%player.getHealth());
    %client = %player.client;

    if(%client)
    {
        %teamColor = %client.getTeam().colorRGB;
    }
    else
    {
        %teamColor = %player.getTeam().colorRGB;
    }

    if(%teamcolor $= "")
    {
        %teamColor = "1 1 1";
    }

    if(%isBot)
    {
        %player.setShapeName(%currentHealth @ "/" @ %maxHealth, 8564862);
        %player.setShapeNameColor(vectorScale(%teamColor, (%currentHealth / %maxHealth)));
    }
    else
    {
        %client = %player.client;

        %name = %client.getPlayerName();
        %level = getHudElement(%client,"level");
        %player.setShapeName( %name SPC "|" SPC %currentHealth @ "/" @ %maxHealth SPC "|" SPC "Lvl: " @ %level, 8564862);
    }
}

function GameConnection::AbilityCooldown(%client)
{
    cancel(%client.abilityCooldownLoop[%name]);

    %client.DisplayMobaHud();

    %count = %client.abilityCount;
    if(%count > 0)
    {
        %client.abilityCooldownLoop = %client.schedule(100,"AbilityCooldown",%name);
    }
}

function GameConnection::setAbilityCooldown(%client,%name,%cooldown)
{
    if(%client.ability[%name,N])
    {
        %client.removeAbilityCooldown(%name);
    }
    
    %count = %client.abilityCount;

    %client.ability[%name,N] = %cooldown + getSimTime();
    %client.ability[%name,C] = %count;
    
    %client.ability[%count,C] = %client.ability[%name,N];
    %client.ability[%count,N] = %name;

    %client.abilityCount++;

    %client.abilityCooldownLoop[%name] =  %client.schedule(0,"AbilityCooldown",%name);

    cancel(%client.abilityCooldownStop[%name]);
    %client.abilityCooldownStop[%name] =  %client.schedule(%cooldown,"removeAbilityCooldown",%name);
}

function GameConnection::removeAbilityCooldown(%client,%name)
{
    cancel(%client.abilityCooldownStop[%name]);

    %num = %client.ability[%name,C];
    %count = %client.abilityCount;

    %client.ability[%name,N] = "";
    %client.ability[%name,C] = "";

    for(%i = %num + 1; %i < %count; %i++)
    {
        %client.ability[%i - 1,C] = %client.ability[%i,C];
        %client.ability[%i - 1,N] = %client.ability[%i,N];

        %currName = %client.ability[%i,N];
        %client.ability[%currName,C] = %i - 1;

        %client.ability[%i,C] = "";
        %client.ability[%i,N] = "";
    }

    %client.abilityCount--;

    %client.DisplayMobaHud();
}

package mobaHud 
{
		function GameConnection::mobaHudLoop(%cl)
		{
			cancel(%cl.hudLoop);
			
			%minigame = getMinigameFromObject(%cl);
			if(!isObject(%minigame) || %cl.mobaHudEnabled)
			{
					%cl.BottomPrint("",0,true);
					return;
			}
			
			// %lvl = getHudElement(%cl, "level");
			// %exp = getHudElement(%cl, "exp");
			// %max = getHudElement(%cl, "expThreshold");

			// if(%exp >= %max)
			// {
			// 	gainHudElement(%cl, "level", 1);
			// 	gainHudElement(%cl, "exp", %exp*-1);
			// }

			checkLevelUp(%cl);

			%cl.DisplayMobaHud();

			%cl.hudLoop = %cl.schedule(200, mobaHudLoop);
		}

    function getHudElement(%client,%name)
    {
        %minigame = getMinigameFromObject(%client);
        if(%minigame <= 0)
        {
            %client.BottomPrint("",0,true);
            return "";
        }

        %varGroup = nameToId("VariableGroup_" @ %minigame .creatorBLID);
        if(%varGroup <= 0)
        {
            %client.BottomPrint("",0,true);
            return "";
        }

        %current = %varGroup.getVariable(%name,%client);

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
        if(%varGroup <= 0)
        {
            %client.BottomPrint("",0,true);
            return "";
        }

        %varGroup.setVariable(%name,%ammount,%client);

        %client.DisplayMobaHud();
    }

    function gainHudElement(%client,%name,%ammount)
    {
        %current = getHudElement(%client,%name);

        setHudElement(%client,%name,%current + %ammount);
    }

    function makeMobaValueBar(%curValue,%maxValue,%barText)
    {
        %valueText = %curValue @ "/" @ %maxValue;
        %valueTextLength = strLen(%valueText);
        %barTextLength = strLen(%barText);
        
        %valueText = getSubStr(%valueText,0,mClamp(%valueTextLength,0,%barTextLength));
        %valueTextLength = strLen(%valueText);

        %barText = getSubStr(%barText,%valueTextLength,%barTextLength - %valueTextLength);

        %tempText = %valueText @ %barText;
        %tempTextLength = strLen(%tempText);

        %availibleValue = mCeil((%curValue / %maxValue) * %tempTextLength);

        %availibleText = getSubStr(%tempText,0,%availibleValue);
        %consumedText = getSubStr(%tempText,%availibleValue, %tempTextLength - %availibleValue);


        %valueBar = %availibleText @ "\c7" @ %consumedText;

        return %valueBar;
    }

    function Armor::onAdd(%this, %obj)
    {
        %client = %Obj.client;        

        if(%client)
        {
            %client.schedule(100,"mobaHudLoop");
            %obj.schedule(100,"UpdateMobaShapeName");
        }

        parent::onAdd(%this, %obj);
    }

    function fxDTSBRICK::onBotDeath(%obj)
    {
        %obj.hBot.setShapeName("",8564862);

        parent::onBotDeath(%obj);        
    }

    function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
    {
        %client.player.UpdateMobaShapeName();

        parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);
    }

    function Armor::onDamage(%this, %obj, %delta)
    {
        %obj.UpdateMobaShapeName();

        %client = %obj.client;

        if(%client)
        {
            %client.DisplayMobaHud();
        }
        
        parent::onDamage(%this, %obj, %delta);
    }
};
deactivatePackage("mobaHud");
activatePackage("mobaHud");