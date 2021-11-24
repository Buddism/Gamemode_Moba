package mobaAutoAim
{
    function isWithinAngle(%observer,%object,%angle)
    {
        %obsFacing = %observer.getForwardVector();
        %obsPos = %observer.getEyePoint();
        %objPos = %object.getHackPosition();

        //we don't care about up and down
        %x = getWord(%obsPos,0) - getWord(%objPos,0);
        %y = getWord(%obsPos,1) - getWord(%objPos,1);

        %betObsObj = vectorNormalize(%x SPC %y SPC "0");

        //converted to degrees for clarity
        %angleBetween = 180 - (mAcos(vectorDot(%obsFacing, %betObsObj)/vectorLen(%obsFacing) * vectorLen(%betObsObj)) * 180 / 3.14);

        return %angle >= %angleBetween;

    }

    function getTarget(%observer,%radius)
    {
        %position = %observer.getposition();
        InitContainerRadiusSearch(%position,%radius,$TypeMasks::PlayerObjectType | $TypeMasks::StaticObjectType);

        %closest = "";
        %closestDistance = %radius + 1;
        while(%next = ContainerSearchNext())
        {
            
        }
    }
}
deactivatePackage("mobaAutoAim");
activatePackage("mobaAutoAim");
