using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    private float cameraShakeTimer = 0;
    private bool shaking = false;
    private float magnitude = 0;
    private void Start()
    {
        player = GameCoordinator.Instance.gardenerInstance.transform;
    }
    void LateUpdate()
    {
        if (player == null || shaking) return;
        Vector3 desiredPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    public void ShakeCamera(float time, float magnitude)
    {
        if (cameraShakeTimer <= 0)
        {
            cameraShakeTimer = time;
            this.magnitude = magnitude;
            StartCoroutine(ScreenShake());
        }
        else if(magnitude > this.magnitude)
        {
            cameraShakeTimer = time;
            this.magnitude = magnitude;
        }
    }
    private IEnumerator ScreenShake()
    {
        shaking = true;
        while (cameraShakeTimer > 0)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = player.position + offset + new Vector3(x, y, 0);

            cameraShakeTimer -= Time.deltaTime;

            yield return null;
        }

        shaking = false;
    }
}
