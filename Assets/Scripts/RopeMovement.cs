using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AngleDirection
{
    Clockwise = 1,
    CounterClockwise = -1
}
public struct AnchorPos
{
    public Vector2 Position;
    public int AngleDir;

    public AnchorPos(Vector2 position, int angleDir)
    {
        Position = position;
        AngleDir = angleDir;
    }
}

public class RopeMovement : MonoBehaviour
{
    // Start is called before the first frame update

    private Rigidbody2D _playerRb;
    private DistanceJoint2D _playerDistanceJoint;
    public Camera mainCamera;
    public Transform hingeAnchorTransform;
    public LineRenderer lineRenderer;

    public float maxRaycastDistance;
    public float valueCloseToZero = 0.1f;
    private List<AnchorPos> _ropePoints = new List<AnchorPos>();
    public List<Vector2> ropePositions = new List<Vector2>();
    public List<int> ropeAngles = new List<int>();

    private bool _raycastOnFixedUpdate = false;
    private Vector2 _displayClosestPoint = Vector2.zero;

    private void Start()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _playerDistanceJoint = GetComponent<DistanceJoint2D>();
        mainCamera = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _raycastOnFixedUpdate = true;
        }

        if (lineRenderer.enabled)
        {
            SetLinePositions();
        }
    }

    private void FixedUpdate()
    {
        if (_raycastOnFixedUpdate)
        {
            var hit = Physics2D.Raycast(transform.position,
                mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position, maxRaycastDistance);

            if (hit.collider != null && !_playerDistanceJoint.enabled)
            {
                _playerDistanceJoint.enabled = true; //turning on the rope
                lineRenderer.enabled = true;

                _playerDistanceJoint.distance = Vector2.Distance(hit.point, transform.position);

                _ropePoints.Add(new AnchorPos(hit.point, 0));
                hingeAnchorTransform.position = _ropePoints[_ropePoints.Count - 1].Position;
            }
            else if (_playerDistanceJoint.enabled)
            {
                _playerDistanceJoint.enabled = false; //turning off the rope
                lineRenderer.enabled = false;
                _ropePoints.Clear();
            }

            _raycastOnFixedUpdate = false;
        }
        else if (_playerDistanceJoint.enabled)
        {
            var raycastDir = hingeAnchorTransform.position - transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, raycastDir,
                Vector3.Magnitude(raycastDir) - valueCloseToZero);

            if (hit.collider != null)
            {
                var closestPoint = GetClosestPoint(GetColliderPoints(hit.collider), hit.point);
                _displayClosestPoint = closestPoint;

                if (_ropePoints[_ropePoints.Count - 1].Position != closestPoint && closestPoint != Vector2.zero)
                {
                    _ropePoints.Add(new AnchorPos(closestPoint,
                        GetAngleDir(_ropePoints[_ropePoints.Count - 1].Position, closestPoint)));
                    hingeAnchorTransform.position = _ropePoints[_ropePoints.Count - 1].Position;
                    _playerDistanceJoint.distance -= Vector2.Distance(_ropePoints[_ropePoints.Count - 1].Position,
                        _ropePoints[_ropePoints.Count - 2].Position);
                }
            }
        }

        HandleRopeUnwrap();
        TransferVector2Positions();
    }

    private void HandleRopeUnwrap()
    {
        if (_ropePoints.Count <= 1)
        {
            return;
        }

        var anchorIndex = _ropePoints.Count - 2;
        var hingeIndex = _ropePoints.Count - 1;

        if (GetAngleDir(_ropePoints[anchorIndex].Position, _ropePoints[hingeIndex].Position) !=
            _ropePoints[hingeIndex].AngleDir)
        {
            UnwrapRopePosition(anchorIndex, hingeIndex);
        }
    }

    private void UnwrapRopePosition(int anchorIndex, int hingeIndex)
    {
        // 1
        var newAnchorPosition = _ropePoints[anchorIndex].Position;
        _playerDistanceJoint.distance += Vector2.Distance(_ropePoints[hingeIndex].Position, newAnchorPosition);
        _ropePoints.RemoveAt(hingeIndex);

        // 2
        hingeAnchorTransform.position = newAnchorPosition;
        // distanceSet = false;
        //
        // // Set new rope distance joint distance for anchor position if not yet set.
        // if (distanceSet)
        // {
        //     return;
        // }
        // distanceSet = true;
    }

    private void SetLinePositions()
    {
        var linePositionsCount = _ropePoints.Count + 1;
        var linePositions = new Vector3[linePositionsCount];
        lineRenderer.positionCount = linePositionsCount;

        for (int i = 0; i < _ropePoints.Count; i++)
        {
            linePositions[i] = _ropePoints[i].Position;
        }

        linePositions[_ropePoints.Count] = transform.position;
        lineRenderer.SetPositions(linePositions);
    }

    private Vector2[] GetColliderPoints(Collider2D collider2D)
    {
        if (collider2D is BoxCollider2D boxCollider2D)
        {
            var boxCollider2DOffset = boxCollider2D.offset;
            var boxCollider2DSize = boxCollider2D.size;

            var top = boxCollider2DOffset.y + (boxCollider2DSize.y / 2f);
            var btm = boxCollider2DOffset.y - (boxCollider2DSize.y / 2f);
            var left = boxCollider2DOffset.x - (boxCollider2DSize.x / 2f);
            var right = boxCollider2DOffset.x + (boxCollider2DSize.x / 2f);

            var boxCollider2DTransform = boxCollider2D.transform;
            var points = new Vector2[4];

            points[0] = boxCollider2DTransform.TransformPoint(new Vector3(left, top, 0f));
            points[1] = boxCollider2DTransform.TransformPoint(new Vector3(right, top, 0f));
            points[2] = boxCollider2DTransform.TransformPoint(new Vector3(left, btm, 0f));
            points[3] = boxCollider2DTransform.TransformPoint(new Vector3(right, btm, 0f));

            return points;
        }
        else if (collider2D is PolygonCollider2D polygonCollider2D)
        {
            return polygonCollider2D.points;
        }
        else
        {
            print("ERROR: unsupported collider type");
            return null;
        }
    }

    private Vector2 GetClosestPoint(Vector2[] points, Vector2 mainPoint)
    {
        var closestPoint = Vector2.zero;
        float minDistance = float.MaxValue;

        foreach (var point in points)
        {
            var pointDistance = Vector2.Distance(point, mainPoint);
            if (minDistance == float.MaxValue ||
                minDistance > pointDistance)
            {
                minDistance = pointDistance;
                closestPoint = point;
            }
        }

        return closestPoint;
    }

    private void TransferVector2Positions() //debug stuff
    {
        ropePositions.Clear();
        ropeAngles.Clear();
        foreach (var point in _ropePoints)
        {
            ropePositions.Add(point.Position);
            ropeAngles.Add(point.AngleDir);
        }
    }

    private int GetAngleDir(Vector2 anchor, Vector2 hinge)
    {
        return Vector2.SignedAngle(hinge - anchor,
            (Vector2)transform.position - hinge) > 0
            ? (int)AngleDirection.CounterClockwise
            : (int)AngleDirection.Clockwise;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (_displayClosestPoint != Vector2.zero)
        {
            Gizmos.DrawSphere(_displayClosestPoint, 0.2f);
        }
    }
}