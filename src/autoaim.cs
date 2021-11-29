package mobaAutoAim
{
    function withinAngle(%observer,%object,%angle)
    {
        %obsFacing = %observer.getForwardVector();
        %obsPos = %observer.getEyePoint();
        %objPos = %object.getPosition();

        //we don't care about up and down
        %x = getWord(%obsPos,0) - getWord(%objPos,0);
        %y = getWord(%obsPos,1) - getWord(%objPos,1);

        %betObsObj = vectorNormalize(%x SPC %y SPC "0");

        //converted to degrees for clarity
        %angleBetween = 180 - (mAcos(vectorDot(%obsFacing, %betObsObj)/vectorLen(%obsFacing) * vectorLen(%betObsObj)) * 180 / 3.14);

        return %angle >= %angleBetween ? %anglebetween : "";
    }

    function getTarget(%observer,%radius,%angle,%targetClosest)
    {
        %position = %observer.getposition();
        %minigame = getMinigameFromObject(%observer);
        InitContainerRadiusSearch(%position,%radius,$TypeMasks::PlayerObjectType | $TypeMasks::StaticShapeObjectType);

        %closest = "";
        %closestAngle = 181;
        %closestDistance = %radius + 1;
        while(%next = ContainerSearchNext())
        {
            if(%minigame)
            {
                if(!%minigame.minigameCanDamage(%observer, %next))
                {
                    continue;
                }
            }

            %distance = ContainerSearchCurrDist();
            %withinAngle = withinAngle(%observer,%next,%angle);
            if((%targetClosest && %distance < %closestDistance)  || (!%targetClosest && %withinAngle < %closestAngle) && %withinAngle !$= "")
            {
                %closest = %next;
                %closestAngle = %withinAngle;
                %closestDistance = %distance;
            }
        }

        return %closest;
    }
};
deactivatePackage("mobaAutoAim");
activatePackage("mobaAutoAim");
