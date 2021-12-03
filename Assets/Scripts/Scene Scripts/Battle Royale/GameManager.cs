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
        public GameObject botPrefab;
        public GameObject localPlayer;
        public PlayerManager localPlayerManager;

        [SerializeField]
        public Dictionary<int, PlayerManager> playerLookup = new Dictionary<int, PlayerManager>();
        public Dictionary<int, int> viewLookup = new Dictionary<int, int>();
        public Text wordCountField;
        public Text prompt;
        public Text playerLeftField;

        public bool logDebug = true;
        public float masterPingInterval = 5000.0f;
        public int maxWordsInQueue = 5;
        public int playersToWin = 1;

        public GameObject alertPrefab;
        public Transform alertSource;

        public Transform attackerSpawnLocation;
        public Transform attackerProjectileSpawnLocation;

        public bool gameOver = false;

        [SerializeField]
        public List<string> positions;

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
            SceneManager.LoadScene("Post Game");
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
            //Debug.Break();
            playerLookup.Add(caller.photonView.ViewID, caller);
            if (!caller.isBot)
                viewLookup.Add(caller.photonView.OwnerActorNr, caller.photonView.ViewID);
        }

        public int CountLivingPlayers(bool includeBots = true)
        {
            int tmp = 0;
            foreach(PlayerManager p in playerLookup.Values)
            {
                if (p.alive && (includeBots || (!p.isBot)))
                {
                    tmp++;
                }
            }
            Debug.Log(playerLookup.Count + " players total");
            Debug.Log(tmp + " players alive");
            //Debug.Break();
            return tmp;
        }

        public void DisableSends()
        {
            foreach (PlayerManager p in playerLookup.Values)
            {
                if(!p.isBot)
                    p.sendInterval.IsEnabled = false;
            }
        }

        public int GetViewByActor(int actorId)
        {
            return viewLookup.ContainsKey(actorId) ? viewLookup[actorId] : -1;
        }

        public PlayerManager GetPlayerManagerByActor(int actorId)
        {
            return GetViewByActor(actorId) > 0 ? playerLookup[GetViewByActor(actorId)] : null;
        }


        public int FindLivingPlayerId()
        {
            foreach (PlayerManager p in playerLookup.Values)
            {
                if (p != null && p.photonView != null && p.alive)
                {
                    return p.photonView.ViewID;
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

                if (PhotonNetwork.IsMasterClient)
                {
                    Debug.Log(LocalData.botCount + " bots");
                    for (int i = 0; i < LocalData.botCount; i++)
                        PhotonNetwork.Instantiate("Bot", new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                }
            }

            positions = new List<string>();
        }
        #endregion

        private void Update()
        {
            wordCountField.text = localPlayerManager.wordQueue.Count.ToString() + " Words To Type";

            if(localPlayerManager.wordQueue.Count > (maxWordsInQueue / 0.85))
            {
                wordCountField.color = Color.red;
            } else if (localPlayerManager.wordQueue.Count > (maxWordsInQueue / 0.50))
            {
                wordCountField.color = Color.yellow;
            } else
            {
                wordCountField.color = Color.green;
            }


            //Debug.Log(PhotonNetwork.PlayerList.Length);
            prompt.text = GetCurrentWord();
        }

        public void ShowAlert(string textForAlert)
        {
            GameObject alert = Instantiate(alertPrefab, alertSource);
            alert.GetComponent<Text>().text = textForAlert;
        }
    }
}