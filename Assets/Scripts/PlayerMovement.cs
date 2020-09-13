using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private List<Vector2> _ropePositions = new List<Vector2>();

    private bool _raycastOnFixedUpdate = false;

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
                _playerDistanceJoint.enabled = true;    //turning on the rope
                lineRenderer.enabled = true;
                
                float distance = Vector2.Distance(hit.point, transform.position);
                _playerDistanceJoint.distance = distance;
                _ropePositions.Add(hit.point);
                hingeAnchorTransform.position = _ropePositions[_ropePositions.Count - 1];
            }
            else if (_playerDistanceJoint.enabled)
            {
                _playerDistanceJoint.enabled = false; //turning off the rope
                lineRenderer.enabled = false;
                _ropePositions.Clear();
            }

            _raycastOnFixedUpdate = false;
        }
        else if (_playerDistanceJoint.enabled)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, hingeAnchorTransform.position - transform.position,
                Vector3.Magnitude(hingeAnchorTransform.position - transform.position) - valueCloseToZero);

            if (hit.collider != null)
            {
                if (_ropePositions[_ropePositions.Count - 1] != hit.collider.ClosestPoint(hit.point))
                {
                    _ropePositions.Add(hit.collider.ClosestPoint(hit.point));
                    hingeAnchorTransform.position = _ropePositions[_ropePositions.Count - 1];
                    _playerDistanceJoint.distance -= Vector2.Distance(_ropePositions[_ropePositions.Count - 1],
                        _ropePositions[_ropePositions.Count - 2]);
                }
            }
        }
    }

    private void SetLinePositions()
    {
        var linePositions = new Vector3[_ropePositions.Count + 1];
        lineRenderer.positionCount = _ropePositions.Count + 1;

         for (int i = 0; i < _ropePositions.Count; i++)
         {
             linePositions[i] = _ropePositions[i];
         }
         
         linePositions[_ropePositions.Count] = transform.position;
         lineRenderer.SetPositions(linePositions);
    }

    private void ForceMovement()
    {
        var inputMovement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        _playerRb.AddForce(inputMovement.normalized * acceleration, mode);
    }

    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.red;
    // }
}