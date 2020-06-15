using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VideoPlayerController : MonoBehaviour
{
    public string videoFileFolderPath;

    void Start()
    {
        DirectoryInfo dir = new DirectoryInfo(videoFileFolderPath);
        FileInfo[] info = dir.GetFiles("*.*");

        foreach (FileInfo f in info)
        {
            // Debug.Log(f.ToString()); // write out to an array!
        }
    }
}
