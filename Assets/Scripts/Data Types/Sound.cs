using UnityEngine.Audio;
using UnityEngine;


namespace Com.TypeGames.TSBR
{
    [System.Serializable]
    public class Sound
    {
        //public string name;
        public AudioClip clip;

        [SerializeField]
        public AudioManager.SoundType name;
        public AudioManager.SoundCategory category;

        public float relativeVolume;

        [HideInInspector]
        public AudioSource source;
    }
}