using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reproduce : MonoBehaviour
{
    #region Fields
    [SerializeField]
    public int PairNumber;
    #endregion

    public List<float> inside;
    public bool entered;
    private bool should_split;
    private float creation_time;
    private Rigidbody2D rigid_self;

    private BuildCellTest BuildCellTest_Instance;

    // Start is called before the first frame update
    void Awake()
    {
        creation_time = Time.time;
        should_split = true;
        inside = new List<float>();
        float x = Random.Range(0,10f);
        float y = Random.Range(0,10);
        inside.Add(1f);
        rigid_self = gameObject.GetComponent<Rigidbody2D>();
        rigid_self.AddForce(new Vector2(x, y));


        BuildCellTest_Instance = GameObject.FindObjectOfType<BuildCellTest>();
    }
        

    // Update is called once per frame
    void FixedUpdate()
    {
        // GameObject[] cells = GameObject.FindGameObjectsWithTag("Center");
        float total_mass =  gameObject.GetComponent<Rigidbody2D>().mass;
        entered = false;

        List<GameObject> CentersToRemove = new List<GameObject>();

        foreach(GameObject cell in BuildCellTest_Instance.Centers)
        {
            Vector2 sep;

            try
            {
                sep = (cell.transform.position - gameObject.transform.position);
            }
            catch (MissingReferenceException)
            {
                
                continue;
            }

            Reproduce reproduce = cell.GetComponent<Reproduce>();

            // Debug.Log(sep.magnitude);
            // Debug.Log(Mathf.Max(inside.ToArray()));
            if((reproduce.PairNumber == this.PairNumber) && (sep.magnitude < 4f) && (reproduce.entered == false))
            {
                // Mathf.Max(inside.ToArray())+1f
                inside.Clear();
                if(Time.time - creation_time > 1)
                {
                    rigid_self.AddForce(-sep.normalized * 100f * total_mass, ForceMode2D.Impulse);
                }
            }
            // Debug.Log(Time.time - creation_time);

            // Tells the cell to split once they are far enough apart
            if((reproduce.PairNumber == this.PairNumber) && (sep.magnitude >= Mathf.Max(inside.ToArray()) + 1) && (should_split == true) && (Time.time - creation_time > 5f))
            {
                should_split = false;
                // int pair = Random.Range(0, 100);
                // gameObject.GetComponent<Reproduce>().PairNumber = pair;
                Vector2 center_coord = gameObject.transform.position;
                Vector2 center_coord_other = cell.transform.position;

                CentersToRemove.Add(cell);
                CentersToRemove.Add(gameObject);

                Destroy(gameObject);
                Destroy(cell);

                BuildCellTest_Instance.MakeCell(center_coord);
                BuildCellTest_Instance.MakeCell(center_coord_other);

            }
        }

        foreach(GameObject center in CentersToRemove)
        {
            BuildCellTest_Instance.Centers.Remove(center);
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
