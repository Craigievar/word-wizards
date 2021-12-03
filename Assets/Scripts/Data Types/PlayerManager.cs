using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

        // note: eventually I might want to switch back
        // to tracking all player stats on every client for master
        // hopping. This has the ?advantage? of making it so that
        // we can do all db updates from master
        // for now, doing it from each client lol.
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

        public string firebaseUserId;

        public bool isBot;



        #endregion

        #region Helper fns

        public void PrintStatus()
        {
            if (isBot)
                return;

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
            if(gm.GetPlayerManagerByActor(remotePlayer.ActorNumber) != null)
            {
                if(!gm.positions.Contains(gm.GetPlayerManagerByActor(remotePlayer.ActorNumber).firebaseUserId))
                    gm.positions.Add(gm.GetPlayerManagerByActor(remotePlayer.ActorNumber).firebaseUserId);

                gm.GetPlayerManagerByActor(remotePlayer.ActorNumber).alive = false;
                gm.playerLeftField.text = gm.CountLivingPlayers() + " Players Alive";
                //gm.playerLookup.Remove(gm.GetViewByActor(remotePlayer.ActorNumber));
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
            //Debug.Log("Word queue is" + String.Join(",", this.wordQueue));
           
            String[] toSend = wordQueue.ToArray<string>();
            //Debug.Log("To send queue is" + String.Join(",", toSend));
            //Debug.Log("To send obj is " + toSend);
            this.photonView.RPC(
                "AddWordFromMaster",
                RpcTarget.AllBuffered,
                toSend
            );

            CheckForDeath();
        }

        public void AddWord()
        {
            //Debug.Log("Above wq");
            wordQueue.Add(wordList[random.Next(wordList.Count)]);
            String[] toSend = wordQueue.ToArray<string>();

            //Debug.Log("Above rpc1");
            this.photonView.RPC(
                "AddWordFromClient",
                RpcTarget.AllBuffered,
                toSend
            );

            //Debug.Log("Above mc check");
            //called on global updates from master
            if (PhotonNetwork.IsMasterClient && alive)
            {
                //Debug.Log("Above cfd");

                CheckForDeath();
                if (photonView.IsMine)
                {
                    //safety check
                    //Debug.Log("Above cfgo");
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
            else if (attackerPlayer != null && attackerPlayer.CustomProperties.ContainsKey("wordList") && !attacker.isBot)
            {
                Debug.LogError("Don't have my attacker's word list yet, but can get");
                try
                {
                    //Debug.Log("Attacker ? " + attacker == null);
                    //Debug.Log("attacker wl :" + String.Join(",", ((String[])attackerPlayer.CustomProperties["wordList"]).ToList<string>()));
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
                Debug.Log("Player died!!!");

                //send death message
                this.photonView.RPC(
                    "PlayerDied",
                    RpcTarget.AllBuffered,
                    lastAttackerId > -1 ? lastAttackerId : -1
                );

                Debug.Log("Checking if game is over with " + gm.CountLivingPlayers());

                CheckForGameOver();
                

                //if master client dies, reassign it...since most likely they'll
                //disconnect
                if (photonView.IsMine && !gm.gameOver)
                {
                    int target = GetTarget(includeBots: false);
                    if (gm.playerLookup.ContainsKey(target))
                    {
                        PhotonNetwork.SetMasterClient(gm.playerLookup[target].photonView.Owner);
                    }

                }

            }

        }

        public void CheckForGameOver()
        {
            Debug.Log("In checkforgameover");
            if(gm.playerLookup.Count < 2)
            {
                Debug.Log("Can't check for game over before 2 players load lol");
                Debug.Log(gm.playerLookup.Count);
                return;
            }
            //only called on MASTER
            //Debug.Log("above first if");
            //if 1 living player incl. bots, or no living non-bot players
            if (gm.CountLivingPlayers() <= gm.playersToWin || gm.CountLivingPlayers(includeBots: false) == 0)
            {
                if (!isBot)
                    sendInterval.IsEnabled = false;


                //first case 
                if (gm.CountLivingPlayers() <= gm.playersToWin)
                {
                    if(!(gm.FindLivingPlayerId() > 0))
                    {
                        Debug.Log("There's " + gm.CountLivingPlayers() + "players, but no living human player");
                        return;
                    }

                    //bots don't have a send interval
                    Debug.Log("won rpc players");

                    WriteGameStats(false);
                    //todo: handle players losing to bots lol
                    gm.playerLookup[gm.FindLivingPlayerId()].photonView.RPC(
                        "PlayerWon",
                        RpcTarget.AllBuffered,
                        false //human victory flag
                    );
                }
                else if (gm.CountLivingPlayers(includeBots: false) == 0)
                {
                    Debug.Log("The distant future...the year 2000...the humans are dead");

                    //master is only client guaranteed to be alive, so it'll
                    //send the data
                    WriteGameStats(true);

                    this.photonView.RPC(
                        "PlayerWon",
                        RpcTarget.AllBuffered,
                        true //botvictory flag
                    );

                   
                }

            }
        }

        public void WriteGameStats(bool botWin)
        {
            //called from master only
            Debug.Log("Running db calls for postgame");
            if (botWin)
            {
                int botsLeft = gm.CountLivingPlayers();
                Debug.Log(botsLeft + " Bot winners");
                for (int i = 0; i < botsLeft; i++)
                    gm.positions.Add("botWinner");
                    
            }
            else
            {
                Debug.Log("Player won, adding the winner's id");
                Debug.Log("Did I win? " + photonView.IsMine);
                Debug.Log(gm.localPlayerManager.firebaseUserId);
                gm.positions.Add(gm.localPlayerManager.firebaseUserId);
                Debug.Log("Positions: " + string.Join(",", gm.positions));
            }

            DatabaseManager.ProcessMatchOutcomes(gm.positions);
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

            SendAttack();
            // animations
            try
            {
                character.Animate(Character.Anim.Attack);
            }
            catch(Exception e)
            {
                Debug.Log("Animation error");
            }

            character.Attack();
        }

        public void SendAttack()
        {
            Debug.Log("Sending my words first");
            String[] toSend = wordQueue.ToArray<string>();
            this.photonView.RPC(
                "UpdateWordsFromClient",
                RpcTarget.AllBuffered,
                toSend
            );

            int target = GetTarget();
            Debug.Log("Attacking " + target);
            if (gm.playerLookup.ContainsKey(target))
            {
                Debug.Log("sending rpc");
                gm.playerLookup[target].photonView.RPC(
                    "PlayerAttacked",
                    RpcTarget.AllBuffered,
                    this.photonView.ViewID,
                    target
                );

            }

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

            try
            {
                character.Animate(Character.Anim.FailCast);
            }
            catch(Exception e)
            {
                Debug.LogError("Animation Error");
            }
            
            LocalData.audioManager.Play(AudioManager.SoundType.playerAttacked);
        }

        public int GetTarget(bool includeBots = true)
        {
            int target = -1;
            //List<Player> options = PhotonNetwork.PlayerList.ToList<Player>();
            List<PlayerManager> options = gm.playerLookup.Values.ToList<PlayerManager>();

            Debug.Log("Options: " + options.Count);

            options.Remove(this);
            Debug.Log("Options: " + options.Count);

            options = options
                .Where(x => x.alive && (includeBots || x.isBot)).ToList<PlayerManager>();

            //foreach(PlayerManager option in options)
            //{
            //    if(!option.alive){
            //        Debug.Log("removing target option as it's dead");
            //        options.Remove(option);
            //    }
            //    else if(!includeBots && option.isBot)
            //    {
            //        Debug.Log("removing target option as it's a bot");
            //        options.Remove(option);
            //    }
            //}

            Debug.Log("Options: " + options.Count);
            //gm.PrintLookup();

            if (options.Count > 0)
            {
                return options[random.Next(options.Count)].photonView.ViewID;
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
            int p1Alive = gm.GetPlayerManagerByActor(p1.ActorNumber) != null
                && gm.GetPlayerManagerByActor(p1.ActorNumber).alive ? 0 : 1000;
            int p2Alive = gm.GetPlayerManagerByActor(p1.ActorNumber) != null
                && gm.GetPlayerManagerByActor(p1.ActorNumber).alive ? 0 : 1000;

            int p1Score = p1.ActorNumber + p1Alive;
            int p2Score = p2.ActorNumber + p2Alive;

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

            lastAttackerId = attacker;


            //todo check if this is correct
            // I think every client was updating it lol
            if (PhotonNetwork.IsMasterClient)
            {
                AddWordFromPlayer(attacker);
            }
           
            if (photonView.IsMine)
            {
                LocalData.timesAttacked++;
                try
                {
                    LocalData.killerNickName = gm.playerLookup[attacker].isBot ?
                        "Bot" : gm.playerLookup[attacker].photonView.Owner.NickName;
                }
                catch (Exception e)
                {
                    Debug.Log("Couldn't get attacker nickname");
                }

                try
                {
                    character.Animate(Character.Anim.Attacked);
                }
                catch(Exception e)
                {
                    Debug.LogError("Animation error");
                }

                //try {
                    //this.photonView.getc
                    if (gm.playerLookup[attacker].photonView.Owner.CustomProperties.ContainsKey("characterId"))
                    {
                        Character attackingCharacter = LocalData
                            .characterSet
                            .GetCharacterById((int)gm.playerLookup[attacker].photonView.Owner.CustomProperties["characterId"]);

                        GameObject attackerChar = GameObject.Instantiate(attackingCharacter.characterPrefab, gm.attackerSpawnLocation);
                        attackerChar.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                        attackerChar.transform.localRotation = Quaternion.Euler(0, 180, 0);
                        Character toAnimate = attackerChar.GetComponent<Character>();
                        toAnimate.Animate(Character.Anim.Attack);
                        toAnimate.AttackMe(gm.attackerProjectileSpawnLocation);

                    }
                    else
                    {
                        Debug.LogError("Couldn't get character from attacker");
                    }
                //}
                //catch(Exception e)
                //{
                //    Debug.Log("Issue with attackedanim");
                //    Debug.Log(e.Message);
                //}
                
                LocalData.audioManager.Play(AudioManager.SoundType.playerAttacked);

                if(gm.playerLookup[attacker] != null && gm.playerLookup[attacker].name != null)
                {
                    gm.ShowAlert("Attacked by " + gm.playerLookup[attacker].name);
                } else
                {
                    gm.ShowAlert("You were attacked!");
                }
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
            if (photonView.IsMine)
            {
                Debug.Log("Adding words from client");
            }
            this.wordQueue = newQueue.ToList<string>();
            //Debug.Log(String.Join(",", this.wordQueue));

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Checking for death");
                CheckForDeath();
            }
            if (photonView.IsMine)
            {
                //Debug.Log("Made thru");
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
            Debug.Log("player died, updating players left");
            gm.playerLeftField.text = gm.CountLivingPlayers() + " Players Alive";

            if (!alive)
            {
                return;
            }

            alive = false;


            if (gm.playerLookup.ContainsKey(killerId))
            {
                Debug.Log("We have a reference to the player");
                gm.playerLookup[killerId].killedPlayers.Add(this.photonView.ViewID);
                Debug.Log("positions: " + String.Join(",", gm.positions));
            }
            else {
                Debug.Log("No reference to the player");
            }


            //if I'm the killer...
            if (killerId == gm.localPlayerManager.photonView.ViewID)
            {
                LocalData.kills++;
                if(this.name != null)
                {
                    gm.ShowAlert("You killed " + this.name);
                }
                else
                {
                    gm.ShowAlert("You killed a player");
                }
            }

            Debug.Log("adding firebase userid " + this.firebaseUserId);
            if(!gm.positions.Contains(this.firebaseUserId))
                gm.positions.Add(this.firebaseUserId);

            Debug.Log("positions: " + String.Join(",", gm.positions));

            if (photonView.IsMine)
            {

                LocalData.playersLeft = gm.CountLivingPlayers();
                //if I'm master and game is over, I need to wait around to write states
                if (PhotonNetwork.IsMasterClient &&
                    (gm.CountLivingPlayers() <= gm.playersToWin ||
                    gm.CountLivingPlayers(includeBots: false) == 0))
                {
                    return;
                }

                

                PhotonNetwork.Disconnect();
                SceneManager.LoadScene("Post Game");
            }

        }

        [PunRPC]
        void PlayerWon(bool botVictory)
        {

            Debug.Log("In playerwon");
            gm.gameOver = true;
            gm.DisableSends();


            //this is sent via the winner player's photonview
            if (photonView.AmOwner && !botVictory)
            {
                LocalData.amWinner = true;
            }

            if (PhotonNetwork.IsMasterClient && !gm.isLoading)
            {
                gm.isLoading = true;

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
            gm = GameObject.Find("Game Manager").GetComponent<GameManager>();
            gm.RegisterPlayer(this);
        }
        // Use this for initialization
        void Start()
        {

            gm.playerLeftField.text = ((int)(PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("bots") ?
                PhotonNetwork.CurrentRoom.CustomProperties["bots"] : 0) + PhotonNetwork.CurrentRoom.PlayerCount)
                + " Players Alive";

            this.debugInterval = new TimeKeeper(5000);
            this.sendInterval = new TimeKeeper(2000);
            this.masterHealthInterval = new TimeKeeper(500);

            Debug.Log("Db? " + LocalData.ShouldUseDB());

            if (photonView.IsMine)
            {
                this.wordList = LocalData.wordList.words;

                this.firebaseUserId = LocalData.ShouldUseDB() ? LocalData.authManager.auth.CurrentUser.UserId : "logged_out";
                Hashtable hash = new Hashtable();
                hash.Add("wordList", LocalData.wordList.words.ToArray<string>());
                hash.Add("characterId", LocalData.character.id);
                hash.Add("FirebaseUserId", this.firebaseUserId);
                this.photonView.Owner.SetCustomProperties(hash);

                //prefab stuff
                GameObject spawn = GameObject.Find("Character Model Spawn");
                characterModel = Instantiate(
                    LocalData.character.characterPrefab,
                    spawn.transform
                );
                character = characterModel.GetComponent<Character>();
                

                for (int i = 0; i < startingWords; i++)
                    AddWord();
            } else
            {
                this.firebaseUserId = (string)photonView.Owner.CustomProperties["FirebaseUserId"];
            }


            
            Debug.Log((photonView.IsMine ? "my " : "other ") + "firebase userid is " + firebaseUserId);
            //if (photonView.Owner.IsMasterClient)
            //{
            //    for (int i = 0; i < 5; i++)
            //    {
            //        Instantiate()
            //    }
            //}




        }

        // Update is called once per frame
        void Update()
        {
            //this.wordQueue.Add("Poop");
            if (sendInterval.ShouldExecute) {
                if (PhotonNetwork.IsMasterClient && !gm.gameOver)
                {
                    //Debug.Log("Triggering Words");
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
                    //Debug.Log("Sending Health Ping");

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
                //Debug.Log("Should I print status?");
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
