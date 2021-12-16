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
    public string blankVideoFileName;

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
    private int firstLoopCounter = 0;

    //          ****************************************** START ****************************************************************
    void Start()
    {
        Debug.Log("START: Time.time = " + Time.time);
        videoFileFolderPath = Application.streamingAssetsPath + "/";
        videoFileFolderPath = videoFileFolderPathMac; // default assumption is Mac platform
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) videoFileFolderPath = videoFileFolderPathWindows;

        startupPanel.SetActive(false);
        videoPanels.SetActive(true);
        launchedTime = Time.time;
        maxPanels = VideoPanel.Count; //If on smaller screen, set maxPanels to a smaller number than VideoPanel.Count

        for (int i = 0; i < maxPanels; i++) // set up videoPlayer for each panel
        {
            var vp = VideoPanel[i].GetComponentInChildren<UnityEngine.Video.VideoPlayer>();
            vp.loopPointReached += PlayNextVideo; // when videos end, automatically play another

            var currentButton = VideoPanel[i].GetComponentInChildren<Button>(); // finds the first button child and sets currentButton to that
            currentButton.onClick.AddListener(() => { ClickedOnVideoPanel(vp); });
            vp.SetDirectAudioVolume(0, 0.0f); // Mute volume
        }
    }
    //           ****************************************** UPDATE ****************************************************************
    void Update()
    {

        // TODO: Delete this waiting period click-to-start thing?
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

    // ---------------------------------------------------- ClickToStart ----------------------------------------------------
    public void ClickToStart()
    {
        SetupVideoList(); // read all file names in the videoFileFolderPath directory into VideoFileNames[]

        startupPanel.SetActive(false);
        videoPanels.SetActive(true);
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
            if ((ValidVideoExtensions.Contains(fileExtension)) && (fileNameString != blankVideoFileName)) VideoFileNames.Add(fileNameString);
        }
        Debug.Log("Total # videos = " + VideoFileNames.Count);
        Debug.Log("Counted: Time.time = " + Time.time);

        if (VideoFileNames.Count == 0) Debug.Log("No files found in Directory");
    }
    // ---------------------------------------------------- PlayNextVideo ----------------------------------------------------
    public void PlayNextVideo(UnityEngine.Video.VideoPlayer vp)
    {
        currentVideo = Random.Range(0, VideoFileNames.Count - 1); // Choose next video at random

        vp.url = videoFileFolderPath + VideoFileNames[currentVideo];
        if(firstLoop) vp.url = videoFileFolderPath + blankVideoFileName;
        vp.prepareCompleted += SetVideoNameText;
        vp.Prepare();
    }
    // ---------------------------------------------------- SetVideoNameText ----------------------------------------------------
    void SetVideoNameText(UnityEngine.Video.VideoPlayer vp) // once video URL is loaded, set the currentFileNameText.text with the file name and length
    {
        TextMeshProUGUI currentFileNameText = vp.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        if (firstLoop)
        {
            currentFileNameText.text = "";
            vp.Play();
            firstLoopCounter++;
            if (firstLoopCounter >= maxPanels) firstLoop = false;
            return;
        }
        float videoLength = (float)vp.length;
        string tempVideoName;
        if (videoLength > longVideoLengthMinimum) vp.frame = Mathf.FloorToInt(vp.frameCount * Random.Range(0.0f, 1.0f));

        int min = Mathf.FloorToInt(videoLength / 60);
        int sec = Mathf.FloorToInt(videoLength % 60);
        string videoLengthString = "";
        if (min > 0) videoLengthString = min.ToString("00") + ":" + sec.ToString("00");
            else videoLengthString = sec.ToString("00") + " secs";

        // Set video name using makeNameString()
        tempVideoName = VideoFileNames[currentVideo]; // lookup full file name via currentVideo
        currentFileNameText.text = makeNameString(tempVideoName) + "\n<alpha=#88><size=70%>(" + videoLengthString + ")</size>";
        vp.Play();
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
            SetVideoNameText(vp);
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
            if (monthNum != 0)
            {
                if (dateNum == 0) // if month is set but date is 00 (i.e. we knew the month but not the day)
                {
                    monthText = monthNames[monthNum - 1].ToString() + " ";
                    dateText = "";
                }
                else // if month and date are valid
                {
                    monthText = monthNames[monthNum - 1].ToString() + " ";
                    dateText = dateNum.ToString() + " ";
                }
            }
            else
            {
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
    // ---------------------------------------------------- ButtonEnter ----------------------------------------------------
    public void ButtonEnter(UnityEngine.Video.VideoPlayer vp)
    {
        vp.Pause();
        ShowVideoName(vp, true);
    }
    // ---------------------------------------------------- ButtonExit ----------------------------------------------------
    public void ButtonExit(UnityEngine.Video.VideoPlayer vp)
    {
        vp.Play();
        ShowVideoName(vp, false);
    }
    // ---------------------------------------------------- JumpToFrame ----------------------------------------------------

    public void JumpToFrame(UnityEngine.Video.VideoPlayer vp, float percentOfClip)
    {
        var newFrame = vp.frameCount * percentOfClip;
        vp.frame = (long)newFrame;
    }
    // ---------------------------------------------------- ShowControlPanel ----------------------------------------------------
    public void ShowControlPanel(UnityEngine.Video.VideoPlayer vp)
    {
        // TBD
    }
}
