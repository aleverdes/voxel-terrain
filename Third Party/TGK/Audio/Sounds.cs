using UnityEngine;

namespace TravkinGames.Audio
{
    public class Sounds : MonoBehaviour
    {
        public static Sounds Instance { get; private set; }

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private float _masterVolume = 1f;
        
        public bool Enabled { get; set; }
        
        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Play(AudioClip audioClip, float volumeScale = 1f)
        {
            if (!audioClip)
                return;

            if (!Enabled)
                return;

            _audioSource.PlayOneShot(audioClip, _masterVolume * volumeScale);
        }
    }
}