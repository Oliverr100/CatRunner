#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates the particle prefabs, the particle material and the hit-flash material used by the polish scripts.
/// Run it from the menu: Tools > Polish > Build FX Assets  (safe to run again, it overwrites its own files).
/// </summary>
public static class PolishFxBuilder
{
    private const string Root = "Assets/Polish";
    public const string PixelTexturePath = Root + "/Textures/Pixel.png";
    public const string ParticleMaterialPath = Root + "/Materials/FX_Pixel.mat";
    public const string FlashMaterialPath = Root + "/Resources/PolishFlash.mat";
    public const string PrefabFolder = Root + "/Prefabs";

    [MenuItem("Tools/Polish/Build FX Assets")]
    public static void BuildAll()
    {
        EnsureFolder(Root);
        EnsureFolder(Root + "/Textures");
        EnsureFolder(Root + "/Materials");
        EnsureFolder(Root + "/Prefabs");
        EnsureFolder(Root + "/Resources");

        BuildPixelTexture();
        Material particleMat = BuildMaterial(ParticleMaterialPath, "Sprites/Default", AssetDatabase.LoadAssetAtPath<Texture2D>(PixelTexturePath));
        BuildMaterial(FlashMaterialPath, "Polish/SpriteFlash", null);

        // name,           gravity, shrink, rotate
        BuildPrefab("FX_Dust", particleMat, -0.05f, true, false);
        BuildPrefab("FX_Coin", particleMat, 1.2f, true, false);
        BuildPrefab("FX_Hit", particleMat, 1.6f, true, false);
        BuildPrefab("FX_Leaf", particleMat, 0.35f, false, true);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Polish] FX assets built in " + Root);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int i = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path.Substring(0, i), path.Substring(i + 1));
    }

    private static void BuildPixelTexture()
    {
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var px = new Color[16];
        for (int i = 0; i < px.Length; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();
        File.WriteAllBytes(PixelTexturePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(PixelTexturePath);

        var importer = (TextureImporter)AssetImporter.GetAtPath(PixelTexturePath);
        importer.textureType = TextureImporterType.Default;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static Material BuildMaterial(string path, string shaderName, Texture mainTex)
    {
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
        {
            Debug.LogError("[Polish] Shader not found: " + shaderName);
            return null;
        }

        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.shader = shader;
        if (mainTex != null) mat.mainTexture = mainTex;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void BuildPrefab(string name, Material mat, float gravity, bool shrink, bool spin)
    {
        var go = new GameObject(name);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.playOnAwake = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = 0.6f;
        main.startSpeed = 0f;
        main.startSize = 0.15f;
        main.startColor = Color.white;
        main.gravityModifier = gravity;
        main.maxParticles = 300;

        // all particles are emitted from code (GameFeel.Burst), so the system itself emits nothing
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.enabled = false;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.55f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, shrink ? AnimationCurve.Linear(0f, 1f, 1f, 0.2f) : AnimationCurve.Linear(0f, 1f, 1f, 1f));

        if (spin)
        {
            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-3f, 3f);       // radians per second, random per particle
        }

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.sharedMaterial = mat;
        r.sortingOrder = 50;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;

        PrefabUtility.SaveAsPrefabAsset(go, PrefabFolder + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
    }
}
#endif
