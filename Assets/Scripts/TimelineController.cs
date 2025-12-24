using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TimelineController : MonoBehaviour
{
    public RectTransform cursor;
    public Transform noteSpawnPoint;
    public GameObject prefabA;
    public GameObject prefabY;
    public GameObject canvasTimeline; // Tarik TimeLineContainer ke sini

    [Header("Visual Alignment")]
    public float noteYOffset = 0f;
    public float cursorYOffset = 0f;

    [Header("Smooth Movement")]
    private float _currentProgress = 0f;
    private float _targetProgress = 0f;
    private float _smoothVelocity = 0f;

    private float _noteSpacing = 120f;
    private List<GameObject> _activeNotes = new List<GameObject>();

    void Start()
    {
        // Sembunyikan UI saat awal permainan
        if (canvasTimeline != null) canvasTimeline.SetActive(false);
    }

    // --- UPDATE: Fungsi ShowTimeline dengan log debug ---
    public void ShowTimeline(bool show)
    {
        if (canvasTimeline != null)
        {
            canvasTimeline.SetActive(show);
            // Debug untuk memastikan fungsi terpanggil oleh MonsterLogic
            Debug.Log("Timeline Display: " + show);
        }
    }

    // --- UPDATE: SpawnPattern dengan alur aktivasi UI yang benar ---
    public void SpawnPattern(string[] command)
    {
        // 1. Pastikan UI Aktif DULU sebelum modifikasi agar RectTransform terbaca benar
        ShowTimeline(true);

        ClearTimeline();

        for (int i = 0; i < command.Length; i++)
        {
            GameObject prefabToSpawn = null;
            if (command[i] == "A") prefabToSpawn = prefabA;
            else if (command[i] == "Y" || command[i] == "W") prefabToSpawn = prefabY;

            if (prefabToSpawn != null)
            {
                GameObject note = Instantiate(prefabToSpawn, noteSpawnPoint);
                RectTransform rt = note.GetComponent<RectTransform>();

                // Set posisi menggunakan noteYOffset yang sudah stabil
                rt.anchoredPosition = new Vector2(i * _noteSpacing, noteYOffset);
                _activeNotes.Add(note);
            }
        }
    }

    public void ClearTimeline()
    {
        foreach (GameObject n in _activeNotes) Destroy(n);
        _activeNotes.Clear();
        _currentProgress = 0;
        _targetProgress = 0;
        cursor.anchoredPosition = new Vector2(0, cursorYOffset);
    }

    void Update()
    {
        // SmoothDamp tetap dipertahankan agar pergerakan kursor tidak patah-patah
        if (canvasTimeline != null && canvasTimeline.activeSelf)
        {
            _currentProgress = Mathf.SmoothDamp(_currentProgress, _targetProgress, ref _smoothVelocity, 0.05f);
            float targetX = _currentProgress * (6 * _noteSpacing);
            cursor.anchoredPosition = new Vector2(targetX, cursorYOffset);
        }
    }

    public void UpdateCursor(float progress)
    {
        _targetProgress = progress;
    }
}