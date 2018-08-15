using UnityEngine;

namespace ECM.Components
{
    public abstract class GroundDetection : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [Tooltip("Layers to be considered as ground (walkables).")]
        [SerializeField]
        private LayerMask _groundMask = 1; // Default layer

        [Tooltip("The character's center position in object’s local space (relative to its pivot).\n" +
                 "This determines the cast starting point.")]
        [SerializeField]
        private Vector3 _center;

        [Tooltip("The bounding volume radius.\n" +
                 "This represent the radius of the 'feet' or bottom of the character. " +
                 "A typical value should be about 90-95% of your collider's radius.")]
        [SerializeField]
        private float _radius = 0.45f;

        [Tooltip("Determines the cast distance.\n" +
                 "A typical value should be about ~10% more than your collider's half height.")]
        [SerializeField]
        private float _distance = 1.1f;

        #endregion

        #region FIELDS

        private bool _detectGround = true;

        private Vector3 _surfaceNormal = Vector3.up;

        protected RaycastHit _hitInfo;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The character's center position in object’s local space (relative to its pivot).
        /// E.g. If your character's height is 2 units, set this to 0.0f, 1.0f, 0.0f.

        /// This determines the cast starting point.
        /// </summary>

        public Vector3 center
        {
            get { return _center; }
            set { _center = value; }
        }

        /// <summary>
        /// The bounding volume radius.
        /// This value should be the radius of the 'feet' or bottom of the character,
        /// a typical value should be about 90-95% of your collider's radius.
        /// 
        /// E.g. If your character collider's radius is 0.5 units, set this to ~ 0.45f to 0.475f.
        /// </summary>

        public float radius
        {
            get { return _radius; }
            set { _radius = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Determines the cast distance.
        /// A typical value should be about ~10% more than your collider's half height.
        /// E.g. If your character's height is 2 units, you should set this to ~1.1 units.
        /// 
        /// Increase it if you loose ground often.
        /// </summary>

        public float distance
        {
            get { return _distance; }
            set { _distance = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Layers to be considered as ground (walkables).
        /// </summary>

        public LayerMask groundMask
        {
            get { return _groundMask; }
            set { _groundMask = value; }
        }

        /// <summary>
        /// Enable / disable ground detection.
        /// If disabled, isGrounded remains as false.
        /// </summary>

        public bool detectGround
        {
            get { return _detectGround; }
            set { _detectGround = value; }
        }

        /// <summary>
        /// Is this character standing on "ground".
        /// </summary>

        public bool isGrounded { get; protected set; }

        /// <summary>
        /// The impact point in world space where the cast hit the collider.
        /// If the character is not on ground, it represent a point at character's base.
        /// </summary>

        public Vector3 groundPoint
        {
            get { return _hitInfo.point; }
            protected set { _hitInfo.point = value; }
        }

        /// <summary>
        /// The normal of the surface the cast hit.
        /// If the character is not grounded, it will point up (Vector3.up).
        /// </summary>

        public Vector3 groundNormal
        {
            get { return _hitInfo.normal; }
            protected set { _hitInfo.normal = value; }
        }

        /// <summary>
        /// The distance from the ray's origin to the impact point.
        /// </summary>

        public float groundDistance
        {
            get { return _hitInfo.distance; }
            protected set { _hitInfo.distance = value; }
        }

        /// <summary>
        /// The Collider that was hit.
        /// This property is null if the cast hit nothing (not grounded) and not-null if it hit a Collider.
        /// </summary>

        public Collider groundCollider
        {
            get { return _hitInfo.collider; }
        }

        /// <summary>
        /// Tells if character is standing on a ledge.
        /// </summary>

        public bool standingOnLedge { get; protected set; }

        /// <summary>
        /// Real normal of the surface when standing on a ledge.
        /// 
        /// This is different from groundNormal, because when the SphereCast contacts the edge of a collider
        /// (rather than a face directly on) the hit.normal that is returned is the interpolation of the two normals
        /// of the faces that are joined to that edge.
        /// </summary>

        public Vector3 surfaceNormal
        {
            get { return _surfaceNormal; }
            protected set { _surfaceNormal = value; }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Abstract method used to perform ground detection.
        /// Returns true if grounded, false if not.
        /// </summary>

        public abstract bool DetectGround();

        #endregion

        #region MONOBEHAVIOUR

        /// <summary>
        /// Validate this fields.
        /// If you overrides it, be sure to call base.OnValidate to fully initialize base class.
        /// </summary>

        public virtual void OnValidate()
        {
            radius = _radius;
            distance = _distance;
        }

        #endregion
    }
}
