using UnityEngine;

namespace MCData
{
    public class YawFixTest : MonoBehaviour
    {
        MotionCapture mc;

        float[] pchFix = new float[5], rollFix = new float[5];
        Quaternion[] modelInit = new Quaternion[5], SensorInit = new Quaternion[5];

        const float GAIN = 100f;
        byte[] buffer;

        void Start()
        {
            mc = GetComponent<MotionCapture>();
            mc.bmc_MoCa2 = new HexaCercleAPI.BodyMotion.BodyMotionCapture();
            if (System.IO.Ports.SerialPort.GetPortNames().Length == 1)
            {
                mc.bmc_MoCa2.Start(System.IO.Ports.SerialPort.GetPortNames()[0]);
            }
            else
            {
                mc.bmc_MoCa2.Start("COM4");
            }
            for (int i = 0; i < pchFix.Length; i++)
            {
                pchFix[i] = 0;
                rollFix[i] = 0;
            }
        }
        void Update()
        {

            Debug.Log("Model angle " + mc.shoulder_l.transform.eulerAngles);
            buffer = mc.bmc_MoCa2.getRawData();
            if (buffer?.Length == 17 || buffer?.Length == 11)
            {
                if (mc.bmc_MoCa2.IsLeftShoulderEnabled && (buffer[1] & 0x3f) == 0x23)
                {
                    var quat = new Quaternion(
                        ((sbyte)buffer[6]) / GAIN,  // x
                        ((sbyte)buffer[7]) / GAIN,  // y
                        ((sbyte)buffer[8]) / GAIN,  // z
                        ((sbyte)buffer[5]) / GAIN   // w
                        );

                    if (quat.IsValid())
                    {

                        float y = quat.y;
                        quat.y = quat.z;
                        quat.z = y;
                        quat.w *= -1f;
                        quat.Normalize();
                        Debug.Log("Unprocess Shoulder_L sensor eul = " + quat.eulerAngles.ToString());
                        if (pchFix[2] == 0)
                        {
                            quat *= Quaternion.Euler(0, 0, 90);
                            pchFix[2] = quat.eulerAngles.x;
                            rollFix[2] = quat.eulerAngles.z;
                            modelInit[2] = mc.shoulder_l.transform.rotation;
                            SensorInit[2] = Quaternion.Euler(0, quat.eulerAngles.y, -90);
                            quat *= Quaternion.Euler(0, 0, -90);
                        }

                        quat = quat *
                            Quaternion.Euler(0, 0, 90) *
                            Quaternion.Euler(new Vector3(0, 0, -rollFix[2])) *
                            Quaternion.Euler(0, 0, -90);
                        ;
                        Debug.Log("After Shoulder_L process eul = " + quat.eulerAngles.ToString());
                        mc.shoulder_l.transform.rotation = modelInit[2] * SensorInit[2].Inverse() * quat;
                    }
                }
            }
        }
    }
}
