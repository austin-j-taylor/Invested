using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages props in the current scene.
/// Keeps track of all Crates that the Waddlers can pick up.
/// </summary>
public class PropsManager : MonoBehaviour {

    //public List<Crate> CratesInScene { get; private set; } = new List<Crate>();

    //private void Awake() {
    //    SceneManager.sceneUnloaded += ClearBeforeSceneChange;
    //}

    ///// <summary>
    ///// Called when exiting a scene
    ///// </summary>
    ///// <param name="scene"></param>
    //private void ClearBeforeSceneChange(Scene scene) {
    //    CratesInScene.Clear();
    //}

    //public void AddProp(Crate crate) {
    //    CratesInScene.Add(crate);
    //}
    //public void RemoveProp(Crate crate) {
    //    CratesInScene.Remove(crate);
    //}
}
