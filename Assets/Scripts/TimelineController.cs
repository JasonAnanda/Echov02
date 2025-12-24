using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TimelineController : MonoBehaviour
{
    public RectTransform cursor;
    public Transform noteSpawnPoint;
    public GameObject prefabA;
    public GameObject prefabY;

    private float _noteSpacing = 120f; // Sesuai NOTE_SPACING di Python
    private List<GameObject> _activeNotes = new List<GameObject>();

    // Fungsi untuk memunculkan visual pola (Dipanggil saat Monster PHASE_DEMO)
    public void SpawnPattern(string[] command)
    {
        ClearTimeline();

        for (int i = 0; i < command.Length; i++)
        {
            GameObject prefabToSpawn = null;
            if (command[i] == "A") prefabToSpawn = prefabA;
            else if (command[i] == "W") prefabToSpawn = prefabY; // W di Python jadi Y

            if (prefabToSpawn != null)
            {
                GameObject note = Instantiate(prefabToSpawn, noteSpawnPoint);
                RectTransform rt = note.GetComponent<RectTransform>();

                // Atur posisi horizontal berdasarkan index pola
                rt.anchoredPosition = new Vector2(i * _noteSpacing, 0);
                _activeNotes.Add(note);
            }
        }
    }

    public void ClearTimeline()
    {
        foreach (GameObject n in _activeNotes) Destroy(n);
        _activeNotes.Clear();
        cursor.anchoredPosition = Vector2.zero; // Reset kursor ke awal
    }

    public void UpdateCursor(float progress)
    {
        // Progress adalah 0.0 sampai 1.0 (satu putaran pola)
        // Total panjang bar = jumlah beat * spacing
        float targetX = progress * (6 * _noteSpacing);
        cursor.anchoredPosition = new Vector2(targetX, 0);
    }
}