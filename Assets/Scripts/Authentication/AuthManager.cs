using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;
using String = System.String;

namespace Com.TypeGames.TSBR
{
    public class AuthManager : MonoBehaviour
    {

        public FirebaseAuth auth;
        protected Dictionary<string, FirebaseUser> userByAuth =
                new Dictionary<string, FirebaseUser>();

        private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

        private bool fetchingToken = false;

        private void Awake()
        {

        }

        // Start is called before the first frame update
        void Start()
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    InitializeFirebase();
                }
                else
                {
                    Debug.LogError(
                      "Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void InitializeFirebase()
        {
            Debug.Log("Setting up firebase auth");
            auth = FirebaseAuth.DefaultInstance;
            auth.StateChanged += AuthStateChanged;
            auth.StateChanged += IdTokenChanged;
            AuthStateChanged(this, null);
        }

        public void AuthStateChanged(object sender, System.EventArgs eventArgs)
        {
            FirebaseAuth senderAuth = (FirebaseAuth)sender;
            FirebaseUser user = null;

            if (senderAuth != null) userByAuth.TryGetValue(senderAuth.App.Name, out user);

            if (senderAuth == auth && senderAuth.CurrentUser != user)
            {
                bool signedIn = user != senderAuth.CurrentUser && senderAuth.CurrentUser != null;
                if (!signedIn && user != null)
                {
                    Debug.Log("Signed out " + user.UserId);
                    Debug.Log("Need to sign in");
                    //SceneManager.LoadScene("SignInScene");
                }

                user = senderAuth.CurrentUser;
                userByAuth[senderAuth.App.Name] = user;

                if (signedIn)
                {
                    Debug.Log("Signed in " + user.DisplayName);
                    //DisplayDetailedUserInfo(user, 1);
                    //SceneManager.LoadScene("MainScene");
                }
            }
            else
            {
                Debug.Log("Need to sign in");
                //SceneManager.LoadScene("SignInScene");
            }
        }

        public void IdTokenChanged(object sender, System.EventArgs eventArgs)
        {
            Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
            if (senderAuth == auth && senderAuth.CurrentUser != null && !fetchingToken)
            {
                senderAuth.CurrentUser.TokenAsync(false).ContinueWithOnMainThread(
                  task => Debug.Log(String.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));
            }
        }

        protected void DisplayDetailedUserInfo(Firebase.Auth.FirebaseUser user, int indentLevel)
        {
            string indent = new String(' ', indentLevel * 2);
            DisplayUserInfo(user, indentLevel);
            Debug.Log(String.Format("{0}Anonymous: {1}", indent, user.IsAnonymous));
            Debug.Log(String.Format("{0}Email Verified: {1}", indent, user.IsEmailVerified));
            Debug.Log(String.Format("{0}Phone Number: {1}", indent, user.PhoneNumber));
            var providerDataList = new List<Firebase.Auth.IUserInfo>(user.ProviderData);
            var numberOfProviders = providerDataList.Count;
            if (numberOfProviders > 0)
            {
                for (int i = 0; i < numberOfProviders; ++i)
                {
                    Debug.Log(String.Format("{0}Provider Data: {1}", indent, i));
                    DisplayUserInfo(providerDataList[i], indentLevel + 2);
                }
            }
        }

        protected void DisplayUserInfo(Firebase.Auth.IUserInfo userInfo, int indentLevel)
        {
            string indent = new String(' ', indentLevel * 2);
            var userProperties = new Dictionary<string, string> {
            {"Display Name", userInfo.DisplayName},
            {"Email", userInfo.Email},
            {"Photo URL", userInfo.PhotoUrl != null ? userInfo.PhotoUrl.ToString() : null},
            {"Provider ID", userInfo.ProviderId},
            {"User ID", userInfo.UserId}
          };
            foreach (var property in userProperties)
            {
                if (!String.IsNullOrEmpty(property.Value))
                {
                    Debug.Log(String.Format("{0}{1}: {2}", indent, property.Key, property.Value));
                }
            }
        }

        void OnDestroy()
        {
            try
            {
                auth.StateChanged -= AuthStateChanged;
            } catch (Exception e)
            {
                Debug.Log("Couldn't remove statechanged ref");
            }
            
            auth = null;
        }

        public Task UpdateUserProfileAsync(string newDisplayName)
        {
            if (auth.CurrentUser == null)
            {
                Debug.Log("Not signed in, unable to update user profile");
                return Task.FromResult(0);
            }
            Debug.Log("Updating user profile " + newDisplayName);
            return auth.CurrentUser.UpdateUserProfileAsync(new Firebase.Auth.UserProfile
            {
                DisplayName = newDisplayName,
                PhotoUrl = auth.CurrentUser.PhotoUrl,
            });
        }

        public bool LogTaskCompletion(Task task, string operation)
        {
            bool complete = false;
            if (task.IsCanceled)
            {
                Debug.Log(operation + " canceled.");
            }
            else if (task.IsFaulted)
            {
                Debug.Log(operation + " encounted an error.");
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    string authErrorCode = "";
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        authErrorCode = String.Format("AuthError.{0}: ",
                          ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
                        //GetErrorMessage((Firebase.Auth.AuthError)firebaseEx.ErrorCode);
                    }
                    Debug.Log(authErrorCode + exception.ToString());
                }
                //EnableUI();
            }
            else if (task.IsCompleted)
            {
                Debug.Log(operation + " completed");
                complete = true;
            }
            return complete;
        }
    }

}