using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PollenProjectile : MonoBehaviour
{
    public Vector3 m_direction;
    public float m_speed;
    public float m_duration;

    private float startTime;

    public void SetUp(Vector3 position, Vector3 direction, float speed, float duration)
    {
        transform.position = position;
        m_direction = direction;
        m_speed = speed;
        m_duration = duration;
    }

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        transform.position += m_direction * m_speed * Time.deltaTime;
        if (Time.time - startTime >= m_duration)
        {
            Destroy(gameObject);
        }
    }
}
