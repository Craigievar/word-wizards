using System;
using UnityEngine;
using UnityEngine.UI;


namespace Com.TypeGames.TSBR
{

    public class PurchaseConfirmationPane : MonoBehaviour
    {

        public WordListButton currentButton;
        public Text priceText;
        public Button continueButton;
        public WordPaneManager parentPane;
        public WordListButtonManager wlbm;


        public void SetValues(WordListButton button)
        {
            this.currentButton = button;
            int price = button.wordList.price;
            
            if (!LocalData.ShouldUseDB())
            {
                priceText.text = LocalData.authManager.auth == null || LocalData.authManager.auth.CurrentUser == null ?
                    "You must be logged in to make purchases" : "You must confirm your email to make purchases";
                continueButton.gameObject.GetComponent<Image>().color = Color.gray;
                continueButton.interactable = false;
                return;
            }

            if (LocalData.user.money < price)
            {
                priceText.text = "Not enough money! You need " + price;
                continueButton.gameObject.GetComponent<Image>().color = Color.gray;
                continueButton.interactable = false;
                return;
            }




            priceText.text = "Confirm purchase for " + price + "?";
            continueButton.gameObject.GetComponent<Image>().color = Color.white;
            continueButton.interactable = true;
        }

        public void ExitPressed()
        {
            parentPane.EnableUI();
            this.gameObject.SetActive(false);
        }

        public async void PurchasePressed()
        {
            await DatabaseManager.OnUserPurchasedWordList(currentButton.wordList);
            LocalData.infoUI.Refresh();
            LocalData.audioManager.Play(AudioManager.SoundType.purchase);
            ExitPressed();
            parentPane.OnExitPressed();
            wlbm.ResetButtons();
        }

    }
}