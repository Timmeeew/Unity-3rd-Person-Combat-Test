using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwordCollision : MonoBehaviour
{
    public GameObject PlayerObj;
    private float knockbackAmount = 20f;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "enemy")
        {
            Debug.Log("Collider Hit!");
            Rigidbody enemyBody = other.GetComponent<Rigidbody>(); //Gets the enemy's rigidbody
            Vector3 newVector3 = other.transform.position - PlayerObj.transform.position; //Calculate the vector3 dir for knockback effect
            enemyBody.AddForce(newVector3 * knockbackAmount, ForceMode.Impulse); //Apply a force for the enemy's rigidbody to that dir
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "enemy")
        {
            //Debug.Log("Stay");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "enemy")
        {
            //Debug.Log("Exit");
        }
    }
}
