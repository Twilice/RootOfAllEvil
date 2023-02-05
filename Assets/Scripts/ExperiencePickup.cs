using System;
using UnityEngine;

public class ExperiencePickup : MonoBehaviour
{
       public int experienceAmount;
       public float followSpeed = 12;
       
       public SphereCollider followCollider;
    public SphereCollider sphereCollider;

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
                    followCollider.gameObject.SetActive(false);
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

    public void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player")
        {
            return;
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