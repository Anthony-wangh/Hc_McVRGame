using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MCEvents
{
    public class MontionCaptureEventDic
    {
        #region 基础数据
        /// <summary>
        /// 所有事件 名称 - 动捕数据 字典
        /// </summary>
        public readonly Dictionary<EMCEType, MontionCaptureEventData> dic = new Dictionary<EMCEType, MontionCaptureEventData>();
        #endregion

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="_event"></param>
        /// <returns></returns>
        public bool AddEvent(EMCEType _type, MontionCaptureEventData _event)
        {
            if (!dic.ContainsKey(_type))
            {
                dic.Add(_type, _event);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 删除事件
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public bool RemoveEvent(EMCEType _type)
        {
            if (dic.ContainsKey(_type))
            {
                dic.Remove(_type);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 修改事件
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="_event"></param>
        /// <returns></returns>
        public bool EditEvent(EMCEType _type, MontionCaptureEventData _event)
        {
            if (!dic.ContainsKey(_type))
            {
                return false;
            }
            dic.Remove(_type);
            dic.Add(_type, _event);
            return true;
        }

        /// <summary>
        /// 查询事件
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public MontionCaptureEventData SelectEvent(EMCEType _type)
        {
            if (dic.ContainsKey(_type))
            {
                return dic[_type];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///  是否包含事件数据
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public bool ContainEvent(EMCEType _type)
        {
            return dic.ContainsKey(_type);
        }

        /// <summary>
        /// 得到所有事件数据
        /// </summary>
        /// <returns></returns>
        public Dictionary<EMCEType, MontionCaptureEventData> GetAllEventsData()
        {
            return dic;
        }

        /// <summary>
        /// 得到字典数量
        /// </summary>
        /// <returns></returns>
        public int SelectEventCount()
        {
            return dic.Count;
        }
    }
}
