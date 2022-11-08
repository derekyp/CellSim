using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
// using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.U2D;

public class SoftBody : MonoBehaviour
{
    #region Constants
    private const float splineOffset = 0.5f;
    #endregion

    public GameObject newGrowSegment;
    
    private Colors colors;

    [SerializeField] public Pause isPaused;

    #region Fields
    [SerializeField]
    public SpriteShapeController spriteShape;
    [SerializeField]
    public List<Transform> points;
    #endregion

    #region MonoBehaviour Callbacks
    private void Start()
    {
        var sprite = Resources.Load("Closed Sprite Shape") as GameObject;
        var skin = Instantiate(sprite, transform.position, transform.rotation, transform);
        colors = FindObjectOfType<Colors>();

        Color newColor = skin.GetComponent<SpriteShapeRenderer>().color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

        while (colors.alreadyExists(newColor))
        {
            newColor = skin.GetComponent<SpriteShapeRenderer>().color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
        
        colors.addColor(newColor);

        spriteShape = skin.GetComponent<SpriteShapeController>();
        newGrowSegment.GetComponent<Grow>().shape = spriteShape;
        Spline spline = spriteShape.spline;
        spline.Clear();
        
        spriteShape.gameObject.transform.Rotate(-transform.eulerAngles);

        GameObject cellSegment = newGrowSegment;
        List<GameObject> orderedSegments = new List<GameObject>();

        GameObject leftEnd = null;
        GameObject rightEnd = null;

        foreach (Transform cell in transform)
        {
            if (cell.childCount > 2 && cell.GetComponent<EndCapInfo>() != null && cell.GetComponent<EndCapInfo>().side is "left")
            {
                leftEnd = cell.gameObject;
            }
            
            if (cell.childCount > 2 && cell.GetComponent<EndCapInfo>() != null && cell.GetComponent<EndCapInfo>().side is "right")
            {
                rightEnd = cell.gameObject;
            }
        }

        leftEnd.tag = "left";
        orderedSegments.Add(leftEnd);
        cellSegment.tag = "left";
        orderedSegments.Add(cellSegment);

        GameObject connectedBody = null;
        
        foreach (SpringJoint2D spring in cellSegment.GetComponentsInChildren<SpringJoint2D>())
        {
            if (spring.connectedBody.transform.childCount > 0)
            {
                connectedBody = spring.connectedBody.gameObject;
            }

            if (spring.connectedAnchor == Vector2.zero && spring.connectedBody.transform.childCount > 0)
            {
                spring.autoConfigureDistance = false;
            }
        }
        
        while (connectedBody != null)
        {
            cellSegment = connectedBody;
            cellSegment.tag = "right";
            orderedSegments.Add(cellSegment);

            foreach (SpringJoint2D spring in cellSegment.GetComponents<SpringJoint2D>())
            {
                connectedBody = null;
                if (spring.connectedBody != null)
                {
                    if (spring.connectedBody.transform.childCount > 0)
                    {
                        connectedBody = spring.connectedBody.gameObject;
                    }   
                }
            }
        }
        
        // Order the segments so that the growth segment is at the start
        rightEnd.tag = "right";
        orderedSegments.Add(rightEnd);

        List<Transform> nodeTop = new List<Transform>();
        List<Transform> nodeBottom = new List<Transform>();
        List<Transform> nodeUpper = new List<Transform>();
        List<Transform> nodeMidUpper = new List<Transform>();
        List<Transform> nodeMiddle = new List<Transform>();
        List<Transform> nodeMidLower = new List<Transform>();
        List<Transform> nodeLower = new List<Transform>();

        // prepare the nodes for their placement in the splines
        foreach (GameObject middle in orderedSegments)
        {
            // mark the location of the nodes
            foreach (Transform node in middle.transform)
            {
                if (node.CompareTag("Top"))
                {
                    nodeTop.Add(node);
                }
                
                if (node.CompareTag("Bottom"))
                {
                    nodeBottom.Add(node);
                }
                
                if (node.CompareTag("Upper end"))
                {
                    nodeUpper.Add(node);
                }
                
                if (node.CompareTag("Upper middle end"))
                {
                    nodeMidUpper.Add(node);
                }
                
                if (node.CompareTag("Middle end"))
                {
                    nodeMiddle.Add(node);
                }
                
                if (node.CompareTag("Lower middle end"))
                {
                    nodeMidLower.Add(node);
                }
                
                if (node.CompareTag("Lower end"))
                {
                    nodeLower.Add(node);
                }
            }
        }

        int segmentLength = orderedSegments.Count;

        // assign the correct spline index and create a spline for each node
        for (int i = 0; i < segmentLength; i++)
        {
            // top
            Quaternion topRotation = nodeTop[i].transform.rotation;
            float topRadius = nodeTop[i].GetComponent<CapsuleCollider2D>().size.y  / 2f;
            Vector2 topPos = nodeTop[i].transform.position - transform.position;
            Vector2 topCenterNorm = ((nodeTop[i].transform.position - transform.position) -
                                     (nodeTop[i].parent.position - transform.position)).normalized;

            spline.InsertPointAt(i, (topPos+topRadius*topCenterNorm));
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
            
            Vector2 topNewRt = Vector2.Perpendicular(topCenterNorm)*0.1f;
            Vector2 topNewLt = Vector2.zero - (topNewRt);

            spline.SetRightTangent(i, topNewRt);
            spline.SetLeftTangent(i, topNewLt);

            points.Insert(i, nodeTop[i]);
        }
        
        // upper
        Quaternion upperRotation = nodeUpper[1].transform.rotation;
        float upperRadius = nodeUpper[1].GetComponent<CapsuleCollider2D>().size.y  / 2f;
        Vector2 upperPos = nodeUpper[1].transform.position - transform.position;
        Vector2 upperCenterNorm = ((nodeUpper[1].transform.position - transform.position) -
                                   (nodeUpper[1].parent.position - transform.position)).normalized;
        
        spline.InsertPointAt(segmentLength, (upperPos+upperRadius*upperCenterNorm));
        spline.SetTangentMode(segmentLength, ShapeTangentMode.Continuous);
            
        Vector2 upperNewRt = Vector2.Perpendicular(upperCenterNorm)*0.1f;
        Vector2 upperNewLt = Vector2.zero - (upperNewRt);

        spline.SetRightTangent(segmentLength, upperNewRt);
        spline.SetLeftTangent(segmentLength, upperNewLt);

        points.Insert(segmentLength, nodeUpper[1]);
        
        // upper middle
        Quaternion upperMidRotation = nodeMidUpper[1].transform.rotation;
        float upperMidRadius = nodeMidUpper[1].GetComponent<CapsuleCollider2D>().size.y  / 2f;
        Vector2 upperMidPos = nodeMidUpper[1].transform.position - transform.position;
        Vector2 upperMidCenterNorm = ((nodeMidUpper[1].transform.position - transform.position) -
                                   (nodeMidUpper[1].parent.position - transform.position)).normalized;
        
        spline.InsertPointAt(segmentLength + 1, (upperMidPos+upperMidRadius*upperMidCenterNorm));
        spline.SetTangentMode(segmentLength + 1, ShapeTangentMode.Continuous);
            
        Vector2 upperMidNewRt = Vector2.Perpendicular(upperMidCenterNorm)*0.1f;
        Vector2 upperMidNewLt = Vector2.zero - (upperMidNewRt);

        spline.SetRightTangent(segmentLength + 1, upperMidNewRt);
        spline.SetLeftTangent(segmentLength + 1, upperMidNewLt);

        points.Insert(segmentLength + 1, nodeMidUpper[1]);
        
        // middle
        Quaternion middleRotation = nodeMiddle[1].transform.rotation;
        float middleRadius = nodeMiddle[1].GetComponent<CapsuleCollider2D>().size.y  / 2f;
        Vector2 middlePos = nodeMiddle[1].transform.position - transform.position;
        Vector2 middleCenterNorm = ((nodeMiddle[1].transform.position - transform.position) -
                                    (nodeMiddle[1].parent.position - transform.position)).normalized;
        
        spline.InsertPointAt(segmentLength + 2, (middlePos+middleRadius*middleCenterNorm));
        spline.SetTangentMode(segmentLength + 2, ShapeTangentMode.Continuous);
            
        Vector2 middleNewRt = Vector2.Perpendicular(middleCenterNorm)*0.1f;
        Vector2 middleNewLt = Vector2.zero - (middleNewRt);

        spline.SetRightTangent(segmentLength + 2, middleNewRt);
        spline.SetLeftTangent(segmentLength + 2, middleNewLt);

        points.Insert(segmentLength + 2, nodeMiddle[1]);
        
        // lower middle
        Quaternion lowerMidRotation = nodeMidLower[1].transform.rotation;
        float lowerMidRadius = nodeMidLower[1].GetComponent<CapsuleCollider2D>().size.y  / 2f;
        Vector2 lowerMidPos = nodeMidLower[1].transform.position - transform.position;
        Vector2 lowerMidCenterNorm = ((nodeMidLower[1].transform.position - transform.position) -
                                      (nodeMidLower[1].parent.position - transform.position)).normalized;
        
        spline.InsertPointAt(segmentLength + 3, (lowerMidPos+lowerMidRadius*lowerMidCenterNorm));
        spline.SetTangentMode(segmentLength + 3, ShapeTangentMode.Continuous);
            
        Vector2 lowerMidNewRt = Vector2.Perpendicular(lowerMidCenterNorm)*0.1f;
        Vector2 lowerMidNewLt = Vector2.zero - (lowerMidNewRt);

        spline.SetRightTangent(segmentLength + 3, lowerMidNewRt);
        spline.SetLeftTangent(segmentLength + 3, lowerMidNewLt);

        points.Insert(segmentLength + 3, nodeMidLower[1]);
        
        // lower
        Quaternion lowerRotation = nodeLower[1].transform.rotation;
        float lowerRadius = nodeLower[1].GetComponent<CapsuleCollider2D>().size.y  / 2f;
        Vector2 lowerPos = nodeLower[1].transform.position - transform.position;
        Vector2 lowerCenterNorm = ((nodeLower[1].transform.position - transform.position) -
                                   (nodeLower[1].parent.position - transform.position)).normalized;
        
        spline.InsertPointAt(segmentLength + 4, (lowerPos+lowerRadius*lowerCenterNorm));
        spline.SetTangentMode(segmentLength + 4, ShapeTangentMode.Continuous);
            
        Vector2 lowerNewRt = Vector2.Perpendicular(lowerCenterNorm)*0.1f;
        Vector2 lowerNewLt = Vector2.zero - (lowerNewRt);

        spline.SetRightTangent(segmentLength + 4, lowerNewRt);
        spline.SetLeftTangent(segmentLength + 4, lowerNewLt);

        points.Insert(segmentLength + 4, nodeLower[1]);

        for (int i = segmentLength; i < (2 * segmentLength ); i++)
        {
            // bottom
            Quaternion bottomRotation = nodeBottom[segmentLength - 1 - (i % segmentLength)].transform.rotation;
            float bottomRadius = nodeBottom[segmentLength - 1 - (i % segmentLength)].GetComponent<CapsuleCollider2D>().size.y / 2f;
            Vector2 bottomPos = nodeBottom[segmentLength - 1 - (i % segmentLength)].transform.position - transform.position;
            Vector2 bottomCenterNorm = ((nodeBottom[segmentLength - 1 - (i % segmentLength)].transform.position - transform.position) -
                                        (nodeBottom[segmentLength - 1 - (i % segmentLength)].parent.position - transform.position)).normalized;
            
            spline.InsertPointAt(i + 5, (bottomPos+bottomRadius*bottomCenterNorm));
            spline.SetTangentMode(i + 5, ShapeTangentMode.Continuous);
            
            Vector2 bottomNewRt = Vector2.Perpendicular(bottomCenterNorm)*0.1f;
            Vector2 bottomNewLt = Vector2.zero - (bottomNewRt);

            spline.SetRightTangent( i + 5, bottomNewRt);
            spline.SetLeftTangent( i + 5, bottomNewLt);

            points.Insert(i + 5, nodeBottom[segmentLength - 1 - (i % segmentLength)]);
        }

        // lower
        lowerRotation = nodeLower[0].transform.rotation;
        lowerRadius = nodeLower[0].GetComponent<CapsuleCollider2D>().size.y  / 2f;
        lowerPos = nodeLower[0].transform.position - transform.position;
        lowerCenterNorm = ((nodeLower[0].transform.position - transform.position) -
                                   (nodeLower[0].parent.position - transform.position)).normalized;
        
        spline.InsertPointAt(2*segmentLength + 5, (lowerPos+lowerRadius*lowerCenterNorm));
        spline.SetTangentMode(2*segmentLength + 5, ShapeTangentMode.Continuous);
            
        lowerNewRt = Vector2.Perpendicular(lowerCenterNorm)*0.1f;
        lowerNewLt = Vector2.zero - (lowerNewRt);

        spline.SetRightTangent(2*segmentLength + 5, lowerNewRt);
        spline.SetLeftTangent(2*segmentLength + 5, lowerNewLt);

        points.Insert(2*segmentLength + 5, nodeLower[0]);
        
        // mid lower
        lowerMidRotation = nodeMidLower[0].transform.rotation;
        lowerMidRadius = nodeMidLower[0].GetComponent<CapsuleCollider2D>().size.y  / 2f;
        lowerMidPos = nodeMidLower[0].transform.position - transform.position;
        lowerMidCenterNorm = ((nodeMidLower[0].transform.position - transform.position) -
                              (nodeMidLower[0].parent.position - transform.position)).normalized;
        
        spline.InsertPointAt(2*segmentLength + 6, (lowerMidPos+lowerMidRadius*lowerMidCenterNorm));
        spline.SetTangentMode(2*segmentLength + 6, ShapeTangentMode.Continuous);
            
        lowerMidNewRt = Vector2.Perpendicular(lowerMidCenterNorm)*0.1f;
        lowerMidNewLt = Vector2.zero - (lowerMidNewRt);

        spline.SetRightTangent(2*segmentLength + 6, lowerMidNewRt);
        spline.SetLeftTangent(2*segmentLength + 6, lowerMidNewLt);

        points.Insert(2*segmentLength + 6, nodeMidLower[0]);
        
        // middle
        middleRotation = nodeMiddle[0].transform.rotation;
        middleRadius = nodeMiddle[0].GetComponent<CapsuleCollider2D>().size.y  / 2f;
        middlePos = nodeMiddle[0].transform.position - transform.position;
        middleCenterNorm = ((nodeMiddle[0].transform.position - transform.position) -
                            (nodeMiddle[0].parent.position - transform.position)).normalized;
        
        spline.InsertPointAt(2*segmentLength + 7, (middlePos+middleRadius*middleCenterNorm));
        spline.SetTangentMode(2*segmentLength + 7, ShapeTangentMode.Continuous);
            
        middleNewRt = Vector2.Perpendicular(middleCenterNorm)*0.1f;
        middleNewLt = Vector2.zero - (middleNewRt);

        spline.SetRightTangent(2*segmentLength + 7, middleNewRt);
        spline.SetLeftTangent(2*segmentLength + 7, middleNewLt);

        points.Insert(2*segmentLength + 7, nodeMiddle[0]);
        
        // mid upper
        upperMidRotation = nodeMidUpper[0].transform.rotation;
        upperMidRadius = nodeMidUpper[0].GetComponent<CapsuleCollider2D>().size.y  / 2f;
        upperMidPos = nodeMidUpper[0].transform.position - transform.position;
        upperMidCenterNorm = ((nodeMidUpper[0].transform.position - transform.position) -
                              (nodeMidUpper[0].parent.position - transform.position)).normalized;
        
        spline.InsertPointAt(2*segmentLength + 8, (upperMidPos+upperMidRadius*upperMidCenterNorm));
        spline.SetTangentMode(2*segmentLength + 8, ShapeTangentMode.Continuous);
            
        upperMidNewRt = Vector2.Perpendicular(upperMidCenterNorm)*0.1f;
        upperMidNewLt = Vector2.zero - (upperMidNewRt);

        spline.SetRightTangent(2*segmentLength + 8, upperMidNewRt);
        spline.SetLeftTangent(2*segmentLength + 8, upperMidNewLt);

        points.Insert(2*segmentLength + 8, nodeMidUpper[0]);
        
        // upper
        upperRotation = nodeUpper[0].transform.rotation;
        upperRadius = nodeUpper[0].GetComponent<CapsuleCollider2D>().size.y  / 2f;
        upperPos = nodeUpper[0].transform.position - transform.position;
        upperCenterNorm = ((nodeUpper[0].transform.position - transform.position) -
                           (nodeUpper[0].parent.position - transform.position)).normalized;
        
        spline.InsertPointAt(2*segmentLength + 9, (upperPos+upperRadius*upperCenterNorm));
        spline.SetTangentMode(2*segmentLength + 9, ShapeTangentMode.Continuous);
            
        upperNewRt = Vector2.Perpendicular(upperCenterNorm)*0.1f;
        upperNewLt = Vector2.zero - (upperNewRt);

        spline.SetRightTangent(2*segmentLength + 9, upperNewRt);
        spline.SetLeftTangent(2*segmentLength + 9, upperNewLt);

        points.Insert(2*segmentLength + 9, nodeUpper[0]);
        
        points.Add(transform);
        
        spriteShape.RefreshSpriteShape();
        

        Destroy(newGrowSegment.GetComponent<Grow>());
        Grow growth = newGrowSegment.AddComponent<Grow>();

        growth.shape = spriteShape;
        growth.midSectionNumber = 2 * (orderedSegments.Count ) + 3;
        growth.isPaused = FindObjectOfType<Pause>();
        growth.primary = true;
        growth.enabled = true;
    }

    private void Update()
    {
        if (!isPaused.pause)
        {
            UpdateVerticies();
        }
    }
    #endregion

    #region privateMethods
    private void UpdateVerticies()
    {
        for(int i  = 0; i< points.Count - 1; i++)
        {
            Vector2 _vertex = (points[i].position - gameObject.transform.position);

            Vector2 _towardsCenter = ((points[i].position - transform.position) - (points[i].parent.position - transform.position)).normalized;

            float _colliderRadius = points[i].gameObject.GetComponent<CapsuleCollider2D>().size.y / 2f;
            try
            {
                spriteShape.spline.SetPosition(i, (_vertex + _towardsCenter * _colliderRadius));
            }
            catch
            {
                Debug.Log("Spline points are too close to each other.. recalculate");
                spriteShape.spline.SetPosition(i, (_vertex - _towardsCenter * (_colliderRadius + splineOffset)));
            }

            Vector2 _lt = spriteShape.spline.GetLeftTangent(i);
            
            Vector2 _newRt = -Vector2.Perpendicular(_towardsCenter) * _lt.magnitude;
            Vector2 _newLt = Vector2.zero - (_newRt);

            spriteShape.spline.SetRightTangent(i, _newRt);
            spriteShape.spline.SetLeftTangent(i, _newLt);
        }
    }
    #endregion
}

