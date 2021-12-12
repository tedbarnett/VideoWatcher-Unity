using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Xml.Schema;

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

    private int currentVideo = 0;
    public float showFilenameTimeSecs = 3;
    private float lastStartTime = 0;
    private int maxPanels = 6;
    private bool awaitingClick = true;
    private float launchedTime;
    public float secondsToAutoStart = 10.0f;
    public float skipVideosShorterThanSecs = 0.0f;
    public float longVideoLengthMinimum = 30.0f; // if video is longer than this minimum, start at a random frame
    private List<string> VideoFileNames;


    void Start()
    {
        videoFileFolderPath = Application.streamingAssetsPath + "/";
        videoFileFolderPath = videoFileFolderPathMac; // default assumption is Mac platform
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) videoFileFolderPath = videoFileFolderPathWindows;

        startupPanel.SetActive(true);
        videoPanels.SetActive(false);
        launchedTime = Time.time;
        maxPanels = VideoPanel.Count; //TODO: If on smaller screen, set maxPanels to a smaller number than VideoPanel.Count

        for (int i = 0; i < maxPanels; i++)
        {
            var VideoFileNameTextTEMP = VideoPanel[i].gameObject.GetComponentInChildren<TextMeshProUGUI>();
            VideoFileNameText.Add(VideoFileNameTextTEMP);

            var videoPlayer = VideoPanel[i].GetComponentInChildren<UnityEngine.Video.VideoPlayer>();
            videoPlayer.loopPointReached += EndReached;
            var currentButton = VideoPanel[i].GetComponentInChildren<Button>(); // finds the first button child and sets currentButton to that
            currentButton.onClick.AddListener(() => { PlayNextVideoByVP(videoPlayer); });
      //      currentButton.onClick.AddListener(() => { ToggleVolume(videoPlayer); });
      //      currentButton.onClick.AddListener(() => { JumpToFrame(videoPlayer, 0.5f); });
      //      currentButton.onClick.AddListener(() => { PlayPause(videoPlayer); });


            videoPlayer.SetDirectAudioVolume(0, 0.0f);

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
            var videoPlayer = VideoPanel[i].GetComponentInChildren<UnityEngine.Video.VideoPlayer>();
            PlayNextVideoByVP(videoPlayer);
        }

        startupPanel.SetActive(false);
        videoPanels.SetActive(true);
    }



    public void ToggleVolume(UnityEngine.Video.VideoPlayer vp)
    {
        // change volume for a specific video panel (ie. the one you are hovering over)
        float tempVolume = vp.GetDirectAudioVolume(0);
        if(tempVolume > 0.0f)
        {
            vp.SetDirectAudioVolume(0, 0.0f);
        } else
        {
            vp.SetDirectAudioVolume(0, 1.0f);
        }


    }
    
    public void PlayPause(UnityEngine.Video.VideoPlayer vp)
    {
        if (vp.isPlaying)
        {
            vp.Pause();
        }
        else
        {
            vp.Play();
        }

    }
    public void PauseVideo(UnityEngine.Video.VideoPlayer vp)
    {
        vp.Pause();
    }
    public void PlayVideo(UnityEngine.Video.VideoPlayer vp)
    {
        vp.Play();
    }
    public void JumpToFrame(UnityEngine.Video.VideoPlayer vp, float percentOfClip)
    {
            var newFrame = vp.frameCount * percentOfClip;
            vp.frame = (long)newFrame;
    }
    public void GetNextVideo()
    {
        currentVideo = Random.Range(0, VideoFileNames.Count - 1); // Choose next video at random
        //currentVideo = 124; // set to a specific video for debugging (i.e. if video format failing)
        Debug.Log("videofilenames.count: " + VideoFileNames.Count + ", currentVideo: " + currentVideo + ", " + "filename: " + VideoFileNames[currentVideo]);
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
        if(videoLength > longVideoLengthMinimum)
        {
            vp.frame = Mathf.FloorToInt(vp.frameCount * Random.Range(0.0f, 1.0f));
        }

        int min = Mathf.FloorToInt(videoLength / 60);
        int sec = Mathf.FloorToInt(videoLength % 60);
        string videoLengthString = "";
        if (min > 0)
        {
            videoLengthString = min.ToString("00") + ":" + sec.ToString("00");
        }
        else
        {
            videoLengthString = sec.ToString("00") + " secs";
        }

        // Set video name using makeNameString()
        TextMeshProUGUI currentFileNameText = vp.gameObject.GetComponentInChildren<TextMeshProUGUI>();

        currentFileNameText.text = makeNameString(VideoFileNames[currentVideo]) + "\n<alpha=#88><size=70%>(" + videoLengthString + ")</size>";

        lastStartTime = Time.time;
        currentFileNameText.color = new Color32(255, 255, 0, 255); // set text to yellow color to make it visible (change this to on-hover)
        vp.Play();
    }

    public string makeNameString(string fileName)
    {
        string newFileName = fileName;
        string dateString = "date unknown";
        int fileExtPos = newFileName.LastIndexOf(".");
        if (fileExtPos >= 0) 
            newFileName = newFileName.Substring(0, fileExtPos); // strip off the file extension
        if (newFileName.Length > 10)
        {
            dateString = newFileName.Substring(0, 10);

            string dateText = "";
            int dateNum;
            int.TryParse(dateString.Substring(8, 2), out dateNum);

            string[] monthNames = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            int monthNum;
            int.TryParse(dateString.Substring(5, 2), out monthNum);
            string monthText;
            if (monthNum != 0) {
                if(dateNum == 0) // if month is set but date is 00 (i.e. we knew the month but not the day)
                {
                    monthText = monthNames[monthNum - 1].ToString() + " ";
                    dateText = "";
                }
                else // if month and date are valid
                {
                    monthText = monthNames[monthNum - 1].ToString() + " ";
                    dateText = dateNum.ToString() + " ";
                }
            } else {
                monthText = "";
                dateText = "";
            };

            //Debug.Log("monthNum = " + monthNum + ", dateNum = " + dateNum);
            dateString = monthText + dateText + dateString.Substring(0, 4);
        }
        newFileName = dateString; // Set name of file to date of movie clip (extracted from front of filename)
        return newFileName;
    }


    public void ShowControlPanel(UnityEngine.Video.VideoPlayer vp)
    {
        // TBD
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        PlayNextVideoByVP(vp);
    }
}
