using  System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Imagine.WebAR{

    [System.Serializable]
    public class CurvedTargetInfo
    {
        public string id;
        public Texture2D texture;
        [Range(1,360)] public float arc = 120;
        public float radMul = 1;
        public float height = 1;
    }

    [System.Serializable]
    public class TrackerSettings
    {
        [Tooltip("Allows tracking multiple image targets simultaneously (Experimental)")]
        [SerializeField] public int maxSimultaneousTargets = 1;

        //public enum TrackingQuality { SPEED_OVER_QUALITY, BALANCED, QUALITY_OVER_SPEED };
        //[SerializeField] public TrackingQuality trackingQuality = TrackingQuality.BALANCED;


        public enum FrameRate { FR_30_FPS = 30, FR_60FPS = 60};

        [SerializeField] public FrameRate targetFrameRate = FrameRate.FR_30_FPS;

        [SerializeField] public AdvancedSettings advancedSettings;
        

        [Tooltip("If enabled, you can display imageTarget feature points by pressing 'I' in desktop browser")]
        [Space][SerializeField] public bool debugMode = false;


        public string Serialize() {
            var json = "{";
            json += "\"MAX_SIMULTANEOUS_TRACK\":" + maxSimultaneousTargets + ",";

            //json += "\"QOS\":" + (int)trackingQuality + ",";

            json += "\"FRAMERATE\":" + (int)targetFrameRate + ",";

            json += "\"MAX_AREA\":" + Mathf.RoundToInt(advancedSettings.maxFrameArea * 1000) + ",";
            
            json += "\"MAX_PIXELS\":" + Mathf.RoundToInt(advancedSettings.maxFrameLength) + ",";

            json += "\"TRACK_TARGET_MATCH_COUNT\":" + advancedSettings.trackedPoints + ",";

            json += "\"DETECT_INTERVAL\":" + advancedSettings.detectInterval;

           
            json += "}";

            return json;

        }
    }

    [System.Serializable]
    public class AdvancedSettings
    {
        [Tooltip("Higher values will increase accuracy, but decreases frame rate")]
        [SerializeField][Range(300, 600)]
        public int maxFrameLength = 500;

        [Tooltip("Higher values will make the image easily detected, but induces a short lag/delay")]
        [SerializeField] [Range(24, 80)] public float maxFrameArea = 40;

        [Tooltip("Higher values will improve stability, but decreases frame rate")]
        [SerializeField] [Range(16, 80)] public int trackedPoints = 25;

        [Tooltip("Lower intervals will speed up detection, especially on multiple targets, but significantly decreases frame rate. Value in millisecods")]
        [SerializeField] [Range(0, 1000)] public int detectInterval = 200;

    }

    //[CreateAssetMenu(menuName = "Imagine WebAR/CurveTracker Global Settings", order = 1300)]
    public class CT_GlobalSettings : ScriptableObject
    {
        [SerializeField] public List<CurvedTargetInfo> curvedTargetInfos;

        [SerializeField] public TrackerSettings defaultTrackerSettings;
        
        
        private static CT_GlobalSettings _instance;
        public static CT_GlobalSettings Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Resources.Load<CT_GlobalSettings>("CT_GlobalSettings");
                }
                return _instance;

            }
        }
    }

    public static class FloatExtensions
    {
        //this is needed to properly convert floating point strings for some languages to JSON
        public static string ToStringInvariantCulture(this float f)
        {
            return f.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

}

