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
    public LineRenderer lR;

    [Header( "Line Control" )]
    public int pointCount = 50;
    public float bezierMinHeight = 1f;
    public float bezierMaxHeight = 3f;
    public float maxDist = 20;

    [Header( "Pickup Control" )]
    public float smoothnessValue = 0.2f;
    public LayerMask layerMask;

    [Header( "Throw Control" )]
    public float throwForce = 20f;

    [Header( "Debug View" )]
    public VRGrabbable hoveredObject;
    public VRGrabbable heldObject;
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
        lR.enabled = false;
        lR.positionCount = pointCount;
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
                Ray ray = new Ray( transform.position, transform.forward );
                RaycastHit hitInfo = new RaycastHit();
                if( Physics.Raycast( ray, out hitInfo, 100, layerMask ) )
                {
                    // Start Coroutine
                    VRGrabbable grabbale = hitInfo.collider.GetComponent<VRGrabbable>();
                    if (grabbale != null)
                    {
                        grabbale.SmoothMove( this );
                    }
                }
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
                heldObject.Release();

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
            VRInteractable interactable = heldObject.GetComponent<VRInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }

        if( handess == Handness.Right )
        {
            // Create and cast ray
            Ray ray = new Ray( transform.position, transform.forward );
            RaycastHit hitInfo = new RaycastHit();
            if( Physics.Raycast( ray, out hitInfo ) )
            {
                // Turn on Visual ref
                teleportVisualRef.gameObject.SetActive( true );
                lR.enabled = true;

                // Smooth out position
                Vector3 desiredPosition = hitInfo.point;
                Vector3 vecToDesired = desiredPosition - teleportVisualRef.position;
                vecToDesired *= smoothnessValue;
                teleportVisualRef.position += vecToDesired;

                // Set up our line renderer's positions
                Vector3 startPoint = transform.position;
                Vector3 endPoint = teleportVisualRef.position;

                Vector3 midPoint = ( ( endPoint - startPoint ) / 2f ) + startPoint;
                midPoint += Vector3.up * Mathf.Lerp(
                    bezierMinHeight, 
                    bezierMaxHeight, 
                    Mathf.Clamp(Vector3.Distance(startPoint,endPoint)/maxDist, 0, 1)
                );

                for( int i = 0; i < pointCount; i++ )
                {
                    Vector3 lerp1 = Vector3.Lerp( startPoint, midPoint, i / (float)pointCount );
                    Vector3 lerp2 = Vector3.Lerp( midPoint, endPoint, i / (float)pointCount );
                    Vector3 curvePos = Vector3.Lerp( lerp1, lerp2, i / (float)pointCount );

                    lR.SetPosition( i, curvePos );
                }

                //lR.SetPosition( 0, startPoint );
                //lR.SetPosition( 1, endPoint );

                if( hitInfo.collider.tag == "Floor" )
                {
                    lR.startColor = Color.cyan;
                    lR.endColor = Color.cyan;
                    // Activate Teleport if trigger pressed
                    if( Input.GetButtonDown( handess + "Trigger" ) )
                    {
                        Vector3 newPos = new Vector3( hitInfo.point.x, vrRig.position.y, hitInfo.point.z );
                        StartCoroutine( MoveWithFade( newPos ) );
                    }
                }
                else if ( hitInfo.collider.tag == "Interactable")
                {
                    lR.startColor = Color.green;
                    lR.endColor = Color.green;
                }
                else 
                {
                    lR.startColor = Color.red;
                    lR.endColor = Color.red;
                }
            }
            else
            {
                // If ray did not hit then turn off visual ref
                teleportVisualRef.gameObject.SetActive( false );
                lR.enabled = false;


            }
        }

        lastPosition = transform.position;
    }

    public void GrabObject( VRGrabbable objectToGrab )
    {
        remote = objectToGrab.GetComponent<Remote>();

        heldObject = objectToGrab;
        

        isHolding = true;

        objectToGrab.Grab( transform );
    }

    private void OnTriggerEnter( Collider other )
    {
        // SEt the currently hovered object if it is something we can pick up
        VRGrabbable grabbale = other.GetComponent<VRGrabbable>(); 
        if( grabbale != null)
        {
            hoveredObject = grabbale;
        }
    }

    private void OnTriggerExit( Collider other )
    {
        // Un set the currently hovered object if it is the one we just exited
        if( other.GetComponent<VRGrabbable>() == hoveredObject )
        {
            hoveredObject = null;
        }
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
