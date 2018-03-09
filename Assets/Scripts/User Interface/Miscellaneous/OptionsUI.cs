using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : InputEnabledUI
{
    public Button quitBattleButton;
    protected override void SetState(UIState state)
    {
        base.SetState(state);
        if (Battle.main != null)
        {
            quitBattleButton.onClick = new Button.ButtonClickedEvent();
            quitBattleButton.onClick.AddListener(Battle.main.QuitBattle);
            quitBattleButton.gameObject.SetActive(true);
        }
        else
        {
            quitBattleButton.gameObject.SetActive(false);
        }
    }
}
