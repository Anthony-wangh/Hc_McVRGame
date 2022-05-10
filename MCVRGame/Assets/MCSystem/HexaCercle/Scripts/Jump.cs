using System;
using System.Collections;
using UnityEngine;

namespace MCData
{
    public class Jump : MonoBehaviour
    {

        public static Vector3 Velocity;
        private static DateTime _t;
        public static bool isJumping = false;
        public static bool? OnGround = null;
        private void Update()
        {
            if (MotionCapture.instance?.bmc_Glove != null && MotionCapture.instance.bmc_Glove.IsLeftAnkleEnabled && MotionCapture.instance.bmc_Glove.IsRightAnkleEnabled)
            {
                OnGround = MotionCapture.instance.bmc_Glove.IsAnkleLeftOnGround || MotionCapture.instance.bmc_Glove.IsAnkleRightOnGround;
            }
        }
        public static IEnumerator JumpObject(GameObject Body, Vector3 V)
        {
            isJumping = true;

            Velocity = V;
            _t = DateTime.Now;

            Vector3 initPos = Body.transform.position;
            Vector3 v = Velocity;
            if (OnGround != null)
            {
                while (OnGround == false)
                {
                    Body.transform.position += v * Time.deltaTime;
                    v.y -= 9.8f * Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                while (Body.transform.position.y >= initPos.y)
                {
                    Body.transform.position += v * Time.deltaTime;
                    v.y -= 9.8f * Time.deltaTime;
                    yield return null;
                }
            }
            yield return new WaitForSeconds(0.5f);
            isJumping = false;

        }
    }
}