using System.Collections;
using UnityEngine;

namespace TaigaGames.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private Music _music;
        [SerializeField] private Sounds _sounds;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(this);
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);
            _music.PlayNext();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _music.gameObject.SetActive(hasFocus);
            _sounds.gameObject.SetActive(hasFocus);
        }
    }
}