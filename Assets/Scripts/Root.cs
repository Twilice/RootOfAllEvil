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
    public float maxBranches = 3;

    [Header("prefab object references")]
    public Transform subRootSpawnPoint;
    public Transform body;

    [Header("game states")]
    public bool IsCutOff;


    // runtime game object references
    [Header("runtime object references")]
    public Root parentRoot;
    public List<Root> subRoots = new List<Root>();

    private Vector3 startScale;
    
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
        var newRootRotation = transform.rotation * Quaternion.AngleAxis(UnityEngine.Random.Range(-spawnAngle, spawnAngle), Vector3.up);
        var newRoot = Instantiate(Assets.rootPrefab, subRootSpawnPoint.position, newRootRotation, transform);
        subRoots.Add(newRoot);
    }

    public void OnCut()
    {
        throw new NotImplementedException();
    }


    // Start is called before the first frame update
    void Start()
    {
        startScale = body.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        float scale = Mathf.Max(sizeScaleMultiplier * Mathf.Log(TotalLength), 0);
        body.localScale = new Vector3(scale + startScale.x, startScale.y, scale + + startScale.z);
        // debug test :: only
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (UnityEngine.Random.Range(0, 3) >= subRoots.Count)
            {
                CreateNewRoot();
            }
        }
    }
}
