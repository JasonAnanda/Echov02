using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TimelineController : MonoBehaviour
{
    public RectTransform cursor;
    public Transform noteSpawnPoint;
    public GameObject prefabA;
    public GameObject prefabY;

    [Tooltip("Pastikan ini adalah Parent Utama dari seluruh elemen Timeline (Bar, Diamond, Cursor)")]
    public GameObject canvasTimeline;

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
        // Sembunyikan saat awal
        ShowTimeline(false);
    }

    public void ShowTimeline(bool show)
    {
        if (canvasTimeline != null)
        {
            canvasTimeline.SetActive(show);

            // PAKSA Canvas untuk merender ulang (Rebuild) agar tidak "ghosting" atau tetap transparan
            if (show) LayoutRebuilder.ForceRebuildLayoutImmediate(canvasTimeline.GetComponent<RectTransform>());

            Debug.Log("<color=green>Timeline Logic:</color> Show is " + show);
        }
        else
        {
            Debug.LogError("<color=red>Timeline Error:</color> Slot 'Canvas Timeline' di Inspector masih KOSONG!");
        }
    }

    public void SpawnPattern(string[] command)
    {
        // Panggil ShowTimeline di awal agar instruksi Instantiate di bawahnya memiliki konteks Parent yang aktif
        ShowTimeline(true);

        ClearTimeline();

        for (int i = 0; i < command.Length; i++)
        {
            GameObject prefabToSpawn = null;
            if (command[i] == "A") prefabToSpawn = prefabA;
            else if (command[i] == "Y" || command[i] == "W") prefabToSpawn = prefabY;

            if (prefabToSpawn != null)
            {
                // Gunakan noteSpawnPoint sebagai parent
                GameObject note = Instantiate(prefabToSpawn, noteSpawnPoint);
                RectTransform rt = note.GetComponent<RectTransform>();

                // Pastikan Scale kembali ke 1,1,1 (Seringkali Instantiate di UI merubah scale jadi aneh)
                rt.localScale = Vector3.one;
                rt.anchoredPosition = new Vector2(i * _noteSpacing, noteYOffset);
                _activeNotes.Add(note);
            }
        }
    }

    public void ClearTimeline()
    {
        foreach (GameObject n in _activeNotes) if (n != null) Destroy(n);
        _activeNotes.Clear();
        _currentProgress = 0;
        _targetProgress = 0;
        if (cursor != null) cursor.anchoredPosition = new Vector2(0, cursorYOffset);
    }

    void Update()
    {
        if (canvasTimeline != null && canvasTimeline.activeInHierarchy)
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