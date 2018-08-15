using UnityEngine;

namespace ECM.Components
{
    public sealed class SphereGroundDetection : GroundDetection
    {
        #region METHODS

        /// <summary>
        /// Performs ledge detection.
        /// </summary>

        private bool DetectLedge()
        {
            const float kTolerance = 0.05f;
            const float kTinyTolerance = 0.01f;

            var position = transform.position;

            var up = transform.up;
            var down = -up;

            var toCenter = Vector3.ProjectOnPlane((position - _hitInfo.point).normalized * kTinyTolerance, up);
            var nearPoint = _hitInfo.point + toCenter + up * kTinyTolerance;

            RaycastHit nearHitInfo;
            Physics.Raycast(nearPoint, down, out nearHitInfo, Mathf.Infinity, groundMask, QueryTriggerInteraction.Ignore);
            if (nearHitInfo.distance < kTolerance)
                return false;

            // We are on a ledge (edge of a surface),
            // so update surfaceNormal with the "most opposing" normal (opposed to the query direction).

            var awayFromCenter = Quaternion.AngleAxis(-80.0f, Vector3.Cross(toCenter, up)) * -toCenter;
            var farPoint = _hitInfo.point + awayFromCenter * 5f;

            RaycastHit farHitInfo;
            Physics.Raycast(farPoint, down, out farHitInfo, Mathf.Infinity, groundMask, QueryTriggerInteraction.Ignore);

            surfaceNormal = farHitInfo.normal;

            return true;
        }

        /// <summary>
        /// Performs ground detection using SphereCast.
        /// </summary>

        public override bool DetectGround()
        {
            var o = transform.TransformPoint(center);
            var d = distance - radius;

            var up = transform.up;
            var down = -up;

            isGrounded = detectGround && Physics.SphereCast(o, radius, down, out _hitInfo, d, groundMask.value,
                             QueryTriggerInteraction.Ignore);
            if (isGrounded)
            {
                // If we are standing on a ~flat surface, return

                if (groundNormal.y > 1.0f - 0.0001f)
                    return true;

                // If we are on a 'slope', this could be an ledge
                // (sphereCast returns a interpolated normal of 2 faces when on an edge)

                standingOnLedge = DetectLedge();
            }
            else
            {
                // If not grounded, reset info
                // This is important in order to ensure continuity even when the character is in air

                groundNormal = Vector3.up;
                groundPoint = o - transform.up * distance;

                // Reset ledge info

                standingOnLedge = false;
                surfaceNormal = groundNormal;
            }

            return isGrounded;
        }

        #endregion
    }
}
