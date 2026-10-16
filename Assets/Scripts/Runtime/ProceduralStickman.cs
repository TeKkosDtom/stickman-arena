using UnityEngine;

namespace StickmanArena
{
    /// <summary>
    /// Stickman armado con primitivas (cabeza, tronco, 4 extremidades) y animado por código:
    /// idle (respira), correr (brazos y piernas se cruzan), golpe y salto. Sirve como
    /// personaje de la demo visual mientras el modelo de Blender no esté importado,
    /// y queda como respaldo ligero: unas 700 caras en total.
    /// </summary>
    public class ProceduralStickman : MonoBehaviour
    {
        public StickmanMotor Motor;
        public Material InkMaterial;
        public Material EyeMaterial;
        public Material AccentMaterial;

        Transform _head, _torso, _armL, _armR, _legL, _legR, _accent;
        float _runPhase;

        public static ProceduralStickman Create(string name, StyleData style, Material ink, Material eye)
        {
            var root = new GameObject(name);
            var motor = root.AddComponent<StickmanMotor>();
            motor.Style = style;
            var stick = root.AddComponent<ProceduralStickman>();
            stick.Motor = motor;
            stick.InkMaterial = ink;
            stick.EyeMaterial = eye;
            var accent = new Material(ink) { color = style.AccentColor };
            stick.AccentMaterial = accent;
            stick.Build();
            return stick;
        }

        void Build()
        {
            const float limb = 0.08f;
            var body = new GameObject("Body").transform;
            body.SetParent(transform, false);

            _torso = Part(PrimitiveType.Capsule, body, new Vector3(0f, 1.15f, 0f), new Vector3(limb * 2f, 0.4f, limb * 2f), InkMaterial);
            _head = Part(PrimitiveType.Sphere, body, new Vector3(0f, 1.62f, 0f), Vector3.one * 0.36f, InkMaterial);
            Part(PrimitiveType.Sphere, _head, new Vector3(-0.3f, 0.15f, 0.42f), new Vector3(0.22f, 0.3f, 0.12f), EyeMaterial);
            Part(PrimitiveType.Sphere, _head, new Vector3(0.3f, 0.15f, 0.42f), new Vector3(0.22f, 0.3f, 0.12f), EyeMaterial);

            _armL = Limb(body, new Vector3(-0.22f, 1.45f, 0f), 0.62f, limb);
            _armR = Limb(body, new Vector3(0.22f, 1.45f, 0f), 0.62f, limb);
            _legL = Limb(body, new Vector3(-0.12f, 0.85f, 0f), 0.85f, limb * 1.1f);
            _legR = Limb(body, new Vector3(0.12f, 0.85f, 0f), 0.85f, limb * 1.1f);

            // Accesorio del estilo: un aro de color sobre la cabeza (en el juego final cada estilo tiene el suyo)
            _accent = Part(PrimitiveType.Cylinder, _head, new Vector3(0f, 0.65f, 0f), new Vector3(0.9f, 0.08f, 0.9f), AccentMaterial);
        }

        static Transform Part(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            Destroy(go.GetComponent<Collider>()); // el CharacterController ya colisiona
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return go.transform;
        }

        /// <summary>Un pivote en el hombro o cadera con un cilindro que cuelga hacia abajo.</summary>
        static Transform Limb(Transform parent, Vector3 pivot, float length, float thickness)
        {
            var joint = new GameObject("Joint").transform;
            joint.SetParent(parent, false);
            joint.localPosition = pivot;
            Part(PrimitiveType.Cylinder, joint, new Vector3(0f, -length * 0.5f, 0f), new Vector3(thickness * 2f, length * 0.5f, thickness * 2f), parent.GetComponentInChildren<Renderer>().sharedMaterial);
            return joint;
        }

        void Update()
        {
            float t = Time.time;
            float speed01 = Motor ? Mathf.Clamp01(Motor.PlanarSpeed / 7.5f) : 0f;
            bool grounded = Motor == null || Motor.IsGrounded;

            // Idle: respiración leve
            float breathe = Mathf.Sin(t * 2.2f) * 0.015f * (1f - speed01);
            _torso.localPosition = new Vector3(0f, 1.15f + breathe, 0f);
            _head.localPosition = new Vector3(0f, 1.62f + breathe * 1.5f, 0f);

            // Correr: brazos y piernas en oposición
            _runPhase += Time.deltaTime * Mathf.Lerp(0f, 14f, speed01);
            float swing = Mathf.Sin(_runPhase) * Mathf.Lerp(0f, 50f, speed01);
            _legL.localRotation = Quaternion.Euler(swing, 0f, 0f);
            _legR.localRotation = Quaternion.Euler(-swing, 0f, 0f);

            if (!grounded)
            {
                // Salto: brazos arriba, piernas recogidas
                _armL.localRotation = Quaternion.Euler(-150f, 0f, 20f);
                _armR.localRotation = Quaternion.Euler(-150f, 0f, -20f);
                _legL.localRotation = Quaternion.Euler(35f, 0f, 0f);
                _legR.localRotation = Quaternion.Euler(-10f, 0f, 0f);
            }
            else if (Motor != null && Motor.IsPunching)
            {
                // Golpe: brazo derecho al frente con squash and stretch del tronco
                float k = 1f - Motor.PunchTimer / StickmanMotor.PunchDuration;
                float punch = Mathf.Sin(k * Mathf.PI);
                _armR.localRotation = Quaternion.Euler(-90f - punch * 20f, 0f, 0f);
                _armL.localRotation = Quaternion.Euler(30f, 0f, 0f);
                _torso.localScale = new Vector3(0.16f + punch * 0.03f, 0.4f - punch * 0.03f, 0.16f + punch * 0.03f);
            }
            else
            {
                _armL.localRotation = Quaternion.Euler(-swing, 0f, 8f);
                _armR.localRotation = Quaternion.Euler(swing, 0f, -8f);
                _torso.localScale = new Vector3(0.16f, 0.4f, 0.16f);
            }

            _accent.Rotate(0f, 90f * Time.deltaTime, 0f, Space.Self);
        }
    }
}
