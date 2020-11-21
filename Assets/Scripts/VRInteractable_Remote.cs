using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInteractable_Remote : VRInteractable
{
    public Cannon cannon;

    public override void Interact()
    {
        cannon.FireCannon();
    }
}
