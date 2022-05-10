using UnityEngine;

/// <summary>
/// 用于寻找旋转的最大角度
/// </summary>
public class RotateTest : MonoBehaviour
{
    public GameObject target;
    public Vector3 EulAffix;
    private Quaternion initRot;
    // Start is called before the first frame update
    void Start()
    {
        initRot = target.transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        target.transform.localRotation = initRot * Quaternion.Euler(EulAffix);
    }
}
