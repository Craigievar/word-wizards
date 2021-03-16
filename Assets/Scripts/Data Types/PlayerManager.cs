using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
using System.Linq;

using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Com.TypeGames.TSBR
{
    /// <summary>
    /// Player manager.
    /// Handles ingame player logic
    /// </summary>
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Private Fields
        
        #endregion

        #region Public Fields
        [Tooltip("The current word queue of our player")]
        public List<string> wordQueue = new List<string>();

        public List<int> killedPlayers = new List<int>();
        public int lastAttackerId;
        public int correctWords;
        public int wrongWords;
        public int timesAttacked;
        public int wordsFromGame;
        public List<string> wordList;

        public const int startingWords = 3;

        public TimeKeeper sendInterval { get; set; }
        public TimeKeeper masterHealthInterval { get; set; }
        public TimeKeeper debugInterval { get; set; }

        public GameManager gm;

        public GameObject characterModel;
        public Character character;

        public Player[] playerList;

        public float inGameTimer = 0.0f;


        

        System.Random random = new System.Random();

        public bool alive = true;

        #endregion

        #region Helper fns

        public void PrintStatus()
        {
            Debug.Log("Alive: " + alive + "\n" +
                "Game Over: " + gm.gameOver + "\n" +
                "Is Master: " + PhotonNetwork.IsMasterClient + "\n" +
                "Players in room: " + PhotonNetwork.PlayerList.Length + "\n" +
                "Players Alive: " + gm.CountLivingPlayers() + "\n" +
                "TimeKeeper active: " + sendInterval.IsEnabled
                );
        }

        #endregion

        #region Networking and Game Logic

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //if (stream.IsWriting)
            //{
            //    stream.SendNext(wordQueue.ToArray<String>());
            //    Debug.Log(wordQueue.ToArray<String>());
            //    Debug.Log("Sending " + String.Join(",", wordQueue));
            //    Debug.Log("Sending " + String.Join(",", wordQueue.ToArray<String>()));

            //}
            //else
            //{
            //    this.wordQueue = ((String[])stream.ReceiveNext()).ToList<String>();
            //    PrintWordList();
            //}
        }


        public override void OnPlayerLeftRoom(Player remotePlayer)
        {
            Debug.Log("A player left the room");
            if (gm.playerLookup.ContainsKey(remotePlayer.ActorNumber))
            {
                gm.playerLookup.Remove(remotePlayer.ActorNumber);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                CheckForGameOver();
            }
        }

        //void OnPhotonPlayerActivityChanged(Player otherPlayer)
        //{
        //    Debug.Log("activity changed");
        //}

        public void GenerateWordsForAllPlayers()
        {
            //TODO: use wordlists...
            Debug.Log("Wordlist " + wordList.ToString());
            if(this.wordList == null || this.wordList.Count == 0)
            {
                Debug.Log("Setting wordlist");
                if (this.photonView.Owner.CustomProperties.ContainsKey("wordList"))
                {
                    this.wordList = ((String[])this.photonView.Owner.CustomProperties["wordList"]).ToList<string>();
                    Debug.Log("Got wordlist from player " + photonView.OwnerActorNr +
                        String.Join(",", this.wordList));
                }
                else
                {
                    Debug.Log("Couldn't get custom wordlist for player...examine this");
                    this.wordList = WordListSet.wordListDict[0].words;//  LocalData.wordList.words;
                }

            }
            else
            {
                Debug.Log("wordlist already exists with length " + this.wordList.Count);
            }


            wordQueue.Add(this.wordList[random.Next(this.wordList.Count)]);
            Debug.Log("Word queue is" + String.Join(",", this.wordQueue));
           
            String[] toSend = wordQueue.ToArray<string>();
            Debug.Log("To send queue is" + String.Join(",", toSend));
            Debug.Log("To send obj is " + toSend);
            this.photonView.RPC(
                "AddWordFromMaster",
                RpcTarget.AllBuffered,
                toSend
            );

            CheckForDeath();
        }

        public void AddWord()
        {
            wordQueue.Add(wordList[random.Next(wordList.Count)]);
            String[] toSend = wordQueue.ToArray<string>();
            this.photonView.RPC(
                "AddWordFromClient",
                RpcTarget.AllBuffered,
                toSend
            );


            //called on global updates from master
            if (PhotonNetwork.IsMasterClient && alive)
            {
                CheckForDeath();
                if (photonView.IsMine)
                {
                    //safety check
                    CheckForGameOver();
                }
            }
        }

        public void AddWordFromPlayer(int attackerId) {
            PlayerManager attacker = gm.playerLookup[attackerId];
            Player attackerPlayer = attacker.photonView.Owner;
            string word;
            if (attacker != null && attacker.wordList != null && attacker.wordList.Count > 0)
            {
                word = attacker.wordList[random.Next(attacker.wordList.Count)];
            }
            else if (attackerPlayer != null && attackerPlayer.CustomProperties.ContainsKey("wordList"))
            {
                Debug.LogError("Don't have my attacker's word list yet, but can get");
                try
                {
                    attacker.wordList = ((String[])attackerPlayer.CustomProperties["wordList"]).ToList<string>();
                }
                catch (Exception e) {
                    Debug.Log("Wordlist error: " + e);
                }


                
                word = attacker.wordList[random.Next(attacker.wordList.Count)];
            }
            else
            {
                Debug.Log("Can't get wordlist, using my own");
                word = wordList[random.Next(wordList.Count)];
            }

            this.wordQueue.Add(word);
            String[] toSend = wordQueue.ToArray<string>();
            this.photonView.RPC(
                "AddWordFromClient",
                RpcTarget.AllBuffered,
                toSend
            );

        }


        public void CheckForDeath()
        {

            // should only be called on master client!
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("Called Check for Death on Non Master");
                return;
            }

            
            if (wordQueue.Count > gm.maxWordsInQueue && !gm.gameOver)
            {
                //if master client dies, reassign it...since most likely they'll
                // disconnect
                if (photonView.IsMine) {
                    int target = GetTarget();
                    if (gm.playerLookup.ContainsKey(target))
                    {
                        PhotonNetwork.SetMasterClient(gm.playerLookup[GetTarget()].photonView.Owner);
                    }
                    
                }

                //send death message
                this.photonView.RPC(
                    "PlayerDied",
                    RpcTarget.AllBuffered,
                    lastAttackerId > -1 ? lastAttackerId : -1
                );

                Debug.Log("Checking if game is over with " + gm.CountLivingPlayers());
                CheckForGameOver();

            }
        }

        public void CheckForGameOver()
        {
            if (gm.CountLivingPlayers() <= gm.playersToWin)
            {
                if (gm.FindLivingPlayerId() > 0)
                {

                    sendInterval.IsEnabled = false;

                    gm.playerLookup[gm.FindLivingPlayerId()].photonView.RPC(
                        "PlayerWon",
                        RpcTarget.AllBuffered
                    );
                    
                }

            }
        }

        public void WordWasCorrect() 
        {
            LocalData.wordsCorrect++;

            if (!photonView.IsMine || !alive)
            {
                //no idea how this would happen
                return;
            }

            Debug.Log("Word was correct");

            if(wordQueue.Count > 0)
            {
                wordQueue.RemoveAt(0);
            }

            Debug.Log("Sending my words first");
            String[] toSend = wordQueue.ToArray<string>();
            this.photonView.RPC(
                "UpdateWordsFromClient",
                RpcTarget.AllBuffered,
                toSend
            );

            int target = GetTarget();
            Debug.Log("Attacking " + target);
            if(gm.playerLookup.ContainsKey(target))
            {
                Debug.Log("sending rpc");
                gm.playerLookup[target].photonView.RPC(
                    "PlayerAttacked",
                    RpcTarget.AllBuffered,
                    this.photonView.OwnerActorNr,
                    target
                );

            }

            // animations
            character.Animate(Character.Anim.Attack);
            character.Attack();


        }

        public void WordWasIncorrect()
        {
            LocalData.wordsIncorrect++;

            if (!photonView.IsMine || !alive)
            {
                //no idea how this would happen
                return;
            }

            if (wordQueue.Count > 0)
            {
                wordQueue.RemoveAt(0);
            }

            AddWord();
            AddWord();

            character.Animate(Character.Anim.FailCast);
            LocalData.audioManager.Play(AudioManager.SoundType.playerAttacked);
        }

        public int GetTarget()
        {
            int target = -1;
            List<Player> options = PhotonNetwork.PlayerList.ToList<Player>();

            Debug.Log("Options: " + options.Count);

            options.Remove(photonView.Owner);
            Debug.Log("Options: " + options.Count);

            foreach(Player option in options)
            {
                if(!gm.playerLookup.ContainsKey(option.ActorNumber)
                    || !gm.playerLookup[option.ActorNumber].alive
                ){
                    options.Remove(option);
                }
            }

            Debug.Log("Options: " + options.Count);
            //gm.PrintLookup();

            if (options.Count > 0)
            {
                return options[random.Next(options.Count)].ActorNumber;
            }

            return -1;
        }

        public Player GetNewMaster()
        {
            List<Player> options = PhotonNetwork.PlayerList.ToList<Player>();
            //options.Remove(photonView.Owner);
            options.Remove(PhotonNetwork.MasterClient);
            options.Sort(SortPlayers);

            return options.Count > 0 ? options[0] : null;

        }

        public int SortPlayers(Player p1, Player p2)
        {
            int p1Score = p1.ActorNumber + (gm.playerLookup[p1.ActorNumber].alive ? 0 : 1000);
            int p2Score = p2.ActorNumber + (gm.playerLookup[p2.ActorNumber].alive ? 0 : 1000);
            return p1Score > p2Score ? 1 : -1;
        }

        //public Player GetPlay

        #endregion

        #region helper functions

        private void PrintWordList()
        {
            Debug.Log(photonView.IsMine.ToString() + " " + String.Join(",", this.wordQueue));
        }

        #endregion

        #region RPCs
        [PunRPC]
        void PlayerAttacked(int attacker, int target) {
            //eventually we want to route this thru the master client.
            //for now it's just whatever
            AddWordFromPlayer(attacker);
            lastAttackerId = attacker;


           
            if (photonView.IsMine)
            {
                LocalData.timesAttacked++;
                try
                {
                    LocalData.killerNickName = gm.playerLookup[attacker].photonView.Owner.NickName;
                }
                catch (Exception e)
                {
                    Debug.Log("Couldn't get attacker nickname");
                }

                character.Animate(Character.Anim.Attacked);
                LocalData.audioManager.Play(AudioManager.SoundType.playerAttacked);
            }
        }

        [PunRPC]
        void AddWordFromMaster(String[] newQueue)
        {
            Debug.Log("Got call to add words");
            Debug.Log("My words are: " + String.Join(",", newQueue));

            this.wordQueue = newQueue.ToList<string>();

            if (photonView.IsMine)
            {
                LocalData.wordsReceived++;
            }
        }

        [PunRPC]
        void AddWordFromMaster(String firstWord)
        {
            Debug.Log("Got call to add first word");
            Debug.Log("My word is: " + firstWord);

            this.wordQueue = new List<string>();
            this.wordQueue.Add(firstWord);

            if (photonView.IsMine)
            {
                LocalData.wordsReceived++;
            }
        }

        [PunRPC]
        void AddWordFromClient(String[] newQueue)
        {
            Debug.Log("Adding word from client (string[] newqueue)");
            this.wordQueue = newQueue.ToList<string>();

            if (PhotonNetwork.IsMasterClient)
            {
                CheckForDeath();
            }
        }

        [PunRPC]
        void AddWordFromClient(string firstWord)
        {
            try
            {
                Debug.Log("Adding word from client (string firstword)");
                this.wordQueue = new List<string>();
                this.wordQueue.Add(firstWord);
            }
            catch(Exception e)
            {
                Debug.Log("Error: " + e);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Checking for death");
                CheckForDeath();
            }

            Debug.Log("Made it through addword;");
            Debug.Log("wordqueue is " + String.Join(",", this.wordQueue));
        }

        [PunRPC]
        void UpdateWordsFromClient(String[] newQueue)
        {
            this.wordQueue = newQueue.ToList<string>();
        }

        [PunRPC]
        void UpdateWordsFromClient(string firstWord)
        {
            this.wordQueue = new List<string>();
            this.wordQueue.Add(firstWord);
        }

        [PunRPC]
        void UpdateWordsFromClient()
        {
            this.wordQueue = new List<string>();
        }

        [PunRPC]
        void PlayerDied(int killerId)
        {
            alive = false;
            if (gm.playerLookup.ContainsKey(killerId))
            {
                gm.playerLookup[killerId].killedPlayers.Add(this.photonView.OwnerActorNr);
            }

            //if I'm the killer...
            if (killerId == gm.localPlayerManager.photonView.OwnerActorNr)
            {
                LocalData.kills++;
            }
            
        }

        [PunRPC]
        void PlayerWon()
        {
            gm.gameOver = true;
            gm.DisableSends();


            //this is sent via the winner player's photonview
            if (photonView.AmOwner)
            {
                LocalData.amWinner = true;
            }

            if (PhotonNetwork.IsMasterClient && !gm.isLoading)
            {
                gm.isLoading = true;
                Debug.Log("Calling loadlevel");
                PhotonNetwork.LoadLevel("Post Game");
            }
        }

        [PunRPC]
        void MasterHealthPing()
        {
            inGameTimer = 0.0f;
        }

        #endregion

        #region Monobehavior callbacks
        void Awake()
        {
            
        }
        // Use this for initialization
        void Start()
        {

            
            if (photonView.IsMine)
            {
                //setup wordlist sh
                this.wordList = LocalData.wordList.words;
                Hashtable hash = new Hashtable();
                hash.Add("wordList", this.wordList.ToArray<String>());
                photonView.Owner.SetCustomProperties(hash);

                //prefab stuff
                GameObject spawn = GameObject.Find("Character Model Spawn");
                characterModel = Instantiate(
                    LocalData.character.characterPrefab,
                    spawn.transform
                );
                character = characterModel.GetComponent<Character>();


                if (photonView.Owner.IsMasterClient)
                {
                    for (int i = 0; i < startingWords; i++)
                        GenerateWordsForAllPlayers();
                }

            }
            
            this.debugInterval = new TimeKeeper(5000);


            


            gm = GameObject.Find("Game Manager").GetComponent<GameManager>();
            gm.RegisterPlayer(this);
            sendInterval = new TimeKeeper(2000);
            masterHealthInterval = new TimeKeeper(500);
            //eventually keep, and populate, from custom properties list
        }

        // Update is called once per frame
        void Update()
        {
            //this.wordQueue.Add("Poop");
            if (sendInterval.ShouldExecute) {
                if (PhotonNetwork.IsMasterClient && !gm.gameOver)
                {
                    Debug.Log("Triggering Words");
                    GenerateWordsForAllPlayers();
                    //this.photonView.RPC(
                    //    "AddWordFromMaster",
                    //    RpcTarget.AllBuffered,
                    //    msg
                    //);
                    sendInterval.Reset();
                }
            }

            if (masterHealthInterval.ShouldExecute) {
                if (PhotonNetwork.IsMasterClient)
                {
                    Debug.Log("Sending Health Ping");

                    this.photonView.RPC(
                        "MasterHealthPing",
                        RpcTarget.AllBuffered
                    );

                    //if (gm.gameOver)
                    //{
                    //    PhotonNetwork.LoadLevel("Post Game");
                    //}
                }

                masterHealthInterval.Reset();
            }

            if (debugInterval.ShouldExecute && photonView.IsMine)
            {
                Debug.Log("Should I print status?");
                if (gm.logDebug)
                {
                    PrintStatus();
                }

                debugInterval.Reset();
            }


            //TODO track active time since last updated
            //if too high, send a master switch

            inGameTimer += Time.deltaTime;
            if (inGameTimer >= gm.masterPingInterval && !gm.gameOver)
            {
                Debug.Log("Checking Master Health");
                Player newMaster = GetNewMaster();
                if (newMaster != null) {
                    PhotonNetwork.SetMasterClient(newMaster);
                }
                inGameTimer = 0.0f;
            }

            if (photonView.IsMine && alive)
            {
                LocalData.gameTimer += Time.deltaTime;
            }
            

        }

        #endregion
    }
}
