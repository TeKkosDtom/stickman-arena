using UnityEngine;

namespace StickmanArena
{
    /// <summary>
    /// Arma la demo visual entera por código al iniciar la escena: luz, el avión de papel
    /// (cubierta de baldosas y alas), un stickman controlable con teclado, tres stickmen de
    /// relleno para probar la cámara, el director de fases de "Cielo de Papel" y un HUD mínimo.
    /// Así la escena del repo queda casi vacía y todo se puede revisar en un diff.
    ///
    /// Si BuildScript asignó el modelo de Blender (StickmanPrefab + clips) se usa
    /// <see cref="StickmanView"/>; si no, el <see cref="ProceduralStickman"/> de primitivas.
    /// </summary>
    public class DemoBootstrap : MonoBehaviour
    {
        /// <summary>Shader plano para todos los materiales. Lo asigna BuildScript al crear la escena
        /// para que entre en el build; si falta, se busca por nombre (solo funciona en el editor).</summary>
        public Shader FlatShader;

        [Header("Modelo de Blender (lo asigna BuildScript; vacío = stickman procedural)")]
        public GameObject StickmanPrefab;
        public AnimationClip IdleClip, RunClip, PunchClip, JumpClip;

        ArenaPhaseDirector _director;
        CollapsingDeck _deck;
        TopDownCamera _camera;
        Camera _cam;
        FloatingRuins _ruins;
        FallAway[] _wings;
        Material _paper, _paperDark, _ruinMat;
        string _banner = "";
        float _bannerUntil;
        Color _skyFrom, _skyTo;
        float _skyT = 1f;

        void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.shadows = ShadowQuality.Disable;

            var shader = FlatShader != null ? FlatShader : (Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
            Material Flat(Color c) => new Material(shader) { color = c };
            var ink = Flat(new Color(0.05f, 0.05f, 0.06f));
            var eye = Flat(Color.white);
            _paper = Flat(new Color(0.93f, 0.93f, 0.90f));
            _paperDark = Flat(new Color(0.80f, 0.82f, 0.85f));
            _ruinMat = Flat(new Color(0.86f, 0.84f, 0.78f));

            // Cielo plano, sin skybox (más barato)
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            _cam = camGo.AddComponent<Camera>();
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = _skyFrom = _skyTo = Sky(0);
            _cam.allowHDR = false;
            _cam.allowMSAA = false;
            _camera = camGo.AddComponent<TopDownCamera>();

            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.None;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Avión de papel: cubierta de baldosas + dos alas decorativas
            var deckGo = new GameObject("Cubierta");
            _deck = deckGo.AddComponent<CollapsingDeck>();
            _deck.TileMaterialA = _paper;
            _deck.TileMaterialB = _paperDark;
            _deck.Build();
            _wings = new[] { Wing(-1), Wing(1) };

            // Jugador 1 y tres acompañantes quietos (para ver cómo encuadra la cámara)
            var styles = new[] { StyleData.Boxeador, StyleData.PunosDeRoca, StyleData.AuraDorada, StyleData.Pistolero };
            var spots = new[] { new Vector3(0f, 1f, 2f), new Vector3(-3f, 1f, -2f), new Vector3(3f, 1f, -1f), new Vector3(0f, 1f, 6f) };
            for (int i = 0; i < styles.Length; i++)
            {
                var stick = SpawnStickman(i == 0 ? "P1" : "Dummy_" + styles[i].Id, styles[i], ink, eye);
                stick.position = spots[i];
                if (i == 0) stick.gameObject.AddComponent<KeyboardInput>();
                _camera.Targets.Add(stick);
            }

            // Director de fases con la línea de tiempo de Cielo de Papel
            _director = gameObject.AddComponent<ArenaPhaseDirector>();
            _director.Phases = ArenaPhaseDirector.CieloDePapel();
            _director.WarningStarted += p => { Banner("¡" + p.Name.ToUpper() + "!  GO →", 3f); _camera.ShakeAmount = 1f; };
            _director.PhaseStarted += OnPhase;
        }

        Transform SpawnStickman(string name, StyleData style, Material ink, Material eye)
        {
            bool hasModel = StickmanPrefab != null && IdleClip != null && RunClip != null && PunchClip != null && JumpClip != null;
            if (hasModel)
                return StickmanView.Create(name, style, StickmanPrefab, IdleClip, RunClip, PunchClip, JumpClip, ink, eye).transform;
            return ProceduralStickman.Create(name, style, ink, eye).transform;
        }

        FallAway Wing(int side)
        {
            var wing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wing.name = side < 0 ? "Ala_Izq" : "Ala_Der";
            Destroy(wing.GetComponent<Collider>()); // decorativa: no se camina por ella
            float halfWidth = _deck.Columns * _deck.TileSize * 0.5f;
            wing.transform.position = new Vector3(side * (halfWidth + 3.2f), -0.2f, -1f);
            wing.transform.rotation = Quaternion.Euler(0f, 0f, side * -14f);
            wing.transform.localScale = new Vector3(6.5f, 0.12f, _deck.Length * 0.75f);
            var r = wing.GetComponent<Renderer>();
            r.sharedMaterial = _paper;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var fall = wing.AddComponent<FallAway>();
            fall.enabled = false;
            return fall;
        }

        void OnPhase(ArenaPhaseDirector.Phase p)
        {
            Banner(p.Name, 2.5f);
            switch (p.Name)
            {
                case "Derrumbe de la cola":
                    // Caen las 6 filas de atrás en 20 s (transición jugable)
                    _deck.StartCollapse(20f, 0, _deck.Rows / 2 - 1);
                    FadeSky(Sky(1), 4f);
                    break;
                case "Parte delantera":
                    // Caen las 3 filas de adelante; queda una isla de 3 filas en el centro
                    _deck.StartCollapse(18f, _deck.Rows - 1, _deck.Rows / 2 + 3);
                    FadeSky(Sky(2), 4f);
                    break;
                case "Caída libre":
                    // Las alas se desprenden, la isla se deshace y llegan las ruinas flotantes
                    foreach (var w in _wings) { w.Delay = 1.5f; w.enabled = true; }
                    _deck.StartCollapse(16f, _deck.Rows / 2, _deck.Rows / 2 + 2);
                    var ruinsGo = new GameObject("Ruinas");
                    ruinsGo.transform.position = new Vector3(0f, -1.5f, 0f);
                    _ruins = ruinsGo.AddComponent<FloatingRuins>();
                    _ruins.Material = _ruinMat;
                    _ruins.Build(10f);
                    StickmanMotor.RespawnPosition = _ruins.SpawnPoint;
                    FadeSky(Sky(3), 6f);
                    break;
                case "Ruinas flotantes":
                    FadeSky(Sky(4), 5f);
                    break;
            }
        }

        /// <summary>Color del cielo por fase: de mediodía claro a atardecer mientras el avión cae.</summary>
        static Color Sky(int phase)
        {
            switch (phase)
            {
                case 0: return new Color(0.55f, 0.78f, 0.95f);
                case 1: return new Color(0.58f, 0.76f, 0.92f);
                case 2: return new Color(0.70f, 0.72f, 0.85f);
                case 3: return new Color(0.85f, 0.60f, 0.55f);
                default: return new Color(0.45f, 0.35f, 0.55f);
            }
        }

        void FadeSky(Color to, float seconds)
        {
            _skyFrom = _cam.backgroundColor;
            _skyTo = to;
            _skyT = 0f;
            _skyDuration = Mathf.Max(0.01f, seconds);
        }
        float _skyDuration = 1f;

        void Update()
        {
            if (_skyT < 1f)
            {
                _skyT = Mathf.Min(1f, _skyT + Time.deltaTime / _skyDuration);
                _cam.backgroundColor = Color.Lerp(_skyFrom, _skyTo, Mathf.SmoothStep(0f, 1f, _skyT));
            }
        }

        void Banner(string text, float seconds) { _banner = text; _bannerUntil = Time.time + seconds; }

        void OnGUI()
        {
            if (_director == null) return;
            // HUD mínimo con OnGUI: cero assets extra. Se reemplaza por UI Toolkit en el prototipo.
            var style = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.UpperLeft };
            style.normal.textColor = Color.black;
            GUI.Label(new Rect(16, 12, 600, 30), $"Cielo de Papel  {FormatTime(_director.MatchTime)}   Fase: {_director.CurrentPhaseName}", style);
            GUI.Label(new Rect(16, 40, 800, 30), "WASD mover · Espacio saltar · J golpear", style);

            if (Time.time < _bannerUntil)
            {
                var big = new GUIStyle(GUI.skin.label) { fontSize = 48, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                big.normal.textColor = _director.WarningActive ? new Color(0.9f, 0.1f, 0.1f) : Color.black;
                GUI.Label(new Rect(0, Screen.height * 0.3f, Screen.width, 80), _banner, big);
            }
        }

        static string FormatTime(float t) => $"{(int)t / 60}:{(int)t % 60:00}";
    }
}
