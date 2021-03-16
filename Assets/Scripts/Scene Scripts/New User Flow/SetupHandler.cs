using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.TypeGames.TSBR
{
    public class SetupHandler : MonoBehaviour
    {
        public GameObject[] panels;
        public int index;


        // Start is called before the first frame update
        public void Start()
        {
            SwitchToPanel(index);
            if (LocalData.setUpIndex == null)
            {
                //Debug.Log("Setting setupindex = 0");
                LocalData.setUpIndex = 0;
            }
            else
            {
                //Debug.Log("saved index: " + LocalData.setUpIndex);
                index = LocalData.setUpIndex;
                SwitchToPanel(index);
            }
        }

        public void NextStep()
        {
            Debug.Log("Continue called");
            LocalData.audioManager.Play(AudioManager.SoundType.button);
            index++;
            SwitchToPanel(index);
        }

        public void SwitchToPanel(int index)
        {
            if(index >= panels.Length)
            {
                Debug.Log("index higher than  length");
                SceneManager.LoadScene("Menu");
            }

            for(int i = 0; i< panels.Length; i++)
            {
                panels[i].SetActive(i == index);
            }
        }

        public void OnNameUpdated(InputField input)
        {
            Debug.Log("Name updated:  " + input.text);

            if(input.text.Length < 4 || input.text.Length >= 12)
            {
                HandleBadNicknameLength();
            } else
            {
                //check if it's nasty
                LocalData.SetPlayerName(input.text);
                NextStep();
            }

        }

        public void AuthClicked(bool hasAccount)
        {
            LocalData.userHasAccount = hasAccount;
            LocalData.returnScene = "New Player";
            LocalData.setUpIndex = index;
            SceneManager.LoadScene("Auth Management");
        }

        public void Update()
        {
        }

        public void HandleBadNicknameLength() {
            Debug.Log("Todo: handle bad setup length");
        }

    }

    

}