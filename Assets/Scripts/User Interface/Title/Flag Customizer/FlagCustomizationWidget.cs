using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagCustomizationWidget : MonoBehaviour
{

    // public GameObject borderPrefab;
    // public GameObject pixelPrefab;
    // public bool secondPlayer;
    // public Image[,] pixels;
    // public Color borderColor;

    // public void Reset()
    // {
    //     Destroy(parent);
    //     parent = new GameObject("Parent");
    //     parent.transform.SetParent(transform);

    //     float[,,] flagData = secondPlayer ? GameLoaderUserInterface.newBattleData.attacked.flag : GameLoaderUserInterface.newBattleData.attacker.flag;
    //     rect.anchoredPosition = Vector2.right * position * BasicUserInterface.referenceResolution.x;
    //     pixels = new Image[flagData.GetLength(0), flagData.GetLength(1)];

    //     Vector2 flagResolution = new Vector2(flagData.GetLength(0), flagData.GetLength(1));
    //     Vector2 reservedSpace = new Vector2(BasicUserInterface.referenceResolution.x / 2.0f, BasicUserInterface.referenceResolution.y / 2.5f);
    //     bool horizontalAdjustment = reservedSpace.x < reservedSpace.y;

    //     int pixelDimensions = Mathf.FloorToInt((horizontalAdjustment ? reservedSpace.x : reservedSpace.y) / (horizontalAdjustment ? resolution.x : resolution.y));

    //     Image border = Instantiate(borderPrefab).GetComponent<Image>();
    //     border.color = borderColor;
    //     RectTransform borderRect = border.GetComponent<RectTransform>();
    //     borderRect.SetParent(parent.transform);
    //     borderRect.anchoredPosition = Vector2.zero;
    //     borderRect.sizeDelta = flagResolution * pixelDimensions + Vector2.one * border.sprite.border.x * 2.0f;
    //     borderRect.localScale = Vector3.one;

    //     Vector2 centerPosition = flagResolution / 2.0f;
    //     for (int x = 0; x < flagResolution.x; x++)
    //     {
    //         for (int y = 0; y < flagResolution.y; y++)
    //         {
    //             RectTransform pixel = Instantiate(pixelPrefab).GetComponent<RectTransform>();
    //             pixel.sizeDelta = Vector2.one * pixelDimensions;
    //             Vector2 position = (new Vector2(x + 0.5f, y + 0.5f) - centerPosition) * pixelDimensions;
    //             pixel.SetParent(parent.transform);
    //             pixel.anchoredPosition = position;
    //             pixel.localScale = Vector3.one;
    //         }
    //     }
    // }
}
