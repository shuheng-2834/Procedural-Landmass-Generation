using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator {
    public static float[,] GeneratorFalloffMap(int size) {
        float[,] map = new float[size, size];
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                // 选出哪个哪个数字更接近1
                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }
        return map;
    }

    static float Evaluate(float value)
    {
        float a = 3;
        float b = 2.2f;

        /* 
                 x^a
           ---------------
            x^a + (b-bx)^a
        */
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
