using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class Cyclone : Event
    {
        public int maximumTileSpacingToTakeEffect;
        public int dispersionRadius;
        public override void OnTurnEnd()
        {
            Effect[] artilleryAttacks = GetEffectsInQueue<ArtilleryAttack>();
            float[] minimumDistances = new float[artilleryAttacks.Length];

            for (int i = 0; i < artilleryAttacks.Length; i++) //Measure the minimum distance to another target for all targets
            {
                Tile target = (artilleryAttacks[i] as ArtilleryAttack).target;
                float minimumDistance = Mathf.Infinity;
                for (int z = 0; z < artilleryAttacks.Length; z++)
                {
                    Tile disruptor = (artilleryAttacks[z] as ArtilleryAttack).target;
                    if (disruptor != target)
                    {
                        float distance = Vector2Int.Distance(target.coordinates, disruptor.coordinates);
                        if (distance < minimumDistance) minimumDistance = distance;
                    }
                }

                minimumDistances[i] = minimumDistance;
            }

            for (int i = 0; i < artilleryAttacks.Length; i++) //Attacks which are too close together will be randomly dispersed into a square defined by the dispersion radius
            {
                if (minimumDistances[i] < maximumTileSpacingToTakeEffect)
                {
                    ArtilleryAttack attack = artilleryAttacks[i] as ArtilleryAttack;
                    Vector2Int pos = attack.target.coordinates;
                    Vector2Int max = new Vector2Int(attack.target.parentBoard.tiles.GetLength(0) - 1, attack.target.parentBoard.tiles.GetLength(1) - 1);
                    Vector2Int newPos = pos + new Vector2Int(Random.Range(Mathf.Clamp(-dispersionRadius, -pos.x, 0), Mathf.Clamp(dispersionRadius, 0, max.x - pos.x) + 1), Random.Range(Mathf.Clamp(-dispersionRadius, -pos.y, 0), Mathf.Clamp(dispersionRadius, 0, max.y - pos.y) + 1));

                    attack.target = attack.target.parentBoard.tiles[newPos.x, newPos.y];
                }
            }
            base.OnTurnEnd();
        }

        public override int GetAdditionalAllowed()
        {
            return (GetEffectsInQueue<Cyclone>().Length == 0) ? 1 : 0;
        }

        public override string GetDescription()
        {
            return "Cyclone - Gun attacks clustered too close together don't always hit their mark. Lasts for " + duration + (duration == 1 ? " turn" : " turns") + ".";
        }
    }
}
