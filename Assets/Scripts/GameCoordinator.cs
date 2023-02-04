using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GameCoordinator : MonoBehaviour
{
    private static GameCoordinator _instance;
    public static GameCoordinator Instance => _instance;

    public AssetReferenceContainer assetReferenceContainer;

    [RuntimeInitializeOnLoadMethod]
    public static void Construct()
    {
        if (_instance == null)
        {
            _instance = new GameObject("GameCoordinator").AddComponent<GameCoordinator>();
        }
        _instance.Init();
    }

    private void Init()
    {
        DontDestroyOnLoad(this.gameObject);
        assetReferenceContainer = Resources.Load<AssetReferenceContainer>(nameof(AssetReferenceContainer));
        if (assetReferenceContainer == null)
            throw new System.NullReferenceException($"{nameof(GameCoordinator)} {transform.name} - scriptableObject type {nameof(AssetReferenceContainer)} with name {assetReferenceContainer} is missing.");
        assetReferenceContainer = Instantiate(assetReferenceContainer); // don't change the asset file object.

        Debug.Log("Test spawn a root");
        Instantiate(assetReferenceContainer.rootPrefab);
    }
  
}
