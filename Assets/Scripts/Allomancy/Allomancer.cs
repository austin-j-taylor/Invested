using UnityEngine;
using System.Collections;

/*
 * Subclass for all other allomancers (i.e. iron/steel, pewter)
 * Used by Seekers to know if someone is burning metals
 */
public abstract class Allomancer : MonoBehaviour {

    public virtual bool IsBurning { get; protected set; } = false;
    //public abstract bool BurnPercentage();
    public virtual void Clear() {
        IsBurning = false;
    }

    private void OnDestroy() {
        GameManager.RemoveAllomancer(this);
    }
}
