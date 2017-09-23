using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagCustomizationModuleUserInterface : InputEnabledUserInterface
{
    public Image colorPalette;
    public FlagCustomizationColorSelector[] colorSelectors;
    public ExclusiveTogglingButtonGroup colorSelectorGroup;
    public GameObject borderPrefab;
    public GameObject pixelPrefab;
    public bool secondPlayer;
    public Image[,] pixels;
    public Color borderColor;
    public int selectedColorID;
    int pixelDimensions;
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
        if (pressed)
        {
            Vector2 lowerLeftFlagCorner = (Vector2)pixels[0, 0].rectTransform.position - Vector2.one * pixelDimensions / 2.0f;
            Vector2 upperRightFlagCorner = (Vector2)pixels[pixels.GetLength(0) - 1, pixels.GetLength(1) - 1].rectTransform.position + Vector2.one * pixelDimensions / 2.0f;
            Vector2 lowerLeftPaletteCorner = (Vector2)colorPalette.rectTransform.position - colorPalette.rectTransform.sizeDelta * Screen.width / referenceResolution.x / 2.0f;
            Vector2 upperRightPaletteCorner = (Vector2)colorPalette.rectTransform.position + colorPalette.rectTransform.sizeDelta * Screen.width / referenceResolution.x / 2.0f;

            if (beginPress)
            {
                // if (currentInputPosition.screen.x > lowerLeftCorner.x && currentInputPosition.screen.y > lowerLeftCorner.y && currentInputPosition.screen.x < upperRightCorner.x && currentInputPosition.screen.y < upperRightCorner.y)
                // {
                //     SlidingUserInterface_Master.lockedDirections = new bool[] { true, true };
                //     focused = true;
                // }

                if (CheckIntersection(GetIntersection(lowerLeftFlagCorner, upperRightFlagCorner, currentInputPosition.screen)) || CheckIntersection(GetIntersection(lowerLeftPaletteCorner, upperRightPaletteCorner, currentInputPosition.screen)))
                {
                    SlidingUserInterface_Master.lockedDirections = new bool[] { true, true };
                    focused = true;
                }
            }

            if (focused)
            {
                Vector4 intersection = GetIntersection(lowerLeftFlagCorner, upperRightFlagCorner, currentInputPosition.screen);
                if (CheckIntersection(intersection))
                {
                    int x = Mathf.FloorToInt(intersection.x * pixels.GetLength(0));
                    int y = Mathf.FloorToInt(intersection.y * pixels.GetLength(1));

                    float[,,] flagData = secondPlayer ? GameLoaderUserInterface.newBattleData.attacked.flag : GameLoaderUserInterface.newBattleData.attacker.flag;
                    flagData[x, y, 0] = colorSelectors[selectedColorID].Color.r;
                    flagData[x, y, 1] = colorSelectors[selectedColorID].Color.g;
                    flagData[x, y, 2] = colorSelectors[selectedColorID].Color.b;
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
                else
                {
                    intersection = GetIntersection(lowerLeftPaletteCorner, upperRightPaletteCorner, currentInputPosition.screen);
                    if (CheckIntersection(intersection))
                    {
                        colorSelectors[selectedColorID].Color = colorPalette.sprite.texture.GetPixel((int)(colorPalette.sprite.texture.width * intersection.x), (int)(colorPalette.sprite.texture.height * intersection.y));
                    }
                }
            }
        }


        if (endPress)
        {
            SlidingUserInterface_Master.lockedDirections = new bool[2];
            focused = false;
        }
    }

    public void SelectColor(int colorSelectorID)
    {
        FlagCustomizationColorSelector selector = colorSelectors[colorSelectorID];
        selectedColorID = colorSelectorID;
        colorSelectorGroup.ResetColors(colorSelectorID);
    }
}
