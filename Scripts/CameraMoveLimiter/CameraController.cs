using System;

using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject sphere;
    public CameraMoveLimiterDebugger debugger;
    Camera cam = null;

    CameraMoveLimiter mLimiter = null;

    private static float Dist = 100f;
    private float Near = 0f;

    float mFov = 0f;
    float mAspect = 0f;

    enum MoveAxis
    {
        None,
        X,
        Y,
        Z,
    }

    // Start is called before the first frame update
    void Start()
    {
        mLimiter = new CameraMoveLimiter(Camera.main, LimitShape.Square_pyramid, Dist);
        debugger.Set(mLimiter);
    }

    private void Update() 
    {
        sphere.transform.position = mLimiter.ClampPos(sphere.transform.position);
    }

    void OnDrawGizmos()
    {
        if(cam != null)
        {
            Gizmos.color = Color.white;
            Gizmos.matrix = Matrix4x4.TRS(cam.transform.position, cam.transform.rotation, Vector3.one);
            Gizmos.DrawFrustum(Vector3.zero, mFov, Dist, Near, mAspect);
        }
    }

    private float GetAngle(Vector3 vec1, Vector3 vec2)
    {
        float theta = Vector3.Dot(vec1, vec2) / (vec1.magnitude * vec2.magnitude);
        Vector3 dirAngle = Vector3.Cross(vec1, vec2);
        float angle = Mathf.Acos(theta) * Mathf.Rad2Deg;
        if (dirAngle.z < 0.0f) 
            angle = 360 - angle;
        
        angle = (float)Math.Round(angle, 2);
        return angle;
    }
}
