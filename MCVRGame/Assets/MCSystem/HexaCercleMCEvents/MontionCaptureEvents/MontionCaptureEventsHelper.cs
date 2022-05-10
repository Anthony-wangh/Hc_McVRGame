using System.Collections;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using MCData;
using Newtonsoft.Json;

namespace MCEvents
{
    public enum ERoleType
    { 
        ERT_Boy,
        ERT_Girl
    }
    /// <summary>
    /// 动捕数据辅助
    /// </summary>
    public class MontionCaptureEventsHelper : MonoBehaviour
    {
        public ERoleType eRoleType;
        private MotionCapture motionCapture = null;
        private MontionCaptureEventsSystem mcSystem = null;
        private MontionCaptureEventDic mcEventsDic = new MontionCaptureEventDic();  //总事件存储字典

        private void Start()
        {
            motionCapture = GetComponent<MotionCapture>();
            mcSystem = GetComponent<MontionCaptureEventsSystem>();

            DataReadToDic();  //读档
        }

        #region 数据存档与读档
        /// <summary>
        /// 数据读档  
        /// </summary>                                                                   
        /// 
        public void DataReadToDic()
        {
            string jsonPath = string.Empty;
            if (eRoleType == ERoleType.ERT_Boy)
            {
                jsonPath = MontionCapturePathConst.MCEventsDataInfoUrl_Boy;
            }
            else
            {
                jsonPath = MontionCapturePathConst.MCEventsDataInfoUrl_Girl;
            }
           
            string data = File.ReadAllText(jsonPath);
            mcEventsDic = JsonConvert.DeserializeObject<MontionCaptureEventDic>(data);
        }
        #endregion


        /// <summary>
        /// 查询事件
        /// </summary>
        /// <param name="_name"></param>
        /// <returns></returns>
        public MontionCaptureEventData SelectEvent(EMCEType _type)
        {
            return mcEventsDic.SelectEvent(_type);
        }

        /// <summary>
        /// 得到所有事件字典 
        /// </summary>
        public Dictionary<EMCEType, MontionCaptureEventData> GetAllEventsDic()
        {
            return mcEventsDic.GetAllEventsData();
        }

        /// <summary>
        /// 查询手套数据是否连接
        /// </summary>
        /// <returns></returns>
        public bool SelectMCHandState()
        {
            return motionCapture.GetGloveState();
        }

        /// <summary>
        /// 查询动捕数据是否连接
        /// </summary>
        /// <returns></returns>
        public bool SelectMCBodyState()
        {
            return motionCapture.GetMontionCaptureState();
        }

        /// <summary>
        /// 得到字典数量
        /// </summary>
        /// <returns></returns>
        public int SelectEventCount()
        {
            return mcEventsDic.SelectEventCount();
        }

        /// <summary>
        /// 得到事件触发器
        /// </summary>
        /// <param name="_type">事件类型</param>
        /// <param name="_index"> 事件下标</param>
        /// <returns></returns>
        public bool GetEventTrigger(EMCEType _type,int _index = 0)
        {
            if (!mcSystem.isOpen)
            {
                return false;
            }
            
            if (SelectEvent(_type).captureCount == 1)
            {
                return SelectEvent(_type).motionTrigger;
            }
            else if(SelectEvent(_type).captureCount >= _index)
            {
                return SelectEvent(_type).expandData[_index].motionTrigger;
            }
            else
            {
                return false;
            }
        }
    }
}
