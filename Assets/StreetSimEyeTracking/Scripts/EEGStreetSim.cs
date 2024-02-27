using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class EEGStreetSim : MonoBehaviour
{

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
    public Camera leftCamera, rightCamera;
    public Transform centerAnchor;
    public Transform topleftAnchor, toprightAnchor, bottomleftAnchor;

    //public CSVWriter myWriter;
    /*
    public Transform topleftAnchor_10m, toprightAnchor_10m, bottomleftAnchor_10m;
    public Transform topleftAnchor_50m, toprightAnchor_50m, bottomleftAnchor_50m;
    */
    public CombinedEyeTracker combinedEyeTracker;
    public LayerMask positionRaycastLayerMask;
    public InstructionsUI textboxUI;

    [Header("Experiment Settings")]
    [SerializeField] private int numTrials = 0;
    public bool deactivateBackground = true;
    public bool deactivateAnchors = true;
    /*
    public bool deactivate10mAnchors = true;
    public bool deactivate50mAnchors = true;
    */
    
    [Header("Trial Metrics")]
    [SerializeField] private string filePath;
    [SerializeField] private long startTime;
    [SerializeField] private float dt = 0.1f;
    private StreamWriter eventWriter;
    private IEnumerator startCoroutine = null;
    private IEnumerator eventCoroutine = null;

    void OnEnable() {
        string fname = System.DateTime.Now.ToString("HH-mm-ss") + ".csv";
        filePath = Path.Combine(Application.persistentDataPath, fname);
        startTime = GetUnixTime();
    }

    void Awake() {
        ESS = this;
        //myWriter.Initialize();
    }

    // Start is called before the first frame update
    void Start() {
        eventWriter = new StreamWriter(new FileStream(filePath, FileMode.Create), Encoding.UTF8);
        // Header Line
        eventWriter.WriteLine("unix_ms,event_type,title,description,x,y,z");
        // First Entry: Start
        eventWriter.WriteLine(EventLine(startTime,"Simulation", Vector3.zero, "Simulation Start"));
        //MyWriterLine(startTime, Vector3.zero, "Simulation", "Simulation Start");

        // Start the event coroutine
        startCoroutine = InitializeCoroutine();
        StartCoroutine(startCoroutine);
    }

    private IEnumerator InitializeCoroutine() {
        // Add the center, topleft, topright, and bottomleft anchor positions relative to the screen
        Vector3 left_center = leftCamera.WorldToScreenPoint(centerAnchor.position);
        Vector3 left_topleft = leftCamera.WorldToScreenPoint(topleftAnchor.position);
        Vector3 left_topright = leftCamera.WorldToScreenPoint(toprightAnchor.position);
        Vector3 left_bottomleft = leftCamera.WorldToScreenPoint(bottomleftAnchor.position);
        Vector3 right_center = rightCamera.WorldToScreenPoint(centerAnchor.position);
        Vector3 right_topleft = rightCamera.WorldToScreenPoint(topleftAnchor.position);
        Vector3 right_topright = rightCamera.WorldToScreenPoint(toprightAnchor.position);
        Vector3 right_bottomleft = rightCamera.WorldToScreenPoint(bottomleftAnchor.position);

        WriteLine("Anchor",left_center,"Left","Center");
        WriteLine("Anchor",left_topleft,"Left","Top Left");
        WriteLine("Anchor",left_topright,"Left","Top Right");
        WriteLine("Anchor",left_bottomleft,"Left","Bottom Left");
        WriteLine("Anchor",right_center,"Right","Center");
        WriteLine("Anchor",right_topleft,"Right","Top Left");
        WriteLine("Anchor",right_topright,"Right","Top Right");
        WriteLine("Anchor",right_bottomleft,"Right","Bottom Left");

        /*
        long sTime = GetUnixTime();
        MyWriterLine(sTime, left_center, "Anchor", "Left", "Center");
        MyWriterLine(sTime, left_topleft, "Anchor", "Left", "Top Left");
        MyWriterLine(sTime, left_topright, "Anchor", "Left", "Top Right");
        MyWriterLine(sTime, left_bottomleft, "Anchor", "Left", "Bottom Left");
        MyWriterLine(sTime, right_center, "Anchor", "Right", "Center");
        MyWriterLine(sTime, right_topleft, "Anchor", "Right", "Top Left");
        MyWriterLine(sTime, right_topright, "Anchor", "Right", "Top Right");
        MyWriterLine(sTime, right_bottomleft, "Anchor", "Right", "Bottom Left");
        */

        yield return new WaitForSeconds(3);

        // We'll iterate through all anchors
        centerAnchor.gameObject.SetActive(true);
        topleftAnchor.gameObject.SetActive(false);
        toprightAnchor.gameObject.SetActive(false);
        bottomleftAnchor.gameObject.SetActive(false);
        yield return new WaitForSeconds(3);

        centerAnchor.gameObject.SetActive(false);
        topleftAnchor.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);

        topleftAnchor.gameObject.SetActive(false);
        toprightAnchor.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);

        toprightAnchor.gameObject.SetActive(false);
        bottomleftAnchor.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);

        centerAnchor.gameObject.SetActive(true);
        topleftAnchor.gameObject.SetActive(true);
        toprightAnchor.gameObject.SetActive(true);
        bottomleftAnchor.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);

        // Make sure all cameras have their colors reset.
        if (deactivateBackground) {
            leftCamera.backgroundColor = new Color(0f,0f,0f,0f);
            rightCamera.backgroundColor = new Color(0f,0f,0f,0f);
        }
        // Deactivate the 10m anchors
        if (deactivateAnchors) {
            centerAnchor.gameObject.SetActive(false);
            topleftAnchor.gameObject.SetActive(false);
            toprightAnchor.gameObject.SetActive(false);
            bottomleftAnchor.gameObject.SetActive(false);
        }
        /*
        if (deactivate10mAnchors) {
            centerAnchor.gameObject.SetActive(false);
            topleftAnchor_10m.gameObject.SetActive(false);
            toprightAnchor_10m.gameObject.SetActive(false);
            bottomleftAnchor_10m.gameObject.SetActive(false);
        }
        if (deactivate50mAnchors) {
            topleftAnchor_50m.gameObject.SetActive(false);
            toprightAnchor_50m.gameObject.SetActive(false);
            bottomleftAnchor_50m.gameObject.SetActive(false);
        }
        */
        eventCoroutine = EventCoroutine();
        StartCoroutine(eventCoroutine);
    }

    private IEnumerator EventCoroutine() {
        startCoroutine = null;
        // Initialize with the first trial
        NextTrial();
        while(true) {
            // Calculate the current time
            long currentTime = GetUnixTime();
            // Print the text to our textboxUI if it exists
            if (textboxUI != null) textboxUI.SetText(currentTime.ToString());
            // Check what's underneath the player currently
            // Create a record for the player's current position
            eventWriter.WriteLine(EventLine(currentTime,"Player",xrCamera.position,"position"));
            // Create a record for the player's current orientation
            eventWriter.WriteLine(EventLine(currentTime,"Player",xrCamera.rotation,"orientation"));
            // Create a record for each eye, as well as the combined eye
            Vector3 rtp = Vector3.zero;
            Vector3 rtd = Vector3.zero;
            if (leftEyeTracker != null) {
                // Get the position of the eye tracking RELATIVE to the forward position of the camera
                rtp = xrCamera.InverseTransformPoint(leftEyeTracker.rayTargetPosition);
                // Get the relative direction
                rtd = rtp.normalized;
                // Record the direction
                eventWriter.WriteLine(EventLine(currentTime,"Eye Tracking", rtd, "Left", "Direction"));
            }
            if (rightEyeTracker != null) {
                // Get the position of the eye tracking RELATIVE to the forward position of the camera
                rtp = xrCamera.InverseTransformPoint(rightEyeTracker.rayTargetPosition);
                // Get the relative direction
                rtd = rtp.normalized;
                // Record the direction
                eventWriter.WriteLine(EventLine(currentTime,"Eye Tracking", rtd, "Right", "Direction"));
            }
            if (combinedEyeTracker != null) {
                // Get the position of the eye tracking RELATIVE to the forward position of the camera
                rtp = xrCamera.InverseTransformPoint(combinedEyeTracker.rayTargetPosition);
                // Get the relative direction
                rtd = rtp.normalized;
                // Record the direction
                eventWriter.WriteLine(EventLine(currentTime,"Eye Tracking", rtd, "Center", "Direction"));
                eventWriter.WriteLine(EventLine(
                    currentTime,
                    "Eye Tracking",
                    leftCamera.WorldToScreenPoint(combinedEyeTracker.rayTargetPosition),
                    "Left",
                    "Screen Position"
                ));
                eventWriter.WriteLine(EventLine(
                    currentTime,
                    "Eye Tracking",
                    rightCamera.WorldToScreenPoint(combinedEyeTracker.rayTargetPosition),
                    "Right",
                    "Screen Position"
                ));
            }
            // Yield return for the next event
            yield return new WaitForSeconds(dt);
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

    public void NextTrial() {
        numTrials += 1;
        WriteLine("Simulation",Vector3.zero,$"Trial {numTrials} Start");
    }

    /*
    public void MyWriterLine(long unix_ms, Vector3 xyz, string event_type="", string title="", string description="") {
        myWriter.AddPayload(unix_ms);
        myWriter.AddPayload(event_type);
        myWriter.AddPayload(title);
        myWriter.AddPayload(description);
        myWriter.AddPayload(xyz);
        myWriter.WriteLine();
    }
    */

    void OnDisable() {
        //myWriter.Disable();

        // Write the final line
        long endTime = GetUnixTime();
        eventWriter.WriteLine(EventLine(endTime, "Simulation", Vector3.zero, "Simulation End"));
        // Close and flush the writer
        eventWriter.Flush();
        eventWriter.Close();
        // End the coroutines
        if (startCoroutine != null) 
            StopCoroutine(startCoroutine);
        if (eventCoroutine != null) 
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
