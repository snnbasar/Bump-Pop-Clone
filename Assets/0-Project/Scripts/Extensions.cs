using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using DG.Tweening.Core;
using DG.Tweening;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;
using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class Extensions
{

    #region Sinan

    private static Camera _camera;

    public static Camera Camera
    {
        get
        {
            if (_camera == null) _camera = Camera.main;
            return _camera;
        }
    }

    // Get Main Canvas Transform
    private static Transform cachedCanvasTransform;
    public static Transform GetCanvasTransform()
    {
        if (cachedCanvasTransform == null)
        {
            Canvas canvas = MonoBehaviour.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                cachedCanvasTransform = canvas.transform;
            }
        }
        return cachedCanvasTransform;
    }

    private static readonly Dictionary<float, WaitForSeconds> WaitDictionary = new Dictionary<float, WaitForSeconds>();

    public static WaitForSeconds GetWait(float time)
    {
        if (WaitDictionary.TryGetValue(time, out var wait)) return wait;

        WaitDictionary[time] = new WaitForSeconds(time);
        return WaitDictionary[time];
    }


    private static PointerEventData _eventDataCurrentPosition;
    private static List<RaycastResult> _results;
    public static bool IsOverUI()
    {
        _eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        _results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(_eventDataCurrentPosition, _results);
        return _results.Count > 0;
    }

    public static Vector2 GetWorldPositionOfCanvasElement(this RectTransform element)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, Camera, out var result);
        return result;
    }


    public static void Fade(this SpriteRenderer renderer, float alpha)
    {
        var color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }


    public static T GetRandomItem<T>(this IList<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static async Task<T> GetUniqueItem<T>(this IList<T> list, T oldGettedItem)
    {
        T itemToGet;
        if (oldGettedItem == null)
            itemToGet = list.GetRandomItem();
        else
        {
            itemToGet = list.GetRandomItem();
            while (oldGettedItem.Equals(itemToGet))
            {
                itemToGet = list.GetRandomItem();
                await Task.Yield();
            }
        }

        return itemToGet;
    }

    public static IEnumerator SetActiveAfterSeconds(this GameObject obje, float time, bool sts = false, Action beforeSetactive = null)
    {
        yield return GetWait(time);
        beforeSetactive?.Invoke();
        obje.SetActive(sts);
    }

    public static void DoAfterSeconds(this MonoBehaviour obje, float time, Action actionAfterSeconds = null)
    {
        obje.StartCoroutine(CoroutineDoAfterSeconds(time, actionAfterSeconds));
    }
    public static IEnumerator CoroutineDoAfterSeconds(float time, Action actionAfterSeconds = null)
    {
        yield return GetWait(time);
        actionAfterSeconds?.Invoke();
    }

    public static string KMBMaker(double num)
    {
        double numStr;
        string suffix;
        string text;
        if (num < 1000d)
        {
            numStr = num;
            text = numStr.ToString();
            suffix = "";
        }
        else if (num < 1000000d)
        {
            numStr = num / 1000d;
            text = num % 1000d != 0 ? numStr.ToString("0.0") : numStr.ToString("0");
            suffix = "K";
        }
        else if (num < 1000000000d)
        {
            numStr = num / 1000000d;
            text = num % 1000000d != 0 ? numStr.ToString("0.0") : numStr.ToString("0");
            suffix = "M";
        }
        else
        {
            numStr = num / 1000000000d;
            text = num % 1000000000d != 0 ? numStr.ToString("0.0") : numStr.ToString("0");
            suffix = "B";
        }
        return text.ToString() + suffix;
    }
    public static float RemapNew(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static string SplitString(this string input)
    {
        return string.Join(" ", input.ToCharArray());
    }

    public static string SubtractText(this string fullText, string textToSubtract)
    {
        return fullText.Replace(textToSubtract, "");
    }

    public static void DestroyChildren(this Transform t)
    {
        foreach (Transform child in t)
        {
            UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    public static void SetLayersRecursively(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        foreach (Transform t in gameObject.transform) t.gameObject.SetLayersRecursively(layer);
    }

    public static Vector2 ToV2(this Vector3 input) => new Vector2(input.x, input.y);

    public static Vector3 Flat(this Vector3 input) => new Vector3(input.x, 0, input.z);

    public static Vector3Int ToVector3Int(this Vector3 vec3) => new Vector3Int((int)vec3.x, (int)vec3.y, (int)vec3.z);

    public static void ResetTransformation(this Transform trans)
    {
        trans.position = Vector3.zero;
        trans.localRotation = Quaternion.identity;
        trans.localScale = new Vector3(1, 1, 1);
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : MonoBehaviour
    {
        var component = gameObject.GetComponent<T>();
        if (component == null) gameObject.AddComponent<T>();
        return component;
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        for (var i = list.Count - 1; i > 1; i--)
        {
            var j = UnityEngine.Random.Range(0, i + 1);
            var value = list[j];
            list[j] = list[i];
            list[i] = value;
        }
    }

    public static T GetClosestTransform<T>(this Transform trans, T[] transforms) where T : MonoBehaviour
    {
        T tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = trans.position;
        foreach (T t in transforms)
        {
            float dist = Vector3.Distance(t.transform.position, currentPos);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        return tMin;
    }

    public static T GetFarestTransform<T>(this Transform trans, T[] transforms) where T : MonoBehaviour
    {
        T tMax = null;
        float maxDist = 0;
        Vector3 currentPos = trans.position;
        foreach (T t in transforms)
        {
            float dist = Vector3.Distance(t.transform.position, currentPos);
            if (dist > maxDist)
            {
                tMax = t;
                maxDist = dist;
            }
        }
        return tMax;
    }

    public static Vector3 XZFormation(this Transform trans, int current, int coloumn, float offset)
    {
        int xPos = current % coloumn;
        int yPos = 0;
        int zPos = current / coloumn;


        return new Vector3(xPos * offset, yPos * offset, zPos * offset);
    }

    public static Component CopyComponent(this Component original, GameObject destination)
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        // Copied fields can be restricted with BindingFlags
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy;
    }

    public static Color GetRandomColor()
    {
        return new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
    }

    public static int FloatToMilisecond(this float input)
    {
        return (int)(input * 1000);
    }

    public static int GetRandomMinusOneOrOne()
    {
        int x;
        if (UnityEngine.Random.value > 0.5f)
            x = 1;
        else
            x = -1;
        return x;
    }

    public static void DoThisToAllChildrenAndMe(this Transform v, Action<Transform> action)
    {
        action.Invoke(v);
        foreach (Transform t in v.transform) t.DoThisToAllChildrenAndMe(action);
    }


    #endregion

    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        if (a == b)
            return 0f;

        Vector3 AV = value - a;
        Vector3 AB = b - a;
        return Mathf.Clamp01(Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB));
    }

    public static float InverseLerp(Color a, Color b, Color value)
    {
        if (a == b)
            return 0f;

        Vector4 vA = new Vector4(a.r, a.g, a.b, a.a);
        Vector4 vB = new Vector4(b.r, b.g, b.b, b.a);
        Vector4 vValue = new Vector4(value.r, value.g, value.b, value.a);

        Vector4 AV = vValue - vA;
        Vector4 AB = vB - vA;
        return Mathf.Clamp01(Vector4.Dot(AV, AB) / Vector4.Dot(AB, AB));
    }
    public static bool ParseInt(this string str)
    {
        int result = 0;
        return int.TryParse(str, out result);
    }

    public static T FindComponentInScene<T>(string path) where T : Component
    {//For example for path : MainCamera/CameraPivot
        if (string.IsNullOrEmpty(path))
            throw new Exception("The path is null");

        path = path.Replace("\\", "/");

        var allRootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        var pathArray = new Queue<string>();

        if (path.Contains("/"))
        {
            string[] paths = path.Split('/');
            for (int i = 0; i < paths.Length; i++)
            {
                if (!string.IsNullOrEmpty(paths[i]))
                {
                    pathArray.Enqueue(paths[i]);
                }
            }
        }
        else
        {
            pathArray.Enqueue(path);
        }

        if (pathArray.Count <= 0)
            throw new Exception("The path is corrupt");

        Transform founded = null;

        while(pathArray.Count > 0)
        {
            string nextPath = pathArray.Dequeue();
            bool foundedPath = false;

            if (founded == null)
            {
                for (int i = 0; i < allRootObjects.Length;i++)
                {
                    if (allRootObjects[i].name == nextPath)
                    {
                        foundedPath = true;
                        founded = allRootObjects[i].transform;
                        break;
                    }
                }

                if (!foundedPath)
                    throw new Exception("The path is corrupt");
            }
            else
            {
                var childCount = founded.childCount;
                for (int i = 0; i < childCount; i ++)
                {
                    if (founded.GetChild(i).gameObject.name == nextPath)
                    {
                        foundedPath = true;
                        founded = founded.GetChild(i).transform;
                        break;
                    }
                }

                if (!foundedPath)
                    throw new Exception("The path is corrupt");
            }
        }

        if (founded == null)
        {
            throw new Exception("The object is not founded");
        }
        else
        {
            return founded.GetComponent<T>();
        }
    }

#if UNITY_EDITOR
    public static T DrawObjectField<T>(this T obj, string desc) where T : UnityEngine.Object
    {
        return EditorGUILayout.ObjectField(desc, obj, typeof(T), true) as T;
    }

    public static T GetTargetObjectOfProperty<T>(this SerializedProperty property) where T : class
    {
        if (property == null) return null;

        var path = property.propertyPath.Replace(".Array.data[", "[");
        object obj = property.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements)
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }
        }
        return obj as T;
    }

    private static object GetValue_Imp(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();

        while (type != null)
        {
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
                return f.GetValue(source);

            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
                return p.GetValue(source, null);

            type = type.BaseType;
        }
        return null;
    }

    private static object GetValue_Imp(object source, string name, int index)
    {
        var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
        if (enumerable == null) return null;
        var enm = enumerable.GetEnumerator();

        for (int i = 0; i <= index; i++)
        {
            if (!enm.MoveNext()) return null;
        }
        return enm.Current;
    }
#endif

    public static Bounds OrthographicBounds(this Camera camera)
    {
        Vector3 vec = camera.transform.position;
        Bounds bounds = new Bounds();
        bounds.min = new Vector3(
            vec.x - camera.orthographicSize,
            vec.y - camera.farClipPlane,
            vec.z - camera.orthographicSize);
        bounds.max = new Vector3(
            vec.x + camera.orthographicSize,
            vec.y - camera.nearClipPlane,
            vec.z + camera.orthographicSize);
        return bounds;
    }

    public static float Clamp0360(float eulerAngles)
    {
        float result = eulerAngles - Mathf.CeilToInt(eulerAngles / 360f) * 360f;
        if (result < 0)
        {
            result += 360f;
        }
        return result;
    }

    public static T GetNextEnum<T>(this T v) where T : struct
    {
        return Enum.GetValues(v.GetType()).Cast<T>().Concat(new[] { default(T) }).SkipWhile(e => !v.Equals(e)).Skip(1).First();
    }

    public static T GetPreviousEnum<T>(this T v) where T : struct
    {
        return Enum.GetValues(v.GetType()).Cast<T>().Concat(new[] { default(T) }).Reverse().SkipWhile(e => !v.Equals(e)).Skip(1).First();
    }

    public static string DeviceID()
    {
#if UNITY_EDITOR
        return SystemInfo.deviceUniqueIdentifier;
#elif UNITY_IOS
        return SystemInfo.deviceUniqueIdentifier;
#elif UNITY_ANDROID
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure");
        string androidId = secure.CallStatic<string>("getString", contentResolver, "android_id");
        up.Dispose();
        up = null;
        currentActivity.Dispose();
        currentActivity = null;
        contentResolver.Dispose();
        contentResolver = null;
        secure.Dispose();
        secure = null;
        return androidId;
#else
        return SystemInfo.deviceUniqueIdentifier;
#endif
    }

#if UNITY_EDITOR
    public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets;
    }
#endif

    public static string FloatToDecimalString(this float value)
    {
        string str = value.ToString();
        str = str.Replace(",",".");
        if (!str.Contains("."))
            str += ".";
        int indexOfDot = str.IndexOf(".");
        for (int i = indexOfDot + 1; i < indexOfDot + 3; i++)
        {

            try
            {
                var tmp = str[i];
            }
            catch (Exception)
            {
                str += "0";
            }
        }
        str = str.Substring(0, indexOfDot + 3);

        return str;
    }

    public static bool HaveIndex(this ICollection list, int index)
    {
        if (list == null || list.Count == 0)
            return false;
        if (index < 0)
            return false;

        return index < list.Count;
    }

    public static bool HaveIndex(this Array array, int index)
    {
        if (array == null || array.Length == 0)
            return false;
        if (index < 0)
            return false;

        return index < array.Length;
    }


    public static DateTime ConvertStringToDateTime(this string value)
    {
        return DateTime.ParseExact(value, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
    }
    public static string ConvertDateTimeToString(this DateTime value)
    {
        return value.ToString("dd.MM.yyyy HH:mm:ss");
    }
    public static int ConvertDateTimeToTimestamp(this DateTime value)
    {
        TimeSpan epoch = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
        return (int)epoch.TotalSeconds;
    }
    public static DateTime ConvertTimestampToDateTime(this float value)
    {
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(value).ToLocalTime();
        return dtDateTime;
    }

    public static string SerializeObject(this object obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }

    public static T DeepCopyObjectWithJson<T>(this T obj)
    {
        string json = JsonConvert.SerializeObject(obj);
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static string SecondsToMinuteFormat(int seconds)
    {
        int remaniningMinutes = seconds / 60;
        int remaniningSeconds = seconds - (remaniningMinutes * 60);
        string result = "";
        result += remaniningMinutes.ToString().Length <= 1 ? ("0" + remaniningMinutes.ToString()) : (remaniningMinutes.ToString());
        result += ":";
        result += remaniningSeconds.ToString().Length <= 1 ? ("0" + remaniningSeconds.ToString()) : (remaniningSeconds.ToString());
        return result;
    }

    public static string GetRandomLetter(System.Random rndm)
    {
        string chars = "_abcdefghijklmnopqrstuvwxyz1234567890";
        int num = rndm.Next(0, chars.Length - 1);
        return chars[num].ToString();
    }

    public static bool ContainsAny(this string haystack, params string[] needles)
    {
        foreach (string needle in needles)
        {
            if (haystack.Contains(needle))
                return true;
        }

        return false;
    }

    public static bool ParseBool(this string str)
    {
        if (str == null)
            return false;

        if (str.ToLower().Contains("true"))
        {
            return true;
        }
        else if (str.ToLower().Contains("false"))
        {
            return false;
        }
        else
        {
            throw new Exception("Not converting to bool!");
        }
    }
    public static int ToInt(this string str)
    {
        int i = 0;
        if (!int.TryParse(str, out i))
        {
            Debug.LogWarning("Can't parse this string: " + str);
        }
        return i;
    }

    public static T[] SubArrayDeepClone<T>(this T[] data, int index, int? length = null)
    {
        int subLength = -1;
        if (length != null)
        {
            subLength = length.Value;
        }
        else
        {
            subLength = data.Length - index;
        }

        T[] arrCopy = new T[subLength];
        Array.Copy(data, index, arrCopy, 0, subLength);
        using (MemoryStream ms = new MemoryStream())
        {
            var bf = new BinaryFormatter();
            bf.Serialize(ms, arrCopy);
            ms.Position = 0;
            return (T[])bf.Deserialize(ms);
        }
    }

    public static void SetColorA(this Image image, float a)
    {
        Color c = image.color;
        c.a = a;
        image.color = c;
    }

    public static bool CheckNet()
    {
        return !(Application.internetReachability == NetworkReachability.NotReachable);
    }

    public static void SwapValue<T>(ref T value1, ref T value2)
    {
        T temp = value1;
        value1 = value2;
        value2 = temp;
    }

    public static float EaseOut(float t)
    {
        return 1.0f - (1.0f - t) * (1.0f - t) * (1.0f - t);
    }

    public static float EaseIn(float t)
    {
        return t * t * t;
    }

    public static char CharToLower(char c)
    {
        return (c >= 'A' && c <= 'Z') ? (char)(c + ('a' - 'A')) : c;
    }

    public static int GCD(int a, int b)
    {
        int start = Mathf.Min(a, b);

        for (int i = start; i > 1; i--)
        {
            if (a % i == 0 && b % i == 0)
            {
                return i;
            }
        }

        return 1;
    }

    public static Canvas GetCanvas(this Transform transform)
    {
        if (transform == null)
        {
            return null;
        }

        Canvas canvas = transform.GetComponent<Canvas>();

        if (canvas != null)
        {
            return canvas;
        }

        return GetCanvas(transform.parent);
    }

    public static void CallExternalAndroid(string methodname, params object[] args)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJavaClass	unity			= new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			AndroidJavaObject	currentActivity	= unity.GetStatic<AndroidJavaObject>("currentActivity");
			currentActivity.Call(methodname, args);
#endif
    }

    public static T CallExternalAndroid<T>(string methodname, params object[] args)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJavaClass	unity			= new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			AndroidJavaObject	currentActivity	= unity.GetStatic<AndroidJavaObject>("currentActivity");
			return currentActivity.Call<T>(methodname, args);
#else
        return default(T);
#endif
    }


    public static void SetLayer(this GameObject gameObject, int layer, bool applyToChildren = false)
    {
        gameObject.layer = layer;

        if (applyToChildren)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                SetLayer(gameObject.transform.GetChild(i).gameObject, layer, true);
            }
        }
    }

    public static List<string[]> ParseCSVFile(this string fileContents, char delimiter)
    {
        List<string[]> csvText = new List<string[]>();
        string[] lines = fileContents.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            csvText.Add(lines[i].Split(delimiter));
        }

        return csvText;
    }

    public static void DestroyAllChildren(this Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(parent.GetChild(i).gameObject);
        }
    }

    public static string FindFile(string fileName, string directory)
    {
        List<string> files = new List<string>(System.IO.Directory.GetFiles(directory));
        string[] directories = System.IO.Directory.GetDirectories(directory);

        for (int i = 0; i < files.Count; i++)
        {
            if (fileName == System.IO.Path.GetFileNameWithoutExtension(files[i]))
            {
                return files[i];
            }
        }

        for (int i = 0; i < directories.Length; i++)
        {
            string path = FindFile(fileName, directories[i]);

            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }
        }

        return null;
    }

    public static string CalculateMD5Hash(this string input)
    {
        System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hash = md5.ComputeHash(inputBytes);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }

        return sb.ToString();
    }

    public static bool CompareLists<T>(List<T> list1, List<T> list2)
    {
        if (list1.Count != list2.Count)
        {
            return false;
        }

        for (int i = list1.Count - 1; i >= 0; i--)
        {
            bool found = false;

            for (int j = 0; j < list2.Count; j++)
            {
                if (list1[i].Equals(list2[j]))
                {
                    found = true;
                    list1.RemoveAt(i);
                    list2.RemoveAt(j);
                    break;
                }
            }

            if (!found)
            {
                return false;
            }
        }

        return true;
    }

    public static void PrintList<T>(this List<T> list)
    {
        string str = "";

        for (int i = 0; i < list.Count; i++)
        {
            if (i != 0)
            {
                str += ", ";
            }

            str += list[i].ToString();
        }

        Debug.Log(str);
    }

    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;

        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);

        return v;
    }

    public static Texture2D CreateColorFilledTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return texture;
    }

    public static List<string> GetFilesRecursively(this string path, string searchPatter)
    {
        List<string> files = new List<string>();

        if (!System.IO.Directory.Exists(path))
        {
            return files;
        }

        List<string> directories = new List<string>() { path };

        while (directories.Count > 0)
        {
            string directory = directories[0];

            directories.RemoveAt(0);

            files.AddRange(System.IO.Directory.GetFiles(directory, searchPatter));
            directories.AddRange(System.IO.Directory.GetDirectories(directory));
        }

        return files;
    }

    public static Vector2 SwitchToRectTransform(RectTransform from, RectTransform to)
    {
        Vector2 localPoint;
        Vector2 fromPivotDerivedOffset = new Vector2(from.rect.width * from.pivot.x + from.rect.xMin, from.rect.height * from.pivot.y + from.rect.yMin);
        Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, from.position);

        screenP += fromPivotDerivedOffset;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenP, null, out localPoint);

        Vector2 pivotDerivedOffset = new Vector2(to.rect.width * to.pivot.x + to.rect.xMin, to.rect.height * to.pivot.y + to.rect.yMin);

        return localPoint - pivotDerivedOffset;
    }
    
    public static void SetAlpha(this UnityEngine.UI.Graphic graphic, float alpha)
    {
        Color color = graphic.color;

        color.a = alpha;

        graphic.color = color;
    }

    public static T ParseEnum<T>(string _value)
    {
        return (T)Enum.Parse(typeof(T), _value, true);
    }

    public static List<T> ShiftElement<T>(List<T> list, int oldIndex, int newIndex)
    {
        T[] _list = list.ToArray();
        if (oldIndex == newIndex)
            return list;

        T tmp = list[oldIndex];

        if (newIndex < oldIndex)
            Array.Copy(list.ToArray(), newIndex, _list, newIndex + 1, oldIndex - newIndex);
        else
            Array.Copy(list.ToArray(), oldIndex + 1, _list, oldIndex, newIndex - oldIndex);

        _list[newIndex] = tmp;

        return _list.ToList();
    }

   


    public static TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions>
        DoMatrix(this Camera _source, Matrix4x4 endValue, float duration)
    {
        Matrix4x4 _initial = _source.projectionMatrix;
        float _a = 0f;
        return DOTween.To(() => _a, (_value) => { _a = _value; _source.projectionMatrix = MatrixLerp(_initial, endValue, _a); }, 1f, duration).SetUpdate(true);

    }
    private static Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float time)
    {
        Matrix4x4 ret = new Matrix4x4();
        for (int i = 0; i < 16; i++)
            ret[i] = Mathf.Lerp(from[i], to[i], time);
        return ret;
    }

    public static bool IsValidJson<T>(string _strInput)
    {
        if (IsValidJson(_strInput))
        {
            try
            {
                var _convertedObj = JsonConvert.DeserializeObject<T>(_strInput);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        return false;
    }
    public static bool IsValidJson<T>(string _strInput, out T result)
    {
        if (IsValidJson(_strInput))
        {
            try
            {
                var _convertedObj = JsonConvert.DeserializeObject<T>(_strInput);
                result = _convertedObj;
                return true;
            }
            catch (Exception)
            {
                result = default(T);
                return false;
            }
        }

        result = default(T);
        return false;
    }
    public static bool IsValidJson(string _strInput)
    {
        if (string.IsNullOrEmpty(_strInput) || _strInput.Length < 2)
            return false;

        _strInput = _strInput.Trim();
        if ((_strInput.StartsWith("{") && _strInput.EndsWith("}")) || //For object
            (_strInput.StartsWith("[") && _strInput.EndsWith("]"))) //For array
        {
            try
            {
                var obj = JToken.Parse(_strInput);
                return true;
            }
            catch (JsonReaderException jex)
            {
                //Exception in parsing json
                Debug.Log(jex.Message);
                return false;
            }
            catch (Exception ex) //some other exception
            {
                Debug.Log(ex.ToString());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public static float Distance(Vector3 a, Vector3 b)
    {
        Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        return Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }

    public static TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions>
        DoVolume(this AudioSource _source, float endValue, float duration)
    {
        return DOTween.To(() => _source.volume, (_value) => { _source.volume = _value; }, endValue, duration).SetUpdate(true);
    }

    public static Color32 SetColorA(this Color32 color, byte value)
    {
        Color32 c = color;
        c.a = value;
        color = c;
        return c;
    }
    public static Color SetColorA(this Color color, float value)
    {
        Color c = color;
        c.a = value;
        color = c;
        return c;
    }

    public static void SetSizeDeltaX(this RectTransform _transform, float _value)
    {
        Vector3 v = _transform.sizeDelta;
        v.x = _value;
        _transform.sizeDelta = v;
    }
    public static void SetSizeDeltaY(this RectTransform _transform, float _value)
    {
        Vector3 v = _transform.sizeDelta;
        v.y = _value;
        _transform.sizeDelta = v;
    }
    public static void SetAnchorPosX(this RectTransform _transform, float _value)
    {
        Vector3 v = _transform.anchoredPosition;
        v.x = _value;
        _transform.anchoredPosition = v;
    }
    public static void SetAnchorPosY(this RectTransform _transform, float _value)
    {
        Vector3 v = _transform.anchoredPosition;
        v.y = _value;
        _transform.anchoredPosition = v;
    }
    public static void SetPivotPosX(this RectTransform _transform, float _value)
    {
        Vector2 v = _transform.pivot;
        v.x = _value;
        _transform.pivot = v;
    }
    public static void SetPivotPosY(this RectTransform _transform, float _value)
    {
        Vector2 v = _transform.pivot;
        v.y = _value;
        _transform.pivot = v;
    }

    

    /*
    public static int ToInt(this string _string)
    {
        return int.Parse(_string);

    }
    public static int ToInt(this float f)
    {
        return (int)f;
    }
    */
    public static float ToFloat(this string _string)
    {
        return float.Parse(_string);

    }

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }


    /*
    public static T GetRandomObjectByPercentage<T>(List<T> _objects, List<float> _percentages)
    {
        //List<Transform> _transforms = new List<Transform>() { null, null, null, null };
        //List<float> _randomPercentages = new List<float>() { 10f,40f,30f,20f };
        //Extension.GetRandomObjectByPercentage<Transform>(_transforms, _randomPercentages);

        if (_objects == null || _percentages == null)
            throw new ArgumentNullException("objects or percentages is null");
        if (_objects.Count != _percentages.Count)
            throw new Exception("objects count is not equal percentages count");
        if (_objects.Any(x => x == null))
            throw new Exception("One or more objects are empty");
        if (_percentages.Sum(x => x) != 100f)
            throw new Exception("The total of percentages ise not equal 100");

        float _randomPercentage = UnityEngine.Random.Range(0f,100f);

        //throw new ArgumentNullException
    }
    */

    public enum RemapType
    {
        f1,
        f2,
        f3,
        f4,
        f5,
        f6,
        f7,
        f8,
        f9,
        f10,
        f11,
        f12,
        f13,
        f14,
        f15,
        f16,
        f17,
        f18,
        f19,
        f20,
        f21,
        f22,
        f23,
        f24,
    }
    public static float Remap(float value, float min1, float max1, float min2, float max2)
    {

        return ((value - min1) / (max1 - min1)) * (max2 - min2) + min2;
    }
    public static float Remap(RemapType remapType, float value, float min1, float max1, float min2, float max2)
    {
        if (remapType == RemapType.f1)
            return Remap(value, min1, min2, max1, max2);
        else if (remapType == RemapType.f2)
            return Remap(value, min1, min2, max2, max1);
        else if (remapType == RemapType.f3)
            return Remap(value, min1, max1, min2, max2);
        else if (remapType == RemapType.f4)
            return Remap(value, min1, max2, min2, max2);
        else if (remapType == RemapType.f5)
            return Remap(value, min1, max1, max2, min2);
        else if (remapType == RemapType.f6)
            return Remap(value, min1, max2, max2, min2);
        else if (remapType == RemapType.f7)
            return Remap(value, min2, min1, max1, max2);
        else if (remapType == RemapType.f8)
            return Remap(value, min2, min1, max2, max1);
        else if (remapType == RemapType.f9)
            return Remap(value, min2, max1, min1, max2);
        else if (remapType == RemapType.f10)
            return Remap(value, min2, max1, max2, min1);
        else if (remapType == RemapType.f11)
            return Remap(value, min2, max2, min1, max1);
        else if (remapType == RemapType.f12)
            return Remap(value, min2, max2, max1, min1);
        else if (remapType == RemapType.f13)
            return Remap(value, max1, min1, min2, max2);
        else if (remapType == RemapType.f14)
            return Remap(value, max1, min1, max2, min2);
        else if (remapType == RemapType.f15)
            return Remap(value, max1, min2, min1, max2);
        else if (remapType == RemapType.f16)
            return Remap(value, max1, min2, max2, min1);
        else if (remapType == RemapType.f17)
            return Remap(value, max1, max2, min1, min2);
        else if (remapType == RemapType.f18)
            return Remap(value, max1, max2, min2, min1);
        else if (remapType == RemapType.f19)
            return Remap(value, max2, min1, min2, max1);
        else if (remapType == RemapType.f20)
            return Remap(value, max2, min1, max1, min2);
        else if (remapType == RemapType.f21)
            return Remap(value, max2, min2, min1, max1);
        else if (remapType == RemapType.f22)
            return Remap(value, max2, min2, max1, min1);
        else if (remapType == RemapType.f23)
            return Remap(value, max2, max1, min1, min2);
        else if (remapType == RemapType.f24)
            return Remap(value, max2, max1, min2, min1);
        else
            return -99999999999;

    }


    public static Quaternion ToQuaternion(this Vector3 v)
    {
        return Quaternion.Euler(v);
    }


}


[System.Serializable]
public class RemapFinder
{
    [SerializeField]
    private float f1;
    [SerializeField]
    private float f2;
    [SerializeField]
    private float f3;
    [SerializeField]
    private float f4;
    [SerializeField]
    private float f5;
    [SerializeField]
    private float f6;
    [SerializeField]
    private float f7;
    [SerializeField]
    private float f8;
    [SerializeField]
    private float f9;
    [SerializeField]
    private float f10;
    [SerializeField]
    private float f11;
    [SerializeField]
    private float f12;
    [SerializeField]
    private float f13;
    [SerializeField]
    private float f14;
    [SerializeField]
    private float f15;
    [SerializeField]
    private float f16;
    [SerializeField]
    private float f17;
    [SerializeField]
    private float f18;
    [SerializeField]
    private float f19;
    [SerializeField]
    private float f20;
    [SerializeField]
    private float f21;
    [SerializeField]
    private float f22;
    [SerializeField]
    private float f23;
    [SerializeField]
    private float f24;

    private bool Log = false;

    public void Remap(float value, float min1, float max1, float min2, float max2)
    {
        f1 = Extensions.Remap(value, min1, min2, max1, max2);
        f2 = Extensions.Remap(value, min1, min2, max2, max1);
        f3 = Extensions.Remap(value, min1, max1, min2, max2);
        f4 = Extensions.Remap(value, min1, max2, min2, max2);
        f5 = Extensions.Remap(value, min1, max1, max2, min2);
        f6 = Extensions.Remap(value, min1, max2, max2, min2);
        f7 = Extensions.Remap(value, min2, min1, max1, max2);
        f8 = Extensions.Remap(value, min2, min1, max2, max1);
        f9 = Extensions.Remap(value, min2, max1, min1, max2);
        f10 = Extensions.Remap(value, min2, max1, max2, min1);
        f11 = Extensions.Remap(value, min2, max2, min1, max1);
        f12 = Extensions.Remap(value, min2, max2, max1, min1);
        f13 = Extensions.Remap(value, max1, min1, min2, max2);
        f14 = Extensions.Remap(value, max1, min1, max2, min2);
        f15 = Extensions.Remap(value, max1, min2, min1, max2);
        f16 = Extensions.Remap(value, max1, min2, max2, min1);
        f17 = Extensions.Remap(value, max1, max2, min1, min2);
        f18 = Extensions.Remap(value, max1, max2, min2, min1);
        f19 = Extensions.Remap(value, max2, min1, min2, max1);
        f20 = Extensions.Remap(value, max2, min1, max1, min2);
        f21 = Extensions.Remap(value, max2, min2, min1, max1);
        f22 = Extensions.Remap(value, max2, min2, max1, min1);
        f23 = Extensions.Remap(value, max2, max1, min1, min2);
        f24 = Extensions.Remap(value, max2, max1, min2, min1);
        if (Log)
            Debug.Log(
                f1.ToString() +
                f2.ToString() +
                f3.ToString() +
                f4.ToString() +
                f5.ToString() +
                f6.ToString() +
                f7.ToString() +
                f8.ToString() +
                f9.ToString() +
                f10.ToString() +
                f11.ToString() +
                f12.ToString() +
                f13.ToString() +
                f14.ToString() +
                f15.ToString() +
                f16.ToString() +
                f17.ToString() +
                f18.ToString() +
                f19.ToString() +
                f20.ToString() +
                f21.ToString() +
                f22.ToString() +
                f23.ToString() +
                f24.ToString());
    }
}