using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using UnityEngine.Events;

/// <summary>
/// Unused. Extension to LeanTouch API's pinch gesture. 
/// </summary>
public class LeanFingerPinch : MonoBehaviour {

    [Tooltip("Ignore fingers with StartedOverGui?")]
    public bool IgnoreStartedOverGui = true;

    [Tooltip("Ignore fingers with IsOverGui?")]
    public bool IgnoreIsOverGui;

    [Tooltip("Allows you to force rotation with a specific amount of fingers (0 = any)")]
    public int RequiredFingerCount;

    [Tooltip("If RequiredSelectable.IsSelected is false, ignore?")]
    public LeanSelectable RequiredSelectable;

    // Called on the first frame the conditions are met
    public UnityEvent OnPinch;

    //public bool ignorePinchOut = false;

#if UNITY_EDITOR
    private void Reset()
    {
        Start();
    }
#endif

    private void Start()
    {
        if (RequiredSelectable == null)
        {
            RequiredSelectable = GetComponent<LeanSelectable>();
        }
    }


    //private void OnEnable()
    //{
    //    LeanTouch.OnGesture += FingerPinch;
    //}

    //private void OnDisable()
    //{
    //    LeanTouch.OnGesture -= FingerPinch;
    //}

    //private void FingerPinch(List<LeanFinger> fingers)
    //{

    //}

    protected virtual void LateUpdate()
    {
        // Get the fingers we want to use
        var fingers = LeanSelectable.GetFingers(IgnoreStartedOverGui, IgnoreIsOverGui, RequiredFingerCount, RequiredSelectable);
        
        // Get the pinch ratio of these fingers
        var pinchRatio = LeanGesture.GetPinchRatio(fingers);
        
        if (pinchRatio < 1 && OnPinch != null)
        {
            OnPinch.Invoke();
            Debug.Log("pinching");
        }
    }
}
