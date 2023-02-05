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
    public int extraHpPerChildRoot = 5;

    [Header("prefab object references")]
    public List<Transform> subRootSpawnPoints;
    public Transform body;
    public Renderer meshRenderer;
    public List<Transform> particlesToMakeStandaloneOnDestroy;
    public GameObject onDeadParticleSystem;

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

    [Header("Hazards")]
    public Flower flower;
    
    private Vector3 startScale;
    private float timeUntilGrown;
    
    private int _hp = 10;

    private Flower flowerInstance;
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

    public void Grow(int baseRootTotalLength, bool growAnimation)
    {
        bool consumeGrowth = true;

        consumeGrowth &= subRoots.Count <= UnityEngine.Random.Range(0, maxBranches);
        consumeGrowth &= subRootSpawnPoints.Count > 0;
        consumeGrowth &= FullyGrown;
        
        if (consumeGrowth)
        {
            CreateNewRoot(growAnimation);
            return;
        }

        var spawnFlower = UnityEngine.Random.value < 0.05f && TotalLength <= 5;
        if (parentRoot != null && flowerInstance == null && spawnFlower)
        {
            flowerInstance = Instantiate(flower);
            flowerInstance.transform.parent = transform;
            flowerInstance.transform.position = transform.position;
            float randRotation = UnityEngine.Random.Range(0f, 360f);
            flowerInstance.transform.rotation = Quaternion.Euler(0, randRotation, 0f);
        }
        else if (subRoots.Count <= UnityEngine.Random.Range(0, maxBranches) == false)
        {
            var branchingRoot = subRoots[UnityEngine.Random.Range(0, subRoots.Count)];
            branchingRoot.Grow(TotalLength, growAnimation);
        }
    }

    public void CreateNewRoot(bool growAnimation)
    {
        var spawnPoint = subRootSpawnPoints[UnityEngine.Random.Range(0, subRootSpawnPoints.Count)];
        subRootSpawnPoints.Remove(spawnPoint);
        var newRootRotation = transform.rotation * spawnPoint.localRotation * Quaternion.AngleAxis(UnityEngine.Random.Range(-spawnAngle, spawnAngle), Vector3.up);
        var newRoot = Instantiate(Assets.rootPrefab, spawnPoint.position, newRootRotation, transform);
        newRoot.Setup(this, growAnimation);
        subRoots.Add(newRoot);
        // this makes no sense haha
        HP = extraHpPerChildRoot;
        StartCoroutine(IncreaseWidthCoroutine());
        if (parentRoot != null)
        {
            parentRoot.ChildCreatedRoot();
        }
    }

    public void Setup(Root parent, bool playGrowAnimation)
    {
        startScale = body.localScale * UnityEngine.Random.Range(0.8f, 1.1f); 
        parentRoot = parent;
        if (playGrowAnimation)
        {
            audioSource.clip = takingDamageClip;
            timeUntilGrown = timeToGrowSeconds;
            InvokeRepeating(nameof(RefreshRotationMovement), 1f, 2f);
            StartCoroutine(StartGrowCoroutine());
        }
        else
        {
            body.localScale = startScale;
        }
    }
    
    public void ChildCreatedRoot()
    {
        HP = 2;
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
        StartCoroutine(IncreaseWidthCoroutine());
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
        onDeadParticleSystem.transform.parent = null;
        onDeadParticleSystem.SetActive(true);
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

        foreach (var particleSystem in particlesToMakeStandaloneOnDestroy)
        {
            particleSystem.parent = null;
            particleSystem.GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (flowerInstance != null)
        {
            Destroy(flowerInstance.gameObject);
        }
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

        var diffVector = GameCoordinator.Instance.gardenerInstance.transform.position - transform.position;
        var hitRotation = Quaternion.FromToRotation(diffVector, transform.rotation.eulerAngles);

        Quaternion newRotation;
        if (UnityEngine.Random.Range(0, 2) > 0)
        {
            newRotation = transform.rotation * Quaternion.AngleAxis(20, Vector3.up);
        }
        else
        {
            newRotation = transform.rotation * Quaternion.AngleAxis(-20, Vector3.up);
        }

        StartCoroutine(_DisplaceOverTime(0.15f, newRotation, 
            () => StartCoroutine(_DisplaceOverTime(1.0f, originalRotation, () => isDisplacing = false))));
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
            // ease the lerpValue for faster early progrewssion then slowdown closer to complete
            lerpValue = 1-Mathf.Pow(1 - lerpValue, 5);
            transform.rotation = Quaternion.Lerp(oldRotation, newRotation, lerpValue);
            yield return null;
        }
        transform.rotation = newRotation;
        onComplete?.Invoke();
    }

    // Update is called once per frame
    private float slowRotationDisplacement;
    void RefreshRotationMovement()
    {
        slowRotationDisplacement = UnityEngine.Random.Range(-0.35f, 0.35f);
        if (TotalLength == 0)
        {
            slowRotationDisplacement *= 4;
        }
        else if (TotalLength == 1)
        {
            slowRotationDisplacement *= 2;
        }
    }

    void Update()
    {
        // debug test :: only
        if (Input.GetKeyDown(KeyCode.Space) && FullyGrown)
        {
            StartCoroutine(IncreaseWidthCoroutine());
            if (UnityEngine.Random.Range(0, 3) >= subRoots.Count)
            {
                CreateNewRoot(true);
            }
        }

        if (isDisplacing == false)
        {
            // use coroutine to move a longer distance before trying to change direction
            transform.rotation = transform.rotation * Quaternion.AngleAxis(slowRotationDisplacement * Time.deltaTime, Vector3.up);
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
