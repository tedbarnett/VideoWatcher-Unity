using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class VideoWatcher : MonoBehaviour
{
    public string videoFileFolderPath;
    public GameObject startupPanel; // hide after loading
    public GameObject videoPanels; // show after loading
    public List<GameObject> VideoPanel; // a list of n videoPanels (4, 6, or whatever can be displayed)
    public Text videoFileName;
    public List<string> ValidVideoExtensions;

    private List<string> VideoFileNames;
    private int currentVideo = 0;
    public float showFilenameTimeSecs = 3;
    private float lastStartTime = 0;


    void Start()
    {
        startupPanel.SetActive(true);
        videoPanels.SetActive(false);
        var videoPlayer = VideoPanel[0].GetComponent<UnityEngine.Video.VideoPlayer>();
            videoPlayer.loopPointReached += EndReached;
    }

    void Update()
    {
        // Hide (or fade out) Video filenames after showFilenameTimeSecs 
        if (lastStartTime != 0 && (Time.time - lastStartTime) > showFilenameTimeSecs)
        {
            videoFileName.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            lastStartTime = 0;
        }
    }

    void SetupVideoList()
    {
        VideoFileNames = new List<string>();
        DirectoryInfo dir = new DirectoryInfo(videoFileFolderPath);
        FileInfo[] info = dir.GetFiles("*.*"); // TODO: Read from iCloud folder?

        foreach (FileInfo f in info)
        {
            string fileNameString = Path.GetFileName(f.ToString());
            string fileExtension = Path.GetExtension(fileNameString).ToLower(); // lowercase extension
            if (ValidVideoExtensions.Contains(fileExtension))
            {
                VideoFileNames.Add(fileNameString);
            }
        }
        Debug.Log("Total # videos = " + VideoFileNames.Count);
    }

    public void ClickToStart() // triggered by the ClickToStart button
    {
        SetupVideoList();
        PlayNextVideo(0);
        startupPanel.SetActive(false);
        videoPanels.SetActive(true);
    }

    public void GetNextVideo()
    {
        currentVideo = Random.Range(0, VideoFileNames.Count); // Choose next video at random
        lastStartTime = Time.time;
    }

    public void PlayNextVideo(int panelID)
    {

        var videoPlayer = VideoPanel[panelID].GetComponent<UnityEngine.Video.VideoPlayer>();

        GetNextVideo();
        videoPlayer.url = videoFileFolderPath + VideoFileNames[currentVideo];
        videoPlayer.Play();
        videoFileName.text = VideoFileNames[currentVideo];
    }
    

    public void ShowName()
    {
        lastStartTime = Time.time;
        videoFileName.color = new Color(0.990566f, 0.9850756f, 0.01401742f, 1.0f);
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        PlayNextVideo(0); // need to figure out which vp just ended!
    }
}
