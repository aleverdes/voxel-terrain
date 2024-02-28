using TravkinGames.Utils;
using UnityEngine;

namespace TravkinGames.Audio
{
    public class Music : MonoBehaviour
    {
        public static Music Instance { get; private set; }

        [SerializeField] private AudioSource[] _playlist;
        [SerializeField] private float _maxVolume = 0.3f;
        [SerializeField] private float _musicChangingSpeed = 0.5f;

        private int _playlistIndex;
        private AudioSource _currentSourceAudio;
        private float _currentTrackDuration;
        
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
            if (Instance == this) 
                Instance = null;
        }

        private void Start()
        {
            _playlist = _playlist.Shuffle();
            foreach (var audioSource in _playlist)
            {
                audioSource.volume = 0;
                audioSource.loop = true;
            }
        }
        
        public void PlayNext()
        {
            if (_playlist == null || _playlist.Length == 0)
                return;

            Play(_playlist[_playlistIndex]);
            _playlistIndex = (_playlistIndex + 1) % _playlist.Length;
        }

        private void Play(AudioSource audioSource)
        {
            _currentSourceAudio = audioSource;
        }

        private void Update()
        {
            if (_playlist == null || _playlist.Length == 0)
                return;

            foreach (var audioSource in _playlist)
            {
                audioSource.mute = Enabled;

                if (audioSource == _currentSourceAudio)
                    audioSource.volume = Mathf.Clamp(audioSource.volume + _musicChangingSpeed * Time.deltaTime, 0, _maxVolume);
                else
                    audioSource.volume = Mathf.Clamp(audioSource.volume - _musicChangingSpeed * Time.deltaTime, 0, _maxVolume);

                if (audioSource.volume < FloatEpsilon.Value)
                {
                    if (audioSource.isPlaying) audioSource.Stop();
                }
                else
                {
                    if (!audioSource.isPlaying) audioSource.Play();
                }
            }

            if (_currentSourceAudio && _currentSourceAudio.clip && _currentSourceAudio.isPlaying)
            {
                _currentTrackDuration += Time.deltaTime;
                if (_currentTrackDuration > _currentSourceAudio.clip.length - (1f / _musicChangingSpeed + 1f))
                {
                    _currentTrackDuration = 0;
                    PlayNext();
                }
            }
        }
    }
}