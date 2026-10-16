#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace StickmanArena.EditorTools
{
    /// <summary>
    /// Punto de entrada de GameCI. Deja el proyecto en URP de bajo consumo, crea la escena
    /// de la demo (si no existe) y compila WebGL. También se puede usar desde el menú.
    /// </summary>
    public static class BuildScript
    {
        const string ScenePath = "Assets/Scenes/Demo.unity";
        const string UrpAssetPath = "Assets/Settings/URP-LowPoly.asset";
        const string UrpRendererPath = "Assets/Settings/URP-LowPoly-Renderer.asset";
        const string FlatShaderName = "Universal Render Pipeline/Unlit";

        [MenuItem("StickMan Arena/Preparar proyecto (URP + escena demo)")]
        public static void Prepare()
        {
            EnsureUrp();
            EnsureDemoScene();
            ApplyLowPolyPlayerSettings();
            AssetDatabase.SaveAssets();
        }

        /// <summary>Método que invoca GameCI: -executeMethod StickmanArena.EditorTools.BuildScript.BuildWebGL</summary>
        public static void BuildWebGL()
        {
            Prepare();
            string output = Path.Combine("build", "WebGL", "StickManArena");
            Directory.CreateDirectory(output);

            // Compresión gzip con fallback: GitHub Pages no manda la cabecera Content-Encoding,
            // así que el cargador descomprime en JavaScript.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
            PlayerSettings.WebGL.initialMemorySize = 256; // MB (memorySize está obsoleto en Unity 6)
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.High);
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.WebGL, Il2CppCompilerConfiguration.Release);
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL, UnityEditor.Build.Il2CppCodeGeneration.OptimizeSize);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"Build WebGL: {report.summary.result}, {report.summary.totalSize / (1024 * 1024)} MB, {report.summary.totalErrors} errores");
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }

        static void EnsureUrp()
        {
            Directory.CreateDirectory("Assets/Settings");
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            if (asset == null)
            {
                // El renderer tiene que ser un asset propio: si queda como objeto suelto en memoria,
                // la referencia se pierde al guardar y el build arranca sin renderer (no dibuja nada).
                var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(UrpRendererPath);
                if (renderer == null)
                {
                    renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                    AssetDatabase.CreateAsset(renderer, UrpRendererPath);
                }
                asset = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(asset, UrpAssetPath);
            }
            // Perfil de bajo consumo: sin sombras en tiempo real, sin HDR, sin post-procesado
            asset.supportsHDR = false;
            asset.msaaSampleCount = 1;
            asset.supportsCameraDepthTexture = false;
            asset.supportsCameraOpaqueTexture = false;
            asset.shadowDistance = 0f;
            asset.renderScale = 1f;
            EditorUtility.SetDirty(asset);
            GraphicsSettings.defaultRenderPipeline = asset;
            QualitySettings.renderPipeline = asset;
        }

        const string StickmanModelPath = "Assets/Models/Stickman/stickman.fbx";

        /// <summary>
        /// La escena no se versiona: se regenera en cada build para que siempre lleve las
        /// referencias actuales (shader plano, modelo de Blender y sus clips).
        /// </summary>
        static void EnsureDemoScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var boot = new GameObject("Demo");
            var demo = boot.AddComponent<DemoBootstrap>();
            // Shader.Find no encuentra shaders que no estén referenciados en el build; al guardarlo
            // en la escena, el shader entra en el WebGL sin tocar "Always Included Shaders".
            demo.FlatShader = Shader.Find(FlatShaderName) ?? Shader.Find("Unlit/Color");

            // Modelo de Blender y sus clips: referenciados desde la escena para que entren en el build
            demo.StickmanPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StickmanModelPath);
            if (demo.StickmanPrefab != null)
            {
                foreach (var obj in AssetDatabase.LoadAllAssetRepresentationsAtPath(StickmanModelPath))
                {
                    if (!(obj is AnimationClip clip)) continue;
                    switch (clip.name)
                    {
                        case "Idle": demo.IdleClip = clip; break;
                        case "Run": demo.RunClip = clip; break;
                        case "Punch": demo.PunchClip = clip; break;
                        case "Jump": demo.JumpClip = clip; break;
                    }
                }
                Debug.Log($"Demo: modelo de Blender {(demo.IdleClip && demo.RunClip && demo.PunchClip && demo.JumpClip ? "con 4 clips" : "SIN todos los clips (se usará el stickman procedural)")}");
            }
            else Debug.LogWarning($"Demo: no se encontró {StickmanModelPath}; se usará el stickman procedural");

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        static void ApplyLowPolyPlayerSettings()
        {
            PlayerSettings.companyName = "StickMan Arena";
            PlayerSettings.productName = "StickMan Arena";
            PlayerSettings.colorSpace = ColorSpace.Gamma; // más barato en GPUs integradas y WebGL
            PlayerSettings.WebGL.template = "APPLICATION:Minimal";
        }
    }
}
#endif
