using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Leap;
using Leap.Unity;
using System.IO;
using System;

public class HoloLensUDPReceiver : MonoBehaviour
{
    public int port = 12345; // This should match the port you're sending data to
    private UdpClient udpClient;
    private Thread udpThread;
    public CapsuleHand capsuleHandInstance;

    void Start()
    {
        udpClient = new UdpClient(port);
        udpThread = new Thread(new ThreadStart(ReceiveData));
        udpThread.Start();
    }

    void OnApplicationQuit()
    {
        udpThread.Abort();
        udpClient.Close();
    }

    private void ReceiveData()
    {
        while (true)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] data = udpClient.Receive(ref remoteEndPoint);
            
            // Convert the byte array back to an ASCII string
            string receivedData = Encoding.ASCII.GetString(data);

            // Deserialize the ASCII string to reconstruct the Frame object
            Frame frameData = DeserializeStringToFrame(receivedData);
            
            UpdateHandVisuals(frameData); // This function updates the hand visuals based on the received data
        }
    }


    private void UpdateHandVisuals(Frame data)
    {
        // Find the CapsuleHands prefab in the scene
        if (data != null && data.Hands.Count > 0)
        {
            Debug.Log("Data transfer edildi.");
            // For simplicity, let's just take the first hand in the frame.
            // You can loop through all hands if needed.
            Hand currentHand = data.Hands[0];

            // Update the CapsuleHand instance with the new hand data
            capsuleHandInstance.SetLeapHand(currentHand);

            // Call the necessary methods to redraw the hand
            capsuleHandInstance.InitHand();
            capsuleHandInstance.BeginHand();
            capsuleHandInstance.UpdateHand();
        }
    }

    private Frame DeserializeStringToFrame(string data)
    {
        StringReader reader = new StringReader(data);

        // Deserialize Frame data
        long frameId = long.Parse(reader.ReadLine());
        long timestamp = long.Parse(reader.ReadLine());
        float fps = float.Parse(reader.ReadLine());
        int handCount = int.Parse(reader.ReadLine());

        List<Hand> hands = new List<Hand>();

        for (int i = 0; i < handCount; i++)
        {
            // Deserialize Hand data
            long handFrameId = long.Parse(reader.ReadLine());
            int handId = int.Parse(reader.ReadLine());
            float confidence = float.Parse(reader.ReadLine());
            float grabStrength = float.Parse(reader.ReadLine());
            float pinchStrength = float.Parse(reader.ReadLine());
            float pinchDistance = float.Parse(reader.ReadLine());
            float palmWidth = float.Parse(reader.ReadLine());
            bool isLeft = bool.Parse(reader.ReadLine());
            float timeVisible = float.Parse(reader.ReadLine());

            // Deserialize Arm data
            Vector3 armPrevJoint = ParseVector3(reader.ReadLine());
            Vector3 armNextJoint = ParseVector3(reader.ReadLine());
            Vector3 armCenter = ParseVector3(reader.ReadLine());
            Vector3 armDirection = ParseVector3(reader.ReadLine());
            float armLength = float.Parse(reader.ReadLine());
            float armWidth = float.Parse(reader.ReadLine());
            Bone.BoneType armType = (Bone.BoneType)Enum.Parse(typeof(Bone.BoneType), reader.ReadLine());
            Quaternion armRotation = ParseQuaternion(reader.ReadLine());

            Arm arm = new Arm(armPrevJoint, armNextJoint, armCenter, armDirection, armLength, armWidth, armRotation);

            // Deserialize Fingers data
            int fingerCount = int.Parse(reader.ReadLine());
            List<Finger> fingers = new List<Finger>();

            for (int j = 0; j < fingerCount; j++)
            {
                // Deserialize Finger data
                long fingerHandId = long.Parse(reader.ReadLine());
                int fingerId = int.Parse(reader.ReadLine());
                float fingerTimeVisible = float.Parse(reader.ReadLine());
                Vector3 fingerTipPosition = ParseVector3(reader.ReadLine());
                Vector3 fingerDirection = ParseVector3(reader.ReadLine());
                float fingerWidth = float.Parse(reader.ReadLine());
                float fingerLength = float.Parse(reader.ReadLine());
                bool fingerIsExtended = bool.Parse(reader.ReadLine());
                Finger.FingerType fingerType = (Finger.FingerType)Enum.Parse(typeof(Finger.FingerType), reader.ReadLine());

                // Deserialize Bone data for each bone in the finger
                Bone[] bones = new Bone[4];
                for (int k = 0; k < 4; k++)
                {
                    Vector3 bonePrevJoint = ParseVector3(reader.ReadLine());
                    Vector3 boneNextJoint = ParseVector3(reader.ReadLine());
                    Vector3 boneCenter = ParseVector3(reader.ReadLine());
                    Vector3 boneDirection = ParseVector3(reader.ReadLine());
                    float boneLength = float.Parse(reader.ReadLine());
                    float boneWidth = float.Parse(reader.ReadLine());
                    Bone.BoneType boneType = (Bone.BoneType)Enum.Parse(typeof(Bone.BoneType), reader.ReadLine());
                    Quaternion boneRotation = ParseQuaternion(reader.ReadLine());

                    bones[k] = new Bone(bonePrevJoint, boneNextJoint, boneCenter, boneDirection, boneLength, boneWidth, boneType, boneRotation);
                }

                Finger finger = new Finger(fingerHandId, fingerId, fingerTimeVisible, fingerTipPosition, fingerDirection, fingerWidth, fingerLength, fingerIsExtended, fingerType, bones[0], bones[1], bones[2], bones[3]);
                fingers.Add(finger);
            }

            // Deserialize other Hand properties
            Vector3 palmPosition = ParseVector3(reader.ReadLine());
            Vector3 stabilizedPalmPosition = ParseVector3(reader.ReadLine());
            Vector3 palmVelocity = ParseVector3(reader.ReadLine());
            Vector3 palmNormal = ParseVector3(reader.ReadLine());
            Quaternion handRotation = ParseQuaternion(reader.ReadLine());
            Vector3 handDirection = ParseVector3(reader.ReadLine());
            Vector3 wristPosition = ParseVector3(reader.ReadLine());

            Hand hand = new Hand(handFrameId, handId, confidence, grabStrength, pinchStrength, pinchDistance, palmWidth, isLeft, timeVisible, arm, fingers, palmPosition, stabilizedPalmPosition, palmVelocity, palmNormal, handRotation, handDirection, wristPosition);
            hands.Add(hand);
        }

        return new Frame(frameId, timestamp, fps, hands);
    }


    private Vector3 ParseVector3(string s)
    {
        string[] parts = s.Split(',');
        if (parts.Length != 3)
        {
            throw new FormatException("Invalid Vector3 format");
        }

        float x = float.Parse(parts[0]);
        float y = float.Parse(parts[1]);
        float z = float.Parse(parts[2]);

        return new Vector3(x, y, z);
    }

    private Quaternion ParseQuaternion(string s)
    {
        string[] parts = s.Split(',');
        if (parts.Length != 4)
        {
            throw new FormatException("Invalid Quaternion format");
        }

        float x = float.Parse(parts[0]);
        float y = float.Parse(parts[1]);
        float z = float.Parse(parts[2]);
        float w = float.Parse(parts[3]);

        return new Quaternion(x, y, z, w);
    }



}

