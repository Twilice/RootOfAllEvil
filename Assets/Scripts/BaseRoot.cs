using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Random = UnityEngine.Random;

public class BaseRoot : MonoBehaviour
{
    public AssetReferenceContainer Assets => GameCoordinator.Instance.assetReferenceContainer;

    [Header("stat data")]
    public int initialRoots = 5;

    public int initialOffshoots = 10;
    public float baseSpawnWeight = 5;
    public float spawnWeightPerSize = 1;
    public float bonusSpawnWeightPerSecond = 3;
    public float bonusSpawnWeightPerSpawn = -5;
    // how to make this nice? Curved weight chance somehow?
    // if random roll is higher than weight, spawn branch
    public float maxSpawnWeightRoll = 100;

    [Header("Visual variables")] 
    public float shakeMagnitude = 25;
    
    [Header("prefab object references")] 
    public WatchPlayer watchPlayer;

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
    private int amountStartRoots;

    private float shakeTimer;
    
    void Awake()
    {
        if (Root.baseRoot != null)
        {
            throw new InvalidOperationException("BaseRoot already instanced, something went wrong.");
        }

        Root.baseRoot = this;
    }

    private const float tickRate = 1/20f;
    private const float tickDelay = 1.5f;
    void Start()
    {
        CreateInitialRoots();
        InvokeRepeating(nameof(TimerTick), tickDelay, tickRate);
    }

    void TimerTick()
    {
        currentBonusSpawnWeight += bonusSpawnWeightPerSecond*tickRate;
        TryBranch();
    }

    public void TryBranch()
    {
        DEBUG_currentSpawnWeight = CurrentSpawnWeight;
        // todo :: this is not taking tickrate into account - changing tickrate will change spawnrate
        if (UnityEngine.Random.Range(0, maxSpawnWeightRoll) < CurrentSpawnWeight)
        {
            Branch(true);
        }
    }

    public void Branch(bool growAnimation)
    {
        if (roots.Count < initialRoots)
        {
            CreateInitialRoot(UnityEngine.Random.Range(0, 360));
        }
        else
        {
            currentBonusSpawnWeight += bonusSpawnWeightPerSpawn;
            var branchingRoot = roots[UnityEngine.Random.Range(0, roots.Count)];
            branchingRoot.Grow(TotalLength, growAnimation);
        }
    }

    public void CreateInitialRoots()
    {
        var angle = 0;
        for (int i = 0; i < initialRoots; i++)
        {
            CreateInitialRoot(angle + UnityEngine.Random.Range(-20,20));
            angle += 360 / initialRoots;
        }
        
        for (int i = 0; i < initialOffshoots; i++)
        {
            Branch(false);
        }
    }

    public void CreateInitialRoot(float angle, int forwardOffset = 1)
    {
        var newRoot = Instantiate(Assets.rootPrefab, transform.position, Quaternion.AngleAxis(angle, Vector3.up));
        newRoot.Setup(null, false);
        roots.Add(newRoot);
    }

    public void TakeDamage()
    {
        watchPlayer.DartEyes(0.5f);
        if (shakeTimer <= 0)
        {
            shakeTimer = 0.5f;
            StartCoroutine(Shake());
        }
        else
        {
            shakeTimer = 0.5f;
        }
    }
    
    private IEnumerator Shake()
    {
        var originalPosition = transform.localPosition;
        
        var shakeX = Random.Range(-1f, 1f);
        var shakeZ = Random.Range(-1f, 1f);
        while (shakeTimer >= 0)
        {
            shakeX = Random.Range(-1f, 1f);
            shakeZ = Random.Range(-1f, 1f);
            transform.localPosition = originalPosition + new Vector3(shakeX, 0, shakeZ) * shakeMagnitude * Time.deltaTime;
            yield return new WaitForEndOfFrame();
            shakeTimer -= Time.deltaTime;
        }

        transform.localPosition = originalPosition;
    }
}
