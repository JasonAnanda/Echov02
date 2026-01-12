using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TimelineController : MonoBehaviour
{
    #region 1. UI & PREFAB REFERENCES
    public RectTransform cursor;
    public Transform noteSpawnPoint;
    public GameObject prefabA; // Untuk BA
    public GameObject prefabY; // Untuk BY
    public GameObject canvasTimeline;
    #endregion

    #region 2. POSITION & BPM SETTINGS
    [Header("Full Screen Timeline Settings")]
    public float cursorStartX = -1000f; // Sesuai permintaan (Gambar 3)
    public float cursorEndX = 1000f;   // Sesuai permintaan (Gambar 3)

    [Tooltip("Jika diisi 0, maka akan otomatis menghitung berdasarkan BPM 120 (3 Detik)")]
    public float cursorDuration = 0f;

    [Header("Visual Alignment")]
    public float noteYOffset = 0f;
    public float cursorYOffset = -442f; // Sesuai permintaan (Gambar 3)
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

            // Area input sekarang aktif selama kursor bergerak melintasi layar (Full Screen)
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

    public void SpawnPattern(string[] command)
    {
        ClearTimeline();

        float totalWidth = Mathf.Abs(cursorEndX - cursorStartX);

        // Membagi rata posisi berdasarkan 6 beat (BPM 120) di lebar 2000 unit
        float step = totalWidth / command.Length;

        for (int i = 0; i < command.Length; i++)
        {
            GameObject prefabToSpawn = null;
            if (command[i] == "BA") prefabToSpawn = prefabA;
            else if (command[i] == "BY") prefabToSpawn = prefabY;

            if (prefabToSpawn != null)
            {
                GameObject note = Instantiate(prefabToSpawn, noteSpawnPoint);
                RectTransform rt = note.GetComponent<RectTransform>();

                // Posisi Diamond/Kotak disebar presisi di sepanjang lintasan kursor
                float posX = cursorStartX + (i * step) + (step / 2f);
                rt.anchoredPosition = new Vector2(posX, noteYOffset);

                _activeNotes.Add(note);
                _noteSlots.Add(i, note);
            }
        }
    }
    #endregion

    #region 6. RHYTHM INTERFACE (Updated)
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

    // UPDATE: Menangani durasi otomatis untuk sinkronisasi BPM 120
    public void StartManualMovement(float overrideDuration = 0)
    {
        _startTime = Time.time;

        // HITUNG DURASI: Untuk 120 BPM pada 6 Beat, durasi ideal adalah 3.0 detik
        if (overrideDuration <= 0 && cursorDuration <= 0)
        {
            _duration = 3.0f;
        }
        else
        {
            _duration = (overrideDuration > 0) ? overrideDuration : cursorDuration;
        }

        _isMoving = true;
    }
    #endregion
}