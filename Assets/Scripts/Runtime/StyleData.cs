using System;
using System.Collections.Generic;
using UnityEngine;

namespace StickmanArena
{
    /// <summary>
    /// Un "estilo" define cómo pelea un stickman. Los 4 valores suman siempre 12
    /// (regla de balance del documento de diseño). El salto es igual para todos.
    /// </summary>
    [Serializable]
    public struct StyleData
    {
        public const int TotalPoints = 12;

        public string Id;
        public string DisplayName;
        public int Speed;   // 1..5
        public int Damage;  // 1..5
        public int Range;   // 1..5
        public int Weight;  // 1..5
        public Color AccentColor;

        public int Sum => Speed + Damage + Range + Weight;
        public bool IsBalanced => Sum == TotalPoints && InRange(Speed) && InRange(Damage) && InRange(Range) && InRange(Weight);
        static bool InRange(int v) => v >= 1 && v <= 5;

        /// <summary>Velocidad de carrera en m/s derivada de la estadística (1 → 4,5 m/s, 5 → 7,5 m/s).</summary>
        public float MoveSpeed => 3.75f + Speed * 0.75f;

        /// <summary>Masa relativa: define cuánto empuja y cuánto lo empujan.</summary>
        public float Mass => 0.6f + Weight * 0.2f;

        public static readonly StyleData PunosDeRoca = new StyleData
        {
            Id = "punos_de_roca", DisplayName = "Puños de Roca",
            Speed = 1, Damage = 5, Range = 2, Weight = 4, AccentColor = new Color(0.55f, 0.45f, 0.35f)
        };
        public static readonly StyleData AuraDorada = new StyleData
        {
            Id = "aura_dorada", DisplayName = "Aura Dorada",
            Speed = 5, Damage = 3, Range = 2, Weight = 2, AccentColor = new Color(1f, 0.85f, 0.1f)
        };
        public static readonly StyleData Boxeador = new StyleData
        {
            Id = "boxeador", DisplayName = "Boxeador",
            Speed = 3, Damage = 3, Range = 3, Weight = 3, AccentColor = new Color(0.85f, 0.15f, 0.15f)
        };
        public static readonly StyleData Pistolero = new StyleData
        {
            Id = "pistolero", DisplayName = "Pistolero",
            Speed = 4, Damage = 2, Range = 5, Weight = 1, AccentColor = new Color(0.35f, 0.25f, 0.15f)
        };

        /// <summary>Los 4 estilos gratis del MVP.</summary>
        public static IReadOnlyList<StyleData> MvpStyles => new[] { PunosDeRoca, AuraDorada, Boxeador, Pistolero };
    }
}
