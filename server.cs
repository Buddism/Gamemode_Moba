%error = forceRequiredAddon("Gamemode_Slayer");
if(%error == $Error::Addon_NotFound)
	error("ERROR: Gamemode_Slayer_Infection - Required add-on Gamemode_Slayer not found!");
else
{
	exec("./src/autoaim.cs");
	exec("./src/energy.cs");
	exec("./src/hitbox.cs");
	exec("./src/hud.cs");
	exec("./src/kills.cs");
	exec("./src/levels.cs");
	exec("./src/onRespawn.cs");
	exec("./src/tower.cs");
	exec("./src/weapon.cs");
}

//Create the game mode template.
new ScriptGroup(Slayer_GameModeTemplateSG)
{
	className = "Slayer_Moba";
	uiName = "Moba";
	useTeams = true;
	
	//Team settings
	teams_minTeams = 2; //user must create at least two teams
};

