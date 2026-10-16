using System;
using System.Collections.Generic;
using UnityEngine;

namespace StickmanArena
{
    /// <summary>
    /// Lleva el reloj de la arena y dispara las fases con sus avisos, siguiendo las reglas
    /// de ritmo del documento de diseño: sección tranquila, aviso de 2 a 4 s y transición
    /// jugable de 15 a 20 s.
    /// </summary>
    public class ArenaPhaseDirector : MonoBehaviour
    {
        [Serializable]
        public class Phase
        {
            public string Name;
            public float StartTime;      // segundos desde el inicio de la partida
            public float WarningLead = 3f; // segundos de aviso antes de StartTime
        }

        public List<Phase> Phases = new List<Phase>();
        public float MatchTime { get; private set; }
        public int CurrentPhaseIndex { get; private set; } = -1;
        public bool WarningActive { get; private set; }
        public string CurrentPhaseName => CurrentPhaseIndex >= 0 && CurrentPhaseIndex < Phases.Count ? Phases[CurrentPhaseIndex].Name : "";

        public event Action<Phase> PhaseStarted;
        public event Action<Phase> WarningStarted;

        int _warnedIndex = -1;

        void Update()
        {
            MatchTime += Time.deltaTime;
            int next = CurrentPhaseIndex + 1;
            if (next >= Phases.Count) { WarningActive = false; return; }

            var p = Phases[next];
            if (!WarningActive && _warnedIndex != next && p.WarningLead > 0f && MatchTime >= p.StartTime - p.WarningLead)
            {
                WarningActive = true;
                _warnedIndex = next;
                WarningStarted?.Invoke(p);
            }
            if (MatchTime >= p.StartTime)
            {
                WarningActive = false;
                CurrentPhaseIndex = next;
                PhaseStarted?.Invoke(p);
            }
        }

        /// <summary>Línea de tiempo de la arena "Cielo de Papel" según el documento de diseño.</summary>
        public static List<Phase> CieloDePapel() => new List<Phase>
        {
            new Phase { Name = "Cubierta del avión", StartTime = 0f, WarningLead = 0f },
            new Phase { Name = "Derrumbe de la cola", StartTime = 35f },
            new Phase { Name = "Parte delantera", StartTime = 55f },
            new Phase { Name = "Caída libre", StartTime = 75f },
            new Phase { Name = "Ruinas flotantes", StartTime = 93f },
        };
    }
}
