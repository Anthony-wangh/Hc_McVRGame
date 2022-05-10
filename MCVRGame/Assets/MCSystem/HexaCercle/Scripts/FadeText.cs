using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeText : MonoBehaviour
{
    public float alpha = 1;
    static float fadeSpeed = 0.01f;
    private Text t;
    // Start is called before the first frame update
    void Start()
    {
        t = gameObject.GetComponent<UnityEngine.UI.Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (alpha == 1)
            StartCoroutine("fading");
    }

    IEnumerator fading()
    {
        while (alpha > 0)
        {
            alpha -= fadeSpeed;
            Color c = t.color;
            c.a = alpha;
            t.color = c;
            yield return null;
        }
    }
}
