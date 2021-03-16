using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com.TypeGames.TSBR
{
    public class WordList
    {
        public int id;
        public string name;
        public string description;
        public List<string> words;

        public WordList(int id, string name, string description, List<string> words)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.words = words;
        }

    }
}