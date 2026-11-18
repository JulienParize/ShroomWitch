using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShroomWitch
{
    /// <summary>
    /// Touche R : éclair en chaîne. Frappe l'ennemi visé, puis saute
    /// d'ennemi en ennemi vers le plus proche pas encore touché.
    /// </summary>
    public class ChainLightningSpell : Spell
    {
        [SerializeField] private float damage = 12f;
        [SerializeField] private float range = 30f;

        [Tooltip("Tolérance de visée : épaisseur du tir en mètres.")]
        [SerializeField] private float aimRadius = 0.3f;

        [Header("Rebonds")]
        [Tooltip("Nombre maximal d'ennemis touchés après le premier.")]
        [SerializeField] private int maxJumps = 4;

        [Tooltip("Distance maximale entre deux ennemis pour que l'éclair saute.")]
        [SerializeField] private float jumpRadius = 8f;

        [Tooltip("Dégâts multipliés par cette valeur à chaque rebond (1 = pas de perte).")]
        [Range(0f, 1f)]
        [SerializeField] private float damageFalloff = 0.8f;

        [Tooltip("Petite pause entre deux rebonds, pour voir l'éclair voyager.")]
        [SerializeField] private float jumpDelay = 0.08f;

        [Tooltip("Couches que l'éclair peut toucher (ennemis et décor).")]
        [SerializeField] private LayerMask hitMask = ~0;

        [Tooltip("Apparence de l'éclair (facultatif).")]
        [SerializeField] private LightningArc arcPrefab;

        protected override bool Cast(SpellCaster caster)
        {
            // Sans ennemi sous le viseur, le sort ne part pas (et ne se recharge pas).
            if (!caster.TryAim(range, aimRadius, hitMask, out RaycastHit hit))
            {
                return false;
            }

            IDamageable first = hit.collider.GetComponentInParent<IDamageable>();

            if (first == null || !first.IsAlive)
            {
                return false;
            }

            StartCoroutine(ChainRoutine(caster, first));
            return true;
        }

        // Coroutine : fonction qui peut faire des pauses et reprendre aux images suivantes.
        private IEnumerator ChainRoutine(SpellCaster caster, IDamageable first)
        {
            HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();
            Vector3 from = caster.CastOrigin;
            IDamageable target = first;
            float currentDamage = damage;

            for (int jump = 0; jump <= maxJumps && target != null; jump++)
            {
                // On retient la position avant les dégâts : la cible peut être détruite.
                Vector3 targetPosition = target.AimPosition;

                LightningArc.Spawn(arcPrefab, from, targetPosition);
                alreadyHit.Add(target);
                target.TakeDamage(currentDamage);

                from = targetPosition;
                currentDamage *= damageFalloff;

                if (jumpDelay > 0f)
                {
                    yield return new WaitForSeconds(jumpDelay);
                }

                target = FindNextTarget(from, caster.transform, alreadyHit);
            }
        }

        private IDamageable FindNextTarget(Vector3 from, Transform casterRoot, HashSet<IDamageable> alreadyHit)
        {
            Collider[] colliders = Physics.OverlapSphere(from, jumpRadius, hitMask, QueryTriggerInteraction.Ignore);

            IDamageable best = null;
            float bestSqrDistance = float.MaxValue;

            foreach (Collider col in colliders)
            {
                if (col.transform.IsChildOf(casterRoot))
                {
                    continue;
                }

                IDamageable candidate = col.GetComponentInParent<IDamageable>();

                if (candidate == null || !candidate.IsAlive || alreadyHit.Contains(candidate))
                {
                    continue;
                }

                float sqrDistance = (candidate.AimPosition - from).sqrMagnitude;

                if (sqrDistance < bestSqrDistance)
                {
                    best = candidate;
                    bestSqrDistance = sqrDistance;
                }
            }

            return best;
        }
    }
}
