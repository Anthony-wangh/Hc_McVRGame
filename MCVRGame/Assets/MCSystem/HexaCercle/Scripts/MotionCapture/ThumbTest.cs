using UnityEngine;

namespace MCData
{
    public class ThumbTest : MonoBehaviour
    {
        private Quaternion thumb2Init, handFix;
        private float yInit2, yInit3;

        MotionCapture mc;
        const float GAIN = 100f;
        byte[] buffer;

        void Start()
        {
            mc = GetComponent<MotionCapture>();

            thumb2Init = mc.thumb2_right.transform.rotation;
            handFix = mc.hand_right.transform.rotation.Inverse() * thumb2Init;
            yInit2 = mc.thumb2_right.transform.localEulerAngles.y;
            yInit3 = mc.thumb2_right.transform.localEulerAngles.y;
        }

        void Update()
        {
            thumb2Init = mc.hand_right.transform.rotation * handFix;

            var current = mc.thumb2_right.transform.rotation;

            mc.thumb1_right.transform.rotation = Quaternion.Slerp(thumb2Init, mc.thumb2_right.transform.rotation, 0.5f);

            mc.thumb2_right.transform.rotation = current;

            var eul = mc.thumb3_right.transform.localEulerAngles;
            eul.y = (mc.thumb2_right.transform.localEulerAngles.y - yInit2) * 2f + yInit3 + 30;
            if (eul.y < 0) eul.y += 360;
            if (eul.y > 290) eul.y = 290;
            mc.thumb3_right.transform.localEulerAngles = eul;
        }
    }
}
