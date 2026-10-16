using System.Collections.Generic;
using UnityEngine;

namespace StickmanArena
{
    /// <summary>
    /// Piso hecho de baldosas que se cae por partes, de atrás hacia adelante, durante una
    /// transición jugable. Cada baldosa tiembla un instante y después cae con física simple
    /// (sin Rigidbody: una caída animada cuesta menos CPU).
    /// </summary>
    public class CollapsingDeck : MonoBehaviour
    {
        public int Columns = 6;
        public int Rows = 12;
        public float TileSize = 2f;
        public Material TileMaterialA;
        public Material TileMaterialB;

        readonly List<Transform> _tiles = new List<Transform>();
        readonly List<float> _dropAt = new List<float>();
        readonly List<Vector3> _rest = new List<Vector3>();
        float _clock = -1f;
        bool _collapsing;

        public float Length => Rows * TileSize;

        public void Build()
        {
            for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Columns; c++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"Tile_{r}_{c}";
                tile.transform.SetParent(transform, false);
                float x = (c - (Columns - 1) * 0.5f) * TileSize;
                float z = (r - (Rows - 1) * 0.5f) * TileSize;
                tile.transform.localPosition = new Vector3(x, -0.25f, z);
                tile.transform.localScale = new Vector3(TileSize * 0.98f, 0.5f, TileSize * 0.98f);
                var rend = tile.GetComponent<Renderer>();
                rend.sharedMaterial = (r + c) % 2 == 0 ? TileMaterialA : TileMaterialB;
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _tiles.Add(tile.transform);
                _rest.Add(tile.transform.localPosition);
                _dropAt.Add(float.PositiveInfinity);
            }
        }

        /// <summary>
        /// Empieza a tirar las filas desde la cola (z negativo) hacia adelante. Se derrumba
        /// la fracción indicada del largo (0,5 = la mitad de atrás) a lo largo de "duration" segundos.
        /// </summary>
        public void StartCollapse(float duration, float fractionOfLength = 0.5f)
        {
            int rowsToDrop = Mathf.RoundToInt(Rows * fractionOfLength);
            for (int i = 0; i < _tiles.Count; i++)
            {
                int row = i / Columns;
                if (row >= rowsToDrop) continue;
                float t = duration * (row + Random.value * 0.6f) / rowsToDrop;
                _dropAt[i] = t;
            }
            _clock = 0f;
            _collapsing = true;
        }

        void Update()
        {
            if (!_collapsing) return;
            _clock += Time.deltaTime;
            bool anyLeft = false;
            for (int i = 0; i < _tiles.Count; i++)
            {
                float d = _dropAt[i];
                if (float.IsInfinity(d)) continue;
                float since = _clock - d;
                var tile = _tiles[i];
                if (since < -0.6f) { anyLeft = true; continue; }
                if (since < 0f)
                {
                    // aviso: la baldosa tiembla
                    tile.localPosition = _rest[i] + new Vector3(Mathf.Sin(_clock * 60f) * 0.05f, 0f, 0f);
                    anyLeft = true;
                    continue;
                }
                var col = tile.GetComponent<Collider>();
                if (col && col.enabled) col.enabled = false;
                float fall = 0.5f * 20f * since * since;
                tile.localPosition = _rest[i] + new Vector3(0f, -fall, 0f);
                tile.localRotation = Quaternion.Euler(since * 90f, 0f, since * 40f);
                if (fall > 40f) tile.gameObject.SetActive(false); else anyLeft = true;
            }
            if (!anyLeft) _collapsing = false;
        }
    }
}
