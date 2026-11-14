using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // To call our sounds from any function without the getComponent aspects
    public static AudioManager instance;
    // Invisible Prefab that is created whenever a sound is played
    [SerializeField] private AudioSource soundEffectObject;
    // Call to set object appropriately 
    private void Awake()
    {
        if (instance == null)
        {
            // If the SoundEffectsManager object is null, then set it to the current object
            instance = this;
            // Dont Destroy on Load
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Destroy previous gameObject for the new one so I can have more than one listener
            Destroy(gameObject);
        }
    }

    // Plays a sound when an action occurs with the sound manager
    public void PlaySoundEffecsClip(AudioClip audioClip, Transform spwnTransform, float volume)
    {
        // Spawn the invisible audioSource object
        AudioSource audioSource = Instantiate(soundEffectObject, spwnTransform.position, Quaternion.identity);

        // Set the audio clip
        audioSource.clip = audioClip;

        // Play the audio clip
        audioSource.Play();

        // Get the length of the audio clip
        float clipLength = audioSource.clip.length;

        // Destory the audio once it finishes playing
        Destroy(audioSource.gameObject, clipLength);

    }

    // Play a random sound from an array of sound effects 
     public void PlayRandomSoundEffecsClip(AudioClip[] audioClip, Transform spwnTransform, float volume)
    {
        // Get a random index
        int rand = Random.Range(0, audioClip.Length);

        // Spawn the invisible audioSource object
        AudioSource audioSource = Instantiate(soundEffectObject, spwnTransform.position, Quaternion.identity);

        // Set the audio clip based on the randomly selected clip's length
        audioSource.clip = audioClip[rand];

        // Play the audio clip
        audioSource.Play();

        // Modify volume]
        audioSource.volume = volume;

        // Get the length of the audio clip
        float clipLength = audioSource.clip.length;

        // Destory the audio once it finishes playing
        Destroy(audioSource.gameObject, clipLength);

    }
}
