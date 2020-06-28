using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoWatcher : MonoBehaviour
{
    private string videoFileFolderPath;
    public string videoFileFolderPathMac = "////Users/tedbarnett/Dropbox (barnettlabs)/Videos/Ted Videos/";
    public string videoFileFolderPathWindows;
    public GameObject startupPanel; // hide after loading
    public GameObject videoPanels; // show after loading
    public List<GameObject> VideoPanel; // a list of n videoPanels (4, 6, or whatever can be displayed)
    public List<TextMeshProUGUI> VideoFileNameText;
    public List<string> ValidVideoExtensions;
    public Text AutoLaunchCountdownText;

    private List<string> VideoFileNames;
    private int currentVideo = 0;
    public float showFilenameTimeSecs = 3;
    private float lastStartTime = 0;
    private int maxPanels = 6;
    private bool awaitingClick = true;
    private float launchedTime;
    public float secondsToAutoStart = 10.0f;


    void Start()
    {
        videoFileFolderPath = Application.streamingAssetsPath + "/";
        #if UNITY_STANDALONE_OSX
                //videoFileFolderPath = videoFileFolderPathMac;
        #endif
        #if UNITY_STANDALONE_Windows
                videoFileFolderPath = videoFileFolderPathWindows;
        #endif
        startupPanel.SetActive(true);
        videoPanels.SetActive(false);
        launchedTime = Time.time;


        maxPanels = VideoPanel.Count; //TODO: If on smaller screen, set maxPanels to a smaller number than VideoPanel.Count

        for (int i = 0; i < maxPanels; i++)
        {
            int closureIndex = i; // prevents the closure problem!  TODO: Test without closureIndex.  Not needed?
            var VideoFileNameTextTEMP = VideoPanel[closureIndex].gameObject.GetComponentInChildren<TextMeshProUGUI>();
            VideoFileNameText.Add(VideoFileNameTextTEMP);

            var videoPlayer = VideoPanel[closureIndex].GetComponentInChildren<UnityEngine.Video.VideoPlayer>();
            videoPlayer.loopPointReached += EndReached;
            var currentButton = VideoPanel[closureIndex].GetComponentInChildren<Button>();
            //Debug.Log("closureIndex = " + closureIndex + ", currentButton = " + currentButton);
            currentButton.onClick.AddListener(() => { PlayNextVideoByVP(videoPlayer); });

        }
        // TODO: Enable filename to appear on mouse-over
    }

    void Update()
    {
        // Hide (or fade out) Video filenames after showFilenameTimeSecs 
        if (lastStartTime != 0 && (Time.time - lastStartTime) > showFilenameTimeSecs)
        {
            for (int i = 0; i < maxPanels; i++)
            {
                VideoFileNameText[i].color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                lastStartTime = 0;
            }
        }

        if (awaitingClick && (Time.time - launchedTime) > secondsToAutoStart)
            {
            awaitingClick = false;
            ClickToStart();
            }
        if (awaitingClick)
        {
            string timeLeft = Mathf.FloorToInt(secondsToAutoStart + launchedTime - Time.time).ToString();
            AutoLaunchCountdownText.text = timeLeft;
        }
    }

    void SetupVideoList()
    {
        VideoFileNames = new List<string>();
        DirectoryInfo dir = new DirectoryInfo(videoFileFolderPath);
        FileInfo[] info = dir.GetFiles("*.*"); // TODO: Read from Dropbox or iCloud folder?

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

        if (VideoFileNames.Count == 0)
        {
            Debug.Log("No files found in Directory"); // TODO: Ask for new file directory location?
        } 
    }

    public void ClickToStart() // triggered by the ClickToStart button
    {
        SetupVideoList();
        for (int i = 0; i < maxPanels; i++)
        {
            // TODO: Enable this!
            var videoPlayer = VideoPanel[i].GetComponentInChildren<UnityEngine.Video.VideoPlayer>();
            PlayNextVideoByVP(videoPlayer);
        }

        startupPanel.SetActive(false);
        videoPanels.SetActive(true);
    }

    public void GetNextVideo()
    {
        currentVideo = Random.Range(0, VideoFileNames.Count); // Choose next video at random
        lastStartTime = Time.time;
    }

    public void PlayNextVideoByVP(UnityEngine.Video.VideoPlayer vp)
    {
        GetNextVideo();
        vp.url = videoFileFolderPath + VideoFileNames[currentVideo];
        //TODO: If video is longer than 30 secs, start at random point
        vp.prepareCompleted += videoIsPrepared;
        vp.Prepare();
        
    }

    void videoIsPrepared(UnityEngine.Video.VideoPlayer vp)
    {

        float videoLength = (float)vp.length; 
        int min = Mathf.FloorToInt(videoLength / 60);
        int sec = Mathf.FloorToInt(videoLength % 60);
        string videoLengthString = min.ToString("00") + ":" + sec.ToString("00");

        // Set video name, without extension
        TextMeshProUGUI currentFileNameText = vp.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        string fileNameLessExtension = VideoFileNames[currentVideo];
        int fileExtPos = fileNameLessExtension.LastIndexOf(".");
        if (fileExtPos >= 0)
            fileNameLessExtension = fileNameLessExtension.Substring(0, fileExtPos);

        currentFileNameText.text = fileNameLessExtension + " <color=#C1C1C1>(" + videoLengthString + ")</color>";

        lastStartTime = Time.time;
        currentFileNameText.color = new Color(0.990566f, 0.9850756f, 0.01401742f, 1.0f);
        vp.Play();
    }


    public void ShowName(int panelID)
    {
        lastStartTime = Time.time;
        VideoFileNameText[0].color = new Color(0.990566f, 0.9850756f, 0.01401742f, 1.0f);
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        PlayNextVideoByVP(vp);
        
    }
}
