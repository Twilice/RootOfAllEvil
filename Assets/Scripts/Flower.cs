using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour
{
    public PollenProjectile spore;
    public int numberOfSpores = 5;
    public float interval = 1.0f;
    public float spread = 360.0f;
    public float speed = 10.0f;
    public float duration = 2.0f;

    private float timer;

    private int _hp = 1;
    public int HP
    {
        get { return _hp; }
        set
        {
            _hp = value;
            if (_hp < 0)
            {
                Destroy(gameObject);
            }
        }
    }

    void Start()
    {
        timer = interval;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = interval;
            for (int i = 0; i < numberOfSpores; i++)
            {
                float angle = i * (spread / numberOfSpores) - spread / 2;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                var projectile = Instantiate(spore);
                spore.SetUp(transform.position, direction, speed, duration);
            }
        }
    }
}
