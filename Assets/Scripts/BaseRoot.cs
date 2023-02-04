using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class BaseRoot : MonoBehaviour
{
    public AssetReferenceContainer Assets => GameCoordinator.Instance.assetReferenceContainer;

    [Header("stat data")]
    public float baseSpawnWeight = 5;
    public float spawnWeightPerSize = 1;
    public float bonusSpawnWeightPerSecond = 3;
    public float bonusSpawnWeightPerSpawn = -5;
    // how to make this nice? Curved weight chance somehow?
    // if random roll is higher than weight, spawn branch
    public float maxSpawnWeightRoll = 100;

    [Header("prefab object references")]

    [Header("game states")]
    public float currentBonusSpawnWeight;
    public float DEBUG_currentSpawnWeight;

    public float CurrentSpawnWeight =>
        baseSpawnWeight +
        spawnWeightPerSize * TotalLength +
        currentBonusSpawnWeight;

            // summed length of all roots
    public int TotalLength => roots.Sum(root => root.TotalLength);

    // runtime game object references
    [Header("runtime object references")]
    public List<Root> roots = new List<Root>();

    void Awake()
    {
        if (Root.baseRoot != null)
        {
            throw new InvalidOperationException("BaseRoot already instanced, something went wrong.");
        }

        Root.baseRoot = this;
    }

    private const float tickRate = 1/10f;
    private const float tickDelay = 1.5f;
    void Start()
    {
        CreateInitialRoots();
        InvokeRepeating("TimerTick", tickDelay, tickRate);
    }

    void TimerTick()
    {
        currentBonusSpawnWeight += bonusSpawnWeightPerSecond*tickRate;
        TryBranch();
    }

    public void TryBranch()
    {
        DEBUG_currentSpawnWeight = CurrentSpawnWeight;
        // todo :: this is not taking tickrate into account
        if (UnityEngine.Random.Range(0, maxSpawnWeightRoll) < CurrentSpawnWeight)
        {
            Branch();
        }
    }

    public void Branch()
    {
        currentBonusSpawnWeight += bonusSpawnWeightPerSpawn;
        var branchingRoot = roots[UnityEngine.Random.Range(0, roots.Count)];
        branchingRoot.Grow(TotalLength);
    }

    public void CreateInitialRoots()
    {
        CreateInitialRoot(0);
        CreateInitialRoot(120);
        CreateInitialRoot(240);
    }

    public void CreateInitialRoot(float angle, int forwardOffset = 1)
    {
        var newRoot = Instantiate(Assets.rootPrefab, transform.position, Quaternion.AngleAxis(angle, Vector3.up));
        roots.Add(newRoot);
    }
}
