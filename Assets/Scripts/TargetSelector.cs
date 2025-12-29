using UnityEngine;
using System.Collections.Generic;

public class TargetSelector : MonoBehaviour
{
    private int _currentIndex = -1;
    private bool _isDPadPressed = false;

    void Update()
    {
        float dPadInput = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(dPadInput) > 0.1f)
        {
            Debug.Log($"<color=orange>D-Pad Terdeteksi:</color> Value: {dPadInput} | Axis: Horizontal");
        }

        // --- UPDATE: Hanya ambil musuh yang belum dikalahkan (isDefeated == false) ---
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        List<GameObject> validEnemies = new List<GameObject>();

        foreach (GameObject go in allEnemies)
        {
            MonsterLogic ml = go.GetComponent<MonsterLogic>();
            // Cek jika monster ada dan belum tuntas
            if (ml != null && !ml.isDefeated)
            {
                validEnemies.Add(go);
            }
        }
        // ----------------------------------------------------------------------------

        if (Input.GetKeyDown(KeyCode.RightArrow) || dPadInput > 0.5f)
        {
            if (!_isDPadPressed)
            {
                SwitchTarget(1, validEnemies.ToArray()); // Gunakan list yang sudah difilter
                _isDPadPressed = true;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || dPadInput < -0.5f)
        {
            if (!_isDPadPressed)
            {
                SwitchTarget(-1, validEnemies.ToArray()); // Gunakan list yang sudah difilter
                _isDPadPressed = true;
            }
        }
        else
        {
            _isDPadPressed = false;
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelection();
        }
    }

    void SwitchTarget(int direction, GameObject[] foundEnemies)
    {
        if (foundEnemies.Length == 0)
        {
            Debug.LogWarning("<color=red>TargetSelector:</color> Tidak ada musuh aktif yang bisa dipilih!");
            GlobalData.currentTarget = null; // Reset target jika tidak ada yang valid
            return;
        }

        List<GameObject> sortedEnemies = new List<GameObject>(foundEnemies);
        sortedEnemies.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        // Pastikan currentIndex tidak out of bounds setelah filter
        _currentIndex = Mathf.Clamp(_currentIndex + direction, 0, sortedEnemies.Count - 1);
        GlobalData.currentTarget = sortedEnemies[_currentIndex];

        // Feedback Visual: Reset semua musuh yang ada di scene (termasuk yang tuntas agar tidak salah skala)
        GameObject[] allSceneEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in allSceneEnemies)
        {
            if (enemy != null) enemy.transform.localScale = Vector3.one;
        }

        if (GlobalData.currentTarget != null)
        {
            GlobalData.currentTarget.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            Debug.Log($"<color=yellow>Targeting:</color> {GlobalData.currentTarget.name} (Index: {_currentIndex})");
        }
    }

    void ConfirmSelection()
    {
        if (GlobalData.currentTarget != null)
        {
            MonsterLogic ml = GlobalData.currentTarget.GetComponent<MonsterLogic>();

            // --- UPDATE: Tambahkan pengecekan !ml.isDefeated ---
            if (ml != null && ml.currentState == MonsterLogic.MonsterState.WAIT && !ml.isDefeated)
            {
                Debug.Log("<color=green>Confirm Selection:</color> Memulai Sequence " + GlobalData.currentTarget.name);
                ml.StartSequence();
            }
        }
        else
        {
            Debug.Log("<color=red>Confirm Failed:</color> Belum ada target aktif yang dipilih!");
        }
    }
}