using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities
{
    public static Vector2Int[] GetExtremeArrayElements(float[,] values, int topCount)
    {
        float[] flattenedArray = new float[values.GetLength(0) * values.GetLength(1)];
        for (int x = 0; x < values.GetLength(0); x++)
        {
            for (int y = 0; y < values.GetLength(0); y++)
            {
                flattenedArray[y * values.GetLength(0) + x] = values[x, y];
            }
        }

        int[] topResults = GetExtremeArrayElements(flattenedArray, topCount);

        Vector2Int[] convertedResults = new Vector2Int[topCount];

        for (int i = 0; i < topResults.Length; i++)
        {
            int index = topResults[i];
            convertedResults[i] =

        }

    }
    public static int[] GetExtremeArrayElements(float[] values, int topCount)
    {

    }
}
