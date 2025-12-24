using UnityEngine;
using System.Collections.Generic;

public class TargetSelector : MonoBehaviour
{
    private int _currentIndex = -1;
    private bool _isDPadPressed = false;

    void Update()
    {
        // 1. Ambil input Axis dari Input Manager (6th Axis)
        float dPadInput = Input.GetAxisRaw("Horizontal");

        // --- TAMBAHAN DEBUGGING INPUT ---
        // Muncul di Console hanya jika ada nilai input yang masuk (melebihi deadzone 0.1)
        if (Mathf.Abs(dPadInput) > 0.1f)
        {
            Debug.Log($"<color=orange>D-Pad Terdeteksi:</color> Value: {dPadInput} | Axis: Horizontal");
        }
        // --------------------------------

        // Mencari semua monster dengan Tag "Enemy"
        GameObject[] foundEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Logika Navigasi D-Pad
        if (Input.GetKeyDown(KeyCode.RightArrow) || dPadInput > 0.5f)
        {
            if (!_isDPadPressed)
            {
                SwitchTarget(1, foundEnemies);
                _isDPadPressed = true;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || dPadInput < -0.5f)
        {
            if (!_isDPadPressed)
            {
                SwitchTarget(-1, foundEnemies);
                _isDPadPressed = true;
            }
        }
        else
        {
            _isDPadPressed = false;
        }

        // Tombol A Joystick (Button 0) atau Spasi untuk Confirm
        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelection();
        }
    }

    void SwitchTarget(int direction, GameObject[] foundEnemies)
    {
        if (foundEnemies.Length == 0)
        {
            Debug.LogWarning("<color=red>TargetSelector:</color> Tidak ada objek dengan Tag 'Enemy' ditemukan!");
            return;
        }

        // Urutkan musuh berdasarkan posisi X agar navigasi logis
        List<GameObject> sortedEnemies = new List<GameObject>(foundEnemies);
        sortedEnemies.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        _currentIndex = Mathf.Clamp(_currentIndex + direction, 0, sortedEnemies.Count - 1);
        GlobalData.currentTarget = sortedEnemies[_currentIndex];

        // Feedback Visual
        foreach (GameObject enemy in sortedEnemies)
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

            if (ml != null && ml.currentState == MonsterLogic.MonsterState.WAIT)
            {
                Debug.Log("<color=green>Confirm Selection:</color> Memulai Sequence " + GlobalData.currentTarget.name);
                ml.StartSequence();
            }
        }
        else
        {
            Debug.Log("<color=red>Confirm Failed:</color> Belum ada target yang dipilih!");
        }
    }
}