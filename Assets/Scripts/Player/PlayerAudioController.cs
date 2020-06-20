using UnityEngine;
using System.Collections;

/// <summary>
/// Handles sounds that the player creates.
/// </summary>
public class PlayerAudioController : MonoBehaviour {

    public AudioListener Listener { get; private set; }

    private AudioSource player_pewter_burst = null,
                player_pewter_intro = null,
                player_pewter_loop = null,
                player_pewter_end = null,
                player_rolling_loop = null;
    private Coroutine coroutine_pewter, coroutine_rolling;

    #region clearing
    void Start() {
        Listener = GetComponent<AudioListener>();
        AudioSource[] sources = GetComponents<AudioSource>();
        player_pewter_burst = sources[0];
        player_pewter_intro = sources[1];
        player_pewter_loop = sources[2];
        player_pewter_end = sources[3];
        player_rolling_loop = sources[4];
    }
    public void Clear() {
        player_pewter_burst.Stop();
        player_pewter_intro.Stop();
        player_pewter_loop.Stop();
        player_pewter_end.Stop();
        player_rolling_loop.Stop();
    }
    #endregion

    #region soundMethods
    public void Play_pewter_burst() {
        player_pewter_burst.Play();
    }
    public void Play_pewter() {
        if (coroutine_pewter != null)
            StopCoroutine(coroutine_pewter);

        coroutine_pewter = StartCoroutine(Playing_pewter_start_loop());
    }
    public void Stop_pewter() {
        if (coroutine_pewter != null)
            StopCoroutine(coroutine_pewter);

        coroutine_pewter = StartCoroutine(Playing_pewter_end());
    }

    public void Play_rolling() {
        if (coroutine_rolling != null)
            StopCoroutine(coroutine_rolling);

        coroutine_rolling = StartCoroutine(Playing_rolling_start_loop());
    }
    public void Stop_rolling() {
        if (coroutine_rolling != null)
            StopCoroutine(coroutine_rolling);

        coroutine_rolling = StartCoroutine(Playing_rolling_end());
    }
    #endregion

    #region soundRoutines
    private IEnumerator Playing_pewter_start_loop() {

        // start the starting sound effect
        while (player_pewter_end.isPlaying || player_pewter_intro.isPlaying || player_pewter_loop.isPlaying) {
            yield return null;
        }
        player_pewter_intro.Play();
        while (player_pewter_intro.isPlaying) {
            yield return null;
        }
        player_pewter_loop.loop = true;
        player_pewter_loop.Play();
    }
    private IEnumerator Playing_pewter_end() {
        if (player_pewter_intro.isPlaying)
            yield break;
        // wait until the last sound effect is done
        player_pewter_loop.loop = false;
        while (player_pewter_end.isPlaying || player_pewter_intro.isPlaying || player_pewter_loop.isPlaying) {
            yield return null;
        }
        player_pewter_end.Play();
    }

    private IEnumerator Playing_rolling_start_loop() {
        // wait until the last sound effect is done
        player_rolling_loop.loop = false;
        while (player_rolling_loop.isPlaying) {
            yield return null;
        }

        //// start the starting sound effect
        //sources[index_rolling].clip = pewter_intro;
        //sources[index_rolling].loop = false;
        //sources[index_rolling].Play();
        //while (sources[index_rolling].isPlaying) {
        //    yield return null;
        //}
        player_rolling_loop.loop = true;
        player_rolling_loop.Play();
    }
    private IEnumerator Playing_rolling_end() {
        // wait until the last sound effect is done
        player_rolling_loop.loop = false;
        while (player_rolling_loop.isPlaying) {
            yield return null;
        }
        //sources[index_rolling].clip = rolling_end;
        //sources[index_rolling].Play();
    }
    #endregion
}
