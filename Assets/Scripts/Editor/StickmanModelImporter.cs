#if UNITY_EDITOR
using UnityEditor;

namespace StickmanArena.EditorTools
{
    /// <summary>
    /// Ajustes de importación del modelo de Blender (Assets/Models/Stickman/*.fbx) por código,
    /// porque no hay editor abierto para tocar el Inspector: rig Generic, sin materiales
    /// importados (los asigna StickmanView con el shader plano de URP) y clips Idle/Run en bucle.
    /// </summary>
    public class StickmanModelImporter : AssetPostprocessor
    {
        const string Folder = "Assets/Models/Stickman";

        bool IsStickman => assetPath.StartsWith(Folder) && assetPath.ToLowerInvariant().EndsWith(".fbx");

        // Subir el número cuando cambie la lógica para que Unity reimporte el modelo
        public override uint GetVersion() => 2;

        void OnPreprocessModel()
        {
            if (!IsStickman) return;
            var importer = (ModelImporter)assetImporter;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.importAnimation = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importBlendShapes = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.animationCompression = ModelImporterAnimationCompression.Optimal;
            importer.optimizeGameObjects = false; // StickmanView busca el hueso "Head" para el accesorio del estilo
        }

        void OnPreprocessAnimation()
        {
            if (!IsStickman) return;
            var importer = (ModelImporter)assetImporter;
            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                // Blender exporta las tomas como "StickmanRig|Idle": nos quedamos con el nombre de la acción
                int bar = clip.takeName.LastIndexOf('|');
                clip.name = bar >= 0 ? clip.takeName.Substring(bar + 1) : clip.takeName;
                bool loop = clip.name == "Idle" || clip.name == "Run";
                clip.loopTime = loop;
                clip.loopPose = loop;
                clip.lockRootRotation = true;
                clip.lockRootHeightY = false;
                clip.lockRootPositionXZ = true;
                clip.keepOriginalPositionY = true;
            }
            importer.clipAnimations = clips;
        }
    }
}
#endif
