using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 180f;
    [SerializeField] private int value = 1;

    [Header("Звуки")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float collectVolume = 1f;
    private void Update()
    {
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectSound != null)
            {
                GameObject soundObject = new GameObject("CollectSound");
                soundObject.transform.position = transform.position;
                AudioSource audioSource = soundObject.AddComponent<AudioSource>();
                audioSource.clip = collectSound;
                audioSource.volume = collectVolume;
                audioSource.Play();
                Destroy(soundObject, collectSound.length);
            }
            CoinCounter.Instance.AddCoins(value);
            Destroy(gameObject);
        }
    }
}