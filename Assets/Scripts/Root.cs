using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour
{
    public AssetReferenceContainer Assets => GameCoordinator.Instance.assetReferenceContainer;
    // todo :: should baseRoot be it's own class?
    public static Root baseRoot;

    [Header("stat data")]

    [Header("prefab object references")]
    public Transform SubRootSpawnPoint;


    [Header("game states")]
    public bool IsCutOff;


    // runtime game object references
    [Header("runtime object references")]
    public Root parentRoot;
    public List<Root> subRoots = new List<Root>();
    

    // length of longest subroot
    public int Length => throw new NotImplementedException(); 
    // summed length of all subroots
    public int TotalLength => throw new NotImplementedException();

    public void Grow()
    {
        throw new NotImplementedException();
    }

    public void CreateNewRoot()
    {
        var newRootRotation = SubRootSpawnPoint.rotation * Quaternion.AngleAxis(UnityEngine.Random.Range(-90, 90), Vector3.forward);
        var newRoot = Instantiate(Assets.rootPrefab, SubRootSpawnPoint.position, newRootRotation, transform);
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateNewRoot();
        }
    }
}
