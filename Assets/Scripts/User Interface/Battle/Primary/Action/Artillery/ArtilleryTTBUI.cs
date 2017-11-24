using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtilleryTTBUI : PrimaryTacticalTargetingBUI
{
    protected override int GetInitialTokenCount()
    {
        return Battle.main.attackerCapabilities.maximumArtilleryCount;
    }
}
