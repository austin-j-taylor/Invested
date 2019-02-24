using UnityEngine;
using System.Collections;

/*
 * Subclass for all other allomancers (i.e. iron/steel, pewter)
 * Used by Seekers to know if someone is burning metals
 */
public abstract class Allomancer : MonoBehaviour {

    public abstract bool IsBurningMetal();
    //public abstract bool BurnRate();

}
