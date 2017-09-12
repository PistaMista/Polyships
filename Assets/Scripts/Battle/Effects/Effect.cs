using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : ScriptableObject
{
    public struct EffectData
    {
        public static implicit operator EffectData(Effect effect)
        {
            return null;
        }
    }
}
