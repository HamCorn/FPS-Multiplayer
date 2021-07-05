using UnityEngine;
using UnityEngine.UI;


public class Sensitive : MonoBehaviour
{
    private static readonly string SensitivePlay = "SensitivePlay";
    private static readonly string sensitivePref = "sensitivePref";
    private int sensitivePlayInt;
    public Slider sensitiveSlider;
    public float sensitiveFloat;

    public static Sensitive singleton;

    void Start()
    {
        singleton = this;
        sensitivePlayInt = PlayerPrefs.GetInt(SensitivePlay);

        if (sensitivePlayInt == 0)
        {
            sensitiveFloat = 1f;
            sensitiveSlider.value = sensitiveFloat;
            PlayerPrefs.SetFloat(sensitivePref, sensitiveFloat);
            PlayerPrefs.SetInt(SensitivePlay, -1);
        }
        else
        {
            sensitiveFloat = PlayerPrefs.GetFloat(sensitivePref);
            sensitiveSlider.value = sensitiveFloat;
        }
    }
    public void SaveSoundSettings()
    {
        PlayerPrefs.SetFloat(sensitivePref, sensitiveSlider.value);
    }
    void OnApplicationFocus(bool inFocus)
    {
        if (!inFocus)
        {
            SaveSoundSettings();
        }
    }
}
