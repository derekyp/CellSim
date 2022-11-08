using System.Collections;
using System.Collections.Generic;
// using UnityEditor.Experimental.GraphView;
// using UnityEditor.SceneManagement;
// using UnityEditor.UI;
using UnityEngine;

public class Reproduction : MonoBehaviour
{
    public List<GameObject> endConnectors;
    
    public bool split;
    
    private GameObject _newCell;

    private GameObject _rightEndCap;
    private GameObject _leftEndCap;
    private SoftBody _shape;
    
    private Rigidbody2D _leftConnectorTop;
    private Rigidbody2D _rightConnectorTop;
    private Rigidbody2D _leftConnectorBottom;
    private Rigidbody2D _rightConnectorBottom;
    private Rigidbody2D _rightConnectorMiddle;
    private Rigidbody2D _leftConnectorMiddle;

    public GameObject sprite;

    [SerializeField] public Pause isPaused;
    
    // Start is called before the first frame update
    void Start()
    {
        _shape = gameObject.GetComponent<SoftBody>();
        
        _rightEndCap = Resources.Load("RightEndCap") as GameObject;
        _leftEndCap = Resources.Load("LeftEndCap") as GameObject;
        
        _newCell = Resources.Load("Cell") as GameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if(split && !isPaused.pause)
        {
            Destroy(sprite);

            List<Transform> newLeft = new List<Transform>();
            List<Transform> newRight = new List<Transform>();
            
            // get the rigid bodies that the end caps will attach to
            foreach (var segment in endConnectors)
            {
                int reproduceNumber = segment.GetComponent<Grow>().reproduceNumber;
                
                if (reproduceNumber is 1)
                {
                    _rightConnectorMiddle = segment.GetComponent<Rigidbody2D>();
                }
                
                if (reproduceNumber is 5)
                {
                    _leftConnectorMiddle = segment.GetComponent<Rigidbody2D>();
                }

                foreach (Transform node in segment.transform)
                {
                    if((reproduceNumber is 1) && node.CompareTag("Top"))
                    {
                        _rightConnectorTop = node.GetComponent<Rigidbody2D>();
                    }
                    
                    if((reproduceNumber is 1) && node.CompareTag("Bottom"))
                    {
                        _rightConnectorBottom = node.GetComponent<Rigidbody2D>();
                    }
                    
                    if((reproduceNumber is 5) && node.CompareTag("Top"))
                    {
                        _leftConnectorTop = node.GetComponent<Rigidbody2D>();
                    }
                    
                    if((reproduceNumber is 5) && node.CompareTag("Bottom"))
                    {
                        _leftConnectorBottom = node.GetComponent<Rigidbody2D>();
                    }
                }
            }

            // Perform the splitting of the cells
            GameObject softBodyLeft = null;
            GameObject softBodyRight = null;
            foreach (Transform segment in transform)
            {
                // turn off cell growth of any kind
                if (segment.GetComponent<Grow>() != null)
                {
                    if (!segment.GetComponent<Grow>().afterEntered)
                    {
                        segment.GetComponent<Grow>().afterEntered = true;
                    }
                    
                    if (segment.GetComponent<Grow>().reproduceNumber is 5 )
                    {
                        segment.GetComponent<Grow>().enabled = false;   
                    }
                }
                
                // Spawn in the left end cap and attach all the springs to the nearest neighbor
                if(segment.CompareTag("reproduce") && segment.GetComponent<Grow>().reproduceNumber is 4)
                {
                    var left = Instantiate(_leftEndCap, segment.position, segment.rotation, transform);
                    left.tag = "right";
                    foreach (SpringJoint2D spring in left.GetComponentsInChildren<SpringJoint2D>())
                    {
                        if (spring.CompareTag("Top"))
                        {
                            spring.connectedBody = _leftConnectorTop;
                        }
                            
                        if (spring.CompareTag("Bottom"))
                        {
                            spring.connectedBody = _leftConnectorBottom;
                        }
                        
                        if (!spring.CompareTag("Bottom") && !spring.CompareTag("Top") && spring.connectedBody is null)
                        {
                            spring.connectedBody = _leftConnectorMiddle;
                        }
                    }

                    foreach (FrictionJoint2D friction in left.GetComponentsInChildren<FrictionJoint2D>())
                    {
                        if (!friction.CompareTag("Bottom") && !friction.CompareTag("Top") && friction.connectedBody is null)
                        {
                            friction.connectedBody = _leftConnectorMiddle;
                        }
                    }
                }
                
                // Spawn in the right end cap and attach all the springs to the nearest neighbor
                if(segment.CompareTag("reproduce") && segment.GetComponent<Grow>().reproduceNumber is 2)
                {
                    var right = Instantiate(_rightEndCap, segment.position, segment.rotation, transform);
                    right.tag = "left";
                    foreach (SpringJoint2D spring in right.GetComponentsInChildren<SpringJoint2D>())
                    {
                        if (spring.CompareTag("Top"))
                        {
                            spring.connectedBody = _rightConnectorTop;
                        }
                            
                        if (spring.CompareTag("Bottom"))
                        {
                            spring.connectedBody = _rightConnectorBottom;
                        }

                        if (!spring.CompareTag("Bottom") && !spring.CompareTag("Top") && spring.connectedBody is null)
                        {
                            spring.connectedBody = _rightConnectorMiddle;
                        }
                    }
                    
                    foreach (FrictionJoint2D friction in right.GetComponentsInChildren<FrictionJoint2D>())
                    {
                        if (!friction.CompareTag("Bottom") && !friction.CompareTag("Top") && friction.connectedBody is null)
                        {
                            friction.connectedBody = _rightConnectorMiddle;
                        }
                    }
                }

                // Destroy springs that were originally on the deleted segment
                if (segment.CompareTag("reproduce") && segment.GetComponent<Grow>().reproduceNumber is 1)
                {
                    Destroy(segment.GetComponent<Grow>());
                    foreach (SpringJoint2D spring in segment.GetComponentsInChildren<SpringJoint2D>())
                    {
                        if (spring.CompareTag("Top"))
                        {
                            spring.connectedBody = null;
                            spring.enabled = false;
                            Destroy(spring);
                        }

                        if (spring.CompareTag("Bottom"))
                        {
                            spring.connectedBody = null;
                            spring.enabled = false;
                            Destroy(spring);
                        }

                        if (spring.CompareTag("reproduce") && spring.connectedBody.transform.childCount > 0)
                        {
                            spring.connectedBody = null;
                            spring.enabled = false;
                            Destroy(spring);
                        }
                    }

                    foreach (FrictionJoint2D friction in segment.GetComponents<FrictionJoint2D>())
                    {
                        if (!friction.connectedBody.transform.parent != segment)
                        {
                            friction.enabled=false;
                        }
                    }
                }
                
                // Set the starting point for the SoftBody nodes at the left most segment of the right cell
                if (segment.CompareTag("reproduce") && segment.GetComponent<Grow>().reproduceNumber is 5)
                {
                    segment.tag = "right";
                    softBodyRight = segment.gameObject;
                }
                
                // Do the same thing but for the left cell produced
                if (segment.CompareTag("reproduce") && segment.GetComponent<Grow>().reproduceNumber is 1)
                {
                    segment.tag = "left";
                }
                if (segment.GetComponent<EndCapInfo>() != null && segment.GetComponent<EndCapInfo>().side is "left")
                {
                    foreach (SpringJoint2D spring in segment.GetComponents<SpringJoint2D>())
                    {
                        if (spring.connectedBody.transform.childCount > 0 && spring.CompareTag("left"))
                        {
                            var connectedBody = spring.connectedBody;
                            softBodyLeft = connectedBody.gameObject;
                        }
                    }
                }

                // Destroy reproducing segments
                if (segment.CompareTag("reproduce") && (segment.GetComponent<Grow>().reproduceNumber is 2 or 4 or 3))
                {
                    Destroy(segment.gameObject);
                }
                if (segment.CompareTag("left"))
                {
                    newLeft.Add(segment);
                }
                if (segment.CompareTag("right"))
                {
                    newRight.Add(segment);
                }
            }
            
            // Instantiate and set the parents of the new cells
            GameObject newCellRight = Instantiate(_newCell, softBodyRight.transform.position, transform.rotation);
            newCellRight.GetComponent<Reproduction>().isPaused = isPaused;
            newCellRight.GetComponent<SoftBody>().isPaused = isPaused;
            GameObject newCellLeft = Instantiate(_newCell, softBodyLeft.transform.position, transform.rotation);
            newCellLeft.GetComponent<Reproduction>().isPaused = isPaused;
            newCellLeft.GetComponent<SoftBody>().isPaused = isPaused;
            foreach (Transform segment in newLeft)
            {
                segment.SetParent(newCellLeft.transform);
            }
            foreach (Transform segment in newRight)
            {
                segment.SetParent(newCellRight.transform);
            }

            // Set the softbody segments of the new cells
            newCellRight.GetComponent<SoftBody>().newGrowSegment = softBodyRight;
            newCellLeft.GetComponent<SoftBody>().newGrowSegment = softBodyLeft;

            newCellRight.GetComponent<SoftBody>().enabled = true;
            newCellLeft.GetComponent<SoftBody>().enabled = true;
            
            // Destroy the old cell parent
            Destroy(gameObject);

        }
    }
}
