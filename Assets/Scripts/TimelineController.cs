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
    // Kamu bisa mengatur nilai ini di Inspector untuk menentukan posisi kursor saat mulai dan selesai
    public float cursorStartX = -450f; // Titik paling kiri timeline kamu
    public float cursorEndX = 450f;   // Titik paling kanan timeline kamu

    [Header("Visual Alignment")]
    public float noteYOffset = 0f;
    public float cursorYOffset = 0f;

    private float _currentProgress = 0f;
    private float _targetProgress = 0f;
    private float _smoothVelocity = 0f;
    private float _noteSpacing = 120f;
    private List<GameObject> _activeNotes = new List<GameObject>();

    void Start()
    {
        if (canvasTimeline != null)
        {
            canvasTimeline.SetActive(true);
        }
        ResetCursor();
    }

    private void ResetCursor()
    {
        if (cursor != null)
        {
            _currentProgress = 0f;
            _targetProgress = 0f;
            _smoothVelocity = 0f;

            // Mengatur posisi kursor ke nilai Start yang kamu tentukan di Inspector
            cursor.anchoredPosition = new Vector2(cursorStartX, cursorYOffset);
        }
    }

    public void ClearTimeline()
    {
        foreach (GameObject n in _activeNotes) if (n != null) Destroy(n);
        _activeNotes.Clear();
        ResetCursor();
    }

    public void ShowTimeline(bool show)
    {
        if (canvasTimeline != null) canvasTimeline.SetActive(true);
    }

    public void SpawnPattern(string[] command)
    {
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
                rt.localScale = Vector3.one;
                // Posisi Note tetap relatif terhadap NoteSpawnPoint
                rt.anchoredPosition = new Vector2(i * _noteSpacing, noteYOffset);
                _activeNotes.Add(note);
            }
        }
    }

    void Update()
    {
        if (cursor != null)
        {
            _currentProgress = Mathf.SmoothDamp(_currentProgress, _targetProgress, ref _smoothVelocity, 0.05f);

            // MENGGUNAKAN LERP: Menghitung posisi kursor di antara Start dan End berdasarkan progres
            float targetX = Mathf.Lerp(cursorStartX, cursorEndX, _currentProgress);
            cursor.anchoredPosition = new Vector2(targetX, cursorYOffset);
        }
    }

    public void UpdateCursor(float progress)
    {
        _targetProgress = progress;
    }
}