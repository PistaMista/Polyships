using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagCustomizationModuleUserInterface : InputEnabledUserInterface
{
    public FlagCustomizationColorSelector[] colorSelectors;
    public GameObject borderPrefab;
    public GameObject pixelPrefab;
    public bool secondPlayer;
    public Image[,] pixels;
    public Color borderColor;
    public Color selectedColor;
    int pixelDimensions;
    bool editingPalette;
    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLED:
                RefreshFlag();
                SelectColor(0);
                break;
        }
    }

    void RefreshFlag()
    {
        if (pixels == null)
        {
            InitializeWidget();
        }

        float[,,] flagData = secondPlayer ? GameLoaderUserInterface.newBattleData.attacked.flag : GameLoaderUserInterface.newBattleData.attacker.flag;
        for (int x = 0; x < flagData.GetLength(0); x++)
        {
            for (int y = 0; y < flagData.GetLength(1); y++)
            {
                pixels[x, y].color = new Color(flagData[x, y, 0], flagData[x, y, 1], flagData[x, y, 2]);
            }
        }
    }

    void InitializeWidget()
    {
        float[,,] flagData = secondPlayer ? GameLoaderUserInterface.newBattleData.attacked.flag : GameLoaderUserInterface.newBattleData.attacker.flag;
        pixels = new Image[flagData.GetLength(0), flagData.GetLength(1)];

        Vector2 reservedSpace = new Vector2(BasicUserInterface.referenceResolution.x / 2.0f, BasicUserInterface.referenceResolution.y / 2.5f);
        bool horizontalAdjustment = reservedSpace.x < reservedSpace.y;

        pixelDimensions = Mathf.FloorToInt((horizontalAdjustment ? reservedSpace.x : reservedSpace.y) / (horizontalAdjustment ? flagData.GetLength(0) : flagData.GetLength(1)));

        Image border = Instantiate(borderPrefab).GetComponent<Image>();
        border.color = borderColor;
        RectTransform borderRect = border.GetComponent<RectTransform>();
        borderRect.SetParent(transform);
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = new Vector2(flagData.GetLength(0), flagData.GetLength(1)) * pixelDimensions + Vector2.one * border.sprite.border.x * 2.0f;
        borderRect.localScale = Vector3.one;

        Vector2 centerPosition = new Vector2(flagData.GetLength(0), flagData.GetLength(1)) / 2.0f;
        for (int x = 0; x < flagData.GetLength(0); x++)
        {
            for (int y = 0; y < flagData.GetLength(1); y++)
            {
                RectTransform pixel = Instantiate(pixelPrefab).GetComponent<RectTransform>();
                pixel.sizeDelta = Vector2.one * pixelDimensions;
                Vector2 position = (new Vector2(x + 0.5f, y + 0.5f) - centerPosition) * pixelDimensions;
                pixel.SetParent(transform);
                pixel.anchoredPosition = position;
                pixel.localScale = Vector3.one;
                pixels[x, y] = pixel.GetComponent<Image>();
            }
        }
    }

    bool focused;
    protected override void ProcessInput()
    {
        base.ProcessInput();
        if (!editingPalette)
        {
            Vector2 lowerLeftCorner = (Vector2)pixels[0, 0].rectTransform.position - Vector2.one * pixelDimensions / 2.0f;
            Vector2 upperRightCorner = (Vector2)pixels[pixels.GetLength(0) - 1, pixels.GetLength(1) - 1].rectTransform.position + Vector2.one * pixelDimensions / 2.0f;
            Vector2 size = upperRightCorner - lowerLeftCorner;

            if (beginPress)
            {
                if (currentInputPosition.screen.x > lowerLeftCorner.x && currentInputPosition.screen.y > lowerLeftCorner.y && currentInputPosition.screen.x < upperRightCorner.x && currentInputPosition.screen.y < upperRightCorner.y)
                {
                    SlidingUserInterface_Master.lockedDirections = new bool[] { true, true };
                    focused = true;
                }
            }

            if (focused && currentInputPosition.screen.x > lowerLeftCorner.x && currentInputPosition.screen.y > lowerLeftCorner.y && currentInputPosition.screen.x < upperRightCorner.x && currentInputPosition.screen.y < upperRightCorner.y)
            {
                int x = Mathf.FloorToInt(((currentInputPosition.screen.x - lowerLeftCorner.x) / size.x) * pixels.GetLength(0));
                int y = Mathf.FloorToInt(((currentInputPosition.screen.y - lowerLeftCorner.y) / size.y) * pixels.GetLength(1));

                float[,,] flagData = secondPlayer ? GameLoaderUserInterface.newBattleData.attacked.flag : GameLoaderUserInterface.newBattleData.attacker.flag;
                flagData[x, y, 0] = selectedColor.r;
                flagData[x, y, 1] = selectedColor.g;
                flagData[x, y, 2] = selectedColor.b;
                if (secondPlayer)
                {
                    GameLoaderUserInterface.newBattleData.attacked.flag = flagData;
                }
                else
                {
                    GameLoaderUserInterface.newBattleData.attacker.flag = flagData;
                }

                RefreshFlag();
            }

            if (endPress)
            {
                SlidingUserInterface_Master.lockedDirections = new bool[2];
                focused = false;
            }
        }

    }

    public void SelectColor(int colorSelectorID)
    {
        FlagCustomizationColorSelector selector = colorSelectors[colorSelectorID];
        selectedColor = selector.Color;

        for (int i = 0; i < colorSelectors.Length; i++)
        {
            colorSelectors[i].SetHighlight(i == colorSelectorID);
        }
    }

    public void EditColorPalette()
    {
        editingPalette = !editingPalette;
        if (editingPalette)
        {
            SlidingUserInterface_Master.lockedDirections = new bool[2] { true, true };
        }
        else
        {
            SlidingUserInterface_Master.lockedDirections = new bool[2];
        }
    }
}
