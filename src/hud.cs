//display
function GameConnection::DisplayMobaHud(%client)
{
    %minigame = getMinigameFromObject(%client);
    if(%minigame <= 0)
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
    
    %maxHealth = mFloor(%player.getMaxHealth());
    %curhealth = mFloor(%player.getHealthLevel());
    %healthText = "\c2H " @ makeMobaValueBar(%curhealth,%maxHealth,"|||||||||||||||||||");

    %curMana = 100;
    %maxMana = 100;
    %manaText = "\c4M " @ makeMobaValueBar(%curMana,%maxMana,"|||||||||||||||||||");

    %ultimate = "\c3Ultimate";
    %ulitmateText = %ultimate;

    %level = getHudElement(%client,"level");
    %levelText = "\c5Level:" SPC %level;

    %exp = getHudElement(%client,"exp");
    %expThreshold = getHudElement(%client,"expThreshold");
    %expText = "\c1Exp:" SPC %exp @ "/" @ %expThreshold;

    %lastHits = getHudElement(%client,"lastHits");
    %denies = getHudElement(%client,"denies");
    %ldText = "\c2" @ %lastHits @ "\c6/\c0" @ %denies;

    %coins =  getHudElement(%client,"gold");
    %coinsText = "\c3Coins:" SPC %coins;

    %client.bottomPrint(%healthText @ "<just:center>" @ %ulitmateText @ "<just:right>" @ %levelText @ "<br><just:left>" @ %manaText @ "<just:right>" @ %expText @ "<br><just:left>" @ %ldText @ "<just:right>" @ %coinsText,-1,true);
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
    %currenthealth = mFloor(%maxHealth - %player.getDamageLevel());
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

package mobaHud 
{
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
            %client.schedule(100,"DisplayMobaHud");
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