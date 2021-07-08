using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class RopeRendering : MonoBehaviour
{
    // Start is called before the first frame update

    public LineRenderer lineRenderer;
    public Vector2[] ropePositions;
    private RopeSection[] _ropeSections;
    public Vector3 gravity = Vector3.down * 9.81f;
    public float ropeSectionLength = 0.2f;

    void Start()
    {
        InitRope(out _ropeSections, 10);
        DrawRope(_ropeSections, lineRenderer);
    }

    private void Update()
    {
        DrawRope(_ropeSections, lineRenderer);
    }

    private void InitRope(out RopeSection[] ropeSections, int count)
    {
        ropeSections = new RopeSection[count];

        for (int i = 0; i < ropeSections.Length; i++)
        {
            ropeSections[i] = new RopeSection(transform.position + Vector3.right * ropeSectionLength * i);
        }
    }

    private void DrawRope(RopeSection[] ropeSections, LineRenderer lineRenderer)
    {
        var linePositions = new Vector3[ropeSections.Length];

        for (int i = 0; i < linePositions.Length; i++)
        {
            linePositions[i] = ropeSections[i].pos;
        }

        lineRenderer.positionCount = ropeSections.Length;
        lineRenderer.SetPositions(linePositions);
    }

    private void RelaxRope()
    {
        
    }

    private void VerletIntegration()
    {
        for (int i = 1; i < _ropeSections.Length; i++)
        {
            RopeSection currentRopeSection = _ropeSections[i];
            Vector3 velocity = currentRopeSection.pos - currentRopeSection.oldPos;
            //updating the old position
            currentRopeSection.oldPos = currentRopeSection.pos;
            //we add the velocity at the current position
            currentRopeSection.pos += velocity;
            //adding the gravity
            currentRopeSection.pos += gravity * Time.deltaTime;
        }
    }

    void RopeStretchMax()
    {
        for (int i = 1; i < _ropeSections.Length - 1; i++)
        {
            RopeSection top = _ropeSections[i];
            RopeSection bottom = _ropeSections[i + 1];
            //calculating the distance
            float distance = Vector3.Distance(top.pos, bottom.pos);
            //calculating the distance error, comparing to the length of the segment
            float distError = Mathf.Abs(distance - ropeSectionLength);
            Vector3 changeDir = Vector3.zero;
            if (distance > ropeSectionLength) //rope fragment stretched => we compressed it
                changeDir = top.pos - bottom.pos; //positive direction is up
            else if (distance > ropeSectionLength) //rope fragment compressed => we stretch it
                changeDir = bottom.pos - top.pos; //positive direction is down
            if (changeDir != Vector3.zero)
            {
                //The distance correction is apportioned between the wo segments at 50%
                bottom.pos += changeDir.normalized * distError * 0.5f;
                top.pos -= changeDir.normalized * distError * 0.5f;
            }
        }
    }
}

public class RopeSection
{
    public Vector3 pos;
    public Vector3 oldPos;

    public RopeSection(Vector3 pos)
    {
        this.pos = pos;
        oldPos = pos;
    }
}