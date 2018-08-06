using UnityEngine;
/*
 * At startup, loads the Title Screen scene
 */
public class GameManager : MonoBehaviour {
    
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private Material targetHighlightMaterial;
    public Material TargetHighlightMaterial { get { return targetHighlightMaterial; } }

    void Awake() {
        if(Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
        }
    }

    void Start() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneSelectMenu.sceneTitleScreen);
    }
}
