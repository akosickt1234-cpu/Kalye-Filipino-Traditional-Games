using System.IO;
using UnityEditor;
using UnityEngine;

public static class AvatarBaker
{
    const string BOY_MODEL_PATH = "Assets/Resources/Characters/timmy/timmy.fbx";
    const string GIRL_MODEL_PATH = "Assets/Resources/Characters/amy/amy.fbx";
    const string OUTPUT_FOLDER = "Assets/Resources/Avatars";
    const string BOY_FILE_NAME = "AvatarBoy1.png";
    const string GIRL_FILE_NAME = "AvatarGirl1.png";
    const string MENU_PATH = "Kalye/Bake Avatars";

    const int IMAGE_SIZE = 1024;
    const int DEPTH_BITS = 24;
    const int ANTI_ALIASING = 4;
    const float FACE_FILL = 0.7f;
    const float HEAD_HEIGHT_RATIO = 0.14f;
    const float HEAD_CENTER_OFFSET = 0.4f;
    const float MODEL_YAW = -15f;
    const float CAMERA_DISTANCE = 10f;
    const float KEY_LIGHT_INTENSITY = 1.1f;
    const float RIM_LIGHT_INTENSITY = 1.4f;
    static readonly Vector3 KEY_LIGHT_ANGLES = new Vector3(25f, 160f, 0f);
    static readonly Vector3 RIM_LIGHT_ANGLES = new Vector3(20f, 20f, 0f);
    static readonly Color KEY_LIGHT_COLOR = new Color(1f, 0.98f, 0.95f, 1f);
    static readonly Color RIM_LIGHT_COLOR = new Color(1f, 0.78f, 0.3f, 1f);
    static readonly Color AMBIENT_COLOR = new Color(0.45f, 0.45f, 0.5f, 1f);

    [MenuItem(MENU_PATH)]
    public static void bake_avatars()
    {
        var baked = 0;
        baked += bake_one(BOY_MODEL_PATH, BOY_FILE_NAME) ? 1 : 0;
        baked += bake_one(GIRL_MODEL_PATH, GIRL_FILE_NAME) ? 1 : 0;
        AssetDatabase.Refresh();
        Debug.Log("avatar baker: " + baked + " of 2 avatars baked into " + OUTPUT_FOLDER);
    }

    static bool bake_one(string model_path, string file_name)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(model_path);
        if (model == null)
        {
            Debug.LogError("avatar baker: model not found at " + model_path);
            return false;
        }

        PreviewRenderUtility preview = null;
        GameObject instance = null;
        RenderTexture render_texture = null;
        Texture2D texture = null;
        var previous_active = RenderTexture.active;

        try
        {
            preview = new PreviewRenderUtility();
            instance = Object.Instantiate(model);
            preview.AddSingleGO(instance);

            var animator = instance.GetComponentInChildren<Animator>();
            if (animator != null) animator.enabled = false;

            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.Euler(0f, MODEL_YAW, 0f);

            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                Debug.LogError("avatar baker: no renderers in " + model_path);
                return false;
            }

            warn_missing_textures(renderers, model_path);

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            var head_size = bounds.size.y * HEAD_HEIGHT_RATIO;
            var head_center = find_head_center(animator, bounds, head_size);

            setup_camera(preview, head_center, head_size);
            setup_lights(preview);

            render_texture = new RenderTexture(IMAGE_SIZE, IMAGE_SIZE, DEPTH_BITS, RenderTextureFormat.ARGB32);
            render_texture.antiAliasing = ANTI_ALIASING;
            preview.camera.targetTexture = render_texture;
            preview.camera.Render();

            RenderTexture.active = render_texture;
            texture = new Texture2D(IMAGE_SIZE, IMAGE_SIZE, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, IMAGE_SIZE, IMAGE_SIZE), 0, 0);
            texture.Apply();

            write_png(texture, file_name);
            return true;
        }
        finally
        {
            RenderTexture.active = previous_active;
            if (preview != null) preview.camera.targetTexture = null;
            if (render_texture != null)
            {
                render_texture.Release();
                Object.DestroyImmediate(render_texture);
            }
            if (texture != null) Object.DestroyImmediate(texture);
            if (instance != null) Object.DestroyImmediate(instance);
            if (preview != null) preview.Cleanup();
        }
    }

    static Vector3 find_head_center(Animator animator, Bounds bounds, float head_size)
    {
        if (animator != null && animator.isHuman)
        {
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            if (head != null) return head.position + Vector3.up * head_size * HEAD_CENTER_OFFSET;
        }
        return new Vector3(bounds.center.x, bounds.max.y - head_size * 0.5f, bounds.center.z);
    }

    static void setup_camera(PreviewRenderUtility preview, Vector3 head_center, float head_size)
    {
        var camera = preview.camera;
        camera.orthographic = true;
        camera.orthographicSize = head_size / FACE_FILL * 0.5f;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = CAMERA_DISTANCE * 4f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        camera.allowHDR = false;
        camera.transform.position = head_center + Vector3.forward * CAMERA_DISTANCE;
        camera.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    static void setup_lights(PreviewRenderUtility preview)
    {
        preview.ambientColor = AMBIENT_COLOR;

        var key = preview.lights[0];
        key.type = LightType.Directional;
        key.color = KEY_LIGHT_COLOR;
        key.intensity = KEY_LIGHT_INTENSITY;
        key.shadows = LightShadows.None;
        key.transform.rotation = Quaternion.Euler(KEY_LIGHT_ANGLES);

        var rim = preview.lights[1];
        rim.type = LightType.Directional;
        rim.color = RIM_LIGHT_COLOR;
        rim.intensity = RIM_LIGHT_INTENSITY;
        rim.shadows = LightShadows.None;
        rim.transform.rotation = Quaternion.Euler(RIM_LIGHT_ANGLES);
    }

    static void warn_missing_textures(Renderer[] renderers, string model_path)
    {
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material == null || material.mainTexture == null)
                {
                    var material_name = material == null ? "none" : material.name;
                    Debug.LogWarning("avatar baker: no main texture on " + renderer.name + " (material " + material_name + ") in " + model_path);
                }
            }
        }
    }

    static void write_png(Texture2D texture, string file_name)
    {
        var project_root = Directory.GetParent(Application.dataPath).FullName;
        var folder = Path.Combine(project_root, OUTPUT_FOLDER);
        Directory.CreateDirectory(folder);
        File.WriteAllBytes(Path.Combine(folder, file_name), texture.EncodeToPNG());

        var asset_path = OUTPUT_FOLDER + "/" + file_name;
        AssetDatabase.ImportAsset(asset_path, ImportAssetOptions.ForceUpdate);

        var importer = AssetImporter.GetAtPath(asset_path) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.maxTextureSize = IMAGE_SIZE;
        importer.SaveAndReimport();
    }
}
