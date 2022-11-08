using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotCollide : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] cells = GameObject.FindGameObjectsWithTag("Center");

        // make sure nodes of two pair cells do not collide with each other
        foreach(GameObject cell in cells)
        {
            if(cell.GetComponent<Reproduce>().PairNumber == gameObject.GetComponent<Reproduce>().PairNumber)
            {
                Physics2D.IgnoreCollision(gameObject.transform.GetComponent<Collider2D>(), cell.transform.GetComponent<Collider2D>(), cell != gameObject);
                foreach (Transform child in transform)
                {
                    Collider2D rigid = child.transform.GetComponent<Collider2D>();
                    foreach (Transform cell_child in cell.transform)
                    {
                        Collider2D rigid_cell = cell_child.transform.GetComponent<Collider2D>();

                        if((child.tag == "Layer2" || child.tag == "Layer1") && (cell_child.tag == "Layer2" || cell_child.tag == "Layer1"))
                        {
                            Physics2D.IgnoreCollision(gameObject.transform.GetComponent<Collider2D>(), rigid_cell, cell != gameObject);
                            Physics2D.IgnoreCollision(rigid, cell.transform.GetComponent<Collider2D>(), cell != gameObject);
                            Physics2D.IgnoreCollision(rigid, rigid_cell, cell != gameObject);
                        }
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
