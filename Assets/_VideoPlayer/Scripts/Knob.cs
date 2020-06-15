using UnityEngine;

// [RequireComponent(typeof(MeshCollider))]
/*

   .-------.                             .--.    .-------.     .--.            .--.     .--.        
   |       |--.--.--------.-----.-----.--|  |    |_     _|--.--|  |_.-----.----|__|---.-|  |-----.
   |   -   |_   _|        |  _  |     |  _  |      |   | |  |  |   _|  _  |   _|  |  _  |  |__ --|
   |_______|__.__|__|__|__|_____|__|__|_____|      |___| |_____|____|_____|__| |__|___._|__|_____|
   © 2019 OXMOND / www.oxmond.com 

*/

public class Knob : MonoBehaviour
{
    public GameObject videoPlayer;
    private MyVideoPlayer videoPlayerScript;

    void Start()
    {
        videoPlayerScript = videoPlayer.GetComponent<MyVideoPlayer>();
    }

    void OnMouseDown()
    {
        videoPlayerScript.KnobOnPressDown();
    }

    void OnMouseUp()
    {
        videoPlayerScript.KnobOnRelease();
    }

    void OnMouseDrag()
    {
        videoPlayerScript.KnobOnDrag();
    }

}
