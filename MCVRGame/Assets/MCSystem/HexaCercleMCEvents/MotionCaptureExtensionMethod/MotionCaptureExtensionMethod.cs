using System;
using UnityEngine;

/// <summary>
/// 动捕拓展方法
/// </summary>
public static class MotionCaptureExtensionMethod
{
    /// <summary>
        /// 四元数转换
        /// </summary>
        /// <param name="qua"></param>
        /// <returns></returns>
    public static Qua ToQua(this Quaternion qua)
    {
        string x = qua.x.ToString("0.00000");
        string y = qua.y.ToString("0.00000");
        string z = qua.z.ToString("0.00000");
        string w = qua.w.ToString("0.00000");


        return new Qua(float.Parse(x), float.Parse(y), float.Parse(z), float.Parse(w));
    }
    public static string ToString2(this Quaternion q)
    {
        return q.x.ToString("0.00") + ", " + q.y.ToString("0.00") + ", " + q.z.ToString("0.00") + ", " + q.w.ToString("0.00");
    }

    /// <summary>
    /// 字符串取中间值
    /// </summary>
    /// <param name="sourse">目标字符串</param>
    /// <param name="startstr">第一个字符串</param>
    /// <param name="endstr">第二个字符串</param>
    /// <returns></returns>
    public static string MidStrEx(this string sourse, string startstr, string endstr)
    {
        string result = string.Empty;
        int startindex, endindex;
        try
        {
            startindex = sourse.IndexOf(startstr);
            if (startindex == -1)
                return result;
            string tmpstr = sourse.Substring(startindex + startstr.Length);
            endindex = tmpstr.IndexOf(endstr);
            if (endindex == -1)
                return result;
            result = tmpstr.Remove(endindex);
        }
        catch (Exception ex)
        {
            Debug.LogError("MidStrEx Err:" + ex.Message);
        }
        return result;
    }


    /// <summary>
    /// Float字符串加减计算
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sign"></param>
    /// <param name="_val"></param>
    /// <returns></returns>
    public static string CalculateFloatVal(this string source, ESign sign,float _val)
    {
        if (float.TryParse(source, out float val))
        {
            switch (sign)
            {
                case ESign.ES_Reduce:
                    val -= _val;
                    break;
                case ESign.ES_Add:
                    val += _val;
                    break;
                case ESign.ES_Rest:
                default:
                    val = 0;
                    break;
            }
        }
        else
        {
            Debug.Log("Float字符串加减计算source转换Float失败：" + source);
        }
        return val.ToString();
    }
}

/// <summary>
/// 自定义四元数类型
/// </summary>
public struct Qua
{
    public float x;
    public float y;
    public float z;
    public float w;

    public Qua(float val1, float val2, float val3, float val4)
    {
        x = val1;
        y = val2;
        z = val3;
        w = val4;
    }

    public Quaternion ToQua()
    {
        return new Quaternion(x, y, z, w);
    }
}

/// <summary>
/// 加减符号
/// </summary>
public enum ESign
{
    ES_Reduce,
    ES_Add,
    ES_Rest
}




