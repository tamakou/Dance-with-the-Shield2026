using UnityEngine;

namespace DWS
{
    /// <summary>
    /// Simple BGM player. Assign an AudioSource + AudioClip in Inspector.
    /// </summary>
    public sealed class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _clip;
        [SerializeField] private bool _loop = true;
        [SerializeField, Range(0f, 1f)] private float _volume = 0.7f;

        private void Reset()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void Awake()
        {
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            _audioSource.playOnAwake = false;
            _audioSource.loop = _loop;
            _audioSource.volume = _volume;

            if (_clip != null)
            {
                _audioSource.clip = _clip;
            }
        }

        public void Play()
        {
            if (_audioSource == null) return;
            if (_audioSource.clip == null && _clip != null) _audioSource.clip = _clip;
            if (_audioSource.clip == null)
            {
                Debug.LogWarning("[DWS] MusicPlayer: No AudioClip assigned.");
                return;
            }

            _audioSource.loop = _loop;
            _audioSource.volume = _volume;

            if (!_audioSource.isPlaying) _audioSource.Play();
        }

        public void Stop()
        {
            if (_audioSource == null) return;
            if (_audioSource.isPlaying) _audioSource.Stop();
        }
    }
}
