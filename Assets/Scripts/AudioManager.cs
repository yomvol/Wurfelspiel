using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// play music in main menu, play several music tracks in game, play sound effects with event handling

public class AudioManager : PersistentSingleton<AudioManager>
{
	public AudioMixer Mixer;

	[SerializeField] private AudioMixerGroup _master;
	[SerializeField] private AudioMixerGroup _music;
	[SerializeField] private AudioMixerGroup _SFX;
	[SerializeField] private AudioMixerGroup _UI;
    [SerializeField] private Sound[] _sounds;
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _effectsSource;

    private List<Sound> _musicTracks;
	

	protected override void Awake()
	{
		base.Awake();

		_musicTracks = new List<Sound>();
		foreach (Sound s in _sounds)
		{
			if (s.MixerGroup != _music)
			{
                s.Source = _effectsSource;
            }
			else
			{
				s.Source = _musicSource;
				_musicTracks.Add(s);
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
            try
            {
                _musicSource.loop = true;
            }
            catch { return; }
            
            Play(_musicTracks[0]);
		}
		else if (activeSceneName == "Main")
		{
			try
			{
                _musicSource.loop = false;
            }
			catch { return; }
            
            Play(_musicTracks[UnityEngine.Random.Range(1, _musicTracks.Count)]);
			var sfx = _sounds.Where(s => s.MixerGroup == _SFX && s.PlayOnStart).ToArray();
			foreach (var effect in sfx) // consistent in-game SFX
			{
				Play(effect);
			}
		}
	}

    public void Play(string sound)
	{
		Sound s = Array.Find(_sounds, item => item.Name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		s.Source.clip = s.Clip;
		s.Source.loop = s.Loop;
		s.Source.volume = s.Volume * (1f + UnityEngine.Random.Range(-s.VolumeVariance / 2f, s.VolumeVariance / 2f));
		s.Source.pitch = s.Pitch * (1f + UnityEngine.Random.Range(-s.PitchVariance / 2f, s.PitchVariance / 2f));

		s.Source.Play();
        if (_musicTracks.Contains(s))
        {
            StartCoroutine(PickNextTrack(s.Clip.length, s.Name));
        }
    }

    public void Play(Sound s)
	{
		s.Source.clip = s.Clip;
		s.Source.loop = s.Loop;
        s.Source.volume = s.Volume * (1f + UnityEngine.Random.Range(-s.VolumeVariance / 2f, s.VolumeVariance / 2f));
        s.Source.pitch = s.Pitch * (1f + UnityEngine.Random.Range(-s.PitchVariance / 2f, s.PitchVariance / 2f));

        s.Source.Play();
		if (_musicTracks.Contains(s))
		{
            StartCoroutine(PickNextTrack(s.Clip.length, s.Name));
        }
    }

	public void PlayEffect(Sound s)
	{
        _effectsSource.outputAudioMixerGroup = s.MixerGroup;
        _effectsSource.volume = s.Volume * (1f + UnityEngine.Random.Range(-s.VolumeVariance / 2f, s.VolumeVariance / 2f));
        _effectsSource.pitch = s.Pitch * (1f + UnityEngine.Random.Range(-s.PitchVariance / 2f, s.PitchVariance / 2f));

		_effectsSource.PlayOneShot(s.Clip);
    }

	public void PlayEffect(string name)
	{
        Sound s = Array.Find(_sounds, item => item.Name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        _effectsSource.outputAudioMixerGroup = s.MixerGroup;
        _effectsSource.volume = s.Volume * (1f + UnityEngine.Random.Range(-s.VolumeVariance / 2f, s.VolumeVariance / 2f));
        _effectsSource.pitch = s.Pitch * (1f + UnityEngine.Random.Range(-s.PitchVariance / 2f, s.PitchVariance / 2f));

        _effectsSource.PlayOneShot(s.Clip);
    }

	private IEnumerator PickNextTrack(float trackLength, string prevName)
	{
		yield return new WaitForSeconds(trackLength);
		_musicSource.Stop();

		Sound s;
		int i = 0;
		do
		{
			s = _musicTracks[UnityEngine.Random.Range(1, _musicTracks.Count)];
			i++;

        } while (s.Name == prevName && i < 3);

        Play(s);
    }
}
