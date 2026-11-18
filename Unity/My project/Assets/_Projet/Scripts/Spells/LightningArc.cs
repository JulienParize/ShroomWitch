using UnityEngine;

namespace ShroomWitch
{
    /// <summary>
    /// Dessin d'un éclair : une ligne en zigzag entre deux points, qui s'efface très vite.
    /// Peut servir de prefab pour régler l'apparence ; sinon les sorts en créent un par défaut.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LightningArc : MonoBehaviour
    {
        [Tooltip("Nombre de morceaux de la ligne : plus il y en a, plus le zigzag est fin.")]
        [SerializeField] private int segments = 10;

        [Tooltip("Écart maximal du zigzag par rapport à la ligne droite, en mètres.")]
        [SerializeField] private float jitter = 0.25f;

        [SerializeField] private float width = 0.08f;

        [Tooltip("Durée d'affichage, en secondes.")]
        [SerializeField] private float lifetime = 0.15f;

        [SerializeField] private Color color = new Color(0.7f, 0.9f, 1f, 1f);

        [Tooltip("Matériau de la ligne. Si vide, un matériau simple est créé automatiquement.")]
        [SerializeField] private Material material;

        private LineRenderer line;
        private Material runtimeMaterial;
        private float spawnTime;

        /// <summary>Crée un éclair entre deux points. Le prefab est facultatif.</summary>
        public static LightningArc Spawn(LightningArc prefab, Vector3 from, Vector3 to)
        {
            LightningArc arc = prefab != null
                ? Instantiate(prefab)
                : new GameObject(nameof(LightningArc)).AddComponent<LightningArc>();

            arc.Show(from, to);
            return arc;
        }

        private void Show(Vector3 from, Vector3 to)
        {
            line = GetComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.widthMultiplier = width;

            if (material != null)
            {
                line.sharedMaterial = material;
            }
            else
            {
                // Shader simple toujours disponible, qui affiche la couleur de la ligne.
                runtimeMaterial = new Material(Shader.Find("Sprites/Default"));
                line.sharedMaterial = runtimeMaterial;
            }

            int pointCount = Mathf.Max(2, segments + 1);
            line.positionCount = pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                Vector3 point = Vector3.Lerp(from, to, i / (float)(pointCount - 1));

                // Les deux extrémités restent exactes, le reste est décalé au hasard.
                if (i > 0 && i < pointCount - 1)
                {
                    point += Random.insideUnitSphere * jitter;
                }

                line.SetPosition(i, point);
            }

            SetAlpha(1f);
            spawnTime = Time.time;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (line != null && lifetime > 0f)
            {
                SetAlpha(1f - (Time.time - spawnTime) / lifetime);
            }
        }

        private void SetAlpha(float alpha)
        {
            Color faded = color;
            faded.a *= Mathf.Clamp01(alpha);
            line.startColor = faded;
            line.endColor = faded;
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }
    }
}
