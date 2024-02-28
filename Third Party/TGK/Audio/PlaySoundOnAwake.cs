using UnityEngine;

namespace TravkinGames.Audio
{
    public class PlaySoundOnAwake : MonoBehaviour
    {
        [SerializeField] private AudioClip _audioClip;
        [SerializeField] private float _volume = 1f;

        private void Awake()
        {
            Sounds.Instance.Play(_audioClip, _volume);
        }
    }
}