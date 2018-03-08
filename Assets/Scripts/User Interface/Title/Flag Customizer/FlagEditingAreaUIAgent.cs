using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagEditingAreaUIAgent : InputEnabledUI
{
    public GameObject pixelParent;
    public int currentPlayer = 0;
    public Vector2 referenceResolution;
    public RectTransform window;

    protected override void SetState(UIState state)
    {
        base.SetState(state);
        if ((int)state >= 2)
        {
            currentPlayer = 0;
            ConstructFlag();
        }
        SetInteractable((int)state >= 2);
    }

    void ConstructFlag()
    {
        float[,,] flag = BattleTweakerUI.flags[currentPlayer];
        Vector2 pixelDimensions = new Vector2(rect.rect.width / flag.GetLength(0), rect.rect.height / flag.GetLength(1));
        Vector2 beginningPos = -rect.rect.size / 2.0f + pixelDimensions / 2.0f;

        Destroy(pixelParent);
        pixelParent = new GameObject("Pixel Parent");
        pixelParent.transform.SetParent(transform, false);

        for (int x = 0; x < flag.GetLength(0); x++)
        {
            for (int y = 0; y < flag.GetLength(1); y++)
            {
                GameObject pixel = new GameObject("Pixel x: " + x + " y: " + y);
                pixel.transform.SetParent(pixelParent.transform, false);
                pixel.transform.localPosition = beginningPos + Vector2.Scale(pixelDimensions, new Vector2Int(x, y));

                RawImage image = pixel.AddComponent<RawImage>();
                image.color = new Color(flag[x, y, 0], flag[x, y, 1], flag[x, y, 2]);
                image.rectTransform.sizeDelta = pixelDimensions;
            }
        }
    }

    protected override void ProcessInput()
    {
        base.ProcessInput();
        //Vector2 scaledInputPosition = ;
        if (tap)
        {
            Vector2 scalingVector = new Vector2(referenceResolution.x / window.rect.width, referenceResolution.y / window.rect.height);
            Vector2 scaledInput = Vector2.Scale(currentInputPosition.screen, scalingVector);
            Vector2 scaledPos = Vector2.Scale(rect.position, scalingVector);
            Vector2 scaledPosition = scaledInput - scaledPos;
            Debug.Log(scaledPosition);
        }
    }
}
