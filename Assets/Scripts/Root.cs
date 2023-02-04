using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class Root : MonoBehaviour
{
    public AssetReferenceContainer Assets => GameCoordinator.Instance.assetReferenceContainer;
    // todo :: should baseRoot be it's own class?
    public static Root baseRoot;

    [Header("stat data")]
    public float sizeScaleMultiplier = 0.5f;

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
    // summed length of all subroots
    public int TotalLength => CalculateTotalLength();

    public void Grow()
    {
        throw new NotImplementedException();
    }

    public void CreateNewRoot()
    {
        var newRootRotation = subRootSpawnPoint.localRotation * Quaternion.AngleAxis(UnityEngine.Random.Range(-90, 90), Vector3.up);
        //var newRootRotation = SubRootSpawnPoint.rotation * Quaternion.AngleAxis(90, Vector3.up);
        var newRoot = Instantiate(Assets.rootPrefab, subRootSpawnPoint.position, newRootRotation, transform);
        subRoots.Add(newRoot);
    }

    public void BranchOut()
    {
        throw new NotImplementedException();
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

        float scale = sizeScaleMultiplier * Mathf.Sqrt(TotalLength);
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
