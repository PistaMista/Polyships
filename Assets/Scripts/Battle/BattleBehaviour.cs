using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBehaviour : MonoBehaviour
{
    /// <summary>
    /// Executes when a battle starts.
    /// </summary>
    public virtual void OnBattleStart()
    {

    }
    /// <summary>
    /// Executes when a battle ends.
    /// </summary>
    public virtual void OnBattleEnd()
    {

    }
    /// <summary>
    /// Executes every time a new turn starts.
    /// </summary>
    public virtual void OnTurnStart()
    {

    }

    /// <summary>
    /// Executes every time a game is loaded and the current turn is therefore resumed.
    /// </summary>
    public virtual void OnTurnResume()
    {

    }

    /// <summary>
    /// Executes every time a turn ends.
    /// </summary>
    public virtual void OnTurnEnd()
    {

    }
}
