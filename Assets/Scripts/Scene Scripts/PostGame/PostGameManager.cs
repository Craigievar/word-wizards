using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace Com.TypeGames.TSBR
{
    public class PostGameManager : MonoBehaviour
    {
        public Text postGameStatText;
        // Start is called before the first frame update
        void Start()
        {
            PhotonNetwork.Disconnect();
            postGameStatText.text = (LocalData.amWinner ? "You won!" : "You Lost")  + "\n" +
                "Words Correct: " + LocalData.wordsCorrect.ToString() + "\n" +
                "WPM: " + (60 * LocalData.wordsCorrect / LocalData.gameTimer).ToString() + "\n" +
                "Accuracy: " + ((LocalData.wordsCorrect + LocalData.wordsIncorrect) > 0 ? ((1.0 * LocalData.wordsCorrect) / (LocalData.wordsCorrect + LocalData.wordsIncorrect)).ToString() : "") + "\n" +
                "Kills: " + LocalData.kills.ToString() + "\n" +
                (LocalData.killerNickName == "" ? "" : "Killed by: " + LocalData.killerNickName);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ContinueButtonPressed()
        {
            Debug.Log("Pressed continue");
            PhotonNetwork.AutomaticallySyncScene = false;
            SceneManager.LoadScene("Postgame Ad");
        }
    }
}