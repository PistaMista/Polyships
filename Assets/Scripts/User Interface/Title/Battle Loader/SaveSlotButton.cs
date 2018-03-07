using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotButton : MonoBehaviour
{
    public BattleLoaderUI boundTo;
    public Button clearButton;
    public Image clearButtonUnprimedImage;
    public Image clearButtonPrimedImage;
    public int managedSlot;
    public float clearConfirmTimeout;
    bool clearerPrimed;

    void OnEnable()
    {
        clearButton.gameObject.SetActive(boundTo.CheckSlot(managedSlot));
    }

    void UnprimeClearer()
    {
        clearerPrimed = false;
        clearButton.image = clearButtonUnprimedImage;
        clearButtonUnprimedImage.gameObject.SetActive(true);
        clearButtonPrimedImage.gameObject.SetActive(false);
    }

    public void OnClickClear()
    {
        if (clearerPrimed)
        {
            boundTo.ClearSlot(managedSlot);
            clearButton.gameObject.SetActive(false);
            CancelInvoke();
        }
        else
        {
            clearButton.image = clearButtonPrimedImage;
            clearButtonPrimedImage.gameObject.SetActive(true);
            clearButtonUnprimedImage.gameObject.SetActive(false);

            clearerPrimed = true;
            Invoke("UnprimeClearer", clearConfirmTimeout);
        }
    }

    public void OnClickLoad()
    {
        boundTo.SelectSlot(managedSlot);
    }
}
