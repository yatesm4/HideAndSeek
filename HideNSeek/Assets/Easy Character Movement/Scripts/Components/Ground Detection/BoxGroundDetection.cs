using UnityEngine;

namespace ECM.Components
{
    public sealed class BoxGroundDetection : GroundDetection
    {
        #region EDITOR EXPOSED FIELDS

        /// <summary>
        /// Determines if the box is axis-aligned or should follows the character's rotation.
        /// </summary>

        public bool axisAligned = true;

        #endregion

        #region METHODS

        /// <summary>
        /// Perform ground detection using an BoxCast.
        /// </summary>

        public override bool DetectGround()
        {
            var o = transform.TransformPoint(center);
            var d = distance - radius;

            var q = axisAligned ? Quaternion.identity : transform.rotation;

            isGrounded = detectGround && Physics.BoxCast(o, Vector3.one * radius, -transform.up, out _hitInfo, q, d,
                             groundMask.value, QueryTriggerInteraction.Ignore);

            if (isGrounded)
                return isGrounded;

            // If not grounded, reset info
            // This is important in order to ensure continuity even when the character is in air

            groundNormal = Vector3.up;
            groundPoint = o - transform.up * distance;

            return isGrounded;
        }

        #endregion
    }
}