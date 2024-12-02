using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    public AudioClip musicClip;
    public AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // If still no AudioSource, add one
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Ensure audio settings are appropriate
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private bool isPlaying = false;
    
    private void OnTriggerEnter(Collider other)
    {   
        if (other.gameObject.CompareTag("Player") && !isPlaying)
        {
            StartCoroutine(PlayMusicCoroutine());
        }
    }
    
    private IEnumerator PlayMusicCoroutine()
    {
        isPlaying = true;
        Debug.Log("Music is playing");
        audioSource.PlayOneShot(musicClip, .9f);
        yield return new WaitForSeconds(musicClip.length);
        isPlaying = false;
    }

}
