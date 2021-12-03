using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Com.TypeGames.TSBR
{


    public class CharacterButtonManager : MonoBehaviour
    {

        public List<CharacterButton> buttons;
        public CharacterButton selectedButton;
        public GameObject characterButtonPrefab;
        public GameObject contentObject;
        public CharacterPaneManager characterPane;
        public Transform mainCanvas;

        // Start is called before the first frame update
        void Start()
        {
            LocalData.SetUpInfoPane(mainCanvas);

            GenerateButtons();
            //wordPane.gameObject.SetActive(false);

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Subscribe(CharacterButton button)
        {
            if (buttons == null)
            {
                buttons = new List<CharacterButton>();
            }

            buttons.Add(button);
        }

        public void OnButtonDown(CharacterButton button)
        {
            if (selectedButton == null || button != selectedButton)
            {
                button.SetIndicator(Color.blue);
            }
        }

        public void OnButtonUp(CharacterButton button)
        {

            characterPane.gameObject.SetActive(true);
            characterPane.RenderCharacter(button);

            //CharacterSelected(button);
        }

        public void CharacterSelected(CharacterButton button)
        {
            selectedButton = button;
            LocalData.SetCharacter(button.character.id);
            Debug.Log(LocalData.character.name + " selected");


            ResetButtons();
        }

        public void ResetButtons()
        {
            foreach (CharacterButton button in buttons)
            {
                if (selectedButton != null && button == selectedButton)
                {
                    button.SetIndicator(Color.green);
                }
                else
                {
                    button.owned = LocalData.user.charactersOwned.Contains(button.character.id);
                    if (button.owned)
                        button.SetIndicator(Color.red);
                    else
                        button.SetIndicator(Color.gray);
                    
                }
            }
        }

        public void GenerateButtons()
        {
            List<Character> characters = LocalData.characterSet.characters.ToList<Character>().Where(x => LocalData.user.charactersOwned.Contains(x.id)).ToList<Character>();
            List<Character> unowned = LocalData.characterSet.characters.ToList<Character>().Where(x => !LocalData.user.charactersOwned.Contains(x.id)).ToList<Character>();
            characters.Remove(LocalData.characterSet.GetCharacterById(LocalData.character.id));
            characters.Insert(0, LocalData.character);
            characters.AddRange(unowned);


            
            for (int i = 0; i < characters.Count; i++)
            {
                MakeButton(characters[i].GetComponent<Character>());
            }

            Debug.Log(characters[0].GetComponent<Character>() == characters[1].GetComponent<Character>());
        }

        public void MakeButton(Character character)
        {
            GameObject button = Instantiate(
                characterButtonPrefab,
                contentObject.transform
            );

            button.GetComponentInChildren<Text>().text = character.name;
            //button.GetComponent<CharacterButton>().character = character;
            button.GetComponent<CharacterButton>().SetUp(character, character.icon);
            button.GetComponent<CharacterButton>().owned = LocalData.user.charactersOwned.Contains(character.id);
            //button.GetComponent<Image>().sprite = character.icon;

            if (character == LocalData.character)
            {
                Debug.Log("Setting default character");
                selectedButton = button.GetComponent<CharacterButton>();
            }

        }
    }

}