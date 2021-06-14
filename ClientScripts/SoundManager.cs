using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public Dictionary<string, Sound> Sounds;

    //List of AudioMixerGroups
    public AudioMixerGroup Master;
    public AudioMixerGroup Music;
    public AudioMixerGroup SoundEffects;
    public AudioMixerGroup Ambience;
    public AudioMixerGroup Interface;
    public AudioMixerGroup Dialogue;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);

        //Initialise Sound List.
        Sounds = new Dictionary<string, Sound>();

        //Add Game Sounds.
        GameSounds();
    }

    //Add sounds here.
    void GameSounds()
    {
        Sounds.Add("DoorOpen", new Sound("DoorOpen", Resources.Load<AudioClip>("Sounds/DoorOpen"), 1, 1, SoundEffects));
        Sounds.Add("DoorClose", new Sound("DoorClose", Resources.Load<AudioClip>("Sounds/DoorClose"), 1, 1, SoundEffects));
        Sounds.Add("UIOpenClose", new Sound("UIOpenClose", Resources.Load<AudioClip>("Sounds/UIOpenClose"), 1, 1, Interface));
        Sounds.Add("ScuffedTheme", new Sound("ScuffedTheme", Resources.Load<AudioClip>("Sounds/ScuffedTheme"), 0.25f, 1, Music));
    }

    
    //Sound Class.
    public class Sound
    {
        public string Name { get; set; }
        public AudioClip Clip { get; set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public AudioSource Source { get; set; }
        public AudioMixerGroup Category { get; set; }

        //New Sound
        public Sound(string name, AudioClip clip, float volume, float pitch, AudioMixerGroup category)
        {
            Name = name;
            Clip = clip;
            Volume = volume;
            Pitch = pitch;
            Category = category;

            //Create empty GameObject set as child of this script's transform.
            GameObject newObj = new GameObject();
            newObj.transform.SetParent(Instance.transform.GetChild(0));
            Source = newObj.AddComponent<AudioSource>();

            //Set Audiosource properties
            newObj.name = Name;
            Source.clip = Clip;
            Source.volume = Volume;
            Source.pitch = Pitch;
            Source.outputAudioMixerGroup = Category;
        }

        public void Play()
        {
            Source.Play();
        }
    }
}
