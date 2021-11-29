package mobaOnRespawn
{
    function VCE_initServer()
    {
        parent::VCE_initServer();
        hookFunctionToVCEEventFunction("GameConnection","SpawnPlayer","%client","true","","onPlayerRespawn");
		activateVCEEventFunctionHooks();

        registerSpecialVar(fxDTSbrick,"hitboxhealth","%this.hitboxShape.getHealth()");
    }

};
deactivatePackage("mobaOnRespawn");
activatePackage("mobaOnRespawn");