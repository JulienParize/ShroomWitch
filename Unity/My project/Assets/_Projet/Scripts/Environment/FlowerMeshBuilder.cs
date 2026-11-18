using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShroomWitch
{
    /// <summary>
    /// Fabrique le maillage (mesh : la liste de triangles) d'un morceau de champ.
    /// On lui ajoute des fleurs une par une avec AddFlower, puis Build crée le mesh final.
    /// Chaque sommet retient le pied de sa fleur : le shader s'en sert pour la plier au vent.
    /// </summary>
    public class FlowerMeshBuilder
    {
        private const float TwoPi = Mathf.PI * 2f;

        // Au-delà de ce nombre de sommets, le mesh doit utiliser des index sur 32 bits.
        private const int UInt16VertexLimit = 65535;

        // Tige : un petit tube à 3 côtés, qui s'affine vers le haut.
        private const int StemSides = 3;
        private const float StemTipScale = 0.6f;

        // Pétales et feuilles : des losanges, plus larges un peu après le milieu.
        private const float DiamondWidestPoint = 0.55f;
        private const float PetalRootInset = 0.5f;
        private const float PetalAngleJitter = 0.35f;
        private const float PetalCupJitter = 0.2f;

        // Cœur : une petite pyramide.
        private const int CenterSides = 5;
        private const float CenterHeightRatio = 0.6f;

        // Feuilles : hauteur sur la tige (0 = pied, 1 = sommet) et relèvement (en radians).
        private const float LeafMinHeight = 0.12f;
        private const float LeafMaxHeight = 0.45f;
        private const float LeafMinElevation = 0.45f;
        private const float LeafMaxElevation = 0.95f;

        // Fraction de tour entre deux feuilles (angle d'or) : elles se répartissent autour de la tige.
        private const float LeafSpiral = 0.382f;

        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Vector3> normals = new List<Vector3>();
        private readonly List<Color32> colors = new List<Color32>();
        private readonly List<Vector4> pivots = new List<Vector4>();
        private readonly List<int> triangles = new List<int>();

        // Pied de la fleur en cours (xyz) + son nombre au hasard (w), recopiés dans chacun de ses sommets.
        private Vector4 currentPivot;

        public int VertexCount => vertices.Count;

        /// <summary>Ajoute une fleur complète : tige, feuilles, pétales et cœur.</summary>
        public void AddFlower(in FlowerShape shape)
        {
            currentPivot = new Vector4(shape.basePosition.x, shape.basePosition.y, shape.basePosition.z, shape.random01);
            Vector3 top = shape.basePosition + new Vector3(shape.leanOffset.x, shape.height, shape.leanOffset.y);

            AddStem(shape, top);
            AddLeaves(shape, top);
            AddHead(shape, top);
        }

        /// <summary>Crée le mesh Unity à partir de tout ce qui a été ajouté.</summary>
        /// <param name="boundsMargin">Marge (en mètres) autour du mesh, pour le mouvement dû au vent.</param>
        public Mesh Build(string name, float boundsMargin)
        {
            Mesh mesh = new Mesh();
            mesh.name = name;
            mesh.indexFormat = vertices.Count > UInt16VertexLimit ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetColors(colors);
            mesh.SetUVs(0, pivots);
            mesh.SetTriangles(triangles, 0);

            // Le vent déplace les sommets : on agrandit la boîte englobante pour qu'Unity
            // ne cache pas un morceau de champ encore visible au bord de l'écran.
            Bounds bounds = mesh.bounds;
            bounds.Expand(boundsMargin * 2f);
            mesh.bounds = bounds;

            // Envoie le mesh à la carte graphique et libère la copie gardée en mémoire.
            mesh.UploadMeshData(true);
            return mesh;
        }

        private void AddStem(in FlowerShape shape, Vector3 top)
        {
            int segments = Mathf.Max(1, shape.stemSegments);
            float yaw = Variation(shape.random01, 1) * TwoPi;
            int start = vertices.Count;

            // Des anneaux de 3 sommets empilés du pied au sommet.
            for (int ring = 0; ring <= segments; ring++)
            {
                float t = ring / (float)segments;
                Vector3 center = StemPoint(shape.basePosition, top, t);
                float radius = shape.stemRadius * Mathf.Lerp(1f, StemTipScale, t);

                for (int side = 0; side < StemSides; side++)
                {
                    float angle = yaw + side * TwoPi / StemSides;
                    Vector3 outward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    AddVertex(center + outward * radius, outward, shape.stemColor);
                }
            }

            // Deux triangles entre chaque paire de sommets voisins de deux anneaux.
            for (int ring = 0; ring < segments; ring++)
            {
                for (int side = 0; side < StemSides; side++)
                {
                    int a = start + ring * StemSides + side;
                    int b = start + ring * StemSides + (side + 1) % StemSides;
                    int c = a + StemSides;
                    int d = b + StemSides;
                    Vector3 outward = normals[a] + normals[b];

                    AddTriangle(a, c, b, outward);
                    AddTriangle(b, c, d, outward);
                }
            }
        }

        private void AddLeaves(in FlowerShape shape, Vector3 top)
        {
            float firstAngle = Variation(shape.random01, 3);

            for (int leaf = 0; leaf < shape.leafCount; leaf++)
            {
                float t = Mathf.Lerp(LeafMinHeight, LeafMaxHeight, Variation(shape.random01, 10 + leaf * 2));
                float elevation = Mathf.Lerp(LeafMinElevation, LeafMaxElevation, Variation(shape.random01, 11 + leaf * 2));
                float azimuth = (firstAngle + leaf * LeafSpiral) * TwoPi;

                Vector3 outward = new Vector3(Mathf.Cos(azimuth), 0f, Mathf.Sin(azimuth));
                Vector3 side = new Vector3(-outward.z, 0f, outward.x);
                Vector3 direction = outward * Mathf.Cos(elevation) + Vector3.up * Mathf.Sin(elevation);

                AddDiamond(StemPoint(shape.basePosition, top, t), direction, side,
                    shape.leafLength, shape.leafWidth, Vector3.up, shape.leafColor);
            }
        }

        private void AddHead(in FlowerShape shape, Vector3 top)
        {
            Vector3 up = shape.headDirection.sqrMagnitude > 0.0001f ? shape.headDirection.normalized : Vector3.up;

            // Deux axes perpendiculaires à "up", pour tourner autour de la tête.
            Vector3 reference = Mathf.Abs(up.z) < 0.9f ? Vector3.forward : Vector3.right;
            Vector3 right = Vector3.Cross(up, reference).normalized;
            Vector3 forward = Vector3.Cross(right, up);

            float startAngle = Variation(shape.random01, 2) * TwoPi;

            for (int petal = 0; petal < shape.petalCount; petal++)
            {
                // Petites irrégularités : les pétales ne sont ni parfaitement espacés ni tous relevés pareil.
                float jitter = (Variation(shape.random01, 20 + petal * 2) - 0.5f) * PetalAngleJitter;
                float angle = startAngle + (petal + jitter) * TwoPi / shape.petalCount;
                float cup = shape.petalCupAngle + (Variation(shape.random01, 21 + petal * 2) - 0.5f) * PetalCupJitter;

                Vector3 outward = right * Mathf.Cos(angle) + forward * Mathf.Sin(angle);
                Vector3 side = forward * Mathf.Cos(angle) - right * Mathf.Sin(angle);
                Vector3 direction = outward * Mathf.Cos(cup) + up * Mathf.Sin(cup);
                Vector3 root = top + outward * (shape.centerRadius * PetalRootInset);

                AddDiamond(root, direction, side, shape.petalLength, shape.petalWidth, up, shape.petalColor);
            }

            AddCenter(top, up, right, forward, shape.centerRadius, shape.centerColor);
        }

        // Cœur de la fleur : une petite pyramide posée au sommet de la tige.
        private void AddCenter(Vector3 center, Vector3 up, Vector3 right, Vector3 forward, float radius, Color32 color)
        {
            Vector3 apex = center + up * (radius * CenterHeightRatio);

            for (int side = 0; side < CenterSides; side++)
            {
                float angle0 = side * TwoPi / CenterSides;
                float angle1 = (side + 1) * TwoPi / CenterSides;
                Vector3 corner0 = center + (right * Mathf.Cos(angle0) + forward * Mathf.Sin(angle0)) * radius;
                Vector3 corner1 = center + (right * Mathf.Cos(angle1) + forward * Mathf.Sin(angle1)) * radius;

                AddFlatTriangle(apex, corner0, corner1, up, color);
            }
        }

        // Losange plat (pétale ou feuille) : part de "root", s'élargit, puis se referme en pointe.
        private void AddDiamond(Vector3 root, Vector3 direction, Vector3 side, float length, float width,
            Vector3 upHint, Color32 color)
        {
            Vector3 middle = root + direction * (length * DiamondWidestPoint);
            Vector3 left = middle - side * (width * 0.5f);
            Vector3 right = middle + side * (width * 0.5f);
            Vector3 tip = root + direction * length;

            // La normale (direction "vers l'extérieur" de la surface) sert à l'éclairage.
            Vector3 normal = Vector3.Cross(side, direction).normalized;
            if (Vector3.Dot(normal, upHint) < 0f)
            {
                normal = -normal;
            }

            int start = vertices.Count;
            AddVertex(root, normal, color);
            AddVertex(left, normal, color);
            AddVertex(right, normal, color);
            AddVertex(tip, normal, color);

            AddTriangle(start, start + 1, start + 2, normal);
            AddTriangle(start + 1, start + 3, start + 2, normal);
        }

        // Triangle isolé à facette plate (look low poly) : ses 3 sommets partagent la même normale.
        private void AddFlatTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 outward, Color32 color)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            if (Vector3.Dot(normal, outward) < 0f)
            {
                normal = -normal;
            }

            int start = vertices.Count;
            AddVertex(a, normal, color);
            AddVertex(b, normal, color);
            AddVertex(c, normal, color);

            AddTriangle(start, start + 1, start + 2, normal);
        }

        // Ajoute un triangle entre 3 sommets existants, en choisissant l'ordre des sommets
        // pour que sa face avant regarde du côté "outward" (Unity déduit la face avant de cet ordre).
        private void AddTriangle(int a, int b, int c, Vector3 outward)
        {
            Vector3 faceNormal = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);

            if (Vector3.Dot(faceNormal, outward) < 0f)
            {
                int swap = b;
                b = c;
                c = swap;
            }

            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        private void AddVertex(Vector3 position, Vector3 normal, Color32 color)
        {
            vertices.Add(position);
            normals.Add(normal);
            colors.Add(color);
            pivots.Add(currentPivot);
        }

        // Point de la tige à la hauteur t (0 = pied, 1 = sommet) : elle part droite puis se penche.
        private static Vector3 StemPoint(Vector3 basePosition, Vector3 top, float t)
        {
            Vector3 delta = top - basePosition;
            return basePosition + new Vector3(delta.x * t * t, delta.y * t, delta.z * t * t);
        }

        // Petit hasard répétable entre 0 et 1 : même fleur + même numéro = même valeur.
        private static float Variation(float seed, int index)
        {
            float value = Mathf.Sin(seed * 91.345f + index * 47.853f) * 43758.547f;
            return value - Mathf.Floor(value);
        }
    }
}
