using UnityEngine;
using UnityEngine.UI; // Wajib untuk Slider

public class GaugeBarLogic : MonoBehaviour
{
    private Slider _gaugeSlider;

    void Start()
    {
        // SEBELUMNYA: GetComponent<Image>()
        // SEKARANG: Mencari komponen Slider
        _gaugeSlider = GetComponent<Slider>();

        if (_gaugeSlider != null)
        {
            _gaugeSlider.minValue = 0f;
            _gaugeSlider.maxValue = 1f;
        }
    }

    void Update()
    {
        if (_gaugeSlider != null)
        {
            // Update nilai slider (0.0 sampai 1.0) dari GlobalData
            _gaugeSlider.value = GlobalData.gauge;
        }
    }
}