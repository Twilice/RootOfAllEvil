using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PollenProjectile : MonoBehaviour
{
    public Vector3 m_direction;
    public float m_speed;
    public float m_duration;
    public SphereCollider collider;
    public Transform pollenTransform;

    private float startTime;
    private Vector3 originalSporePos;
    private bool shake = false;

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

        Collider[] colliders = Physics.OverlapSphere(collider.bounds.center, collider.bounds.extents.magnitude);

        // Check if there are any colliders within the given collider
        if (colliders.Length > 0)
        {
            foreach (var c in colliders)
            {
                if (c.gameObject.tag == "Player")
                {
                    c.GetComponentInParent<GardenerController>().HP -= 1;
                    Destroy(gameObject);
                }
            }
        }

        if (!shake)
        {
            StartCoroutine(FlowShake(5f, 2, 2f));
        }

        if (Time.time - startTime >= m_duration)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator FlowShake(float duration, float sinusSpeed, float magnitude)
    {
        shake = true;
        originalSporePos = pollenTransform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Mathf.Sin(elapsed*sinusSpeed) * magnitude;
            //float y = Random.Range(-1f, 1f) * magnitude;

            pollenTransform.localPosition = new Vector3(originalSporePos.x + x, originalSporePos.y, originalSporePos.z + x);

            elapsed += Time.deltaTime;

            yield return null;
        }

        pollenTransform.localPosition = originalSporePos;
        shake = false;
    }
}
