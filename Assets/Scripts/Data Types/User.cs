using System;
using System.Collections.Generic;
using UnityEngine;



namespace Com.TypeGames.TSBR
{
    public class User
    {
        public string name;

        public int currentCharacterId;
        public int currentWordListId;

        public int money;
        public int kills;
        public int wordsCorrect;
        public int wordsIncorrect;
        public int losses;
        public int wins;
        public int inGameTime;
        public string updateHash;

        public List<string> words;
        public List<int> charactersOwned;
        public List<int> wordlistsOwned;
        public List<WordList> customWordLists;

        public bool setupDone = false;


        // logged out user values
        public User()
        {
            this.currentCharacterId = 0;
            this.currentWordListId = 0;

            this.money = 0;
            this.wordlistsOwned = new List<int>
            {
                0,
                1
            };

            this.charactersOwned = new List<int>
            {
                0,
                1
            };

            this.customWordLists = new List<WordList>();
        }

        // only instantiated on first setup
        public void DataSetup()
        {
            //name should be set...
            this.money = 10000;
            this.kills = 0;
            this.wordsCorrect = 0;
            this.wordsIncorrect = 0;
            this.losses = 0;
            this.wins = 0;
            this.inGameTime = 0;

            this.words = new List<string>
            {
                "default",
                "words",
                "go",
                "here",
            };

            this.charactersOwned = new List<int>
            {
                0,
                1
            };

            this.wordlistsOwned = new List<int>
            {
                0,
                1
            };

            this.customWordLists = new List<WordList>();

            this.setupDone = true;

        }

        public void PrintDebug()
        {
            Debug.Log(JsonUtility.ToJson(this));
        }
    }
}
