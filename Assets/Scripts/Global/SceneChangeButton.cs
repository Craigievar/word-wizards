using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


namespace Com.TypeGames.TSBR
{
    [RequireComponent(typeof(Button))]
    public class SceneChangeButton : MonoBehaviour
    {
        public string sceneName;

        public void Start()
        {
            var button = this.gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        public void OnClick()
        {
            LocalData.audioManager.Play(AudioManager.SoundType.button);
            SceneManager.LoadScene(sceneName);
        }
    }
}