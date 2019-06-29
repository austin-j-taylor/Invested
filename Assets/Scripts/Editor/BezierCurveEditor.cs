using UnityEditor;
using UnityEngine;

/*
 * 
 * Modified from https://catlikecoding.com/unity/tutorials/curves-and-splines/
 */
[CustomEditor(typeof(BezierCurve), true)]
public class BezierCurveEditor : Editor {

    private const int stepsPerCurve = 30;
    private const float directionScale = 0.5f;
    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;

    private BezierCurve spline;
    private Transform handleTransform;
    private Quaternion handleRotation;
    private int selectedIndex = -1;

    private static readonly Color[] modeColors = {
        Color.cyan,
        Color.white,
        Color.yellow
    };

    public override void OnInspectorGUI() {
        spline = target as BezierCurve;
        EditorGUI.BeginChangeCheck();
        serializedObject.FindProperty("loop").boolValue = EditorGUILayout.Toggle("Loop", spline.Loop);
        serializedObject.FindProperty("animationTime").floatValue = EditorGUILayout.FloatField("Animation Time", spline.AnimationTime);
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(spline, "Toggle Loop");
            Undo.RecordObject(spline, "Change Animation Time");
            EditorUtility.SetDirty(spline);
            spline.Loop = serializedObject.FindProperty("loop").boolValue;
            spline.AnimationTime = serializedObject.FindProperty("animationTime").floatValue;
            serializedObject.ApplyModifiedProperties();
        }
        if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount) {
            DrawSelectedPointInspector();
        }
        if (GUILayout.Button("Add Curve")) {
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }
    }

    private void DrawSelectedPointInspector() {
        GUILayout.Label("Selected Point");
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(spline, "Move Point");
            EditorUtility.SetDirty(spline);
            spline.SetControlPoint(selectedIndex, point);
        }
        EditorGUI.BeginChangeCheck();
        BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(spline, "Change Point Mode");
            spline.SetControlPointMode(selectedIndex, mode);
            EditorUtility.SetDirty(spline);
        }
    }

    private void OnSceneGUI() {
        spline = target as BezierCurve;
        handleTransform = spline.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        Vector3 p0 = ShowPoint(0);
        for (int i = 1; i < spline.ControlPointCount; i += 3) {
            Vector3 p1 = ShowPoint(i);
            Vector3 p2 = ShowPoint(i + 1);
            Vector3 p3 = ShowPoint(i + 2);

            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
            p0 = p3;
        }
        // Show directions
        Handles.color = Color.green;
        Vector3 point = spline.GetPoint(0f);
        Handles.DrawLine(point, point + spline.GetDirection(0f) * directionScale);
        int steps = stepsPerCurve * spline.CurveCount;
        for (int i = 1; i <= steps; i++) {
            point = spline.GetPoint(i / (float)steps);
            Handles.DrawLine(point, point + spline.GetDirection(i / (float)steps) * directionScale);
        }
    }

    private Vector3 ShowPoint(int index) {
        Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
        float size = HandleUtility.GetHandleSize(point);
        if (index == 0) {
            size *= 2f;
        }
        Handles.color = modeColors[(int)spline.GetControlPointMode(index)];
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) {
            selectedIndex = index;
            Repaint();
        }
        if (selectedIndex == index) {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }
        return point;
    }
}