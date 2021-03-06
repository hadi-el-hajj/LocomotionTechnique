using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LocomotionTechnique : MonoBehaviour
{
    public OVRInput.Controller leftController;
    public OVRInput.Controller rightController;
    public GameObject hmd;
    [SerializeField] private float leftTriggerValue;    
    [SerializeField] private float rightTriggerValue;

    //Superman Locomotion
    [SerializeField] private Vector3 flightDirection; 
    [SerializeField] private float relativePosition; 
    [SerializeField] private float armExtensionRight; 
    [SerializeField] private Vector3 directionOfExtensionInPlaneRight; 
    [SerializeField] private float armExtensionLeft; 
    [SerializeField] private Vector3 directionOfExtensionInPlaneLeft; 
 
    //original height position of controller depending on a person's height
    [SerializeField] private float originY = 0.0f; 
    [SerializeField] private bool originYisSet= false; 

    /////////////////////////////////////////////////////////
    // These are for the game mechanism.
    public ParkourCounter parkourCounter;
    public string stage;
    
    
    void Start()
    {
        
    }

    void Update()
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Superman Locomotion Technique
        if(!originYisSet){
            StartCoroutine(getOriginY()); 
        }
        else{
            leftTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, leftController); 
            rightTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, rightController); 
            if (rightTriggerValue > 0.95f){

                ///////////////////////////////////////////////////////////////////////////////////////
                //ROTATION
                // Quaternion rotation = OVRInput.GetLocalControllerRotation(leftController); 
                // float angleOfRotation = QuaternionAngle(rotation, hmd.transform.forward); 

                // //if pointing both hands forward BOOST MODE
                // if(angleOfRotation>= 60.0f){
                //     flightDirection.y= 0 ; 
                //     flightDirection.x = flightDirection.x*5 ; 
                //     flightDirection.z = flightDirection.z*5 ; 
                // }
                ///////////////////////////////////////////////////////////////////////////////////////
                
                //BOOST with left trigger
                if(leftTriggerValue > 0.95f){
                    //follow where the right controller is pointing
                    flightDirection = OVRInput.GetLocalControllerPosition(rightController);

                    //substract the y component from flight direction

                    //VERSION 1 without originY
                    // substract height of right controller from height of left controller to adjust height
                    // flightDirection.y = flightDirection.y - OVRInput.GetLocalControllerPosition(leftController).y;    

                    //VERSION2 with originY, i.e. height adjusted
                    flightDirection.y = flightDirection.y - originY;    

                    //BOOST Mode comparing positions
                    // - Controllers have to be close 
                    // - Arms have to be extended (for both controllers)

                    // extension left arm
                    directionOfExtensionInPlaneLeft = OVRInput.GetLocalControllerPosition(leftController);
                    directionOfExtensionInPlaneLeft.y = directionOfExtensionInPlaneLeft.y - originY ; 
                    armExtensionLeft = directionOfExtensionInPlaneLeft.magnitude ; 
                    
                    //extension right arm
                    directionOfExtensionInPlaneRight = OVRInput.GetLocalControllerPosition(rightController);
                    directionOfExtensionInPlaneRight.y = directionOfExtensionInPlaneRight.y - originY ; 
                    armExtensionRight = directionOfExtensionInPlaneRight.magnitude ; 

                    // distance of controllers one apart from the other
                    relativePosition = (OVRInput.GetLocalControllerPosition(rightController) - OVRInput.GetLocalControllerPosition(leftController)).magnitude;
                    if(relativePosition<= 0.1f && armExtensionLeft>= 0.5f && armExtensionRight >= 0.5f){
                        flightDirection.y = flightDirection.y*3 ; 
                        flightDirection.x = flightDirection.x*3 ; 
                        flightDirection.z = flightDirection.z*3 ; 
                    }
                }
                else{
                    //follow where the right controller is pointing
                    flightDirection = OVRInput.GetLocalControllerPosition(rightController);
                    // substract height of right controller from height of left controller to adjust height
                    flightDirection.y = flightDirection.y - originY;    
                }
                
                //move the player 
                this.transform.position = this.transform.position + flightDirection*0.5f ;
            }


            ////////////////////////////////////////////////////////////////////////////////
            // These are for the game mechanism.
            if (OVRInput.Get(OVRInput.Button.Two) || OVRInput.Get(OVRInput.Button.Four))
            {
                if (parkourCounter.parkourStart)
                {
                    this.transform.position = parkourCounter.currentRespawnPos;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {

        // These are for the game mechanism.
        if (other.CompareTag("banner"))
        {
            stage = other.gameObject.name;
            parkourCounter.isStageChange = true;
        }
        else if (other.CompareTag("coin"))
        {
            parkourCounter.coinCount += 1;
            this.GetComponent<AudioSource>().Play();
            other.gameObject.SetActive(false);
        }
        // These are for the game mechanism.

    }


    //returns the angle in degrees around a specified axis from a given quaternion and axis rotation
    public float QuaternionAngle(Quaternion rotation, Vector3 axis)
    {
     float angle;
     Vector3 rotAxis;

     rotation.ToAngleAxis(out angle, out rotAxis);

     float angleAroundAxis = Vector3.Dot(rotAxis, axis);

     return angleAroundAxis ; 
    }


    //coroutine to get a base height of the controllers depending on each person's height
    //the coroutine will wait for left trigger to be pressed and capture its relative height depending on every person
    IEnumerator getOriginY()
    {    
        //wait for rightTrigger to be pressed
        leftTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, leftController); 

        //while the player does not set height pressing the left trigger, keep pausing execution
        while(!(leftTriggerValue > 0.95f))
        {
            //wait
            yield return null ; 
        }

        //once the player presses the key, set originY and resume execution
        originY = OVRInput.GetLocalControllerPosition(leftController).y ; 
        originYisSet = true ; 
    }
}

