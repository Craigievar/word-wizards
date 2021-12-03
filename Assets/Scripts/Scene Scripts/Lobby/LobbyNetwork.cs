using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;


namespace Com.TypeGames.TSBR
{
    public class LobbyNetwork : MonoBehaviourPunCallbacks
    {
        #region Variables
        [Tooltip("The maximum number of players per room.")]
        [SerializeField]
        private byte maxPlayersPerRoom = 50;

        [SerializeField]
        private int minPlayersToStart = 5;

        [Tooltip("The connect button.")]
        [SerializeField]
        private GameObject connectButton;

        [SerializeField]
        string gameVersion = "1";

        private bool isConnecting = false;

        #endregion

        //[SerializeField]
        //private Button settingsButton;
        //[SerializeField]
        //private Button libraryButton;
        //[SerializeField]
        //private Button characterButton;
        [SerializeField]
        private LobbyUI lobbyUI;

        private bool isLoadingLevel = false;

        private int myBotCount = 0;
        private int roomBotCount = 0;
        private TimeKeeper addBotTimeKeeper;
        private TimeKeeper startBotsTimeKeeper;

        private int secondsBeforeBots = 10;

        public GameObject popupPrefab;
        private GameObject canvas;

        #region MonoBehavior Callbacks
        void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            WordListSet.Generate();
        }

        void Start()
        {
            canvas = GameObject.Find("Main Canvas");
            addBotTimeKeeper = new TimeKeeper(1000);
            startBotsTimeKeeper = new TimeKeeper(secondsBeforeBots * 1000);
            startBotsTimeKeeper.IsEnabled = false;
        }

        void Update()
        {
            //Debug.Log(PhotonNetwork.NickName);
            if (startBotsTimeKeeper.ShouldExecute && PhotonNetwork.CurrentRoom != null)
            {
                addBotTimeKeeper.IsEnabled = true;
                if (addBotTimeKeeper.ShouldExecute)
                {
                    myBotCount += 1;
                    addBotTimeKeeper.Reset();
                }

                roomBotCount = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("bots") ?
                    (int)PhotonNetwork.CurrentRoom.CustomProperties["bots"] :
                    0;

                if(myBotCount >= roomBotCount)
                {
                    Hashtable hash = new Hashtable();
                    roomBotCount = myBotCount;
                    hash.Add("bots", roomBotCount);
                    PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                }

                Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount + " players and " + roomBotCount + " bots");

                if (PhotonNetwork.IsMasterClient && ((PhotonNetwork.CurrentRoom.PlayerCount
                    + Mathf.Max(roomBotCount, myBotCount)) >= minPlayersToStart))
                {
                    LocalData.botCount = myBotCount;

                    PhotonNetwork.CurrentRoom.IsOpen = false;

                    if(!isLoadingLevel) {
                        isLoadingLevel = true;
                        PhotonNetwork.LoadLevel("Battle Royale");
                    }
                    
                }
            }

        }

        #endregion


        #region Public Methods


        public void OnConnectPressed()
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                //LocalData.audioManager.KillAll(AudioManager.SoundType.theme);
                Debug.Log("Ok, disconnecting");
                lobbyUI.EnableNonBattleUI();
                PhotonNetwork.Disconnect();
                connectButton.GetComponentInChildren<Text>().text = "Battle";
                return;
            }

            if (!isConnecting)
            {
                //LocalData.audioManager.Play(AudioManager.SoundType.theme);

                connectButton.GetComponentInChildren<Text>().text = "Connecting";
                isConnecting = true;
                lobbyUI.DisableNonBattleUI();

                if (PhotonNetwork.IsConnected)
                {
                    PhotonNetwork.JoinRandomRoom();
                }
                else
                {
                    PhotonNetwork.ConnectUsingSettings();
                    PhotonNetwork.GameVersion = gameVersion;
                }
                return;
            }

            if (isConnecting)
            {
                connectButton.GetComponentInChildren<Text>().text = "Battle";
                PhotonNetwork.Disconnect();
                lobbyUI.EnableNonBattleUI();
            }
        }

        #endregion


        #region MonoBehaviorPunCallbacks Callbacks

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to master");

            if (isConnecting)
            {
                PhotonNetwork.JoinRandomRoom();
                isConnecting = false;
            }

        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            connectButton.SetActive(true);
            lobbyUI.EnableNonBattleUI();
            connectButton.GetComponentInChildren<Text>().text = "Connect";
            //base.OnDisconnected(cause);
            Debug.LogWarningFormat("Disconnected for reason {0}", cause);

            if(cause != DisconnectCause.DisconnectByClientLogic)
            {
                GameObject pane = Instantiate(popupPrefab, canvas.transform);
                pane.GetComponentInChildren<PopUpButton>().SetText("Network Error. " +
                    "Please check your connection");

            }
            //LocalData.audioManager.KillAll(AudioManager.SoundType.theme);
            isConnecting = false;
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("Failed to join random room, creating one");
            PhotonNetwork.CreateRoom(null, new RoomOptions
            {
                MaxPlayers = maxPlayersPerRoom
            });
            //base.OnJoinRandomFailed(returnCode, message);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined Room");
            connectButton.GetComponentInChildren<Text>().text = "Waiting...";

            myBotCount = 0;
            addBotTimeKeeper.Reset();
            addBotTimeKeeper.IsEnabled = false;
            startBotsTimeKeeper.Reset();
            startBotsTimeKeeper.IsEnabled = true;

            //base.OnJoinedRoom();
            isConnecting = false;

            //if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            //{
            //    Debug.Log("We load 'Battle Royale'");

            //    // #Critical
            //    // Load the Room Level.
            //    //PhotonNetwork.LoadLevel("Battle Royale");
            //}
            //if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= minPlayersToStart)
            //{
            //    Debug.Log("Master client and enough players, loading");

            //    PhotonNetwork.LoadLevel("Battle Royale");
            //}

        }
        #endregion
        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

            startBotsTimeKeeper.Reset();

            if (PhotonNetwork.IsMasterClient && (PhotonNetwork.CurrentRoom.PlayerCount
                + Mathf.Max(roomBotCount, myBotCount)) >= minPlayersToStart)
            {
                Debug.Log("Master client and enough players, loading");
                LocalData.botCount = myBotCount;
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.LoadLevel("Battle Royale");
            }
        }

        //public void EnableNonBattleUI()
        //{
        //    Debug.Log("Enabling UI");
        //    characterButton.interactable = true;
        //    settingsButton.interactable = true;
        //    libraryButton.interactable = true;
        //}

        //public void DisableNonBattleUI()
        //{
        //    Debug.Log("Disabling UI");
        //    characterButton.interactable = false;
        //    settingsButton.interactable = false;
        //    libraryButton.interactable = false;
        //}

    }
}