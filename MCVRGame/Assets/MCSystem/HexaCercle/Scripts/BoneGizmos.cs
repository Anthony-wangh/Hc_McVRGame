using UnityEngine;

public class BoneGizmos : MonoBehaviour
{
    public bool Show = true;
    public Transform rootNode;
    public Transform[] childNodes;

    void OnDrawGizmos()
    {
        if (Show)
        {
            if (rootNode != null)
            {
                if (childNodes == null || childNodes.Length == 0)
                {
                    //get all joints to draw
                    PopulateChildren();
                }


                foreach (Transform child in childNodes)
                {

                    if (child == rootNode)
                    {
                        //list includes the root, if root then larger, green cube
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(child.position, .01f);
                    }
                    else if (child.name.ToLower().Contains("thumb") ||
                        child.name.ToLower().Contains("index") ||
                        child.name.ToLower().Contains("middle") ||
                        child.name.ToLower().Contains("ring") ||
                        child.name.ToLower().Contains("pinky"))
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(child.position, child.parent.position);
                        Gizmos.DrawSphere(child.position, .005f);
                    }
                    else
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(child.position, child.parent.position);
                        Gizmos.DrawSphere(child.position, .01f);
                    }
                }
            }
        }
    }

    public void PopulateChildren()
    {
        childNodes = rootNode.GetComponentsInChildren<Transform>();
    }
}
