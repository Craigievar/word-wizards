using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com.TypeGames.TSBR
{
    public class CharacterSet: MonoBehaviour
    {
        //public GameObject[] characters;
        [SerializeField]
        public Character[] characters;

        public Character GetCharacterById(int id){
            foreach(Character c in characters){
                if(c.id == id){
                    return c;
                }
            }

            return null;
        }

    }
}