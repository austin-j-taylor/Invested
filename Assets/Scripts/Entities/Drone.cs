using UnityEngine;
using System.Collections;

public class Drone : MonoBehaviour {

    /*
     * Raycasts down for ground effect?
     * Apply Thrust towards transform.up
     * 


    // Global Planner
        // ignore for now

    // Local Planner
    P(ID) control to reach target waypoints for:
      position (2D)
        x (1D)
        y (1D)
      heading (1D)
      velocity (1D)
      height (1D)
     
     */


    private void FixedUpdate() {
        StepHeading();
        StepHeight();
        // etc. Steps()
    }

    private void StepHeading() {

    }
    private void StepHeight() {

    }
}
