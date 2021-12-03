using System;
using UnityEngine;
using UnityEngine.UI;

namespace Com.TypeGames.TSBR
{
    public class CharacterButton: MonoBehaviour
    {
        public Character character;
        public CharacterButtonManager manager;
        public Image selectionIndicator;
        public Image icon;
        public bool owned;

        public void Start()
        {
            manager = GameObject.Find("Character Spawner").GetComponent<CharacterButtonManager>();
            Debug.Log("Subscribing myself");
            manager.Subscribe(this);
            manager.ResetButtons();
        }

        public void OnSelect()
        {
            manager.OnButtonUp(this);
        }

        public void SetIndicator(Color color)
        {
            selectionIndicator.color = color;
        }

        public void SetUp(Character setCharacter, Sprite setSprite)
        {
            this.character = setCharacter;
            this.icon.sprite = setSprite;
        }
    }
}
