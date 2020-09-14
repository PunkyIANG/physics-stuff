using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update

    public float speed;
    public float acceleration;
    private Rigidbody2D _playerRb;
    public ForceMode2D mode;
    private DistanceJoint2D _playerDistanceJoint;
    public Camera mainCamera;
    public Transform hingeAnchorTransform;
    public LineRenderer lineRenderer;

    public float maxRaycastDistance;
    public float valueCloseToZero = 0.1f;
    private List<AnchorPos> _ropePoints = new List<AnchorPos>();

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
        ForceMovement();

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
                ;
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
            RaycastHit2D hit = Physics2D.Raycast(transform.position, hingeAnchorTransform.position - transform.position,
                Vector3.Magnitude(hingeAnchorTransform.position - transform.position) - valueCloseToZero);

            if (hit.collider != null)
            {
                var closestPoint = GetClosestPoint(GetColliderPoints(hit.collider), hit.point);
                _displayClosestPoint = closestPoint;
                
                if (_ropePoints[_ropePoints.Count - 1].Position != closestPoint &&
                    closestPoint != Vector2.zero)
                {
                    _ropePoints.Add(new AnchorPos(closestPoint, 0));
                    hingeAnchorTransform.position = _ropePoints[_ropePoints.Count - 1].Position;
                    _playerDistanceJoint.distance -= Vector2.Distance(_ropePoints[_ropePoints.Count - 1].Position,
                        _ropePoints[_ropePoints.Count - 2].Position);
                }
            }
        }

        HandleRopeUnwrap();
    }

    private void HandleRopeUnwrap()
    {
        if (_ropePoints.Count <= 1)
        {
            return;
        }

        var anchorIndex = _ropePoints.Count - 2;
        var hingeIndex = _ropePoints.Count - 1;
        var anchorPosition = _ropePoints[anchorIndex].Position;
        var hingePosition = _ropePoints[hingeIndex].Position;
        var hingeDir = hingePosition - anchorPosition;
        var hingeAngle = Vector2.Angle(anchorPosition, hingeDir);
        var playerDir = (Vector2) transform.position - anchorPosition;
        var playerAngle = Vector2.Angle(anchorPosition, playerDir);

        if (playerAngle < hingeAngle)
        {
            if (_ropePoints[hingeIndex].AngleDir == 1)
            {
                UnwrapRopePosition(anchorIndex, hingeIndex);
                return;
            }

            _ropePoints[hingeIndex] = new AnchorPos(_ropePoints[hingeIndex].Position, -1);
        }
        else
        {
            if (_ropePoints[hingeIndex].AngleDir == -1)
            {
                UnwrapRopePosition(anchorIndex, hingeIndex);
                return;
            }

            _ropePoints[hingeIndex] = new AnchorPos(_ropePoints[hingeIndex].Position, 1);
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

        print("Unwrapped rope");
    }

    private void SetLinePositions()
    {
        var linePositions = new Vector3[_ropePoints.Count + 1];
        lineRenderer.positionCount = _ropePoints.Count + 1;

        for (int i = 0; i < _ropePoints.Count; i++)
        {
            linePositions[i] = _ropePoints[i].Position;
        }

        linePositions[_ropePoints.Count] = transform.position;
        lineRenderer.SetPositions(linePositions);
    }

    private void ForceMovement()
    {
        var inputMovement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        _playerRb.AddForce(inputMovement.normalized * acceleration, mode);
    }

    private Vector2[] GetColliderPoints(Collider2D collider2D)
    {
        if (collider2D is BoxCollider2D)
        {
            var boxCollider2D = collider2D as BoxCollider2D;
            
            float top = boxCollider2D.offset.y + (boxCollider2D.size.y / 2f);
            float btm = boxCollider2D.offset.y - (boxCollider2D.size.y / 2f);
            float left = boxCollider2D.offset.x - (boxCollider2D.size.x / 2f);
            float right = boxCollider2D.offset.x + (boxCollider2D.size.x / 2f);

            var transform = boxCollider2D.transform;
            var points = new Vector2[4];

            points[0] = transform.TransformPoint(new Vector3(left, top, 0f));
            points[1] = transform.TransformPoint(new Vector3(right, top, 0f));
            points[2] = transform.TransformPoint(new Vector3(left, btm, 0f));
            points[3] = transform.TransformPoint(new Vector3(right, btm, 0f));

            return points;
        }
        else if (collider2D is PolygonCollider2D)
        {
            var polygonCollider2D = collider2D as PolygonCollider2D;
            return polygonCollider2D.points;
        }
        else
        {
            return null;
        }
    }

    private Vector2 GetClosestPoint(Vector2[] points, Vector2 mainPoint)
    {
        var closestPoint = Vector2.zero;

        foreach (var point in points)
        {
            if (closestPoint == Vector2.zero ||
                Vector2.Distance(closestPoint, mainPoint) >
                Vector2.Distance(point, mainPoint))
            {
                closestPoint = point;
            }
        }

        return closestPoint;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (_displayClosestPoint != Vector2.zero)
        {
            Gizmos.DrawSphere(_displayClosestPoint, 0.1f);
        }
    }
}