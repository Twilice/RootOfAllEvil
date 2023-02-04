using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

public class Root : MonoBehaviour
{
    public AssetReferenceContainer Assets => GameCoordinator.Instance.assetReferenceContainer;
    // todo :: should baseRoot be it's own class?
    public static BaseRoot baseRoot;

    [Header("stat data")]
    public float sizeScaleMultiplier = 0.5f;
    public float spawnAngle = 70;
    public float timeToGrowSeconds = 3;
    public AnimationCurve growSpeedCurve;
    public float deathTime = 0.4f;
    public float maxBranches = 3;

    [Header("prefab object references")]
    public List<Transform> subRootSpawnPoints;
    public Transform body;
    public Renderer meshRenderer;
    
    [Header("game states")]
    public bool IsCutOff;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip takingDamageClip;
    [Space]
    public ParticleSystem hitEffect;


    // runtime game object references
    [Header("runtime object references")]
    public Root parentRoot;
    public List<Root> subRoots = new List<Root>();
    
    private Vector3 startScale;
    private float timeUntilGrown;
    
    private int _hp = 10;
    public int HP
    {
        get { return _hp; }
        set { _hp += value;
            if (_hp <= 0) {
                OnCut(0);
            }
        }
    }
    public bool FullyGrown => timeUntilGrown <= 0;
    
    // length of longest subroot
    public int Length => throw new NotImplementedException();
    // summed length of all subroots
    public int TotalLength => CalculateTotalLength();

    public int CalculateLongestLength()
    {
        throw new NotImplementedException();
        //if (subRoots.Count == 0)
        //    return 0;

        //int depthLength = 0;
        //foreach (var subRoot in subRoots)
        //{
        //    depthLength += subRoot.CalculateLongestLength();
        //}

        //return depthLength;
    }

    public int CalculateTotalLength()
    {
        if (subRoots.Count == 0)
            return 0;

        int depthLength = 0;
        foreach(var subRoot in subRoots)
        {
            depthLength += subRoot.CalculateTotalLength() + 1;
        }

        return depthLength;
    }

    public void Grow(int baseRootTotalLength)
    {
        bool consumeGrowth = true;

        consumeGrowth &= subRoots.Count <= UnityEngine.Random.Range(0, maxBranches);
        consumeGrowth &= subRootSpawnPoints.Count > 0;

        if (consumeGrowth)
        {
            CreateNewRoot();
        }
        else
        {
            var branchingRoot = subRoots[UnityEngine.Random.Range(0, subRoots.Count)];
            branchingRoot.Grow(TotalLength);
        }
    }

    public void CreateNewRoot()
    {
        var spawnPoint = subRootSpawnPoints[UnityEngine.Random.Range(0, subRootSpawnPoints.Count)];
        subRootSpawnPoints.Remove(spawnPoint);
        var newRootRotation = transform.rotation * spawnPoint.localRotation * Quaternion.AngleAxis(UnityEngine.Random.Range(-spawnAngle, spawnAngle), Vector3.up);
        var newRoot = Instantiate(Assets.rootPrefab, spawnPoint.position, newRootRotation, transform);
        newRoot.parentRoot = this;
        subRoots.Add(newRoot);
        StartCoroutine(IncreaseWidthCoroutine());
        if (parentRoot != null)
        {
            parentRoot.ChildCreatedRoot();
        }
    }

    public void ChildCreatedRoot()
    {
        StartCoroutine(IncreaseWidthCoroutine());
        if (parentRoot != null)
        {
            parentRoot.ChildCreatedRoot();
        }
    }
    
    public void BranchOut()
    {
        throw new NotImplementedException();
    }

    void ChildBranchCut(Root root)
    {
        subRoots.Remove(root);
        
    }
    
    private void OnCut(int depth)
    {
        var subRootsCopy = subRoots.ToList();
        foreach (var child in subRootsCopy)
        {
            child.OnCut(depth + 1);
        }

        if (baseRoot.roots.Contains(this))
        {
            baseRoot.roots.Remove(this);
        }
        else
        {
            parentRoot.subRoots.Remove(this);
        }
        
        transform.SetParent(GameCoordinator.Instance.transform);
        StartCoroutine(Die(deathTime * depth));
    }

    private IEnumerator Die(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        var posx = UnityEngine.Random.Range(-meshRenderer.bounds.extents.x, meshRenderer.bounds.extents.x);
        var posy = UnityEngine.Random.Range(-meshRenderer.bounds.extents.y, meshRenderer.bounds.extents.y);
        var posz = UnityEngine.Random.Range(-meshRenderer.bounds.extents.z, meshRenderer.bounds.extents.z);
        var pos = new Vector3(posx, posy, posz) + meshRenderer.bounds.center;
        Instantiate(GameCoordinator.Instance.assetReferenceContainer.experiencePickupPrefab, pos, Quaternion.identity);
        Destroy(this.gameObject);
    }
    public void TakeDamage(int damage)
    {
        audioSource.Play();
        hitEffect.Play();
        HP = -damage;
        DisplaceRoots(damage);
    }

    private bool isDisplacing = false;
    public void DisplaceRoots(int damage)
    {
        if (isDisplacing) return;
        
        isDisplacing = true;
        var originalRotation = transform.rotation;
        var newRotation = transform.rotation * Quaternion.AngleAxis(40, Vector3.up);

        StartCoroutine(_DisplaceOverTime(0.10f, newRotation, 
            () => StartCoroutine(_DisplaceOverTime(0.85f, originalRotation, () => isDisplacing = false))));
    }

    // todo :: would be cool if effect is stackable
    IEnumerator _DisplaceOverTime(float duration, Quaternion newRotation, Action onComplete = null)
    {
        var timeElapsed = -Time.deltaTime;
        var oldRotation = transform.rotation;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            var lerpValue = timeElapsed / duration;
            // ease the lerpValue for faster early progression then slowdown closer to complete
            lerpValue = 1-Mathf.Pow(1 - lerpValue, 5);
            transform.rotation = Quaternion.Lerp(oldRotation, newRotation, lerpValue);
            yield return null;
        }
        transform.rotation = newRotation;
        onComplete?.Invoke();
    }

    void Start()
    {
        audioSource.clip = takingDamageClip;
        startScale = body.localScale * UnityEngine.Random.Range(0.8f, 1.1f);
        timeUntilGrown = timeToGrowSeconds;
        StartCoroutine(StartGrowCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        // debug test :: only
        if (Input.GetKeyDown(KeyCode.Space) && FullyGrown)
        {
            StartCoroutine(IncreaseWidthCoroutine());
            if (UnityEngine.Random.Range(0, 3) >= subRoots.Count)
            {
                CreateNewRoot();
            }
        }
    }
    
    
    
    IEnumerator StartGrowCoroutine()
    {
        body.localScale = new Vector3(startScale.x, startScale.y, 0);
        while (!FullyGrown)
        {
            yield return new WaitForEndOfFrame();
            timeUntilGrown -= Time.deltaTime;
            body.localScale = new Vector3(startScale.x, startScale.y, startScale.z * (1 - growSpeedCurve.Evaluate(timeUntilGrown / timeToGrowSeconds)));
        }
    }

    IEnumerator IncreaseWidthCoroutine()
    {
        HP = 2;
        Vector3 beforeScale = body.localScale;
        float scale = Mathf.Max(sizeScaleMultiplier * Mathf.Log(TotalLength), 0);
        
        Vector3 targetScale = new Vector3(scale + startScale.x, scale + startScale.y, startScale.z);
        float increaseWidthTimer = timeToGrowSeconds;
        while (increaseWidthTimer > 0)
        {
            yield return new WaitForEndOfFrame();
            increaseWidthTimer -= Time.deltaTime;
            body.localScale = Vector3.Lerp(beforeScale, targetScale, 1 - increaseWidthTimer/timeToGrowSeconds);
        }
    }
}
