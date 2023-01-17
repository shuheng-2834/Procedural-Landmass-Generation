using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainData : UpdatableData {

    public float uniformScale = 1f;

    [Tooltip("是否使用平面着色")]
    public bool useFlatShading;
    [Tooltip("是否使用衰减图")]
    public bool useFalloff;

    public float meshHeightMultiplierl;
    public AnimationCurve meshHeightCurve;

    public float minHeight {
        get {
            return uniformScale * meshHeightMultiplierl * meshHeightCurve.Evaluate(0);
        }
    }
    public float maxHeight {
        get {
            return uniformScale * meshHeightMultiplierl * meshHeightCurve.Evaluate(1);
        }
    }
}
