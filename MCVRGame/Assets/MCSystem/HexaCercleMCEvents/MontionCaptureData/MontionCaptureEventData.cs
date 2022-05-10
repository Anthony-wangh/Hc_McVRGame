using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件枚举
/// </summary>
public enum EMCEType
{
    EMCET_None         = 0,
    EMCET_HandOpen_L      ,
    EMCET_HandClaw_L      ,
    EMCET_HandFist_L      ,
    EMCET_Mitsurugi_L     ,
    EMCET_Ridicule_L      ,
    EMCET_HandOpen_R      ,
    EMCET_HandCRaw_R      ,
    EMCET_HandFist_R      ,
    EMCET_Mitsurugi_R     ,
    EMCET_RidicuRe_R      , 
}

namespace MCEvents
{
    /// <summary>
    /// 身体选择枚举
    /// </summary>
    public enum ESelectBody
    {
        Thumb1_L,   //左拇指1  
        Thumb2_L,   //左拇指2
        Thumb3_L,   //左拇指3
        Index1_L,   //左食指1
        Index2_L,   //左食指2
        Index3_L,   //左食指3
        Middle1_L,  //左中指1
        Middle2_L,  //左中指2
        Middle3_L,  //左中指3
        Ring1_L,    //左无名指1
        Ring2_L,    //左无名指2
        Ring3_L,    //左无名指3
        Pinky1_L,   //左小指1
        Pinky2_L,   //左小指2
        Pinky3_L,   //左小指3

        Thumb1_R,   //右拇指1               
        Thumb2_R,   //右拇指2
        Thumb3_R,   //右拇指3
        Index1_R,   //右食指1
        Index2_R,   //右食指2
        Index3_R,   //右食指3
        Middle1_R,  //右中指1
        Middle2_R,  //右中指2
        Middle3_R,  //右中指3
        Ring1_R,    //右无名指1
        Ring2_R,    //右无名指2
        Ring3_R,    //右无名指3
        Pinky1_R,   //右小指1
        Pinky2_R,   //右小指2
        Pinky3_R,   //右小指3
                    //
        Head,       //头                    
        Chest,      //胸  
        Abdomen,    //腹  

        Shoulder_L, //左肩                  
        Elbow_L,    //左肘  
        Hand_L,     //左手                  
        Hip_L,      //左大腿      
        Knee_L,     //左小腿      
        Foot_L,     //左脚        
        Toe_End_L,  //左脚尖
                    //
        Shoulder_R, //右肩         
        Elbow_R,    //右肘         
        Hand_R,     //右手         
        Hip_R,      //右大腿      
        Knee_R,     //右小腿      
        Foot_R,     //右脚        
        Toe_End_R,  //右脚尖      
    }

    /// <summary>
    /// 动捕事件拓展数据：动作触发，动作间隔时间，动作间隔时间计时器，单个动作持续时长，单个动作作持续计时器 
    /// </summary>
    public class MCEEexpandData
    {
        /// <summary>
        /// 动作触发       
        /// </summary>
        public bool  motionTrigger;

        /// <summary>
        /// 动作间隔时间---自定义
        /// </summary>
        public float motionTime;

        /// <summary>
        /// 动作间隔时间计时器---自定义
        /// </summary>
        public float motionCoolTime;

        /// <summary>
        /// 单个动作持续时长---固定值0.1f
        /// </summary>
        public float delayTime;

        /// <summary>
        /// 单个动作作持续计时器---固定值0.1f
        /// </summary>
        public float delayCoolTime;

        /// <summary>
        /// 身体部位所有的差值  47
        /// </summary>
        public float[] dataOffectArray;

        /// <summary>
        /// 身体数据
        /// </summary>
        public Qua[] bodyData;
    }

    /// <summary>
    /// 动捕身体数据
    /// </summary>
    public class MCBodyData
    {
        public Qua[] mcDataArray; //身体数据
        public MCBodyData(int _count)
        {
            mcDataArray = new Qua[_count];
        }
    }

    public class MontionCaptureEventData
    {
        #region 固定数据
        /// <summary>
        /// 固定动作最大数量
        /// </summary>
        public static int CAPTURE_MAX_COUNT = 5;

        /// <summary>
        /// 固定身体选择最大数量
        /// </summary>
        public static int BODY_MAX_COUNT = 47;

        /// <summary>
        /// 固定动作持续时长
        /// </summary>
        public static float CAPTURE_DELAY_TIME = 0.1f;

        /// <summary>
        /// 固定手指最大数量
        /// </summary>
        public static int FINGER_MAX_COUNT = 5;

        /// <summary>
        /// 身体最大选择部位
        /// </summary>
        public const int BODY_MAX_SELECT_COUNT = 10;

        public const int INPUT_MAX_COUNT = 100;
        #endregion

        #region 基础数据
        /// <summary>
        /// 动作类型
        /// </summary>
        public EMCEType eventType;

        /// <summary>
        /// 动作数量
        /// </summary>
        public  int captureCount;

        /// <summary>
        ///  是否启用身体部位  10
        /// </summary>
        public  bool[] selectBodyArray;


        public bool[] selectBodymappingArray;
        #endregion

        #region 配置数据
        public int inputCount;
        #endregion

        #region 拓展数据

        /// <summary>
        /// 动作触发器
        /// </summary>
        public bool motionTrigger;

        /// <summary>
        /// 拓展数据数组：动作触发，动作间隔时间，动作间隔时间计时器，单个动作持续时长，单个动作作持续计时器  
        /// </summary>
        public MCEEexpandData[] expandData;
        #endregion

        public MontionCaptureEventData(EMCEType _type, int _eventCount,int _inputCount, bool[] _selectBodyArray,float[] _motionTimeArray)
        {
            eventType = _type;
            captureCount = _eventCount > CAPTURE_MAX_COUNT ? CAPTURE_MAX_COUNT : _eventCount;
            inputCount = _inputCount > INPUT_MAX_COUNT ? INPUT_MAX_COUNT : _inputCount;
            motionTrigger = false;

            selectBodyArray = _selectBodyArray;
            selectBodymappingArray = MappingSelectBodyArray(_selectBodyArray);
            expandData = new MCEEexpandData[captureCount];

            for (int i = 0; i < captureCount; i++)
            {
                expandData[i] = new MCEEexpandData();
                expandData[i].bodyData = new Qua[BODY_MAX_COUNT];
                expandData[i].delayTime = CAPTURE_DELAY_TIME;
                expandData[i].dataOffectArray = new float[BODY_MAX_COUNT];
               
                for (int j = 0; j < BODY_MAX_COUNT; j++)
                {
                    expandData[i].dataOffectArray[j] = CAPTURE_DELAY_TIME;
                }
                expandData[i].motionTime = _motionTimeArray[i];
            }
        }
                                                                                
        public MontionCaptureEventData(MontionCaptureEventData _data)
        {
            eventType = _data.eventType;
            captureCount = _data.captureCount;
            motionTrigger = _data.motionTrigger;

            selectBodyArray = _data.selectBodyArray;
            selectBodymappingArray = _data.selectBodymappingArray;
            expandData = _data.expandData;
            inputCount = _data.inputCount;
        }

        public MontionCaptureEventData(){}

        public void InitData()
        {
            eventType = EMCEType.EMCET_None;
            captureCount = 1;
            motionTrigger = false;
            selectBodyArray = new bool[BODY_MAX_SELECT_COUNT];
            selectBodymappingArray = new bool[BODY_MAX_COUNT];
            expandData = new MCEEexpandData[captureCount];

            for (int i = 0; i < captureCount; i++)
            {
                expandData[i].bodyData = new Qua[BODY_MAX_COUNT];
                expandData[i].delayTime = CAPTURE_DELAY_TIME;
                expandData[i].dataOffectArray = new float[BODY_MAX_COUNT];

                for (int j = 0; j < BODY_MAX_COUNT; j++)
                {
                    expandData[i].dataOffectArray[i] = CAPTURE_DELAY_TIME;
                }
            }
        }

        //保存身体偏差值数据
        public void SaveCalculateAverageResult(int _cueCapture, CalculateAverageResult _ret)
        {
            for (int i = 0; i < BODY_MAX_COUNT; i++)
            {
                expandData[_cueCapture].bodyData[i] = _ret.bodyRetArray.mcDataArray[i];
                expandData[_cueCapture].dataOffectArray[i] = _ret.dotRetArray[i];
            }
        }

        private bool[] MappingSelectBodyArray(bool[] _selectBodyArray)
        {
            bool[] retArray = new bool[BODY_MAX_COUNT];

            //左手
            for (int i = 0; i < 15; i++)
            {
                retArray[i] = _selectBodyArray[0];
            }
            retArray[35] = _selectBodyArray[0];

            //右手
            for (int i = 15; i < 30; i++)
            {
                retArray[i] = _selectBodyArray[1];
            }
            retArray[42] = _selectBodyArray[1];

          

            //左臂
            retArray[33] = _selectBodyArray[2];
            retArray[34] = _selectBodyArray[2];

            //右臂
            retArray[40] = _selectBodyArray[3];
            retArray[41] = _selectBodyArray[3];

            //头
            retArray[30] = _selectBodyArray[4];

            //胸腹
            retArray[31] = _selectBodyArray[5];
            retArray[32] = _selectBodyArray[5];

            //左腿
            retArray[36] = _selectBodyArray[6];
            retArray[37] = _selectBodyArray[6];

            //右腿
            retArray[43] = _selectBodyArray[7];
            retArray[44] = _selectBodyArray[7];

            //左脚
            retArray[38] = _selectBodyArray[8];
            retArray[39] = _selectBodyArray[8];

            //右脚
            retArray[45] = _selectBodyArray[9];
            retArray[46] = _selectBodyArray[9];
            return retArray;
        }
    }
}
