using UnityEngine;

public class RandomVoice : MonoBehaviour
{
    [SerializeField] private AudioClip[] voiceClips;
    [SerializeField] private float minInterval = 8f;
    [SerializeField] private float maxInterval = 15f;

    private AudioSource audioSource;
    private float timer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        timer = Random.Range(minInterval, maxInterval);
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            PlayRandomVoice();
            timer = Random.Range(minInterval, maxInterval);
        }
    }

    private void PlayRandomVoice()
    {
        if (voiceClips.Length == 0)
            return;

        int index = Random.Range(0, voiceClips.Length);
        audioSource.PlayOneShot(voiceClips[index]);
    }
}