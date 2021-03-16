using System;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System;
using System.Threading.Tasks;

namespace Com.TypeGames.TSBR
{
    public class DatabaseManager: MonoBehaviour
    {
        public static DatabaseReference reference;

        void Start()
        {
            //reference = FirebaseDatabase.DefaultInstance.RootReference;
            reference = FirebaseDatabase.GetInstance("https://word-wizards-default-rtdb.firebaseio.com/").RootReference;
        }

        public static void WriteUser(string userId, string name)
        {
            reference.Child("users").Child(userId).Child("username").SetValueAsync(name)
                .ContinueWith(task => TaskDebug(task));
        }

        public static void OnUserAccountCreated()
        {
            //create initial database entry; user can't have progress until now
            //anyways
        }

        public static void ProcessMatchOutcomes()
        {
            //todo: master server processes match outcomes for all users
            //updates users with wins, losses, wpm data, match id
            //updates match records with match data
            //requires users to all share their IDs with master :)

            //update leaderboards?
        }

        public static void OnUserAcquiredCharacter(int characterId)
        {
            reference
                .Child("users")
                .Child(LocalData.authManager.auth.CurrentUser.UserId)
                .Child("CharactersOwned")
                .Child(characterId.ToString())
                .SetValueAsync(true).ContinueWith(task => TaskDebug(task));
        }

        public static void OnUserAcquiredWord(string word)
        {
            reference
                .Child("users")
                .Child(LocalData.authManager.auth.CurrentUser.UserId)
                .Child("words")
                .Child(word)
                .SetValueAsync(true)
                .ContinueWith(task => TaskDebug(task));
        }

        public static void OnUserModifiedWordList(int wordListId, WordList wordList)
        {
            string toSet = JsonUtility.ToJson(wordList);

            Debug.Log(LocalData.authManager.auth.CurrentUser.UserId);
            Debug.Log(toSet);

            reference
                .Child("users")
                .Child(LocalData.authManager.auth.CurrentUser.UserId)
                .Child("WordLists")
                .Child("Custom")
                .Child(wordList.id.ToString())
                .SetRawJsonValueAsync(toSet)
                .ContinueWith(task => TaskDebug(task));
        }

        public static void OnUserPurchasedWordList(int wordListId)
        {
            reference
                .Child("users")
                .Child(LocalData.authManager.auth.CurrentUser.UserId)
                .Child("WordLists")
                .Child("PreExisting")
                .Child(wordListId.ToString())
                .SetValueAsync(true)
                .ContinueWith(task => TaskDebug(task));
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

        public static void SetUserField(string field, string value)
        {
            reference
                .Child("users")
                .Child(LocalData.authManager.auth.CurrentUser.UserId)
                .Child(field).SetValueAsync(value)
                .ContinueWith(task => TaskDebug(task));
        }

        public static void SetOtherUserField(string userId, string field, string value)
        {
            reference
                .Child("users")
                .Child(userId)
                .Child(field).SetValueAsync(value)
                .ContinueWith(task => TaskDebug(task));
        }

    }


}
