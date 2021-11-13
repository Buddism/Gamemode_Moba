package mobaOnRespawn
{
    function VCE_initServer()
    {
        parent::VCE_initServer();
        hookFunctionToVCEEventFunction("GameConnection","SpawnPlayer","%client","true","","onPlayerRespawn");
		activateVCEEventFunctionHooks();
    }

};
deactivatePackage("mobaOnRespawn");
activatePackage("mobaOnRespawn");