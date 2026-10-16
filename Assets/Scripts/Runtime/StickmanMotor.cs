using UnityEngine;

namespace StickmanArena
{
    /// <summary>
    /// Movimiento básico del stickman con CharacterController: correr, saltar y un golpe.
    /// La velocidad sale del estilo; el salto es igual para todos (regla de diseño).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class StickmanMotor : MonoBehaviour
    {
        public const float JumpHeight = 2.2f;   // igual para todos los estilos
        public const float Gravity = -22f;
        public const float PunchDuration = 0.35f;

        public StyleData Style = StyleData.Boxeador;

        /// <summary>Dónde reaparece quien cae de la arena; la demo lo mueve a las ruinas flotantes en la última fase.</summary>
        public static Vector3 RespawnPosition = new Vector3(0f, 3f, 0f);

        [Header("Entrada (se puede reemplazar por un controlador de IA)")]
        public Vector2 MoveInput;
        public bool JumpPressed;
        public bool PunchPressed;

        public Vector3 Velocity { get; private set; }
        public bool IsGrounded => _controller.isGrounded;
        public float PlanarSpeed => new Vector2(Velocity.x, Velocity.z).magnitude;
        public float PunchTimer { get; private set; }
        public bool IsPunching => PunchTimer > 0f;

        CharacterController _controller;
        float _verticalSpeed;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _controller.height = 1.8f;
            _controller.radius = 0.3f;
            _controller.center = new Vector3(0f, 0.9f, 0f);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            Vector3 planar = new Vector3(MoveInput.x, 0f, MoveInput.y);
            if (planar.sqrMagnitude > 1f) planar.Normalize();
            planar *= Style.MoveSpeed;

            if (_controller.isGrounded)
            {
                _verticalSpeed = -1f; // pegado al piso
                if (JumpPressed) _verticalSpeed = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }
            _verticalSpeed += Gravity * dt;

            if (PunchPressed && !IsPunching) PunchTimer = PunchDuration;
            if (PunchTimer > 0f) PunchTimer -= dt;

            Velocity = new Vector3(planar.x, _verticalSpeed, planar.z);
            _controller.Move(Velocity * dt);

            if (planar.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(planar, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, 12f * dt);
            }

            JumpPressed = false;
            PunchPressed = false;

            // Cayó de la arena: vuelve al centro (en el prototipo de combate esto será perder una vida)
            if (transform.position.y < -15f) Respawn();
        }

        public void Respawn()
        {
            _controller.enabled = false;
            transform.position = RespawnPosition;
            _verticalSpeed = 0f;
            _controller.enabled = true;
        }
    }
}
