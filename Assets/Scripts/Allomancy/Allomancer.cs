using UnityEngine;
using System.Collections;

/*
 * 
 * 
 */
/// <summary>
/// Subclass for all other allomancers (i.e. iron/steel, pewter)
/// Eventually will be used by Seekers to know if someone is burning metals
/// </summary>
public abstract class Allomancer : MonoBehaviour {

    public virtual bool IsBurning { get; protected set; } = false;
    //public abstract bool BurnPercentage();
    public virtual void Clear() {
        IsBurning = false;
    }

    // the GameManager keeps track of all Allomancers in the scene
    private void OnDestroy() {
        GameManager.RemoveAllomancer(this);
    }
}
