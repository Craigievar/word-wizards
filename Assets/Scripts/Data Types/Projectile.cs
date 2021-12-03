using UnityEngine;
using System.Collections;


namespace Com.TypeGames.TSBR
{
    public class Projectile : MonoBehaviour
    {
        public Animator animator;
        public AudioManager.SoundType sound;
        public Character attacker;

        // should spawn, animate, then destroy itself
        // called by animation
        public void Remove()
        {
            if (attacker != null)
            {
                Destroy(attacker.gameObject);
            }
            Destroy(this.gameObject);

        }

        public void Start()
        {
            //this.Animate();
            Debug.Log("Fireball created");
            if(this.sound != AudioManager.SoundType.None)
                LocalData.audioManager.Play(this.sound);
        }

    }
}