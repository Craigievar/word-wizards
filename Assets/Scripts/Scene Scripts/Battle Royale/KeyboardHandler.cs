using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// my job is to open the keyboard


namespace Com.TypeGames.TSBR
{
    public class KeyboardHandler : MonoBehaviour
    {

        public InputField input;
        public string unityDebugText;
        public GameManager gm;
        private bool keyboardIsOn;
        private int wordIndex = 0;
        private string[] wordList = new string[] { "hello", "my", "name", "is", "craig" };

        [SerializeField]
        private Text userInputFeedback;


        void Awake()
        {
        }

        void Start()
        {
            TurnOnKeyboard();
            TouchScreenKeyboard.hideInput = true;
        }

        // Update is called once per frame
        void Update()
        {
            //Debug.Log(gm.GetCurrentWord());


            

            if(input.text != null && input.text.Contains(System.Environment.NewLine))
            {
                Debug.Log("Handle input");
                HandleInput();
                input.text = "";
            }

            if (input.text.Length >= gm.GetCurrentWord().Length)
            {
                input.text = input.text.Substring(0, gm.GetCurrentWord().Length);
            }

            userInputFeedback.text = MakeInputText();

        }

        void HandleInput()
        {
            if(input.text.Equals(""))
            {
                return;
            }

            if (input.text.ToLower().Trim().Equals(gm.GetCurrentWord()))
            {
                gm.localPlayerManager.WordWasCorrect();
            } else
            {
                gm.localPlayerManager.WordWasIncorrect();
            }
        }

        public void TurnOnKeyboard()
        {
            Debug.Log("Activating input field");
            input.enabled = true;
            input.ActivateInputField();
            TouchScreenKeyboard.hideInput = true;
            //Debug.Log("Is it active? " + input.IsActive());
        }

        public void TurnOffKeyboard()
        {
            input.DeactivateInputField();
            input.enabled = false;
        }

        public string MakeInputText()
        {
            string inputText = input.text;
            string target = gm.GetCurrentWord();
            string output = "";


            if (target == null || target.Length == 0)
            {
                return "";
            }

            for (int i = 0; i < inputText.Length; i++)
            {
                if (i >= target.Length)
                {
                    //this shouldn't even happen, we truncate above
                    output += "<color=red><b>" + inputText[i] + "</b></color>";
                }
                else
                {
                    output += inputText[i] == target[i] ?
                        "<color=green><b>" + inputText[i] + "</b></color>" :
                        "<color=red><b>" + inputText[i] + "</b></color>";
                }
            }

            if(target.Length > inputText.Length)
            {
                output += new System.String('-', target.Length - inputText.Length);
            }

            return output;
        }
    }
}