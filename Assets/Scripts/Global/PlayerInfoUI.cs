using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Com.TypeGames.TSBR
{
    public class PlayerInfoUI : MonoBehaviour
    {
        private PlayerInfoUI instance;

        [SerializeField]
        private Text playerNameText;
        [SerializeField]
        private Text moneyText;
        //private Text characterName;

        private bool enabled = true;

        public void Awake()
        {
            DontDestroyOnLoad(this.gameObject);

            if (instance == null || this == instance)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }


        // Use this for initialization
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Refresh()
        {
            if (!enabled)
            {
                this.gameObject.SetActive(false);
                return;
            }

            if(LocalData.user.name != null && LocalData.user.name.Length > 0)
            {
                playerNameText.text = LocalData.user.name;
            }
                

            if(LocalData.user.money != null)
            {
                moneyText.text = LocalData.user.money.ToString();
            }
        }
    }
}