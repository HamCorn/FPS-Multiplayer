using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//±±®ÓSlider¨∞∞∏º∆
public class PairValuesSlider : MonoBehaviour
{
    Slider sliderComponent;

    void Start()
    {
        sliderComponent = GetComponent<Slider>();
    }

    void Update()
    {
        if (sliderComponent.value % 2 != 0)
            sliderComponent.value--;

        if (sliderComponent.value < 0)
            sliderComponent.value = 0;
    }
}
