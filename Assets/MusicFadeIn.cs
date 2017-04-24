using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicFadeIn : MonoBehaviour {

    AudioSource aSource;
	// Use this for initialization
	void Start () {
        aSource = GetComponent<AudioSource>();
        aSource.mute = PlayerPrefs.GetInt("MutePref", 0) == 1;
        SoundManager.instance.SFXVolume = aSource.mute ? 0 : 1;

    }
	
	// Update is called once per frame
	void Update () {
        if (aSource.volume < 1)
            aSource.volume += Time.deltaTime * .03f;
        else
            aSource.volume = 1;
	}

    public void SetMute(bool value)
    {
        //aSource.mute = true;
        aSource.mute = value;
        SoundManager.instance.SFXVolume = aSource.mute ? 0 : 1;
    }
}
