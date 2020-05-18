using UnityEngine;
using System.Collections;
using static ConversationManager;

// Put on a character who speaks in a conversation. Plays "voice beeps" as they speak.
public class VoiceBeeper : MonoBehaviour {

    [SerializeField]
    private AudioClip beepLow = null, beepHigh = null;

    private AudioSource sourceLow, sourceHigh;
    private bool lastPlayedHigh;

    [SerializeField]
    Speaker speaker;


    private void Start() {
        sourceLow = gameObject.AddComponent<AudioSource>();
        sourceHigh = gameObject.AddComponent<AudioSource>();
        sourceLow.clip = beepLow;
        sourceHigh.clip = beepHigh;
        sourceLow.outputAudioMixerGroup = GameManager.AudioManager.MixerVoiceBeepsGroup;
        sourceHigh.outputAudioMixerGroup = GameManager.AudioManager.MixerVoiceBeepsGroup;
        sourceLow.playOnAwake = false;
        sourceHigh.playOnAwake = false;
        lastPlayedHigh = false;
    }

    public void Beep() {
        if (lastPlayedHigh)
            sourceLow.Play();
        else
            sourceHigh.Play();
        lastPlayedHigh = !lastPlayedHigh;
    }
}
