using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Handness
{
    Right,
    Left
}


public class VRHand: MonoBehaviour
{
    public Transform vrRig;
    public Transform teleportVisualRef;

    // Reference to animator component on the hand
    private Animator anim;

    // Enum to determine whether this is the left or right hand
    public Handness handess;

    public Transform holdPosition;
    public Transform hoveredObject;

    public bool isHolding = false;

    public 

    // Start is called before the first frame update
    void Start()
    {
        // Get the reference to the animator component of the hand
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // IF the grip button of the proper hand is pressed run the close animation
        if( Input.GetButtonDown( handess + "Grip" ) )
        {
            anim.SetBool( "GripPressed", true );
            //pick up object
            if (hoveredObject != null)
            {
                hoveredObject.SetParent( holdPosition );
                hoveredObject.GetComponent<Rigidbody>().useGravity = false;
                hoveredObject.localPosition = Vector3.zero;
                hoveredObject.localRotation = Quaternion.identity;

                isHolding = true;
            }
        }

        // IF the grip button of the proper hand is released run the open animation
        if( Input.GetButtonUp( handess + "Grip" ) )
        {
            anim.SetBool( "GripPressed", false );

            // Drop the object (if we are holding one)
            if( isHolding == true )
            {
                hoveredObject.SetParent( null );
                hoveredObject.GetComponent<Rigidbody>().useGravity = true;

                isHolding = false;
            }
        }

        if (handess == Handness.Right)
        {
            Ray ray = new Ray( transform.position, transform.forward );
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(ray, out hitInfo))
            {
                teleportVisualRef.gameObject.SetActive( true );
                teleportVisualRef.position = hitInfo.point;
                if( Input.GetButtonUp( handess + "Trigger" ) )
                {
                    vrRig.position = new Vector3( hitInfo.point.x, vrRig.position.y, hitInfo.point.z );
                }
            }
            else
            {
                teleportVisualRef.gameObject.SetActive( false );
            }
        }

    }

    private void OnTriggerEnter( Collider other )
    {
        // SEt the currently hovered object if it is something we can pick up
        if (other.tag == "Interactable")
        {
            hoveredObject = other.transform;
        }
    }

    private void OnTriggerExit( Collider other )
    {
        // Un set the currently hovered object if it is the one we just exited
        if (other.transform == hoveredObject)
        {
            hoveredObject = null;
        }
    }
}
