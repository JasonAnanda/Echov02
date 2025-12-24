using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TimelineController : MonoBehaviour
{
    public RectTransform cursor;
    public Transform noteSpawnPoint;
    public GameObject prefabA;
    public GameObject prefabY;

    // --- TAMBAHAN UNTUK KALIBRASI POSISI ---
    [Header("Visual Alignment")]
    public float yOffset = 0f;
    // -----------------------------------------------------------

    private float _noteSpacing = 120f; // Sesuai NOTE_SPACING di Python
    private List<GameObject> _activeNotes = new List<GameObject>();

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

                // Gunakan yOffset agar diamond turun ke arah garis
                rt.anchoredPosition = new Vector2(i * _noteSpacing, yOffset);
                _activeNotes.Add(note);
            }
        }
    }

    public void ClearTimeline()
    {
        foreach (GameObject n in _activeNotes) Destroy(n);
        _activeNotes.Clear();

        // Reset kursor ke posisi awal dengan menghormati yOffset
        cursor.anchoredPosition = new Vector2(0, yOffset);
    }

    public void UpdateCursor(float progress)
    {
        float targetX = progress * (6 * _noteSpacing);

        // Gunakan yOffset agar kursor sejajar dengan diamond dan garis
        cursor.anchoredPosition = new Vector2(targetX, yOffset);
    }
}