using UnityEngine;

public class PlayerBounce : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float bounceHeight = 0.3f;

    private Vector3 _startPos;
    private Vector3 _upPos;
    private int _moveState = 0; // 0 = di bawah, 1 = di atas

    void Start()
    {
        _startPos = transform.position;
        _upPos = _startPos + Vector3.up * bounceHeight;
    }

    void OnEnable()
    {
        RhythmManager.OnBeat += HandleHalfStep;
    }

    void OnDisable()
    {
        RhythmManager.OnBeat -= HandleHalfStep;
    }

    void HandleHalfStep(int beatCounter)
    {
        // Kita hanya bergerak setiap kali systemBeatCounter == 0 (saat tick berbunyi)
        if (beatCounter == 0)
        {
            if (_moveState == 0)
            {
                // Jika sedang di bawah, pindah ke atas
                transform.position = _upPos;
                _moveState = 1;
            }
            else
            {
                // Jika sedang di atas, pindah ke bawah
                transform.position = _startPos;
                _moveState = 0;
            }
        }
        // Jika beatCounter == 1, karakter akan diam di posisinya sekarang (Tahan)
    }
}