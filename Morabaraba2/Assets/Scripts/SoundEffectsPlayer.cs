using UnityEngine;

/// <summary>
/// Singleton audio player for all game sound effects.
///
/// INSPECTOR SETUP:
///   - Attach to a persistent GameObject
///   - Assign all AudioClip fields in the Inspector
/// </summary>
public class SoundEffectsPlayer : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip invalidMoveSound;
    [SerializeField] private AudioClip validMoveSound;
    [SerializeField] private AudioClip placementSound;
    [SerializeField] private AudioClip removalSound;

    private static SoundEffectsPlayer instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        if (audioSource != null)
            audioSource.playOnAwake = false;
    }

    public static void PlayInvalidMove()
    {
        if (instance != null && instance.invalidMoveSound != null)
            instance.audioSource.PlayOneShot(instance.invalidMoveSound);
    }

    public static void PlayValidMove()
    {
        if (instance != null && instance.validMoveSound != null)
            instance.audioSource.PlayOneShot(instance.validMoveSound);
    }

    public static void PlayPlacement()
    {
        if (instance != null && instance.placementSound != null)
            instance.audioSource.PlayOneShot(instance.placementSound);
    }

    public static void PlayRemoval()
    {
        if (instance != null && instance.removalSound != null)
            instance.audioSource.PlayOneShot(instance.removalSound);
    }
}
