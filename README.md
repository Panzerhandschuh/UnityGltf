# UnityGltf
glTF importer for Unity

# Usage
Loading a non-reference counted glTF asset:
```
GltfLoader loader = new GltfLoader(gltfPath);
GameObject obj = loader.Load();
```

Loading a reference counted glTF asset:
```
GameObject obj = AssetManager.Instance.LoadGltf(gltfPath);
```

# Example
Load Scenes/SampleScene.unity and press play
