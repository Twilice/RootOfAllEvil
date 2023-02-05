using System;
using UnityEngine;

public class ExperiencePickup : MonoBehaviour
{
       public int experienceAmount;
       public float followSpeed = 12;
       
       public SphereCollider followCollider;

       public bool startedFollowing;
       
       public void OnTriggerEnter(Collider other)
       {
              if (other.tag != "Player")
              {
                     return;
              }
              if (!startedFollowing)
              {
                     startedFollowing = true;
                     followCollider.radius = 0.5f;
              }
              else
              {
                     var gardener = other.GetComponent<GardenerController>();
                     if (gardener != null)
                     {
                            gardener.AddExperience(experienceAmount);
                     }       
                     Destroy(gameObject);
              }      
       }

       private void Update()
       {
              if (startedFollowing && GameCoordinator.Instance.gardenerInstance != null)
              {
                     transform.position = Vector3.MoveTowards(transform.position,
                            GameCoordinator.Instance.gardenerInstance.transform.position, followSpeed * Time.deltaTime);
              }
       }
}