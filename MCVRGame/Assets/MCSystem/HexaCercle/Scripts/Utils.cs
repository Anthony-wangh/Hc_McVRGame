using System.Globalization;
using UnityEngine;

public static class Utils
{
    public static GameObject TryFind(params string[] keyWords)
    {
        var objs = GameObject.FindObjectsOfType<GameObject>();
        foreach (var obj in objs)
        {
            bool keyMatched = true;
            foreach (var key in keyWords)
            {
                //if (!obj.name.ToLower().Contains(key.ToLower()))
                if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(obj.name, key, CompareOptions.IgnoreCase) < 0)
                {
                    keyMatched = false;
                    break;
                }
            }
            if (keyMatched)
                return obj;
        }
        return null;
    }

    public static Quaternion Inverse(this Quaternion quat)
    {
        quat.Normalize();
        quat.x *= -1;
        quat.y *= -1;
        quat.z *= -1;
        return quat;
    }

    /// <summary>
    /// Check if magnitude of Quaternion is close to 1
    /// </summary>
    public static bool IsValid(this Quaternion quat)
    {
        return (quat.w * quat.w + quat.x * quat.x + quat.y * quat.y + quat.z * quat.z > 0.95);
    }
}

//public class ReadOnlyAttribute : PropertyAttribute
//{

//}

//[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
//public class ReadOnlyDrawer : PropertyDrawer
//{
//    public override float GetPropertyHeight(SerializedProperty property,
//                                            GUIContent label)
//    {
//        return EditorGUI.GetPropertyHeight(property, label, true);
//    }

//    public override void OnGUI(Rect position,
//                               SerializedProperty property,
//                               GUIContent label)
//    {
//        GUI.enabled = false;
//        EditorGUI.PropertyField(position, property, label, true);
//        GUI.enabled = true;
//    }
//}
