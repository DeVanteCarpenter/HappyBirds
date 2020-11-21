using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInteractable_PaintBrush : VRInteractable
{
    public MeshRenderer mR;

    public override void Interact()
    {
        mR.enabled = !mR.enabled;
    }
}
