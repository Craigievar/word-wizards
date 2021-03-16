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

        public List<GameObject> currentWordsShown;

        public void RenderWordList(WordListButton button)
        {
            this.currentButton = button;
            WordList wl = button.wordList;

            title.text = wl.name;
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
            ClearWordList();
            Debug.Log("Word list selected");
            buttonManager.WordListSelected(this.currentButton);
            this.gameObject.SetActive(false);

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
    }
}