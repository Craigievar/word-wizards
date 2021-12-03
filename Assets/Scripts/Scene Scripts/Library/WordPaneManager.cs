using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.TypeGames.TSBR
{
    public class WordPaneManager : MonoBehaviour
    {

        public WordListButtonManager buttonManager;
        public WordListButton currentButton;
        public GameObject contentPane;
        public GameObject wordPrefab;
        public Text title;
        public Text description;
        public Text selectButtonText;
        public PurchaseConfirmationPane purchaseConfirmationPane;
        public Button exitButton;
        public Button selectButton;

        public List<GameObject> currentWordsShown;

        private bool uiEnabled;

        void Start()
        {
            purchaseConfirmationPane.gameObject.SetActive(false);
        }

        public void RenderWordList(WordListButton button)
        {
            this.currentButton = button;
            WordList wl = button.wordList;


            title.text = button.owned ? wl.name : "Purchase " + wl.name + " for " + currentButton.wordList.price + "?";
            selectButtonText.text = button.owned ? "Select Spellbook" : "Purchase Spellbook";

            description.text = wl.description;

            for (int i = 0; i < wl.words.Count; i++)
            {
                GameObject wordDescription = Instantiate(
                    wordPrefab,
                    contentPane.transform
                );

                currentWordsShown.Add(wordDescription);
                //wordDescription.transform.localScale

                wordDescription.GetComponentInChildren<Text>().text = wl.words[i];
                wordDescription.transform.localPosition = new Vector3(0.0f, i * -110.0f);

            }
        }

        public void OnSelectButtonPressed()
        {
            if (this.currentButton.owned)
            {
                ClearWordList();
                Debug.Log("Word list selected");
                buttonManager.WordListSelected(this.currentButton);
                this.gameObject.SetActive(false);
            }
            else {
                purchaseConfirmationPane.SetValues(currentButton);
                DisableUI();
                purchaseConfirmationPane.gameObject.SetActive(true);
            }


        }

        public void OnExitPressed()
        {
            ClearWordList();
            this.gameObject.SetActive(false);
        }

        public void ClearWordList()
        {
            foreach(GameObject word in currentWordsShown)
            {
                //currentWordsShown.Remove(word);
                Destroy(word);
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