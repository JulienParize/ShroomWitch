using System.Collections.Generic;
using UnityEngine;

namespace ShroomWitch
{
    /// <summary>
    /// La boule de la fronde éclair. Elle avance toute seule en retombant,
    /// puis explose au premier obstacle (ou à la fin de sa durée de vie).
    /// Ses réglages viennent du sort qui la lance (LightningSlingSpell).
    /// </summary>
    public class LightningSlingProjectile : MonoBehaviour
    {
        private static readonly RaycastHit[] HitBuffer = new RaycastHit[16];

        private Vector3 velocity;
        private float gravity;
        private float collisionRadius;
        private float explosionRadius;
        private float damage;
        private LayerMask hitMask;
        private Transform owner;
        private LightningArc arcPrefab;
        private float explodeTime;
        private bool launched;

        public void Launch(Vector3 velocity, float gravity, float collisionRadius, float lifetime,
            float explosionRadius, float damage, LayerMask hitMask, Transform owner, LightningArc arcPrefab)
        {
            this.velocity = velocity;
            this.gravity = gravity;
            this.collisionRadius = collisionRadius;
            this.explosionRadius = explosionRadius;
            this.damage = damage;
            this.hitMask = hitMask;
            this.owner = owner;
            this.arcPrefab = arcPrefab;
            explodeTime = Time.time + lifetime;
            launched = true;
        }

        private void Update()
        {
            if (!launched)
            {
                return;
            }

            velocity += Vector3.down * (gravity * Time.deltaTime);

            Vector3 step = velocity * Time.deltaTime;
            float distance = step.magnitude;

            // On "balaie" le trajet de cette image pour ne pas traverser un mur fin.
            if (distance > 0f && TryFindImpact(step / distance, distance, out float impactDistance))
            {
                transform.position += step / distance * impactDistance;
                Explode();
                return;
            }

            transform.position += step;

            if (Time.time >= explodeTime)
            {
                Explode();
            }
        }

        private bool TryFindImpact(Vector3 direction, float distance, out float impactDistance)
        {
            int count = Physics.SphereCastNonAlloc(transform.position, collisionRadius, direction, HitBuffer,
                distance, hitMask, QueryTriggerInteraction.Ignore);

            impactDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = HitBuffer[i];

                if (IsOwner(hit.transform))
                {
                    continue;
                }

                impactDistance = Mathf.Min(impactDistance, hit.distance);
            }

            return impactDistance < float.MaxValue;
        }

        private void Explode()
        {
            launched = false;

            Vector3 center = transform.position;
            Collider[] colliders = Physics.OverlapSphere(center, explosionRadius, hitMask, QueryTriggerInteraction.Ignore);

            // Un ennemi peut avoir plusieurs collisions : on ne le blesse qu'une fois.
            HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();

            foreach (Collider col in colliders)
            {
                if (IsOwner(col.transform))
                {
                    continue;
                }

                IDamageable target = col.GetComponentInParent<IDamageable>();

                if (target == null || !target.IsAlive || !alreadyHit.Add(target))
                {
                    continue;
                }

                LightningArc.Spawn(arcPrefab, center, target.AimPosition);
                target.TakeDamage(damage);
            }

            Destroy(gameObject);
        }

        private bool IsOwner(Transform other)
        {
            return owner != null && other.IsChildOf(owner);
        }
    }
}
