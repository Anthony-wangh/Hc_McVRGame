using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MCData
{
    public enum ComMode
    {
        Glove,
        MoCa2
    }
    public class ComSelector : MonoBehaviour
    {
        public ComMode Mode;
        [HideInInspector()]
        public Dropdown dd;
        private List<string> items;
        // Start is called before the first frame update
        private void Awake()
        {
            dd = GetComponent<Dropdown>();
            //dd.onValueChanged.AddListener((i) => ToggleValueChanged(i));
            dd.ClearOptions();
            items = new List<string>(System.IO.Ports.SerialPort.GetPortNames());
            dd.AddOptions(items);
        }
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var list = new List<string>(System.IO.Ports.SerialPort.GetPortNames());
            if (list.Any(x => !items.Contains(x)) || items.Any(x => !list.Contains(x)))
            {
                dd.ClearOptions();
                items = list;
                dd.AddOptions(items);
                return;
            }
            if (Mode == ComMode.MoCa2)
            {
                if (dd.options[dd.value].text == MotionCapture.instance.ComName)
                {
                    if (dd.value == 0)
                    {
                        if (items.Count >= 2)
                        {
                            dd.value = 1;
                            dd.captionText.text = dd.options[1].text;
                        }
                        else
                        {
                            dd.value = -1;
                            dd.captionText.text = "";
                        }
                    }
                    else
                    {
                        dd.value = 0;
                        dd.captionText.text = dd.options[0].text;
                    }
                }
            }
            try
            {
                var a = dd.options[dd.value];
                switch (Mode)
                {
                    case ComMode.Glove:
                        {
                            if (a.text != MotionCapture.instance.ComName)
                            {
                                //dd.value = -1;
                                dd.captionText.text = a.text;
                                MotionCapture.instance.ComName = a.text;
                                MotionCapture.instance.ResetCom();
                            }
                            break;
                        }
                    case ComMode.MoCa2:
                        {
                            if (a.text != MotionCapture.instance.MoCa2Com)
                            {
                                dd.captionText.text = a.text;
                                MotionCapture.instance.MoCa2Com = a.text;
                                MotionCapture.instance.ResetCom();
                            }
                            break;
                        }
                }

            }
            catch
            { dd.value = -1; }
        }
        void ToggleValueChanged(int newValue)
        {
            Debug.Log(this.gameObject.name + "toggled value change for " + dd.options[newValue].text);
            dd.captionText.text = dd.options[newValue].text;
            switch (Mode)
            {
                case ComMode.Glove:
                    {
                        MotionCapture.instance.ComName = dd.options[newValue].text;
                        //MotionCapture.instance.ResetCom();
                        break;
                    }
                case ComMode.MoCa2:
                    {
                        MotionCapture.instance.MoCa2Com = dd.options[newValue].text;
                        MotionCapture.instance.ResetCom();
                        break;
                    }
                default:
                    {
                        Debug.LogError("Drop down mode missing");
                        break;
                    }
            }
        }


        private void OnDestroy()
        {
            dd.ClearOptions();
        }
    }
}
