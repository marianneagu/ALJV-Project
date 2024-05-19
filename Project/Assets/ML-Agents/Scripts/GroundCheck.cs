using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private bool isGrounded;
    private void OnTriggerEnter(Collider other)
    {
        isGrounded = true;
        gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
    }

    private void OnTriggerExit(Collider other)
    {
        isGrounded = false;
        gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }
}
