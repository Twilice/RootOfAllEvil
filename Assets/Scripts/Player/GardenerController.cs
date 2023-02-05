using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GardenerController : MonoBehaviour
{
    // Publics
    [Header("Move")]
    public float m_moveSpeed = 10f;
    public int m_maxRootStrengthWalkability = 20;
    public float minMoveSpeed = 3f;
    [Header("Turning")]
    public Transform m_body;
    public float m_sensitivity = 10f;
    [Header("Swing")]
    public float m_swingSpeed = 0.25f;
    public float m_rotationAngle = 90f;
    public Transform m_axePivot;
    public SphereCollider m_hitCollider;
    public Transform m_axeMesh;
    public int m_damage = 3;

    [Header("Animations")]
    public Animator runAnimator;
    public Animator swingAnimator;

    [Header("Effects")]
    public Transform m_cameraTransform;
    public ParticleSystem m_levelUpEffect;

    [Header("Sounds")]
    public AudioSource m_axeAudioSource;
    public AudioClip m_swooshSoundClip;
    public AudioSource m_bodyAudioScorce;
    public AudioClip m_gettingHit;
    public AudioClip m_levelUpClip;

    // Privates
    private Quaternion originalRotation;
    private bool rotateBack = false;

    private Vector3 originalScreenPos;

    private float currentMoveSpeed;
    private int _xp = 0;
    public int XP
    {
        get { return _xp; }
        set 
        { 
            int oldXP = _xp;
            _xp = value;
            var nextLvl = Mathf.Pow(5f, (float)LVL);
            if ((float)_xp >= nextLvl)
            {
                _xp -= (int)nextLvl;
                LVL++;
            }
        }
    }

    private int _lvl = 1;
    public int LVL
    {
        get { return _lvl; }
        private set
        {
            int oldLVL = _lvl;
            _lvl = value;
            if (_lvl > oldLVL)
            {
                float axeSize = 1 + (((float)_lvl - 1f) * 0.2f);
                m_axeMesh.localScale = new Vector3(-axeSize, axeSize, axeSize);
                m_hitCollider.radius += 0.2f;
                m_bodyAudioScorce.clip = m_levelUpClip;
                m_bodyAudioScorce.Play();
                m_levelUpEffect.Play();
            }
        }
    }

    private int _hp = 100;
    public int HP
    {
        get { return _hp; }
        set
        {
            int oldHP = _hp;
            _hp = value < 100 ? value : 100;
            if (_hp < 0)
            {
                Destroy(gameObject);
            }

            if (_hp < oldHP)
            {
                m_bodyAudioScorce.clip = m_gettingHit;
                m_bodyAudioScorce.Play();
                StartCoroutine(ScreenShake(0.2f, 0.3f));
            }
        }
    }

    public void AddExperience(int amount)
    {
        XP += amount;
        Debug.Log("XP " + XP.ToString());
    }
    
    // Start is called before the first frame update
    void Start()
    {
        m_axePivot.localEulerAngles = new Vector3(0f, -(m_rotationAngle / 2f), 0f);
        originalRotation = m_axePivot.rotation;
        currentMoveSpeed = m_moveSpeed;
        m_cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    private bool workaroundAttackCooldown = true;
    void Update()
    {
        //Movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        runAnimator.SetBool("isRunning", (horizontal != 0 || vertical != 0));

        transform.position = transform.position + new Vector3(horizontal, 0, vertical) * currentMoveSpeed * Time.deltaTime;
        
        var moveDir = new Vector3(horizontal, 0, vertical);
        if (moveDir.sqrMagnitude > 0.1)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveDir.x, 0f, moveDir.z));
            m_body.rotation = Quaternion.Slerp(m_body.rotation, targetRotation, m_sensitivity * Time.deltaTime);
        }

        // Swing
        if (workaroundAttackCooldown && Input.GetKeyDown(KeyCode.Mouse0))
        {
            rotateBack = false;
            m_axeAudioSource.clip = m_swooshSoundClip;
            //StartCoroutine(SmoothRotate(m_rotationAngle));
            swingAnimator.SetTrigger("Swing");
            CheckForRoots(out bool missedRoot);
            if (missedRoot)
            {
                PlaySoundClip();
            }
            workaroundAttackCooldown = false;
        }
        
        // Movement through roots
        Collider[] colliders = Physics.OverlapSphere(m_body.position, 0.5f);
        int currentRootLengthTouched = 0;
        foreach (var col in colliders)
        {
            if (col.tag == "Root")
            {
                var root = col.GetComponentInParent<Root>();
                if (root != null)
                {
                    currentRootLengthTouched += col.GetComponentInParent<Root>().TotalLength;
                }
            }
        }
        currentMoveSpeed = MathF.Max(m_moveSpeed*(1-currentRootLengthTouched/(float)m_maxRootStrengthWalkability), minMoveSpeed);
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawSphere(m_body.position, 0.5f);
    }

    public void SetSwingCooldown()
    {
        workaroundAttackCooldown = true;
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

    void CheckForRoots(out bool missedRoot)
    {
        missedRoot = true;
        // Use Physics.OverlapSphere to check for colliders within the given collider
        Collider[] colliders = Physics.OverlapSphere(m_hitCollider.bounds.center, m_hitCollider.bounds.extents.magnitude);

        // Check if there are any colliders within the given collider
        if (colliders.Length > 0)
        {
            foreach(var collider in colliders)
            {
                if (collider.gameObject.tag == "Root")
                {
                    missedRoot = false;
                    var root = collider.GetComponentInParent<Root>();    
                    root.TakeDamage(m_damage * LVL, collider.ClosestPoint(m_hitCollider.transform.position));
                    float duration = m_swingSpeed * 0.8f;
                    StartCoroutine(ScreenShake(duration, 0.15f));
                }
                else if (collider.gameObject.tag == "Flower")
                {
                    missedRoot = false;
                    var flower = collider.GetComponentInParent<Flower>();
                    flower.HP -= m_damage * LVL;
                }
                else if(collider.gameObject.tag == "BaseRoot")
                {
                    var baseRoot = collider.GetComponentInParent<BaseRoot>();
                    baseRoot.TakeDamage();
                }
            }
        }
    }

    public IEnumerator ScreenShake(float duration, float magnitude)
    {
        originalScreenPos = m_cameraTransform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            m_cameraTransform.localPosition = new Vector3(originalScreenPos.x + x, originalScreenPos.y + y, originalScreenPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        m_cameraTransform.localPosition = originalScreenPos;
    }

    private static float pitch = 1;
    private static int frameLastPitchChange = 0;
    public void PlaySoundClip()
    {
        if (frameLastPitchChange != Time.frameCount)
        {
            frameLastPitchChange = Time.frameCount;
            pitch = UnityEngine.Random.Range(0.96f, 1.05f);
        }
        m_axeAudioSource.pitch = pitch;
        m_axeAudioSource.Play();
    }
}
