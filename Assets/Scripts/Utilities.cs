using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities
{
    public static Vector2Int[] GetExtremeArrayElements(float[,] values, int topCount, bool coldest, float threshold)
    {
        float[] flattenedArray = new float[values.GetLength(0) * values.GetLength(1)];
        for (int x = 0; x < values.GetLength(0); x++)
        {
            for (int y = 0; y < values.GetLength(0); y++)
            {
                flattenedArray[y * values.GetLength(0) + x] = values[x, y];
            }
        }

        int[] topResults = GetExtremeArrayElements(flattenedArray, topCount, coldest, threshold);

        Vector2Int[] convertedResults = new Vector2Int[topCount];

        for (int i = 0; i < topResults.Length; i++)
        {
            int index = topResults[i];
            int y = Mathf.FloorToInt(index / (float)values.GetLength(0));
            convertedResults[i] = new Vector2Int(index - y * values.GetLength(0), y);
        }

        return convertedResults;
    }
    public static int[] GetExtremeArrayElements(float[] values, int topCount, bool coldest, float threshold)
    {
        List<int> bestIndices = new List<int>();
        float rankValue = coldest ? Mathf.NegativeInfinity : Mathf.Infinity;

        for (int cycle = 0; cycle < topCount; cycle++)
        {
            float bestValue = coldest ? Mathf.Infinity : Mathf.NegativeInfinity;
            int bestIndex = -1;

            for (int i = 0; i < values.Length; i++)
            {
                float value = values[i];

                bool lower = coldest ? value < bestValue : value > bestValue;
                bool upper = coldest ? value >= rankValue : value <= rankValue;
                bool limiter = coldest ? value > threshold : value < threshold;


                if (!bestIndices.Contains(i) && lower && upper && limiter)
                {
                    bestIndex = i;
                    bestValue = value;
                }
            }

            bestIndices.Add(bestIndex);
            rankValue = bestValue;
        }

        return bestIndices.ToArray();
    }
}
