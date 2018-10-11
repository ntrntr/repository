using UnityEngine;

namespace KinematicCharacterController
{
    public class CompareUtility
    {
        public static bool IsApproximatelyEqual(bool left, bool right)
        {
            return left == right;
        }

        public static bool IsApproximatelyEqual(int left, int right)
        {
            return left == right;
        }

        public static bool IsApproximatelyEqual(float left, float right, float maxError = 0.001f)
        {
            return System.Math.Abs(left - right) < maxError;
        }
        public static bool IsApproximatelyEqual(double left, double right, double maxError = 0.001f)
        {
            return System.Math.Abs(left - right) < maxError;
        }
        public static bool IsApproximatelyEqual(Vector3 left, Vector3 right, float maxError = 0.001f)
        {
            Vector3 l = left;
            Vector3 r = right;
            return IsApproximatelyEqual(l.x, r.x, maxError) && 
                   IsApproximatelyEqual(l.y, r.y, maxError) &&
                   IsApproximatelyEqual(l.z, r.z, maxError);
        }

        public static bool IsApproximatelyEqual(Quaternion left, Quaternion right, float maxError = 0.001f)
        {
            Quaternion l = left;
            Quaternion r = right;
            return IsApproximatelyEqual(l.x, r.x, maxError) && 
                   IsApproximatelyEqual(l.y, r.y, maxError) &&
                   IsApproximatelyEqual(l.z, r.z, maxError) &&
                   IsApproximatelyEqual(l.w, r.w, maxError);
        }
    }
}