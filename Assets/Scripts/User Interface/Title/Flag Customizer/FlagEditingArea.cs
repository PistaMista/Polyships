using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TitleUI
{
    public class FlagEditingArea : BasicUI
    {
        public GameObject pixelParent;
        public int currentPlayer = 0;
        public RawImage[,] flagPixels;


        protected override void SetState(UIState state)
        {
            base.SetState(state);
            if ((int)state >= 2)
            {
                currentPlayer = 0;
                ReconstructFlag();
            }
        }

        public void ReconstructFlag()
        {
            float[,,] flag = BattleTweaker.flags[currentPlayer];
            Vector2 pixelDimensions = new Vector2(rect.rect.width / flag.GetLength(0), rect.rect.height / flag.GetLength(1));
            Vector2 beginningPos = -rect.rect.size / 2.0f + pixelDimensions / 2.0f;

            Destroy(pixelParent);
            pixelParent = new GameObject("Pixel Parent");
            pixelParent.transform.SetParent(transform, false);

            flagPixels = new RawImage[flag.GetLength(0), flag.GetLength(1)];

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

                    flagPixels[x, y] = image;
                }
            }
        }


        public bool GetPixelPosition(Vector2 screenInputPos, out Vector2Int pixelPos)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenInputPos))
            {
                Vector2 localPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenInputPos, null, out localPosition);

                Vector2 positionFromLowerLeft = localPosition + rect.rect.size / 2.0f;
                Vector2 normalizedPosition = new Vector2(positionFromLowerLeft.x / rect.rect.width, positionFromLowerLeft.y / rect.rect.height);
                pixelPos = new Vector2Int(Mathf.FloorToInt(normalizedPosition.x * flagPixels.GetLength(0)), Mathf.FloorToInt(normalizedPosition.y * flagPixels.GetLength(1)));
                return true;
            }

            pixelPos = Vector2Int.one;
            return false;
        }

        public bool TryPaint(Color color, Vector2 screenInputPos)
        {
            Vector2Int pixelPos;
            if (GetPixelPosition(screenInputPos, out pixelPos))
            {
                flagPixels[pixelPos.x, pixelPos.y].color = color;
                return true;
            }

            return false;
        }

        public bool TryFill(Color color, Vector2 screenInputPos)
        {
            Vector2Int pixelPos;
            if (GetPixelPosition(screenInputPos, out pixelPos))
            {
                return RecursiveFloodFill(flagPixels[pixelPos.x, pixelPos.y].color, color, pixelPos);
            }
            return false;
        }

        bool RecursiveFloodFill(Color replacedColor, Color newColor, Vector2Int point)
        {
            if (replacedColor == newColor)
            {
                return false;
            }

            if (point.x >= 0 && point.y >= 0 && point.x < flagPixels.GetLength(0) && point.y < flagPixels.GetLength(1) && flagPixels[point.x, point.y].color == replacedColor)
            {
                flagPixels[point.x, point.y].color = newColor;
            }
            else
            {
                return false;
            }

            RecursiveFloodFill(replacedColor, newColor, point + Vector2Int.up);
            RecursiveFloodFill(replacedColor, newColor, point + Vector2Int.down);
            RecursiveFloodFill(replacedColor, newColor, point + Vector2Int.right);
            RecursiveFloodFill(replacedColor, newColor, point + Vector2Int.left);
            return true;
        }
    }
}
