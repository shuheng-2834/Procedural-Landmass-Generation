﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TextureData : UpdatableData {

    public Color[] baseColours;
    [Range(0,1)]
    public float[] baseStartHeights;
    
    float saveMinHeight;
    float saveMaxHeight;

    public void ApplyToMaterial(Material material) {

        material.SetInt("baseColourCount", baseColours.Length);
        material.SetColorArray("baseColours", baseColours);
        material.SetFloatArray("baseStartHeights", baseStartHeights);

        UpdateMeshHeights(material, saveMinHeight, saveMaxHeight);
    }
    // 向着色器发送值
    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight) {
        saveMaxHeight = maxHeight;
        saveMinHeight = minHeight;

        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }
}
