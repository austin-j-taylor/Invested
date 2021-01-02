using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Controls the "mists" that cover the camera's view when the player needs to warp somewhere, or load somewhere, etc.
/// </summary>
public class LoadingFadeController : MonoBehaviour {

    [SerializeField]
    private ParticleSystem mists = null;

    public void Clear() {
        //mists.Stop();
        transform.localRotation = Quaternion.identity;
    }

    private void LateUpdate() {
        mists.transform.position = CameraController.ActiveCamera.transform.position + CameraController.ActiveCamera.transform.forward * (CameraController.ActiveCamera.nearClipPlane + 0.01f);
    }

    public void Enshroud() {
        mists.Play();
    }
}
