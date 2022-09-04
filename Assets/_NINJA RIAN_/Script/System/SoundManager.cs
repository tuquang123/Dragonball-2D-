﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/*
 * This is SoundManager
 * In other script, you just need to call SoundManager.PlaySfx(AudioClip) to play the sound
*/
public class SoundManager : MonoBehaviour {
    public static SoundManager Instance;
    //[Header("FORCE PLAY MUSIC")]
    //public AudioClip forcePlayMusic;

    [Header("MAIN MENU")]
    public AudioClip beginSoundInMainMenu;
    [Tooltip("Play music clip when start")]
    public AudioClip musicsMenu;
    [Range(0, 1)]
    public float musicMenuVolume = 0.5f;
    public AudioClip musicWorldChoose, musicLevelChoose;

    [Header("GAMEPLAY")]
    public AudioClip musicsGame;
    [Range(0, 1)]
    public float musicsGameVolume = 0.5f;

    public AudioClip musicFinishPanel;

    [Header("Shop")]
    public AudioClip soundPurchased;
    public AudioClip soundUpgrade;
    public AudioClip soundNotEnoughCoin;

    [Tooltip("Place the sound in this to call it in another script by: SoundManager.PlaySfx(soundname);")]
    public AudioClip soundClick;
    public AudioClip soundGamefinish;
    public AudioClip soundGameover;

    private AudioSource musicAudio;
    private AudioSource soundFx;

    public AudioClip soundCheckpoint;
    [Range(0, 1)]
    public float soundCheckpointVolume = 0.5f;
    public AudioClip soundTimeLess;
    [Range(0, 1)]
    public float soundTimeLessVolume = 0.5f;
    public AudioClip soundTimeUp;
    [Range(0, 1)]
    public float soundTimeUpVolume = 0.5f;

    //GET and SET
    public static float MusicVolume {
        set { Instance.musicAudio.volume = value; }
        get { return Instance.musicAudio.volume; }
    }
    public static float SoundVolume {
        set { Instance.soundFx.volume = value; }
        get { return Instance.soundFx.volume; }
    }

    public static void ResetMusic()
    {
        Instance.musicAudio.Stop();
        Instance.musicAudio.Play();
    }

    public void PauseMusic(bool isPause)
    {
        if (isPause)
            Instance.musicAudio.mute = true;
        //			Instance.musicAudio.Pause ();
        else
            Instance.musicAudio.mute = false;
        //			Instance.musicAudio.UnPause ();
    }

    public static void Click() {
        PlaySfx(Instance.soundClick, 1);
    }
    // Use this for initialization
    void Awake() {
        Instance = this;
        musicAudio = gameObject.AddComponent<AudioSource>();
        musicAudio.loop = true;
        musicAudio.volume = 0.5f;
        soundFx = gameObject.AddComponent<AudioSource>();
    }
    void Start()
    {
        //if (GameManager.Instance != null)
        //    PlayMusic(musicsGame, musicsGameVolume);
        //else if (forcePlayMusic != null)
        //{
        //    PlayMusic(forcePlayMusic);
        //}
    }

    public static void PlayGameMusic()
    {
        PlayMusic(Instance.musicsGame, Instance.musicsGameVolume);
    }

	public static void PlaySfx(AudioClip clip){
		Instance.PlaySound(clip, Instance.soundFx);
	}

    public static void PlaySfx(AudioClip[] clips)
    {
        if (Instance != null && clips.Length > 0)
            Instance.PlaySound(clips[Random.Range(0, clips.Length)], Instance.soundFx);
    }

    public static void PlaySfx(AudioClip[] clips, float volume)
    {
        if (Instance != null && clips.Length > 0)
            Instance.PlaySound(clips[Random.Range(0, clips.Length)], Instance.soundFx, volume);
    }

    public static void PlaySfx(AudioClip clip, float volume){
		Instance.PlaySound(clip, Instance.soundFx, volume);
	}

	public static void PlayMusic(AudioClip clip, bool loop = true){
        if (Instance != null)
            return;

        Instance.musicAudio.loop = loop;
        Instance.PlaySound (clip, Instance.musicAudio);
	}

	public static void PlayMusic(AudioClip clip, float volume){
		Instance.PlaySound (clip, Instance.musicAudio, volume);
	}

	private void PlaySound(AudioClip clip,AudioSource audioOut){
		if (clip == null) {
//			Debug.Log ("There are no audio file to play", gameObject);
			return;
		}

		if (audioOut == musicAudio) {
			audioOut.clip = clip;
			audioOut.Play ();
		} else
			audioOut.PlayOneShot (clip, SoundVolume);
	}

	private void PlaySound(AudioClip clip,AudioSource audioOut, float volume){
		if (clip == null) {
//			Debug.Log ("There are no audio file to play", gameObject);
			return;
		}

		if (audioOut == musicAudio) {
			audioOut.clip = clip;
			audioOut.Play ();
		} else
			audioOut.PlayOneShot (clip, SoundVolume * volume);
	}


}
