using UnityEngine;
using Leap;
using Leap.Unity;
using System.Collections.Generic;

public class MoveObject : MonoBehaviour
{
    private LeapProvider leapProvider;
    private HandRayCaster handRayCaster;
    private Vector3 initialObjectPosition;
    private Vector3 initialPalmPosition;
    private GameObject objectToMove;
    private bool isMoving = false;
    private bool moveGestureDetected = false;
    private void Update()
    {
        leapProvider = FindObjectOfType<LeapProvider>();
        handRayCaster = FindObjectOfType<HandRayCaster>();
        objectToMove = handRayCaster.GetSelectionCompletedObject();
        
        Frame frame = leapProvider.CurrentFrame;
        if (objectToMove != null && frame.Hands.Count == 1 && frame.Hands[0].IsRight)
        {
            Move(frame.Hands[0], objectToMove);
        }
        else
        {
            isMoving = false; // Reset the moving state if conditions aren't met
        }
    }

    void Move(Hand hand, GameObject objectMove)
    {
        //Debug.Log("Move methodunun içindeyim.");
        Finger thumb = hand.Fingers[(int)Finger.FingerType.TYPE_THUMB];
        Finger index = hand.Fingers[(int)Finger.FingerType.TYPE_INDEX];

        float distanceBetweenFingers = Vector3.Distance(thumb.TipPosition, index.TipPosition);

        //Debug.Log(distanceBetweenFingers);
        // Check if thumb and index finger's tips are close together
        if (distanceBetweenFingers < 0.05f) // You can adjust this threshold as needed
        {
            if (!isMoving)
            {
                isMoving = true;
                initialObjectPosition = objectMove.transform.position;
                initialPalmPosition = hand.PalmPosition;
            }
            else
            {
                Vector3 currentPalmPosition = hand.PalmPosition;
                Vector3 moveDirection = currentPalmPosition - initialPalmPosition;
                moveDirection = moveDirection*5.0f;

                // Apply the movement in x, y, and z dimensions
                objectMove.transform.position = initialObjectPosition + new Vector3(moveDirection.x, moveDirection.y, 0);
            }
        }
        else
        {
            isMoving = false;
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
