using UnityEngine;

namespace ECM.Components
{
    #region METHODS

    public sealed class RaycastGroundDetection : GroundDetection
    {
        /// <summary>
        /// Performs ground detection using Raycast.
        /// </summary>

        public override bool DetectGround()
        {
            var o = transform.TransformPoint(center);

            isGrounded = detectGround && Physics.Raycast(o, -transform.up, out _hitInfo, distance, groundMask.value,
                             QueryTriggerInteraction.Ignore);

            if (isGrounded)
                return isGrounded;

            // If not grounded, reset info
            // This is important in order to ensure continuity even when the character is in air

            groundNormal = Vector3.up;
            groundPoint = o - transform.up * distance;

            return isGrounded;
        }
    }

    #endregion
}
