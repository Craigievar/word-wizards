using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;

namespace Com.TypeGames.TSBR
{
    // This static class persists between scenes
    public class LocalData : MonoBehaviour
    {

        public static CharacterSet characterSet;

        #region global variables
        public static WordList wordList;
        public static Character character;
        public static AudioManager audioManager;
        #endregion

        #region game instance variable
        public static bool amWinner;
        public static int wordsCorrect;
        public static int wordsIncorrect;
        public static int timesAttacked;
        public static int wordsReceived;
        public static int kills;
        public static float gameTimer = 0.0f;
        public static string killerNickName;
        #endregion

        #region player pref keys
        const string playerNamePrefKey = "PlayerName";
        const string wordListPrefKey = "WordListId";
        const string characterPrefKey = "CharacterId";
        #endregion

        #region setup management
        public static string returnScene;
        public static int setUpIndex;
        public static bool userHasAccount;
        public static AuthManager authManager;
        #endregion

        #region global methods

        public static void ResetGameStats()
        {
            Debug.Log("Resetting Local Data");
            amWinner = false;
            wordsCorrect = 0;
            wordsIncorrect = 0;
            timesAttacked = 0;
            wordsReceived = 0;
            gameTimer = 0.0f;
            kills = 0;
            killerNickName = "";
        }
        #endregion

        #region instantiation
        public static LocalData localData;

        public static LocalData trackingInstance;

        public static LocalData instance
        {
            get
            {
                if (!localData)
                {
                    localData = FindObjectOfType(typeof(LocalData)) as LocalData;
                    
                    if (!localData)
                    {
                        Debug.Log("There needs to be one active EventManger script on a GameObject in your scene.");
                    }
                    else
                    {
                        localData.Init();
                    }
                }

                return localData;
            }
        }

        void Init()
        {

        }
        #endregion

        #region Monobehavior Callbacks

        void Awake()
        {

            DontDestroyOnLoad(transform.gameObject);

            if(trackingInstance == null)
            {
                trackingInstance = this;
                //PlayerPrefs.DeleteAll();
                //var auth = FirebaseAuth.DefaultInstance;
                //auth.SignOut();
            } else {
                Destroy(gameObject);
            }


            
        }

        // Start is called before the first frame update
        void Start()
        {
            characterSet = this.gameObject.GetComponent<CharacterSet>();
            authManager = this.gameObject.GetComponent<AuthManager>();
            audioManager = GameObject.FindObjectOfType<AudioManager>();
            ResetGameStats();
            HandleNameInit();
            HandleWordListInit();
            HandleCharacterInit();
            //HandleNewPlayer(); //managed by name init
        }

        // Update is called once per frame
        void Update()
        {
            //Debug.Log("test local");
            //Debug.Log(WordListSet.wordListDict[0].description);
        }
        #endregion

        #region saved local data
        private void HandleNameInit()
        {

            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                SetPlayerName(PlayerPrefs.GetString(playerNamePrefKey));
            }
            else
            {
                //SceneManager.LoadScene("Settings");
                SceneManager.LoadScene("New Player");
            }

        }

        private void HandleWordListInit()
        {
            //Debug.Log("Getting wordlist");
            LocalData.wordList = PlayerPrefs.HasKey(wordListPrefKey) ?
                WordListSet.GetWordListById(PlayerPrefs.GetInt(wordListPrefKey)) :
                WordListSet.GetWordListById(0);
            Debug.Log("Wordlist is " + LocalData.wordList.name);
        }

        private void HandleCharacterInit()
        {
            //Debug.Log("Getting character");
            LocalData.character = PlayerPrefs.HasKey(characterPrefKey) ?
                characterSet.GetCharacterById(PlayerPrefs.GetInt(characterPrefKey)) :
                characterSet.GetCharacterById(0);
            Debug.Log("Character is " + LocalData.character.name);
        }

        public static void SetPlayerName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("Name is null or empty");

                //SceneManager.LoadScene("Setup");
                //return;
            }

            PhotonNetwork.NickName = value;
            PlayerPrefs.SetString(playerNamePrefKey, value);
            //todo: fix execution order, auth loads after this
            if(authManager.auth != null && authManager.auth.CurrentUser != null)
            {
                try
                {
                    authManager.UpdateUserProfileAsync(value);
                } catch(Exception e)
                {
                    Debug.Log("Couldn't update display name: " + e);
                }
            }
        }

        public static void SetWordList(int value)
        {
            PlayerPrefs.SetInt(wordListPrefKey, value);
        }

        public static void SetCharacter(int value)
        {
            PlayerPrefs.SetInt(characterPrefKey, value);
        }

        #endregion

    }

}