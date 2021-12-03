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

        public Text selectButtonText;
        public CharacterPurchaseConfirmationPane purchaseConfirmationPane;
        public Button exitButton;
        public Button selectButton;

        private GameObject currentCharacter;

        public void Start()
        {
            Hide();
            EnableUI();
            purchaseConfirmationPane.gameObject.SetActive(false);
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
            selectButtonText.text = button.owned ? "Select Character" : "Purchase Character";
        }

        public void OnSelectButtonPressed()
        {
            if (currentButton.owned)
            {
                Debug.Log("Character selected");
                buttonManager.CharacterSelected(this.currentButton);
                Hide();
            }
            else {
                purchaseConfirmationPane.SetValues(currentButton);
                DisableUI();
                purchaseConfirmationPane.gameObject.SetActive(true);
            }
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

        public void EnableUI()
        {
            exitButton.enabled = true;
            selectButton.enabled = true;
            this.gameObject.GetComponent<Image>().color = new Color32(0, 0, 0, 128);
        }

        public void DisableUI()
        {
            exitButton.enabled = false;
            selectButton.enabled = false;
            this.gameObject.GetComponent<Image>().color = new Color32(0, 0, 0, 190);
        }
    }
}