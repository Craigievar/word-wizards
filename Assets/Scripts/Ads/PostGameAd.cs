using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;
using Photon.Pun;


namespace Com.TypeGames.TSBR
{
    public class PostGameAd : MonoBehaviour
    {

        private string gameId = "4020412";
        private bool testMode = true;
        private string mySurfacingId = "Post_Game_iOS";
        private bool hasShown;

        // Start is called before the first frame update
        void Start()
        {


            Debug.Log("Initializing ads");
            if (!Advertisement.isInitialized)
            {
                Advertisement.Initialize(gameId, testMode);
            }

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ShowInterstitialAd()
        {

            if (Advertisement.IsReady() && !Advertisement.isShowing)
            {
                Debug.Log("Showing ad");
                hasShown = true;

                ShowOptions options = new ShowOptions
                {
                    resultCallback = HandleShowResult
                };


                Advertisement.Show(mySurfacingId, options);
            }
            else
            {
                Debug.Log("Interstitial Ad Not Ready, try again later");
                LeaveScene();
            }
        }

        private void HandleShowResult(ShowResult result)
        {
            Debug.Log("Closing ads out, finished");
            LeaveScene();
        }

        public void LeaveScene()
        {
            SceneManager.LoadScene("Menu");
        }

    }
}
