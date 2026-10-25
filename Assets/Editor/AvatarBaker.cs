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
    const string BOY_IDLE_CLIP_PATH = "Assets/Resources/Characters/timmy/Idle.anim";
    const string GIRL_IDLE_CLIP_PATH = "Assets/Resources/Characters/amy/Idle.anim";
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
    const string BASE_MAP_PROPERTY = "_BaseMap";
    const string MAIN_TEX_PROPERTY = "_MainTex";
    const string BUMP_MAP_PROPERTY = "_BumpMap";
    const string SPEC_GLOSS_MAP_PROPERTY = "_SpecGlossMap";
    const string METALLIC_GLOSS_MAP_PROPERTY = "_MetallicGlossMap";
    const string BASE_COLOR_PROPERTY = "_BaseColor";
    const string EMISSION_COLOR_PROPERTY = "_EmissionColor";
    const string METALLIC_PROPERTY = "_Metallic";
    const string SMOOTHNESS_PROPERTY = "_Smoothness";
    static readonly string[] SLOT_PROPERTIES = { BASE_MAP_PROPERTY, MAIN_TEX_PROPERTY, BUMP_MAP_PROPERTY, SPEC_GLOSS_MAP_PROPERTY, METALLIC_GLOSS_MAP_PROPERTY };
    static readonly string[] REPAIR_DISABLED_KEYWORDS = { "_NORMALMAP", "_METALLICSPECGLOSSMAP", "_SPECGLOSSMAP", "_EMISSION" };

    const bool USE_STANDARD_SHADER_FALLBACK = false;
    const bool BOY_REPAIR_MATERIALS = false;
    const bool GIRL_REPAIR_MATERIALS = true;
    const float REPAIR_SMOOTHNESS = 0.3f;
    const int IMAGE_SIZE = 1024;
    const int DEPTH_BITS = 24;
    const int ANTI_ALIASING = 4;
    const int RENDER_LAYER = 31;
    const float HEAD_FALLBACK_RATIO = 0.3f;
    const float FRAME_HEIGHT_IN_HEADS = 1.8f;
    const float HEAD_CENTER_OFFSET = 0.35f;
    const float IDLE_SAMPLE_TIME = 0f;
    const float MODEL_YAW = 0f;
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
        baked += bake_one(BOY_MODEL_PATH, BOY_FILE_NAME, BOY_IDLE_CLIP_PATH, BOY_FALLBACK_TEXTURE_PATH, BOY_REPAIR_MATERIALS) ? 1 : 0;
        baked += bake_one(GIRL_MODEL_PATH, GIRL_FILE_NAME, GIRL_IDLE_CLIP_PATH, GIRL_FALLBACK_TEXTURE_PATH, GIRL_REPAIR_MATERIALS) ? 1 : 0;
        AssetDatabase.Refresh();
        Debug.Log("avatar baker: " + baked + " of 2 avatars baked into " + OUTPUT_FOLDER);
    }

    static bool bake_one(string model_path, string file_name, string idle_clip_path, string fallback_texture_path, bool repair_materials)
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

            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.Euler(0f, MODEL_YAW, 0f);

            var animator = instance.GetComponentInChildren<Animator>();
            apply_idle_pose(instance, idle_clip_path);
            if (animator != null) animator.enabled = false;

            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                Debug.LogError("avatar baker: no renderers in " + model_path);
                return false;
            }

            log_slots(renderers, "before");
            if (repair_materials)
            {
                repair_slots(renderers, fallback_texture_path, temp_materials);
                log_slots(renderers, "after");
            }
            if (USE_STANDARD_SHADER_FALLBACK) apply_standard_fallback(renderers, fallback_texture_path, temp_materials);
            log_renderers(renderers);

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            var head = find_head_transform(instance, animator);
            var head_height = head != null ? bounds.max.y - head.position.y : bounds.size.y * HEAD_FALLBACK_RATIO;
            var head_base_y = head != null ? head.position.y : bounds.max.y - head_height;
            var center_x = head != null ? head.position.x : bounds.center.x;
            var center_z = head != null ? head.position.z : bounds.center.z;
            var center = new Vector3(center_x, head_base_y + head_height * HEAD_CENTER_OFFSET, center_z);
            var orthographic_size = head_height * FRAME_HEIGHT_IN_HEADS * 0.5f;

            Debug.Log("avatar baker: framing " + instance.name + ", head bone " + (head != null ? head.name : "none (bounds fallback)") + ", bounds height " + bounds.size.y + ", head height " + head_height + ", orthographic size " + orthographic_size);

            var camera = create_camera(center, orthographic_size);
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

    static void apply_idle_pose(GameObject instance, string idle_clip_path)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(idle_clip_path);
        if (clip == null)
        {
            Debug.LogWarning("avatar baker: idle clip not found at " + idle_clip_path + ", using bind pose");
            return;
        }
        clip.SampleAnimation(instance, IDLE_SAMPLE_TIME);
    }

    static Camera create_camera(Vector3 center, float orthographic_size)
    {
        var camera_object = new GameObject("avatar_baker_camera");
        var camera = camera_object.AddComponent<Camera>();
        var camera_data = camera_object.GetComponent<UniversalAdditionalCameraData>();
        if (camera_data == null) camera_data = camera_object.AddComponent<UniversalAdditionalCameraData>();

        camera_data.renderType = CameraRenderType.Base;
        camera_data.renderPostProcessing = false;

        camera.orthographic = true;
        camera.orthographicSize = orthographic_size;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = CAMERA_DISTANCE * 4f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = PlayTheme.panel_blue;
        camera.allowHDR = false;
        camera.cullingMask = 1 << RENDER_LAYER;
        camera.transform.position = center + Vector3.forward * CAMERA_DISTANCE;
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

    static void log_slots(Renderer[] renderers, string stage)
    {
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material == null) continue;
                var parts = new List<string>();
                foreach (var property in SLOT_PROPERTIES) parts.Add(property + "=" + describe_slot(material, property));
                parts.Add(BASE_COLOR_PROPERTY + "=" + describe_color(material, BASE_COLOR_PROPERTY));
                parts.Add(EMISSION_COLOR_PROPERTY + "=" + describe_color(material, EMISSION_COLOR_PROPERTY));
                var shader_name = material.shader != null ? material.shader.name : "none";
                Debug.Log("avatar baker: slots " + stage + " | renderer " + renderer.name + " | material " + material.name + " | shader " + shader_name + " | " + string.Join(", ", parts));
            }
        }
    }

    static string describe_slot(Material material, string property)
    {
        if (!material.HasProperty(property)) return "missing";
        var texture = material.GetTexture(property);
        return texture != null ? texture.name : "none";
    }

    static string describe_color(Material material, string property)
    {
        if (!material.HasProperty(property)) return "missing";
        return material.GetColor(property).ToString();
    }

    static void repair_slots(Renderer[] renderers, string diffuse_texture_path, List<Material> temp_materials)
    {
        var diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(diffuse_texture_path);
        if (diffuse == null)
        {
            Debug.LogError("avatar baker: diffuse texture not found at " + diffuse_texture_path + ", skipping repair");
            return;
        }

        foreach (var renderer in renderers)
        {
            var originals = renderer.sharedMaterials;
            var replacements = new Material[originals.Length];
            for (var i = 0; i < originals.Length; i++)
            {
                if (originals[i] == null) continue;
                var repaired = new Material(originals[i]);
                set_texture_if_present(repaired, BASE_MAP_PROPERTY, diffuse);
                set_texture_if_present(repaired, MAIN_TEX_PROPERTY, diffuse);
                set_texture_if_present(repaired, BUMP_MAP_PROPERTY, null);
                set_texture_if_present(repaired, SPEC_GLOSS_MAP_PROPERTY, null);
                set_texture_if_present(repaired, METALLIC_GLOSS_MAP_PROPERTY, null);
                if (repaired.HasProperty(BASE_COLOR_PROPERTY)) repaired.SetColor(BASE_COLOR_PROPERTY, Color.white);
                if (repaired.HasProperty(EMISSION_COLOR_PROPERTY)) repaired.SetColor(EMISSION_COLOR_PROPERTY, Color.black);
                if (repaired.HasProperty(METALLIC_PROPERTY)) repaired.SetFloat(METALLIC_PROPERTY, 0f);
                if (repaired.HasProperty(SMOOTHNESS_PROPERTY)) repaired.SetFloat(SMOOTHNESS_PROPERTY, REPAIR_SMOOTHNESS);
                foreach (var keyword in REPAIR_DISABLED_KEYWORDS) repaired.DisableKeyword(keyword);
                temp_materials.Add(repaired);
                replacements[i] = repaired;
            }
            renderer.sharedMaterials = replacements;
        }
    }

    static void set_texture_if_present(Material material, string property, Texture texture)
    {
        if (material.HasProperty(property)) material.SetTexture(property, texture);
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
