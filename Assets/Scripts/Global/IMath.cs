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

    /// <summary>
    /// Returns the taxi-cab distance between two points.
    /// </summary>
    public static float TaxiDistance(float x1, float y1, float x2, float y2) {
        return Mathf.Abs(x2 - x1) + Mathf.Abs(y2 - y1);
    }
    /// <summary>
    /// Returns the taxi-cab distance between two points.
    /// </summary>
    public static float TaxiDistance(Vector2 v1, Vector2 v2) {
        return TaxiDistance(v1.x, v1.y, v2.x, v2.y);
    }
    /// <summary>
    /// Returns the taxi-cab distance between two points, using their x and z components.
    /// </summary>
    public static float TaxiDistance(Vector3 v1, Vector3 v2) {
        return TaxiDistance(v1.x, v1.z, v2.x, v2.z);
    }

    /// <summary>
    /// Returns true if destination1 is closer to the source than destination2 by taxicab distance
    /// </summary>
    /// <param name="source">the source position</param>
    /// <param name="destination1">the first destination to check</param>
    /// <param name="destination2">the second destination to check</param>
    /// <returns>true if source-to-destination1 is shorter than source-to-destination2</returns>
    public static bool IsCloserTaxi(Vector3 source, Vector3 destination1, Vector3 destination2) {
        return TaxiDistance(source, destination1) < TaxiDistance(source, destination2);
    }
}
