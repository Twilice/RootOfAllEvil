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
    public int startHp = 3;
    public float dieTimer = 3;
    public float shakeMagnitude = 0.1f;
    public WatchPlayer watchPlayer;
    
    private float timer;

    private int _hp = 1;
    private bool dying;
    
    public int HP
    {
        get { return _hp; }
        set
        {
            if (dying)
            {
                return;
            }
            _hp = value;
            if (_hp < 0)
            {
                Die();
            }
            else
            {
                TakeDamage();
            }
        }
    }

    void Start()
    {
        timer = interval;
        _hp = startHp;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0 && !dying)
        {
            timer = interval;
            for (int i = 0; i < numberOfSpores; i++)
            {
                float angle = i * (spread / numberOfSpores) - spread / 2;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                var projectile = Instantiate(spore);
                projectile.SetUp(transform.position, direction, speed, duration);
            }
        }
    }

    public void TakeDamage()
    {
        watchPlayer.DartEyes(0.5f);
        StartCoroutine(Shake(0.4f));
    }
    
    public void Die()
    {
        watchPlayer.DartEyes(dieTimer);
        StartCoroutine(Shake(dieTimer - 0.3f));
        dying = true;
        Destroy(this.gameObject, dieTimer);
    }

    private IEnumerator Shake(float time)
    {
        var timer = time;
        var originalPosition = transform.localPosition;
        
        var shakeX = Random.Range(-1f, 1f);
        var shakeZ = Random.Range(-1f, 1f);
        while (timer >= 0)
        {
            shakeX = Random.Range(-1f, 1f);
            shakeZ = Random.Range(-1f, 1f);
            transform.localPosition = originalPosition + new Vector3(shakeX, 0, shakeZ) * shakeMagnitude * Time.deltaTime;
            yield return new WaitForEndOfFrame();
            timer -= Time.deltaTime;
        }

        transform.localPosition = originalPosition;
    }
}
