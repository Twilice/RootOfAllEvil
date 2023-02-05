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
              }    
       }

       public void GiveExperience()
       {
              var gardener = GameCoordinator.Instance.gardenerInstance;
              gardener.AddExperience(experienceAmount);
              Destroy(gameObject);
       }
       
       private void Update()
       {
              if (startedFollowing && GameCoordinator.Instance.gardenerInstance != null)
              {
                     transform.position = Vector3.MoveTowards(transform.position,
                            GameCoordinator.Instance.gardenerInstance.transform.position, followSpeed * Time.deltaTime);
              }
       }

       private void FixedUpdate()
       {
              if (startedFollowing)
              {
                     var collisions = Physics.OverlapSphere(transform.position, 0.5f);
                     foreach (var collision in collisions)
                     {
                            if (collision.tag == "Player")
                            {
                                   GiveExperience();
                            }
                     }
              }
       }
}