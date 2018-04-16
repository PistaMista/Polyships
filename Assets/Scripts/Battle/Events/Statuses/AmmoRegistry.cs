using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class AmmoRegistry : Event
    {
        public int guns;
        public int torpedoes;
        public int loadedTorpedoes;
        public int loadedTorpedoCap;
        public int torpedoesFiredLastTurn;
        public int aircraft;
        public override void OnTurnStart()
        {
            base.OnTurnStart();
            if (Battle.main.attacker == affectedPlayer) torpedoesFiredLastTurn = 0;
        }
        protected override int[] GetMetadata()
        {
            return Battle.main.fighting ? new int[] { guns, torpedoes, loadedTorpedoes, aircraft } : new int[4];
        }
        public override void Initialize(EffectData data)
        {
            base.Initialize(data);
            guns = data.metadata[0];
            torpedoes = data.metadata[1];
            loadedTorpedoes = data.metadata[2];
            aircraft = data.metadata[3];
        }
        public override string GetDescription()
        {
            return "Munitions - Torpedo x" + torpedoes;
        }
    }
}
