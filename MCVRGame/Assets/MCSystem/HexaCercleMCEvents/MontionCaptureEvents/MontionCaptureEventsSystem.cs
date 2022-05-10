using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCData;
namespace MCEvents
{
    public class MontionCaptureEventsSystem : MonoBehaviour
    {
        #region 基础数据
        private MotionCapture motionCapture = null;
        private MontionCaptureEventsHelper mcHelper = null;
        private Dictionary<EMCEType, MontionCaptureEventData> mcEventsDic = null;


        private Transform chest, head, abdomen,
                  shoulder_l, elbow_l, hip_l, knee_l, foot_l, footEnd_l,
                  shoulder_r, elbow_r, hip_r, knee_r, foot_r, footEnd_r;

        private Transform lFinger_1, lFinger_2, lFinger_3, lFinger_4, lFinger_5;
        private Transform rFinger_1, rFinger_2, rFinger_3, rFinger_4, rFinger_5;

        #endregion

        #region 拓展数据
        public bool isOpen = false;             //打开
        public bool isPause = false;            //暂停
        //public bool isGetHandState = false;     //使用手套
        //public bool isGetBodyState = false;     //使用动捕

        #endregion


        #region 计算数据
       
        #endregion


        private void Start()
        {
            motionCapture = GetComponent<MotionCapture>();
            mcHelper = GetComponent<MontionCaptureEventsHelper>();
            StartCoroutine(InitBodyGameObject());
        }

        private void Update()
        {
            #region 测试

            if (!isOpen || isPause || mcEventsDic?.Count == 0)
            {
                return;
            }

            MCEventsUpdate();
            #endregion
        }

        /// <summary>
        /// 初始化身体对象
        /// </summary>
        public IEnumerator InitBodyGameObject()
        {
            yield return new WaitForSeconds(1);
            if (motionCapture == null)
            {
                Debug.LogWarning("MotionCapture为空...");
                yield return null;
            }
            //Debug.Log("MotionCapture已经初始化...");
            head = motionCapture.GetNode(Body.Head).transform;
            chest = motionCapture.GetNode(Body.Chest).transform;
            abdomen = motionCapture.GetNode(Body.Abdomen).transform;

            shoulder_l = motionCapture.GetNode(Body.Shoulder_L).transform;
            elbow_l = motionCapture.GetNode(Body.Elbow_L).transform;
            hip_l = motionCapture.GetNode(Body.Hip_L).transform;
            knee_l = motionCapture.GetNode(Body.Knee_L).transform;
            foot_l = motionCapture.GetNode(Body.Foot_L).transform;
            footEnd_l = motionCapture.GetNode(Body.Toe_End_L).transform;

            shoulder_r = motionCapture.GetNode(Body.Shoulder_R).transform;
            elbow_r = motionCapture.GetNode(Body.Elbow_R).transform;
            hip_r = motionCapture.GetNode(Body.Hip_R).transform;
            knee_r = motionCapture.GetNode(Body.Knee_R).transform;
            foot_r = motionCapture.GetNode(Body.Foot_R).transform;
            footEnd_r = motionCapture.GetNode(Body.Toe_End_R).transform;

            lFinger_1 = motionCapture.GetNode(Body.Thumb2_L).transform;
            lFinger_2 = motionCapture.GetNode(Body.Index2_L).transform;
            lFinger_3 = motionCapture.GetNode(Body.Middle2_L).transform;
            lFinger_4 = motionCapture.GetNode(Body.Ring2_L).transform;
            lFinger_5 = motionCapture.GetNode(Body.Pinky2_L).transform;

            rFinger_1 = motionCapture.GetNode(Body.Thumb2_R).transform;
            rFinger_2 = motionCapture.GetNode(Body.Index2_R).transform;
            rFinger_3 = motionCapture.GetNode(Body.Middle2_R).transform;
            rFinger_4 = motionCapture.GetNode(Body.Ring2_R).transform;
            rFinger_5 = motionCapture.GetNode(Body.Pinky2_R).transform;

            mcEventsDic = mcHelper.GetAllEventsDic();
            isOpen = true;
        }

        /// <summary>
        /// 动捕事件更新
        /// </summary>
        private void MCEventsUpdate()
        {
            foreach (var item in mcEventsDic)
            {
                MCEventUpdate(item.Value);
            }
        }

        /// <summary>
        /// 事件更新
        /// </summary>
        /// <param name="_eventName"></param>
        public void MCEventUpdate(MontionCaptureEventData eventData)
        {
            if (eventData.eventType == EMCEType.EMCET_None)
            {
                //Debug.LogError("MontionCaptureEventsSystem--动捕事件为空： " + eventData.eventType);
                return;
            }

            int motionCount = eventData.captureCount;

            for (int j = 0; j < motionCount; j++) //当前事件动作数量
            {
                eventData.expandData[j].motionCoolTime -= Time.deltaTime;   //动作持续事件---自定义
                if (eventData.expandData[j].motionCoolTime < 0)
                {
                    eventData.expandData[j].motionCoolTime = 0;
                    eventData.expandData[j].motionTrigger = false;
                    if (j == motionCount - 1) //当前动作是最后一个动作
                    {
                        eventData.motionTrigger = false;
                        //Debug.Log(eventData.eventType + "动作事件未触发");
                    }
                }
                if (j == 0)  //第一个动作
                {
                    if (CompareEventData(eventData, 0)) //触发事件
                    {
                        eventData.expandData[0].delayCoolTime -= Time.deltaTime;
                        if (eventData.expandData[0].delayCoolTime < 0)
                        {
                            eventData.expandData[0].delayCoolTime = 0;
                            eventData.expandData[0].motionTrigger = true;
                            eventData.expandData[0].motionCoolTime = eventData.expandData[0].motionTime;
                            if (1 == motionCount)
                            {
                                eventData.motionTrigger = true;
                               // Debug.Log(eventData.eventType + "已经触发");
                            }
                        }
                    }
                    else
                    {
                        eventData.expandData[j].delayCoolTime = eventData.expandData[j].delayTime;
                    }
                }
                else  //非第一个动作
                {
                    if (eventData.expandData[j - 1].motionTrigger && CompareEventData(eventData, j)) //触发事件
                    {
                        eventData.expandData[j].delayCoolTime -= Time.deltaTime;
                        if (eventData.expandData[j].delayCoolTime < 0)
                        {
                            eventData.expandData[0].delayCoolTime = 0;
                            eventData.expandData[j].motionTrigger = true;
                            eventData.expandData[j].motionCoolTime = eventData.expandData[j].motionTime;
                            if (j == motionCount - 1)
                            {
                                eventData.motionTrigger = true;
                               // Debug.Log(eventData.eventType + "已经触发");
                            }
                        }
                    }
                    else
                    {
                        eventData.expandData[j].delayCoolTime = eventData.expandData[j].delayTime;
                    }
                }
                //Debug.Log(eventData.eventType +"当前冷却时间："+eventData.expandData[j].motionCoolTime);
            }
        }

        /// <summary>
        ///  比较四元数
        /// </summary>
        /// <param name="_fir">存储数据</param>
        /// <param name="_sec">当前身体数据</param>
        /// <param name="_val">偏差值</param>
        /// <returns></returns>
        public bool CompareQua(Qua _fir, Quaternion _sec, float _val)
        {
            float tmp = Mathf.Clamp01(Quaternion.Dot(_fir.ToQua(), _sec));
            bool ret = tmp >= _val;
            return ret;
        }

        /// <summary>
        ///  比较事件数据---事件是否触发
        /// </summary>
        /// <param name="_name"></param>
        /// <param name="_pos"></param>
        /// mcDataArray 的索引 参考  ESelectBody枚举---> eg: mcDataArray[0]==  mcDataArray[(int)ESelectBody.Thumb1_L]
        /// <returns></returns>
        private bool CompareEventData(MontionCaptureEventData eventData,int _pos)
        {
            bool ret = true;
            bool[] selectBodyArray = eventData.selectBodymappingArray;
            Qua[] mcDataArray = eventData.expandData[_pos].bodyData;
            float[] dataOffectArray = eventData.expandData[_pos].dataOffectArray;

            //左手
            if (selectBodyArray[(int)ESelectBody.Hand_L])
            {
                if ( !(/*CompareQua(mcDataArray[(int)ESelectBody.Thumb2_L], lFinger_1.localRotation, dataOffectArray[(int)ESelectBody.Thumb2_L]) &&*/
                     CompareQua(mcDataArray[(int)ESelectBody.Index2_L], lFinger_2.localRotation, dataOffectArray[(int)ESelectBody.Index2_L]) &&
                     CompareQua(mcDataArray[(int)ESelectBody.Middle2_L], lFinger_3.localRotation, dataOffectArray[(int)ESelectBody.Middle2_L]) &&
                     CompareQua(mcDataArray[(int)ESelectBody.Ring2_L], lFinger_4.localRotation, dataOffectArray[(int)ESelectBody.Ring2_L]) &&
                     CompareQua(mcDataArray[(int)ESelectBody.Pinky2_L], lFinger_5.localRotation, dataOffectArray[(int)ESelectBody.Pinky2_L])))
                     ret = false;
            }
            //右手
            if (selectBodyArray[(int)ESelectBody.Hand_R])
            {
                if(!(/*CompareQua(mcDataArray[(int)ESelectBody.Thumb2_R], rFinger_1.localRotation, dataOffectArray[(int)ESelectBody.Thumb2_R]) &&*/
                    CompareQua(mcDataArray[(int)ESelectBody.Index2_R], rFinger_2.localRotation, dataOffectArray[(int)ESelectBody.Index2_R]) &&
                    CompareQua(mcDataArray[(int)ESelectBody.Middle2_R], rFinger_3.localRotation, dataOffectArray[(int)ESelectBody.Middle2_R]) &&
                    CompareQua(mcDataArray[(int)ESelectBody.Ring2_R], rFinger_4.localRotation, dataOffectArray[(int)ESelectBody.Ring2_R]) &&
                    CompareQua(mcDataArray[(int)ESelectBody.Pinky2_R], rFinger_5.localRotation, dataOffectArray[(int)ESelectBody.Pinky2_R])))
                    ret = false;
            }
            //头
            if (selectBodyArray[(int)ESelectBody.Head])
            {
                if (!CompareQua(mcDataArray[(int)ESelectBody.Head], head.localRotation, dataOffectArray[(int)ESelectBody.Head])) ret = false;
            }
            //胸
            if (selectBodyArray[(int)ESelectBody.Chest])
            {
                if (!(CompareQua(mcDataArray[(int)ESelectBody.Chest], chest.localRotation, dataOffectArray[(int)ESelectBody.Chest]) &&
                     CompareQua(mcDataArray[(int)ESelectBody.Abdomen], abdomen.localRotation, dataOffectArray[(int)ESelectBody.Abdomen])))
                     ret = false;
            }
            //左臂
            if (selectBodyArray[(int)ESelectBody.Shoulder_L])
            {
                if (!(CompareQua(mcDataArray[(int)ESelectBody.Shoulder_L], shoulder_l.localRotation, dataOffectArray[(int)ESelectBody.Shoulder_L]) &&
                     CompareQua(mcDataArray[(int)ESelectBody.Elbow_L], elbow_l.localRotation, dataOffectArray[(int)ESelectBody.Elbow_L])))
                     ret = false;
            }
            //左腿
            if (selectBodyArray[(int)ESelectBody.Hip_L])
            {
                if (!(CompareQua(mcDataArray[(int)ESelectBody.Hip_L], hip_l.localRotation, dataOffectArray[(int)ESelectBody.Hip_L])&&
                     CompareQua(mcDataArray[(int)ESelectBody.Knee_L], knee_l.localRotation, dataOffectArray[(int)ESelectBody.Knee_L])))
                     ret = false;
            }
            //左脚
            if (selectBodyArray[(int)ESelectBody.Foot_L])
            {
                if (!(CompareQua(mcDataArray[(int)ESelectBody.Foot_L], foot_l.localRotation, dataOffectArray[(int)ESelectBody.Foot_L]) &&
                     CompareQua(mcDataArray[(int)ESelectBody.Toe_End_L], footEnd_l.localRotation, dataOffectArray[(int)ESelectBody.Toe_End_L])))
                     ret = false;
            }
            //右臂
            if (selectBodyArray[(int)ESelectBody.Shoulder_R])
            {
                if (!(CompareQua(mcDataArray[(int)ESelectBody.Shoulder_R], shoulder_r.localRotation, dataOffectArray[(int)ESelectBody.Shoulder_R]) &&
                     CompareQua(mcDataArray[(int)ESelectBody.Elbow_R], elbow_r.localRotation, dataOffectArray[(int)ESelectBody.Elbow_R])))
                     ret = false;
            }
            //右腿
            if (selectBodyArray[(int)ESelectBody.Hip_R])
            {
                if (!(CompareQua(mcDataArray[(int)ESelectBody.Hip_R], hip_r.localRotation, dataOffectArray[(int)ESelectBody.Hip_R])&&
                     CompareQua(mcDataArray[(int)ESelectBody.Knee_R], knee_r.localRotation, dataOffectArray[(int)ESelectBody.Knee_R])))
                     ret = false;
            }
            //右脚
            if (selectBodyArray[(int)ESelectBody.Foot_R])
            {
                if (!(CompareQua(mcDataArray[(int)ESelectBody.Foot_R], foot_r.localRotation, dataOffectArray[(int)ESelectBody.Foot_R])&&
                     CompareQua(mcDataArray[(int)ESelectBody.Toe_End_R], footEnd_r.localRotation, dataOffectArray[(int)ESelectBody.Toe_End_R])))
                     ret = false;
            }
            return ret;
        }
    }

    public class CalculateAverageResult
    {
       public MCBodyData bodyRetArray = new MCBodyData(47);//身体四元数结果数据
       public float[] dotRetArray = new float[47];     //身体计算点乘结果
    }
}
