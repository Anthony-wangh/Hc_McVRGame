using UnityEngine;

public class SwitchColor : MonoBehaviour
{
    private Color[] colors = new Color[4];
    private int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        colors[0] = Color.green;
        colors[1] = Color.white;
        colors[2] = Color.black;
        colors[3] = Color.blue;
        GetComponent<Camera>().backgroundColor = colors[index];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            index++;
            if (index >= colors.Length)
                index = 0;
            GetComponent<Camera>().backgroundColor = colors[index];
        }
    }
}
