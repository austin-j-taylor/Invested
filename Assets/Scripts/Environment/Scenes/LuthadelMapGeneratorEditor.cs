using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Environment_LuthadelMapGenerator))]
public class LuthadelMapGeneratorEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        Environment_LuthadelMapGenerator myScript = (Environment_LuthadelMapGenerator)target;
        if (GUILayout.Button("Generate Houses")) {
            myScript.GenerateHouses();
        }
        if (GUILayout.Button("Destroy Houses")) {
            myScript.DestroyHouses();
        }
        if (GUILayout.Button("Reset Map Colors")) {
            myScript.ResetMapColors();
        }
    }


}