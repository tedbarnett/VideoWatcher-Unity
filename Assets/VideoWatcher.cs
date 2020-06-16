using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class VideoWatcher : MonoBehaviour
{
    public string videoFileFolderPath;
    public GameObject startupPanel; // hide after loading
    public GameObject VideoPanels; // show after loading
    public GameObject VideoPanel00;
    public Text FileNameVideoPanel00;
    public GameObject VideoPanel01;
    public Text FileNameVideoPanel01;
    public GameObject VideoPanel02;
    public Text FileNameVideoPanel02;
    public GameObject VideoPanel03;
    public Text FileNameVideoPanel03;
    public List<string> ValidVideoFileExtensions;

    private List<string> VideoFileNames;
    private int currentVideo = 0;
    public float showFilenameTimeSecs = 3;
    private float lastStartTime = 0;


    void Start()
    {
        VideoPanels.SetActive(false);
        startupPanel.SetActive(true);
        var videoPlayer00 = VideoPanel00.GetComponent<UnityEngine.Video.VideoPlayer>();
        var videoPlayer01 = VideoPanel01.GetComponent<UnityEngine.Video.VideoPlayer>();
        var videoPlayer02 = VideoPanel02.GetComponent<UnityEngine.Video.VideoPlayer>();
        var videoPlayer03 = VideoPanel03.GetComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer00.loopPointReached += EndReached00;
        videoPlayer01.loopPointReached += EndReached01;
        videoPlayer02.loopPointReached += EndReached02;
        videoPlayer03.loopPointReached += EndReached03;
    }

    void Update()
    {
        if(lastStartTime != 0 && (Time.time - lastStartTime) > showFilenameTimeSecs)
        {
            //FileNameVideoPanel00.text = "";
            //FileNameVideoPanel01.text = "";
            //FileNameVideoPanel02.text = "";
            //FileNameVideoPanel03.text = "";

            FileNameVideoPanel00.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            FileNameVideoPanel01.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            FileNameVideoPanel02.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            FileNameVideoPanel03.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);

            lastStartTime = 0;
        }
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
        //Debug.Log("filename: " + videoFileFolderPath + VideoFileNames[currentVideo]);
        lastStartTime = Time.time;
    }

    public void ClickedOnPanel00() // there MUST be a way to do this once and pass parameters!
    {

        var videoPlayer00 = VideoPanel00.GetComponent<UnityEngine.Video.VideoPlayer>();

        //Debug.Log("videoFileFolderPath: " + videoFileFolderPath);
        //Debug.Log("VideoFileNames[currentVideo]: " + VideoFileNames[currentVideo]);

        GetNextVideo();

        videoPlayer00.url = videoFileFolderPath + VideoFileNames[currentVideo];
        videoPlayer00.Play();
        FileNameVideoPanel00.text = VideoFileNames[currentVideo];
    }
    public void ClickedOnPanel01() // there MUST be a way to do this once and pass parameters!
    {

        GetNextVideo();
        var videoPlayer01 = VideoPanel01.GetComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer01.url = videoFileFolderPath + VideoFileNames[currentVideo];
        videoPlayer01.Play();
        FileNameVideoPanel01.text = VideoFileNames[currentVideo];
    }
    public void ClickedOnPanel02() // there MUST be a way to do this once and pass parameters!
    {

        GetNextVideo();
        var videoPlayer02 = VideoPanel02.GetComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer02.url = videoFileFolderPath + VideoFileNames[currentVideo];
        videoPlayer02.Play();
        FileNameVideoPanel02.text = VideoFileNames[currentVideo];
    }
    public void ClickedOnPanel03() // there MUST be a way to do this once and pass parameters!
    {

        GetNextVideo();
        var videoPlayer03 = VideoPanel03.GetComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer03.url = videoFileFolderPath + VideoFileNames[currentVideo];
        videoPlayer03.Play();
        FileNameVideoPanel03.text = VideoFileNames[currentVideo];
    }

    public void ShowName00()
    {
        //FileNameVideoPanel00.text = VideoFileNames[currentVideo]; // TODO: Fix since this won't be the filename!
        lastStartTime = Time.time;
        FileNameVideoPanel00.color = new Color(0.990566f, 0.9850756f, 0.01401742f, 1.0f);

    }
    public void ShowName01()
    {
        //FileNameVideoPanel01.text = VideoFileNames[currentVideo];
        lastStartTime = Time.time;
        FileNameVideoPanel01.color = new Color(0.990566f, 0.9850756f, 0.01401742f, 1.0f);
    }
    public void ShowName02()
    {
        //FileNameVideoPanel02.text = VideoFileNames[currentVideo];
        lastStartTime = Time.time;
        FileNameVideoPanel02.color = new Color(0.990566f, 0.9850756f, 0.01401742f, 1.0f);
    }
    public void ShowName03()
    {
        //FileNameVideoPanel03.text = VideoFileNames[currentVideo];
        lastStartTime = Time.time;
        FileNameVideoPanel03.color = new Color(0.990566f, 0.9850756f, 0.01401742f, 1.0f);
    }

    void EndReached00(UnityEngine.Video.VideoPlayer vp)
    {
        ClickedOnPanel00();
    }

    void EndReached01(UnityEngine.Video.VideoPlayer vp)
    {
        ClickedOnPanel01();
    }

    void EndReached02(UnityEngine.Video.VideoPlayer vp)
    {
        ClickedOnPanel02();
    }

    void EndReached03(UnityEngine.Video.VideoPlayer vp)
    {
        ClickedOnPanel03();
    }
}
