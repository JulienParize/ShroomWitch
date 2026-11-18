using UnityEngine;
using UnityEngine.InputSystem;

namespace ShroomWitch
{
    /// <summary>
    /// À poser sur la sorcière. Lit les touches, calcule où elle vise
    /// (le centre de l'écran) et déclenche les 3 sorts du grimoire équipé.
    /// </summary>
    public class SpellCaster : MonoBehaviour
    {
        [Header("Visée")]
        [Tooltip("Caméra utilisée pour viser. Si vide, on prend la caméra principale.")]
        [SerializeField] private Camera aimCamera;

        [Tooltip("Point d'où partent les sorts (la main, le bâton...). Si vide, on part du centre de la sorcière.")]
        [SerializeField] private Transform castOrigin;

        [Header("Sorts")]
        [SerializeField] private Spell primarySpell;
        [SerializeField] private Spell secondarySpell;
        [SerializeField] private Spell specialSpell;

        [Header("Touches")]
        [SerializeField] private InputAction primaryAction = new InputAction("Primary", InputActionType.Button, "<Mouse>/leftButton");
        [SerializeField] private InputAction secondaryAction = new InputAction("Secondary", InputActionType.Button, "<Mouse>/rightButton");
        [SerializeField] private InputAction specialAction = new InputAction("Special", InputActionType.Button, "<Keyboard>/r");

        // Tampon réutilisé pour éviter de créer un tableau à chaque tir.
        private static readonly RaycastHit[] HitBuffer = new RaycastHit[32];

        public Vector3 CastOrigin => castOrigin != null ? castOrigin.position : transform.position;

        /// <summary>Rayon qui part de la caméra et passe par le centre de l'écran (le viseur).</summary>
        public Ray AimRay
        {
            get
            {
                Camera cam = aimCamera != null ? aimCamera : Camera.main;
                return cam != null
                    ? cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))
                    : new Ray(CastOrigin, transform.forward);
            }
        }

        private void OnEnable()
        {
            primaryAction.Enable();
            secondaryAction.Enable();
            specialAction.Enable();
        }

        private void OnDisable()
        {
            primaryAction.Disable();
            secondaryAction.Disable();
            specialAction.Disable();
        }

        private void Update()
        {
            // Touche maintenue = le sort se relance dès que sa recharge est finie.
            TryCast(primaryAction, primarySpell);
            TryCast(secondaryAction, secondarySpell);
            TryCast(specialAction, specialSpell);
        }

        private void TryCast(InputAction action, Spell spell)
        {
            if (spell != null && action.IsPressed())
            {
                spell.TryCast(this);
            }
        }

        /// <summary>
        /// Cherche ce qui se trouve sous le viseur. Ignore la sorcière elle-même
        /// et tout ce qui est entre la caméra et elle.
        /// </summary>
        /// <param name="radius">Épaisseur du tir : 0 = rayon fin, plus grand = visée plus tolérante.</param>
        public bool TryAim(float range, float radius, LayerMask mask, out RaycastHit bestHit)
        {
            Ray ray = AimRay;

            // La caméra est derrière la sorcière : on allonge la portée d'autant
            // et on ignore ce qui est touché avant d'arriver à sa hauteur.
            float minDistance = Mathf.Max(0f, Vector3.Dot(CastOrigin - ray.origin, ray.direction));
            float maxDistance = minDistance + range;

            int count = radius > 0f
                ? Physics.SphereCastNonAlloc(ray, radius, HitBuffer, maxDistance, mask, QueryTriggerInteraction.Ignore)
                : Physics.RaycastNonAlloc(ray, HitBuffer, maxDistance, mask, QueryTriggerInteraction.Ignore);

            bestHit = default;
            bool found = false;

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = HitBuffer[i];

                if (hit.distance <= 0f || hit.distance < minDistance || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (!found || hit.distance < bestHit.distance)
                {
                    bestHit = hit;
                    found = true;
                }
            }

            return found;
        }

        /// <summary>Point du monde visé : l'obstacle sous le viseur, ou le bout de la portée s'il n'y a rien.</summary>
        public Vector3 GetAimPoint(float range, LayerMask mask)
        {
            if (TryAim(range, 0f, mask, out RaycastHit hit))
            {
                return hit.point;
            }

            Ray ray = AimRay;
            float originDistance = Mathf.Max(0f, Vector3.Dot(CastOrigin - ray.origin, ray.direction));
            return ray.GetPoint(originDistance + range);
        }
    }
}
