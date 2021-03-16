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

        public GameObject verifiedEmailPane;

        public TimeKeeper checkEmail { get; set; }

        private bool emailLastVerified = true;

        // Start is called before the first frame update
        void Start()
        {
            confirmEmail.SetActive(false);

            master.value = LocalData.audioManager.masterVolume;
            music.value = LocalData.audioManager.musicVolume;
            effect.value = LocalData.audioManager.effectVolume;

            checkEmail = new TimeKeeper(3000);

            HideNameChangePane();
            HideVerifiedEmailPane();

            //Debug.Log(LocalData.authManager.auth.CurrentUser);
            if (LocalData.authManager.auth.CurrentUser == null)
            {
                authPaneText.text = "Login/Sign Up";
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
            verifiedEmailPane.SetActive(true);
        }

        public void HideVerifiedEmailPane()
        {
            verifiedEmailPane.SetActive(false);
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


            if (checkEmail.ShouldExecute)
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

                checkEmail.Reset();
            }
        }

        public void AdjustMasterVolume(float value)
        {
            LocalData.audioManager.SetVolume(AudioManager.SoundCategory.master, value);
            LocalData.audioManager.Play(AudioManager.SoundType.button);
        }

        public void AdjustMusicVolume(float value)
        {
            LocalData.audioManager.SetVolume(AudioManager.SoundCategory.music, value);
            LocalData.audioManager.Play(AudioManager.SoundType.button);
        }

        public void AdjustEffectVolume(float value)
        {
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
                LocalData.authManager.auth.SignOut();
                authPaneText.text = "Login/Sign Up";
            } else
            {
                Debug.Log("Going to autht screen");
                LocalData.returnScene = "Settings v2";
                SceneManager.LoadScene("Auth Management");
            }
        }

        public void TestDatabase()
        {
            DatabaseManager.WriteUser("abcd", "1234");
            DatabaseManager.OnUserModifiedWordList(1, WordListSet.GetWordListById(1));
            DatabaseManager.OnUserAcquiredCharacter(1);
            DatabaseManager.OnUserAcquiredWord("Poop"); //todo make word data type
            DatabaseManager.OnUserPurchasedWordList(5);
            DatabaseManager.SetUserField("Money", "500");
        }
    }
}