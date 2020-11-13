using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public enum Handness
{
    Right,
    Left
}


public class VRHand: MonoBehaviour
{
    [Header( "Type" )]
    // Enum to determine whether this is the left or right hand
    public Handness handess;


    [Header( "Scene Refs" )]
    public Transform vrRig;
    public Transform holdPosition;
    public Transform teleportVisualRef;
    public Collider collisionCollider;
    public RawImage fadeScreen;

    [Header( "Pickup Control" )]
    public float smoothnessValue = 0.2f;
    public LayerMask layerMask;

    [Header( "Throw Control" )]
    public float throwForce = 20f;

    [Header( "Debug View" )]
    public Transform hoveredObject;
    public Transform heldObject;
    public bool isHolding = false;


    // Reference to animator component on the hand
    private Animator anim;
    private Vector3 lastPosition;
    private Remote remote;
    private bool isHoldingRight = false;
    private bool isHoldingLeft = false;

    // Start is called before the first frame update
    void Start()
    {
        // Get the reference to the animator component of the hand
        anim = GetComponent<Animator>();
        lastPosition = transform.position;
        collisionCollider.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if( handess == Handness.Left )
        {
            if( !isHoldingRight && Input.GetAxis( handess + "Stick_Horizontal" ) > 0.8f )
            {
                isHoldingRight = true;
                vrRig.Rotate( 0, 30, 0 );
            }

            if( isHoldingRight && Input.GetAxis( handess + "Stick_Horizontal" ) < 0.8f )
            {
                isHoldingRight = false;
            }

            if( !isHoldingLeft && Input.GetAxis( handess + "Stick_Horizontal" ) < -0.8f )
            {
                isHoldingLeft = true;
                vrRig.Rotate( 0, -30, 0 );
            }

            if( isHoldingLeft && Input.GetAxis( handess + "Stick_Horizontal" ) > -0.8f )
            {
                isHoldingLeft = false;
            }
        }

        // IF the grip button of the proper hand is pressed run the close animation
        if( Input.GetButtonDown( handess + "Grip" ) )
        {
            anim.SetBool( "GripPressed", true );
            //pick up object
            if( hoveredObject != null )
            {
                GrabObject( hoveredObject );
            }
            else
            {
                //Ray ray = new Ray( transform.position, transform.forward );
                //RaycastHit hitInfo = new RaycastHit();
                //if( Physics.Raycast( ray, out hitInfo, 100, layerMask ) )
                //{
                //    // Start Coroutine
                //    StartCoroutine( SmoothMoveToHand( hitInfo.collider.transform ) );
                //}
                collisionCollider.enabled = true;
            }
        }

        // IF the grip button of the proper hand is released run the open animation
        if( Input.GetButtonUp( handess + "Grip" ) )
        {
            anim.SetBool( "GripPressed", false );
            collisionCollider.enabled = false;
            // Drop the object (if we are holding one)
            if( isHolding == true )
            {
                heldObject.SetParent( null );
                heldObject.GetComponent<Rigidbody>().useGravity = true;

                XRNode nodeType;
                if( handess == Handness.Right )
                {
                    nodeType = XRNode.RightHand;
                }
                else
                {
                    nodeType = XRNode.LeftHand;
                }

                List<XRNodeState> nodeStates = new List<XRNodeState>();
                InputTracking.GetNodeStates( nodeStates );

                for( int i = 0; i < nodeStates.Count; i++ )
                {
                    if( nodeStates[i].nodeType == nodeType )
                    {
                        Vector3 velocity;
                        if( nodeStates[i].TryGetVelocity( out velocity ) )
                        {
                            heldObject.GetComponent<Rigidbody>().velocity = velocity * throwForce;
                        }
                    }
                }

                //Vector3 velo = ( transform.position - lastPosition) / Time.deltaTime;
                // = velo;

                isHolding = false;

                heldObject = null;
                remote = null;
            }
        }

        if( Input.GetButtonDown( "Fire" ) && remote )
        {
            remote.OnHitBigRedButton();
        }

        if( handess == Handness.Right )
        {
            Ray ray = new Ray( transform.position, transform.forward );
            RaycastHit hitInfo = new RaycastHit();
            if( Physics.Raycast( ray, out hitInfo ) )
            {
                teleportVisualRef.gameObject.SetActive( true );
                Vector3 desiredPosition = hitInfo.point;
                Vector3 vecToDesired = desiredPosition - teleportVisualRef.position;
                vecToDesired *= smoothnessValue;
                teleportVisualRef.position += vecToDesired;
                if( Input.GetButtonDown( handess + "Trigger" ) )
                {
                    Vector3 newPos = new Vector3( hitInfo.point.x, vrRig.position.y, hitInfo.point.z );
                    StartCoroutine( MoveWithFade( newPos ) );
                }
            }
            else
            {
                teleportVisualRef.gameObject.SetActive( false );
            }
        }

        lastPosition = transform.position;
    }

    private void GrabObject( Transform objectToGrab )
    {
        remote = objectToGrab.GetComponent<Remote>();

        heldObject = objectToGrab;
        heldObject.SetParent( holdPosition );
        heldObject.GetComponent<Rigidbody>().useGravity = false;
        heldObject.localPosition = Vector3.zero;
        heldObject.localRotation = Quaternion.identity;

        isHolding = true;
    }

    private void OnTriggerEnter( Collider other )
    {
        // SEt the currently hovered object if it is something we can pick up
        if( other.tag == "Interactable" )
        {
            hoveredObject = other.transform;
        }
    }

    private void OnTriggerExit( Collider other )
    {
        // Un set the currently hovered object if it is the one we just exited
        if( other.transform == hoveredObject )
        {
            hoveredObject = null;
        }
    }

    private IEnumerator SmoothMoveToHand( Transform objectToMove )
    {
        float currentTime = 0;
        Vector3 startPos = objectToMove.position;
        while( currentTime < 1 )
        {
            objectToMove.position = Vector3.Lerp( startPos, holdPosition.position, currentTime );

            yield return null;

            currentTime += Time.deltaTime;
        }

        GrabObject( objectToMove );
    }


    private IEnumerator MoveWithFade(Vector3 pos)
    {
        fadeScreen.color = Color.clear;

        float currentTime = 0;

        while(currentTime < 1)
        {
            fadeScreen.color = Color.Lerp( Color.clear, Color.black, currentTime );

            yield return null;

            currentTime += Time.deltaTime;
        }

        vrRig.position = pos;
        yield return new WaitForSeconds( 0.4f );

        currentTime = 0;
        while( currentTime < 1 )
        {
            fadeScreen.color = Color.Lerp( Color.black, Color.clear, currentTime );

            yield return null;

            currentTime += Time.deltaTime;
        }

        fadeScreen.color = Color.clear;
    }

}
