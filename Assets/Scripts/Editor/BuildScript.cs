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
            PlayerSettings.WebGL.memorySize = 256;
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
                asset = UniversalRenderPipelineAsset.Create();
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

        static void EnsureDemoScene()
        {
            if (File.Exists(ScenePath)) return;
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var boot = new GameObject("Demo");
            boot.AddComponent<DemoBootstrap>();
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
