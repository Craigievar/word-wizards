using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Com.TypeGames.TSBR
{
    // This static class persists between scenes
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager trackingInstance;

        [SerializeField]
        public Sound[] sounds;

        [Serializable]
        public enum SoundType{
            None,
            theme,
            fireballAttack,
            playerAttacked,
            button
        }

        [Serializable]
        public enum SoundCategory
        {
            None,
            music,
            effect,
            master
        }

        public string masterVolumePrefKey = "masterVolume";
        public string musicVolumePrefKey = "musicVolume";
        public string effectVolumePrefKey = "effectVolume";

        public float masterVolume;
        public float musicVolume;
        public float effectVolume;

        private void Awake()
        {
            DontDestroyOnLoad(this);

            if (trackingInstance == null)
            {
                trackingInstance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            masterVolume = PlayerPrefs.HasKey(masterVolumePrefKey) ?
                PlayerPrefs.GetFloat(masterVolumePrefKey) : 1.0f;

            musicVolume = PlayerPrefs.HasKey(musicVolumePrefKey) ?
                PlayerPrefs.GetFloat(musicVolumePrefKey) : 1.0f;

            effectVolume = PlayerPrefs.HasKey(effectVolumePrefKey) ?
                PlayerPrefs.GetFloat(effectVolumePrefKey) : 1.0f;


            

            foreach (Sound s in sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;

                s.source.volume = s.relativeVolume;
            }

            UpdateVolume();

            Play(SoundType.theme);
        }

        public void Play(SoundType name)
        {
            Debug.Log("Playing " + name);
            Sound s = Array.Find(sounds, sound => sound.name == name);

            if(s == null) {
                Debug.Log("Couldn't find source with name + " + name);
            }

            s.source.Play();
            if (s.source.mute)
            {
                s.source.mute = false;
            }
        }

        public void KillAll(SoundType name)
        {
            Sound[] targets = Array.FindAll<Sound>(sounds, sound => sound.name == name);
            foreach(Sound s in targets)
            {
                s.source.mute = true;
            }

        }

        public void SetVolume(SoundCategory category, float value)
        {
            switch (category)
            {
                case SoundCategory.master:
                    masterVolume = value;
                    PlayerPrefs.SetFloat(masterVolumePrefKey, value);
                    break;
                case SoundCategory.music:
                    musicVolume = value;
                    PlayerPrefs.SetFloat(musicVolumePrefKey, value);
                    break;
                case SoundCategory.effect:
                    effectVolume = value;
                    PlayerPrefs.SetFloat(effectVolumePrefKey, value);
                    break;
            }

            UpdateVolume();

        }

        private void UpdateVolume()
        {
            foreach (Sound s in sounds)
            {
                s.source.volume = s.relativeVolume *
                    masterVolume *
                    (s.category == SoundCategory.music ? musicVolume : 1) *
                    (s.category == SoundCategory.effect ? effectVolume : 1);
            }
        }
    }
}