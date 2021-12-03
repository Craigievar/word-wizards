using UnityEngine;
using System.Collections;


namespace Com.TypeGames.TSBR
{
    public class AlertText : MonoBehaviour
    {
        //public Animator animator;
        //public AudioManager.SoundType sound;

        // should spawn, animate, then destroy itself
        // called by animation
        public void Remove()
        {
            Destroy(this.gameObject);
        }

        public void Start()
        {
            ////this.Animate();
            //Debug.Log("Fireball created");
            //if(this.sound != AudioManager.SoundType.None)
            //    LocalData.audioManager.Play(this.sound);
            Debug.Log("I exist");
        }

    }
}