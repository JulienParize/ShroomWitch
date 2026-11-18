using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShroomWitch
{
    /// <summary>
    /// Champ de fleurs procédural : les fleurs sont fabriquées par le code (aucun modèle 3D),
    /// à partir d'une graine de hasard. Le vent est animé par le shader "ShroomWitch/FlowerWind".
    ///
    /// Utilisation : poser ce composant sur un objet vide, à l'endroit du champ.
    /// Le champ s'affiche aussi dans l'éditeur sans lancer le jeu, et se refait tout seul
    /// dès qu'on change un réglage. Rien n'est enregistré dans la scène : tout est recréé à chaque fois.
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    public class FlowerField : MonoBehaviour
    {
        private const string WindShaderName = "ShroomWitch/FlowerWind";
        private const float TwoPi = Mathf.PI * 2f;

        // Limites de sécurité des réglages.
        private const float MinFieldSize = 0.5f;
        private const float MinPatchSize = 0.5f;
        private const float MinFlowerHeight = 0.02f;
        private const float MinPetalLength = 0.005f;
        private const float MaxCupAngle = 80f;
        private const int MinPetalCount = 3;
        private const int MaxChunksPerAxis = 64;

        // Réglages internes de la répartition (bruit de Perlin : un hasard "en nuages", lisse).
        private const float PatchNoiseLow = 0.35f;
        private const float PatchNoiseHigh = 0.65f;
        private const float NoiseOffsetBase = 1000f;
        private const float NoiseOffsetRange = 1000f;
        private const float ColorLayerSpacing = 37.7f;

        // Réglages internes des couleurs, des feuilles et du dessin d'aide dans la scène.
        private const float HueVariation = 0.06f;
        private const float SaturationVariation = 0.3f;
        private const float BrightnessVariation = 0.4f;
        private const float LeafWidthRatio = 0.4f;
        private const float WindGizmoScale = 0.25f;

        private static readonly int WindDirectionId = Shader.PropertyToID("_WindDirection");
        private static readonly int WindStrengthId = Shader.PropertyToID("_WindStrength");
        private static readonly int WindSpeedId = Shader.PropertyToID("_WindSpeed");
        private static readonly int GustFrequencyId = Shader.PropertyToID("_GustFrequency");
        private static readonly int FlutterStrengthId = Shader.PropertyToID("_FlutterStrength");
        private static readonly int FlutterSpeedId = Shader.PropertyToID("_FlutterSpeed");
        private static readonly Color GizmoColor = new Color(1f, 0.55f, 0.8f, 1f);

        [Header("Zone")]
        [Tooltip("Taille du champ en mètres (largeur X, profondeur Z), centrée sur cet objet.")]
        [SerializeField] private Vector2 size = new Vector2(30f, 30f);

        [Tooltip("Nombre de fleurs par mètre carré dans les zones les plus fleuries.")]
        [SerializeField, Min(0f)] private float density = 6f;

        [Tooltip("Sécurité : nombre maximal de fleurs, pour ne pas bloquer Unity avec un champ trop grand.")]
        [SerializeField, Min(0)] private int maxFlowers = 30000;

        [Tooltip("Graine du hasard : la même graine redonne exactement le même champ.")]
        [SerializeField] private int seed = 1;

        [Tooltip("Le champ est découpé en morceaux de cette taille (en mètres) : ceux qui sont hors de l'écran ne sont pas dessinés.")]
        [SerializeField, Min(1f)] private float chunkSize = 10f;

        [Header("Répartition")]
        [Tooltip("0 = des fleurs partout, 1 = des touffes séparées par des clairières.")]
        [SerializeField, Range(0f, 1f)] private float patchiness = 0.6f;

        [Tooltip("Taille moyenne des touffes et des clairières, en mètres.")]
        [SerializeField, Min(MinPatchSize)] private float patchSize = 6f;

        [Header("Sol")]
        [Tooltip("Pose chaque fleur sur le sol trouvé en dessous (le sol doit avoir un collider). Sans sol, elle reste à la hauteur de cet objet.")]
        [SerializeField] private bool snapToGround = true;

        [Tooltip("Couches (layers) considérées comme du sol.")]
        [SerializeField] private LayerMask groundMask = ~0;

        [Tooltip("Distance de recherche du sol au-dessus et en dessous du champ, en mètres.")]
        [SerializeField, Min(0.1f)] private float groundSearchDistance = 20f;

        [Header("Forme des fleurs")]
        [Tooltip("Hauteur minimale (X) et maximale (Y) des fleurs, en mètres.")]
        [SerializeField] private Vector2 heightRange = new Vector2(0.25f, 0.6f);

        [Tooltip("Épaisseur de la tige (rayon en mètres).")]
        [SerializeField, Min(0.001f)] private float stemRadius = 0.009f;

        [Tooltip("Nombre de tronçons de la tige : plus il y en a, mieux elle se courbe dans le vent (mais plus le champ est lourd).")]
        [SerializeField, Range(1, 4)] private int stemSegments = 2;

        [Tooltip("À quel point les tiges poussent de travers (0 = bien droites).")]
        [SerializeField, Range(0f, 0.5f)] private float stemLean = 0.2f;

        [Tooltip("Nombre minimal (X) et maximal (Y) de pétales.")]
        [SerializeField] private Vector2Int petalCountRange = new Vector2Int(5, 8);

        [Tooltip("Longueur minimale (X) et maximale (Y) des pétales, en mètres.")]
        [SerializeField] private Vector2 petalLengthRange = new Vector2(0.05f, 0.09f);

        [Tooltip("Largeur des pétales par rapport à leur longueur.")]
        [SerializeField, Range(0.2f, 1f)] private float petalWidthRatio = 0.6f;

        [Tooltip("Relèvement des pétales en degrés, minimum (X) et maximum (Y) : 0 = fleur à plat, 60 = fleur en coupe.")]
        [SerializeField] private Vector2 petalCupAngleRange = new Vector2(10f, 40f);

        [Tooltip("Taille du cœur par rapport à la longueur des pétales.")]
        [SerializeField, Range(0.1f, 0.8f)] private float centerSizeRatio = 0.3f;

        [Tooltip("Inclinaison maximale de la tête, en degrés (0 = toutes les fleurs regardent le ciel).")]
        [SerializeField, Range(0f, 60f)] private float headTilt = 25f;

        [Tooltip("Nombre maximal de feuilles sur une tige.")]
        [SerializeField, Range(0, 3)] private int maxLeaves = 2;

        [Tooltip("Longueur des feuilles par rapport à la hauteur de la fleur.")]
        [SerializeField, Range(0.1f, 0.8f)] private float leafSizeRatio = 0.3f;

        [Header("Couleurs")]
        [Tooltip("Couleurs possibles des pétales.")]
        [SerializeField] private Color[] petalColors =
        {
            new Color(0.96f, 0.95f, 0.90f),
            new Color(0.98f, 0.82f, 0.25f),
            new Color(0.95f, 0.55f, 0.70f),
            new Color(0.62f, 0.45f, 0.85f),
            new Color(0.85f, 0.22f, 0.20f),
            new Color(0.40f, 0.55f, 0.92f),
        };

        [Tooltip("Couleur du cœur des fleurs.")]
        [SerializeField] private Color centerColor = new Color(0.90f, 0.58f, 0.12f);

        [Tooltip("Couleur des tiges et des feuilles.")]
        [SerializeField] private Color stemColor = new Color(0.30f, 0.52f, 0.22f);

        [Tooltip("Taille des taches d'une même couleur, en mètres.")]
        [SerializeField, Min(MinPatchSize)] private float colorPatchSize = 8f;

        [Tooltip("0 = grandes taches d'une seule couleur, 1 = couleurs complètement mélangées.")]
        [SerializeField, Range(0f, 1f)] private float colorMixing = 0.3f;

        [Tooltip("Petites différences de teinte et de luminosité d'une fleur à l'autre.")]
        [SerializeField, Range(0f, 1f)] private float colorVariation = 0.3f;

        [Header("Vent")]
        [Tooltip("Direction vers laquelle souffle le vent, en degrés (0 = vers l'axe Z, 90 = vers l'axe X). Une flèche la montre dans la scène.")]
        [SerializeField, Range(0f, 360f)] private float windAngle = 30f;

        [Tooltip("Force du vent : à quel point les fleurs se couchent (0 = immobiles).")]
        [SerializeField, Range(0f, 2f)] private float windStrength = 0.6f;

        [Tooltip("Vitesse à laquelle les rafales traversent le champ, en mètres par seconde.")]
        [SerializeField, Min(0f)] private float windSpeed = 3f;

        [Tooltip("Taille des rafales, en mètres.")]
        [SerializeField, Min(MinPatchSize)] private float gustSize = 6f;

        [Tooltip("Petit tremblement propre à chaque fleur, en plus des rafales.")]
        [SerializeField, Range(0f, 1f)] private float flutter = 0.2f;

        [Tooltip("Rapidité de ce tremblement.")]
        [SerializeField, Min(0f)] private float flutterSpeed = 4f;

        [Header("Rendu")]
        [Tooltip("Les fleurs projettent une ombre.")]
        [SerializeField] private bool castShadows = true;

        [Tooltip("Shader du vent (ShroomWitch/FlowerWind). Rempli automatiquement à l'ajout du composant.")]
        [SerializeField] private Shader windShader;

        // Objets créés par le script (un par morceau de champ) : cachés et jamais enregistrés dans la scène.
        private readonly List<GameObject> chunks = new List<GameObject>();
        private readonly List<Mesh> meshes = new List<Mesh>();
        private Material runtimeMaterial;
        private bool needsRebuild;
        private uint randomState = 1u;

        // Appelé par Unity (dans l'éditeur) quand on ajoute le composant : on branche le shader du vent.
        private void Reset()
        {
            windShader = Shader.Find(WindShaderName);
        }

        private void OnEnable()
        {
            RequestRebuild();
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall -= RebuildFromEditor;
#endif
            ClearChunks();
            DestroyGenerated(runtimeMaterial);
            runtimeMaterial = null;
        }

        // Appelé par Unity (dans l'éditeur) dès qu'un réglage change dans l'inspecteur.
        private void OnValidate()
        {
            size = Vector2.Max(size, new Vector2(MinFieldSize, MinFieldSize));
            heightRange = SortRange(heightRange, MinFlowerHeight, float.MaxValue);
            petalLengthRange = SortRange(petalLengthRange, MinPetalLength, float.MaxValue);
            petalCupAngleRange = SortRange(petalCupAngleRange, 0f, MaxCupAngle);
            petalCountRange.x = Mathf.Max(MinPetalCount, petalCountRange.x);
            petalCountRange.y = Mathf.Max(petalCountRange.x, petalCountRange.y);

            if (isActiveAndEnabled)
            {
                RequestRebuild();
            }
        }

        private void Update()
        {
            // Dans l'éditeur (hors Play), si on déplace le champ, les fleurs doivent se reposer sur le sol.
            if (!Application.isPlaying && snapToGround && transform.hasChanged)
            {
                needsRebuild = true;
            }

            if (needsRebuild)
            {
                Rebuild();
            }
        }

        /// <summary>Demande de refaire tout le champ (fait juste après, pas immédiatement).</summary>
        [ContextMenu("Régénérer le champ")]
        public void RequestRebuild()
        {
            needsRebuild = true;

#if UNITY_EDITOR
            // Hors Play, Update n'est pas appelé en continu : on demande à l'éditeur de nous rappeler juste après.
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.delayCall -= RebuildFromEditor;
                UnityEditor.EditorApplication.delayCall += RebuildFromEditor;
            }
#endif
        }

#if UNITY_EDITOR
        private void RebuildFromEditor()
        {
            if (this != null && isActiveAndEnabled && needsRebuild)
            {
                Rebuild();
            }
        }
#endif

        private void Rebuild()
        {
            needsRebuild = false;
            ClearChunks();

            if (!PrepareMaterial())
            {
                return;
            }

            ApplyWindSettings();

            // Met la physique à jour pour que la recherche du sol voie les objets à leur place actuelle.
            Physics.SyncTransforms();

            float safeChunkSize = Mathf.Max(1f, chunkSize);
            int chunksX = Mathf.Clamp(Mathf.CeilToInt(size.x / safeChunkSize), 1, MaxChunksPerAxis);
            int chunksZ = Mathf.Clamp(Mathf.CeilToInt(size.y / safeChunkSize), 1, MaxChunksPerAxis);
            FlowerMeshBuilder[] builders = new FlowerMeshBuilder[chunksX * chunksZ];

            // Décalages des "cartes" de bruit, tirés de la graine : une autre graine donne d'autres touffes.
            SeedRandom(-1);
            Vector2 patchOffset = RandNoiseOffset();
            Vector2 colorOffset = RandNoiseOffset();

            int candidateCount = (int)Mathf.Min(size.x * size.y * Mathf.Max(0f, density), Mathf.Max(0, maxFlowers));

            for (int i = 0; i < candidateCount; i++)
            {
                // Chaque fleur a sa propre suite de hasard : changer un réglage ne déplace pas les autres fleurs.
                SeedRandom(i);

                float x = (Rand() - 0.5f) * size.x;
                float z = (Rand() - 0.5f) * size.y;

                // Clairière : pas de fleur ici.
                if (Rand() >= GetPatchDensity(x, z, patchOffset))
                {
                    continue;
                }

                Vector3 basePosition = new Vector3(x, 0f, z);
                if (snapToGround)
                {
                    basePosition = SnapToGround(basePosition);
                }

                int chunkX = Mathf.Clamp(Mathf.FloorToInt((x / size.x + 0.5f) * chunksX), 0, chunksX - 1);
                int chunkZ = Mathf.Clamp(Mathf.FloorToInt((z / size.y + 0.5f) * chunksZ), 0, chunksZ - 1);
                int chunkIndex = chunkZ * chunksX + chunkX;

                if (builders[chunkIndex] == null)
                {
                    builders[chunkIndex] = new FlowerMeshBuilder();
                }

                builders[chunkIndex].AddFlower(CreateShape(basePosition, x, z, colorOffset));
            }

            // Marge autour de chaque morceau : une fleur couchée par le vent dépasse de sa place d'origine.
            float boundsMargin = heightRange.y + petalLengthRange.y;

            for (int i = 0; i < builders.Length; i++)
            {
                if (builders[i] != null)
                {
                    CreateChunk(builders[i].Build("FlowerChunk " + i, boundsMargin));
                }
            }

            transform.hasChanged = false;
        }

        // Tire au hasard toutes les mesures et couleurs d'une fleur.
        private FlowerShape CreateShape(Vector3 basePosition, float x, float z, Vector2 colorOffset)
        {
            FlowerShape shape = new FlowerShape();
            shape.basePosition = basePosition;
            shape.random01 = Rand();

            // Tige
            shape.height = RandRange(heightRange);
            float leanAngle = Rand() * TwoPi;
            float leanDistance = Rand() * stemLean * shape.height;
            shape.leanOffset = new Vector2(Mathf.Cos(leanAngle), Mathf.Sin(leanAngle)) * leanDistance;
            shape.stemRadius = stemRadius;
            shape.stemSegments = stemSegments;

            // Tête
            float tilt = Rand() * headTilt * Mathf.Deg2Rad;
            float tiltAngle = Rand() * TwoPi;
            shape.headDirection = new Vector3(
                Mathf.Sin(tilt) * Mathf.Cos(tiltAngle),
                Mathf.Cos(tilt),
                Mathf.Sin(tilt) * Mathf.Sin(tiltAngle));
            shape.petalCount = RandRangeInt(petalCountRange.x, petalCountRange.y);
            shape.petalLength = RandRange(petalLengthRange);
            shape.petalWidth = shape.petalLength * petalWidthRatio;
            shape.petalCupAngle = RandRange(petalCupAngleRange) * Mathf.Deg2Rad;
            shape.centerRadius = shape.petalLength * centerSizeRatio;

            // Feuilles
            shape.leafCount = Mathf.Min(maxLeaves, Mathf.FloorToInt(Rand() * (maxLeaves + 1)));
            shape.leafLength = shape.height * leafSizeRatio;
            shape.leafWidth = shape.leafLength * LeafWidthRatio;

            // Couleurs
            shape.petalColor = VaryColor(PickPetalColor(x, z, colorOffset), colorVariation);
            shape.centerColor = VaryColor(centerColor, colorVariation);
            shape.stemColor = VaryColor(stemColor, colorVariation);
            shape.leafColor = VaryColor(stemColor, colorVariation);
            return shape;
        }

        // Probabilité (0 à 1) qu'une fleur pousse à cet endroit : dessine les touffes et les clairières.
        private float GetPatchDensity(float x, float z, Vector2 noiseOffset)
        {
            float scale = 1f / Mathf.Max(MinPatchSize, patchSize);
            float noise = Mathf.PerlinNoise(noiseOffset.x + x * scale, noiseOffset.y + z * scale);
            float patch = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(PatchNoiseLow, PatchNoiseHigh, noise));
            return Mathf.Lerp(1f, patch, patchiness);
        }

        // Choisit la couleur des pétales : chaque couleur a sa propre "carte" de bruit,
        // et la couleur la plus forte à cet endroit gagne. Cela forme des taches de couleur.
        private Color PickPetalColor(float x, float z, Vector2 noiseOffset)
        {
            if (petalColors == null || petalColors.Length == 0)
            {
                return Color.white;
            }

            float scale = 1f / Mathf.Max(MinPatchSize, colorPatchSize);
            int best = 0;
            float bestScore = float.MinValue;

            for (int i = 0; i < petalColors.Length; i++)
            {
                float layer = i * ColorLayerSpacing;
                float score = Mathf.PerlinNoise(noiseOffset.x + layer + x * scale, noiseOffset.y + layer + z * scale);
                score += Rand() * colorMixing;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }

            return petalColors[best];
        }

        // Modifie légèrement teinte, saturation et luminosité pour que deux fleurs ne soient pas identiques.
        private Color VaryColor(Color color, float amount)
        {
            Color.RGBToHSV(color, out float hue, out float saturation, out float brightness);
            hue = Mathf.Repeat(hue + (Rand() - 0.5f) * amount * HueVariation, 1f);
            saturation = Mathf.Clamp01(saturation * (1f + (Rand() - 0.5f) * amount * SaturationVariation));
            brightness = Mathf.Clamp01(brightness * (1f + (Rand() - 0.5f) * amount * BrightnessVariation));
            return Color.HSVToRGB(hue, saturation, brightness);
        }

        // Cherche le sol sous la fleur avec un rayon lancé vers le bas (raycast).
        private Vector3 SnapToGround(Vector3 localPosition)
        {
            Vector3 worldPosition = transform.TransformPoint(localPosition);
            Vector3 rayStart = worldPosition + Vector3.up * groundSearchDistance;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundSearchDistance * 2f, groundMask,
                    QueryTriggerInteraction.Ignore))
            {
                return transform.InverseTransformPoint(hit.point);
            }

            return localPosition;
        }

        private void CreateChunk(Mesh mesh)
        {
            mesh.hideFlags = HideFlags.HideAndDontSave;

            GameObject chunk = new GameObject(mesh.name);
            chunk.hideFlags = HideFlags.HideAndDontSave;
            chunk.layer = gameObject.layer;
            chunk.transform.SetParent(transform, false);

            chunk.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer chunkRenderer = chunk.AddComponent<MeshRenderer>();
            chunkRenderer.sharedMaterial = runtimeMaterial;
            chunkRenderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;

            chunks.Add(chunk);
            meshes.Add(mesh);
        }

        private void ClearChunks()
        {
            foreach (GameObject chunk in chunks)
            {
                DestroyGenerated(chunk);
            }

            foreach (Mesh mesh in meshes)
            {
                DestroyGenerated(mesh);
            }

            chunks.Clear();
            meshes.Clear();
        }

        // Crée le matériau (invisible dans le projet) à partir du shader du vent.
        private bool PrepareMaterial()
        {
            if (windShader == null)
            {
                windShader = Shader.Find(WindShaderName);
            }

            if (windShader == null)
            {
                Debug.LogWarning($"FlowerField : shader \"{WindShaderName}\" introuvable, le champ ne peut pas être dessiné.", this);
                return false;
            }

            if (runtimeMaterial == null || runtimeMaterial.shader != windShader)
            {
                DestroyGenerated(runtimeMaterial);
                runtimeMaterial = new Material(windShader);
                runtimeMaterial.name = "FlowerField (généré)";
                runtimeMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            return true;
        }

        // Envoie les réglages du vent au shader.
        private void ApplyWindSettings()
        {
            Vector3 wind = GetWindDirection();
            runtimeMaterial.SetVector(WindDirectionId, new Vector4(wind.x, 0f, wind.z, 0f));
            runtimeMaterial.SetFloat(WindStrengthId, windStrength);
            runtimeMaterial.SetFloat(WindSpeedId, windSpeed);
            runtimeMaterial.SetFloat(GustFrequencyId, 1f / Mathf.Max(MinPatchSize, gustSize));
            runtimeMaterial.SetFloat(FlutterStrengthId, flutter);
            runtimeMaterial.SetFloat(FlutterSpeedId, flutterSpeed);
        }

        private Vector3 GetWindDirection()
        {
            float radians = windAngle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
        }

        // Dessin d'aide quand l'objet est sélectionné : contour du champ et flèche du vent.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = GizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, 0f, size.y));

            Gizmos.matrix = Matrix4x4.identity;
            Vector3 wind = GetWindDirection();
            float length = Mathf.Min(size.x, size.y) * WindGizmoScale;
            Vector3 start = transform.position + Vector3.up * heightRange.y;
            Vector3 end = start + wind * length;
            Vector3 barb = Vector3.Cross(Vector3.up, wind) * (length * 0.2f);
            Vector3 barbBase = end - wind * (length * 0.3f);

            Gizmos.DrawLine(start, end);
            Gizmos.DrawLine(end, barbBase + barb);
            Gizmos.DrawLine(end, barbBase - barb);
        }

        // --- Hasard répétable ---

        // Démarre une suite de hasard propre à un numéro : même graine + même numéro = même suite.
        private void SeedRandom(int index)
        {
            unchecked
            {
                uint hash = (uint)seed * 747796405u + (uint)index * 2891336453u + 0x9E3779B9u;
                hash ^= hash >> 16;
                hash *= 0x7FEB352Du;
                hash ^= hash >> 15;
                hash *= 0x846CA68Bu;
                hash ^= hash >> 16;
                randomState = hash != 0u ? hash : 1u;
            }
        }

        // Nombre au hasard entre 0 (inclus) et 1 (exclu). Méthode "xorshift" : rapide,
        // et identique sur toutes les machines (contrairement à un hasard qui dépend de l'ordinateur).
        private float Rand()
        {
            randomState ^= randomState << 13;
            randomState ^= randomState >> 17;
            randomState ^= randomState << 5;
            return (randomState >> 8) * (1f / 16777216f);
        }

        private float RandRange(Vector2 range)
        {
            return Mathf.Lerp(range.x, range.y, Rand());
        }

        // Entier au hasard entre min et max, tous deux inclus.
        private int RandRangeInt(int min, int max)
        {
            return min + Mathf.FloorToInt(Rand() * (Mathf.Max(min, max) - min + 1));
        }

        private Vector2 RandNoiseOffset()
        {
            return new Vector2(NoiseOffsetBase + Rand() * NoiseOffsetRange, NoiseOffsetBase + Rand() * NoiseOffsetRange);
        }

        // --- Petits outils ---

        // Remet une plage (min, max) dans l'ordre et dans les limites autorisées.
        private static Vector2 SortRange(Vector2 range, float min, float max)
        {
            float low = Mathf.Clamp(range.x, min, max);
            float high = Mathf.Clamp(range.y, low, max);
            return new Vector2(low, high);
        }

        // Dans l'éditeur hors Play, Unity impose DestroyImmediate ; en jeu, Destroy.
        private static void DestroyGenerated(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
