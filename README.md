# UnityGltf
glTF importer for Unity (similar to https://github.com/KhronosGroup/UnityGLTF)

Use https://github.com/Panzerhandschuh/UnityGltfTool to configure colliders and convert images to dds files.

Features:
- Runtime gltf/glb importing
- Animation importing (excluding morph targets)
- DDS extension (https://github.com/Panzerhandschuh/glTF/tree/master/extensions/2.0/Vendor/MSFT_texture_dds)
- Unity collider extension (https://github.com/Panzerhandschuh/glTF/tree/master/extensions/2.0/Vendor/Unity_colliders)
- Reference counting (assets loaded with the AssetManager will re-use resources if they are already loaded)

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
