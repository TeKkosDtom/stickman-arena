using UnityEngine;

namespace StickmanArena
{
    /// <summary>
    /// Arma la demo visual entera por código al iniciar la escena: luz, cubierta de
    /// baldosas, un stickman controlable con teclado, tres stickmen de relleno para probar
    /// la cámara, el director de fases y un HUD mínimo. Así la escena del repo queda casi
    /// vacía y todo se puede revisar en un diff.
    /// </summary>
    public class DemoBootstrap : MonoBehaviour
    {
        ArenaPhaseDirector _director;
        CollapsingDeck _deck;
        TopDownCamera _camera;
        string _banner = "";
        float _bannerUntil;

        void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.shadows = ShadowQuality.Disable;

            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            Material Flat(Color c) => new Material(shader) { color = c };
            var ink = Flat(new Color(0.05f, 0.05f, 0.06f));
            var eye = Flat(Color.white);

            // Cielo plano, sin skybox (más barato)
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.78f, 0.95f);
            cam.allowHDR = false;
            cam.allowMSAA = false;
            _camera = camGo.AddComponent<TopDownCamera>();

            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.None;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Cubierta del avión de papel
            var deckGo = new GameObject("Cubierta");
            _deck = deckGo.AddComponent<CollapsingDeck>();
            _deck.TileMaterialA = Flat(new Color(0.93f, 0.93f, 0.90f));
            _deck.TileMaterialB = Flat(new Color(0.80f, 0.82f, 0.85f));
            _deck.Build();

            // Jugador 1 y tres acompañantes quietos (para ver cómo encuadra la cámara)
            var player = ProceduralStickman.Create("P1", StyleData.Boxeador, ink, eye);
            player.gameObject.AddComponent<KeyboardInput>();
            player.transform.position = new Vector3(0f, 1f, 2f);
            _camera.Targets.Add(player.transform);

            var styles = new[] { StyleData.PunosDeRoca, StyleData.AuraDorada, StyleData.Pistolero };
            var spots = new[] { new Vector3(-3f, 1f, -2f), new Vector3(3f, 1f, -1f), new Vector3(0f, 1f, 6f) };
            for (int i = 0; i < styles.Length; i++)
            {
                var s = ProceduralStickman.Create("Dummy_" + styles[i].Id, styles[i], ink, eye);
                s.transform.position = spots[i];
                _camera.Targets.Add(s.transform);
            }

            // Director de fases con la línea de tiempo de Cielo de Papel
            _director = gameObject.AddComponent<ArenaPhaseDirector>();
            _director.Phases = ArenaPhaseDirector.CieloDePapel();
            _director.WarningStarted += p => { Banner("¡" + p.Name.ToUpper() + "!  GO →", 3f); _camera.ShakeAmount = 1f; };
            _director.PhaseStarted += p =>
            {
                if (p.Name == "Derrumbe de la cola") _deck.StartCollapse(20f, 0.5f);
                Banner(p.Name, 2.5f);
            };
        }

        void Banner(string text, float seconds) { _banner = text; _bannerUntil = Time.time + seconds; }

        void OnGUI()
        {
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
