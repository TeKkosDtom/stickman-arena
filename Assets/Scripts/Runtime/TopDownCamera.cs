using System.Collections.Generic;
using UnityEngine;

namespace StickmanArena
{
    /// <summary>
    /// Cámara cenital en tres cuartos, como Power Stone: sigue el centro de todos los
    /// jugadores y se aleja lo justo para que entren todos en pantalla.
    /// </summary>
    public class TopDownCamera : MonoBehaviour
    {
        public List<Transform> Targets = new List<Transform>();
        public float PitchDegrees = 55f;
        public float MinDistance = 9f;
        public float MaxDistance = 26f;
        public float Padding = 3f;
        public float Smoothing = 5f;

        [HideInInspector] public float ShakeAmount;

        Vector3 _focus;
        float _distance;

        void Start()
        {
            _distance = MinDistance;
            _focus = Centroid();
            Snap();
        }

        void LateUpdate()
        {
            if (Targets.Count == 0) return;
            Vector3 center = Centroid();
            float spread = 0f;
            foreach (var t in Targets)
                if (t) spread = Mathf.Max(spread, Vector3.Distance(center, t.position));

            float wanted = Mathf.Clamp(spread * 2f + Padding * 2f, MinDistance, MaxDistance);
            float k = 1f - Mathf.Exp(-Smoothing * Time.deltaTime);
            _focus = Vector3.Lerp(_focus, center, k);
            _distance = Mathf.Lerp(_distance, wanted, k);
            Snap();

            if (ShakeAmount > 0f)
            {
                transform.position += Random.insideUnitSphere * ShakeAmount * 0.3f;
                ShakeAmount = Mathf.MoveTowards(ShakeAmount, 0f, Time.deltaTime * 2f);
            }
        }

        void Snap()
        {
            Quaternion rot = Quaternion.Euler(PitchDegrees, 0f, 0f);
            transform.rotation = rot;
            transform.position = _focus - rot * Vector3.forward * _distance;
        }

        Vector3 Centroid()
        {
            Vector3 sum = Vector3.zero; int n = 0;
            foreach (var t in Targets) if (t) { sum += t.position; n++; }
            return n > 0 ? sum / n : Vector3.zero;
        }
    }
}
