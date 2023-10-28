using UnityEngine;
using Leap;
using Leap.Unity;

public class RotateObject : MonoBehaviour
{
    private LeapProvider leapProvider;
    private HandRayCaster handRayCaster;
    private Vector3 initialHandDirection; // Changed from initialFingerTipPosition to initialHandDirection
    private GameObject objectToRotate;
    private bool isRotating = false;

    private void Awake()
    {
        leapProvider = FindObjectOfType<LeapProvider>();
        handRayCaster = FindObjectOfType<HandRayCaster>();
    }

    private void Update()
    {
        objectToRotate = handRayCaster.GetSelectionCompletedObject();
        Frame frame = leapProvider.CurrentFrame;

        if (objectToRotate != null && frame.Hands.Count == 1 && frame.Hands[0].IsRight)
        {
            Rotate(frame.Hands[0]);
        }
        else
        {
            isRotating = false; // Reset the rotating state if conditions aren't met
        }
    }

    void Rotate(Hand hand)
    {
        Vector3 currentHandDirection = hand.Direction;

        if (!isRotating)
        {
            isRotating = true;
            initialHandDirection = currentHandDirection;
        }
        else
        {
            Quaternion rotationDelta = Quaternion.FromToRotation(initialHandDirection, currentHandDirection);
            objectToRotate.transform.rotation *= rotationDelta;

            initialHandDirection = currentHandDirection; // Update the initial direction for the next frame
        }
    }


    /*
    bool DetectMoveControl(Hand hand, GameObject objectToMove)
    {
        Finger indexFinger = hand.Fingers[(int)Finger.FingerType.TYPE_INDEX];
        Finger thumb = hand.Fingers[(int)Finger.FingerType.TYPE_THUMB];
        Finger middleFinger = hand.Fingers[(int)Finger.FingerType.TYPE_MIDDLE];
        Finger ringFinger = hand.Fingers[(int)Finger.FingerType.TYPE_RING];
        Finger pinkyFinger = hand.Fingers[(int)Finger.FingerType.TYPE_PINKY];

        // Check if only the index finger is extended and the other fingers are not
        if (indexFinger.IsExtended && 
            !thumb.IsExtended && 
            !middleFinger.IsExtended && 
            !ringFinger.IsExtended && 
            !pinkyFinger.IsExtended)
        {
            moveGestureDetected = true;
        }
        
    }

    */

}
