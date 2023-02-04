using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(AssetReferenceContainer), menuName = "ScriptableObjects/"+nameof(AssetReferenceContainer), order = 1)]
public class AssetReferenceContainer : ScriptableObject
{
    public Root rootPrefab;
    public ExperiencePickup experiencePickupPrefab;
    public GardenerController GardenerPrefab;
    public CameraFollow FollowCamera;
}
