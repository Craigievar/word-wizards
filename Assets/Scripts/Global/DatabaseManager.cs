using System;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace Com.TypeGames.TSBR
{
    public class DatabaseManager : MonoBehaviour
    {
        public static DatabaseReference reference;
        private static string filepath;

        void Start()
        {
            //reference = FirebaseDatabase.DefaultInstance.RootReference;
            reference = FirebaseDatabase.GetInstance("https://word-wizards-default-rtdb.firebaseio.com/").RootReference;
            LocalData.OnDatabaseReady();
            filepath = Application.persistentDataPath + "/save.json";
        }

        public static async Task OnDatabaseAndAuthReady()
        {
            Debug.Log("Database handling database + auth ready");
            await LocalData.authManager.auth.CurrentUser.ReloadAsync();

            if (LocalData.authManager.auth.CurrentUser == null)
            {
                Debug.Log("No user logged in, not checking for db updates");
            }

            string localHash = PlayerPrefs.HasKey(LocalData.updateHashKey) ?
                PlayerPrefs.GetString(LocalData.updateHashKey) : "";

            string dbHash;

            //async
            await reference
                .Child("users")
                .Child(LocalData.authManager.auth.CurrentUser.UserId)
                .Child("updateHash")
                .GetValueAsync().ContinueWith(task =>
                {
                    var taskCast = (Task<DataSnapshot>)task;
                    if (task.IsFaulted)
                    {
                        Debug.Log("Database error, reading local data");
                        ReadUserDataFromJson();
                    }

                    if (task.IsCompleted)
                    {
                        Debug.Log(taskCast.Result);
                        if (taskCast.Result.Exists)
                        {
                            dbHash = (string)taskCast.Result.GetValue(false);
                            if (dbHash == localHash)
                            {
                                Debug.Log("Up to date!");
                                ReadUserDataFromJson();
                            }
                            else
                            {
                                Debug.Log("Pulling user data in auth ready");
                                PullUserData();
                            }
                        }
                    }
                });
        }

        public static void FlagDatabaseUpdate()
        {
            Debug.Log("Database flagging update");
            Guid guid = Guid.NewGuid();
            LocalData.user.updateHash = guid.ToString();
            PlayerPrefs.SetString(LocalData.updateHashKey, guid.ToString());
        }

        public static void ProcessMatchOutcomes(List<string> positions)
        {
            //todo: master server processes match outcomes for all users
            //updates users with wins, losses, wpm data, match id
            //updates match records with match data
            //requires users to all share their IDs with master :)

            //update leaderboards?
            Debug.Log("Database logging match outcomes");

            string basePath = "/matches/" + Guid.NewGuid().ToString() + "/";
            Dictionary<string, System.Object> childUpdates = new Dictionary<string, System.Object>();

            positions.Reverse();
            MatchData sl = new MatchData(positions);


            Debug.Log("sending positions to db: " + JsonUtility.ToJson(sl));
            childUpdates[basePath + "Data"] = JsonUtility.ToJson(sl);
            childUpdates[basePath + "Timestamp"] = Firebase.Database.ServerValue.Timestamp;

            reference.UpdateChildrenAsync(childUpdates);
        }

        public static async Task SendMyPostMatchData()
        {
            await PullUserData();

            if (LocalData.amWinner)
            {
                LocalData.user.wins++;
            }
            else
            {
                LocalData.user.losses++;
            }

            LocalData.user.wordsCorrect += LocalData.wordsCorrect;
            LocalData.user.wordsIncorrect += LocalData.wordsIncorrect;
            LocalData.user.kills += LocalData.kills;
            LocalData.user.inGameTime += (int)Math.Round(LocalData.gameTimer);
             
            await WriteUser();
        }

        public static async Task OnUserSelectedCharacter(int characterId)
        {
            Debug.Log("Database handling character selection");

            if (!LocalData.user.charactersOwned.Contains(characterId))
            {
                Debug.LogError("Selected character I don't own");
                return;
            }

            await PullUserData();
            LocalData.user.currentCharacterId = characterId;
            await WriteUser();
        }

        public static async Task OnUserSelectedWordList(int wordListId)
        {
            Debug.Log("Database handling wordlist selection");
            if (!LocalData.user.wordlistsOwned.Contains(wordListId) &&
                !(LocalData.user.customWordLists.FindIndex(x => x.id == wordListId) > 0))
            {
                Debug.LogError("Selected character I don't own");
                return;
            }


            await PullUserData();
            LocalData.user.currentWordListId = wordListId;
            await WriteUser();
        }


        public static async Task OnUserAcquiredCharacter(int characterId, int price)
        {
            Debug.Log("Database handling character acquisition");
            await PullUserData();

            if (LocalData.user.charactersOwned.Contains(characterId))
                return;
            
            LocalData.user.charactersOwned.Add(characterId);

            LocalData.user.money -= price;

            await WriteUser();
        }

        public static async Task OnUserAcquiredWord(string word)
        {
            if (!LocalData.user.words.Contains(word))
            {
                LocalData.user.words.Add(word);
                await WriteUser();
            }

            await Task.Run(() => { });
        }

        public static async void OnUserModifiedWordList(int wordListId, WordList wordList, int price)
        {
            await PullUserData();

            if (LocalData.user.customWordLists.FindIndex(x=>x.id == wordListId) > 0)
            {
                LocalData.user.customWordLists[
                    LocalData.user.customWordLists.FindIndex(x => x.id == wordListId)
                ] = wordList;
            }
            else
            {
                LocalData.user.customWordLists.Add(wordList);
            }

            LocalData.user.money -= price;

            await WriteUser();
        }

        public static async Task OnUserPurchasedWordList(WordList wordList)
        {
            Debug.Log("Database handling wordlist purchase");
            await PullUserData();

            if (LocalData.user.wordlistsOwned.Contains(wordList.id))
                return;

            LocalData.user.money -= wordList.price;
            LocalData.user.wordlistsOwned.Add(wordList.id);
            await WriteUser();
        }

        public static void TaskDebug(Task task) {
            {
                if (task.IsFaulted)
                {
                    Debug.Log("Database error: " + task.Exception);
                }
                else
                {
                    Debug.Log("Database operation successful");
                }
            }
        }

        public static async Task OnUserCreated()
        {
            Debug.Log("Database handling created user");
            LocalData.user.DataSetup();
            await WriteUser();
        }

        public static async Task WriteUser()
        {
            Debug.Log("Database writing user");
            FlagDatabaseUpdate();

            if (!LocalData.user.setupDone)
            {
                Debug.Log("You are trying to write a user, but haven't set up\n" +
                    "this user for the DB. Investigate this");
            }

            if (!LocalData.ShouldUseDB())
            {
                Debug.Log("You are trying to write to the db, but the user is either\n" +
                    "not logged in, or not verified. Investigate this");
            }

            WriteLocalData();

            string toSet = JsonUtility.ToJson(LocalData.user);
            await reference
                .Child("users")
                .Child(LocalData.authManager.auth.CurrentUser.UserId)
                .SetRawJsonValueAsync(toSet)
                .ContinueWith(task => TaskDebug(task));
        }

        public static async Task PullUserData()
        {
            //set local data to all of the user fields

            await reference
                .Child("users")
                .Child(LocalData.authManager.auth.CurrentUser.UserId)
                .GetValueAsync().ContinueWith(
                    task =>
                    {
                        LocalData.user = JsonUtility.FromJson<User>(task.Result.GetRawJsonValue());
                        LocalData.user.PrintDebug();
                        if(LocalData.user.currentCharacterId >= 0)
                            LocalData.character = LocalData.characterSet.GetCharacterById(LocalData.user.currentCharacterId);
                        if (LocalData.user.currentWordListId >= 0)
                            LocalData.wordList = WordListSet.GetWordListById(LocalData.user.currentWordListId);
                    }
                );

        }

        public static void ReadUserDataFromJson()
        {
            using (StreamReader sr = File.OpenText(filepath)) 
            {
                string saveJSON = "";

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    saveJSON += line;
                }

                Debug.Log("Read: " + saveJSON);

                LocalData.user = JsonUtility.FromJson<User>(saveJSON);
            }
        }

        public static void OnLogout()
        {
            Debug.Log("Database handling logout");
            LocalData.user = new User();
            LocalData.SetCharacter(useDb: false);
            LocalData.SetWordList(useDb: false);
            WriteLocalData();
        }

        public static void WriteLocalData()
        {
            string toSet = JsonUtility.ToJson(LocalData.user);
            Debug.Log("Database riting local data : " + toSet);
            using (StreamWriter sw = File.CreateText(filepath))
            {
                sw.WriteLine(toSet);
            }
        }
    }


}
