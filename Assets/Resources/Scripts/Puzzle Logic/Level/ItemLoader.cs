using UnityEngine;
using System.Collections;
using System;

public static class ItemLoader
{
    static public GameObject LoadObject(string path, int code)
    {
        GameObject obj = Resources.Load<GameObject>(path + Convert.ToString(code, 16));
        if (obj != null)
        {
            //Debug.Log($"Load successully with {code:X}");
            return obj;
        }

        //First Alternate Try : replace all height which is not connected to 3(0x11)
        int alterCode = code;
        int c1 = code >> 4 & 0x1;
        int c2 = code >> 5 & 0x1;

        if (c1 == 0)
        {
            alterCode |= 0x3;
        }
        if (c2 == 0)
        {
            alterCode |= 0xC;
        }

        if (code != alterCode)
        {
            obj = Resources.Load<GameObject>(path + Convert.ToString(alterCode, 16));
            if (obj != null)
            {
                //Debug.Log($"Load successully with {alterCode:X}");
                return obj;
            }
        }

        //Second Alternate Try : only happen if it contains extra code
        if ((code >> 8 & 0xF) != 0)
        {
            alterCode = code;

            c1 = code >> 8 & 1;
            c2 = code >> 9 & 1;

            if (c1 == 0)
            {
                alterCode |= 0x3;
            }
            if (c2 == 0)
            {
                alterCode |= 0xC;
            }

            if (code != alterCode)
            {
                obj = Resources.Load<GameObject>(path + Convert.ToString(alterCode, 16));
                if (obj != null)
                {
                    //Debug.Log($"Load successully with {alterCode:X}");
                    return obj;
                }
            }
        }

        //Third Alternate Try: Replace all height to 3, which result in F (0x1111)
        alterCode = code | 0xF;
        if (code != alterCode)
        {
            obj = Resources.Load<GameObject>(path + Convert.ToString(alterCode, 16));
            if (obj != null)
            {
                //Debug.Log($"Load successully with {alterCode:X}");
                return obj;
            }
        }

        return obj;
    }
}
