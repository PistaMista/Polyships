using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TitleUI
{
    public class TitleSlave : InputEnabledUI
    {
        public UIAgent nextUI;
        public UIAgent previousUI;
        public virtual void OnTitleSetState(UIState state)
        {

        }

        public virtual void Next()
        {
            State = UIState.DISABLED;
            nextUI.State = UIState.ENABLING;
        }
        public virtual void Previous()
        {
            State = UIState.DISABLED;
            previousUI.State = UIState.ENABLING;
        }
    }
}
