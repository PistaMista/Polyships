﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIModule : ScriptableObject
{
    [Serializable]
    public struct AIModuleData
    {
        public static implicit operator AIModuleData(AIModule module)
        {
            AIModuleData result;
            return result;
        }
    }
    public Player owner;

    public virtual void Initialize(AIModuleData data)
    {

    }

    public virtual void AssignReferences(AIModuleData data)
    {

    }

    public void DoTurn()
    {
        Battle.main.NextTurn();
    }
}
