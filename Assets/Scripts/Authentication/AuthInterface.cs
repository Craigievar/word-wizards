using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using Firebase.Extensions;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.SceneManagement;



// this class is exploratory
// we'll want to refactor the deep stuff into authmanager
namespace Com.TypeGames.TSBR
{
    public class AuthInterface : MonoBehaviour
    {
        // Use this for initialization
        public InputField emailInput;
        public InputField passwordInput;
        public InputField passwordConfirm;
        public Text errorText;
        public Button signupButton;
        public Button signinButton;
        public Text confirmText;

        public GameObject signUpGroup;
        public GameObject signInGroup;
        public GameObject confirmGroup;

        public string displayName;

        private FirebaseAuth auth;

        private bool createdAccount = false;

        void Start()
        {
            auth = FirebaseAuth.DefaultInstance;
            errorText.enabled = false;

            if (LocalData.userHasAccount)
            {
                SwitchToSignIn();
            } else
            {
                SwitchToCreate();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        #region creating an account
        public void OnCreateAccountPressed()
        {
            if (passwordInput.text.Equals(passwordConfirm.text))
            {
                Debug.Log("Passwords match! Creating acct");
                CreateUserWithEmailAsync();
            }
            else
            {
                errorText.text = "Passwords do not match";
                errorText.gameObject.SetActive(true);
            }

        }

        #endregion

        #region logging in

        public void OnLoginPressed()
        {
            string email = emailInput.text;
            string password = passwordInput.text;
            Debug.Log(String.Format("Attempting to sign in as {0}...", email));
            DisableUI();

            //if (signInAndFetchProfile)
            //{
            auth.SignInAndRetrieveDataWithCredentialAsync(
             Firebase.Auth.EmailAuthProvider.GetCredential(email, password)).ContinueWithOnMainThread(
               HandleSignInWithSignInResult);
            //}
            //else
            //{
            //    return auth.SignInWithEmailAndPasswordAsync(email, password)
            //      .ContinueWithOnMainThread(HandleSignInWithUser);
            //}
        }


        public Task CreateUserWithEmailAsync()
        {
            string email = emailInput.text;
            string password = passwordInput.text;

            Debug.Log(String.Format("Attempting to create user {0}...", email));
            DisableUI();

            // This passes the current displayName through to HandleCreateUserAsync
            // so that it can be passed to UpdateUserProfile().  displayName will be
            // reset by AuthStateChanged() when the new user is created and signed in.
            return auth.CreateUserWithEmailAndPasswordAsync(email, password)
              .ContinueWithOnMainThread((task) =>
              {
                  EnableUI();
                  if(LogTaskCompletion(task, "User Creation"))
                  {
                      //created account
                      createdAccount = true;
                      if (!auth.CurrentUser.IsEmailVerified)
                      {
                          auth.CurrentUser.SendEmailVerificationAsync();
                      }
                      ShowConfirmation();
                  }
                  //UpdateUserProfileAsync(PhotonNetwork.NickName);
                  //LocalData.authManager.UpdateUserProfileAsync("test");
                  return task;
              }).Unwrap();
        }

        public void ResetPasswordPressed()
        {
            Debug.Log("Reset pressed");
            if (emailInput.text.Length < 5)
            {
                errorText.text = "Enter your email to reset";
                errorText.gameObject.SetActive(true);
                errorText.enabled = true;
            }

            Debug.Log("Fetch providers");
            auth.FetchProvidersForEmailAsync(emailInput.text).ContinueWithOnMainThread(
                task => {
                    if (task.IsFaulted)
                    {
                        errorText.text = "Error with reset request: " + task.Exception;
                        errorText.gameObject.SetActive(true);
                        errorText.enabled = true;
                        return;
                    }
                    if (task.IsCanceled)
                    {
                        errorText.text = "Reset request cancelled";
                        errorText.gameObject.SetActive(true);
                        errorText.enabled = true;
                        return;
                    }


                    Debug.Log("Trying to send email reset");
                    SendEmailReset();
                }
            );
        }

        public void SendEmailReset()
        {
            Debug.Log("Send request");
            auth.SendPasswordResetEmailAsync(emailInput.text).ContinueWithOnMainThread(task =>
                {
                    Debug.Log("In task management");
                    if (task.IsFaulted)
                    {
                        Debug.Log("Faulted");
                        errorText.text = "Error with reset request: " + task.Exception;
                    }
                    if (task.IsCanceled)
                    {
                        errorText.text = "Reset request cancelled";
                    }


                    Debug.Log("Should be successful");

                    errorText.text = "Sent reset request to " + emailInput.text;
                    errorText.gameObject.SetActive(true);
                    errorText.enabled = true;
                }
            );

        }

        // Log the result of the specified task, returning true if the task
        // completed successfully, false otherwise.
        protected bool LogTaskCompletion(Task task, string operation)
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
                        GetErrorMessage((Firebase.Auth.AuthError)firebaseEx.ErrorCode);
                        errorText.enabled = true;
                    }
                    Debug.Log(authErrorCode + exception.ToString());
                }
                EnableUI();
            }
            else if (task.IsCompleted)
            {
                Debug.Log(operation + " completed");
                complete = true;
            }
            return complete;
        }

        private void HandleSignInWithSignInResult(Task<Firebase.Auth.SignInResult> task)
        {
            EnableUI();
            if (LogTaskCompletion(task, "Sign-in"))
            {
                createdAccount = false;
                DisplaySignInResult(task.Result, 1);
                ShowConfirmation();
            }

            if(auth.CurrentUser.DisplayName != null && auth.CurrentUser.DisplayName.Length > 0)
            {
                Debug.Log("Got name from login (" + auth.CurrentUser.DisplayName + ")");
                LocalData.SetPlayerName(auth.CurrentUser.DisplayName);
            }
            else
            {
                Debug.Log("Couldn't get name at sign in");
            }
            
        }

        protected void DisplaySignInResult(Firebase.Auth.SignInResult result, int indentLevel)
        {
            string indent = new String(' ', indentLevel * 2);
            var metadata = result.Meta;
            if (metadata != null)
            {
                Debug.Log(String.Format("{0}Created: {1}", indent, metadata.CreationTimestamp));
                Debug.Log(String.Format("{0}Last Sign-in: {1}", indent, metadata.LastSignInTimestamp));
            }
            var info = result.Info;
            if (info != null)
            {
                Debug.Log(String.Format("{0}Additional User Info:", indent));
                Debug.Log(String.Format("{0}  User Name: {1}", indent, info.UserName));
                Debug.Log(String.Format("{0}  Display Name: {1}", indent, auth.CurrentUser.DisplayName));
                Debug.Log(String.Format("{0}  Provider ID: {1}", indent, info.ProviderId));
                DisplayProfile<string>(info.Profile, indentLevel + 1);
            }
        }

        protected void DisplayProfile<T>(IDictionary<T, object> profile, int indentLevel)
        {
            string indent = new String(' ', indentLevel * 2);
            foreach (var kv in profile)
            {
                var valueDictionary = kv.Value as IDictionary<object, object>;
                if (valueDictionary != null)
                {
                    Debug.Log(String.Format("{0}{1}:", indent, kv.Key));
                    DisplayProfile<object>(valueDictionary, indentLevel + 1);
                }
                else
                {
                    Debug.Log(String.Format("{0}{1}: {2}", indent, kv.Key, kv.Value));
                }
            }
        }

        #endregion

        #region

        public void OnSignOutPressed()
        {
            auth.SignOut();
        }
        #endregion

        #region handle UI
        void DisableUI()
        {
            emailInput.DeactivateInputField();
            passwordInput.DeactivateInputField();
            passwordConfirm.DeactivateInputField();
            signupButton.interactable = false;
            signinButton.interactable = false;
        }


        void EnableUI()
        {
            emailInput.ActivateInputField();
            passwordInput.ActivateInputField();
            passwordConfirm.ActivateInputField();
            signupButton.interactable = true;
            signinButton.interactable = true;
        }

        public Task UpdateUserProfileAsync(string newDisplayName = null)
        {
            if (auth.CurrentUser == null)
            {
                Debug.Log("Not signed in, unable to update user profile");
                return Task.FromResult(0);
            }
            displayName = newDisplayName ?? displayName;
            Debug.Log("Updating user profile " + displayName);
            return auth.CurrentUser.UpdateUserProfileAsync(new Firebase.Auth.UserProfile
            {
                DisplayName = displayName,
                PhotoUrl = auth.CurrentUser.PhotoUrl,
            });
        }

        private void GetErrorMessage(AuthError errorCode)
        {
            switch (errorCode)
            {
                case AuthError.MissingPassword:
                    errorText.text = "Missing password.";
                    errorText.enabled = true;
                    break;
                case AuthError.WeakPassword:
                    errorText.text = "Too weak of a password.";
                    errorText.enabled = true;
                    break;
                case AuthError.InvalidEmail:
                    errorText.text = "Invalid email.";
                    errorText.enabled = true;
                    break;
                case AuthError.MissingEmail:
                    errorText.text = "Missing email.";
                    errorText.enabled = true;
                    break;
                case AuthError.UserNotFound:
                    errorText.text = "Account not found.";
                    errorText.enabled = true;
                    break;
                case AuthError.EmailAlreadyInUse:
                    errorText.text = "Email already in use.";
                    errorText.enabled = true;
                    break;
                default:
                    errorText.text = "Unknown error occurred.";
                    errorText.enabled = true;
                    break;
            }
        }

        public void OnReturn()
        {
            if (LocalData.returnScene == null)
            {
                Debug.Log("No scene to return to");
            }

            Debug.Log("Auth finished, returning to " + LocalData.returnScene);

            if(auth.CurrentUser == null)
            {
                Debug.Log("going back without logging in or making an account");
                LocalData.setUpIndex++;
            } else if(auth.CurrentUser.DisplayName == null || auth.CurrentUser.DisplayName.Length == 0)
            {
                Debug.Log("Going back with an account, but no name -- prompt for name.");
                LocalData.setUpIndex++;
            } else
            {
                Debug.Log("Logged in and have a name");
                LocalData.setUpIndex += 2;
            }

            SceneManager.LoadScene(LocalData.returnScene);
        }

        public void SwitchToCreate()
        {
            signUpGroup.SetActive(true);
            signInGroup.SetActive(false);
            confirmGroup.SetActive(false);
            emailInput.enabled = true;
            passwordInput.enabled = true;
        }

        public void SwitchToSignIn()
        {
            signUpGroup.SetActive(false);
            signInGroup.SetActive(true);
            confirmGroup.SetActive(false);
            emailInput.enabled = true;
            passwordInput.enabled = true;
        }

        public void ShowConfirmation()
        {
            signUpGroup.SetActive(false);
            signInGroup.SetActive(false);
            emailInput.gameObject.SetActive(false);
            passwordInput.gameObject.SetActive(false);

            confirmGroup.SetActive(true);

            if (createdAccount)
            {
                confirmText.text = "Account successfully created!\n\n";
                confirmText.text += "We've sent you a confirmation email. Click the link to confirm your account.";
            } else
            {
                confirmText.text = "Logged in!";
            }
        }



        #endregion
    }

}