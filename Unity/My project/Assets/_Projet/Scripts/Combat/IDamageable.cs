using UnityEngine;

namespace ShroomWitch
{
    /// <summary>
    /// Tout ce qui peut recevoir des dégâts (ennemi, objet destructible, joueur).
    /// Les sorts ne connaissent que cette interface, pas le type exact de la cible.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>Vrai tant que la cible n'est pas morte.</summary>
        bool IsAlive { get; }

        /// <summary>Point visé par les sorts (le centre du corps, en général).</summary>
        Vector3 AimPosition { get; }

        void TakeDamage(float amount);
    }
}
