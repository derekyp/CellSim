using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
// using UnityEditor.Animations;
// using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;

public class Grow : MonoBehaviour
// Only act on springs attaching the two middle segments of the cell

// when reproducing take a duplicate of the segment connected to the center one, but towards the end cap and shift it over

// When deciding to divide have the spring constants slowly get stiffer as you add new segments so it looks like it is getting pinched off
// or slowly make eq. distance for spring shorter as it gets longer.

{
    public float growthrate;
    private bool _entered;
    public bool afterEntered;
    List<SpringJoint2D> _springs;
    private float _maxDist;
    private GameObject _newseg;
    public SpriteShapeController shape;
    private SoftBody _softBodyInstance;
    private Spline _spline;
    private float _tangentlength;
    private List<Transform> _newPoints;
    public int _segmentGeneration;
    private float _startTime;
    public int midSectionNumber;

    private bool _enteredSplit;

    private bool _reproduce;
    private int _reproduceTotal;
    [FormerlySerializedAs("_reproduceNumber")] public int reproduceNumber;

    private bool resetSpringCondition;
    private bool resetColliderCondition;

    [SerializeField] public Pause isPaused;
    
    public bool primary;

    public bool _center;

    // Start is called before the first frame update
    void Awake()
    {
        _springs = sortSprings(GetComponentsInChildren<SpringJoint2D>().ToList());
        growthrate = 0.0175f;
        _maxDist = 0;
        _softBodyInstance = gameObject.transform.parent.GetComponent<SoftBody>();
        _tangentlength = 0.1f;
        _newPoints = new List<Transform>();
        afterEntered = false;
        _reproduceTotal = transform.parent.GetComponent<CellInformation>().reproduceTot;
        reproduceNumber = _reproduceTotal + 1;
        _startTime = gameObject.transform.parent.GetComponent<CellInformation>().startTime;

        resetSpringCondition = false;
        resetColliderCondition = false;
    }

    // Update is called once per frame
    void Update()
    {
        int cellLength = gameObject.transform.parent.GetComponentsInChildren<Rigidbody2D>().Length;

        if (!isPaused.pause && !transform.parent.GetComponent<Reproduction>().split)
        {
            _spline = shape.spline;
            
            // Reset the segment but non-primary so it just grows
            // if (resetColliderCondition && resetSpringCondition)
            // {
            //     afterEntered = false;
            //     resetColliderCondition = false;
            //     resetSpringCondition = false;
            // }

            // Check if the current segment is marked as a "reproduction" segment
            if (!gameObject.CompareTag("reproduce"))
            {
                reproduceNumber = 0;
            }

            // Should the cell begin the reproduction process
            _reproduceTotal = transform.parent.GetComponent<CellInformation>().reproduceTot;
            if (cellLength + Random.Range(-10,10) > 12 && _reproduceTotal < 5 && primary)
            {
                _reproduce = true;
            }
            
            // Handles spring manipulations during the frame
            _entered = false;
            foreach (SpringJoint2D spring in _springs)
            {
                // check if the segment is the center of the segment
                if (spring.transform.childCount > 0)
                {
                    _center = true;
                }
                
                // check the distance between the current segment and the selected segment
                Vector3 sep = (spring.connectedBody.transform.position -
                               transform.position);
                
                // Grow the cell by extending the spring in the designated growth segment
                if (!spring.autoConfigureDistance && spring.connectedAnchor == Vector2.zero
                                                  && primary)
                {
                    spring.distance += growthrate;
                    _maxDist = spring.distance;
                }
                
                // Grow the cell by extending the spring in the designated growth segment(prototype)
                // if ((spring.connectedBody.transform != (transform)) && (spring.connectedBody.transform.parent != transform)
                //                                                     && spring.anchor == Vector2.zero)
                // {
                //     spring.distance += growthrate;
                //     _maxDist = spring.distance;
                // }
                
                // Grow the capsule collider until it reaches its full size(only grows in direction parallel to cell axis)
                foreach (CapsuleCollider2D node in gameObject.GetComponentsInChildren<CapsuleCollider2D>())
                {
                    if(node.size.x < 1f)
                    {
                        node.size += new Vector2(0.003f,0f);
                    }
                    
                    // if(node.size.x >= 1f && afterEntered
                    //    && (!node.CompareTag("reproduce") || !node.transform.parent.CompareTag("reproduce")))
                    // {
                    //     resetColliderCondition = true;
                    // }
                    
                    if(_reproduceTotal >= 5 && reproduceNumber is 3 && node.size.x < 2.5f)
                    {
                        node.direction = CapsuleDirection2D.Horizontal;
                        node.size += new Vector2(0.007f,-0.0005f);
                    }
                }

                // Start this once the separation is just large enough to fit a new segment in
                if ((_maxDist > (0.5f*gameObject.GetComponent<CapsuleCollider2D>().size.y)) && (transform.childCount > 0) 
                    && (spring.connectedAnchor == Vector2.zero) && (!_entered)
                    && primary)
                {
                    _entered = true;
                    _newPoints = _softBodyInstance.points;

                    //instantiate the new segment
                    _newseg = Instantiate(gameObject, transform.position + 0.5f * sep, transform.rotation, transform.parent);
                    _newseg.tag = "left";

                    // part of the right side of the cell after reproduction sections are added
                    if (_reproduceTotal >= 5)
                    {
                        _newseg.tag = "right";
                    }

                    // initiate the segments that will be used for dividing the cell
                    if (_reproduce)
                    {
                        _newseg.tag = "reproduce";
                        transform.parent.GetComponent<CellInformation>().reproduceTot += 1;
                        _reproduceTotal = transform.parent.GetComponent<CellInformation>().reproduceTot;

                        if(_reproduceTotal is 1 or 5)
                        {
                            transform.parent.GetComponent<Reproduction>().endConnectors.Add(_newseg);
                        }

                        _newseg.GetComponent<Grow>()._reproduceTotal = _reproduceTotal;
                        if (_reproduceTotal < 5)
                        {
                            _newseg.GetComponent<Grow>()._reproduce = true;
                        }
                    }
                    
                    // add to the segment generation
                    _newseg.GetComponent<Grow>()._segmentGeneration += 1;
                    
                    //set the size of the new capsules spawned in for the segment
                    foreach (CapsuleCollider2D node in _newseg.GetComponentsInChildren<CapsuleCollider2D>())
                    {
                        node.size = new Vector2(0.1f,1f);
                    }

                    // updating the visuals for the primary segments
                    int newTopPos = 0;
                    int newBottomPos = 0;
                    
                    // find the index of the old node positions
                    foreach (Transform node in transform)
                    {
                        if (node.CompareTag("Top"))
                        {
                            newTopPos = _softBodyInstance.points.IndexOf(node.transform) + 1;
                        }
                        
                        if (node.CompareTag("Bottom"))
                        {
                            newBottomPos = _softBodyInstance.points.IndexOf(node.transform);
                        }
                    }
                    
                    // the position in the SoftBody script that the new node will be placed
                    // Debug.Log(midSectionNumber);
                    // var newPos = midSectionNumber + _segmentGeneration;

                    // Place sprite splines in the corresponding new node locations
                    foreach (Transform child in _newseg.transform)
                    {
                        if (child.CompareTag("Top"))
                        {
                            Quaternion rotation = child.rotation;
                            float radius = child.GetComponent<CapsuleCollider2D>().size.y/2f;
                            Vector2 pos = child.position - transform.parent.position;
                            Vector2 posNorm = ((child.position - transform.parent.position)
                                               - (transform.position - transform.parent.position)).normalized;
                
                            _spline.InsertPointAt(newTopPos, (pos + radius * posNorm));
                            _spline.SetTangentMode(newTopPos, ShapeTangentMode.Continuous);
                    
                            Vector2 _newRt = -Vector2.Perpendicular(posNorm) * _tangentlength;
                            Vector2 _newLt = Vector2.zero - (_newRt);
                    
                            _spline.SetRightTangent(newTopPos, _newRt);
                            _spline.SetLeftTangent(newTopPos, _newLt);
                    
                            _newPoints.Insert(newTopPos, child);   
                        }
                        
                        if (child.CompareTag("Bottom"))
                        {
                            Quaternion rotation = child.rotation;
                            float radius = child.GetComponent<CapsuleCollider2D>().size.y/2f;
                            Vector2 pos = child.position - transform.parent.position;
                            Vector2 posNorm = ((child.position - transform.parent.position)
                                               - (transform.position - transform.parent.position)).normalized;
                
                            _spline.InsertPointAt(newBottomPos, (pos + radius * posNorm));
                            _spline.SetTangentMode(newBottomPos, ShapeTangentMode.Continuous);
                    
                            Vector2 _newRt = -Vector2.Perpendicular(posNorm) * _tangentlength;
                            Vector2 _newLt = Vector2.zero - (_newRt);
                    
                            _spline.SetRightTangent(newBottomPos, _newRt);
                            _spline.SetLeftTangent(newBottomPos, _newLt);
                    
                            _newPoints.Insert(newBottomPos, child);   
                        }

                        // newPos = 2 + _segmentGeneration;
                    }
                    _softBodyInstance.points = _newPoints;
                    shape.RefreshSpriteShape();
                    primary = false;

                    // midSectionNumber = transform.parent.GetComponent<CellInformation>().midSectionCount;

                    // update visuals for the non primary growth segments
                }
                
                // Set the growth rate of the now old segment
                // reset spring defaults
                if (_entered)
                {
                    afterEntered = true;

                    spring.distance = 1.118034f;
                    // if (reproduceNumber is 2 or 4 && gameObject.CompareTag("reproduce") && _reproduceTotal >= 5 && _center)
                    // {
                    //     spring.distance = 0.7f;
                    // }
                    // if ((reproduceNumber == 3) && gameObject.CompareTag("reproduce") && _reproduceTotal >= 5 && _center)
                    // {
                    //     spring.distance = 0.15f;
                    // }

                    //if it is a middle spring be sure to set the distance as such
                    if (!spring.autoConfigureDistance && spring.connectedBody.transform.parent != transform
                                                      && (spring.connectedAnchor == Vector2.zero))
                    {
                        // spring.distance = 0.05f*math.abs((spring.attachedRigidbody.position
                        //                             - spring.connectedBody.position
                        //                             + spring.connectedAnchor).magnitude);
                        spring.distance = 0.5f;
                    }
                    
                    
                    // Take springs of old segment and attach to new segment
                    foreach (SpringJoint2D newspring in _newseg.GetComponentsInChildren<SpringJoint2D>())
                    {
                        newspring.distance = 1.1f;
                        if (!newspring.autoConfigureDistance)
                        {
                            // newspring.distance = 0.05f*math.abs((newspring.attachedRigidbody.position
                            //                                     - newspring.connectedBody.position).magnitude);
                            //                                     + newspring.connectedAnchor).magnitude);
                            newspring.distance = 0.005f;
                        }

                        if(newspring.connectedBody == spring.connectedBody)
                        {
                            spring.connectedBody = newspring.attachedRigidbody;
                        }
                    }
                    
                    // Connect Friction join from old
                    foreach (FrictionJoint2D friction in gameObject.GetComponents<FrictionJoint2D>())
                    {
                        foreach (FrictionJoint2D newfriction in _newseg.GetComponents<FrictionJoint2D>())
                        {
                            
                            // Increase frictional torque to septating segments
                            if (newfriction.connectedBody.CompareTag("reproduce")
                                || newfriction.attachedRigidbody.CompareTag("reproduce"))
                            {
                                newfriction.maxTorque = 200f;
                            }
                            
                            if (newfriction.connectedBody == friction.connectedBody && newfriction.connectedBody.transform.childCount > 0)
                            {
                                friction.connectedBody = newfriction.attachedRigidbody;
                            }
                        }   
                    }

                }

                if (afterEntered)
                {
                    primary = false;
                    // Continue Growth to equilibrium position
                    // if (spring.distance < 1.118034f
                    //     && (!gameObject.CompareTag("reproduce") || spring.connectedBody.transform.parent != transform)
                    //     && spring.connectedBody.transform.parent != transform
                    //     && !spring.autoConfigureDistance)
                    // {
                    //     spring.distance += growthrate;
                    // }
                    
                    // Continue Growth to equilibrium position
                    if (spring.distance < 1.118034f
                        && (!gameObject.CompareTag("reproduce") || spring.connectedBody.transform.parent != transform)
                        && spring.connectedBody.transform.parent != transform
                        && (spring.connectedAnchor != Vector2.zero))
                    {
                        spring.distance = 1.118034f;
                    }
                    
                    if (spring.distance < 1.85f 
                                      && (!gameObject.CompareTag("reproduce")
                                          || spring.connectedBody.transform.parent != transform)
                                      && (reproduceNumber is 2 or 3))
                    {
                        spring.autoConfigureDistance = false;
                        spring.distance += growthrate;
                    }
                    
                    // Continue Growth to equilibrium position for reproduction segments
                    if ((_reproduceTotal >= 5) && (spring.distance > 1.118034f) && reproduceNumber is 2 or 4
                        && spring.connectedBody.transform.parent == transform)
                    {
                        spring.distance -= .5f * growthrate;
                    }
                    
                    if ((_reproduceTotal >= 5) && (spring.distance > 0.6f) && reproduceNumber is 3
                        && spring.connectedBody.transform.parent == transform)
                    {
                        spring.distance -= .5f * growthrate;
                    }

                    // if (spring.distance >= 1.118034f && (!gameObject.CompareTag("reproduce")
                    //                                      || spring.connectedBody.transform.parent != transform)
                    //     && (spring.connectedAnchor != Vector2.zero))
                    // {
                    //     spring.autoConfigureDistance = true;
                    // }
                    
                    // if (spring.distance >= 1.118034f
                    //     && (!gameObject.CompareTag("reproduce") || spring.connectedBody.transform.parent != transform
                    //     && spring.attachedRigidbody.GetComponent<CapsuleCollider2D>().size.x >= 1)
                    //     && !spring.attachedRigidbody.GetComponent<Grow>().primary
                    //     && spring.anchor == Vector2.zero
                    //     && spring.connectedBody.transform.parent != transform
                    //     && !spring.autoConfigureDistance)
                    // {
                    //     resetSpringCondition = true;
                    // }

                    // Should the Cell split into two
                    if ((_reproduceTotal >= 5) && (spring.distance < 0.6f) && reproduceNumber is 3 &&
                        spring.connectedBody.transform.parent == transform && !_enteredSplit )
                    {
                        transform.parent.GetComponent<Reproduction>().split = true;
                        _enteredSplit = true;
                    }
                }
            }
        }

    }
    
    #region privateMethods
    // Puts the middle springs in the front of the list of springs
    private List<SpringJoint2D> sortSprings(List<SpringJoint2D> springs)
    {
        List<SpringJoint2D> front = new List<SpringJoint2D>();
        List<int> loc = new List<int>();
        foreach (SpringJoint2D spring in springs)
        {
            if ((spring.autoConfigureDistance == false) && (spring.transform.childCount > 0)
                                                        && (spring.connectedAnchor == Vector2.zero))
            {
                loc.Add(springs.IndexOf(spring));
                front.Add(spring);
            }  
        }

        foreach (int index in loc)
        {
            springs.RemoveAt(index);
        }
        front.AddRange(springs);

        return front;
    }

    #endregion
}
