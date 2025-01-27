using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class Weapon : MonoBehaviour
{
    Rigidbody rb;
    BoxCollider bc;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();    
        bc = GetComponent<BoxCollider>();
        PlayerController.OnControllerColliderHitInternal += OnPlayerControllerHit;
    }

    void OnPlayerControllerHit(Collider playerCollider, ControllerColliderHit thingThatHitPlayer)
    {
        Debug.Log($"Player has been hit by {thingThatHitPlayer.collider.name}");
    }

    public void Equip(Collider playerCollider, Transform weaponAttachPoint)
    {
        rb.isKinematic = true;
        bc.isTrigger = true;
        transform.SetParent(weaponAttachPoint);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        Physics.IgnoreCollision(playerCollider, bc);
    }

    public void Drop(Collider playerCollider, Vector3 playerForward)
    {
        transform.parent = null;
        rb.isKinematic = false;
        bc.isTrigger = false;
        rb.AddForce(playerForward * 10.0f, ForceMode.Impulse);
        StartCoroutine(DropCooldown(playerCollider));
    }

    IEnumerator DropCooldown(Collider playerCollider)
    {
        yield return new WaitForSeconds(3);

        //happens after cooldown
        Physics.IgnoreCollision(bc, playerCollider, false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
