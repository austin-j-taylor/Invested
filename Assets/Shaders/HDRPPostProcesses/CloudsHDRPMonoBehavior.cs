using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CloudsHDRPMonoBehavior : MonoBehaviour
{

    const string headerDecoration = " --- ";

    public Light sunLight;
    public Transform container;

    [HideInInspector]
    public WeatherMap weatherMapGen;
    [HideInInspector]
    public NoiseGenerator noise;

    [HideInInspector]
    public bool paramsSet = false;

    private void Awake() {
        if (weatherMapGen == null)
            weatherMapGen = gameObject.GetComponentInChildren<WeatherMap>();
        if (noise == null)
            noise = gameObject.GetComponentInChildren<NoiseGenerator>();
        if (Application.isPlaying && weatherMapGen) {
            weatherMapGen.container = container;
            weatherMapGen.UpdateMap();
        }
        paramsSet = false;
    }

}
