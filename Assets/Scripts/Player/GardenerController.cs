using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GardenerController : MonoBehaviour
{
    // Publics
    public Transform m_body;
    public float m_moveSpeed = 10f;
    public float m_sensitivity = 10f;
    public float m_swingSpeed = 0.25f;
    public float m_rotationAngle = 90f;
    public Transform m_axePivot;
    
    // Privates
    private Quaternion originalRotation;
    private bool rotateBack = false;


    // Start is called before the first frame update
    void Start()
    {
        m_axePivot.localEulerAngles = new Vector3(0f, -(m_rotationAngle / 2f), 0f);
        originalRotation = m_axePivot.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        //Movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        transform.position = transform.position + new Vector3(horizontal, 0, vertical) * m_moveSpeed * Time.deltaTime;

        //Rotation
        Vector3 mousePos = Input.mousePosition;
        Vector3 screenPoint = Camera.main.ScreenToViewportPoint(mousePos);
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(screenPoint.x - 0.5f, 0f, screenPoint.y - 0.5f));
        m_body.rotation = Quaternion.Slerp(m_body.rotation, targetRotation, m_sensitivity * Time.deltaTime);


        // Swing
        if (m_axePivot.localRotation == originalRotation && Input.GetKeyDown(KeyCode.Space))
        {
            rotateBack = false;
            StartCoroutine(SmoothRotate(m_rotationAngle));
        }
    }

    IEnumerator SmoothRotate(float angle)
    {
        float startTime = Time.time;
        float endTime = startTime + m_swingSpeed;
        float progress = 0;
        Quaternion startRotation = m_axePivot.localRotation;
        Quaternion endRotation = m_axePivot.localRotation * Quaternion.Euler(0, angle, 0);

        while (progress < 1)
        {
            progress = (Time.time - startTime) / (endTime - startTime);
            m_axePivot.localRotation = Quaternion.Slerp(startRotation, endRotation, progress);
            yield return new WaitForEndOfFrame();
        }

        if (!rotateBack)
        {
            rotateBack = true;
            StartCoroutine(SmoothRotate(-angle));
        }
        else
        {
            m_axePivot.localRotation = originalRotation;
        }
    }
}
