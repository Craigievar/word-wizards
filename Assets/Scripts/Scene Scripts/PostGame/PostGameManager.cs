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
        public Transform mainCanvas;
        // Start is called before the first frame update
        void Start()
        {
            PhotonNetwork.Disconnect();

            DatabaseManager.SendMyPostMatchData();

            postGameStatText.text = (LocalData.amWinner ? "You won!" : "You placed " + AddNumberEnd(LocalData.playersLeft + 1))  + "\n\n" +
                "Words Correct: " + LocalData.wordsCorrect.ToString() + "\n\n" +
                "WPM: " + string.Format("{0:0.##}", (60 * LocalData.wordsCorrect / LocalData.gameTimer)) + "\n\n" +
                "Accuracy: " + string.Format("{0:0.00}", ((LocalData.wordsCorrect + LocalData.wordsIncorrect) > 0 ? ((100.0f * LocalData.wordsCorrect) / (LocalData.wordsCorrect + LocalData.wordsIncorrect)) : 0)) + "%\n\n" +
                "Kills: " + LocalData.kills.ToString() + "\n\n" +
                (LocalData.killerNickName == "" || LocalData.amWinner ? "" : "Killed by: " + LocalData.killerNickName);


            LocalData.SetUpInfoPane(mainCanvas);

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

        public string AddNumberEnd(int number) 
        {
            if(number % 10 == 1)
                return number + "st";
            if (number % 10 == 2)
                return number + "nd";
            if (number % 10 == 3)
                return number + "rd";
            else
                return number + "th";
        }
    }
}