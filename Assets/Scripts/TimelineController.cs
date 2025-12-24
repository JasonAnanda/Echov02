using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TimelineController : MonoBehaviour
{
    public RectTransform cursor;
    public Transform noteSpawnPoint;
    public GameObject prefabA;
    public GameObject prefabY;

    [Header("Visual Alignment")]
    public float noteYOffset = 0f;    // Offset untuk Diamond/Notes
    public float cursorYOffset = 0f;  // Offset khusus untuk Kursor

    private float _noteSpacing = 120f;
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

                // Diamond menggunakan noteYOffset
                rt.anchoredPosition = new Vector2(i * _noteSpacing, noteYOffset);
                _activeNotes.Add(note);
            }
        }
    }

    public void ClearTimeline()
    {
        foreach (GameObject n in _activeNotes) Destroy(n);
        _activeNotes.Clear();

        // Reset kursor ke awal dengan cursorYOffset
        cursor.anchoredPosition = new Vector2(0, cursorYOffset);
    }

    public void UpdateCursor(float progress)
    {
        float targetX = progress * (6 * _noteSpacing);

        // Kursor menggunakan cursorYOffset agar tidak melayang
        cursor.anchoredPosition = new Vector2(targetX, cursorYOffset);
    }
}