using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public AudioSource MusicSource;
    public AudioClip mainpageClip;
    public AudioClip fightClip;
    public Slider Volumn;
    public Button NoMusic;
    public Button HasMusic;
    public static SoundManager instance = null;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        
        if(instance == null)
        {
            instance = this;
        }
        else if(instance !=this)
        {
            Destroy(gameObject);
        }
        MusicSource.clip = mainpageClip;
        MusicSource.Play();
        Volumn.onValueChanged.AddListener(delegate {MusicSource.volume = Volumn.value; });
        NoMusic.onClick.AddListener(delegate { MusicSource.Play();NoMusic.gameObject.SetActive(false);HasMusic.gameObject.SetActive(true); });
        HasMusic.onClick.AddListener(delegate { MusicSource.Pause();HasMusic.gameObject.SetActive(false);NoMusic.gameObject.SetActive(true); });
        NoMusic.gameObject.SetActive(false);
    }
    public void ChangeClip()
    {
        MusicSource.Stop();
        if(MusicSource.clip==mainpageClip)
            MusicSource.clip = fightClip;
        else
            MusicSource.clip = mainpageClip;
        MusicSource.Play();
    }

}
