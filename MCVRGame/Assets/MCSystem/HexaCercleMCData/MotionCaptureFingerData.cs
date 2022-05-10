using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MCData
{
    public enum Body
    {
        Thumb1_L,   //左拇指1  
        Thumb2_L,   //左拇指2
        Thumb3_L,   //左拇指3
        Index1_L,   //左食指1
        Index2_L,   //左食指2
        Index3_L,   //左食指3
        Middle1_L,  //左中指1
        Middle2_L,  //左中指2
        Middle3_L,  //左中指3
        Ring1_L,    //左无名指1
        Ring2_L,    //左无名指2
        Ring3_L,    //左无名指3
        Pinky1_L,   //左小指1
        Pinky2_L,   //左小指2
        Pinky3_L,   //左小指3

        Thumb1_R,   //右拇指1     
        Thumb2_R,   //右拇指2
        Thumb3_R,   //右拇指3
        Index1_R,   //右食指1
        Index2_R,   //右食指2
        Index3_R,   //右食指3
        Middle1_R,  //右中指1
        Middle2_R,  //右中指2
        Middle3_R,  //右中指3
        Ring1_R,    //右无名指1
        Ring2_R,    //右无名指2
        Ring3_R,    //右无名指3
        Pinky1_R,   //右小指1
        Pinky2_R,   //右小指2
        Pinky3_R,   //右小指3  

        Head,       //头  
        Chest,      //胸  
        Abdomen,    //腹  

        Shoulder_L, //左肩   
        Elbow_L,    //左肘   
        Hand_L,     //左手  

        Hip_L,      //左大腿      
        Knee_L,     //左小腿      
        Foot_L,     //左脚        
        Toe_End_L,  //左脚尖    

        Shoulder_R, //右肩        
        Elbow_R,    //右肘        
        Hand_R,     //右手

        Hip_R,      //右大腿       
        Knee_R,     //右小腿       
        Foot_R,     //右脚         
        Toe_End_R,  //右脚尖       
    }
    public class MotionCaptureFingerData
    {
        public static readonly Vector3 thumb2_l = new Vector3(32.23f, 20f, 8.796f);
        public static readonly Vector3 thumb3_l = new Vector3(0f, 0f, 10f);

        public static readonly Vector3 index1_l = new Vector3(100f, 0, 0);
        public static readonly Vector3 index2_l = new Vector3(100f, 0, 0);
        public static readonly Vector3 index3_l = new Vector3(80f, 0, 0);

        public static readonly Vector3 middle1_l = new Vector3(80f, 0, 0);
        public static readonly Vector3 middle2_l = new Vector3(86f, 0, 0);
        public static readonly Vector3 middle3_l = new Vector3(76f, 0, 0);

        public static readonly Vector3 ring1_l = new Vector3(95f, 0, 0);
        public static readonly Vector3 ring2_l = new Vector3(83f, 0, 0);
        public static readonly Vector3 ring3_l = new Vector3(73f, 0, 0);

        public static readonly Vector3 pinky2_l = new Vector3(96f, 0, 0);
        public static readonly Vector3 pinky3_l = new Vector3(118f, 0, 0);
        public static readonly Vector3 pinky4_l = new Vector3(35f, 0, 0);

        public static readonly Vector3 thumb2_r = new Vector3(32.23f, 20f, 8.796f);
        public static readonly Vector3 thumb3_r = new Vector3(0f, 0, 10);

        public static readonly Vector3 index1_r = new Vector3(-100f, 0, 0);
        public static readonly Vector3 index2_r = new Vector3(-92f, 0, 0);
        public static readonly Vector3 index3_r = new Vector3(-85f, 0, 0);

        public static readonly Vector3 middle1_r = new Vector3(-101f, 0, 0);
        public static readonly Vector3 middle2_r = new Vector3(-86f, 0, 0);
        public static readonly Vector3 middle3_r = new Vector3(-76f, 0, 0);

        public static readonly Vector3 ring1_r = new Vector3(-95f, 0, 0);
        public static readonly Vector3 ring2_r = new Vector3(-83f, 0, 0);
        public static readonly Vector3 ring3_r = new Vector3(-73f, 0, 0);

        public static readonly Vector3 pinky2_r = new Vector3(-96f, 0, 0);
        public static readonly Vector3 pinky3_r = new Vector3(-118f, 0, 0);
        public static readonly Vector3 pinky4_r = new Vector3(-35f, 0, 0);
    }
}
