//display
function GameConnection::DisplayMobaHud(%client)
{
    %player = %client.player;

    if (!isObject(%player))
    {
        return;
    }

    %curhealth = 100;
    %maxHealth = 100;

    %healthText = "\c2H " @ makeMobaValueBar(%curhealth,%maxHealth,"|||||||||||||||||||");

    %curMana = 100;
    %maxMana = 100;

    %manaText = "\c4M " @ makeMobaValueBar(%curMana,%maxMana,"|||||||||||||||||||");

    %client.bottomPrint(%healthText @ "<just:center>\c3Ultimate" @ "<just:right>\c5Level: 25" @ "<br><just:left>" @ %manaText @ "<just:right>\c1Exp: 1000/1000" @ "<br><just:left>\c21000\c6/\c01000" @ "<just:right>\c3Coins: 10000",100,true);
}

package mobaScript 
{
    function makeMobaValueBar(%curValue,%maxValue,%barText,%color)
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
};
deactivatePackage("mobaScript");
activatePackage("mobaScript");