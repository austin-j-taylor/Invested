using UnityEngine;
using System.Collections;

/// <summary>
/// Some math functions I commonly use
/// </summary>
public static class IMath {

    /// <summary>
    /// Returns the signed angle between two vectors in radians.
    /// </summary>
    /// <param name="Va">the first vector</param>
    /// <param name="Vb">the second vector</param>
    /// <param name="plane">the plane that determines the sign</param>
    /// <param name="vectorsOnPlane">false if the vectors need to be projected onto the plane before getting their angle</param>
    /// <returns></returns>
    public static float AngleBetween_Signed(Vector3 Va, Vector3 Vb, Vector3 plane, bool vectorsOnPlane) {
        if(!vectorsOnPlane) {
            Va = Vector3.ProjectOnPlane(Va, plane);
            Vb = Vector3.ProjectOnPlane(Vb, plane);
        }

        float angle = Mathf.Acos(Vector3.Dot(Va.normalized, Vb.normalized));
        Vector3 cross = Vector3.Cross(Va, Vb);
        angle *= -Mathf.Sign(Vector3.Dot(plane, cross));
        if (float.IsNaN(angle))
            angle = 0;

        return angle;
    }

    /// <summary>
    /// Lerps from a to b by t. If the difference between a and b is less than the threshold, just return b.
    /// </summary>
    /// <param name="a">from</param>
    /// <param name="b">to</param>
    /// <param name="t">the 't'</param>
    /// <param name="threshold">the threshold between a and b</param>
    /// <returns>The lerp</returns>
    public static float FuzzyLerp(float a, float b, float t, float threshold = 0.001f) {
        if (Mathf.Abs(a - b) < threshold)
            return b;
        else
            return Mathf.Lerp(a, b, t);
    }
}
