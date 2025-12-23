using UnityEngine;
using UnityEngine.UI; // Wajib ada untuk mengakses komponen UI

public class GaugeBarLogic : MonoBehaviour
{
    private Image _gaugeImage;

    void Start()
    {
        // Mengambil komponen Image yang ada di objek ini secara otomatis
        _gaugeImage = GetComponent<Image>();
    }

    void Update()
    {
        if (_gaugeImage != null)
        {
            // Update tampilan bar berdasarkan nilai di GlobalData (0f - 1f)
            // FillAmount 0 = Kosong, FillAmount 1 = Penuh
            _gaugeImage.fillAmount = GlobalData.gauge;

            // Efek Warna (Opsional): Semakin penuh semakin menyala merahnya
            _gaugeImage.color = Color.Lerp(Color.yellow, Color.red, GlobalData.gauge);
        }
    }
}