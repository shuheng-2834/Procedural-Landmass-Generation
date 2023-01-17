using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class MapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;

    public Material terrainMaterial;

    [Range(0, 6)]
    public int editorPreviewLOD;

    public bool autoUpdate;

    // 衰减图数组
    float[,] falloffMap;

    // 地图信息生成线程队列
    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<Meshdata>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<Meshdata>>();

    void OnValuesUpdated() {
        if (!Application.isPlaying) {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated() {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    public int mapChunkSize {
        get {
            if (terrainData.useFlatShading) {
                return 95;
            } else {
                return 239;
            }
        }
    }


    public void DrawMapInEditor() {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        // 判断枚举来选择生成对应的贴图
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        } else if (drawMode == DrawMode.Mesh) {
            display.DrawMesh(MeshGenerator.GeneratorTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplierl, terrainData.meshHeightCurve, editorPreviewLOD, terrainData.useFlatShading));
        } else if (drawMode == DrawMode.FalloffMap) {
            // 衰减图的绘制
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GeneratorFalloffMap(mapChunkSize)));
        }
    }
    // 请求地图信息
    public void RequestMapData(Vector2 centre, Action<MapData> callback) {
        ThreadStart threadStart = delegate {
            MapDataThread(centre, callback);
        };
        new Thread(threadStart).Start();
    }
    // 地图信息执行线程
    void MapDataThread(Vector2 centre, Action<MapData> callback) {
        MapData mapData = GenerateMapData(centre);
        // 确保只有一个线程可以执行这个代码
        lock (mapDataThreadInfoQueue) {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }
    public void RequestMeshData(MapData mapData, int lod, Action<Meshdata> callback) {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<Meshdata> callback) {
        Meshdata meshData = MeshGenerator.GeneratorTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplierl, terrainData.meshHeightCurve, lod, terrainData.useFlatShading);
        lock (meshDataThreadInfoQueue) {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<Meshdata>(callback, meshData));
        }
    }

    private void Update() {
        lock (mapDataThreadInfoQueue) {
            if (mapDataThreadInfoQueue.Count > 0) {
                for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
                    MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
            }
        }

        lock (meshDataThreadInfoQueue) {
            if (meshDataThreadInfoQueue.Count > 0) {
                for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
                    MapThreadInfo<Meshdata> threadInfo = meshDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
            }
        }
    }
    MapData GenerateMapData(Vector2 centre) {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode);

        if (terrainData.useFalloff) {
            if (falloffMap == null) {
                falloffMap = FalloffGenerator.GeneratorFalloffMap(mapChunkSize + 2);
            }
            for (int y = 0; y < mapChunkSize + 2; y++) {
                for (int x = 0; x < mapChunkSize + 2; x++) {
                    if (terrainData.useFalloff) {
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                    }
                }
            }
        }
        return new MapData(noiseMap);
    }
    /// <summary>
    /// 自动验证
    /// </summary>
    void OnValidate() {
        if (terrainData != null) {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if (noiseData != null) {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null) {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    struct MapThreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;
        public MapThreadInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
public struct MapData {
    public readonly float[,] heightMap;
    public MapData(float[,] heightMap) {
        this.heightMap = heightMap;
    }
}