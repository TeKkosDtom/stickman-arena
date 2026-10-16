using NUnit.Framework;
using StickmanArena;

namespace StickmanArena.Tests
{
    public class StyleDataTests
    {
        [Test]
        public void TodosLosEstilosDelMvpSuman12Puntos()
        {
            foreach (var s in StyleData.MvpStyles)
                Assert.IsTrue(s.IsBalanced, $"{s.DisplayName} suma {s.Sum}, debe sumar {StyleData.TotalPoints} con valores de 1 a 5");
        }

        [Test]
        public void LosEstilosTienenIdsDistintos()
        {
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (var s in StyleData.MvpStyles) Assert.IsTrue(ids.Add(s.Id), $"Id repetido: {s.Id}");
        }

        [Test]
        public void LaLineaDeTiempoDeCieloDePapelEstaOrdenadaYRespetaElRitmo()
        {
            var phases = ArenaPhaseDirector.CieloDePapel();
            Assert.AreEqual(0f, phases[0].StartTime);
            Assert.GreaterOrEqual(phases[1].StartTime, 35f, "La primera sección tranquila dura al menos 35 s");
            for (int i = 1; i < phases.Count; i++)
            {
                Assert.Greater(phases[i].StartTime, phases[i - 1].StartTime);
                Assert.That(phases[i].WarningLead, Is.InRange(2f, 4f), "El aviso dura entre 2 y 4 s");
            }
        }
    }
}
