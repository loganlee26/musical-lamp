using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicController : MonoBehaviour
{
    public Slider volumeSlider;
    private AudioSource musicSource;

    // Start is called before the first frame update
    void Start()
    {
        // Find the AudioSource playing music
        musicSource = GameObject.FindWithTag("Music").GetComponent<AudioSource>();

        // Load saved volume or default to max volume
        volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        ApplyVolume(volumeSlider.value);

        // Add listener for slider changes
        volumeSlider.onValueChanged.AddListener(ApplyVolume);
    }

    void ApplyVolume(float volume)
    {
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume); // Save the volume setting
    }

    public void ToggleMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
        }
        else
        {
            musicSource.Play();
        }
    }
}
