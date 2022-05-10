using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Slider))]
public class TextSlider : MonoBehaviour
{
    Slider s;
    Text t;
    // Start is called before the first frame update
    void Start()
    {
        s = gameObject.GetComponent<Slider>();
        t = gameObject.GetComponentInChildren<Text>();
        s.onValueChanged.AddListener(SliderValueChanged);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void SliderValueChanged(float value)
    {
        t.text = value.ToString("0.00");
    }

}
