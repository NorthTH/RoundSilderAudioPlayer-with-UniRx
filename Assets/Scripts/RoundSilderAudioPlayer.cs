using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections;
using UniRx;
using UniRx.Triggers;

public class RoundSilderAudioPlayer : MonoBehaviour {

	[SerializeField]
	AudioSource Audio;
	[SerializeField]
	Image TimeLine;
	[SerializeField]
	Image PlayButton;
	[SerializeField]
	Sprite[] PlayButtonImg;
	[SerializeField]
	RoundSlider slider;

	float amount = 0;
	bool isRecordPlaying = false;
	private ReactiveProperty<float> playTime = new ReactiveProperty<float>(0);

	public UnityEvent AudioStart;
	public UnityEvent AudioStop;
	
	// Use this for initialization
	void Start () {
		playTime.Value = Audio.clip.length;

		TimeLine.UpdateAsObservable()
			.Where( _ => Audio.isPlaying)
			.Subscribe( _ => {
				TimeLine.fillAmount = Audio.time/Audio.clip.length;
				amount = TimeLine.fillAmount;
		}).AddTo(this);

		TimeLine.UpdateAsObservable()
			.Where( _ => !Audio.isPlaying && amount != TimeLine.fillAmount)
			.Subscribe( _ => {
				TimeLine.fillAmount = Mathf.Lerp(amount, TimeLine.fillAmount, 10.0f * Time.deltaTime);
		}).AddTo(this);

		this.UpdateAsObservable()
			.Where( _ => isRecordPlaying)
			.Subscribe( _ => {
				playTime.Value -= Time.deltaTime;
		});

		playTime.Where( value => value < 0)
			.Subscribe(value => {
				init();
		}).AddTo(this);

		slider.Amount
			.Where( _ => !Audio.isPlaying)
			.Subscribe(value => {
				slider.DragMode = true;
				amount = value;
				Audio.time = Audio.clip.length * value;
				playTime.Value = Audio.clip.length - Audio.time;
		}).AddTo(this);

		slider.Amount
			.Where( _ => Audio.isPlaying)
			.Subscribe(value => {
				slider.DragMode = false;
				Audio.Stop();
				amount = value;
				TimeLine.fillAmount = value;
				Audio.time = Audio.clip.length * value;
				playTime.Value = Audio.clip.length - Audio.time;
				Audio.Play();
		}).AddTo(this);
	}
	
	public void init()
	{
		Audio.Stop();
		Audio.time = 0.0f;
		amount = 0;
		TimeLine.fillAmount = 0;
		playTime.Value = Audio.clip.length;
		isRecordPlaying = false;
		PlayButton.sprite = PlayButtonImg[0];
		AudioStop.Invoke();
	}

	public void changeAudioClip(AudioClip clip)
	{
		Audio.clip = clip;
		init();
	}

	public void PlayAndPause()
	{
		if(isRecordPlaying)
		{
			isRecordPlaying = false;
			PlayButton.sprite = PlayButtonImg[0];
			Audio.Pause();
			AudioStop.Invoke();
		}
		else
		{
			isRecordPlaying = true;
			PlayButton.sprite = PlayButtonImg[1];
			Audio.Play();
			AudioStart.Invoke();
		}
	}
}
