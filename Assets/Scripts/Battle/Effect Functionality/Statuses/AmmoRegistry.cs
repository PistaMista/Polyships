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
        public int aircraft;
        public int radars;

        int[] startingMetadata;
        void Start()
        {
            startingMetadata = new int[] { guns, torpedoes, loadedTorpedoes, loadedTorpedoCap, aircraft, radars };
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

        protected override bool Legal()
        {
            bool playerAlreadyHasHisAmmoRegistry = Battle.main.effects.Exists(x => x is AmmoRegistry && x.targetedPlayer == targetedPlayer);
            return !playerAlreadyHasHisAmmoRegistry;
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
