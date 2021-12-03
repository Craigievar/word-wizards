using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Com.TypeGames.TSBR
{
    [Serializable]
    public class WordList
    {
        public int id;
        public string name;
        public string description;
        public List<string> words;
        public int price;

        public WordList(int id, string name, string description, List<string> words, int price=500)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.words = words;
            this.price = price;
        }

    }
}