using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StartSimulation : MonoBehaviour
{
    
    private GameObject _newCell;
    
    // Start is called before the first frame update
    void Awake()
    {
        // _newCell = Resources.Load("StartCell") as GameObject;
        //
        // GameObject starCell = Instantiate(_newCell, Vector2.zero + new Vector2(Random.Range(0f,3f),Random.Range(0f,3f)),
        //     Quaternion.Euler(Random.Range(0f,360f),Random.Range(0f,360f),0));
        //
        // Pause pause = FindObjectOfType<Pause>();
        //
        // List<SoftBody> softBodies = starCell.GetComponentsInChildren<SoftBody>().ToList();
        // List<Reproduction> reproductions = starCell.GetComponentsInChildren<Reproduction>().ToList();
        //
        // foreach (SoftBody softBody in softBodies)
        // {
        //     softBody.isPaused = pause;
        // }
        //
        // foreach (Reproduction reproduction in reproductions)
        // {
        //     reproduction.isPaused = pause;
        // }
        
        // Instantiate(_newCell, Vector2.zero, Quaternion.identity);
        
        List<SpringJoint2D> springs = FindObjectsOfType<SpringJoint2D>().ToList();
        List<Rigidbody2D> rigids = FindObjectsOfType<Rigidbody2D>().ToList();

        foreach (SpringJoint2D spring in springs)
        {
            spring.dampingRatio = 1000000f;
            spring.enableCollision = true;

            if (spring.connectedAnchor == Vector2.zero)
            {
                spring.frequency = 0.3f;
            }
            if (spring.connectedBody.transform.childCount < 0)
            {
                spring.frequency = 3f;
            }

            if (spring.connectedBody.transform.childCount < 1 && spring.transform.childCount > 0)
            {
                spring.frequency = 7f;
            }
            
            if (spring.transform.parent.GetComponent<EndCapInfo>() != null
                && spring.connectedBody.transform.parent.GetComponent<EndCapInfo>() != null)
            {
                spring.frequency = 10f;
            }

            // spring.frequency = 10f;

        }
        
        foreach (Rigidbody2D rigid in rigids)
        {
            rigid.drag = 2f;
            rigid.mass = 3f;
            rigid.angularDrag = 0f;
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
