using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtilleryAttack_Effect : Effect
{
    public Tile target;
    public override void OnTurnEnd()
    {
        Battle.main.log[0].artilleryImpacts.Add(target);
        Battle.main.attacker.hitTiles.Add(target);
        target.hit = true;

        if (target.containedShip)
        {
            target.containedShip.Damage(1);
        }
    }

    public override void OnOtherEffectAdd(Effect addedEffect)
    {

    }

    public override int GetAdditionalAllowed()
    {
        return Mathf.Clamp(Battle.main.attackerCapabilities.maximumArtilleryCount, 0, base.GetAdditionalAllowed()) * (Battle.main.attacker.hitTiles.Contains(target) ? 0 : 1);
    }

    protected override bool ConflictsWith(Effect effect)
    {
        if (!base.ConflictsWith(effect))
        {
            if (effect is ArtilleryAttack_Effect && ((ArtilleryAttack_Effect)effect).target == target)
            {
                return true;
            }
            return false;
        }

        return true;
    }
}
