using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmController : MonoBehaviour {

    public GameObject myElbowTarget;
    public GameObject myWristTarget;
    public Renderer myElbowRenderer;
    public Renderer myWristRenderer;

    public GameObject armMesh;
    // public GameObject myElbowBone;
    // public GameObject myWristBone;

    float width = 0.05f;
    public GameObject cylinder;
    public GameObject cylinderPrefab; //assumed to be 1m x 1m x 2m default unity cylinder to make calculations easy
    public GameObject cylinderParent;

    // Use this for initialization
    void Start () {
        myElbowRenderer = myElbowTarget.GetComponent<Renderer>();
        myWristRenderer = myWristTarget.GetComponent<Renderer>();
    }
    
    // Update is called once per frame
    void Update () {
        if (myWristRenderer && myWristRenderer.enabled) {
            Debug.Log("wrist visible!");
        }
        
        if (myElbowRenderer && myElbowRenderer.enabled) {
            Debug.Log("elbow visible!");
        }

        if (!cylinder) {
            Debug.Log("Making cylinder!");
            CreateCylinderBetweenPoints(myElbowTarget.transform.position, myWristTarget.transform.position, width);
        } else if (myElbowRenderer && myElbowRenderer.enabled && myWristRenderer && myWristRenderer.enabled) {
            UpdateCylinderBetweenPoints(myElbowTarget.transform.position, myWristTarget.transform.position, width);
        }
    }

    void CreateCylinderBetweenPoints(Vector3 start, Vector3 end, float width) {
        Vector3 offset = end - start;
        Vector3 scale = new Vector3 (width, offset.magnitude / 2.0f, width);
        Vector3 position = start + (offset / 2.0f);

        // myElbowBone.transform.position = start;
        // myWristBone.transform.position = end;

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

        // myElbowBone.transform.position = start;
        // myWristBone.transform.position = end;

        armMesh.transform.position = myElbowTarget.transform.position;

        cylinder.transform.up = offset;
        cylinder.transform.position = position;
        cylinder.transform.localScale = scale;
    }

}
