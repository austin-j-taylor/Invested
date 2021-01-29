using UnityEngine;
using System.Collections;

public class KogAudioController : MonoBehaviour {

    #region constants
    [SerializeField]
    private float foot_maxVolume = 0.5f;
    #endregion

    private AudioSource player_foot_left = null, player_foot_right = null;

    #region clearing
    void Awake() {
        AudioSource[] sources = GetComponents<AudioSource>();
        player_foot_left = sources[0];
        player_foot_right = sources[1];
    }
    public void Clear() {
        player_foot_left.Stop();
        player_foot_right.Stop();
    }
    #endregion


    #region soundMethods
    public void Play_footstep(bool isLeft, float speed) {
        if (isLeft) {
            player_foot_left.volume = (0.5f + speed) /1.5f * foot_maxVolume;
            player_foot_left.Play();
        } else {
            player_foot_right.volume = (0.5f + speed) /1.5f * foot_maxVolume;
            player_foot_right.Play();
        }
    }
    #endregion

}
