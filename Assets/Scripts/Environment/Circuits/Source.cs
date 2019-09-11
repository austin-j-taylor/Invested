using UnityEngine;
using System.Collections;

public class Source : Destructable {

    [SerializeField]
    private Powered[] connected = null;

    protected void PowerConnected(bool on) {
        foreach (Powered powered in connected) {
            if (powered != null)
                powered.On = on;
        }
    }
}
