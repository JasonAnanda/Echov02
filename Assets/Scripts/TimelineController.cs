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

    #region 2. POSITION & SPACING SETTINGS
    [Header("Manual Cursor Position")]
    public float cursorStartX = -450f;
    public float cursorEndX = 450f;

    [Header("Visual Alignment")]
    public float noteYOffset = 0f;
    public float cursorYOffset = 0f;

    private float _noteSpacing = 120f;
    #endregion

    #region 3. INTERNAL DATA & STATE
    private float _currentProgress = 0f;
    private GameObject[] _noteSlots = new GameObject[6];
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
            // Menghitung progress linear berdasarkan waktu nyata (Time.time)
            float elapsed = Time.time - _startTime;
            _currentProgress = Mathf.Clamp01(elapsed / _duration);

            // Pergerakan visual kursor
            float targetX = Mathf.Lerp(cursorStartX, cursorEndX, _currentProgress);
            cursor.anchoredPosition = new Vector2(targetX, cursorYOffset);

            if (_currentProgress >= 1f) _isMoving = false;
        }
    }
    #endregion

    #region 5. TIMELINE CONTROL METHODS
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
                if (i < _noteSlots.Length) _noteSlots[i] = note;
            }
        }
    }
    #endregion

    #region 6. RHYTHM INTERFACE (Input & Progress)
    // Digunakan oleh PlayerInputHandler untuk cek presisi
    public float GetCurrentProgress()
    {
        return _currentProgress;
    }

    // Digunakan oleh MonsterLogic untuk feedback visual sukses
    public void MarkDiamondHit(int index)
    {
        if (index >= 0 && index < _noteSlots.Length && _noteSlots[index] != null)
        {
            Image img = _noteSlots[index].GetComponent<Image>();
            if (img != null)
            {
                img.color = Color.green; // Diamond berubah hijau saat kena hit
            }
            _noteSlots[index].transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }
    }

    // Dipanggil oleh MonsterLogic saat masuk fase USER
    public void StartManualMovement(float totalDuration)
    {
        _startTime = Time.time;
        _duration = totalDuration; // Durasi sesuai beatInterval * 6 beat
        _isMoving = true;
    }
    #endregion

    #region 7. LEGACY / UNUSED
    public void UpdateCursor(float progress) { }
    #endregion
}