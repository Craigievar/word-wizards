using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.TypeGames.TSBR
{
    public class LobbyUI : MonoBehaviour
    {

        [SerializeField]
        private Button settingsButton;
        [SerializeField]
        private Button libraryButton;
        [SerializeField]
        private Button characterButton;

        [SerializeField]
        private Text characterText;

        [SerializeField]
        private Transform mainCanvas;

        [SerializeField]
        private GameObject uiPrefab;

        private Character character;

        [SerializeField]
        private Transform characterSpawn;

        private bool characterExists = false;

        public void Start()
        {
        }

        public void Update()
        {
            if(!characterExists && LocalData.user.currentCharacterId != null)
            {
                character = LocalData
                    .characterSet
                    .GetCharacterById(LocalData.user.currentCharacterId);

                characterText.text = "You're playing as <b>the " + character.name + "</b>";

                Debug.Log("Instantiating character in menu");
                Instantiate(character.characterPrefab, characterSpawn);
                characterExists = true;

                LocalData.uiPrefab = this.uiPrefab;
                LocalData.SetUpInfoPane(mainCanvas);
            }
        }

        public void EnableNonBattleUI()
        {
            Debug.Log("Enabling UI");
            characterButton.interactable = true;
            settingsButton.interactable = true;
            libraryButton.interactable = true;
        }

        public void DisableNonBattleUI()
        {
            Debug.Log("Disabling UI");
            characterButton.interactable = false;
            settingsButton.interactable = false;
            libraryButton.interactable = false;
        }
    }
}