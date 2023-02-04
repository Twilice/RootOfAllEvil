using System;
using UnityEngine;

public class WatchPlayer : MonoBehaviour
{
       public float maxRotationZ = 50;
       public float maxRotationX = 40;

       private GardenerController gardener;

       private void Start()
       {
              gardener = GameCoordinator.Instance.gardenerInstance;
       }

       void Update()
       {
              var loc = transform.position;
              var ploc = gardener.transform.position;
              float angleBetweenEyeAndPlayer = Mathf.Atan2(loc.z - ploc.z, loc.x - ploc.x);
              transform.rotation = Quaternion.Euler(maxRotationX*Mathf.Cos(angleBetweenEyeAndPlayer + Mathf.PI/2),0,maxRotationZ*Mathf.Sin(angleBetweenEyeAndPlayer + Mathf.PI/2));
       }
}