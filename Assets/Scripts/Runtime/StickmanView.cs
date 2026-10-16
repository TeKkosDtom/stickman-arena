using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace StickmanArena
{
    /// <summary>
    /// Vista del stickman modelado en Blender (Assets/Models/Stickman/stickman.fbx): instancia
    /// el prefab del FBX, le pone los materiales planos y reproduce sus clips (Idle, Run, Punch,
    /// Jump) con la API de Playables, sin Animator Controller, mezclando según lo que hace
    /// el <see cref="StickmanMotor"/>. Sustituye a <see cref="ProceduralStickman"/> cuando el
    /// modelo está disponible.
    /// </summary>
    public class StickmanView : MonoBehaviour
    {
        public StickmanMotor Motor;
        public AnimationClip Idle, Run, Punch, Jump;
        /// <summary>Giro extra del modelo si Blender y Unity no coinciden en el frente (0 = el modelo mira a +Z).</summary>
        public float ModelYawFix;

        public float BlendSpeed = 10f;
        public float RunClipSpeedMin = 0.7f;
        public float RunClipSpeedMax = 1.35f;
        /// <summary>Instante del clip Jump que se congela mientras el stickman está en el aire.</summary>
        public float JumpAirTime = 0.5f;

        enum State { Idle, Run, Punch, Jump }

        PlayableGraph _graph;
        AnimationMixerPlayable _mixer;
        AnimationClipPlayable[] _clips;
        float[] _weights;
        State _state = State.Idle;
        Transform _accent;

        public static StickmanView Create(string name, StyleData style, GameObject modelPrefab,
            AnimationClip idle, AnimationClip run, AnimationClip punch, AnimationClip jump,
            Material ink, Material eye)
        {
            var root = new GameObject(name);
            var motor = root.AddComponent<StickmanMotor>();
            motor.Style = style;

            var model = Instantiate(modelPrefab, root.transform, false);
            model.name = "Model";
            model.transform.localPosition = Vector3.zero;

            var view = root.AddComponent<StickmanView>();
            view.Motor = motor;
            view.Idle = idle; view.Run = run; view.Punch = punch; view.Jump = jump;
            view.Setup(model, ink, eye, style.AccentColor);
            return view;
        }

        void Setup(GameObject model, Material ink, Material eye, Color accent)
        {
            model.transform.localRotation = Quaternion.Euler(0f, ModelYawFix, 0f);

            // Materiales: el FBX se importa sin materiales; la ranura 0 es la tinta y la 1 los ojos/sonrisa
            foreach (var r in model.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = i == 1 ? eye : ink;
                r.sharedMaterials = mats;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                if (r is SkinnedMeshRenderer smr)
                {
                    smr.updateWhenOffscreen = true; // el bounding box del rig es pequeño y la cámara está lejos
                    smr.quality = SkinQuality.Bone2;
                }
            }

            // Accesorio del estilo: aro de color sobre la cabeza, colgado del hueso Head
            var head = FindDeep(model.transform, "Head");
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(ring.GetComponent<Collider>());
            ring.name = "Accent";
            ring.transform.SetParent(head != null ? head : model.transform, false);
            ring.transform.localPosition = head != null ? new Vector3(0f, 0.42f, 0f) : new Vector3(0f, 2.0f, 0f);
            ring.transform.localScale = new Vector3(0.5f, 0.03f, 0.5f);
            var rr = ring.GetComponent<Renderer>();
            rr.sharedMaterial = new Material(ink) { color = accent };
            rr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _accent = ring.transform;

            var animator = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            BuildGraph(animator);
        }

        void BuildGraph(Animator animator)
        {
            _graph = PlayableGraph.Create(name + ".Anim");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var output = AnimationPlayableOutput.Create(_graph, "Animation", animator);
            var clips = new[] { Idle, Run, Punch, Jump };
            _mixer = AnimationMixerPlayable.Create(_graph, clips.Length);
            output.SetSourcePlayable(_mixer);
            _clips = new AnimationClipPlayable[clips.Length];
            _weights = new float[clips.Length];
            for (int i = 0; i < clips.Length; i++)
            {
                _clips[i] = AnimationClipPlayable.Create(_graph, clips[i]);
                _clips[i].SetApplyFootIK(false);
                _graph.Connect(_clips[i], 0, _mixer, i);
                _mixer.SetInputWeight(i, i == 0 ? 1f : 0f);
            }
            _weights[0] = 1f;
            _graph.Play();
        }

        void Update()
        {
            if (!_graph.IsValid()) return;
            bool grounded = Motor == null || Motor.IsGrounded;
            float speed01 = Motor ? Mathf.Clamp01(Motor.PlanarSpeed / 7.5f) : 0f;

            State wanted;
            if (Motor != null && Motor.IsPunching) wanted = State.Punch;
            else if (!grounded) wanted = State.Jump;
            else if (speed01 > 0.1f) wanted = State.Run;
            else wanted = State.Idle;

            if (wanted != _state) Enter(wanted);
            _state = wanted;

            // Correr más rápido o más lento según el estilo
            if (Run != null) _clips[(int)State.Run].SetSpeed(Mathf.Lerp(RunClipSpeedMin, RunClipSpeedMax, speed01));

            float k = 1f - Mathf.Exp(-BlendSpeed * Time.deltaTime);
            for (int i = 0; i < _weights.Length; i++)
            {
                _weights[i] = Mathf.Lerp(_weights[i], i == (int)_state ? 1f : 0f, k);
                _mixer.SetInputWeight(i, _weights[i]);
            }

            if (_accent) _accent.Rotate(0f, 90f * Time.deltaTime, 0f, Space.Self);
        }

        void Enter(State s)
        {
            switch (s)
            {
                case State.Punch:
                    // El clip dura más que el golpe del motor: se acelera para que coincidan
                    _clips[(int)State.Punch].SetTime(0);
                    _clips[(int)State.Punch].SetSpeed(Punch != null ? Punch.length / StickmanMotor.PunchDuration : 1f);
                    break;
                case State.Jump:
                    // Pose de vuelo congelada; al aterrizar vuelve a Idle/Run con la mezcla
                    _clips[(int)State.Jump].SetTime(Mathf.Min(JumpAirTime, Jump != null ? Jump.length * 0.9f : 0f));
                    _clips[(int)State.Jump].SetSpeed(0);
                    break;
                case State.Run:
                case State.Idle:
                    _clips[(int)s].SetSpeed(1);
                    break;
            }
        }

        void OnDestroy()
        {
            if (_graph.IsValid()) _graph.Destroy();
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
