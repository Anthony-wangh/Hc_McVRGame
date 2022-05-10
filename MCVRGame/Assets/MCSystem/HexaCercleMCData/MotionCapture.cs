#define FOOTSTEP
#define FINGER_YNEG     
using HexaCercleAPI.BodyMotion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Quaternion = UnityEngine.Quaternion;

namespace MCData
{
    public class MotionCapture : MonoBehaviour
    {
        public static MotionCapture instance;
        public BodyMotionCapture bmc_Glove;
        public BodyMotionCapture bmc_MoCa2;
        private float footEuler;
        private List<float> offSetLeft = new List<float>();
        private List<float> offSetRight = new List<float>();

        [HideInInspector]
        public bool Paused;

        [HideInInspector]
        public string ComName, MoCa2Com;

        [HideInInspector]
        public GameObject RootObj;

        [HideInInspector]
        public GameObject chest, head, body,
                          shoulder_l, foreArm_l, thigh_l, calf_l, foot_l, footEnd_l,
                          shoulder_r, foreArm_r, thigh_r, calf_r, foot_r, footEnd_r;

        [HideInInspector]
        public GameObject HeadAim, JumpCenter, hand_left, thumb1_left, thumb2_left, thumb3_left,
                          index1_left, index2_left, index3_left, middle1_left, middle2_left, middle3_left,
                          ring1_left, ring2_left, ring3_left,
                          pinky1_left, pinky2_left, pinky3_left,
                          hand_right, thumb1_right, thumb2_right, thumb3_right,
                          index1_right, index2_right, index3_right,
                          middle1_right, middle2_right, middle3_right,
                          ring1_right, ring2_right, ring3_right,
                          pinky1_right, pinky2_right, pinky3_right;

        // 记录程序启动时的姿态，用于重置
        private readonly Dictionary<Body, Vector3> InitPose = new Dictionary<Body, Vector3>();

        // 拇指各关节启动时的姿态
        private Quaternion Thumb2LeftInit, Thumb2RightInit, HandLeftFix, HandRightFix, Thumb1RightInit, Thumb1LeftInit;
        private float pitch2LInit, pitch2RInit, pitch3LInit, pitch3RInit;
        private FootSteps footSteps;

        //初始化模型身体部位参数
        private const string HCBodyName = "hc_";
        private static string[] keys = new string[] { "Index", "Middle", "Ring", "Pinky", "Thumb3" };
        private Dictionary<Body, GameObject> NodeObjDic = new Dictionary<Body, GameObject>();

        public enum MoCaVers
        {
            MoCaM11 = 1,    // 手套与MoCa使用各自的中继站
            MoCaS = 2       // 手套与MoCa共同一个中继站
        }
        [HideInInspector]
        public MoCaVers MoCaVer = MoCaVers.MoCaM11;


        public  double[] mcLPitchArray;
        public  double[] mcRPitchArray;

        /// <summary>
        /// 初始化串口
        /// </summary>
        /// <returns></returns>
        private IEnumerator InitComPorts()
        {
            Paused = true;
            Debug.Log("初始化设备");
            var ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                var testBmc = new BodyMotionCapture();
                if (testBmc.Start(port))
                {
                    yield return new WaitForSeconds(0.5f);

                    if ((testBmc.IsAbdomenEnabled || testBmc.IsChestEnabled
                        || testBmc.IsLeftElbowEnabled || testBmc.IsLeftShoulderEnabled
                        || testBmc.IsRightElbowEnabled || testBmc.IsRightShoulderEnabled
                        || testBmc.IsLeftKneeEnabled || testBmc.IsLeftHipEnabled
                        || testBmc.IsRightKneeEnabled || testBmc.IsRightHipEnabled
                        || testBmc.IsHeadEnabled)
                        && (testBmc.IsLeftGloveEnabled || testBmc.IsRightGloveEnabled))
                    {
                        MsgLog.WriteLine("MoCa S设备已发现 " + MoCa2Com);
                        MoCaVer = MoCaVers.MoCaS;
                        ComName = port;
                        MoCa2Com = port;
                        testBmc.Dispose();
                        break;
                    }
                    else if (testBmc.IsAbdomenEnabled || testBmc.IsChestEnabled
                        || testBmc.IsLeftElbowEnabled || testBmc.IsLeftShoulderEnabled
                        || testBmc.IsRightElbowEnabled || testBmc.IsRightShoulderEnabled
                        || testBmc.IsLeftKneeEnabled || testBmc.IsLeftHipEnabled
                        || testBmc.IsRightKneeEnabled || testBmc.IsRightHipEnabled
                        || testBmc.IsHeadEnabled)
                    {
                        MoCaVer = MoCaVers.MoCaM11;
                        MoCa2Com = port;
                        MsgLog.WriteLine("MoCa M11设备已发现 " + MoCa2Com);
                    }
                    else if (testBmc.IsLeftGloveEnabled || testBmc.IsRightGloveEnabled)
                    {
                        MoCaVer = MoCaVers.MoCaM11;
                        ComName = port;
                        MsgLog.WriteLine("MoCa G6已发现 " + ComName);
                    }
                    testBmc.Dispose();
                }
                yield return null;
            }

            if (!string.IsNullOrEmpty(ComName))
            {
                bmc_Glove?.Dispose();
                bmc_Glove = new BodyMotionCapture();
                bmc_Glove.Start(ComName);
                if (MoCaVer == MoCaVers.MoCaS)
                {
                    bmc_MoCa2 = bmc_Glove;
                }
            }
            else
            {
                MsgLog.WriteLine("未发现手套");
            }
            if (MoCaVer == MoCaVers.MoCaM11 && !string.IsNullOrEmpty(MoCa2Com))
            {
                bmc_MoCa2?.Dispose();
                bmc_MoCa2 = new BodyMotionCapture();
                bmc_MoCa2.Start(MoCa2Com);
#if FOOTSTEP
                footSteps.SetEnable(true);
#endif
            }
            else if (MoCaVer != MoCaVers.MoCaS)
            {
                MsgLog.WriteLine("未发现MoCa");
            }

            Paused = false;

            yield return new WaitForSeconds(MsgLog.HideDelay);
            MsgLog.DisableMsg();
        }

        void Start()
        {
            instance = this;
            RootObj = gameObject;
            footSteps = new FootSteps();

            InitModelBody();

            // 为姿态做备份，用于重置
            InitPose.Add(Body.Shoulder_L, shoulder_l.transform.localEulerAngles);
            InitPose.Add(Body.Elbow_L, foreArm_l.transform.localEulerAngles);
            InitPose.Add(Body.Hip_L, thigh_l.transform.localEulerAngles);
            InitPose.Add(Body.Knee_L, calf_l.transform.localEulerAngles);
            InitPose.Add(Body.Chest, chest.transform.localEulerAngles);
            InitPose.Add(Body.Abdomen, body.transform.localEulerAngles);
            InitPose.Add(Body.Shoulder_R, shoulder_r.transform.localEulerAngles);
            InitPose.Add(Body.Elbow_R, foreArm_r.transform.localEulerAngles);
            InitPose.Add(Body.Hip_R, thigh_r.transform.localEulerAngles);
            InitPose.Add(Body.Knee_R, calf_r.transform.localEulerAngles);
            InitPose.Add(Body.Foot_L, foot_l.transform.localEulerAngles);
            InitPose.Add(Body.Foot_R, foot_r.transform.localEulerAngles);
            InitPose.Add(Body.Head, head.transform.localEulerAngles);
            InitPose.Add(Body.Hand_L, hand_left.transform.localEulerAngles);
            InitPose.Add(Body.Hand_R, hand_right.transform.localEulerAngles);
            InitPose.Add(Body.Thumb1_L, thumb1_left.transform.localEulerAngles);
            InitPose.Add(Body.Thumb1_R, thumb1_right.transform.localEulerAngles);
            InitPose.Add(Body.Thumb2_L, thumb2_left.transform.localEulerAngles);
            InitPose.Add(Body.Thumb2_R, thumb2_right.transform.localEulerAngles);
            InitPose.Add(Body.Thumb3_L, thumb3_left.transform.localEulerAngles);
            InitPose.Add(Body.Thumb3_R, thumb3_right.transform.localEulerAngles);
            InitPose.Add(Body.Index1_L, index1_left.transform.localEulerAngles);
            InitPose.Add(Body.Index2_L, index2_left.transform.localEulerAngles);
            InitPose.Add(Body.Index3_L, index3_left.transform.localEulerAngles);
            InitPose.Add(Body.Middle1_L, middle1_left.transform.localEulerAngles);
            InitPose.Add(Body.Middle2_L, middle2_left.transform.localEulerAngles);
            InitPose.Add(Body.Middle3_L, middle3_left.transform.localEulerAngles);
            InitPose.Add(Body.Ring1_L, ring1_left.transform.localEulerAngles);
            InitPose.Add(Body.Ring2_L, ring2_left.transform.localEulerAngles);
            InitPose.Add(Body.Ring3_L, ring3_left.transform.localEulerAngles);
            InitPose.Add(Body.Pinky1_L, pinky1_left.transform.localEulerAngles);
            InitPose.Add(Body.Pinky2_L, pinky2_left.transform.localEulerAngles);
            InitPose.Add(Body.Pinky3_L, pinky3_left.transform.localEulerAngles);

            InitPose.Add(Body.Index1_R, index1_right.transform.localEulerAngles);
            InitPose.Add(Body.Index2_R, index2_right.transform.localEulerAngles);
            InitPose.Add(Body.Index3_R, index3_right.transform.localEulerAngles);
            InitPose.Add(Body.Middle1_R, middle1_right.transform.localEulerAngles);
            InitPose.Add(Body.Middle2_R, middle2_right.transform.localEulerAngles);
            InitPose.Add(Body.Middle3_R, middle3_right.transform.localEulerAngles);
            InitPose.Add(Body.Ring1_R, ring1_right.transform.localEulerAngles);
            InitPose.Add(Body.Ring2_R, ring2_right.transform.localEulerAngles);
            InitPose.Add(Body.Ring3_R, ring3_right.transform.localEulerAngles);
            InitPose.Add(Body.Pinky1_R, pinky1_right.transform.localEulerAngles);
            InitPose.Add(Body.Pinky2_R, pinky2_right.transform.localEulerAngles);
            InitPose.Add(Body.Pinky3_R, pinky3_right.transform.localEulerAngles);

            Thumb2LeftInit = thumb2_left.transform.rotation;
            Thumb2RightInit = thumb2_right.transform.rotation;
            HandLeftFix = hand_left.transform.rotation.Inverse() * Thumb2LeftInit;
            HandRightFix = hand_right.transform.rotation.Inverse() * Thumb2RightInit;
            pitch2LInit = thumb2_left.transform.localEulerAngles.x;
            pitch3LInit = thumb3_left.transform.localEulerAngles.x;
            pitch2RInit = thumb2_right.transform.localEulerAngles.x;
            pitch3RInit = thumb3_right.transform.localEulerAngles.x;

            StartCoroutine(InitComPorts());
            footSteps.SetFootObj(this);
        }


        /// <summary>
        /// 初始化模型身体部位
        /// </summary>
        private void InitModelBody()
        {
            NodeObjDic.Clear();
            gameObject.transform.localEulerAngles = Vector3.zero;
            var children = gameObject.GetComponentsInChildren<Transform>(true);
            foreach (var item in children)
            {
                var name = item.name;
                if (!name.Contains(HCBodyName))
                    continue;
                var bodyName = name.Replace(HCBodyName, "");
                if (Enum.TryParse(bodyName, out Body body))
                {
                    if (IsFingerWithoutThumb(bodyName))
                    {
                        Vector3 goEuler = item.transform.localEulerAngles;
                        goEuler = VerifyFingerEuler(goEuler);
                        item.transform.localEulerAngles = goEuler;
                    }
                    if (NodeObjDic.ContainsKey(body))
                        NodeObjDic[body] = item.gameObject;
                    else
                        NodeObjDic.Add(body, item.gameObject);
                }
            }

            head = GetNode(Body.Head);
            chest = GetNode(Body.Chest);
            body = GetNode(Body.Abdomen);
            JumpCenter = body;
            shoulder_l = GetNode(Body.Shoulder_L);
            foreArm_l = GetNode(Body.Elbow_L);
            thigh_l = GetNode(Body.Hip_L);
            calf_l = GetNode(Body.Knee_L);
            foot_l = GetNode(Body.Foot_L);
            footEnd_l = GetNode(Body.Toe_End_L);
            hand_left = GetNode(Body.Hand_L);
            thumb1_left = GetNode(Body.Thumb1_L);
            thumb2_left = GetNode(Body.Thumb2_L);
            thumb3_left = GetNode(Body.Thumb3_L);
            index1_left = GetNode(Body.Index1_L);
            index2_left = GetNode(Body.Index2_L);
            index3_left = GetNode(Body.Index3_L);
            middle1_left = GetNode(Body.Middle1_L);
            middle2_left = GetNode(Body.Middle2_L);
            middle3_left = GetNode(Body.Middle3_L);
            ring1_left = GetNode(Body.Ring1_L);
            ring2_left = GetNode(Body.Ring2_L);
            ring3_left = GetNode(Body.Ring3_L);
            pinky1_left = GetNode(Body.Pinky1_L);
            pinky2_left = GetNode(Body.Pinky2_L);
            pinky3_left = GetNode(Body.Pinky3_L);

            shoulder_r = GetNode(Body.Shoulder_R);
            foreArm_r = GetNode(Body.Elbow_R);
            thigh_r = GetNode(Body.Hip_R);
            calf_r = GetNode(Body.Knee_R);
            foot_r = GetNode(Body.Foot_R);
            footEnd_r = GetNode(Body.Toe_End_R);
            hand_right = GetNode(Body.Hand_R);
            thumb1_right = GetNode(Body.Thumb1_R);
            thumb2_right = GetNode(Body.Thumb2_R);
            thumb3_right = GetNode(Body.Thumb3_R);
            index1_right = GetNode(Body.Index1_R);
            index2_right = GetNode(Body.Index2_R);
            index3_right = GetNode(Body.Index3_R);
            middle1_right = GetNode(Body.Middle1_R);
            middle2_right = GetNode(Body.Middle2_R);
            middle3_right = GetNode(Body.Middle3_R);
            ring1_right = GetNode(Body.Ring1_R);
            ring2_right = GetNode(Body.Ring2_R);
            ring3_right = GetNode(Body.Ring3_R);
            pinky1_right = GetNode(Body.Pinky1_R);
            pinky2_right = GetNode(Body.Pinky2_R);
            pinky3_right = GetNode(Body.Pinky3_R);
        }

        /// <summary>
        /// 除大拇指以外的手指
        /// </summary>
        /// <param name="body">身体部位</param>
        /// <returns></returns>
        private bool IsFingerWithoutThumb(string body)
        {
            foreach (var key in keys)
            {
                if (body.Contains(key))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 将手指关节原始角度修正为90°整数
        /// </summary>
        private Vector3 VerifyFingerEuler(Vector3 angle)
        {
            return new Vector3(VerifyAngleValue(angle.x), VerifyAngleValue(angle.y), VerifyAngleValue(angle.z));
        }
        private float VerifyAngleValue(float value)
        {
            float angleT = (value % 360 + 360) % 360;
            int result;
            if (Mathf.RoundToInt(angleT) % 90 <= 45)
            {
                result = Mathf.RoundToInt(angleT) - Mathf.RoundToInt(angleT) % 90;
            }
            else if (Mathf.RoundToInt(angleT) % 90 > 45)
            {
                result = Mathf.RoundToInt(angleT) + 90 - Mathf.RoundToInt(angleT) % 90;
            }
            else
            {
                result = Mathf.RoundToInt(angleT);
            }
            return result;
        }

        public GameObject GetNode(Body body)
        {
            if (NodeObjDic.ContainsKey(body))
                return NodeObjDic[body];
            return null;
        }


        public Vector3 GetEulerAngle1(Body body)
        {
            if (NodeObjDic.ContainsKey(body))
                return NodeObjDic[body].transform.eulerAngles;
            return Vector3.zero;
        }
        public Vector3 GetEulerAngle2(Body body)
        {
            if (NodeObjDic.ContainsKey(body))
                return NodeObjDic[body].transform.localEulerAngles;
            return Vector3.zero;
        }
        public Vector3 GetEulerAngle3(Quaternion _qua)
        {
            return _qua.eulerAngles;
        }

        /// <summary>
        /// 左手坐标系
        /// </summary>
        /// <param name="_qua"></param>
        /// <returns></returns>
        public Vector3 GetEulerAngle4(Quaternion _qua)
        {
            Vector3 eulerAngle = new Vector3(); ;
            // roll (x-axis rotation)
            float sinr_cosp = 2 * (_qua.w * _qua.x + _qua.y * _qua.z);
            float cosr_cosp = 1 - 2 * (_qua.x * _qua.x + _qua.y * _qua.y);
            eulerAngle.x = Mathf.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            float sinp = 2 * (_qua.w * _qua.y - _qua.z * _qua.x);
            if (Math.Abs(sinp) >= 1)
            {
                float val = Mathf.Sign(sinp);
                if (val >= 0)
                {
                    eulerAngle.y = Mathf.PI * 0.5f;
                }
                else
                {
                    eulerAngle.y = -Mathf.PI * 0.5f;
                }
            }
            else
                eulerAngle.y = Mathf.Asin(sinp);

            // yaw (z-axis rotation)
            float siny_cosp = 2 * (_qua.w * _qua.z + _qua.x * _qua.y);
            float cosy_cosp = 1 - 2 * (_qua.y * _qua.y + _qua.z * _qua.z);
            eulerAngle.z = Mathf.Atan2(siny_cosp, cosy_cosp);

            return eulerAngle;
        }

        /// <summary>
        /// 右手坐标系
        /// </summary>
        /// <param name="_qua"></param>
        /// <returns></returns>
        public Vector3 GetEulerAngle5(Quaternion _qua)
        {
            // Store the Euler angles in radians
            Vector3 pitchYawRoll = new Vector3();

            double sqw = _qua.w * _qua.w;
            double sqx = _qua.x * _qua.x;
            double sqy = _qua.y * _qua.y;
            double sqz = _qua.z * _qua.z;

            // If quaternion is normalised the unit is one, otherwise it is the correction factor
            double unit = sqx + sqy + sqz + sqw;
            double test = _qua.x * _qua.y + _qua.z * _qua.w;

            if (test > 0.4999f * unit)                              // 0.4999f OR 0.5f - EPSILON
            {
                // Singularity at north pole
                pitchYawRoll.y = 2f * (float)Math.Atan2(_qua.x, _qua.w);  // Yaw
                pitchYawRoll.x =(float)Math.PI * 0.5f;                         // Pitch
                pitchYawRoll.z = 0f;                                // Roll
                return pitchYawRoll;
            }
            else if (test < -0.4999f * unit)                        // -0.4999f OR -0.5f + EPSILON
            {
                // Singularity at south pole
                pitchYawRoll.y = -2f * (float)Math.Atan2(_qua.x, _qua.w); // Yaw
                pitchYawRoll.x = (float)-Math.PI * 0.5f;                        // Pitch
                pitchYawRoll.z = 0f;                                // Roll
                return pitchYawRoll;
            }
            else
            {
                pitchYawRoll.y = (float)Math.Atan2(2f * _qua.y * _qua.w - 2f * _qua.x * _qua.z, sqx - sqy - sqz + sqw);       // Yaw
                pitchYawRoll.x = (float)Math.Asin(2f * test / unit);                                             // Pitch
                pitchYawRoll.z = (float)Math.Atan2(2f * _qua.x * _qua.w - 2f * _qua.y * _qua.z, -sqx + sqy - sqz + sqw);      // Roll
            }

            return pitchYawRoll;
        }
        public  Vector3 GetEulerAngle6(Quaternion _qua)
        {
            float sqw = _qua.w * _qua.w;
            float sqx = _qua.x * _qua.x;
            float sqy = _qua.y * _qua.y;
            float sqz = _qua.z * _qua.z;
            float unit = sqx + sqy + sqz + sqw; 
            float test = _qua.x * _qua.w - _qua.y * _qua.z;
            Vector3 v;

            if (test > 0.4995f * unit)
            { 
                v.y = 2f * Mathf.Atan2(_qua.y, _qua.x);
                v.x = Mathf.PI / 2;
                v.z = 0;
                return NormalizeAngles(v * Mathf.Rad2Deg);
            }
            if (test < -0.4995f * unit)
            { 
                v.y = -2f * Mathf.Atan2(_qua.y, _qua.x);
                v.x = -Mathf.PI / 2;
                v.z = 0;
                return NormalizeAngles(v * Mathf.Rad2Deg);
            }
            Quaternion q = new Quaternion(_qua.w, _qua.z, _qua.x, _qua.y);
            v.y = (float)Math.Atan2(2f * q.x * q.w + 2f * q.y * q.z, 1 - 2f * (q.z * q.z + q.w * q.w));     
            v.x = (float)Math.Asin(2f * (q.x * q.z - q.w * q.y));                             
            v.z = (float)Math.Atan2(2f * q.x * q.y + 2f * q.z * q.w, 1 - 2f * (q.y * q.y + q.z * q.z));    
            return NormalizeAngles(v * Mathf.Rad2Deg);
        }


        private Vector3 NormalizeAngles(Vector3 angles)
        {
            angles.x = NormalizeAngle(angles.x);
            angles.y = NormalizeAngle(angles.y);
            angles.z = NormalizeAngle(angles.z);
            return angles;
        }
        private float NormalizeAngle(float angle)
        {
            while (angle > 360)
                angle -= 360;
            while (angle < 0)
                angle += 360;
            return angle;
        }

        // 释放资源，重置串口
        public void ResetCom()
        {
            Debug.Log("Resetting com with glove at " + ComName + "\t moca2 at " + MoCa2Com);
            try
            {
                bmc_MoCa2?.Dispose();
                bmc_Glove.Dispose();
            }
            catch
            {
            }
            Thread.Sleep(20);
            bmc_Glove = new BodyMotionCapture();
            if (!bmc_Glove.Start(ComName))
                Debug.LogWarning("MoCa glove start failed");
            Debug.Log("MoCa glove started at " + ComName);
            if (!string.IsNullOrEmpty(MoCa2Com))
            {
                bmc_MoCa2?.Dispose();
                Thread.Sleep(20);
                bmc_MoCa2 = new BodyMotionCapture();
                if (!bmc_MoCa2.Start(MoCa2Com))
                    Debug.LogWarning("MoCa2 start failed");
                Debug.Log("HexaCercle MoCa2 started at " + MoCa2Com);
            }
            if (!string.IsNullOrEmpty(MoCa2Com))
            {
                var footEulerL = ToUnityQuat(bmc_MoCa2.GetAnkleLeft(ToHcQuat(foot_l.transform.rotation))).eulerAngles.x;
                var footEulerR = ToUnityQuat(bmc_MoCa2.GetAnkleRight(ToHcQuat(foot_r.transform.rotation))).eulerAngles.x;
                footEuler = (footEulerL + footEulerR) / 2f;
                Debug.Log("set foot flat angle to " + footEuler + " by " + footEulerL + " and " + footEulerR);
            }
            else
            {
                var footEulerL = ToUnityQuat(bmc_Glove.GetAnkleLeft(ToHcQuat(foot_l.transform.rotation))).eulerAngles.x;
                var footEulerR = ToUnityQuat(bmc_Glove.GetAnkleRight(ToHcQuat(foot_r.transform.rotation))).eulerAngles.x;
                footEuler = (footEulerL + footEulerR) / 2f;
                Debug.Log("set foot flat angle to " + footEuler + " by " + footEulerL + " and " + footEulerR);
            }
        }

        private void Bmc_Glove_OnGestureChanged(BodyMotionCapture sender, EventArgs e)
        {
            if (bmc_Glove.GloveStatus[0] == GestureCode.Grip_Magazine)
            {
                throw new NotImplementedException();
            }
        }

        void Update()
        {
            footSteps?.Update();

            if (Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(Reset(3));
                return;
            }
   
            if (Paused)
            {
                resetModelAngle();
                return;
            }

            if (!string.IsNullOrEmpty(MoCa2Com)) //身体动捕
            {
                try
                {
                    setRotation(body, Body.Abdomen);
                    setRotation(chest, Body.Chest);
                    setRotation(head, Body.Head);
                }
                catch (DllNotFoundException e)
                {
                    Debug.LogError("HexaCercleAPI.Utils.dll missing" + e.Message);
                    return;
                }

                // 左半身 
                setRotation(shoulder_l, Body.Shoulder_L);
                setRotation(foreArm_l, Body.Elbow_L);
                setRotation(thigh_l, Body.Hip_L);
                setRotation(calf_l, Body.Knee_L);
                setRotation(foot_l, Body.Foot_L);
                // 右半身     
                setRotation(shoulder_r, Body.Shoulder_R);
                setRotation(foreArm_r, Body.Elbow_R);
                setRotation(thigh_r, Body.Hip_R);
                setRotation(calf_r, Body.Knee_R);
                setRotation(foot_r, Body.Foot_R);
            }

            if (!string.IsNullOrEmpty(ComName)) //手套动捕
            {
                Vector3 to;
                var sensi = new float[2][] { new float[] { 25, 25, 25, 25, 25 }, new float[] { 25, 25, 25, 25, 25 } };

                float YawCoef = 0f;
                if (sensi == null)
                    return;

                var leftPitch = (double[])bmc_Glove.GetGloveLeft()[0].Clone();
                var rightPitch = (double[])bmc_Glove.GetGloveRight()[0].Clone();
                var leftYaw = (double[])bmc_Glove.GetGloveLeft()[1].Clone();
                var rightYaw = (double[])bmc_Glove.GetGloveRight()[1].Clone();

                mcLPitchArray = leftPitch;
                mcRPitchArray = rightPitch;
                if (leftPitch[0] == 0 && rightPitch[0] == 0)
                    return;

                // 限制大拇指yaw角的最大值
                if (leftYaw[0] > 10) leftYaw[0] = 10;
                else if (leftPitch[0] > 60 && leftYaw[0] < -30) leftYaw[0] = -30;
                if (rightYaw[0] < -10) rightYaw[0] = -10;
                else if (rightYaw[0] > 50) rightYaw[0] = 50;

                if (offSetLeft?.Count != 4 || offSetLeft[0] == 0)
                {
                    offSetLeft = new List<float>
                    {
                        (float)leftYaw[1] * -1,
                        (float)leftYaw[2] * -1,
                        (float)leftYaw[3] * -1,
                        (float)leftYaw[4] * -1
                    };
                }
                if (offSetRight?.Count != 4 || offSetRight[0] == 0)
                {
                    offSetRight = new List<float>
                    {
                        (float)rightYaw[1] * -1,
                        (float)rightYaw[2] * -1,
                        (float)rightYaw[3] * -1,
                        (float)rightYaw[4] * -1
                    };
                }

                // 处理pitch转动超过180变为-180的情况
                for (int i = 0; i < 5; i++)
                {
                    if (i > 0 && leftPitch[i] < -100)
                        leftPitch[i] = leftPitch[i] + 360;
                    leftPitch[i] = (leftPitch[i] * sensi[0][i] / 50);
                    if (leftPitch[i] >= 90)
                        leftPitch[i] = 90;
                    // pitch小于0变为-1的情况
                    else if (leftPitch[i] <= -10 && i > 0) leftPitch[i] = -10;
                    if (leftYaw[i] == 255) leftYaw[i] = 0;

                    if (i > 0 && rightPitch[i] < -100)
                        rightPitch[i] = rightPitch[i] + 360;
                    rightPitch[i] = (rightPitch[i] * sensi[1][i] / 50);
                    if (rightPitch[i] >= 90) rightPitch[i] = 90;
                    if (rightYaw[i] == 255) rightYaw[i] = 0;

                    if (rightPitch[i] <= -10 && i > 0) rightPitch[i] = -10;
                }
                // 使用四元数设置转动
                setRotation(hand_left, Body.Hand_L);
                setRotation(hand_right, Body.Hand_R);
                setRotation(thumb2_left, Body.Thumb2_L);
                setRotation(thumb2_right, Body.Thumb2_R);

                // Thumb1, 拇指根部随动
                if (Thumb1LeftInit.normalized.w == 1)
                {
                    Thumb2LeftInit = thumb2_left.transform.localRotation;
                    Thumb1LeftInit = thumb1_left.transform.localRotation;
                }
                if (Thumb1RightInit.normalized.w == 1)
                {
                    Thumb2RightInit = thumb2_right.transform.localRotation;
                    Thumb1RightInit = thumb1_right.transform.localRotation;
                }
                thumb1_left.transform.localRotation = Quaternion.Lerp(Thumb1LeftInit,
                          Thumb1LeftInit * thumb2_left.transform.localRotation * Thumb2LeftInit.Inverse(),
                          0.3f);
                thumb1_right.transform.localRotation = Quaternion.Lerp(Thumb1RightInit,
                            Thumb1RightInit * thumb2_right.transform.localRotation * Thumb2RightInit.Inverse(),
                            0.3f);
                setRotation(thumb2_left, Body.Thumb2_L);
                setRotation(thumb2_right, Body.Thumb2_R);

                // Thumb3 拇指尖的y角随动, 因为万向节锁效果不太好
                {
                    var eul = thumb3_left.transform.localEulerAngles;
                    eul.x = (thumb2_left.transform.localEulerAngles.x - pitch2LInit) * 1.1f + pitch3LInit + 0;
                    if (eul.x > 360) eul.x -= 360;
                    else if (eul.x < 0) eul.x += 360;
                    if (eul.x > 90 && eul.x < 270) eul.x = 90;
                    if (eul.x >= 270 && eul.x <= 360) eul.x = 0;
                    thumb3_left.transform.localEulerAngles = eul;

                    eul = thumb3_right.transform.localEulerAngles;
                    eul.x = (thumb2_right.transform.localEulerAngles.x - pitch2RInit) * 1.1f + pitch3RInit + 0;
                    if (eul.x > 360) eul.x -= 360;
                    else if (eul.x < 0) eul.x += 360;
                    if (eul.x > 90 && eul.x < 270) eul.x = 90;
                    if (eul.x >= 270 && eul.x <= 360) eul.x = 0;
                    thumb3_right.transform.localEulerAngles = eul;
                }

                #region 转动手指播放动画---数据读取方法1
                // 使用欧拉角转动手指   效果约等于播放动画

                to = (MotionCaptureFingerData.index1_l) * (float)(leftPitch[1] / 90) + InitPose[Body.Index1_L];
                CalCoef(ref YawCoef, leftPitch[1]);
                to.y += ((float)leftYaw[1] + offSetLeft[0]) * YawCoef;
                setEulerAngle(index1_left, to);

                to = (MotionCaptureFingerData.index2_l) * (float)(leftPitch[1] / 90) + InitPose[Body.Index2_L];
                setEulerAngle(index2_left, to);

                to = (MotionCaptureFingerData.index3_l) * (float)(leftPitch[1] / 90) + InitPose[Body.Index3_L];
                setEulerAngle(index3_left, to);


                to = (MotionCaptureFingerData.middle1_l) * (float)(leftPitch[2] / 90) + InitPose[Body.Middle1_L];
                CalCoef(ref YawCoef, leftPitch[2]);
                to.y += ((float)leftYaw[2] + offSetLeft[1]) * YawCoef;
                setEulerAngle(middle1_left, to);

                to = (MotionCaptureFingerData.middle2_l) * (float)(leftPitch[2] / 90) + InitPose[Body.Middle2_L];
                setEulerAngle(middle2_left, to);

                to = (MotionCaptureFingerData.middle3_l) * (float)(leftPitch[2] / 90) + InitPose[Body.Middle3_L];
                setEulerAngle(middle3_left, to);



                to = (MotionCaptureFingerData.ring1_l) * (float)(leftPitch[3] / 90) + InitPose[Body.Ring1_L];
                CalCoef(ref YawCoef, leftPitch[3]);
                to.y += ((float)leftYaw[3] + offSetLeft[2]) * YawCoef;
                setEulerAngle(ring1_left, to);

                to = (MotionCaptureFingerData.ring2_l) * (float)(leftPitch[3] / 90) + InitPose[Body.Ring2_L];
                setEulerAngle(ring2_left, to);

                to = (MotionCaptureFingerData.ring3_l) * (float)(leftPitch[3] / 90) + InitPose[Body.Ring3_L];
                setEulerAngle(ring3_left, to);



                to = (MotionCaptureFingerData.pinky2_l) * (float)(leftPitch[4] / 90) + InitPose[Body.Pinky1_L];
                CalCoef(ref YawCoef, leftPitch[4]);
                to.y += ((float)leftYaw[4] + offSetLeft[3]) * YawCoef;
                setEulerAngle(pinky1_left, to);

                to = (MotionCaptureFingerData.pinky3_l) * (float)(leftPitch[4] / 90) + InitPose[Body.Pinky2_L];
                setEulerAngle(pinky2_left, to);

                to = (MotionCaptureFingerData.pinky4_l) * (float)(leftPitch[4] / 90) + InitPose[Body.Pinky3_L];
                setEulerAngle(pinky3_left, to);



                // right hand
                to = (MotionCaptureFingerData.index1_l) * (float)(rightPitch[1] / 90) + InitPose[Body.Index1_R];
                CalCoef(ref YawCoef, rightPitch[1]);
                to.y += ((float)rightYaw[1] + offSetRight[0]) * YawCoef;
                setEulerAngle(index1_right, to);

                to = (MotionCaptureFingerData.index2_l) * (float)(rightPitch[1] / 90) + InitPose[Body.Index2_R];
                setEulerAngle(index2_right, to);

                to = (MotionCaptureFingerData.index3_l) * (float)(rightPitch[1] / 90) + InitPose[Body.Index3_R];
                setEulerAngle(index3_right, to);




                to = (MotionCaptureFingerData.middle1_l) * (float)(rightPitch[2] / 90) + InitPose[Body.Middle1_R];
                CalCoef(ref YawCoef, rightPitch[2]);
                to.y -= ((float)rightYaw[2] + offSetRight[1]) * YawCoef;
                setEulerAngle(middle1_right, to);

                to = (MotionCaptureFingerData.middle2_l) * (float)(rightPitch[2] / 90) + InitPose[Body.Middle2_R];
                setEulerAngle(middle2_right, to);

                to = (MotionCaptureFingerData.middle3_l) * (float)(rightPitch[2] / 90) + InitPose[Body.Middle3_R];
                setEulerAngle(middle3_right, to);




                to = (MotionCaptureFingerData.ring1_l) * (float)(rightPitch[3] / 90) + InitPose[Body.Ring1_R];
                CalCoef(ref YawCoef, rightPitch[3]);
                to.y -= ((float)rightYaw[3] + offSetRight[2]) * YawCoef;
                setEulerAngle(ring1_right, to);

                to = (MotionCaptureFingerData.ring2_l) * (float)(rightPitch[3] / 90) + InitPose[Body.Ring2_R];
                setEulerAngle(ring2_right, to);

                to = (MotionCaptureFingerData.ring3_l) * (float)(rightPitch[3] / 90) + InitPose[Body.Ring3_R];
                setEulerAngle(ring3_right, to);



                to = (MotionCaptureFingerData.pinky2_l) * (float)(rightPitch[4] / 90) + InitPose[Body.Pinky1_R];
                CalCoef(ref YawCoef, rightPitch[4]);
                to.y -= ((float)rightYaw[4] + offSetRight[3]) * YawCoef;
                setEulerAngle(pinky1_right, to);

                to = (MotionCaptureFingerData.pinky3_l) * (float)(rightPitch[4] / 90) + InitPose[Body.Pinky2_R];
                setEulerAngle(pinky2_right, to);

                to = (MotionCaptureFingerData.pinky4_l) * (float)(rightPitch[4] / 90) + InitPose[Body.Pinky3_R];
                setEulerAngle(pinky3_right, to);
                #endregion
            }

#if FOOTSTEP
            if (!string.IsNullOrEmpty(MoCa2Com))
            {
                moveOnFoot();
            }
#endif
        }

        /// <summary>
        /// 得到当前身体动捕是否存在
        /// </summary>
        /// <returns></returns>
        public bool GetMontionCaptureState()
        {
            return !string.IsNullOrEmpty(MoCa2Com);
        }

        /// <summary>
        /// 得到当前手套动捕是否存在
        /// </summary>
        /// <returns></returns>
        public bool GetGloveState()
        {
            return !string.IsNullOrEmpty(ComName);
        }

        /// <summary>
        /// 得到身体Qua数据
        /// </summary>
        /// <returns></returns>
        public MCEvents.MCBodyData GetBodyData()
        {
            MCEvents.MCBodyData mCBodyData = new MCEvents.MCBodyData(47);
            for (int i = 0; i < 47; i++)
            {
                mCBodyData.mcDataArray[i] = GetNode((Body)i).transform.localRotation.ToQua();
            }
            return mCBodyData;
        }

        /// <summary>
        /// 得到身体Quaternion数据
        /// </summary>
        /// <returns></returns>
        public Quaternion[] GetBodyQuaternionData()
        {
            Quaternion[] mCBodyData = new Quaternion[47];
            for (int i = 0; i < 47; i++)
            {
                mCBodyData[i] = GetNode((Body)i).transform.localRotation;
            }
            return mCBodyData;
        }

#if FOOTSTEP
        private void moveOnFoot()
        {
            // 使用脚底压力传感器判断迈步   未配置时双脚都是255
            if (bmc_MoCa2.IsLeftAnkleEnabled && bmc_MoCa2.IsLeftAnkleEnabled
                && bmc_MoCa2.GetAnkleLeftPressureFront() != 255
                && bmc_MoCa2.GetAnkleRightPressureRear() != 255)
            {
                if (!bmc_MoCa2.IsAnkleLeftOnGround && !bmc_MoCa2.IsAnkleRightOnGround)
                {

                }
                else
                {
                    if (bmc_MoCa2.GetAnkleLeftPressureRear() <= bmc_MoCa2.GetAnkleRightPressureRear())
                    {
                        footSteps.MoveOnFoot = 1;
                    }
                    else
                    {
                        footSteps.MoveOnFoot = 2;
                    }
                }

            }
            else
            {

            }
        }
#endif
    
        private void setEulerAngle(GameObject target, Vector3 localEulerAngles)
        {
            if (target != null)
            {
                target.transform.localEulerAngles = localEulerAngles;
            }
        }

        /// <summary>
        /// 设置节点角度
        /// </summary>
        /// <param name="target"></param>
        /// <param name="position"></param>
        private void setRotation(GameObject target, Body position)
        {
            if (target == null)
                return;
            var _bmc = bmc_Glove;
            if (!string.IsNullOrEmpty(MoCa2Com) &&
                !(position == Body.Hand_L ||
                position == Body.Hand_R ||
                position == Body.Thumb2_L ||
                position == Body.Thumb2_R))
                _bmc = bmc_MoCa2;
            var hcQuat = ToHcQuat(target.transform.rotation);
            UnityEngine.Quaternion q = new UnityEngine.Quaternion();
            switch (position)
            {
                case Body.Shoulder_L:
                    q = ToUnityQuat(_bmc.GetShoulderLeft(hcQuat));
                    break;
                case Body.Elbow_L:
                    q = ToUnityQuat(_bmc.GetElbowLeft(hcQuat));
                    break;
                case Body.Hip_L:
                    q = ToUnityQuat(_bmc.GetHipLeft(hcQuat));
                    break;
                case Body.Knee_L:
                    q = ToUnityQuat(_bmc.GetKneeLeft(hcQuat));
                    break;
                case Body.Foot_L:
                    q = ToUnityQuat(_bmc.GetAnkleLeft(hcQuat));
                    break;
                case Body.Chest:
                    q = ToUnityQuat(_bmc.GetChest(hcQuat));
                    break;
                case Body.Abdomen:
                    q = ToUnityQuat(_bmc.GetAbdomen(hcQuat));
                    break;
                case Body.Head:
                    q = ToUnityQuat(_bmc.GetHead(hcQuat));
                    break;
                case Body.Shoulder_R:
                    q = ToUnityQuat(_bmc.GetShoulderRight(hcQuat));
                    break;
                case Body.Elbow_R:
                    q = ToUnityQuat(_bmc.GetElbowRight(hcQuat));
                    break;
                case Body.Hip_R:
                    q = ToUnityQuat(_bmc.GetHipRight(hcQuat));
                    break;
                case Body.Knee_R:
                    q = ToUnityQuat(_bmc.GetKneeRight(hcQuat));
                    break;
                case Body.Foot_R:
                    q = ToUnityQuat(_bmc.GetAnkleRight(hcQuat));
                    break;
                case Body.Hand_L:
                    q = ToUnityQuat(_bmc.GetWaistLeft(hcQuat));
                    break;
                case Body.Hand_R:
                    q = ToUnityQuat(_bmc.GetWaistRight(hcQuat));
                    break;
                case Body.Thumb2_L:
                    q = ToUnityQuat(_bmc.GetThumbLeft(hcQuat));
                    break;
                case Body.Thumb2_R:
                    q = ToUnityQuat(_bmc.GetThumbRight(hcQuat));
                    break;
            }
            if (isConnected(q))
            {
                target.transform.rotation = UnityEngine.Quaternion.Slerp(target.transform.rotation, q, 0.7f);

                //Debug.Log("系统值EulerAngle:" + target.transform.rotation.eulerAngles);
                //Debug.Log("系统值LocalEulerAngle:" + target.transform.localEulerAngles);
                //
                //Debug.Log("输出值EulerAngle:" + GetEulerAngle4(target.transform.rotation));
                //Debug.Log("输出值EulerAngle:" + GetEulerAngle5(target.transform.rotation));
                //Debug.Log("输出值EulerAngle:" + GetEulerAngle6(target.transform.rotation));
            }
        }

        /// <summary>
        /// 重置模型肢体和位置， 延迟单位为秒(s)
        /// </summary>
        private IEnumerator Reset(float delay)
        {
            if (!Paused)
            {
                Paused = true;
                MsgLog.EnableMsg();
                MsgLog.WriteLine("重启捕捉设备");
                bmc_Glove?.Dispose();
                bmc_MoCa2?.Dispose();
                bmc_Glove = null;
                bmc_MoCa2 = null;
#if FOOTSTEP
                footSteps.Reset();
                footSteps.SetEnable(true);
#endif
                try
                {
                    resetModelAngle();
                }
                catch { }
                offSetLeft.Clear();
                offSetRight.Clear();
                yield return new WaitForSeconds(delay);
                StartCoroutine(InitComPorts());
            }
        }

        // 强制模型回归Tpose
        private void resetModelAngle()
        {
            footSteps.SetEnable(true);
            setEulerAngle(shoulder_l, InitPose[Body.Shoulder_L]);
            setEulerAngle(foreArm_l, InitPose[Body.Elbow_L]);
            setEulerAngle(thigh_l, InitPose[Body.Hip_L]);
            setEulerAngle(calf_l, InitPose[Body.Knee_L]);
            setEulerAngle(chest, InitPose[Body.Chest]);
            setEulerAngle(body, InitPose[Body.Abdomen]);
            setEulerAngle(shoulder_r, InitPose[Body.Shoulder_R]);
            setEulerAngle(foreArm_r, InitPose[Body.Elbow_R]);
            setEulerAngle(thigh_r, InitPose[Body.Hip_R]);
            setEulerAngle(calf_r, InitPose[Body.Knee_R]);
            setEulerAngle(foot_l, InitPose[Body.Foot_L]);
            setEulerAngle(foot_r, InitPose[Body.Foot_R]);
            setEulerAngle(head, InitPose[Body.Head]);
            setEulerAngle(hand_left, InitPose[Body.Hand_L]);
            setEulerAngle(hand_right, InitPose[Body.Hand_R]);
            setEulerAngle(thumb1_left, InitPose[Body.Thumb1_L]);
            setEulerAngle(thumb1_right, InitPose[Body.Thumb1_R]);
            setEulerAngle(thumb2_left, InitPose[Body.Thumb2_L]);
            setEulerAngle(thumb2_right, InitPose[Body.Thumb2_R]);
            setEulerAngle(thumb3_left, InitPose[Body.Thumb3_L]);
            setEulerAngle(thumb3_right, InitPose[Body.Thumb3_R]);

            setEulerAngle(index1_left, InitPose[Body.Index1_L]);
            setEulerAngle(index2_left, InitPose[Body.Index2_L]);
            setEulerAngle(index3_left, InitPose[Body.Index3_L]);
            setEulerAngle(middle1_left, InitPose[Body.Middle1_L]);
            setEulerAngle(middle2_left, InitPose[Body.Middle2_L]);
            setEulerAngle(middle3_left, InitPose[Body.Middle3_L]);
            setEulerAngle(ring1_left, InitPose[Body.Ring1_L]);
            setEulerAngle(ring2_left, InitPose[Body.Ring2_L]);
            setEulerAngle(ring3_left, InitPose[Body.Ring3_L]);
            setEulerAngle(pinky1_left, InitPose[Body.Pinky1_L]);
            setEulerAngle(pinky2_left, InitPose[Body.Pinky2_L]);
            setEulerAngle(pinky3_left, InitPose[Body.Pinky3_L]);

            setEulerAngle(index1_right, InitPose[Body.Index1_R]);
            setEulerAngle(index2_right, InitPose[Body.Index2_R]);
            setEulerAngle(index3_right, InitPose[Body.Index3_R]);
            setEulerAngle(middle1_right, InitPose[Body.Middle1_R]);
            setEulerAngle(middle2_right, InitPose[Body.Middle2_R]);
            setEulerAngle(middle3_right, InitPose[Body.Middle3_R]);
            setEulerAngle(ring1_right, InitPose[Body.Ring1_R]);
            setEulerAngle(ring2_right, InitPose[Body.Ring2_R]);
            setEulerAngle(ring3_right, InitPose[Body.Ring3_R]);
            setEulerAngle(pinky1_right, InitPose[Body.Pinky1_R]);
            setEulerAngle(pinky2_right, InitPose[Body.Pinky2_R]);
            setEulerAngle(pinky3_right, InitPose[Body.Pinky3_R]);
        }

        // 判断四元数是否有效
        private bool isConnected(UnityEngine.Quaternion input)
        {
            return (Math.Pow(input.x, 2) + Math.Pow(input.y, 2) + Math.Pow(input.z, 2) + Math.Pow(input.w, 2) >= 0.95
                && input.w != 1);
        }

        // 随着pitch增大， yaw将逐渐失准， 限制其范围
        void CalCoef(ref float coef, double pitch)
        {
            coef = 1;
            if (pitch >= -10 && pitch < 0)
                coef = (float)((10 + pitch) / 10f);
            else if (pitch >= 0 && pitch < 30)
                coef = (float)((30 - pitch) / 30f);
            else if (pitch < -30 || pitch >= 30)
                coef = 0;
        }

        private UnityEngine.Quaternion ToUnityQuat(HexaCercleAPI.BodyMotion.Quaternion q)
        {
            return new UnityEngine.Quaternion(q.x, q.y, q.z, q.w);
        }
        private HexaCercleAPI.BodyMotion.Quaternion ToHcQuat(UnityEngine.Quaternion q)
        {
            return new HexaCercleAPI.BodyMotion.Quaternion(q.x, q.y, q.z, q.w);
        }

        private void OnDestroy()
        {
            bmc_Glove?.Dispose();
            bmc_MoCa2?.Dispose();
        }
    }
    public static class Extension
    {
        public static float SumOfPow2(this Queue<float> queue)
        {
            float re = 0;
            foreach (var f in queue)
                re += Mathf.Pow(f, 2);
            return re;
        }
    }
}