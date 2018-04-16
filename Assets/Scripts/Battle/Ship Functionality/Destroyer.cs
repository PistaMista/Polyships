using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Gameplay.Effects;

namespace Gameplay.Ships
{
    public class Destroyer : Ship
    {
        public int torpedoBonus;
        public override void Place(Tile[] location)
        {
            base.Place(location);
            parentBoard.owner.arsenal.torpedoes += torpedoBonus;
        }

        public override void Pickup()
        {
            base.Pickup();
            if (tiles != null && tiles.Length > 0) parentBoard.owner.arsenal.torpedoes -= torpedoBonus;
        }

        public override void Destroy()
        {
            base.Destroy();
            parentBoard.owner.arsenal.torpedoes = Mathf.Clamp(parentBoard.owner.arsenal.torpedoes - torpedoBonus, 0, int.MaxValue);
            parentBoard.owner.arsenal.loadedTorpedoes = 0;
        }
    }
}
