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
	[SerializeField] private AudioMixerGroup _master;
	[SerializeField] private AudioMixerGroup _music;
	[SerializeField] private AudioMixerGroup _SFX;
	[SerializeField] private AudioMixerGroup _dialog;
    [SerializeField] private Sound[] _sounds;
	
	private List<Sound> _musicTracks;
	private AudioSource _musicSource;

	protected override void Awake()
	{
		base.Awake();

		_musicTracks = new List<Sound>();
		_musicSource= GetComponent<AudioSource>();
		foreach (Sound s in _sounds)
		{
			if (s.MixerGroup != _music)
			{
                s.Source = gameObject.AddComponent<AudioSource>();
                s.Source.clip = s.Clip;
                s.Source.loop = s.Clip;
                s.Source.outputAudioMixerGroup = s.MixerGroup;
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
            _musicSource.loop = true;
            Play(_musicTracks[0]);
		}
		else if (activeSceneName == "Main")
		{
            _musicSource.loop = false;
            Play(_musicTracks[UnityEngine.Random.Range(1, _musicTracks.Count)]);
			var sfx = _sounds.Where(s => s.MixerGroup != _music && s.PlayOnStart).ToArray();
			foreach (var effect in sfx)
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
