using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;
using Photon.Pun;


namespace Com.TypeGames.TSBR
{
    public class AdManager : MonoBehaviour, IUnityAdsListener
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

            Advertisement.AddListener(this);

            ShowInterstitialAd();

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void ShowInterstitialAd()
        {

            if (Advertisement.IsReady() && !Advertisement.isShowing)
            {
                Debug.Log("Showing ad");
                hasShown = true;
                Advertisement.Show();
            }
            else
            {
                Debug.Log("Interstitial Ad Not Ready, try again later");
            }
        }

        public void OnUnityAdsDidFinish(string surfacingId, ShowResult showResult)
        {
            // Define conditional logic for each ad completion status:
            //if (showResult == ShowResult.Finished)
            //{
            //    // Reward the user for watching the ad to completion.
            //}
            //else if (showResult == ShowResult.Skipped)
            //{
            //    // Do not reward the user for skipping the ad.
            //}
            //else if (showResult == ShowResult.Failed)
            //{
            //    Debug.LogWarning("The ad did not finish due to an error.");
            //}
            Debug.Log("Closing ads out, finished");
            Advertisement.RemoveListener(this);
            SceneManager.LoadScene("Menu");
        }

        public void OnUnityAdsReady(string surfacingId)
        {
            // If the ready Ad Unit or legacy Placement is rewarded, show the ad:
            Debug.Log(surfacingId);
            if (surfacingId.Equals(mySurfacingId) && !hasShown)
            {
                ShowInterstitialAd();
            }
        }

        public void OnUnityAdsDidError(string message)
        {
            // Log the error.
            Debug.Log("ads error: " + message);
            PhotonNetwork.AutomaticallySyncScene = false;
            SceneManager.LoadScene("Menu");
        }

        public void OnUnityAdsDidStart(string surfacingId)
        {
            // Optional actions to take when the end-users triggers an ad.
        }

        // When the object that subscribes to ad events is destroyed, remove the listener:
        public void OnDestroy()
        {
            Advertisement.RemoveListener(this);
        }

        public void LeaveScene()
        {
            SceneManager.LoadScene("Menu");
        }

    }
}
