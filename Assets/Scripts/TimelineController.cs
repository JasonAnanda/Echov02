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

    // --- UPDATE: Menggunakan array untuk akses index yang pasti ---
    private GameObject[] _noteSlots = new GameObject[6];
    private List<GameObject> _activeNotes = new List<GameObject>();

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

        // Reset array slots
        for (int i = 0; i < _noteSlots.Length; i++) _noteSlots[i] = null;

        ResetCursor();
    }

    public void SpawnPattern(string[] command)
    {
        ClearTimeline();
        for (int i = 0; i < command.Length; i++)
        {
            GameObject prefabToSpawn = (command[i] == "A") ? prefabA : (command[i] == "Y" || command[i] == "W") ? prefabY : null;
            if (prefabToSpawn != null)
            {
                GameObject note = Instantiate(prefabToSpawn, noteSpawnPoint);
                note.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * _noteSpacing, noteYOffset);

                _activeNotes.Add(note);
                // --- UPDATE: Simpan ke slot sesuai urutan beat (0-5) ---
                if (i < _noteSlots.Length) _noteSlots[i] = note;
            }
        }
    }

    // --- FIX ERROR 1: Menambahkan GetCurrentProgress ---
    public float GetCurrentProgress()
    {
        return _currentProgress;
    }

    // --- FIX ERROR 2: Menambahkan MarkDiamondHit ---
    public void MarkDiamondHit(int index)
    {
        if (index >= 0 && index < _noteSlots.Length && _noteSlots[index] != null)
        {
            // Feedback Visual: Ubah warna menjadi hijau (asumsi prefab punya komponen Image)
            Image img = _noteSlots[index].GetComponent<Image>();
            if (img != null)
            {
                img.color = Color.green;
            }
            // Efek tambahan: sedikit membesar saat kena hit
            _noteSlots[index].transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }
    }

    void Update()
    {
        if (_isMoving && cursor != null)
        {
            float elapsed = Time.time - _startTime;
            _currentProgress = Mathf.Clamp01(elapsed / _duration);

            float targetX = Mathf.Lerp(cursorStartX, cursorEndX, _currentProgress);
            cursor.anchoredPosition = new Vector2(targetX, cursorYOffset);

            if (_currentProgress >= 1f) _isMoving = false;
        }
    }

    public void StartManualMovement(float totalDuration)
    {
        _startTime = Time.time;
        _duration = totalDuration;
        _isMoving = true;
    }

    public void UpdateCursor(float progress) { }
}