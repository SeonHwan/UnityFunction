using System;

using UnityEngine;

public class CameraMoveLimiterDebugger : MonoBehaviour
{
    bool _isSet = false;

    Vector3 mCamPos = Vector3.zero;
    Quaternion mCamRot = Quaternion.identity;
    float mFov = 0f;
    float mNear = 0f;
    float mFar = 0f;
    float mAspect = 0f;

    public void Set(CameraMoveLimiter limiter)
    {
        mCamPos = limiter.TargetCam.transform.position;
        mCamRot = limiter.TargetCam.transform.rotation;

        mFar = limiter.DepthVolume;

        mNear = limiter.Planes[4].GetDistanceToPoint(mCamPos) * -1f;

        Quaternion _normalRotate = Quaternion.FromToRotation(Vector3.up, limiter.Planes[2].normal);
        Vector3 _downForwardNormal = _normalRotate * Vector3.forward;

        _normalRotate = Quaternion.FromToRotation(Vector3.down, limiter.Planes[3].normal);
        Vector3 _upForwardNormal = _normalRotate * Vector3.forward;
        mFov = GetAngle(_downForwardNormal, _upForwardNormal);

        float _width = Vector3.Distance(
            limiter.TargetCam.ViewportToWorldPoint(new Vector3(0f, 1f, mFar))
            , limiter.TargetCam.ViewportToWorldPoint(new Vector3(1f, 1f, mFar)));

        float _height = Vector3.Distance(
            limiter.TargetCam.ViewportToWorldPoint(new Vector3(1f, 1f, mFar))
            , limiter.TargetCam.ViewportToWorldPoint(new Vector3(1f, 0f, mFar)));

        mAspect = _width / _height;

        // for(int i = 0; i < limiter.Planes.Length; ++i)
        // {
        //     Plane _plane = limiter.Planes[i];
        //     if(_plane.Equals(default(Plane)))
        //         continue;

        //     GameObject p = GameObject.CreatePrimitive(PrimitiveType.Plane);
        //     p.name = "Plane " + i.ToString();
        //     p.transform.rotation = Quaternion.FromToRotation(Vector3.up, _plane.normal);
        //     p.transform.position = -_plane.normal * _plane.distance;
        //     p.transform.localScale = new Vector3(10, 10, 10);
        // }

        _isSet = true;
    }

    float GetAngle(Vector3 v1, Vector3 v2)
    {
        float theta = Vector3.Dot(v1, v2) / (v1.magnitude * v2.magnitude);
        Vector3 dirAngle = Vector3.Cross(v1, v2);
        float angle = Mathf.Acos(theta) * Mathf.Rad2Deg;
        if (dirAngle.z < 0.0f) 
            angle = 360 - angle;
        
        angle = (float)Math.Round(angle, 2);
        return angle;
    }

    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    void OnDrawGizmos()
    {
        if(_isSet)
        {
            Gizmos.color = Color.white;
            Gizmos.matrix = Matrix4x4.TRS(mCamPos, mCamRot, Vector3.one);
            Gizmos.DrawFrustum(Vector3.zero, mFov, mFar, mNear, mAspect);
        }
    }
}
