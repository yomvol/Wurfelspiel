using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : PersistentSingleton<AudioManager>
{
	public AudioMixerGroup Master;
	public AudioMixerGroup Music;
	public AudioMixerGroup SFX;
	public AudioMixerGroup Dialog;
	public Sound[] Sounds;

	private AudioSource _musicSource;

	protected override void Awake()
	{
		base.Awake();
		
		_musicSource= GetComponent<AudioSource>();
		foreach (Sound s in Sounds)
		{
			if (s.MixerGroup != Music)
			{
                s.Source = gameObject.AddComponent<AudioSource>();
                s.Source.clip = s.Clip;
                s.Source.loop = s.Clip;
                s.Source.outputAudioMixerGroup = s.MixerGroup;
            }
			else
			{
				s.Source = _musicSource;
			}
			
		}

		SceneManager.sceneLoaded += OnSceneLoaded;
	}

    private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
		string activeSceneName = SceneManager.GetActiveScene().name;

		if (activeSceneName == "Menu")
		{
			Play(Sounds[0].Name);
		}
		else if (activeSceneName == "Main")
		{
			Play(Sounds[1].Name);
		}
	}

    public void Play(string sound)
	{
		Sound s = Array.Find(Sounds, item => item.Name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		s.Source.volume = s.Volume * (1f + UnityEngine.Random.Range(-s.VolumeVariance / 2f, s.VolumeVariance / 2f));
		s.Source.pitch = s.Pitch * (1f + UnityEngine.Random.Range(-s.PitchVariance / 2f, s.PitchVariance / 2f));

		s.Source.Play();
	}
}
