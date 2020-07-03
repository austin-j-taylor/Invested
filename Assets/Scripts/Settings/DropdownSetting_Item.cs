using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// An item in a dropdown list.
/// </summary>
public class DropdownSetting_Item : MonoBehaviour {

    [SerializeField]
    public Selectable scrollBar = null;

    void Start() {
        Toggle toggle = GetComponent<Toggle>();
        Navigation nav = toggle.navigation;
        nav.selectOnRight = scrollBar;
        toggle.navigation = nav;
    }
}
