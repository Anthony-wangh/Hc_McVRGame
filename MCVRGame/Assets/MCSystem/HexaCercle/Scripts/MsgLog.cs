using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class MsgLog : MonoBehaviour
{
    private static TextMeshPro txt;
    private static Queue<string> messages;
    public static readonly float HideDelay = 2f;
    void Start()
    {
        txt = GetComponent<TextMeshPro>();
    }

    public static void WriteLine(string message)
    {
        Debug.Log(message);
        if (messages == null)
            messages = new Queue<string>();
        if (txt != null)
        {
            messages.Enqueue(message);
            while (messages.Count > 10)
                messages.Dequeue();
            var tmp = txt.GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = "";
                foreach (var s in messages)
                    tmp.text += s + "\r\n";
            }
            else
            {
                var text = txt.GetComponent<UnityEngine.UI.Text>();
                if (text != null)
                {
                    text.text = "";
                    foreach (var s in messages)
                        text.text += s + "\r\n";
                }
            }
        }
    }

    public static void DisableMsg()
    {
        txt?.gameObject?.SetActive(false);
        messages = null;
    }
    public static void EnableMsg()
    {
        txt?.gameObject?.SetActive(true);
    }

    private void OnDestroy()
    {
        txt = null;
        messages = null;
    }
}
