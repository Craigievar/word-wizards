using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

namespace Com.TypeGames.TSBR
{
    public class LobbyNetwork : MonoBehaviourPunCallbacks
    {
        #region Variables
        [Tooltip("The maximum number of players per room.")]
        [SerializeField]
        private byte maxPlayersPerRoom = 50;

        [SerializeField]
        private byte minPlayersToStart = 2;

        [Tooltip("The connect button.")]
        [SerializeField]
        private GameObject connectButton;

        [SerializeField]
        string gameVersion = "1";

        private bool isConnecting = false;

        #endregion

        #region MonoBehavior Callbacks
        void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            WordListSet.Generate();
        }

        void Start()
        {

        }

        void Update()
        {
            //Debug.Log(PhotonNetwork.NickName);
        }

        #endregion


        #region Public Methods


        public void OnConnectPressed()
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                //LocalData.audioManager.KillAll(AudioManager.SoundType.theme);
                Debug.Log("Ok, disconnecting");
                PhotonNetwork.Disconnect();
                connectButton.GetComponentInChildren<Text>().text = "Connect";
                return;
            }

            if (!isConnecting)
            {
                //LocalData.audioManager.Play(AudioManager.SoundType.theme);

                connectButton.GetComponentInChildren<Text>().text = "Connecting";
                isConnecting = true;
                if (PhotonNetwork.IsConnected)
                {
                    PhotonNetwork.JoinRandomRoom();
                }
                else
                {
                    PhotonNetwork.ConnectUsingSettings();
                    PhotonNetwork.GameVersion = gameVersion;
                }
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
            connectButton.GetComponentInChildren<Text>().text = "Connect";
            //base.OnDisconnected(cause);
            Debug.LogWarningFormat("Disconnected for reason {0}", cause);
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
            //base.OnJoinedRoom();
            isConnecting = false;

            //if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
            //{
            //    Debug.Log("We load 'Battle Royale'");

            //    // #Critical
            //    // Load the Room Level.
            //    //PhotonNetwork.LoadLevel("Battle Royale");
            //}
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= minPlayersToStart)
            {
                Debug.Log("Master client and enough players, loading");

                PhotonNetwork.LoadLevel("Battle Royale");
            }
        }
        #endregion
        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting


            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= minPlayersToStart)
            {
                Debug.Log("Master client and enough players, loading");

                PhotonNetwork.LoadLevel("Battle Royale");
            }
        }



    }
}