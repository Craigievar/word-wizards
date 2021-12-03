using System;
using UnityEngine;


namespace Com.TypeGames.TSBR
{

    public class WordListButton: MonoBehaviour
    {
        public WordList wordList;
        public WordListButtonManager manager;
        public bool owned;

        public void Start()
        {
            manager = GameObject.Find("Book Spawner").GetComponent<WordListButtonManager>();
            //Debug.Log("Subscribing myself");
            manager.Subscribe(this);
            manager.ResetButtons();
        }

        public void OnSelect()
        {
            manager.OnButtonUp(this);
        }

    }
}