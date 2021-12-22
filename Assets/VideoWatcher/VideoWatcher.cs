//using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Xml.Schema;

public class VideoWatcher : MonoBehaviour
{
    public string videoFileFolderPathMac = "////Users/tedbarnett/Dropbox (barnettlabs)/Videos/Ted Videos/";
    public string videoFileFolderPathWindows;
    public GameObject startupPanel; // hide after loading
    public GameObject videoPanels; // show after loading
    public GameObject canvasOfVideos; // canvas with all videos showing
    public List<GameObject> VideoPanel; // a list of n videoPanels (4, 6, or whatever can be displayed)
    public List<string> ValidVideoExtensions;
    public Text LoadingText;
    public string blankVideoFileName;
    private List<string> FavoriteVideosList = new List<string>();

    private int currentVideo = 0;
    public float showFilenameTimeSecs = 3;
    private int maxPanels = 6;
    private bool preparingSetup = false; // ensures nothing happens in Update loop
    private float launchedTime;
    public float secondsToAutoStart = 10.0f;
    public float skipVideosShorterThanSecs = 0.0f;
    public float longVideoLengthMinimum = 30.0f; // if video is longer than this minimum, start at a random frame
    public List<string> VideoFileNames;
    private bool firstLoop = true;
    private int firstLoopCounter = 0;
    private GameObject[] itemsToHideAtStart;
    private string videoFileFolderPath;
    private GameObject[] MinimizedVideoPanels;

    public struct FavoriteVideo
    {
        public string FileName;     // name of file, not including path
        public string Description;  // will default to null
        public float StartPointPct; // percent of file length
        public float EndPointPct;   // will set to zero if unknown

    }

    // ****************************************** START ****************************************************************
    void Start()
    {
        launchedTime = Time.time;
        //videoFileFolderPath = Application.streamingAssetsPath + "/";
        videoFileFolderPath = videoFileFolderPathMac; // default assumption is Mac platform
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) videoFileFolderPath = videoFileFolderPathWindows;

        startupPanel.SetActive(true);
        videoPanels.SetActive(true);

        maxPanels = VideoPanel.Count; //If on smaller screen, set maxPanels to a smaller number than VideoPanel.Count
        for (int i = 0; i < maxPanels; i++) // set up videoPlayer for each panel
        {
            var vp = VideoPanel[i].GetComponentInChildren<UnityEngine.Video.VideoPlayer>();
            vp.Stop(); // TODO: Stop vs. Play?
            vp.loopPointReached += PlayNextVideo; // when videos end, automatically play another
            vp.SetDirectAudioMute(0, true);
            vp.url = videoFileFolderPath + blankVideoFileName; // set to blank video
        }
        // Hide items tagged "HideAtStart" (e.g. if accidently left on in inspector!)
        if (itemsToHideAtStart == null)
            itemsToHideAtStart = GameObject.FindGameObjectsWithTag("HideAtStart");
        foreach (GameObject hideItem in itemsToHideAtStart)
        {
            hideItem.SetActive(false);
        }
        var canvasMaximized = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.CompareTag("MaximizedCanvas"));
        canvasMaximized.SetActive(false);

        SetupVideoList();
        preparingSetup = true; // starts wait cycle countdown (and covers over setup process)
    }

    // ---------------------------------------------------- SetupVideoList ----------------------------------------------------
    void SetupVideoList()
    {
        VideoFileNames = new List<string>();
        DirectoryInfo dir = new DirectoryInfo(videoFileFolderPath);
        FileInfo[] info = dir.GetFiles("*.*"); // TODO: Read from Dropbox or Google Drive instead?

        foreach (FileInfo f in info)
        {
            string fileNameString = Path.GetFileName(f.ToString());
            string fileExtension = Path.GetExtension(fileNameString).ToLower(); // lowercase extension
            if ((ValidVideoExtensions.Contains(fileExtension)) && (fileNameString != blankVideoFileName)) VideoFileNames.Add(fileNameString);
        }
        LoadingText.text = "Loading " + VideoFileNames.Count.ToString() + " videos...";

        if (VideoFileNames.Count == 0) Debug.Log("No files found in Directory");
    }


    // ****************************************************** UPDATE ****************************************************************
    void Update()
    {
        if (preparingSetup) {
            if ((Time.time - launchedTime) > secondsToAutoStart) // if time runs out, BeginPlaying()...
            {
                preparingSetup = false;
                BeginPlaying();
            }
        }
    }

    // ---------------------------------------------------- BeginPlaying ----------------------------------------------------
    public void BeginPlaying()
    {
        startupPanel.SetActive(false);
        videoPanels.SetActive(true);

        //for (int i = 0; i < maxPanels; i++) // start playing each video panel
        //{
        //    var vp = VideoPanel[i].GetComponentInChildren<UnityEngine.Video.VideoPlayer>();
        //    vp.Play(); // start playing each panel TODO: Not needed since playOnWake already?
        //}
    }


    // ---------------------------------------------------- PlayNextVideo ----------------------------------------------------
    public void PlayNextVideo(UnityEngine.Video.VideoPlayer vp)
    {
        // Runs when a video ends: load the next!
        currentVideo = Random.Range(0, VideoFileNames.Count - 1); // Choose next video at random
        if (firstLoop) vp.url = videoFileFolderPath + blankVideoFileName; // use blank video for first set of panels
        else vp.url = videoFileFolderPath + VideoFileNames[currentVideo];
        vp.prepareCompleted += SetVideoCaption;
        vp.Prepare();

    }
    // ---------------------------------------------------- SetVideoCaption ----------------------------------------------------
    void SetVideoCaption(UnityEngine.Video.VideoPlayer vp) // once video URL is prepared, set the currentFileNameText.text with the file name and length
    {
        vp.Pause(); // required on Mac to allow VideoPlayer to load attributes below
        TextMeshProUGUI currentFileNameText = vp.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        //TODO: Make sure audio is muted to avoid "pop" sound?
        if (firstLoop) // TODO: unnecessary now?
        {
            currentFileNameText.text = "";
            firstLoopCounter++;
            if (firstLoopCounter >= maxPanels) firstLoop = false;
        }
        else
        {
            float videoLength = (float)vp.length;
            if (videoLength > longVideoLengthMinimum) vp.frame = Mathf.FloorToInt(vp.frameCount * Random.Range(0.0f, 1.0f));

            int min = Mathf.FloorToInt(videoLength / 60);
            int sec = Mathf.FloorToInt(videoLength % 60);
            string videoLengthString;
            if (min > 0) videoLengthString = min.ToString("00") + ":" + sec.ToString("00");
            else videoLengthString = sec.ToString("00") + " secs";
            string vpFileName = vp.url;
            vpFileName = vpFileName.Replace(videoFileFolderPath, "");
            currentFileNameText.text = makeNameString(vpFileName) + "\n<alpha=#88><size=70%>(" + videoLengthString + ")</size>";
        }

        vp.Play();
        ResetVideoControls(vp);

    }

    // ---------------------------------------------------- ResetVideoControls ----------------------------------------------------

    public void ResetVideoControls(UnityEngine.Video.VideoPlayer vp)
    {
        var scrubSlider = vp.GetComponentInChildren<Slider>(true); // true means find even if inactive!
        scrubSlider.value = 0.0f;
        // reset Favorite "heart"
        GameObject heartIconON = vp.transform.Find("Favorite is ON").gameObject;
        heartIconON.SetActive(false); // TODO: Change this if this video HAD been favorited!

        GameObject videoControlPanel = vp.transform.Find("Video Control Panel").gameObject;
        GameObject heartIconOFF = videoControlPanel.transform.Find("Favorite is OFF").gameObject;
        heartIconOFF.SetActive(true);
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
            else monthText = dateText = "";
            dateString = monthText + dateText + dateString.Substring(0, 4);
        }
        newFileName = dateString; // Set name of file to date of movie clip (extracted from front of filename)
        return newFileName;
    }

    // ---------------------------------------------------- ShowVideoCaption ----------------------------------------------------
    public void ShowVideoCaption(UnityEngine.Video.VideoPlayer vp, bool showNameNow)
    {
        TextMeshProUGUI currentFileNameText = vp.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        if (showNameNow)
        {
            Debug.Log("Showing caption: video player is " + vp);

            SetVideoCaption(vp);
            currentFileNameText.color = new Color32(255, 255, 0, 255); // set text to yellow color to make it visible
        }
        else
        {
            Debug.Log("Hiding caption: video player is " + vp);
            currentFileNameText.color = new Color32(255, 255, 0, 0); // set text to transparent to hide
        }
    }

    // ---------------------------------------------------- ToggleVolume ----------------------------------------------------
    public void ToggleVolume(UnityEngine.Video.VideoPlayer vp)
    {
        // toggle mute for this video panel
        vp.SetDirectAudioMute(0, !vp.GetDirectAudioMute(0));
        // TODO: Set corresponding "Mute" icon in the minimized version of this video!
    }

    // ---------------------------------------------------- SetFavorite ----------------------------------------------------
    public void SetFavorite(UnityEngine.Video.VideoPlayer vp)
    {
        // TODO: Set corresponding "Favorite" icon in the minimized version of this video!
        videoFileFolderPath = videoFileFolderPathMac; // default assumption is Mac platform
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) videoFileFolderPath = videoFileFolderPathWindows;

        float favoriteStartPointPct = (float)vp.frame / vp.frameCount;

        string vpFileName = vp.url;
        vpFileName = vpFileName.Replace(videoFileFolderPath, "");
        vpFileName = vpFileName.Replace("\"", "\"\""); // deal with quotation marks in file names for CSV


        FavoriteVideo vpInfo = new FavoriteVideo
        {
            FileName = vpFileName,
            Description = "(tbd)",
            StartPointPct = favoriteStartPointPct,
            EndPointPct = 0.0f
        };

        string csvString = '"' + vpInfo.FileName + '"' + "," + '"' + vpInfo.Description + '"' + "," + vpInfo.StartPointPct.ToString() + "," + vpInfo.EndPointPct.ToString();
        Debug.Log("SETFAVORITE: " + csvString);
        StreamWriter favoritesFile = new StreamWriter(Application.persistentDataPath + "_favorites2021.csv", true);
        favoritesFile.WriteLine(csvString);
        favoritesFile.Close();
    }

    // ---------------------------------------------------- DeleteVideo ----------------------------------------------------
    public void DeleteVideo(UnityEngine.Video.VideoPlayer vp)
    {
        videoFileFolderPath = videoFileFolderPathMac; // default assumption is Mac platform
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) videoFileFolderPath = videoFileFolderPathWindows;

        string vpFileName = vp.url;
        vpFileName = vpFileName.Replace(videoFileFolderPath, "");
        vpFileName = vpFileName.Replace("\"", "\"\""); // deal with quotation marks in file names for CSV

        string csvString = '"' + vpFileName + '"';
        Debug.Log("DELETEVIDEO: " + csvString);
        StreamWriter deletesFile = new StreamWriter(Application.persistentDataPath + "_delete_list_2021.csv", true);
        deletesFile.WriteLine(csvString);
        deletesFile.Close();
    }

    // ---------------------------------------------------- JumpToFrame ----------------------------------------------------

    public void JumpToFrame(UnityEngine.Video.VideoPlayer vp, float percentOfClip)
    {
        var newFrame = vp.frameCount * percentOfClip;
        vp.frame = (long)newFrame;
    }


    // ---------------------------------------------------- MaximizeVideoPanel ----------------------------------------------------
    public void MaximizeVideoPanel(UnityEngine.Video.VideoPlayer vp)
    {
        // find all of the Minimized Video Panels and Pause() them
        //if (MinimizedVideoPanels == null)
        MinimizedVideoPanels = GameObject.FindGameObjectsWithTag("MinimizedVideoPanels");

        foreach (GameObject smallPanel in MinimizedVideoPanels)
        {
            var smallVP = smallPanel.GetComponentInChildren<UnityEngine.Video.VideoPlayer>();
            if (smallVP != vp)
            {
                smallVP.Pause();
            }
        }
        var canvasMaximized = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.CompareTag("MaximizedCanvas"));
        var vpTexture = vp.targetTexture;
        var newVP = canvasMaximized.GetComponentInChildren<UnityEngine.Video.VideoPlayer>();
        var newVPRawImage = newVP.GetComponentInChildren<RawImage>();
        TextMeshProUGUI originalVPFileName = vp.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI newVPFileName = newVP.gameObject.GetComponentInChildren<TextMeshProUGUI>();

        newVP.targetTexture = vpTexture;
        newVPRawImage.texture = vpTexture;
        newVP.frame = vp.frame;
        newVPFileName.text = originalVPFileName.text;
        canvasMaximized.SetActive(true);
    }

    // ---------------------------------------------------- MinimizeVideoPanel ----------------------------------------------------
    public void MinimizeVideoPanel(UnityEngine.Video.VideoPlayer vp)
    {
        // find all of the Minimized Video Panels and Play() them
        if (MinimizedVideoPanels == null)
            MinimizedVideoPanels = GameObject.FindGameObjectsWithTag("MinimizedVideoPanels");
        foreach (GameObject smallPanel in MinimizedVideoPanels)
        {
            var smallVP = smallPanel.GetComponentInChildren<UnityEngine.Video.VideoPlayer>();
            smallVP.Play();
        }
        var canvasMaximized = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.CompareTag("MaximizedCanvas"));
        canvasMaximized.SetActive(false);
    }
    // ---------------------------------------------------- ScrubVideoPosition ----------------------------------------------------
    public void ScrubVideoPosition(UnityEngine.Video.VideoPlayer vp)
    {
        // Move video to the frame indicated by the scrubber (% of all frames)
        var scrubSlider = vp.GetComponentInChildren<Slider>();
        float newFrameFloat = (scrubSlider.value * (float)vp.frameCount);
        long newFrame = (long)newFrameFloat;

        vp.frame = newFrame;
    }

    // ---------------------------------------------------- SkipToNextVideo ----------------------------------------------------
    public void SkipToNextVideo(UnityEngine.Video.VideoPlayer vp)
    {
        vp.Stop();
        PlayNextVideo(vp);

    }
    // ---------------------------------------------------- QuitApplication ----------------------------------------------------
    public void QuitApplication()
    {
        Application.Quit();
    }



    /* TODO List
     * 
     * Fix popping audio during transition
     * Enable "Delete Video"
     * Make sure maximized video Favorite and Mute work properly
     * Show file name on maximized version
     * Add a slider to each video, showing % played.  Update when hovering?  Or anytime?
     * Allow user to set video to "loop" (e.g. for short ones)
     * Allow user to jump forward or back on a video
     * Do JumpToFrame for longer videos (i.e. don't start on frame 0)
     * 
     * LONG TERM:
     * Instantiate video panels when needed (avoid prefab issues?)
     * Make an iPad version
     * TEST: Favorites: escape " or ' characters for CSV (e.g. 2020-12-26 05-17 pm Testing 2.8" LCD touchscreen.mov)
     */
}
