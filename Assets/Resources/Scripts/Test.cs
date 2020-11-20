using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class Test : MonoBehaviour
{
    private string prefabPath = "\\Assets\\Resources\\Prefabs\\Level Elements\\";
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Directory.GetCurrentDirectory());
        string wholePrefabPath = Directory.GetCurrentDirectory() + prefabPath + "NotSprayable";
        string[] p = Directory.GetDirectories(wholePrefabPath);
        string pp = "NotSprayable";
        Regex regex = new Regex(@"(?<=" + pp + @"\\)[\W\w]+$");
        for (int i = 0; i < p.Length; i++)
        {
            p[i] = regex.Match(p[i]).Value;
        }

        foreach (string path in p)
        {
            Debug.Log(path);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
