using UnityEngine;

namespace StickmanArena
{
    /// <summary>Teclado para la demo: WASD o flechas para moverse, espacio salta, J golpea.</summary>
    [RequireComponent(typeof(StickmanMotor))]
    public class KeyboardInput : MonoBehaviour
    {
        StickmanMotor _motor;
        void Awake() => _motor = GetComponent<StickmanMotor>();

        void Update()
        {
            float x = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f)
                    - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            float z = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f)
                    - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
            _motor.MoveInput = new Vector2(x, z);
            if (Input.GetKeyDown(KeyCode.Space)) _motor.JumpPressed = true;
            if (Input.GetKeyDown(KeyCode.J)) _motor.PunchPressed = true;
        }
    }
}
