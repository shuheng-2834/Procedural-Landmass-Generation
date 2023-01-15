using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        MapGenerator mapGenerator = (MapGenerator)target;
        // 如果热更新被打开，则会随时更新噪点图
        if (DrawDefaultInspector()) {
            if (mapGenerator.autoUpdate) {
                mapGenerator.DrawMapInEditor();
            }
        }
        if (GUILayout.Button("Generate")) {
            mapGenerator.DrawMapInEditor();
        }
    }
}
