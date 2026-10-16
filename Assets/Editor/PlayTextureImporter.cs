using UnityEditor;
using UnityEngine;

public class PlayTextureImporter : AssetPostprocessor
{
    const string ROOT_PATH = "Assets/Resources/";
    const string PAGE_FOLDER = "/Play/";
    const string BACKGROUND_PATH = "Assets/Resources/Background/Play/";
    const int BACKGROUND_MAX_SIZE = 4096;
    const int DEFAULT_MAX_SIZE = 2048;

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(ROOT_PATH) || !assetPath.Contains(PAGE_FOLDER)) return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.maxTextureSize = assetPath.StartsWith(BACKGROUND_PATH) ? BACKGROUND_MAX_SIZE : DEFAULT_MAX_SIZE;
    }
}
