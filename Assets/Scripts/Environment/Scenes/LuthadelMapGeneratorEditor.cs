using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Environment_LuthadelMapGenerator))]
public class LuthadelMapGeneratorEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        Environment_LuthadelMapGenerator myScript = (Environment_LuthadelMapGenerator)target;
        if (GUILayout.Button("Build House")) {
            myScript.PlaceHouse();
        }
    }


}