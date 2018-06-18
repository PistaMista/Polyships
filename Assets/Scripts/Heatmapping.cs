﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace Heatmapping
{
    public static class Extensions
    {
        public delegate float Heatspreader(float amount, int axial_distance);
        public delegate void Injector<TSubject, TInjected>(ref TSubject subject, TInjected injection);
        public static float[,] AddHeat(this float[,] array, Vector2Int position, float amount, Heatspreader spreader)
        {
            float[] values = new float[Mathf.Max(position.x, position.y, array.GetLength(0) - position.x, array.GetLength(1) - position.y)];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = spreader(amount, i);
            }

            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    int distance = Mathf.Abs(position.x - x) + Mathf.Abs(position.y - y);
                    array[x, y] += values[distance];
                }
            }

            return array;
        }

        public static Vector2Int Max(this float[,] array)
        {
            Vector2Int highest = Vector2Int.zero;
            float highestValue = array[0, 0];
            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    float value = array[x, y];
                    if (value > highestValue)
                    {
                        highest = new Vector2Int(x, y);
                        highestValue = value;
                    }
                }
            }

            return highest;
        }

        public static Vector2Int Min(this float[,] array)
        {
            Vector2Int lowest = Vector2Int.zero;
            float lowestValue = array[0, 0];
            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    float value = array[x, y];
                    if (value < lowestValue)
                    {
                        lowest = new Vector2Int(x, y);
                        lowestValue = value;
                    }
                }
            }

            return lowest;
        }

        public static float[,] Add(this float[,] a, float[,] b)
        {
            for (int x = 0; x < a.GetLength(0); x++)
            {
                for (int y = 0; y < a.GetLength(1); y++)
                {
                    a[x, y] += b[x, y];
                }
            }

            return a;
        }

        public static float[,] Scale(this float[,] array, float scale)
        {
            for (int x = 0; x < array.GetLength(0); x++)
            {
                for (int y = 0; y < array.GetLength(1); y++)
                {
                    array[x, y] *= scale;
                }
            }

            return array;
        }

        public static TOutput[,] ExtractArray<TInput, TOutput>(this TInput[,] input, Converter<TInput, TOutput> converter)
        {
            TOutput[,] output = new TOutput[input.GetLength(0), input.GetLength(1)];
            for (int x = 0; x < input.GetLength(0); x++)
            {
                for (int y = 0; y < input.GetLength(1); y++)
                {
                    output[x, y] = converter(input[x, y]);
                }
            }

            return output;
        }

        public static void InjectArray<TSubject, TInjected>(this TSubject[,] subject, TInjected[,] injection, Injector<TSubject, TInjected> injector)
        {
            for (int x = 0; x < subject.GetLength(0); x++)
            {
                for (int y = 0; y < subject.GetLength(1); y++)
                {
                    injector(ref subject[x, y], injection[x, y]);
                }
            }
        }

    }
}