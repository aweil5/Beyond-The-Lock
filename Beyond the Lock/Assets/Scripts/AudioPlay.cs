using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip audioClip;

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
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (audioClip != null)
            {
                audioSource.PlayOneShot(audioClip);

            }


        }
    }
    
}
