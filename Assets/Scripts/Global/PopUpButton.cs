using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


namespace Com.TypeGames.TSBR
{
    
    [RequireComponent(typeof(Button))]
    public class PopUpButton : MonoBehaviour
    {
        public Text popupText;

        public void Start()
        {
            var button = this.gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        public void OnClick()
        {
            Destroy(this.gameObject.transform.parent.parent.gameObject);
        }

        public void SetText(string text)
        {
            popupText.text = text;
        }
    }
}