using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class WatchPlayer : MonoBehaviour
{
    public float maxRotationZ = 50;
    public float maxRotationX = 40;

    public float intervalBetweenEyeDarts = 0.2f;
    public float eyeDartSpeed = 5;
    
    private GardenerController gardener;
    private bool darting;

    private float dartTime;
    
    public void DartEyes(float time)
    {
        if (!darting)
        {
            darting = true;
            dartTime = time;
            StartCoroutine(DartEyesCoroutine());
        }
        else
        {
            dartTime = time;
        }
    }

    public void StopDart()
    {
        darting = false;
        dartTime = 0;
    }
    
    private void Start()
    {
        gardener = GameCoordinator.Instance.gardenerInstance;
    }

    void Update()
    {
        if (gardener == null || darting) return;
        var loc = transform.position;
        var ploc = gardener.transform.position;
        float angleBetweenEyeAndPlayer = Mathf.Atan2(loc.z - ploc.z, loc.x - ploc.x);
        transform.rotation = AngleToEdge(angleBetweenEyeAndPlayer);
    }

    private Quaternion AngleToEdge(float angle)
    {
        return Quaternion.Euler(maxRotationX * Mathf.Cos(angle + Mathf.PI / 2), 0, maxRotationZ * Mathf.Sin(angle + Mathf.PI / 2));
    }
    
    IEnumerator DartEyesCoroutine()
    {
        float dartDirectionTimer = intervalBetweenEyeDarts;
        float dartAngle = Random.Range(0, 2 * Mathf.PI);
        while (darting)
        {
            if (dartDirectionTimer < 0)
            {
                dartDirectionTimer = intervalBetweenEyeDarts;
                dartAngle = Random.Range(0, 2 * Mathf.PI);
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, AngleToEdge(dartAngle), eyeDartSpeed*Time.deltaTime);
            
            yield return new WaitForEndOfFrame();
            dartDirectionTimer -= Time.deltaTime;
            dartTime -= Time.deltaTime;
            darting = dartTime >= 0;
        }
    }
    
}