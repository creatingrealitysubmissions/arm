using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmController : MonoBehaviour {

    bool isInIntro = true;
    int activeIntroScreenIndex = 0;
    public GameObject[] introScreens = new GameObject[4];
    public GameObject introBackdrop;

    public GameObject elbowTarget;
    public GameObject wristTarget;

    public GameObject elbowIKTarget;
    public GameObject wristIKTarget;

    public Renderer[] armRenderers = new Renderer[3];
    public int activeArmIndex = 0;

    Renderer myElbowRenderer;
    Renderer myWristRenderer;

    Vector3 actualElbowPosition;
    Vector3 actualWristPosition;
    Vector3 smoothedElbowPosition = Vector3.zero;
    Vector3 smoothedWristPosition = Vector3.zero;
    float magicSmoothValue = 20.0f;

    GameObject armMesh;

    GameObject cylinder;
    float width = 0.02f;
    public GameObject cylinderPrefab; //assumed to be 1m x 1m x 2m default unity cylinder to make calculations easy
    public GameObject cylinderParent;

    // Use this for initialization
    void Start () {
        myElbowRenderer = elbowTarget.GetComponent<Renderer>();
        myWristRenderer = wristTarget.GetComponent<Renderer>();

        if (!armMesh) {
            armMesh = GameObject.FindWithTag("ArmArmature");

            if (armMesh == null) {
                Debug.Log("No armMesh gameObject found. Did you forget to tag your gameObject?");
            }
        }

        // At launch, disable all armRenderers except the first:
        for (int i = 0; i < armRenderers.Length; i++) {
            if (i != activeArmIndex) { 
                armRenderers[i].enabled = false;
            }
        }

        for (int i = 0; i < introScreens.Length; i++) {
            if (i != activeIntroScreenIndex) { 
                introScreens[i].SetActive(false);
            }
        }

    }
    
    // Update is called once per frame
    void Update () {

        if (!isInIntro) {
            for (int i = 0; i < Input.touchCount; ++i) {
                if (Input.GetTouch(i).phase == TouchPhase.Ended) {
                    Debug.Log("TouchPhase.ended detected");
                    SwitchArms(activeArmIndex);
                }
            }

            if (Input.GetMouseButtonUp(0)) {
                Debug.Log("mouse button pressed");
                SwitchArms(activeArmIndex);
            }
        } else {
            if (Input.GetMouseButtonUp(0)) {
                Debug.Log("Intro navigated");
                SwitchIntroScreen(activeIntroScreenIndex);
            }            
        }

        // Manage Vuforia jitter through lerping:
        actualElbowPosition = elbowTarget.transform.position;
        actualWristPosition = wristTarget.transform.position;

        // naive/basic lerp:
        // smoothedElbowPosition = Vector3.Lerp (smoothedElbowPosition, actualElbowPosition, magicSmoothValue * Time.deltaTime);
        // smoothedWristPosition = Vector3.Lerp (smoothedWristPosition, actualWristPosition, magicSmoothValue * Time.deltaTime);

        // Debug.Log("Initial smoothed elbow pos: " + smoothedElbowPosition);
        // Debug.Log("Actual elbow pos: " + actualElbowPosition);

        // `SuperSmoothLerp` is a more sophisticated approach via comtinuous integration. 
        // Could alsp try `Vector3.SmoothDamp`?:
        smoothedElbowPosition = SuperSmoothLerp(smoothedElbowPosition, actualElbowPosition, elbowTarget.transform.position, Time.deltaTime, magicSmoothValue);
        // Debug.Log("Updated smoothed elbow pos: " + smoothedElbowPosition);
        smoothedWristPosition = SuperSmoothLerp(smoothedWristPosition, actualWristPosition, wristTarget.transform.position, Time.deltaTime, magicSmoothValue);

        elbowIKTarget.transform.position = smoothedElbowPosition;
        wristIKTarget.transform.position = smoothedWristPosition;

        elbowIKTarget.transform.rotation = Quaternion.Lerp(elbowIKTarget.transform.rotation, elbowTarget.transform.rotation, Time.deltaTime * magicSmoothValue);
        wristIKTarget.transform.rotation = Quaternion.Lerp(wristIKTarget.transform.rotation, wristTarget.transform.rotation, Time.deltaTime * magicSmoothValue);

        if (!cylinder) {
            Debug.Log("Making cylinder!");
            CreateCylinderBetweenPoints(smoothedElbowPosition, smoothedWristPosition, width);
        } else if (myElbowRenderer && myElbowRenderer.enabled && myWristRenderer && myWristRenderer.enabled) {
            UpdateCylinderBetweenPoints(smoothedElbowPosition, smoothedWristPosition, width);
        }
    }

    void SwitchIntroScreen(int screenIndex) {
        var newScreenIndex = screenIndex + 1;

        if (newScreenIndex > introScreens.Length - 1) {
            for (int i = 0; i < introScreens.Length; i++) {introScreens[i].SetActive(false);}
            introBackdrop.SetActive(false);
            isInIntro = false;
            activeIntroScreenIndex = 0;
            return;
        }

        for (int i = 0; i < introScreens.Length; i++) {
            if (i != newScreenIndex) {
                introScreens[i].SetActive(false);
            }
        }

        introScreens[newScreenIndex].SetActive(true);

        activeIntroScreenIndex = newScreenIndex;
    }

    void SwitchArms(int armIndex) {
        var newArmIndex = armIndex == (armRenderers.Length - 1) ? 0 : armIndex + 1;

        for (int i = 0; i < armRenderers.Length; i++) {
            if (i != newArmIndex) { 
                armRenderers[i].enabled = false;
            }
        }

        armRenderers[newArmIndex].enabled = true;

        activeArmIndex = newArmIndex;
    }

    void CreateCylinderBetweenPoints(Vector3 start, Vector3 end, float width) {
        Vector3 offset = end - start;
        Vector3 scale = new Vector3 (width, offset.magnitude / 2.0f, width);
        Vector3 position = start + (offset / 2.0f);

        cylinder = Instantiate(cylinderPrefab, position, Quaternion.identity);
        cylinder.transform.parent = cylinderParent.transform;
        cylinder.transform.up = offset;
        cylinder.transform.localScale = scale;
    }
    
    void UpdateCylinderBetweenPoints(Vector3 start, Vector3 end, float width) {
        Debug.Log("updating cylinder position!");
        var offset = end - start;
        var scale = new Vector3(width, offset.magnitude / 2.0f, width);
        var position = start + (offset / 2.0f);

        if (armMesh == null) {
            Debug.Log("no armMesh gameObject found. Did you forget to tag your gameObject?");
        } else {
            armMesh.transform.position = start;
        }

        cylinder.transform.up = offset;
        cylinder.transform.position = position;
        cylinder.transform.localScale = scale;
    }

    Vector3 SuperSmoothLerp(Vector3 x0, Vector3 y0, Vector3 yt, float t, float k) {
        Vector3 f = x0 - y0 + (yt - y0) / (k * t);
        return yt - (yt - y0) / (k*t) + f * Mathf.Exp(-k*t);
    }
}
