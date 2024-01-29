using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public SoundType type;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 1.5f)] public float pitch = 1f;
    public bool loop = false;

    [HideInInspector] public AudioSource source;
}

public enum SoundType { Music, SFX, Voice }

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private Sound[] sounds;

    private Dictionary<string, Sound> soundDictionary;
    private Dictionary<SoundType, float> volumeLevels = new Dictionary<SoundType, float>()
    {
        { SoundType.Music, 1f },
        { SoundType.SFX, 1f },
        { SoundType.Voice, 1f }
    };
    private bool isMuted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSounds();
            LoadPlayerSettings();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSounds()
    {
        soundDictionary = new Dictionary<string, Sound>();
        foreach (Sound sound in sounds)
        {
            GameObject soundGameObject = new GameObject("Sound_" + sound.name);
            soundGameObject.transform.SetParent(transform);

            AudioSource source = soundGameObject.AddComponent<AudioSource>();
            source.clip = sound.clip;
            source.volume = sound.volume * volumeLevels[sound.type];
            source.pitch = sound.pitch;
            source.loop = sound.loop;

            sound.source = source;
            soundDictionary.Add(sound.name, sound);
        }
    }

    private void LoadPlayerSettings()
    {
        // Load volume settings and other preferences
        // Example: volumeLevels[SoundType.Music] = PlayerPrefs.GetFloat("MusicVolume", 1f);
    }

    public void Play(string name)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound) && !isMuted)
        {
            sound.source.Play();
        }
    }

    public void Stop(string name)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            sound.source.Stop();
        }
    }

    public void SetVolume(SoundType type, float volume)
    {
        volumeLevels[type] = volume;
        foreach (var sound in soundDictionary.Values)
        {
            if (sound.type == type)
            {
                sound.source.volume = sound.volume * volume;
            }
        }
        // Save the volume setting
        // Example: PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void MuteAll(bool mute)
    {
        isMuted = mute;
        foreach (var sound in soundDictionary.Values)
        {
            sound.source.mute = mute;
        }
    }

    // Other methods like FadeIn, FadeOut, etc.
}
