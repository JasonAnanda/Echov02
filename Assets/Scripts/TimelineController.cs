using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TimelineController : MonoBehaviour
{
    #region 1. UI & PREFAB REFERENCES
    public RectTransform cursor;
    public Transform noteSpawnPoint;
    public GameObject prefabA;
    public GameObject prefabY;
    public GameObject canvasTimeline;
    #endregion

    #region 2. POSITION & BPM SETTINGS
    [Header("Full Screen Timeline Settings")]
    public float cursorStartX = -1000f;
    public float cursorEndX = 1000f;

    [Tooltip("Jika diisi 0, maka akan otomatis menghitung berdasarkan BPM Musik")]
    public float cursorDuration = 4.0f;

    [Header("Visual Alignment & Balance")]
    public float noteYOffset = 0f;
    public float cursorYOffset = -442f;

    [Tooltip("Gunakan nilai positif untuk menggeser Diamond lebih ke kanan")]
    public float visualOffset = 130f;
    #endregion

    #region 3. INTERNAL DATA & STATE
    private float _currentProgress = 0f;
    private Dictionary<int, GameObject> _noteSlots = new Dictionary<int, GameObject>();
    private List<GameObject> _activeNotes = new List<GameObject>();

    private float _startTime;
    private float _duration;
    private bool _isMoving = false;
    #endregion

    #region 4. UNITY CALLBACKS
    void Start()
    {
        if (canvasTimeline != null) canvasTimeline.SetActive(true);
        ResetCursor();
    }

    void Update()
    {
        if (_isMoving && cursor != null)
        {
            float elapsed = Time.time - _startTime;
            _currentProgress = Mathf.Clamp01(elapsed / _duration);

            float targetX = Mathf.Lerp(cursorStartX, cursorEndX, _currentProgress);
            cursor.anchoredPosition = new Vector2(targetX, cursorYOffset);

            if (_currentProgress >= 1f)
            {
                _isMoving = false;
                ResetCursor();
            }
        }
    }
    #endregion

    #region 5. TIMELINE CONTROL METHODS
    private void ResetCursor()
    {
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
        _noteSlots.Clear();
        ResetCursor();
    }

    // UPDATE: Logika Spawn yang diperbaiki untuk memastikan Diamond 0f terlihat
    public void SpawnPatternRhythmDoctor(float[] beatTimings, string[] types)
    {
        ClearTimeline();

        float totalWidth = Mathf.Abs(cursorEndX - cursorStartX);
        float unitPerBeat = totalWidth / 4f;

        for (int i = 0; i < beatTimings.Length; i++)
        {
            GameObject prefabToSpawn = (types[i] == "A") ? prefabA : prefabY;
            if (prefabToSpawn != null)
            {
                GameObject note = Instantiate(prefabToSpawn, noteSpawnPoint);
                RectTransform rt = note.GetComponent<RectTransform>();

                // FIX 1: Memaksa Anchor dan Pivot ke tengah agar posisi absolut konsisten
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.localScale = Vector3.one;

                // FIX 2: Perhitungan posisi menggunakan visualOffset yang sudah disesuaikan
                float posX = cursorStartX + (beatTimings[i] * unitPerBeat) + visualOffset;

                // Set posisi UI Diamond
                rt.anchoredPosition = new Vector2(posX, noteYOffset);

                _activeNotes.Add(note);

                // Simpan ke slot untuk deteksi hit
                if (!_noteSlots.ContainsKey(i))
                    _noteSlots.Add(i, note);
            }
        }
    }
    #endregion

    #region 6. RHYTHM INTERFACE
    public float GetCurrentProgress()
    {
        return _currentProgress;
    }

    public void MarkDiamondHit(int index)
    {
        if (_noteSlots.ContainsKey(index) && _noteSlots[index] != null)
        {
            Image img = _noteSlots[index].GetComponent<Image>();
            if (img != null) img.color = Color.green;
            _noteSlots[index].transform.localScale = Vector3.one * 1.2f;
        }
    }

    public void StartManualMovement(float overrideDuration = 0)
    {
        _startTime = Time.time;

        if (overrideDuration <= 0)
        {
            _duration = (cursorDuration <= 0) ? 2.0f : cursorDuration;
        }
        else
        {
            _duration = overrideDuration;
        }

        _isMoving = true;
    }
    #endregion
}