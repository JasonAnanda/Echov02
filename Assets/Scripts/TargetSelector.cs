using UnityEngine;
using System.Collections.Generic;

public class TargetSelector : MonoBehaviour
{
    private int _currentIndex = -1;

    void Update()
    {
        // Mencari semua monster dengan Tag "Enemy"
        GameObject[] foundEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        float dPadInput = Input.GetAxisRaw("Horizontal");

        // Tombol A Joystick atau Spasi untuk Confirm
        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelection();
        }

        // Navigasi target menggunakan D-Pad atau Keyboard Arrow
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

        // Memastikan index tidak keluar dari jumlah musuh yang ada
        _currentIndex = Mathf.Clamp(_currentIndex + direction, 0, foundEnemies.Length - 1);
        GlobalData.currentTarget = foundEnemies[_currentIndex];

        Debug.Log("Targeting: " + GlobalData.currentTarget.name);
    }

    void ConfirmSelection()
    {
        if (GlobalData.currentTarget != null)
        {
            MonsterLogic ml = GlobalData.currentTarget.GetComponent<MonsterLogic>();

            // Menggunakan currentState dan MonsterState sesuai script MonsterLogic stabil
            if (ml != null && ml.currentState == MonsterLogic.MonsterState.WAIT)
            {
                ml.StartSequence();
            }
        }
    }
}