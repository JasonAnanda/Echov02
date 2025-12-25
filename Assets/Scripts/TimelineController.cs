using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TimelineController : MonoBehaviour
{
    public RectTransform cursor;
    public Transform noteSpawnPoint;
    public GameObject prefabA;
    public GameObject prefabY;
    public GameObject canvasTimeline;

    [Header("Manual Cursor Position")]
    public float cursorStartX = -450f;
    public float cursorEndX = 450f;

    [Header("Visual Alignment")]
    public float noteYOffset = 0f;
    public float cursorYOffset = 0f;

    private float _currentProgress = 0f;
    private float _noteSpacing = 120f;
    private List<GameObject> _activeNotes = new List<GameObject>();

    // Variabel untuk sinkronisasi waktu
    private float _startTime;
    private float _duration;
    private bool _isMoving = false;

    void Start()
    {
        if (canvasTimeline != null) canvasTimeline.SetActive(true);
        ResetCursor();
    }

    private void ResetCursor()
    {
        _isMoving = false;
        _currentProgress = 0f;
        if (cursor != null)
        {
            cursor.anchoredPosition = new Vector2(cursorStartX, cursorYOffset);
        }
    }

    public void ClearTimeline()
    {
        foreach (GameObject n in _activeNotes) if (n != null) Destroy(n);
        _activeNotes.Clear();
        ResetCursor();
    }

    public void SpawnPattern(string[] command)
    {
        ClearTimeline();
        // Logika spawn tetap sama...
        for (int i = 0; i < command.Length; i++)
        {
            GameObject prefabToSpawn = (command[i] == "A") ? prefabA : (command[i] == "Y" || command[i] == "W") ? prefabY : null;
            if (prefabToSpawn != null)
            {
                GameObject note = Instantiate(prefabToSpawn, noteSpawnPoint);
                note.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * _noteSpacing, noteYOffset);
                _activeNotes.Add(note);
            }
        }
    }

    // --- UPDATE UTAMA: Gerakan Linear ---
    void Update()
    {
        if (_isMoving && cursor != null)
        {
            // Hitung sudah berapa lama waktu berlalu sejak musik/sequence mulai
            float elapsed = Time.time - _startTime;

            // Hitung progres (0 sampai 1) secara linear berdasarkan total durasi
            _currentProgress = Mathf.Clamp01(elapsed / _duration);

            // Tentukan posisi X dengan Lerp agar sangat halus (60+ FPS)
            float targetX = Mathf.Lerp(cursorStartX, cursorEndX, _currentProgress);
            cursor.anchoredPosition = new Vector2(targetX, cursorYOffset);

            // Berhenti jika sudah sampai ujung
            if (_currentProgress >= 1f) _isMoving = false;
        }
    }

    // Panggil fungsi ini saat MonsterLogic memulai sequence
    public void StartManualMovement(float totalDuration)
    {
        _startTime = Time.time;
        _duration = totalDuration;
        _isMoving = true;
    }

    // Fungsi lama UpdateCursor (Opsional: Bisa tetap dipakai untuk koreksi ringan)
    public void UpdateCursor(float progress)
    {
        // Jika ingin sinkronisasi ketat, biarkan Update yang menangani linear-nya
    }
}