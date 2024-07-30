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
            Debug.Log("Enter");
            Rigidbody enemyBody = other.GetComponent<Rigidbody>();
            Vector3 newVector3 = other.transform.position - PlayerObj.transform.position;
            enemyBody.AddForce(newVector3 * knockbackAmount, ForceMode.Impulse);
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
