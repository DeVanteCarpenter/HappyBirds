using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRGrabbable : MonoBehaviour
{
    public void Grab(Transform holdingParent)
    {
        transform.SetParent( holdingParent );
        GetComponent<Rigidbody>().useGravity = false;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Release()
    {
        transform.SetParent( null );
        GetComponent<Rigidbody>().useGravity = true;
    }

    public void SmoothMove(VRHand hand)
    {
        StartCoroutine( SmoothMoveToHand( hand ) );
    }


    private IEnumerator SmoothMoveToHand( VRHand hand )
    {
        float currentTime = 0;
        Vector3 startPos = transform.position;
        while( currentTime < 1 )
        {
            transform.position = Vector3.Lerp( startPos, hand.transform.position, currentTime );

            yield return null;

            currentTime += Time.deltaTime;
        }

        hand.GrabObject( this );
    }
}
