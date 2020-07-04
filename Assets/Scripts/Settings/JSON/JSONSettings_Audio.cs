using UnityEngine;
using System.Collections;
using System.IO;

public class JSONSettings_Audio : JSONSettings {

    protected override string ConfigFileName => Path.Combine(Application.streamingAssetsPath, "Data", "Config", "config_Audio.json");
    protected override string DefaultConfigFileName => Path.Combine(Application.streamingAssetsPath, "Data", "Config", "config_Audio_default.json");

    public float audioMaster;
    public float audioMusic;
    public float audioEffects;
    public float audioVoiceBeeps;

    /// <summary>
    /// Manually apply certain setting effects when they are changed
    /// </summary>
    public override void SetSettingsWhenChanged() {
        GameManager.AudioManager.SetAudioLevels(audioMaster, audioMusic, audioEffects, audioVoiceBeeps);
    }
}
