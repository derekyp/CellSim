using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellInformation : MonoBehaviour
{
    public int reproduceTot;
    
    public float startTime;

    public int midSectionCount;

    private int _count;
    
    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        _count = 0;
        foreach (Transform segment in transform)
        {
            Grow growth = segment.GetComponent<Grow>();
            if(growth != null)
            {
                if (growth.primary)
                {
                    _count = growth.midSectionNumber - 0;
                }
        
                if (!growth.primary)
                {
                    _count++;
                }
            }
        }
        // Debug.Log(_count);
        
        midSectionCount = _count;
    }
}
