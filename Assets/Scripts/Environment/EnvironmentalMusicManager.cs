using UnityEngine;
using System.Collections;

public class EnvironmentalMusicManager : MonoBehaviour {

    private const float crossfadeTime = 3;
    private AudioSource interior, exterior;

    private void Start() {
        AudioSource[] sources = GetComponents<AudioSource>();
        interior = sources[0];
        exterior = sources[1];
    }

    public void StartInterior() {
        interior.Play();
        exterior.Play();
        exterior.volume = 0;
        interior.volume = 1;
    }
    public void StartExterior() {
        interior.Play();
        exterior.Play();
        interior.volume = 0;
        exterior.volume = 1;
    }

    public void EnterInterior(MusicTrigger trigger) {
        StartCoroutine(Crossfade(exterior, interior));
    }
    
    public void ExitInterior(MusicTrigger trigger) {
        StartCoroutine(Crossfade(interior, exterior));
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to) {
        float count = 0;
        while(count < 1) {
            count += Time.deltaTime / crossfadeTime;
            from.volume = 1 - count;
            to.volume = count;
            yield return null;
        }
        from.volume = 0;
        to.volume = 1;
    }
}
