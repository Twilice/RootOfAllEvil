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

    [Header("game states")]
    public bool IsCutOff;


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
        Destroy(this.gameObject, deathTime*depth);
    }

    public void TakeDamage(int damage)
    {
        HP = -damage;
    }


    // Start is called before the first frame update
    void Start()
    {
        startScale = body.localScale/* * UnityEngine.Random.Range(0.8f, 1.1f)*/;
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
