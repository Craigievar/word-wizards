using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

namespace Com.TypeGames.TSBR
{
    public class BotManager : PlayerManager
    {
        //what's diff for bots?
        //new public bool isBot = true;
        public int attackBaseInterval = 2000;
        public TimeKeeper botAttackTimekeeper;
        public TimeKeeper sendInterval;
        System.Random random = new System.Random();

        public void Start()
        {
            gm = GameObject.Find("Game Manager").GetComponent<GameManager>();
            this.firebaseUserId = "bot" + photonView.ViewID.ToString();
            this.botAttackTimekeeper = new TimeKeeper(attackBaseInterval);
            this.wordList = WordListSet.GetWordListById(0).words;
            this.wordQueue = new List<string>() {
                this.wordList[random.Next(this.wordList.Count)]
            };
            Debug.Log("Bot wordqueue length " + wordQueue.Count);


            this.sendInterval = new TimeKeeper(2000);

            gm.RegisterPlayer(this);
        }

        public void Awake()
        {
            attackBaseInterval = 5000;
        }

        public void Update()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (sendInterval.ShouldExecute)
                {
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
                //Debug.Log("Bot update loop");
                if (botAttackTimekeeper.ShouldExecute)
                {
                    if(this.wordQueue.Count > 0 && !gm.gameOver && this.alive)
                        SendBotAttack();

                    botAttackTimekeeper.Interval = attackBaseInterval / 2
                        + random.Next(attackBaseInterval);
                    botAttackTimekeeper.Reset();
                }
            }
            
        }

        public void SendBotAttack()
        {
            Debug.Log("Bot attacking");
            SendAttack();
        }

        [PunRPC]
        void AddWordFromMaster(String[] newQueue)
        {
            Debug.Log(Environment.StackTrace);
            Debug.Log("Got call to add words (bot)");
            Debug.Log("My words are: " + String.Join(",", newQueue));

            Debug.Log("Bot got word");
            this.wordQueue = newQueue.ToList<string>();

            if (photonView.IsMine)
            {
                LocalData.wordsReceived++;
            }
        }

        [PunRPC]
        void AddWordFromMaster(String firstWord)
        {
            Debug.Log("Bot got word");
            Debug.Log(Environment.StackTrace);
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
            //Debug.Log(Environment.StackTrace);
            if (photonView.IsMine)
            {
                Debug.Log("Adding words from client");
            }
            this.wordQueue = newQueue.ToList<string>();
            Debug.Log(String.Join(",", this.wordQueue));

            if (PhotonNetwork.IsMasterClient)
            {
                CheckForDeath();
            }
            if (photonView.IsMine)
            {
                Debug.Log("Made thru");
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
            catch (Exception e)
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
            Debug.Log("Bot UpdateWordsFromClient");
            this.wordQueue = newQueue.ToList<string>();
        }

        [PunRPC]
        void UpdateWordsFromClient(string firstWord)
        {
            Debug.Log("Bot UpdateWordsFromClient");
            this.wordQueue = new List<string>();
            this.wordQueue.Add(firstWord);
        }

        [PunRPC]
        void UpdateWordsFromClient()
        {
            Debug.Log("Bot UpdateWordsFromClient");
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
                //gm.playerLookup[killerId].killedPlayers.Add(this.photonView.ViewID);
                Debug.Log("positions: " + String.Join(",", gm.positions));
            }
            else
            {
                Debug.Log("No reference to the player");
            }

            //if I'm the killer...
            if (killerId == gm.localPlayerManager.photonView.ViewID)
            {
                LocalData.kills++;

                if (this.name != null)
                {
                    gm.ShowAlert("You killed " + this.name);
                }
                else
                {
                    gm.ShowAlert("You killed a player");
                }
            }

            if(!gm.positions.Contains(this.firebaseUserId))
                gm.positions.Add(this.firebaseUserId);
        }

        [PunRPC]
        void PlayerWon()
        { }

        [PunRPC]
        void PlayerAttacked(int attacker, int target)
        {
            //eventually we want to route this thru the master client.
            //for now it's just whatever

            lastAttackerId = attacker;


            if (PhotonNetwork.IsMasterClient)
            {
                AddWordFromPlayer(attacker);
            }

            if (photonView.IsMine)
            {
                //LocalData.timesAttacked++;
                //try
                //{
                //    LocalData.killerNickName = gm.playerLookup[attacker].isBot ?
                //        "Bot" : gm.playerLookup[attacker].photonView.Owner.NickName;
                //}
                //catch (Exception e)
                ////{
                //    Debug.Log("Couldn't get attacker nickname");
                //}

                //character.Animate(Character.Anim.Attacked);
                //LocalData.audioManager.Play(AudioManager.SoundType.playerAttacked);
            }
        }
    }
}