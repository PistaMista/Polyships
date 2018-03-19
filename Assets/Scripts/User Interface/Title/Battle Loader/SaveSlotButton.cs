using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TitleUI
{
    public class SaveSlotButton : MonoBehaviour
    {
        public BattleLoader boundTo;
        public Button clearButton;
        public Image clearButtonUnprimedImage;
        public Image clearButtonPrimedImage;
        public GameObject pixelPrefab;
        GameObject pixelParent;
        public float pixelSide;
        public Text label;
        public string emptyMessage;
        public string usedMessage;
        public int managedSlot;
        public float clearConfirmTimeout;
        bool clearerPrimed;

        void OnEnable()
        {
            if (boundTo.CheckSlot(managedSlot))
            {
                Destroy(pixelParent);
                pixelParent = new GameObject("Pixel Parent");
                pixelParent.transform.SetParent(transform, false);
                pixelParent.transform.SetAsLastSibling();

                Battle.BattleData data = boundTo.saveSlotContents[managedSlot];
                for (int player = 0; player < 2; player++)
                {
                    Player.PlayerData playerData = player == 0 ? data.attacker : data.defender;
                    float[,,] flag = playerData.flag;
                    Vector2 beginningPos = (player == 0 ? Vector2.one * (pixelSide / 2.0f + 20) : new Vector2((640 - pixelSide / 2.0f) - pixelSide * flag.GetLength(0), pixelSide / 2.0f + 20)) - new Vector2(320, 80);
                    for (int x = 0; x < flag.GetLength(0); x++)
                    {
                        for (int y = 0; y < flag.GetLength(1); y++)
                        {
                            Color color = new Color(flag[x, y, 0], flag[x, y, 1], flag[x, y, 2]);
                            GameObject pixel = Instantiate(pixelPrefab);
                            pixel.transform.SetParent(pixelParent.transform, false);
                            pixel.transform.localPosition = beginningPos + new Vector2(x, y) * pixelSide;

                            pixel.GetComponent<RawImage>().color = color;
                        }
                    }
                }
                clearButton.gameObject.SetActive(true);
                label.text = usedMessage;
            }
            else
            {
                label.text = emptyMessage;
                clearButton.gameObject.SetActive(false);
            }
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
                label.text = emptyMessage;
                Destroy(pixelParent);
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
}