using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Com.TypeGames.TSBR
{
    public class Character : MonoBehaviour
    {
        public int id;
        public string name;
        public string description;
        public GameObject characterPrefab;
        public Sprite icon;

        public Animator animator;
        public GameObject projectile;
        public Dictionary<Anim, string> animDict;
        public int price = 5000;

        //public Character(int id, string name, string description)
        //{
        //    this.id = id;
        //    this.name = name;
        //    this.description = description;
        //}

        public enum Anim
        {
            Idle,
            Attack,
            FailCast,
            Attacked,
            Death
        }

        public void Awake()
        {
            animDict = new Dictionary<Anim, string>() {
                { Anim.Idle, "idle" },
                { Anim.Attack, "Attack" },
                { Anim.FailCast, "Bad Attack" },
                { Anim.Attacked, "Attacked" },
                { Anim.Death, "death" }
            };

        }

        public void Animate(Anim animation)
        {
            if (!animDict.ContainsKey(animation))
            {
                Debug.LogError("Unhandled Animation");
            }
            animator.SetTrigger(animDict[animation]);
        }

        public void Attack()
        {
            GameObject tmp = Instantiate(projectile, this.transform);
            tmp.transform.localPosition = new Vector3(100f, 0f, 0f);
            tmp.GetComponent<Projectile>().animator.SetTrigger("attack");
        }

        public void AttackMe(Transform attackerTransform)
        {
            GameObject tmp = Instantiate(projectile, attackerTransform);
            tmp.GetComponent<Projectile>().animator.SetTrigger("attackMe");
            tmp.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            tmp.transform.localRotation = Quaternion.Euler(0, 180, 0);
            tmp.GetComponent<Projectile>().attacker = this;
            
        }
    }
}