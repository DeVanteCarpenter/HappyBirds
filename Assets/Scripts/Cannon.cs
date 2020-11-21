using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    // Prefab asset dragged in from the project window
    public GameObject cannonBallPrefab;
    // Empty transform for reference for where to shoot cannon balls from
    public Transform shootPositionRef;

    // Speed and power of cannon
    public float rotSpeed;
    public float power = 25;

    public List<GameObject> objs;
    // Update is called once per frame
    void Update()
    {
        // Rotate canon up and down when using the left and right arrows
        if( Input.GetKey( "left" ) )
        {
            transform.Rotate( 0, 0, rotSpeed, Space.Self );
            Destroy( objs[3] );
        }
        else if( Input.GetKey( "right" ) )
        {
            transform.Rotate( 0, 0, -rotSpeed, Space.Self );
        }


        // Increase and decrease power when using the up and down arrow keys
        if( Input.GetKey( "up" ) )
        {
            power += 5 * Time.deltaTime;
        }
        else if( Input.GetKey( "down" ) )
        {
            power -= 5 * Time.deltaTime;
        }

        // Use the space bar to fire the cannon
        if (Input.GetKeyDown("space"))
        {
            FireCannon();
        }

    }

    // Turned Fire cannon into an function so it can be called in VR as well
    public void FireCannon()
    {
        // Create a new cannon ball from the prefab at the position of the shootPositionRef transform
        GameObject cannonBall = Instantiate( cannonBallPrefab, shootPositionRef.position, Quaternion.identity );

        // Get the rigidbody and add force forward
        cannonBall.GetComponent<Rigidbody>().AddForce( shootPositionRef.up * power, ForceMode.Impulse );
    }
}
