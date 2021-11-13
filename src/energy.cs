registerOutputEvent("Player", "SetMaxEnergyLevel", "int 0 99999 100", false);

function Player::SetMaxEnergyLevel(%player,%level)
{
    if(%level < 0)
        %level = 0;

    %player.Leveling_energy += %level - %player.client.Leveling_maxenergy;
    %player.client.Leveling_maxenergy = %level;
}
//replicates normal recharge functionality
function Player::DoEnergyRecharge(%player)
{   
    %max = %player.client.Leveling_maxenergy;
    %rate = %player.dataBlock.rechargeRate;

    if((%player.Leveling_energy += %rate) >= %max)
    {
        %player.leveling_energy = %max;
    }
    else  if(isObject(%player.client))
    {
        %player.schedule(100,DoEnergyRecharge);
    }

    %client = %player.client;

    if(%client)
    {
        %client.DisplayMobaHud();
    }
}

package MobaEngery
{
	function Player::GetEnergyLevel(%player)
    {
        //if this player has a max energy set use custom stuff
        if(%player.client.Leveling_maxenergy !$= "")
        {
            %level = %player.Leveling_energy;
            return %level;
        }
        return Parent::GetEnergyLevel(%player);
    }
    function Player::GetEnergyPercent(%player)
    {
        //if this player has a max energy set use custom stuff
        if(%player.client.Leveling_maxenergy !$= "")
        {
            %max = %player.client.Leveling_maxenergy;
            %level = %player.Leveling_energy;
            return %level / %max;
        }
        return Parent::GetEnergyPercent(%player);
    }
    function Player::SetEnergyLevel(%player,%level)
    {
        %client = %player.client;
        if(%client)
        {
            %client.DisplayMobaHud();
        }

        //if this player has a max energy set use custom stuff
        if(%player.client.Leveling_maxenergy !$= "")
        {
            %max = %player.client.Leveling_maxenergy;
            if(%level < 0)
                %level = 0;
            
            if(%level >= %max)
                %level = %max;
            else
                %player.schedule(100,DoEnergyRecharge);
            
            %player.Leveling_Energy = %level;
        }
        Parent::SetEnergyLevel(%player,%level);

    }
};
deactivatePackage("MobaEngery");
activatePackage("MobaEngery");