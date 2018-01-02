using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BattleUIType
{
    TURN_NOTIFIER,
    FLEET_PLACEMENT,
    BATTLE_OVERVIEW,
    ATTACK_VIEW,
    ATTACKER_INFO,
    DAMAGE_REPORT,
    CINEMATIC_VIEW,
    BATTLE_RESULT_VIEW
}
public class BattleUIMaster : InputEnabledUI
{
    static BattleUIMaster it;
    public UIAgent[] primaryBUIs;
    public UIAgent[] secondaryBUIs;

    void Awake()
    {
        it = this;
    }

    public static void ForceResetAllBUIs()
    {
        for (int i = 0; i < it.primaryBUIs.Length; i++)
        {
            UIAgent x = it.primaryBUIs[i];
            if ((int)x.State >= 2)
            {
                x.State = UIState.DISABLING;
            }
        }

        for (int i = 0; i < it.secondaryBUIs.Length; i++)
        {
            UIAgent x = it.secondaryBUIs[i];
            if ((int)x.State >= 2)
            {
                x.State = UIState.DISABLING;
            }
        }
    }

    public static void EnablePrimaryBUI(BattleUIType type)
    {
        it.primaryBUIs[(int)type].State = UIState.ENABLING;
    }
}
