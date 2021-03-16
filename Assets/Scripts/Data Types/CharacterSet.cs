using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com.TypeGames.TSBR
{
    public class CharacterSet: MonoBehaviour
    {
        public GameObject[] characters;

        public Character GetCharacterById(int id){
            foreach(GameObject g in characters){
                if(g.GetComponent<Character>().id == id){
                    return g.GetComponent<Character>();
                }
            }

            return null;
        }

    }
}