using UnityEngine;

public class SimpleBounce : MonoBehaviour
{
    public float bounceAmount = 0.2f; // Jarak anggukan
    public float speed = 4f;        // Kecepatan anggukan
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Logika PingPong mirip dengan step % 2 di Python
        float newY = Mathf.PingPong(Time.time * speed, bounceAmount);
        transform.position = new Vector3(startPos.x, startPos.y + newY, startPos.z);
    }
}