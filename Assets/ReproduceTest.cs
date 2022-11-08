using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReproduceTest : MonoBehaviour
{
    #region Fields
    [SerializeField]
    public int PairNumber;
    #endregion

    private List<float> inside;

    // Start is called before the first frame update
    void Awake()
    {
        inside = new List<float>();
        float x = Random.Range(0,0.1f);
        float y = Random.Range(0,0.1f);
        inside.Add(1f);
        gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(x, y));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Search through other cells to find the one with the same pair number and apply a force opposite to each other
        GameObject[] cells = GameObject.FindGameObjectsWithTag("Center");
        float total_mass =  gameObject.GetComponent<Rigidbody2D>().mass;

        foreach(GameObject cell in cells)
        {
            Vector2 sep = (cell.transform.position - gameObject.transform.position);

            // Be sure to have the force pushing the cells apart only act if they are overlapping.
            if((cell.GetComponent<Reproduce>().PairNumber == gameObject.GetComponent<Reproduce>().PairNumber) && (sep.magnitude < Mathf.Max(inside.ToArray()) + 1))
            {
                inside.Clear();
                gameObject.GetComponent<Rigidbody2D>().AddForce(-sep.normalized * 1 * total_mass, ForceMode2D.Impulse);
            }
        }
    }

    #region privateMethods

    // private bool IsInside() 
    // {
    //     for ( int i = 0; i < inside.Count; ++i ) 
    //     {
    //         if ( inside[i] == true ) 
    //         {
    //             return true;
    //         }
    //     }
    
    //     return false;
    // }

    #endregion
}
