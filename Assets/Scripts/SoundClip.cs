using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SoundClip : MonoBehaviour {

	public RoundSilderAudioPlayer main;
	public AudioClip[] sounds;
	
	public void buttonOnClick () {
		main.changeAudioClip(sounds[Random.Range(0, sounds.Length)]);
	}
}
