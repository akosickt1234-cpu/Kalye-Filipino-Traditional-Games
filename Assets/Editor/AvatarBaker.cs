using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class AvatarBaker
{
    const string BOY_MODEL_PATH = "Assets/Resources/Characters/timmy/timmy.fbx";
    const string GIRL_MODEL_PATH = "Assets/Resources/Characters/amy/amy.fbx";
    const string BOY_FALLBACK_TEXTURE_PATH = "Assets/Resources/Characters/char_texture/Ch09_1001_Diffuse.png";
    const string GIRL_FALLBACK_TEXTURE_PATH = "Assets/Resources/Characters/char_texture/Ch46_1001_Diffuse 4.png";
    const string OUTPUT_FOLDER = "Assets/Resources/Avatars";
    const string BOY_FILE_NAME = "AvatarBoy1.png";
    const string GIRL_FILE_NAME = "AvatarGirl1.png";
    const string MENU_PATH = "Kalye/Bake Avatars";
    const string FALLBACK_SHADER_NAME = "Standard";
    const string HEAD_NAME_SUFFIX = "head";
    const string HEAD_EXCLUDE_TOP = "top";
    const string HEAD_EXCLUDE_END = "end";

    const bool USE_STANDARD_SHADER_FALLBACK = false;
    const int IMAGE_SIZE = 1024;
    const int DEPTH_BITS = 24;
    const int ANTI_ALIASING = 4;
    const int RENDER_LAYER = 31;
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
        baked += bake_one(BOY_MODEL_PATH, BOY_FILE_NAME, BOY_FALLBACK_TEXTURE_PATH) ? 1 : 0;
        baked += bake_one(GIRL_MODEL_PATH, GIRL_FILE_NAME, GIRL_FALLBACK_TEXTURE_PATH) ? 1 : 0;
        AssetDatabase.Refresh();
        Debug.Log("avatar baker: " + baked + " of 2 avatars baked into " + OUTPUT_FOLDER);
    }

    static bool bake_one(string model_path, string file_name, string fallback_texture_path)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(model_path);
        if (model == null)
        {
            Debug.LogError("avatar baker: model not found at " + model_path);
            return false;
        }

        var previous_scene = SceneManager.GetActiveScene();
        var previous_render_texture = RenderTexture.active;
        var temp_scene = default(Scene);
        RenderTexture render_texture = null;
        Texture2D texture = null;
        var temp_materials = new List<Material>();

        try
        {
            temp_scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(temp_scene);

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = AMBIENT_COLOR;

            var instance = Object.Instantiate(model);
            set_layer_recursive(instance, RENDER_LAYER);

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

            if (USE_STANDARD_SHADER_FALLBACK) apply_standard_fallback(renderers, fallback_texture_path, temp_materials);
            log_renderers(renderers);

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            var head_size = bounds.size.y * HEAD_HEIGHT_RATIO;
            var head_center = find_head_center(instance, animator, bounds, head_size);

            var camera = create_camera(head_center, head_size);
            create_lights();

            render_texture = new RenderTexture(IMAGE_SIZE, IMAGE_SIZE, DEPTH_BITS, RenderTextureFormat.ARGB32);
            render_texture.antiAliasing = ANTI_ALIASING;
            camera.targetTexture = render_texture;
            camera.Render();

            RenderTexture.active = render_texture;
            texture = new Texture2D(IMAGE_SIZE, IMAGE_SIZE, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, IMAGE_SIZE, IMAGE_SIZE), 0, 0);
            texture.Apply();

            write_png(texture, file_name);
            return true;
        }
        finally
        {
            RenderTexture.active = previous_render_texture;
            if (render_texture != null)
            {
                render_texture.Release();
                Object.DestroyImmediate(render_texture);
            }
            if (texture != null) Object.DestroyImmediate(texture);
            foreach (var material in temp_materials)
            {
                if (material != null) Object.DestroyImmediate(material);
            }
            if (previous_scene.IsValid() && previous_scene.isLoaded) SceneManager.SetActiveScene(previous_scene);
            if (temp_scene.IsValid()) EditorSceneManager.CloseScene(temp_scene, true);
        }
    }

    static void set_layer_recursive(GameObject root, int layer)
    {
        foreach (var child in root.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = layer;
    }

    static Camera create_camera(Vector3 head_center, float head_size)
    {
        var camera_object = new GameObject("avatar_baker_camera");
        var camera = camera_object.AddComponent<Camera>();
        var camera_data = camera_object.GetComponent<UniversalAdditionalCameraData>();
        if (camera_data == null) camera_data = camera_object.AddComponent<UniversalAdditionalCameraData>();

        camera_data.renderType = CameraRenderType.Base;
        camera_data.renderPostProcessing = false;

        camera.orthographic = true;
        camera.orthographicSize = head_size / FACE_FILL * 0.5f;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = CAMERA_DISTANCE * 4f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        camera.allowHDR = false;
        camera.cullingMask = 1 << RENDER_LAYER;
        camera.transform.position = head_center + Vector3.forward * CAMERA_DISTANCE;
        camera.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        return camera;
    }

    static void create_lights()
    {
        create_light("avatar_baker_key_light", KEY_LIGHT_COLOR, KEY_LIGHT_INTENSITY, KEY_LIGHT_ANGLES);
        create_light("avatar_baker_rim_light", RIM_LIGHT_COLOR, RIM_LIGHT_INTENSITY, RIM_LIGHT_ANGLES);
    }

    static void create_light(string light_name, Color color, float intensity, Vector3 angles)
    {
        var light_object = new GameObject(light_name);
        var light = light_object.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = color;
        light.intensity = intensity;
        light.shadows = LightShadows.None;
        light.transform.rotation = Quaternion.Euler(angles);
    }

    static Vector3 find_head_center(GameObject instance, Animator animator, Bounds bounds, float head_size)
    {
        var head = find_head_transform(instance, animator);
        if (head != null)
        {
            Debug.Log("avatar baker: head transform " + head.name + " in " + instance.name);
            return head.position + Vector3.up * head_size * HEAD_CENTER_OFFSET;
        }

        Debug.Log("avatar baker: head transform not found in " + instance.name + ", using bounds fallback");
        return new Vector3(bounds.center.x, bounds.max.y - head_size * 0.5f, bounds.center.z);
    }

    static Transform find_head_transform(GameObject instance, Animator animator)
    {
        if (animator != null && animator.isHuman)
        {
            var human_head = animator.GetBoneTransform(HumanBodyBones.Head);
            if (human_head != null) return human_head;
        }

        foreach (var candidate in instance.GetComponentsInChildren<Transform>(true))
        {
            var lower_name = candidate.name.ToLowerInvariant();
            if (!lower_name.EndsWith(HEAD_NAME_SUFFIX)) continue;
            if (lower_name.Contains(HEAD_EXCLUDE_TOP) || lower_name.Contains(HEAD_EXCLUDE_END)) continue;
            return candidate;
        }
        return null;
    }

    static void log_renderers(Renderer[] renderers)
    {
        foreach (var renderer in renderers)
        {
            var parts = new List<string>();
            foreach (var material in renderer.sharedMaterials)
            {
                if (material == null)
                {
                    parts.Add("material none");
                    continue;
                }
                var shader_name = material.shader != null ? material.shader.name : "none";
                var texture_name = material.mainTexture != null ? material.mainTexture.name : "none";
                parts.Add("material " + material.name + ", shader " + shader_name + ", main texture " + texture_name);
            }
            Debug.Log("avatar baker: renderer " + renderer.name + " | " + string.Join(" | ", parts));
        }
    }

    static void apply_standard_fallback(Renderer[] renderers, string fallback_texture_path, List<Material> temp_materials)
    {
        var shader = Shader.Find(FALLBACK_SHADER_NAME);
        var fallback_texture = AssetDatabase.LoadAssetAtPath<Texture2D>(fallback_texture_path);
        if (shader == null)
        {
            Debug.LogError("avatar baker: shader not found: " + FALLBACK_SHADER_NAME);
            return;
        }

        foreach (var renderer in renderers)
        {
            var originals = renderer.sharedMaterials;
            var replacements = new Material[originals.Length];
            for (var i = 0; i < originals.Length; i++)
            {
                var albedo = originals[i] != null && originals[i].mainTexture != null ? originals[i].mainTexture : fallback_texture;
                var replacement = new Material(shader);
                replacement.mainTexture = albedo;
                temp_materials.Add(replacement);
                replacements[i] = replacement;
            }
            renderer.sharedMaterials = replacements;
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
