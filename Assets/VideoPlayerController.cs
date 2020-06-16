using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VideoPlayerController : MonoBehaviour
{
    public string videoFileFolderPath;
    public GameObject startupPanel; // hide after loading
    public GameObject VideoPanels; // show after loading
    public List<string> ValidVideoFileExtensions;

    private List<string> VideoFileNames;


    void Start()
    {
        VideoPanels.SetActive(false);
        startupPanel.SetActive(true);
    }

    void SetupVideoList()
    {


        VideoFileNames = new List<string>();
        DirectoryInfo dir = new DirectoryInfo(videoFileFolderPath);
        FileInfo[] info = dir.GetFiles("*.*");

        foreach (FileInfo f in info)
        {
            // Debug.Log(f.ToString()); // write out to an array!
            //string fileNameString = f.ToString();
            string fileNameString = Path.GetFileName(f.ToString());
            string fileExtension = Path.GetExtension(fileNameString);
  //          if(fileExtension == ".mp4")
                if (ValidVideoFileExtensions.Contains(fileExtension))
                {
                Debug.Log(fileNameString);
                VideoFileNames.Add(fileNameString);
            }


        }
        Debug.Log("Total files = " + VideoFileNames.Count);
        startupPanel.SetActive(false);
        VideoPanels.SetActive(true);

    }

    public void ClickToStart() // triggered by the ClickToStart button
    {
        SetupVideoList();
    }
}
