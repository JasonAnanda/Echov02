using UnityEngine;
using System.Collections.Generic;

public class TargetSelector : MonoBehaviour
{
    private List<GameObject> _enemies = new List<GameObject>();
    private int _currentIndex = -1;

    void Update()
    {
        // 1. Ambil semua monster yang ada di scene (yang memiliki tag "Enemy")
        GameObject[] foundEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        // 2. Deteksi Input D-Pad (Left/Right)
        // Horizontal pada D-Pad biasanya terbaca sebagai Axis
        float dPadInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Space)) // Tombol A untuk Lock-On
        {
            ConfirmSelection();
        }

        // Logika pindah target (Sederhana)
        if (Input.GetKeyDown(KeyCode.RightArrow) || dPadInput > 0.5f)
        {
            SwitchTarget(1, foundEnemies);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || dPadInput < -0.5f)
        {
            SwitchTarget(-1, foundEnemies);
        }
    }

    void SwitchTarget(int direction, GameObject[] foundEnemies)
    {
        if (foundEnemies.Length == 0) return;

        _currentIndex = Mathf.Clamp(_currentIndex + direction, 0, foundEnemies.Length - 1);
        GlobalData.currentTarget = foundEnemies[_currentIndex];

        Debug.Log("Targeting: " + GlobalData.currentTarget.name);
    }

    void ConfirmSelection()
    {
        if (GlobalData.currentTarget != null)
        {
            MonsterLogic ml = GlobalData.currentTarget.GetComponent<MonsterLogic>();
            if (ml != null && ml.currentPhase == MonsterLogic.Phase.Wait)
            {
                // Memulai Fase Demo seperti di Python
                ml.StartSequence();
            }
        }
    }
}