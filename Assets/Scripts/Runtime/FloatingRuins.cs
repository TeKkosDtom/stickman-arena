using System.Collections.Generic;
using UnityEngine;

namespace StickmanArena
{
    /// <summary>
    /// Última fase de "Cielo de Papel": trozos del avión que flotan en el cielo y suben y bajan
    /// despacio. Son cubos planos con collider; el movimiento es por código, sin Rigidbody.
    /// </summary>
    public class FloatingRuins : MonoBehaviour
    {
        public Material Material;
        public float BobAmplitude = 0.35f;
        public float BobSpeed = 0.6f;

        readonly List<Transform> _ruins = new List<Transform>();
        readonly List<Vector3> _rest = new List<Vector3>();
        readonly List<float> _phase = new List<float>();

        /// <summary>Posición segura para reaparecer: encima de la primera ruina.</summary>
        public Vector3 SpawnPoint => _ruins.Count > 0 ? _ruins[0].position + Vector3.up * 2.5f : new Vector3(0f, 3f, 0f);

        public void Build(float riseDuration)
        {
            // Una ruina grande en el centro y cuatro alrededor, a distintas alturas
            Add(new Vector3(0f, 0f, 0f), new Vector3(5f, 0.5f, 5f), 0f);
            Add(new Vector3(-6f, -1f, 3f), new Vector3(3f, 0.5f, 3f), 1.2f);
            Add(new Vector3(6f, 0.5f, 2f), new Vector3(3f, 0.5f, 3f), 2.4f);
            Add(new Vector3(-3f, 1f, -6f), new Vector3(3f, 0.5f, 3f), 3.6f);
            Add(new Vector3(4f, -0.5f, -6f), new Vector3(3f, 0.5f, 3f), 4.8f);
            // Suben desde abajo para que se vean llegar
            _rise = riseDuration;
            _riseClock = 0f;
            foreach (var r in _ruins) r.localPosition += Vector3.down * 30f;
        }

        void Add(Vector3 pos, Vector3 size, float phase)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Ruina_" + _ruins.Count;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            var r = go.GetComponent<Renderer>();
            if (Material != null) r.sharedMaterial = Material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            _ruins.Add(go.transform);
            _rest.Add(pos);
            _phase.Add(phase);
        }

        float _rise, _riseClock;

        void Update()
        {
            _riseClock += Time.deltaTime;
            float rise = _rise > 0f ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_riseClock / _rise)) : 1f;
            for (int i = 0; i < _ruins.Count; i++)
            {
                float bob = Mathf.Sin(Time.time * BobSpeed + _phase[i]) * BobAmplitude;
                _ruins[i].localPosition = _rest[i] + Vector3.up * (bob - 30f * (1f - rise));
            }
        }
    }
}
