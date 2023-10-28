using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Leap;
using Leap.Unity;

public class LeapMotionUDPSender : MonoBehaviour
{
    public LeapProvider leapProvider;
    public string targetIP = "HOLOLENS_IP_ADDRESS"; // Replace with your HoloLens's IP address
    public int targetPort = 12345; // Or any port you prefer

    private UdpClient udpClient;

    void Start()
    {
        udpClient = new UdpClient();

    }

    void Update()
    {
        Frame currentFrame = leapProvider.CurrentFrame;
        SendData(currentFrame);
    }

    private byte[] SerializeFrameToBytes(Frame frameData)
    {
        StringBuilder sb = new StringBuilder();

        // Serialize Frame data
        sb.AppendLine(frameData.Id.ToString());
        sb.AppendLine(frameData.Timestamp.ToString());
        sb.AppendLine(frameData.CurrentFramesPerSecond.ToString());
        sb.AppendLine(frameData.Hands.Count.ToString());
        // Yukarısı doğru. Frame'de 4 data var.
        foreach (var hand in frameData.Hands)
        {
            // Serialize Hand data
            sb.AppendLine(hand.FrameId.ToString());
            sb.AppendLine(hand.Id.ToString());
            sb.AppendLine(hand.Confidence.ToString());
            sb.AppendLine(hand.GrabStrength.ToString());
            sb.AppendLine(hand.PinchStrength.ToString());
            sb.AppendLine(hand.PinchDistance.ToString());
            sb.AppendLine(hand.PalmWidth.ToString());
            sb.AppendLine(hand.IsLeft.ToString());
            sb.AppendLine(hand.TimeVisible.ToString());

            // Serialize Arm data
            sb.AppendLine(hand.Arm.Elbow.ToString());
            sb.AppendLine(hand.Arm.Wrist.ToString());
            sb.AppendLine(hand.Arm.Center.ToString());
            sb.AppendLine(hand.Arm.Direction.ToString());
            sb.AppendLine(hand.Arm.Length.ToString());
            sb.AppendLine(hand.Arm.Width.ToString());
            sb.AppendLine(hand.Arm.Type.ToString()); // Assuming BoneType is an enum or has a ToString method
            sb.AppendLine(hand.Arm.Rotation.ToString());

            // Serialize Fingers data
            sb.AppendLine(hand.Fingers.Count.ToString());
            foreach (var finger in hand.Fingers)
            {
                // Serialize Finger data
                sb.AppendLine(finger.HandId.ToString());
                sb.AppendLine(finger.Id.ToString());
                sb.AppendLine(finger.TimeVisible.ToString());
                sb.AppendLine(finger.TipPosition.ToString());
                sb.AppendLine(finger.Direction.ToString());
                sb.AppendLine(finger.Width.ToString());
                sb.AppendLine(finger.Length.ToString());
                sb.AppendLine(finger.IsExtended.ToString());
                sb.AppendLine(finger.Type.ToString()); // Assuming FingerType is an enum or has a ToString method

                // Serialize Bone data for each bone in the finger
                Bone[] bones = { finger.bones[0], finger.bones[1], finger.bones[2], finger.bones[3]};
                foreach (var bone in bones)
                {
                    sb.AppendLine(bone.PrevJoint.ToString());
                    sb.AppendLine(bone.NextJoint.ToString());
                    sb.AppendLine(bone.Center.ToString());
                    sb.AppendLine(bone.Direction.ToString());
                    sb.AppendLine(bone.Length.ToString());
                    sb.AppendLine(bone.Width.ToString());
                    sb.AppendLine(bone.Type.ToString()); // Assuming BoneType is an enum or has a ToString method
                    sb.AppendLine(bone.Rotation.ToString());
                }
            }

            // Serialize other Hand properties
            sb.AppendLine(hand.PalmPosition.ToString());
            sb.AppendLine(hand.StabilizedPalmPosition.ToString());
            sb.AppendLine(hand.PalmVelocity.ToString());
            sb.AppendLine(hand.PalmNormal.ToString());
            sb.AppendLine(hand.Rotation.ToString());
            sb.AppendLine(hand.Direction.ToString());
            sb.AppendLine(hand.WristPosition.ToString());
        }

        return Encoding.ASCII.GetBytes(sb.ToString());

    }

    private void SendData(Frame frameData)
    {

        byte[] bytes = SerializeFrameToBytes(frameData);
        //string asciiString = Encoding.ASCII.GetString(bytes);
        //Debug.Log(asciiString);
        udpClient.Send(bytes, bytes.Length, targetIP, targetPort);
    }

}
