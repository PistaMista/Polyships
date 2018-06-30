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

        protected override int[] GetMetadata()
        {
            if (targetedPlayer.board.ShipsPlaced)
            {
                return new int[] { guns, torpedoes, loadedTorpedoes, loadedTorpedoCap, aircraft };
            }
            else
            {
                AmmoRegistry prefab = MiscellaneousVariables.it.effectPrefabs[prefabIndex] as AmmoRegistry;
                return new int[] { prefab.guns, prefab.torpedoes, prefab.loadedTorpedoes, prefab.loadedTorpedoCap, prefab.aircraft };
            }
        }
        public override void Initialize(EffectData data)
        {
            base.Initialize(data);
            guns = data.metadata[0];
            torpedoes = data.metadata[1];
            loadedTorpedoes = data.metadata[2];
            loadedTorpedoCap = data.metadata[3];
            aircraft = data.metadata[4];
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
