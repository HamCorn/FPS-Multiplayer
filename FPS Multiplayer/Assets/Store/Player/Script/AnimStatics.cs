using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimStatics
{
    public static string walking = "walk";  //走路
    public static string running = "run";   //跑步
    public static string aim = "isAiming";  //瞄準
    public static string angle = "aimAngle";    //瞄準角度
    public static string crouch ="crouch";  //蹲下
    public static string horizontal ="horizontal";  //Y軸
    public static string vertical ="vertical";  //X軸
    public static string isInAngle="isInAngle"; //角度
    public static string onGround ="onGround";  //地板

    public enum animLayers
    {
        //enumerator for each layer
         normalLayer = 0,aimLayer = 1, crouchLayer =2
    }

}
