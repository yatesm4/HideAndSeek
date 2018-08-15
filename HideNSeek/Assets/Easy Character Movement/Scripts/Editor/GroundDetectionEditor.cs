using ECM.Components;
using UnityEditor;
using UnityEngine;

namespace ECM.EditorTools
{
    public class GroundDetectionEditor : Editor
    {
        [DrawGizmo(GizmoType.Selected)]
        public static void DrawGroundDetectionGizmos(GroundDetection groundDetection, GizmoType gizmosType)
        {
            if (!Application.isPlaying)
                return;

            var groundPoint = groundDetection.groundPoint;
            var groundNormal = groundDetection.groundNormal;

            // Ground Normal

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(groundPoint, groundNormal);

            // Ground point

            var r = Vector3.ProjectOnPlane(Vector3.right, groundNormal);
            var f = Vector3.ProjectOnPlane(Vector3.forward, groundNormal);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(groundPoint - r * 0.25f, groundPoint + r * 0.25f);
            Gizmos.DrawLine(groundPoint - f * 0.25f, groundPoint + f * 0.25f);
        }

        [DrawGizmo(GizmoType.Selected)]
        public static void DrawSphereGroundDetectionGizmos(SphereGroundDetection sphereGroundDetection, GizmoType gizmoType)
        {
            // SphereCast (origin and end point)

            Gizmos.color = !Application.isPlaying
                ? new Color(1.0f, 0.7f, 0.0f)
                : sphereGroundDetection.isGrounded ? Color.green : Color.red;

            var transform = sphereGroundDetection.transform;

            var o = transform.TransformPoint(sphereGroundDetection.center);
            var d = sphereGroundDetection.distance - sphereGroundDetection.radius;

            // SphereCast (origin and end point)

            Gizmos.DrawWireSphere(o, 0.05f);
            Gizmos.DrawWireSphere(o - transform.up * d, sphereGroundDetection.radius);
        }

        [DrawGizmo(GizmoType.Selected)]
        public static void DrawBoxGroundDetectionGizmos(BoxGroundDetection boxGroundDetection, GizmoType gizmoType)
        {
            // BoxCast (origin and end point)

            Gizmos.color = !Application.isPlaying
                ? new Color(1.0f, 0.7f, 0.0f)
                : boxGroundDetection.isGrounded ? Color.green : Color.red;

            var transform = boxGroundDetection.transform;

            var o = transform.TransformPoint(boxGroundDetection.center);
            var d = boxGroundDetection.distance - boxGroundDetection.radius;

            if (boxGroundDetection.axisAligned)
            {
                Gizmos.DrawWireSphere(o, 0.05f);
                Gizmos.DrawWireCube(o - transform.up * d, Vector3.one * (boxGroundDetection.radius * 2.0f));
            }
            else
            {
                Gizmos.DrawWireSphere(o, 0.05f);

                Gizmos.matrix = Matrix4x4.TRS(o, transform.rotation, transform.lossyScale);
                Gizmos.DrawWireCube(-Vector3.up * d, Vector3.one * (boxGroundDetection.radius * 2.0f));
            }
        }

        [DrawGizmo(GizmoType.Selected)]
        public static void DrawRaycastGroundDetectionGizmos(RaycastGroundDetection raycastGroundDetection, GizmoType gizmoType)
        {
            Gizmos.color = !Application.isPlaying
                ? new Color(1.0f, 0.7f, 0.0f)
                : raycastGroundDetection.isGrounded ? Color.green : Color.red;

            var transform = raycastGroundDetection.transform;

            var o = transform.TransformPoint(raycastGroundDetection.center);
            var d = raycastGroundDetection.distance;

            // origin and end point

            Gizmos.DrawWireSphere(o, 0.05f);
            Gizmos.DrawLine(o, o - transform.up * d);
        }
    }
}