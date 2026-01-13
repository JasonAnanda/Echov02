using UnityEngine;
using System.Collections.Generic;

public class TargetSelector : MonoBehaviour
{
    #region 1. INTERNAL STATE
    private int _currentIndex = -1;
    private bool _isDPadPressed = false;
    #endregion

    void Update()
    {
        float dPadInput = Input.GetAxisRaw("Horizontal");

        // --- FILTER MUSUH AKTIF ---
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        List<GameObject> validEnemies = new List<GameObject>();

        foreach (GameObject go in allEnemies)
        {
            MonsterLogic ml = go.GetComponent<MonsterLogic>();
            if (ml != null && !ml.isDefeated)
            {
                validEnemies.Add(go);
            }
        }

        // --- INPUT DETECTION (Direct Execution) ---
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || dPadInput > 0.5f)
        {
            if (!_isDPadPressed)
            {
                SwitchAndExecute(1, validEnemies.ToArray());
                _isDPadPressed = true;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || dPadInput < -0.5f)
        {
            if (!_isDPadPressed)
            {
                SwitchAndExecute(-1, validEnemies.ToArray());
                _isDPadPressed = true;
            }
        }
        else
        {
            _isDPadPressed = false;
        }

        // Bagian ConfirmSelection (Space/JoystickButton0) dihapus agar tidak perlu tekan dua kali
    }

    void SwitchAndExecute(int direction, GameObject[] foundEnemies)
    {
        if (foundEnemies.Length == 0)
        {
            Debug.LogWarning("<color=red>TargetSelector:</color> Tidak ada musuh aktif!");
            GlobalData.currentTarget = null;
            return;
        }

        List<GameObject> sortedEnemies = new List<GameObject>(foundEnemies);
        sortedEnemies.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        // Logic pemilihan index
        _currentIndex = Mathf.Clamp(_currentIndex + direction, 0, sortedEnemies.Count - 1);
        GlobalData.currentTarget = sortedEnemies[_currentIndex];

        // --- FEEDBACK VISUAL ---
        GameObject[] allSceneEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in allSceneEnemies)
        {
            if (enemy != null) enemy.transform.localScale = Vector3.one;
        }

        if (GlobalData.currentTarget != null)
        {
            GlobalData.currentTarget.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            // --- UPDATE: LANGSUNG EKSEKUSI TANPA CONFIRM ---
            MonsterLogic ml = GlobalData.currentTarget.GetComponent<MonsterLogic>();
            if (ml != null && ml.currentState == MonsterLogic.MonsterState.WAIT && !ml.isDefeated)
            {
                Debug.Log($"<color=green>Direct Execution:</color> Memulai {GlobalData.currentTarget.name}");
                ml.StartSequence();
            }
        }
    }
}