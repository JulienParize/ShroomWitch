using UnityEngine;

namespace ShroomWitch
{
    /// <summary>
    /// Base commune à tous les sorts : gère le temps de recharge (cooldown).
    /// Chaque sort concret décrit seulement ce qui se passe quand on le lance.
    /// </summary>
    public abstract class Spell : MonoBehaviour
    {
        [Tooltip("Temps d'attente entre deux lancers, en secondes.")]
        [SerializeField] private float cooldown = 0.3f;

        private float nextCastTime;

        public bool IsReady => Time.time >= nextCastTime;

        /// <summary>Temps restant avant de pouvoir relancer (0 si prêt). Utile pour l'interface plus tard.</summary>
        public float CooldownRemaining => Mathf.Max(0f, nextCastTime - Time.time);

        /// <summary>Lance le sort s'il est prêt. Renvoie vrai si le sort est parti.</summary>
        public bool TryCast(SpellCaster caster)
        {
            if (!IsReady || !Cast(caster))
            {
                return false;
            }

            nextCastTime = Time.time + cooldown;
            return true;
        }

        /// <summary>Effet du sort. Renvoyer faux annule le lancer (pas de recharge consommée).</summary>
        protected abstract bool Cast(SpellCaster caster);
    }
}
