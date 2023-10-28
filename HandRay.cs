using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class HandRayCaster : MonoBehaviour
{
    public LeapProvider leapProvider;
    public GameObject rayObject;
    public float rayLength = 100f;
    public float selectionDuration = 5f;
    public UnityEngine.UI.Slider progressBar;
    private float currentSelectionTime = 0f;
    private GameObject currentSelectedObject = null;
    private GameObject selectionCompletedObject = null;
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
    private Vector3 selectionCompletedObjectOriginalScale;

    private void Update()
    {
        Frame frame = leapProvider.CurrentFrame;
        LineRenderer lr = rayObject.GetComponent<LineRenderer>();

        if (frame.Hands.Count == 0)
        {
            rayObject.SetActive(false);
            lr.enabled = true; // Ensure the LineRenderer is enabled when no hands are detected
        }
        else if (frame.Hands.Count >= 1)
        {
            rayObject.SetActive(true);
            Hand firstHand = frame.Hands[0];

            Vector3 palmPosition = ConvertPosition(firstHand.PalmPosition);
            Vector3 palmNormal = ConvertDirection(firstHand.PalmNormal);

            Ray handRay = new Ray(palmPosition, palmNormal);
            RaycastHit hit;

            if (firstHand.IsRight && IsFist(firstHand))
            {
                if (selectionCompletedObject != null)
                {
                    selectionCompletedObject.transform.localScale = selectionCompletedObjectOriginalScale;
                    selectionCompletedObject = null;
                }
                lr.enabled = true; // Ensure the LineRenderer is enabled when a fist is detected
                return;
            }

            if (selectionCompletedObject != null)
            {
                lr.enabled = false; // Disable the LineRenderer when an object is selected
                return;
            }
            else
            {
                lr.enabled = true; // Enable the LineRenderer when no object is selected
            }

            if (Physics.Raycast(handRay, out hit, rayLength))
            {
                if (currentSelectedObject == hit.collider.gameObject)
                {
                    currentSelectionTime += Time.deltaTime;
                    if (currentSelectionTime >= selectionDuration)
                    {
                        Debug.Log("Object selected: " + hit.collider.name);
                        currentSelectedObject = null;

                        selectionCompletedObject = hit.collider.gameObject;
                        selectionCompletedObjectOriginalScale = selectionCompletedObject.transform.localScale;
                        selectionCompletedObject.transform.localScale *= 1.5f;
                    }
                }
                else
                {
                    currentSelectedObject = hit.collider.gameObject;
                    currentSelectionTime = 0f;
                }

                UpdateRayObject(palmPosition, hit.point);
            }
            else
            {
                currentSelectedObject = null;
                currentSelectionTime = 0f;
                UpdateRayObject(palmPosition, palmPosition + palmNormal * rayLength);
            }

            progressBar.value = currentSelectionTime / selectionDuration;
        }
    }


    void UpdateRayObject(Vector3 start, Vector3 end)
    {
        LineRenderer lr = rayObject.GetComponent<LineRenderer>();
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    Vector3 ConvertPosition(Vector3 position)
    {
        return new Vector3(position.x, position.y, position.z);
    }

    Vector3 ConvertDirection(Vector3 direction)
    {
        return new Vector3(direction.x, direction.y, direction.z);
    }

    public GameObject GetSelectionCompletedObject()
    {
        return selectionCompletedObject;
    }

    private bool IsFist(Hand hand)
    {   
        if(hand.GrabStrength > 0.9f)
        {
            Debug.Log("Fist");
            return true;
        }
        else
        {
            Debug.Log("Fist değil.");
            return false;
        }
        
    }

}
