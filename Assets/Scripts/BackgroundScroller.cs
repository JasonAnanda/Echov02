using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 0.7f; // Sesuaikan agar selaras dengan kecepatan monster
    public float backgroundWidth = 21.3f; // Lebar gambar background kamu dalam satuan Unity

    private Vector3 _startPosition;

    void Start()
    {
        _startPosition = transform.position;
    }

    void Update()
    {
        // Gerakkan background ke kiri
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        // Jika background sudah bergeser sejauh lebarnya, reset posisinya
        if (transform.position.x <= _startPosition.x - backgroundWidth)
        {
            transform.position = _startPosition;
        }
    }
}