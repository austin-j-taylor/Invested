using UnityEngine;

/*
 * Oversees a series of TriggerBeadPopups. 
 */
public class TriggerBeadOverhead : MonoBehaviour {

    public enum Section { tutorial1, tutorial2 };

    public Section section;
    
    protected TriggerBeadPopup[] beads;

    // Use this for initialization
    void Start() {
        beads = GetComponentsInChildren<TriggerBeadPopup>();

        // Assume messages.length = number of TriggerBeads in scene with same section number
        if (beads.Length == GameManager.TriggerBeadMessages[(int)section].Count)
            for (int i = 0; i < beads.Length; i++) {
                beads[i].message = GameManager.TriggerBeadMessages[(int)section][i];
            }
        else {
            Debug.LogError("Error: beads.Length != messages.Length: " + beads.Length + " != " + GameManager.TriggerBeadMessages[(int)section].Count);
        }
    }
}
