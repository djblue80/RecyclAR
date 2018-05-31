using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class UtilityExtensions {

    public static Vector3 infiniteVector3 = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

	public static Vector3 Sign(this Vector3 value){
		return new Vector3(Mathf.Sign(value.x), Mathf.Sign(value.y), Mathf.Sign(value.z));
	}

	public static Vector3 Abs(this Vector3 value){
		return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
	}

    public static float Remap(this float value, float from1, float to1, float from2, float to2) {
		float result = (value - from1) / (to1 - from1) * (to2 - from2) + from2;
		return Mathf.Clamp(result,Mathf.Min(from2,to2), Mathf.Max(to2, from2));
    }

    public static int Remap(this int value, int from1, int to1, int from2, int to2) {
		float result = ((float)value - from1) / (to1 - from1) * (to2 - from2) + from2;
		return (int)Mathf.Clamp(result,Mathf.Min(from2,to2), Mathf.Max(to2,from2));
    }


	public static bool IsWithinLimits(this float value, float min, float max){
		return (value >= min) && (value <= max);
	}

    public static void DestroyAllChildren(this Transform target) {
        int childCount = target.childCount;
        for (int i = childCount - 1; i >= 0; i--) {
            GameObject.Destroy(target.GetChild(i).gameObject);
        }
    }

    /// <summary>compares two floats and returns true of their difference is less than floatDiff</summary>
    public static bool AlmostEquals(this float target, float second, float floatDiff)
    {
        return Mathf.Abs(target - second) < floatDiff;
    }

    public static GameObject[] FindGameObjectsWithTag(this Transform transform, string tag, bool includeInactive = false) {

        List<GameObject> results = new List<GameObject>();

        foreach (Transform child in transform) {
            if (!child.gameObject.activeSelf && !includeInactive) continue;
            if (child.CompareTag(tag)) results.Add(child.gameObject);
        }

        return results.ToArray();
    }

    public static GameObject FindGameObjectByName(this Transform transform, string name, bool includeInactive = false) {

        foreach (Transform child in transform) {
            if (!child.gameObject.activeSelf && !includeInactive) continue;
            if (child.name == name) return child.gameObject;
        }

        return null;
    }

    public static GameObject[] FindGameObjectsByName(this Transform transform, string name, bool includeInactive = false) {

        List<GameObject> results = new List<GameObject>();

        foreach (Transform child in transform) {
            if (!child.gameObject.activeSelf && !includeInactive) continue;
            if (child.name == name) results.Add(child.gameObject);
        }

        return results.ToArray();
    }

    public static Vector3 RotateAroundPivot(this Vector3 point, Vector3 pivot, Quaternion rotation) {
        Vector3 dir = point - pivot;
        return rotation * dir + pivot;
    }


    #region VertexColor Utilities

    public static Color RemapToColor(this float current, float fromF, float toF, Color fromC, Color toC)
    {
        float amt = current.Remap(fromF, toF, 0f, 1f);
        return Color.Lerp(fromC, toC, amt);
    }


    public static void ChangeVertexColor(this GameObject go, Color newColor) {
        MeshFilter mF = go.GetComponentInChildren<MeshFilter>();
        if (mF == null) return;
        Mesh mesh = mF.mesh;

		int vertCount = mesh.vertexCount;
		Color32[] colors = new Color32[vertCount];
        int i = 0;
		if (vertCount == 0) return;
		while (i < vertCount)
        {
            colors[i] = newColor;
            i++;
        }
		mesh.colors32 = colors;
    }

    public static Color GetVertexColor(this GameObject go) {
        MeshFilter mF = go.GetComponentInChildren<MeshFilter>();
        if (mF == null) return Color.clear;
        Color[] colors = mF.mesh.colors;
        if (colors.Length == 0) {
            return Color.clear;
        }
        else return colors[0];
    }

    #endregion


    #region Color Utilities

	public static Color ConvertHexColor(float r, float g, float b, float a=255){
		return new Color(r/255.0f, g/255.0f, b/255.0f, a/255.0f);
	}

    public static Vector3 ConvertRGBtoHSV(this Color color)
    {
        Vector3 hsv = new Vector3();
        float min, max, delta;
        min = Mathf.Min(color.r, color.g, color.b);
        max = Mathf.Max(color.r, color.g, color.b);
        hsv.z = max;               // v
        delta = max - min;

        if (max != 0) hsv.y = delta / max;       // s
        else
        {
            // r = g = b = 0		// s = 0, v is undefined
            hsv.y = 0;
            hsv.x = -1;
            return hsv;
        }
        if (color.r == max)
            hsv.x = (color.g - color.b) / delta;       // between yellow & magenta
        else if (color.g == max)
            hsv.x = 2 + (color.b - color.r) / delta;   // between cyan & yellow
        else
            hsv.x = 4 + (color.r - color.g) / delta;   // between magenta & cyan

        hsv.x *= 60;               // degrees
        if (hsv.x < 0) hsv.x += 360;

		return hsv;
    }

    public static Color ConvertHSVtoRGB(float h, float s, float v)
    {
        int i;
        float f, p, q, t;
        if (s == 0)
        {
            // achromatic (grey)
            return new Color(v,v,v);
        }
        h /= 60;            // sector 0 to 5
        i = Mathf.FloorToInt(h);
        f = h - i;          // factorial part of h
        p = v * (1 - s);
        q = v * (1 - s * f);
        t = v * (1 - s * (1 - f));
        Color rgb = Color.black;
        switch (i)
        {
            case 0:
                rgb.r = v;
                rgb.g = t;
                rgb.b = p;
                break;
            case 1:
                rgb.r = q;
                rgb.g = v;
                rgb.b = p;
                break;
            case 2:
                rgb.r = p;
                rgb.g = v;
                rgb.b = t;
                break;
            case 3:
                rgb.r = p;
                rgb.g = q;
                rgb.b = v;
                break;
            case 4:
                rgb.r = t;
                rgb.g = p;
                rgb.b = v;
                break;
            default:        // case 5:
                rgb.r = v;
                rgb.g = p;
                rgb.b = q;
                break;
        }

        return rgb;
    }

    public static Color GetDifferentColor(this Color original)
    {
        Vector3 newColor_hsv = new Vector3(Random.Range(0, 360f), Random.Range(0.2f, 1f), 1f);
        Vector3 oldColor_hsv = original.ConvertRGBtoHSV();
        //Debug.Log("original color hsv: " + oldColor_hsv + " old color: " + original);
        float nextStepColor = 120f;
		float nextStepColorY = 0.4f;
        //newColor_hsv.x = (Mathf.Abs(newColor_hsv.x - oldColor_hsv.x) > nextStepColor) ? newColor_hsv.x : Mathf.Repeat(newColor_hsv.x + nextStepColor, 360);
		if (Mathf.Abs (newColor_hsv.x - oldColor_hsv.x) < nextStepColor) 
		{
//			Debug.Log ("using next step color x");
			newColor_hsv.x = Mathf.Repeat (newColor_hsv.x + nextStepColor, 360f);
		}
		if(Mathf.Abs(newColor_hsv.y - oldColor_hsv.y) < nextStepColorY)
		{
			
			newColor_hsv.y = Mathf.Repeat (newColor_hsv.y + nextStepColorY, 1f);
			if(newColor_hsv.y < 0.2f) newColor_hsv.y += 0.2f; 
//			Debug.Log ("using next step color y");
		}
		//		Debug.Log("new color hsv: " + newColor_hsv);
        return ConvertHSVtoRGB(newColor_hsv.x, newColor_hsv.y, newColor_hsv.z);
    }


    #endregion

    public static IEnumerator WaitForRealSeconds(float time)
    {
        float start = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < (start + time))
        {
            yield return null;
        }
    }

	public static void Shuffle<T>(ref T[] source){
		for(int i=0;i<source.Length;++i){
			int r = Random.Range(0,i);
			T tmp = source[i];
			source[i] = source[r];
			source[r] = tmp;
		}
	}

	public static T DeepClone<T>(T obj)
	{
		using (var ms = new MemoryStream())
		{
			var formatter = new BinaryFormatter();
			formatter.Serialize(ms, obj);
			ms.Position = 0;

			return (T) formatter.Deserialize(ms);
		}
	}

}