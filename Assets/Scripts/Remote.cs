using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Remote : MonoBehaviour
{
    public Cannon cannon;

    public void OnHitBigRedButton()
    {
        cannon.FireCannon();
    }

}
