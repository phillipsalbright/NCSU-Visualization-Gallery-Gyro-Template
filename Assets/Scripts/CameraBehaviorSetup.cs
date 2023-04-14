using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviorSetup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(15360, 1080, true);
        //if you want the windows build to emulate the vis gallery on any monitor, uncomment this code and delete line 10:
        /**
        int resx = Screen.resolutions[0].width;
        int resy = Screen.resolutions[0].height;
        Screen.SetResolution(resx, Mathf.RoundToInt(1080 * resx / 15360), true);
        */

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
