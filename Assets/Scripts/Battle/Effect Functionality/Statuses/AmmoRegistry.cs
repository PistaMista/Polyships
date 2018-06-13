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
        public int radars;
        public bool radarUsed;

        int[] startingMetadata;
        void Start()
        {
            startingMetadata = new int[] { guns, torpedoes, loadedTorpedoes, loadedTorpedoCap, aircraft, radars };
        }
        public override void OnTurnStart()
        {
            base.OnTurnStart();
            if (Battle.main.attacker == targetedPlayer)
            {
                torpedoesFiredLastTurn = 0;
                radarUsed = false;
            }
        }
        protected override int[] GetMetadata()
        {
            return targetedPlayer.board.ShipsPlaced ? new int[] { guns, torpedoes, loadedTorpedoes, loadedTorpedoCap, aircraft, radars } : startingMetadata;
        }
        public override void Initialize(EffectData data)
        {
            base.Initialize(data);
            guns = data.metadata[0];
            torpedoes = data.metadata[1];
            loadedTorpedoes = data.metadata[2];
            loadedTorpedoCap = data.metadata[3];
            aircraft = data.metadata[4];
            radars = data.metadata[5];
        }
        public override string GetDescription()
        {
            return "Munitions - Torpedo x" + torpedoes;
        }

        public override int Max()
        {
            return 2;
        }

        protected override bool Conflicts(Effect effect)
        {
            return effect is AmmoRegistry && effect.targetedPlayer == targetedPlayer;
        }

        protected override bool Legal()
        {
            return true;
        }

        public override void OnBattleStart()
        {
            base.OnBattleStart();
            if (torpedoes > 0 && loadedTorpedoes == 0)
            {
                turnStartAction += () =>
                {
                    Effect torpedoReload = CreateEffect(typeof(TorpedoReload));

                    torpedoReload.targetedPlayer = targetedPlayer;
                    torpedoReload.visibleTo = targetedPlayer;

                    AddToStack(torpedoReload);
                };
            }
        }
    }
}
