using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Gameplay.Effects;

namespace Gameplay.Ships
{
    public class Radarboat : Ship
    {
        public int radarBonus;
        public override void Place(Tile[] location)
        {
            base.Place(location);
            if (location != null && location.Length > 0) parentBoard.owner.arsenal.radars += radarBonus;
        }

        public override void Pickup()
        {
            if (tiles != null && tiles.Length > 0) parentBoard.owner.arsenal.radars -= radarBonus;
            base.Pickup();
        }

        public override void Destroy()
        {
            base.Destroy();
            parentBoard.owner.arsenal.radars -= radarBonus;

            Effect radar = Battle.main.effects.Find(x => (x is RadarRecon || x is RadarRecharge) && x.visibleTo == parentBoard.owner);
            if (radar != null)
            {
                Effect.turnEndAction += () => Effect.RemoveFromStack(radar);
            }
        }
    }
}