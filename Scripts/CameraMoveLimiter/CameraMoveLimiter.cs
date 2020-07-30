using UnityEngine;

public enum LimitShape
{
    Square_pyramid,
}

public class CameraMoveLimiter
{
    public Camera TargetCam { get { return mCam; } }
    private Camera mCam = null;

    // Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
    public Plane[] Planes { get { return mPlanes; } }
    private Plane[] mPlanes = new Plane[6];

    public float DepthVolume = 0f;

    public CameraMoveLimiter(Camera cam, LimitShape shape, float depthVolume)
    {
        mCam = cam;
        DepthVolume = depthVolume;

        switch(shape)
        {
            case LimitShape.Square_pyramid:
                {
                    CreateSquarePyramidPlanes();
                }
                break;
        }
    }

    void CreateSquarePyramidPlanes()
    {
        Vector3 _pt1 = mCam.ViewportToWorldPoint(new Vector3(0f, 1f, DepthVolume));
        Vector3 _pt2 = mCam.ViewportToWorldPoint(new Vector3(0f, 0f, DepthVolume));
        mPlanes[0] = new Plane(_pt1, _pt2, mCam.transform.position);

        _pt1 = mCam.ViewportToWorldPoint(new Vector3(1f, 0f, DepthVolume));
        _pt2 = mCam.ViewportToWorldPoint(new Vector3(1f, 1f, DepthVolume));
        mPlanes[1] = new Plane(_pt1, _pt2, mCam.transform.position);

        _pt1 = mCam.ViewportToWorldPoint(new Vector3(0f, 0f, DepthVolume));
        _pt2 = mCam.ViewportToWorldPoint(new Vector3(1f, 0f, DepthVolume));
        mPlanes[2] = new Plane(_pt1, _pt2, mCam.transform.position);

        _pt1 = mCam.ViewportToWorldPoint(new Vector3(1f, 1f, DepthVolume));
        _pt2 = mCam.ViewportToWorldPoint(new Vector3(0f, 1f, DepthVolume));
        mPlanes[3] = new Plane(_pt1, _pt2, mCam.transform.position);

        _pt1 = mCam.ViewportToWorldPoint(new Vector3(0f, 0f, mCam.nearClipPlane));
        _pt2 = mCam.ViewportToWorldPoint(new Vector3(1f, 0f, mCam.nearClipPlane));
        Vector3 _tempPt = mCam.ViewportToWorldPoint(new Vector3(1f, 1f, mCam.nearClipPlane));
        mPlanes[4] = new Plane(_pt1, _pt2, _tempPt);

        _pt1 = mCam.ViewportToWorldPoint(new Vector3(0f, 1f, DepthVolume));
        _pt2 = mCam.ViewportToWorldPoint(new Vector3(1f, 1f, DepthVolume));
        _tempPt = mCam.ViewportToWorldPoint(new Vector3(1f, 0f, DepthVolume));
        mPlanes[5] = new Plane(_pt1, _pt2, _tempPt);

        Debug.Log("Create Success");
    }

    public void Clear()
    {
        mPlanes = new Plane[6];
    }

    public Vector3 ClampPos(Vector3 checkPos)
    {
        Vector3 _adjustPos = checkPos;
        for(int i = 0; i < mPlanes.Length; ++i)
        {
            Plane _plane = mPlanes[i];
            if(!_plane.GetSide(checkPos))
            {
                _adjustPos = _plane.ClosestPointOnPlane(_adjustPos);
            }
        }

        return _adjustPos;
    }
}