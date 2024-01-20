using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class EEGStreetSim : MonoBehaviour
{
    [System.Serializable]
    public class SetCarManager {
        public StreetSimCarManager.CarManagerStatus setCarsTo;
        public int numCrossings;
    }

    public class StreetSimEvent {
        public long unix_ts;
        public string event_type;
        public string title;
        public string description;
        public float x, y, z;
    }

    public static EEGStreetSim ESS;

    [Header("References")]
    public Transform xrCamera;
    public EyeTrackingRay leftEyeTracker, rightEyeTracker;
    public CombinedEyeTracker combinedEyeTracker;
    public LayerMask positionRaycastLayerMask;
    public InstructionsUI textboxUI;

    [Header("Experiment Settings")]
    public string name;
    public List<SetCarManager> trialCarEscalation = new List<SetCarManager>();
    [SerializeField] private string belowTargetName = "Unknown";
    [SerializeField] private string currentSide = "Unknown";
    [SerializeField] private int numSuccessfulTrials = 0;
    [SerializeField] private int carEscalationIndex = 0;

    [Header("Trial Metrics")]
    [SerializeField] private string filePath;
    [SerializeField] private long startTime;
    private StreamWriter eventWriter;
    private IEnumerator eventCoroutine = null;

    void OnEnable() {
        string fname = name + "-" + System.DateTime.Now.ToString("HH-mm-ss") + ".csv";
        filePath = Path.Combine(Application.persistentDataPath, fname);
        startTime = GetUnixTime();
    }

    void Awake() {
        ESS = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        eventWriter = new StreamWriter(new FileStream(filePath, FileMode.Create), Encoding.UTF8);
        // Header Line
        eventWriter.WriteLine("unix_ms,event_type,title,description,x,y,z");
        // First Entry: Start
        eventWriter.WriteLine(EventLine(startTime,"Simulation", Vector3.zero, $"Trial {numSuccessfulTrials+1} Start"));
        // Start the event coroutine
        eventCoroutine = EventCoroutine();
        StartCoroutine(eventCoroutine);
    }

    private IEnumerator EventCoroutine() {
        while(true) {
            // Calculate the current time
            long currentTime = GetUnixTime();
            // Print the text to our textboxUI if it exists
            if (textboxUI != null) textboxUI.SetText(currentTime.ToString());
            // Check what's underneath the player currently
            RaycastHit hit;
            if (Physics.Raycast(xrCamera.position, -Vector3.up, out hit, 5f, positionRaycastLayerMask)) {
                belowTargetName = hit.transform.gameObject.name;
                if (belowTargetName == "SouthSidewalk" || belowTargetName == "NorthSidewalk") {
                    if (currentSide != "Unknown" && currentSide != belowTargetName) {
                        // At this point, we've crossed successfully. We'll add to our count of successful trials and modify our congestion if necessary
                        numSuccessfulTrials += 1;
                        eventWriter.WriteLine(EventLine(currentTime,"Simulation", Vector3.zero, $"Trial {numSuccessfulTrials+1} Start"));
                        if (carEscalationIndex < trialCarEscalation.Count && numSuccessfulTrials == trialCarEscalation[carEscalationIndex].numCrossings) {
                            StreetSimCarManager.CM.SetCongestionStatus(trialCarEscalation[carEscalationIndex].setCarsTo);
                            carEscalationIndex += 1;
                            TrafficSignalController.current.StartAtSessionIndex(0);
                        }
                    }
                    currentSide = belowTargetName;
                }
            } else {
                belowTargetName = "-";
            }
            // Create a record for the player's current position
            eventWriter.WriteLine(EventLine(currentTime,"Player",xrCamera.position,"position",belowTargetName));
            // Create a record for the player's current orientation
            eventWriter.WriteLine(EventLine(currentTime,"Player",xrCamera.rotation,"orientation"));
            // Create a record for each eye, as well as the combined eye
            if (leftEyeTracker != null) {
                eventWriter.WriteLine(EventLine(currentTime,"Global Eye Tracking", leftEyeTracker.rayTargetPosition, "Left", leftEyeTracker.rayTargetName));
            }
            if (rightEyeTracker != null) {
                eventWriter.WriteLine(EventLine(currentTime,"Global Eye Tracking", rightEyeTracker.rayTargetPosition, "Right", rightEyeTracker.rayTargetName));
            }
            if (combinedEyeTracker != null && combinedEyeTracker.rayHit) {
                eventWriter.WriteLine(EventLine(currentTime,"Combined Eye Tracking", combinedEyeTracker.rayTargetPosition, "Center", combinedEyeTracker.rayTargetName));
            }
            // Yield return for the next event
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void WriteLine(string event_type, Vector3 xyz, string title="", string description="") {
        // Only continue if the event writer is not null
        if (eventWriter == null) return;
        // Calculate the current time
        long currentTime = GetUnixTime();
        // Write to the event writer
        eventWriter.WriteLine(EventLine(currentTime, event_type, xyz, title, description));
    }

    void OnDisable() {
        // Write the final line
        long endTime = GetUnixTime();
        eventWriter.WriteLine(EventLine(endTime, "Simulation", Vector3.zero, "Simulation End"));
        // Close and flush the writer
        eventWriter.Flush();
        eventWriter.Close();
        // End the coroutine
        StopCoroutine(eventCoroutine);
    }

    public static long GetUnixTime() {
        DateTime currentTime = DateTime.UtcNow;
        return ((DateTimeOffset)currentTime).ToUnixTimeMilliseconds();
    }

    public static string EventLine(long unix_ts, string event_type, Vector3 xyz, string title="", string description="") {
        return $"{unix_ts},{event_type},{title},{description},{xyz.x},{xyz.y},{xyz.z}";
    }

    public static string EventLine(long unix_ts, string event_type, Quaternion q, string title="", string description="") {
        Vector3 xyz = q.eulerAngles;
        return $"{unix_ts},{event_type},{title},{description},{xyz.x},{xyz.y},{xyz.z}";
    }
}
