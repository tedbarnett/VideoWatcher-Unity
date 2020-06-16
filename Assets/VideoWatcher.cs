using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VideoWatcher : MonoBehaviour
{
    public string videoFileFolderPath;
    public GameObject startupPanel; // hide after loading
    public GameObject VideoPanels; // show after loading
    public GameObject VideoPanel00;
    public GameObject VideoPanel01;
    public GameObject VideoPanel02;
    public GameObject VideoPanel03;
    public List<string> ValidVideoFileExtensions;

    private List<string> VideoFileNames;
    private int currentVideo = 0;


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
            if (ValidVideoFileExtensions.Contains(fileExtension))
            {
                // Debug.Log(fileNameString);
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

    public void GetNextVideo()
    {
        //++currentVideo;
        //if (currentVideo > VideoFileNames.Count) currentVideo = 0;

        currentVideo = Random.Range(0, VideoFileNames.Count);
        Debug.Log("filename: " + videoFileFolderPath + VideoFileNames[currentVideo]);
    }

    public void ClickedOnPanel00() // there MUST be a way to do this once and pass parameters!
    {

        var videoPlayer00 = VideoPanel00.GetComponent<UnityEngine.Video.VideoPlayer>();

        Debug.Log("Panel 00");
        //Debug.Log("filename: " + videoFileFolderPath + VideoFileNames[currentVideo]);
        Debug.Log("videoFileFolderPath: " + videoFileFolderPath);
        Debug.Log("VideoFileNames[currentVideo]: " + VideoFileNames[currentVideo]);

        GetNextVideo();

        videoPlayer00.url = videoFileFolderPath + VideoFileNames[currentVideo];
        videoPlayer00.Play();
    }
    public void ClickedOnPanel01() // there MUST be a way to do this once and pass parameters!
    {
        Debug.Log("Clicked panel 01");

        GetNextVideo();
        var videoPlayer01 = VideoPanel01.GetComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer01.url = videoFileFolderPath + VideoFileNames[currentVideo];
        videoPlayer01.Play();
    }
    public void ClickedOnPanel02() // there MUST be a way to do this once and pass parameters!
    {
        Debug.Log("Clicked panel 02");

        GetNextVideo();
        var videoPlayer02 = VideoPanel02.GetComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer02.url = videoFileFolderPath + VideoFileNames[currentVideo];
        videoPlayer02.Play();
    }
    public void ClickedOnPanel03() // there MUST be a way to do this once and pass parameters!
    {
        Debug.Log("Clicked panel 03");

        GetNextVideo();
        var videoPlayer03 = VideoPanel03.GetComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer03.url = videoFileFolderPath + VideoFileNames[currentVideo];
        videoPlayer03.Play();
    }
}
