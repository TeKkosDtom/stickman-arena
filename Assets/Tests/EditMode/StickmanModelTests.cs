using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace StickmanArena.Tests
{
    /// <summary>
    /// Comprueba que el modelo de Blender se importa en Unity como espera StickmanView:
    /// rig con hueso "Head", los cuatro clips con su nombre, bucle en Idle/Run, dos ranuras de
    /// material y menos de 1.000 triángulos (regla de "ligero siempre").
    /// </summary>
    public class StickmanModelTests
    {
        const string Path = "Assets/Models/Stickman/stickman.fbx";

        [Test]
        public void ElFbxExisteYSeImportaComoPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            Assert.IsNotNull(prefab, $"No se importó {Path}");
            Assert.IsNotNull(prefab.GetComponentInChildren<SkinnedMeshRenderer>(), "El modelo debe venir con SkinnedMeshRenderer (rig)");
            Assert.IsNotNull(FindDeep(prefab.transform, "Head"), "Falta el hueso Head (lo usa el accesorio del estilo)");
        }

        [Test]
        public void TieneLosCuatroClipsYLosBuclesCorrectos()
        {
            var clips = AssetDatabase.LoadAllAssetRepresentationsAtPath(Path).OfType<AnimationClip>().ToList();
            foreach (var name in new[] { "Idle", "Run", "Punch", "Jump" })
            {
                var clip = clips.FirstOrDefault(c => c.name == name);
                Assert.IsNotNull(clip, $"Falta el clip {name}; hay: {string.Join(", ", clips.Select(c => c.name))}");
                Assert.Greater(clip.length, 0.1f, $"El clip {name} está vacío");
                bool shouldLoop = name == "Idle" || name == "Run";
                Assert.AreEqual(shouldLoop, clip.isLooping, $"Bucle incorrecto en {name} (revisar StickmanModelImporter)");
            }
        }

        [Test]
        public void EsLowPolyYTieneDosRanurasDeMaterial()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            var smr = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            int tris = 0;
            for (int i = 0; i < smr.sharedMesh.subMeshCount; i++) tris += (int)smr.sharedMesh.GetIndexCount(i) / 3;
            Assert.Less(tris, 1000, $"El stickman tiene {tris} triángulos; el tope es 1.000");
            Assert.AreEqual(2, smr.sharedMesh.subMeshCount, "Se esperan dos submallas: tinta y ojos/sonrisa");
        }

        static Transform FindDeep(Transform t, string childName)
        {
            if (t.name == childName) return t;
            for (int i = 0; i < t.childCount; i++)
            {
                var r = FindDeep(t.GetChild(i), childName);
                if (r != null) return r;
            }
            return null;
        }
    }
}
