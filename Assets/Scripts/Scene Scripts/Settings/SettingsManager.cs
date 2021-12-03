using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using UnityEngine.UIElements;


namespace Com.TypeGames.TSBR
{

    public class SettingsManager : MonoBehaviour
    {
        public GameObject confirmEmail;
        public Text authPaneText;

        public Slider master;
        public Slider music;
        public Slider effect;

        public InputField nameManager;
        public GameObject nameChangePane;

        private GameObject canvas;

        public TimeKeeper checkEmail { get; set; }

        private bool emailLastVerified = true;

        public GameObject popUpPrefab;

        public bool soundsEnabled = false;

        // Start is called before the first frame update
        void Start()
        {
            CheckForEmailVerification();


            checkEmail = new TimeKeeper(3000);
            confirmEmail.SetActive(false);

            canvas = GameObject.Find("Canvas");

            LocalData.SetUpInfoPane(canvas.transform);

            master.value = LocalData.audioManager.masterVolume;
            music.value = LocalData.audioManager.musicVolume;
            effect.value = LocalData.audioManager.effectVolume;

            soundsEnabled = true;



            HideNameChangePane();

            //Debug.Log(LocalData.authManager.auth.CurrentUser);
            if (LocalData.authManager.auth.CurrentUser == null)
            {
                authPaneText.text = "Login/Sign Up";
                confirmEmail.SetActive(false);
            }
            else
            {
                if (!LocalData.authManager.auth.CurrentUser.IsEmailVerified)
                {
                    emailLastVerified = false;
                    confirmEmail.SetActive(true);
                }

                authPaneText.text = "Log Out";
            }



        }


        public void ShowNameChangePane()
        {
            nameChangePane.SetActive(true);
        }

        public void HideNameChangePane()
        {
            nameChangePane.SetActive(false);
        }

        public void ShowVerifiedEmailPane()
        {
            GameObject pane = Instantiate(popUpPrefab, canvas.transform);
            pane.GetComponentInChildren<Text>().text = "Email Verified!";
        }

        public void SelectName()
        {
            if (nameManager.text.Length < 4 || nameManager.text.Length >= 12)
            {
                HandleBadNicknameLength();
            }
            else
            {
                //check if it's nasty
                LocalData.SetPlayerName(nameManager.text);
                LocalData.infoUI.Refresh();
                nameManager.text = "";
                HideNameChangePane();
            }
        }

        //todo dedupe with the version of this that exists elsewhere
        public void HandleBadNicknameLength()
        {
            Debug.Log("Todo: handle bad setup length");
        }

        // Update is called once per frame
        void Update()
        {

            CheckForEmailVerification();

        }

        void CheckForEmailVerification()
        {
            if (checkEmail == null || checkEmail.ShouldExecute)
            {
                if (LocalData.authManager.auth.CurrentUser != null)
                {
                    if (!LocalData.authManager.auth.CurrentUser.IsEmailVerified)
                    {
                        LocalData.authManager.auth.CurrentUser.ReloadAsync();
                    }
                    else if (!emailLastVerified)
                    {
                        emailLastVerified = true;
                        confirmEmail.SetActive(false);
                        ShowVerifiedEmailPane();
                    }
                }

                if(checkEmail!= null)
                    checkEmail.Reset();
            }
        }

        public void AdjustMasterVolume(float value)
        {
            if (!soundsEnabled)
                return;

            LocalData.audioManager.SetVolume(AudioManager.SoundCategory.master, value);
            LocalData.audioManager.Play(AudioManager.SoundType.button);
        }

        public void AdjustMusicVolume(float value)
        {
            if (!soundsEnabled)
                return;

            LocalData.audioManager.SetVolume(AudioManager.SoundCategory.music, value);
            LocalData.audioManager.Play(AudioManager.SoundType.button);
        }

        public void AdjustEffectVolume(float value)
        {
            if (!soundsEnabled)
                return;

            LocalData.audioManager.SetVolume(AudioManager.SoundCategory.effect, value);
            LocalData.audioManager.Play(AudioManager.SoundType.button);
        }

        public void ResendConfirmationEmailClicked()
        {
            LocalData.authManager.auth.CurrentUser.SendEmailVerificationAsync();
        }


        public void AuthButtonClicked()
        {
            if (LocalData.authManager.auth.CurrentUser != null)
            {
                Debug.Log("Logging out");
                DatabaseManager.OnLogout();
                LocalData.authManager.auth.SignOut();
                authPaneText.text = "Login/Sign Up";
                confirmEmail.SetActive(false);
            } else
            {
                Debug.Log("Going to auth screen");
                LocalData.returnScene = "Settings v2";
                SceneManager.LoadScene("Auth Management");
            }
        }

        public void TestDatabase()
        {
            //DatabaseManager.WriteUser("abcd", "1234");
            //DatabaseManager.OnUserModifiedWordList(1, WordListSet.GetWordListById(1));

            //DatabaseManager.OnUserAcquiredWord("Poop"); //todo make word data type
            //DatabaseManager.OnUserPurchasedWordList(5);
            //DatabaseManager.SetUserField("Money", "500");
            LocalData.user.setupDone = false;
            if (!LocalData.user.setupDone)
            {
                DatabaseManager.OnUserCreated();
                //DatabaseManager.IncrementUserField("money", 10);
                //DatabaseManager.OnUserAcquiredCharacter(1);
                //DatabaseManager.WriteUser();
                //DatabaseManager.PullUserData();
                //LocalData.user = new User();
                LocalData.user.setupDone = true;
                LocalData.user.PrintDebug();
            } else
            {
                //DatabaseManager.OnUserPurchasedWordList(WordListSet.GetWordListById(3));
                //DatabaseManager.OnUserAcquiredCharacter(10, 5);
                //DatabaseManager.OnUserAcquiredWord("test12345");
                ////DatabaseManager.OnUserPurchasedWordList(1, 500);
                //DatabaseManager.OnUserModifiedWordList(1, new WordList(1, "pooptest", "pooptest", new List<string> { "poop" }), 500);



                //DatabaseManager.ReadUserDataFromJson();
                //LocalData.user.PrintDebug();

            }

            
        }
    }
}