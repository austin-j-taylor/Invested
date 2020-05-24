using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Wire), true), CanEditMultipleObjects]
public class WireEditor : Editor {

    private const float handleSize = 0.15f;
    private const float pickSize = 0.15f;

    private Wire wire;
    private int selectedIndex = -1;
    private Transform handleTransform;
    private Quaternion handleRotation;

    public override void OnInspectorGUI() {
        wire = target as Wire;
        if (selectedIndex >= 0 && selectedIndex < wire.PointCount) {
            DrawSelectedPointInspector();
        }
        if (GUILayout.Button("Add Wire")) {
            Undo.RegisterCompleteObjectUndo(wire, "Add Curve");
            wire.AddWire();
            EditorUtility.SetDirty(wire);
        }
        if (GUILayout.Button("Remove Wire")) {
            Undo.RegisterCompleteObjectUndo(wire, "Set Scale");
            wire.RemoveWire();
            EditorUtility.SetDirty(wire);
        }
        DrawDefaultInspector();
    }

    private void DrawSelectedPointInspector() {
        GUILayout.Label("Selected Point");
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", wire.GetPoint(selectedIndex));
        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(wire, "Move Point");
            EditorUtility.SetDirty(wire);
            wire.MovePoint(selectedIndex, point);
        }
    }

    private void OnSceneGUI() {
        wire = target as Wire;
        handleTransform = wire.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        if (wire.PointCount > 0) {
            Vector3 p0 = ShowPoint(0);
            for (int i = 1; i < wire.PointCount; i++) {
                Vector3 p1 = ShowPoint(i);
            }
        }
    }
    private Vector3 ShowPoint(int index) {
        Vector3 point = handleTransform.TransformPoint(wire.GetPoint(index));
        float size = HandleUtility.GetHandleSize(point);
        Handles.color = Color.red;
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) {
            selectedIndex = index;
            Repaint();
        }
        if (selectedIndex == index) {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(wire, "Move Point");
                EditorUtility.SetDirty(wire);
                wire.MovePoint(index, handleTransform.InverseTransformPoint(point));
            }
        }
        return point;
    }
}
