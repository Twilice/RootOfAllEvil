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

    [Header("prefab object references")]
    public Transform subRootSpawnPoint;
    public Transform body;

    [Header("game states")]
    public bool IsCutOff;


    // runtime game object references
    [Header("runtime object references")]
    public Root parentRoot;
    public List<Root> subRoots = new List<Root>();
    

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
        var newRootRotation = transform.localRotation * Quaternion.AngleAxis(UnityEngine.Random.Range(-45, 45), Vector3.up);
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
        
    }

    // Update is called once per frame
    void Update()
    {
        body.localScale = new Vector3(Mathf.Sqrt(TotalLength)+1, 1, Mathf.Sqrt(TotalLength) + 1);
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
