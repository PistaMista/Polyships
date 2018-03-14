using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtilleryAttack_Effect : Effect
{
    public Tile target;
    public override void OnTurnEnd()
    {

    }

    public override void OnOtherEffectAdd(Effect addedEffect)
    {
        if (addedEffect is ArtilleryAttack_Effect && ((ArtilleryAttack_Effect)addedEffect).target == target)
        {
            Battle.main.RemoveEffect(addedEffect);
        }
    }

}
