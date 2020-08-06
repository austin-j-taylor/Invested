using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor;

[VolumeComponentEditor(typeof(CloudsHDRP))]

sealed class GrayScaleEditor : VolumeComponentEditor {

    SerializedDataParameter m_Intensity;

    public override bool hasAdvancedMode => false;

    public override void OnEnable() {
        base.OnEnable();
        PropertyFetcher<CloudsHDRP> o = new PropertyFetcher<CloudsHDRP>(serializedObject);
        m_Intensity = Unpack(o.Find(x => x.intensity));
    }

    public override void OnInspectorGUI() {
        GUILayout.Label("Cloud Parameters");
        PropertyField(m_Intensity);
    }
}