using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowCell : MonoBehaviour
{
    private float Growthrate = 5000f;
    float start_time;
    Rigidbody2D cell_center;
    float _old_time;
    Vector2 _old_vel;
    float total_mass;

    // Start is called before the first frame update
    void Awake()
    {
        start_time = Time.time;
        cell_center = gameObject.GetComponent<Rigidbody2D>();
        total_mass = cell_center.mass;
        _old_vel = cell_center.velocity;
        _old_time = Time.time;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float relative_time = Time.time-start_time;
        total_mass = cell_center.mass;

        foreach (Transform child in transform)
        {
            Rigidbody2D rigid = child.GetComponent<Rigidbody2D>();
            Vector2 radial = child.transform.localPosition;

            if(child.tag == "Layer2" || child.tag == "Layer1")
            {
                total_mass += rigid.mass;
                if(relative_time < 3)
                {
                    rigid.AddForce(radial.normalized * Growthrate * relative_time);
                }
                else
                {
                    rigid.AddForce(radial.normalized * Growthrate * 3);
                }

            }
        }

        // Stop the force from acting on the entire cell
        // Vector2 new_vel = cell_center.velocity;
        // try
        // {
        //     cell_center.AddForce(-1.5f * new_vel*total_mass, ForceMode2D.Impulse);
        // }
        // catch
        // {
        //     Debug.Log("Cannot calculate force... adding 0 force");
        //     cell_center.AddForce(Vector2.zero);
        // }

        // _old_time = Time.time;
        // _old_vel = new_vel;
    }
}
