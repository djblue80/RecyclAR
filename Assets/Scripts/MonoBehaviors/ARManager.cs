using UnityEngine;
using System.Collections.Generic;
using GoogleARCore;

/// <summary>
/// Manages the current AR API.
/// </summary>
[DisallowMultipleComponent]
public class ARManager : MonoBehaviour
{
    /// <summary>
    /// Singleton reference to ARManager.
    /// </summary>
    private static ARManager m_Instance;
    public static ARManager Instance
    {
        get
        {
            if (!m_Instance)
            {
                GameObject go = GameObject.Find("GameManager");
                if (!go)
                {
                    Debug.LogError("Cannot find GameManager");
                    return null;
                }
                return go.GetComponent<ARManager>();
            }
            return m_Instance;
        }

        set
        {
            m_Instance = value;
        }
    }

    /// <summary>
    /// A list to hold all planes ARCore is tracking in the current frame. This object is used across
    /// the application to avoid per-frame allocations.
    /// </summary>
    private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();

    /// <summary>
    /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
    /// </summary>
    private bool m_IsQuitting = false;

    /// <summary>
    /// List of established anchors.
    /// </summary>
    public List<DetectedPlane> anchors = new List<DetectedPlane>();

    /// <summary>
    /// Current number of tracked poses.
    /// </summary>
    public int CurrentTrackedCount { get { return anchors.Count;  } }

    /// <summary>
    /// Unity Update function.
    /// </summary>
    private void Update()
    {
        // Check that motion tracking is tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        //if (GameManager.Instance.CurrentState != GameManager.AppState.Preparation &&
        //    GameManager.Instance.CurrentState != GameManager.AppState.InPlay)
        //    return;

        UpdateApplicationLifeCycle();

        Session.GetTrackables<DetectedPlane>(m_AllPlanes, TrackableQueryFilter.New);
        for (int i = 0; i < m_AllPlanes.Count; i++)
        {
            if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
            {
                anchors.Add(m_AllPlanes[i]);
                ////TODO: reparent instances to another transform component
                //GameObject planeObject = Instantiate(DetectedPlanePrefab, m_AllPlanes[i].CenterPose.position, m_AllPlanes[i].CenterPose.rotation, transform);
                //planeObject.transform.localScale = Vector3.one * GameManager.UnitScaleMultiplier;
            }
        }
    }

    /// <summary>
    /// Check and update the application lifecycle.
    /// </summary>
    private void UpdateApplicationLifeCycle()
    {
        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_InvokeQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_InvokeQuit", 0.5f);
        }
    }


    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _InvokeQuit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
