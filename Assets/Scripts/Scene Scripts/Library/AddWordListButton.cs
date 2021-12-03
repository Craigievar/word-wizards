using System;
using UnityEngine;


namespace Com.TypeGames.TSBR
{
 
    public class AddWordListButton: MonoBehaviour
    {
        public GameObject purchaseWordListPanel;
        public void AddWordListPressed()
        {
            purchaseWordListPanel.SetActive(true);
        }

    }
}