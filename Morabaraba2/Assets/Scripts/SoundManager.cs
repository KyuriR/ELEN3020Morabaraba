using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSource;

    [Header("Sound Effects")]
    [SerializeField] public AudioClip invalidMoveSound;
    [SerializeField] public AudioClip validMoveSound;
    [SerializeField] public AudioClip removalSound;

    public static SoundManager instance;

    void Awake()
    {
        // Singleton pattern for easy access
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Get or add AudioSource component
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }


    void Start()
    {
        // Configure audio source
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }
    }

    public static void PlayInvalidMove()
    {
        if (instance != null && instance.invalidMoveSound != null)
        {
            instance.audioSource.PlayOneShot(instance.invalidMoveSound);
        }
        else
        {
            Debug.LogWarning("Invalid move sound not assigned!");
        }
    }

    public static void PlayValidMove()
    {
        if (instance != null && instance.validMoveSound != null)
        {
            instance.audioSource.PlayOneShot(instance.validMoveSound);
        }
    }

    public static void PlayRemoval()
    {
        if (instance != null && instance.removalSound != null)
        {
            instance.audioSource.PlayOneShot(instance.removalSound);
        }
    }

}
