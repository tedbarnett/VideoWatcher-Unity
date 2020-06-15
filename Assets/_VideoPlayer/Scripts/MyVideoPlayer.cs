/*
modified by Barnett Labs from: © 2019 OXMOND / www.oxmond.com
*/

using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class MyVideoPlayer : MonoBehaviour
{

    public GameObject cinemaPlane;
    public GameObject btnPlay;
    public GameObject btnPause;
    public GameObject knob;
    public GameObject progressBar;
    public GameObject progressBarBG;

    private float maxKnobValue;
    private float newKnobX;
    private float maxKnobX;
    private float minKnobX;
    private float knobPosY;
    private float simpleKnobValue;
    private float knobValue;
    private float progressBarWidth;
    private bool knobIsDragging;
    private bool videoIsJumping = false;
    private bool videoIsPlaying = false;
    private VideoPlayer videoPlayer;
    private bool haveReadFrameCount = false;
    
    private void Start  ()
    {
        knobPosY = knob.transform.localPosition.y;
        videoPlayer = GetComponent<VideoPlayer>();
        btnPause.SetActive(true);
        btnPlay.SetActive(false);
        videoPlayer.frame = (long)100;
        progressBarWidth = progressBarBG.GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void Update()
    {
        if (!knobIsDragging && !videoIsJumping)
        {
            if (videoPlayer.frameCount > 0)
            {
                float progress = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
                progressBar.transform.localScale = new Vector3(progressBarWidth * progress, progressBar.transform.localScale.y, 0);
                knob.transform.localPosition = new Vector2(progressBar.transform.localPosition.x + (progressBarWidth * progress), knob.transform.localPosition.y);
                if (!haveReadFrameCount)
                {
                    Debug.Log(videoPlayer.frameCount + " frames: " + videoPlayer.url);
                    haveReadFrameCount = true;
                }

            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Input.mousePosition;
            Collider2D hitCollider = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(pos));
            
            if (hitCollider != null && hitCollider.CompareTag(btnPause.tag))
            {
                BtnPlayVideo();
            }
            if (hitCollider != null && hitCollider.CompareTag(btnPlay.tag))
            {
                print("playBtn");
                BtnPlayVideo();
            }
            if (hitCollider != null && hitCollider.CompareTag(cinemaPlane.tag))
            {
                print("clicked Video window");
                // BtnPlayVideo();
            }
        }

    }

    public void KnobOnPressDown()
    {
        VideoStop();
        minKnobX = progressBar.transform.localPosition.x;
        maxKnobX = minKnobX + progressBarWidth;
    }

    public void KnobOnRelease()
    {
        knobIsDragging = false;
        CalcKnobSimpleValue();
        VideoPlay();
        VideoJump();
        StartCoroutine(DelayedSetVideoIsJumpingToFalse());
    }

    IEnumerator DelayedSetVideoIsJumpingToFalse()
    {
        yield return new WaitForSeconds(2);
        SetVideoIsJumpingToFalse();
    }

    public void KnobOnDrag()
    {
        knobIsDragging = true;
        videoIsJumping = true;
        Vector3 curScreenPoint = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
        knob.transform.position = new Vector2(curPosition.x, curPosition.y);
        newKnobX = knob.transform.localPosition.x;
        if (newKnobX > maxKnobX) { newKnobX = maxKnobX; }
        if (newKnobX < minKnobX) { newKnobX = minKnobX; }
        knob.transform.localPosition = new Vector2(newKnobX, knobPosY);
        CalcKnobSimpleValue();
        progressBar.transform.localScale = new Vector3(simpleKnobValue * progressBarWidth, progressBar.transform.localScale.y, 0);
    }

    private void SetVideoIsJumpingToFalse()
    {
        videoIsJumping = false;
    }

    private void CalcKnobSimpleValue()
    {
        maxKnobValue = maxKnobX - minKnobX;
        knobValue = knob.transform.localPosition.x - minKnobX;
        simpleKnobValue = knobValue / maxKnobValue;
    }

    private void VideoJump()
    {
        var frame = videoPlayer.frameCount * simpleKnobValue;
        videoPlayer.frame = (long)frame;
    }

    private void BtnPlayVideo()
    {
        if (videoIsPlaying)
        {
            VideoStop();
        }
        else
        {
            VideoPlay();
        }
    }

    private void VideoStop()
    {
        videoIsPlaying = false;
        videoPlayer.Pause();
        btnPause.SetActive(false);
        btnPlay.SetActive(true);
    }

    private void VideoPlay()
    {
        videoIsPlaying = true;
        videoPlayer.Play();
        btnPause.SetActive(true);
        btnPlay.SetActive(false);
    }
}
