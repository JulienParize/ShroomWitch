using UnityEngine;
using UnityEngine.Events;

namespace ShroomWitch
{
    /// <summary>
    /// Points de vie d'un objet. À poser sur un ennemi (ou un objet destructible)
    /// pour qu'il puisse être touché par les sorts.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 30f;

        [Tooltip("Point visé par les sorts. Si vide, on vise le centre de l'objet.")]
        [SerializeField] private Transform aimPoint;

        [Tooltip("Détruit l'objet quand sa vie tombe à zéro.")]
        [SerializeField] private bool destroyOnDeath = true;

        [Tooltip("Écrit les dégâts reçus dans la console (pratique pour tester).")]
        [SerializeField] private bool logDamage;

        [SerializeField] private UnityEvent<float> onDamaged;
        [SerializeField] private UnityEvent onDied;

        private float currentHealth;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0f;
        public Vector3 AimPosition => aimPoint != null ? aimPoint.position : transform.position;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - amount);

            if (logDamage)
            {
                Debug.Log($"{name} reçoit {amount:0.#} dégâts (vie : {currentHealth:0.#}/{maxHealth:0.#})", this);
            }

            onDamaged?.Invoke(amount);

            if (!IsAlive)
            {
                onDied?.Invoke();

                if (destroyOnDeath)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
