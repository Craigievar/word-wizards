using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.TypeGames.TSBR
{
    public class CharacterPaneManager : MonoBehaviour
    {
        public CharacterButtonManager buttonManager;
        public CharacterButton currentButton;
        //public GameObject contentPane;
        //public GameObject characterPrefab;
        public Text title;
        public Text description;
        public GameObject characterHolder;
        
        private GameObject currentCharacter;

        public void Start()
        {
            Hide();
        }

        public void RenderCharacter(CharacterButton button)
        {
            this.currentButton = button;
            Character character = button.character;

            currentCharacter = Instantiate(
                character.characterPrefab,
                characterHolder.transform
            );

            title.text = character.name;
            description.text = character.description;
        }

        public void OnSelectButtonPressed()
        {
            Debug.Log("Character selected");
            buttonManager.CharacterSelected(this.currentButton);
            Hide();
        }

        public void OnExitPressed()
        {
            Hide();
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
            if(currentCharacter != null)
            {
                Destroy(currentCharacter);
            }          
        }
    }
}