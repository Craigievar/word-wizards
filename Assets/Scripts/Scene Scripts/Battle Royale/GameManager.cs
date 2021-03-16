using System;
using System.Collections;
using System.Collections.Generic;


using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;


namespace Com.TypeGames.TSBR
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        [Tooltip("The prefab to use for representing a player")]
        public GameObject playerPrefab;
        public GameObject localPlayer;
        public PlayerManager localPlayerManager;
        public Dictionary<int, PlayerManager> playerLookup = new Dictionary<int, PlayerManager>();
        public Text wordCountField;
        public Text prompt;
        public bool logDebug = true;
        public float masterPingInterval = 5000.0f;
        public int maxWordsInQueue = 20;
        public int playersToWin = 1;

        public bool gameOver = false;

        //public bool amWinner;
        //public int wordsCorrect;
        //public int wordsIncorrect;
        //public int timesAttacked;
        //public int wordsReceived;
        //public float gameTimer = 0.0f;


        public bool isLoading = false;

        #region Photon Callbacks


        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }


        #endregion


        #region Public Methods

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void SpoofRightWord()
        {
            if (!localPlayerManager.photonView.IsMine)
            {
                return;
            }

            localPlayerManager.WordWasCorrect();

        }

        public void SpoofWrongWord()
        {
            if (!localPlayerManager.photonView.IsMine)
            {
                return;
            }

            localPlayerManager.WordWasIncorrect();
        }

        public void RegisterPlayer(PlayerManager caller)
        {
            playerLookup.Add(caller.photonView.OwnerActorNr, caller);
        }

        public int CountLivingPlayers()
        {
            int tmp = 0;
            foreach(PlayerManager p in playerLookup.Values)
            {
                if (p.alive)
                {
                    tmp++;
                }
            }
            Debug.Log(tmp + " players alive");
            return tmp;
        }

        public void DisableSends()
        {
            foreach (PlayerManager p in playerLookup.Values)
            {
                p.sendInterval.IsEnabled = false;
            }
        }


        public int FindLivingPlayerId()
        {
            foreach (PlayerManager p in playerLookup.Values)
            {
                if (p != null && p.photonView != null && p.alive)
                {
                    return p.photonView.OwnerActorNr;
                }
            }

            return -1;
        }

        public void PrintLookup()
        {
            Debug.Log("printing lookup table");
            foreach(int k in playerLookup.Keys)
            {
                Debug.Log(k + " " + playerLookup[k].alive);
            }
        }

        public string GetCurrentWord()
        {
            return localPlayerManager.wordQueue.Count > 0 ?
                localPlayerManager.wordQueue[0] :
                "";
        }


        #endregion

        #region MonoBehavior Callbacks

        void Start()
        {
            //LocalData localData = GameObject.Find("Local Data").GetComponent<LocalData>();
            LocalData.ResetGameStats();

            if(playerPrefab == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference", this);
            }
            else
            {
                Debug.Log("Instantiating local player");

                localPlayer = PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                localPlayerManager = localPlayer.GetComponent<PlayerManager>();
            }
        }
        #endregion

        private void Update()
        {
            wordCountField.text = localPlayerManager.wordQueue.Count.ToString();
            //Debug.Log(PhotonNetwork.PlayerList.Length);
            prompt.text = GetCurrentWord();
        }
    }
}