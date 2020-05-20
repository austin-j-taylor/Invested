using UnityEngine;
using System;

public enum BezierControlPointMode {
    Mirrored,
    Free,
    Aligned
}

/*
 * Modified from https://catlikecoding.com/unity/tutorials/curves-and-splines/
 */
 [System.Serializable]
public class BezierCurve : MonoBehaviour {

    [SerializeField]
    public BezierControlPointMode[] modes;
    [SerializeField]
    public Vector3[] points;

    public bool Loop {
        get {
            return loop;
        }
        set {
            loop = value;
            if (value == true) {
                modes[modes.Length - 1] = modes[0];
                SetControlPoint(0, points[0]);
            }
        }
    }
    public float AnimationTime {
        get {
            return animationTime;
        }
        set {
            animationTime = value;
        }
    }
    public float Scale {
        get {
            return scale;
        }
        set {
            scale = value;
        }
    }
    public bool LocalSpace {
        get {
            return localSpace;
        }
        set {
            localSpace = value;
        }
    }
    public int ControlPointCount { get { return points.Length; } }
    public int CurveCount { get { return (points.Length - 1) / 3; } }

    [SerializeField]
    private bool loop;
    [SerializeField]
    private float animationTime = 5;
    [SerializeField]
    protected float scale = 1;
    [SerializeField]
    private bool localSpace = true;

    public void Reset() {
        points = new Vector3[] {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(4f, 0f, 0f)
        };
        modes = new BezierControlPointMode[] {
            BezierControlPointMode.Mirrored,
            BezierControlPointMode.Mirrored
        };
        scale = 1;
    }

    public void FollowCurve(Transform target, float t, bool rotateToVelocity = true) {
        target.position = GetPoint(t);
        if (rotateToVelocity)
            target.rotation = Quaternion.LookRotation(GetDirection(t));
    }

    public Vector3 GetControlPoint(int index) {
        return points[index];
    }

    public Vector3 GetPoint(float t) {
        int i;
        if (t >= 1f) {
            t = 1f;
            i = points.Length - 4;
        } else {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        Vector3 point = GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t);
        if (localSpace)
            point = transform.TransformPoint(point);
        return point;
    }

    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
        t = Mathf.Clamp01(t);
        float OneMinusT = 1f - t;
        return
            OneMinusT * OneMinusT * OneMinusT * p0 +
            3f * OneMinusT * OneMinusT * t * p1 +
            3f * OneMinusT * t * t * p2 +
            t * t * t * p3;
    }

    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
        return
            2f * (1f - t) * (p1 - p0) +
            2f * t * (p2 - p1);
    }

    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            3f * oneMinusT * oneMinusT * (p1 - p0) +
            6f * oneMinusT * t * (p2 - p1) +
            3f * t * t * (p3 - p2);
    }

    public Vector3 GetVelocity(float t) {
        int i;
        if (t >= 1f) {
            t = 1f;
            i = points.Length - 4;
        } else {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
    }

    public Vector3 GetDirection(float t) {
        return GetVelocity(t).normalized;
    }

    public BezierControlPointMode GetControlPointMode(int index) {
        return modes[(index + 1) / 3];
    }

    public void SetInitialPoint(Vector3 point, Vector3 slope) {
        Vector3 tangentOffset = points[1] - points[0];
        points[0] = point;
        points[1] = points[0] + slope;
    }

    public void SetControlPoint(int index, Vector3 point) {
        if (index % 3 == 0) {
            Vector3 delta = point - points[index];
            if (loop) {
                if (index == 0) {
                    points[1] += delta;
                    points[points.Length - 2] += delta;
                    points[points.Length - 1] = point;
                } else if (index == points.Length - 1) {
                    points[0] = point;
                    points[1] += delta;
                    points[index - 1] += delta;
                } else {
                    points[index - 1] += delta;
                    points[index + 1] += delta;
                }
            } else {
                if (index > 0) {
                    points[index - 1] += delta;
                }
                if (index + 1 < points.Length) {
                    points[index + 1] += delta;
                }
            }
        }
        points[index] = point;
        EnforceMode(index);
    }

    public void SetControlPointMode(int index, BezierControlPointMode mode) {
        int modeIndex = (index + 1) / 3;
        modes[modeIndex] = mode;
        if (loop) {
            if (modeIndex == 0) {
                modes[modes.Length - 1] = mode;
            } else if (modeIndex == modes.Length - 1) {
                modes[0] = mode;
            }
        }
        EnforceMode(index);
    }

    // Sets the points in the vector without any modification, overriding the mode, etc.
    // This assumes you know what you're doing.
    // Remember: for every 4 points:
    // point 0 = position of start
    // point 1 = latter tangent of start
    // point 2 = former tangent of end
    // point 3 = position of end
    // This loops, so point 4 is the latter tangent of end, etc...
    public void SetPoints(Vector3[] newPoints) {
        points = new Vector3[newPoints.Length];
        for(int i = 0; i < newPoints.Length; i++) {
            points[i] = newPoints[i];
        }
        Array.Resize(ref modes, newPoints.Length / 3+ 1);
        for (int i = 0; i < modes.Length; i++) {
            modes[i] = BezierControlPointMode.Free;
        }
    }

    private void EnforceMode(int index) {
        int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode = modes[modeIndex];
        if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1)) {
            return;
        }

        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex) {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0) {
                fixedIndex = points.Length - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= points.Length) {
                enforcedIndex = 1;
            }
        } else {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= points.Length) {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0) {
                enforcedIndex = points.Length - 2;
            }
        }

        Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixedIndex];
        if (mode == BezierControlPointMode.Aligned) {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }
        points[enforcedIndex] = middle + enforcedTangent;
    }
    public void SetScale() {
        for(int i = 0; i < points.Length; i++) {
            points[i] *= scale;
        }
    }
    public void AddCurve() {
        Vector3 point = points[points.Length - 1];
        Array.Resize(ref points, points.Length + 3);
        point.x += 1f;
        points[points.Length - 3] = point;
        point.x += 1f;
        points[points.Length - 2] = point;
        point.x += 1f;
        points[points.Length - 1] = point;

        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
        EnforceMode(points.Length - 4);

        if (loop) {
            points[points.Length - 1] = points[0];
            modes[modes.Length - 1] = modes[0];
            EnforceMode(0);
        }
    }
}