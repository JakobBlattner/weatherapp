using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DimmScreen : MonoBehaviour
{
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }
}
