using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum dir {POSX, NEGX, POSY, NEGY, POSZ, NEGZ};
public enum axis {NONE, HORIZONTAL, VERTICAL};

public class LookAt : MonoBehaviour {

    public Transform target;
    public Vector3 rotationCorrection = new Vector3();

    public dir front = dir.POSZ;
    public dir up = dir.POSY;

    public axis locktoaxis = axis.NONE;

    public bool limHorizontal = false;
    public Vector2 limitHorizontal = new Vector2(-20,20);
    public bool limVertical = false;
    public Vector2 limitVertical = new Vector2(-20,20);

    [Range(0.0F, 1.0F)]
    public float influence = 1;

    public bool Roll = false;

    // Private
    private Quaternion lookAtRotation = new Quaternion();
    private Quaternion neutralQuat = new Quaternion();

    private Vector3 tempVec = new Vector3 ();
    private Quaternion tempQuat = new Quaternion ();


    private Vector3 tempPos = new Vector3();
    private Vector3 tempTargetPos = new Vector3();
    private Vector3 tempUp = new Vector3();

    void Start(){

        neutralQuat = Quaternion.identity;
    }

    void Update()
    {

        // Define vector up
        switch(up){
        case dir.NEGX: tempVec.Set (-1.0f, 0.0f, 0.0f); break;
        case dir.POSX: tempVec.Set (1.0f, 0.0f, 0.0f); break;
        case dir.NEGZ: tempVec.Set (0.0f, 0.0f, -1.0f); break;
        case dir.POSZ: tempVec.Set (0.0f, 0.0f, 1.0f); break;
        case dir.NEGY: tempVec.Set (0.0f, -1.0f, 0.0f); break;
        case dir.POSY: goto default;
        default: tempVec.Set (0.0f, 1.0f, 0.0f); break;
        }


        // Transform to local coordinates
        if (transform.parent)
        {
            var gm = transform.parent.worldToLocalMatrix;
            //gm = gm.inverse;?
            tempPos = gm.MultiplyPoint(transform.position);
            tempTargetPos = gm.MultiplyPoint(target.transform.position);
            tempUp = gm.MultiplyVector(tempVec); // up;
        }
        else {
            tempPos = transform.position;
            tempTargetPos = target.transform.position;
            tempUp = tempVec;
        }
        
        // LookAt rotation
        lookAtRotation = Quaternion.LookRotation(tempTargetPos - tempPos, tempUp);


        // Rotate according to vector front (forward)
        switch (front)
        {
            case dir.POSY: tempVec.Set(90.0f, 0.0f, 0.0f); break;
            case dir.NEGY: tempVec.Set(-90.0f, 0.0f, 0.0f); break;
            case dir.POSX: tempVec.Set(0.0f, -90.0f, 0.0f); break;
            case dir.NEGX: tempVec.Set(0.0f, 90.0f, 0.0f); break;
            case dir.POSZ: tempVec.Set(0.0f, 0.0f, 0.0f); break;
            case dir.NEGZ: tempVec.Set(0.0f, 180.0f, 0.0f); break;
            default: break;
        }
        tempQuat = Quaternion.Euler(tempVec);
        // Rotation front
        lookAtRotation *= tempQuat;

        // Influence
        lookAtRotation = Quaternion.Slerp(neutralQuat, lookAtRotation, influence);


        // Limit Horizontal Axis
        if (limHorizontal){
            
            float horAngle = lookAtRotation.eulerAngles.y;

            if (horAngle > 180) horAngle -= 360;
            if (horAngle < -180) horAngle += 360;

            tempVec = lookAtRotation.eulerAngles;

            if (horAngle > limitHorizontal.y) {
                tempVec.y = limitHorizontal.y;
            }
            else if (horAngle < limitHorizontal.x) {
                tempVec.y = limitHorizontal.x;
            }

            lookAtRotation.eulerAngles = tempVec;
        }

        // Limit Vertical Axis
        if (limVertical)
        {
            float verAngle = lookAtRotation.eulerAngles.x;
            if (verAngle > 180) verAngle -= 360;
            if (verAngle < -180) verAngle += 360;

            tempVec = lookAtRotation.eulerAngles;

            if (verAngle > limitVertical.y)
            {
                tempVec.x = limitVertical.y;
     
            }
            else if (verAngle < limitVertical.x)
            {
                tempVec.x = limitVertical.x;
            }

            lookAtRotation.eulerAngles = tempVec;
        }


        // Lock to axis
        if (locktoaxis == axis.HORIZONTAL){
            tempVec.Set(0.0f, lookAtRotation.eulerAngles.y, 0.0f);
            lookAtRotation.eulerAngles = tempVec;
        }
        else if (locktoaxis == axis.VERTICAL) {
            tempVec.Set(lookAtRotation.eulerAngles.x, 0.0f, 0.0f);
            lookAtRotation.eulerAngles = tempVec;
        }
        // Remove Z rotation or Roll
        if (!Roll)
        {
            tempVec.Set(lookAtRotation.eulerAngles.x, lookAtRotation.eulerAngles.y, 0.0f);
            lookAtRotation.eulerAngles = tempVec;
        }


        // Correction
        tempQuat = lookAtRotation * Quaternion.Euler(rotationCorrection);

        // Apply rotation
        transform.localRotation = tempQuat;


    }

}
