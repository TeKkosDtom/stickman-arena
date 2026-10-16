using UnityEngine;

namespace StickmanArena
{
    /// <summary>Hace caer y girar un objeto decorativo (sin Rigidbody) y lo apaga cuando está lejos.</summary>
    public class FallAway : MonoBehaviour
    {
        public float Delay;
        public Vector3 Spin = new Vector3(40f, 0f, 25f);

        float _t;
        Vector3 _rest;
        Quaternion _restRot;

        void Start()
        {
            _rest = transform.localPosition;
            _restRot = transform.localRotation;
        }

        void Update()
        {
            _t += Time.deltaTime;
            float since = _t - Delay;
            if (since < 0f)
            {
                transform.localPosition = _rest + new Vector3(Mathf.Sin(_t * 50f) * 0.04f, 0f, 0f);
                return;
            }
            float fall = 0.5f * 20f * since * since;
            transform.localPosition = _rest + Vector3.down * fall;
            transform.localRotation = _restRot * Quaternion.Euler(Spin * since);
            if (fall > 60f) gameObject.SetActive(false);
        }
    }
}
