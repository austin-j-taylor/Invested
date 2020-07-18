using UnityEngine;
using System.Collections;

/// <summary>
/// Subclass for all menus, like the pause menu and pause menu.
/// A Menu is something that appears on the screen canvas that the user can interact with.
/// If a menu is open, the player cannot control the player character.
/// </summary>
public class Menu : MonoBehaviour {

    public bool IsOpen => gameObject.activeSelf;

    /// <summary>
    /// Opens the menu.
    /// </summary>
    public virtual void Open() {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Closes the menu.
    /// </summary>
    public virtual void Close() {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Renders the menu invisible, but doesn't actually close it.
    /// Only used by some menus.
    /// </summary>
    public virtual void HideContents() {}
}
