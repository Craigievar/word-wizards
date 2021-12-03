using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Com.TypeGames.TSBR
{
    public class WordListButtonManager : MonoBehaviour
    {

        List<WordListButton> buttons;
        public WordListButton selectedButton;
        public GameObject bookPrefab;
        public WordPaneManager wordPane;
        public GameObject contentObject;
        public GameObject addWordListButtonPrefab;
        public Transform mainCanvas;


        // Start is called before the first frame update
        void Start()
        {
            WordListSet.Generate();
            GenerateButtons();
            wordPane.gameObject.SetActive(false);

            LocalData.SetUpInfoPane(mainCanvas);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Subscribe(WordListButton button)
        {
            if (buttons == null)
            {
                buttons = new List<WordListButton>();
            }

            buttons.Add(button);
        }

        public void OnButtonDown(WordListButton button)
        {
            if (selectedButton == null || button != selectedButton)
            {
                button.GetComponent<Image>().color = Color.blue;
            }
        }

        public void OnButtonUp(WordListButton button)
        {

            wordPane.gameObject.SetActive(true);

            wordPane.RenderWordList(button);

            //WordListSelected(button);
        }

        public void WordListSelected(WordListButton button)
        {
            selectedButton = button;
            LocalData.SetWordList(button.wordList.id);
            Debug.Log(LocalData.wordList.name + " selected");
            ResetButtons();
        }

        public void ResetButtons()
        {
            foreach (WordListButton button in buttons)
            {
                if (selectedButton != null && button == selectedButton)
                {
                    button.GetComponent<Image>().color = Color.green;
                }
                else
                {
                    button.owned = LocalData.user.wordlistsOwned.Contains(button.wordList.id);
                    if (button.owned)
                        button.GetComponent<Image>().color = Color.red;
                    else
                        button.GetComponent<Image>().color = Color.gray;
                }
            }
        }

        public void GenerateButtons()
        {
            //todo: add custom wordlists
            //Instantiate(
            //    addWordListButtonPrefab,
            //    contentObject.transform
            //);

            //List<WordList> wordLists = WordListSet.wordListDict.Values.ToList();
            List<WordList> wordLists = WordListSet.wordListDict.Values.Where(
                    wl => LocalData.user.wordlistsOwned.Contains(wl.id)
                ).ToList<WordList>();

            List<WordList> wordListsUnowned = WordListSet.wordListDict.Values.Where(
                wl => !LocalData.user.wordlistsOwned.Contains(wl.id)
                ).ToList<WordList>();

            wordLists.AddRange(LocalData.user.customWordLists);
            wordLists.AddRange(wordListsUnowned);

            wordLists.RemoveAt(wordLists.IndexOf(wordLists.Find(x => x.id == LocalData.wordList.id)));
            wordLists.Insert(0, LocalData.wordList);

            for (int i = 0; i < wordLists.Count; i++)
            {
                MakeButton(wordLists[i]);
            }


        }

        public void MakeButton(WordList wl)
        {
            GameObject button = Instantiate(
                bookPrefab,
                contentObject.transform
            );

            button.GetComponentInChildren<Text>().text = wl.name;
            button.GetComponent<WordListButton>().wordList = wl;
            button.GetComponent<WordListButton>().owned = LocalData.user
                .wordlistsOwned.Contains(wl.id);

            if (wl == LocalData.wordList) 
            {
                Debug.Log("Setting default wordlist");
                selectedButton = button.GetComponent<WordListButton>();
            }

        }


    }

}