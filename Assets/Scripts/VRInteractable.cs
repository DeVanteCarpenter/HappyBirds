using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInteractable : MonoBehaviour
{
    public virtual void Interact()
    {
        Debug.Log( "You interacted with me! Hooray!" );
    }
}
