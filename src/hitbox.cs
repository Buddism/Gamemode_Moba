registerInputEvent("fxDTSBrick","onHitBoxDestroyed","self fxDTSBrick" TAB "Player Player" TAB "Client GameConnection" TAB "Minigame Minigame");
registerInputEvent("fxDTSBrick","onHitBoxHit","self fxDTSBrick" TAB "Player Player" TAB "Client GameConnection" TAB "Minigame Minigame");

registerOutputEvent("fxDTSBrick","HitboxMake","vector" TAB "vector" TAB "int 1 9999999 100" TAB "string 200 100");
registerOutputEvent("fxDTSBrick","HitboxRemove");


datablock StaticShapeData(hitbox_cube)
{
    shapeFile = "Add-ons/Gamemode_Moba/res/Cube_col.dts";
    maxDamage = 100;
};

function serverCmdClearHitboxes(%client)
{  
    %set = HitboxSet;
    %length = HitboxSet.GetCount();
    if(%client.isAdmin)
    {
        for(%i = 0; %i < %length; %i++)
        {
            %hitbox = %set.getObject(%i);
            %brick = %hitbox.ownerBrick;

            %brick.HitboxRemove();
        }
    }
}

function serverCmdToggleHitboxes(%client)
{  
    %set = HitboxSet;
    %length = HitboxSet.GetCount();

    $Server::Hitboxe::Show = !$Server::Hitboxe::Show;
    if(%client.isAdmin)
    {
        for(%i = 0; %i < %length; %i++)
        {
            %hitbox = %set.getObject(%i);
            
            UpdateHitboxVisibility(%hitbox);
        }
    }
}

function fxDTSBrick::onHitboxDestroyed(%brick,%client,%player)
{
    $inputTarget_Self = %brick;
	$inputTarget_Player = %player;
	$inputTarget_Client = %client;
	$inputTarget_Vehicle = %brick.vehicle;
	$inputTarget_Minigame = getMinigameFromObject(%client);
}

function fxDTSBrick::onHitboxHit(%brick,%client,%player)
{
    $inputTarget_Self = %brick;
	$inputTarget_Player = %player;
	$inputTarget_Client = %client;
	$inputTarget_Vehicle = %brick.vehicle;
	$inputTarget_Minigame = getMinigameFromObject(%client);
}

function fxDTSBrick::HitboxMake(%brick,%size,%offset,%health,%team)
{
    if(isObject(%brick.hitboxSHape))
    {
        %brick.HitboxRemove();
    }

    %brickPos = %brick.getPosition();
    %hitboxCenter = vectorAdd(%brickPos, %offset);

    %hitbox = new StaticShape()
    {
        className = HitboxShape;
        position = %hitboxCenter;
        scale = %size;
        dataBlock = "hitbox_cube";
    };

    missionCleanup.add(%hitbox);

    %hitbox.ownerBrick = %brick;
    %hitbox.setMaxHealth(%health);
    UpdateHitboxVisibility(%hitbox);

    %brick.hitBoxTeam = %team;
    %brick.hitboxShape = %hitbox;
    %brick.hitboxHealth = %health;

    if(!isObject(HitboxSet))
    {
        new SimSet(HitboxSet){};
    }

    HitboxSet.add(%hitbox);
}

function fxDTSBrick::HitboxRemove(%brick)
{
    %hitbox = %brick.hitboxShape;

    if(isObject(%hitbox))
    {
        %brick.hitboxShape.delete();
        %brick.hitboxShape = "";
        %brick.hitboxHealth = "";
        %brick.hitBoxTeam = "";
    }
}

package mobaHitbox
{
    function fxDTSBrick::delete(%brick)
    {   
        %brick.HitboxRemove();
        parent::delete(%brick);
    }

    function fxDTSBrick::killBrick(%brick)
    {
        %brick.HitboxRemove();
        parent::killBrick(%brick); 
    }

    function UpdateHitboxVisibility(%hitbox)
    {
        if($Server::Hitboxe::Show)
        {
            %hitbox.unhideNode("ALL");
        }
        else
        {
            %hitbox.hideNode("ALL");
        }
    }

    function StaticShape::Damage(%this, %sourceObject, %position, %damage, %damageType)
    {
        %brick = %this.ownerBrick;
        %classname = %this.className;

        parent::Damage(%this, %sourceObject, %position, %damage, %damageType);

        if(%className $= "HitboxShape")
        {
            if(%this.getHealth() <= 0)
            {
                %brick.processInputEvent ("OnHitboxDestroyed", %client);
            }
            else
            {
                %brick.processInputEvent ("OnHitboxHit", %client);
            }
        }
    }

    function StaticShape::GetState()
    {
        return "";
    }

    function ProjectileData::onCollision (%this, %obj, %col, %fade, %pos, %normal, %velocity)
    {
        parent::onCollision (%this, %obj, %col, %fade, %pos, %normal, %velocity);

        %client = %obj.client;
        if (isObject (%obj.sourceObject) && %obj.sourceObject.isBot)
        {
            %client = %obj.sourceObject;
        }
        %victimClient = %col.client;
        %clientBLID = getBL_IDFromObject (%obj);
        if (isObject (%client.miniGame))
        {
            if (getSimTime () - %client.lastF8Time < 3000)
            {
                return;
            }
        }
        %scale = getWord (%obj.getScale (), 2);
        %mask = $TypeMasks::StaticObjectType;
        if(%mask & %col.getType())
        {
            if(%col.className $= "HitboxShape")
            {
                if (!isObject (%client))
                {
                    return;
                }
                
                %this.Damage (%obj, %col, %fade, %pos, %normal);
            }
        }
    }
};
deactivatePackage("mobaHitbox");
activatePackage("mobaHitbox");