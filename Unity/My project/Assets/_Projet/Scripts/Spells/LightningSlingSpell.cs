using UnityEngine;

namespace ShroomWitch
{
    /// <summary>
    /// Clic droit : fronde éclair. Lance une boule de foudre en cloche,
    /// qui explose à l'impact et touche tous les ennemis proches.
    /// </summary>
    public class LightningSlingSpell : Spell
    {
        [Header("Lancer")]
        [SerializeField] private float launchSpeed = 18f;

        [Tooltip("Vitesse ajoutée vers le haut, pour donner la trajectoire en cloche.")]
        [SerializeField] private float upwardBoost = 4f;

        [Tooltip("Force qui fait retomber la boule (9.81 = gravité normale).")]
        [SerializeField] private float gravity = 15f;

        [Tooltip("Distance de visée maximale.")]
        [SerializeField] private float aimRange = 40f;

        [Tooltip("Durée de vie maximale de la boule, en secondes. Elle explose à la fin.")]
        [SerializeField] private float lifetime = 4f;

        [Header("Explosion")]
        [SerializeField] private float damage = 15f;
        [SerializeField] private float explosionRadius = 3.5f;

        [Tooltip("Couches que la boule peut toucher (ennemis et décor).")]
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Apparence")]
        [Tooltip("Prefab de la boule (facultatif). Si vide, une simple sphère est créée.")]
        [SerializeField] private LightningSlingProjectile projectilePrefab;

        [Tooltip("Rayon de la boule, en mètres (sert aussi à détecter l'impact).")]
        [SerializeField] private float projectileRadius = 0.2f;

        [Tooltip("Apparence des éclairs de l'explosion (facultatif).")]
        [SerializeField] private LightningArc arcPrefab;

        protected override bool Cast(SpellCaster caster)
        {
            Vector3 origin = caster.CastOrigin;
            Vector3 aimPoint = caster.GetAimPoint(aimRange, hitMask);
            Vector3 velocity = (aimPoint - origin).normalized * launchSpeed + Vector3.up * upwardBoost;

            LightningSlingProjectile projectile = projectilePrefab != null
                ? Instantiate(projectilePrefab, origin, Quaternion.identity)
                : CreateDefaultProjectile(origin);

            projectile.Launch(velocity, gravity, projectileRadius, lifetime, explosionRadius, damage, hitMask, caster.transform, arcPrefab);
            return true;
        }

        // Boule par défaut pour prototyper sans prefab : une sphère sans collision.
        private LightningSlingProjectile CreateDefaultProjectile(Vector3 position)
        {
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = nameof(LightningSlingProjectile);
            ball.transform.position = position;
            ball.transform.localScale = Vector3.one * (projectileRadius * 2f);
            Destroy(ball.GetComponent<Collider>());
            return ball.AddComponent<LightningSlingProjectile>();
        }
    }
}
