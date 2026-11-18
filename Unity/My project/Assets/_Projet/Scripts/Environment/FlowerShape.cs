using UnityEngine;

namespace ShroomWitch
{
    /// <summary>
    /// "Fiche d'identité" d'une fleur : ses mesures et ses couleurs.
    /// FlowerField la remplit au hasard, FlowerMeshBuilder la transforme en triangles.
    /// </summary>
    public struct FlowerShape
    {
        /// <summary>Position du pied de la fleur, dans l'espace local du champ.</summary>
        public Vector3 basePosition;

        /// <summary>Nombre entre 0 et 1 propre à la fleur : petites variations de forme et rythme dans le vent.</summary>
        public float random01;

        // --- Tige ---

        /// <summary>Hauteur de la fleur, en mètres.</summary>
        public float height;

        /// <summary>Décalage horizontal (x, z) du sommet de la tige par rapport au pied, en mètres.</summary>
        public Vector2 leanOffset;

        public float stemRadius;

        /// <summary>Nombre de tronçons de la tige (plus il y en a, mieux elle se courbe).</summary>
        public int stemSegments;

        // --- Tête ---

        /// <summary>Direction vers laquelle la fleur regarde (vecteur de longueur 1).</summary>
        public Vector3 headDirection;

        public int petalCount;
        public float petalLength;
        public float petalWidth;

        /// <summary>Relèvement des pétales en radians : 0 = à plat, plus grand = fleur en coupe.</summary>
        public float petalCupAngle;

        /// <summary>Rayon du cœur de la fleur, en mètres.</summary>
        public float centerRadius;

        // --- Feuilles ---

        public int leafCount;
        public float leafLength;
        public float leafWidth;

        // --- Couleurs ---

        public Color32 petalColor;
        public Color32 centerColor;
        public Color32 stemColor;
        public Color32 leafColor;
    }
}
