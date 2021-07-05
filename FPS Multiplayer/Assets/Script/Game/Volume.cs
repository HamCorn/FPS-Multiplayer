using UnityEngine;
using UnityEngine.UI;


public class Volume : MonoBehaviour
{
    private static readonly string FirstPlay = "FirstPlay";
    private static readonly string soundPref = "soundPref";
    private int firstPlayInt;
    public Slider soundSlider;
    public float soundFloat;

    public static Volume singleton;

    void Start()
    {
        singleton = this;
        firstPlayInt = PlayerPrefs.GetInt(FirstPlay);

        if (firstPlayInt == 0)
        {
            soundFloat = 1f;
            soundSlider.value = soundFloat;
            PlayerPrefs.SetFloat(soundPref, soundFloat);
            PlayerPrefs.SetInt(FirstPlay, -1);
        }
        else
        {
            soundFloat = PlayerPrefs.GetFloat(soundPref);
            soundSlider.value = soundFloat;
        }
    }
    public void SaveSoundSettings()
    {
        PlayerPrefs.SetFloat(soundPref, soundSlider.value);
    }
    void OnApplicationFocus(bool inFocus)
    {
        if (!inFocus)
        {
            SaveSoundSettings();
        }
    }

    public void UpdateSound()
    {
        AudioListener.volume = soundSlider.value;
    }
}
