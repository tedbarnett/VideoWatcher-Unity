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
    private int maxPanels = 6;
    private bool awaitingClick = true;
    private float launchedTime;
    public float secondsToAutoStart = 10.0f;
    public float skipVideosShorterThanSecs = 0.0f;
    public float longVideoLengthMinimum = 30.0f; // if video is longer than this minimum, start at a random frame
    private List<string> VideoFileNames;
    private bool firstLoop = true;
    private int firstCounter;
    private int panelIndex;
    private List<int> panelCurrentVideo; // making a temp array to hold "currentVideo" number for each panel number

    // **************************************************** START ****************************************************************
    void Start()
    {
        videoFileFolderPath = Application.streamingAssetsPath + "/";
        videoFileFolderPath = videoFileFolderPathMac; // default assumption is Mac platform
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) videoFileFolderPath = videoFileFolderPathWindows;

        startupPanel.SetActive(false); // TODO: Temporary!
        videoPanels.SetActive(true);
        launchedTime = Time.time;
        maxPanels = VideoPanel.Count; //TODO: If on smaller screen, set maxPanels to a smaller number than VideoPanel.Count

        panelCurrentVideo = new List<int>();
        //firstCounter = 0; // will count down before enabling re-write of all names

        for (int i = 0; i < maxPanels; i++)
        {
            var videoPlayer = VideoPanel[i].GetComponentInChildren<UnityEngine.Video.VideoPlayer>();
            videoPlayer.loopPointReached += PlayNextVideo; // when videos end, automatically play another

            var currentButton = VideoPanel[i].GetComponentInChildren<Button>(); // finds the first button child and sets currentButton to that
            currentButton.onClick.AddListener(() => { ClickedOnVideoPanel(videoPlayer); });
            videoPlayer.SetDirectAudioVolume(0, 0.0f); // Mute volume
            panelCurrentVideo.Add(0); // clear out panel numbers
        }
    }
    // **************************************************** UPDATE ****************************************************************
    void Update()
    {

        // TO DO: Delete this waiting period click-to-start thing?
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

        if(firstLoop && firstCounter >= maxPanels)
        {
            // update all panel text
            Debug.Log("======= Updating panels...");
            for (int i = 0; i < maxPanels; i++)
            {
                var videoPlayer = VideoPanel[i].GetComponentInChildren<UnityEngine.Video.VideoPlayer>();
                panelIndex = i;
                UpdateVideoName(videoPlayer);
            }
            firstLoop = false;
        }
    }
    // ---------------------------------------------------- SetupVideoList ----------------------------------------------------
    void SetupVideoList()
    {
        VideoFileNames = new List<string>();
        DirectoryInfo dir = new DirectoryInfo(videoFileFolderPath);
        FileInfo[] info = dir.GetFiles("*.*"); // TODO: Read from Dropbox or iCloud or Google Drive instead?

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
        if (VideoFileNames.Count == 0) Debug.Log("No files found in Directory");

    }
    // ---------------------------------------------------- ClickToStart ----------------------------------------------------
    public void ClickToStart() // triggered by the ClickToStart button
    {
        SetupVideoList();

        for (int i = 0; i < maxPanels; i++)
        {
            firstCounter = i;
            Debug.Log("*** in ClickToStart, firstCounter = " + firstCounter);

            var videoPlayer = VideoPanel[i].GetComponentInChildren<UnityEngine.Video.VideoPlayer>();

            TextMeshProUGUI currentFileNameText = videoPlayer.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            currentFileNameText.text = "Panel #" + i; // clear names to start, later use videoPlayer.url;

            PlayNextVideo(videoPlayer);
        }
        if (firstLoop) firstCounter++;

        startupPanel.SetActive(false);
        videoPanels.SetActive(true);
    }
    // ---------------------------------------------------- PlayNextVideo ----------------------------------------------------
    public void PlayNextVideo(UnityEngine.Video.VideoPlayer vp)
    {
        currentVideo = Random.Range(0, VideoFileNames.Count - 1); // Choose next video at random

        if (firstCounter < maxPanels) panelCurrentVideo[firstCounter] = currentVideo;

        Debug.Log("  currentVideo: " + currentVideo + ", " + "filename: " + VideoFileNames[currentVideo]);

        if (firstCounter >= maxPanels) { 
            for (int i = 0; i < maxPanels; i++)
            {
                Debug.Log("panelCurrentVideo[" + i + "] = " + panelCurrentVideo[i]);
            }
        }

        vp.url = videoFileFolderPath + VideoFileNames[currentVideo];
        vp.prepareCompleted += SetVideoNameText;
        vp.Prepare();
    }
    // ---------------------------------------------------- SetVideoNameText ----------------------------------------------------
    void SetVideoNameText(UnityEngine.Video.VideoPlayer vp) // once video URL is loaded, set the currentFileNameText.text with the file name and length
    {
        if (firstLoop) return;
        Debug.Log("_____ WHY am I in SetVideoNameText");
        float videoLength = (float)vp.length;
        string tempVideoName;
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
        tempVideoName = VideoFileNames[currentVideo];
        if (!firstLoop)
        {
            currentFileNameText.text = makeNameString(tempVideoName) + "\n<alpha=#88><size=70%>(" + videoLengthString + ")</size>";
        }
        vp.Play();
    }
    // ---------------------------------------------------- UpdateVideoName ----------------------------------------------------
    void UpdateVideoName(UnityEngine.Video.VideoPlayer vp)
    {
        float videoLength = (float)vp.length;
        string tempVideoName;
        if (videoLength > longVideoLengthMinimum)
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
        string vpCurrentText = currentFileNameText.text;
        Debug.Log("   ---- in UpdateVideoName: firstLoop = " + firstLoop);

        tempVideoName = VideoFileNames[panelCurrentVideo[panelIndex]];
        currentVideo = panelCurrentVideo[panelIndex];
        Debug.Log("... panelIndex = " + panelIndex + ", tempVideoName = " + tempVideoName);
        Debug.Log(" - tempVideoName = " + tempVideoName + ", filename = " + makeNameString(tempVideoName) + " currentVideo = " + currentVideo);
        Debug.Log(" - vpCurrentText = " + vpCurrentText);


        currentFileNameText.text = makeNameString(tempVideoName) + "\n<alpha=#88><size=70%>(" + videoLengthString + ")</size>";
        Debug.Log(" - currentFileNameText.text = " + currentFileNameText.text);

        //currentFileNameText.color = new Color32(255, 255, 0, 0); // set text to transparent to hide by default
        //vp.Play();
    }

    // ---------------------------------------------------- ClickedOnVideoPanel ----------------------------------------------------
    public void ClickedOnVideoPanel(UnityEngine.Video.VideoPlayer vp)
    {
        ToggleVolume(vp);
    }

    // ---------------------------------------------------- ShowVideoName ----------------------------------------------------
    public void ShowVideoName(UnityEngine.Video.VideoPlayer vp, bool showNameNow)
    {
        Debug.Log("video is " + vp);
        TextMeshProUGUI currentFileNameText = vp.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        if (showNameNow)
        {
            currentFileNameText.color = new Color32(255, 255, 0, 255); // set text to yellow color to make it visible
        }
        else
        {
            currentFileNameText.color = new Color32(255, 255, 0, 0); // set text to transparent to hide
        }
    }

    // ---------------------------------------------------- makeNameString ----------------------------------------------------

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


    // ---------------------------------------------------- ToggleVolume ----------------------------------------------------
    public void ToggleVolume(UnityEngine.Video.VideoPlayer vp)
    {
        // change volume for a specific video panel (ie. the one you are hovering over)
        float tempVolume = vp.GetDirectAudioVolume(0);
        if (tempVolume > 0.0f)
        {
            vp.SetDirectAudioVolume(0, 0.0f);
        }
        else
        {
            vp.SetDirectAudioVolume(0, 1.0f);
        }
    }

    public void ButtonEnter(UnityEngine.Video.VideoPlayer vp)
    {
        vp.Pause();
        ShowVideoName(vp, true);
    }

    public void ButtonExit(UnityEngine.Video.VideoPlayer vp)
    {
        vp.Play();
        ShowVideoName(vp, false);
    }
    public void JumpToFrame(UnityEngine.Video.VideoPlayer vp, float percentOfClip)
    {
        var newFrame = vp.frameCount * percentOfClip;
        vp.frame = (long)newFrame;
    }

    public void ShowControlPanel(UnityEngine.Video.VideoPlayer vp)
    {
        // TBD
    }
}
