using System.Collections;
using UnityEngine;

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
        [Range(0,1)] [SerializeField] private float soundVolume = 0.75f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlaySound(AudioClip audioClip, float volume = 1f, bool randomPitch = false)
        {
            if (audioClip == null) return;
            GameObject tempAudioParent = GameObject.Find("Temp Audio Parent") ?? new GameObject("Temp Audio Parent");
            GameObject tempAudio = new GameObject("Temp Audio");
            tempAudio.transform.parent = tempAudioParent.transform;
            AudioSource tempAudioSource = tempAudio.AddComponent<AudioSource>();

            tempAudioSource.playOnAwake = false;
            tempAudioSource.clip = audioClip;
            tempAudioSource.volume = soundVolume * volume;
            if (randomPitch)
            {
                tempAudioSource.pitch = Random.Range(0.7f, 1.3f);
            }
            tempAudioSource.Play();
            StartCoroutine(DestroyTempAudio(tempAudio));
        }

        public GameObject PlayLoopSound(AudioClip audioClip)
        {
            if (audioClip == null) return null;
            GameObject tempAudio = new GameObject("Temp Audio");
            AudioSource tempAudioSource = tempAudio.AddComponent<AudioSource>();
            tempAudioSource.playOnAwake = false;
            tempAudioSource.loop = true;
            tempAudioSource.clip = audioClip;
            tempAudioSource.volume = soundVolume;
            tempAudioSource.Play();

            return tempAudio;
        }

        public void StopLoopSound(GameObject tempAudio)
        {
            Destroy(tempAudio);
        }

        private IEnumerator DestroyTempAudio(GameObject tempAudio)
        {
            float clipLength = tempAudio.GetComponent<AudioSource>().clip.length;
            yield return new WaitForSeconds(clipLength + 1f);
            Destroy(tempAudio);
        }
    }
