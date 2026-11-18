using UnityEngine;

namespace ShroomWitch
{
    /// <summary>
    /// Clic gauche : éclair simple, instantané, sur l'ennemi sous le viseur.
    /// </summary>
    public class LightningBoltSpell : Spell
    {
        [SerializeField] private float damage = 8f;
        [SerializeField] private float range = 30f;

        [Tooltip("Tolérance de visée : épaisseur du tir en mètres (0 = il faut viser pile dessus).")]
        [SerializeField] private float aimRadius = 0.15f;

        [Tooltip("Couches que l'éclair peut toucher (ennemis et décor).")]
        [SerializeField] private LayerMask hitMask = ~0;

        [Tooltip("Apparence de l'éclair (facultatif).")]
        [SerializeField] private LightningArc arcPrefab;

        protected override bool Cast(SpellCaster caster)
        {
            Vector3 origin = caster.CastOrigin;

            if (!caster.TryAim(range, aimRadius, hitMask, out RaycastHit hit))
            {
                // Rien de touché : l'éclair part quand même dans le vide.
                LightningArc.Spawn(arcPrefab, origin, caster.GetAimPoint(range, hitMask));
                return true;
            }

            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();

            if (target != null && target.IsAlive)
            {
                LightningArc.Spawn(arcPrefab, origin, target.AimPosition);
                target.TakeDamage(damage);
            }
            else
            {
                LightningArc.Spawn(arcPrefab, origin, hit.point);
            }

            return true;
        }
    }
}
