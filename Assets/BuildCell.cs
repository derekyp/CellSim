using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class BuildCell : MonoBehaviour
{
    // Start is called before the first frame update
    // public GameObject cell_center;
    void Start()
    {
        // ConstructCell(new Vector2(5, 4));
        MakeCell(new Vector2(0, -4));
        // ConstructCell(new Vector2(-5, 4));
        // ConstructCell(new Vector2(-2, 0));
        // ConstructCell(new Vector2(2, 0));
        // ConstructCell(new Vector2(3, 10));
        // ConstructCell(new Vector2(-3, 10));
    }

    // Update is called once per frame
    void Update()
    {
        // Build while loop to check if the radius is a certain value then build new layer
    }

    #region privateMethods

    // puts two cells together to allow division in the future
    private void MakeCell(Vector2 center_coord)
    {
        int pair = Random.Range(0, 100);
        ConstructCellObject(center_coord, pair);
        ConstructCellObject(center_coord, pair);
    }

    // makes the the object that constitutes an individual cell
    private void ConstructCellObject(Vector2 center_coord, int pair)
    {
        // Build the cell center and add rigid body
        var cell_center = Resources.Load("CellCenter") as GameObject;
        var Center = Instantiate(cell_center, center_coord, Quaternion.identity);
        Center.GetComponent<Reproduce>().PairNumber = pair;

        // Place down n equally spaced cell nodes at radius r and store them in a list
        // The first entry in the list is the node at 360/n degrees and subsequently added with increasing degree
        var Nodes = new List<GameObject>();
        int  n = 10;
        float r = 1;
        var cell_node = Resources.Load("CellNode") as GameObject;
        for (int i = 0; i < n; i++) 
        {
            Nodes.Add(Instantiate(cell_node, center_coord - new Vector2(r*Mathf.Cos(((i+1) * 2*Mathf.PI) / n), r*Mathf.Sin(((i+1) * 2*Mathf.PI) / n)), Quaternion.identity, Center.transform));
            Nodes[i].tag = "Layer1";
        }

        // Build the second layer of cell nodes
        for (int j = 0; j < 2*n; j++) 
        {
            Nodes.Add(Instantiate(cell_node, center_coord - new Vector2(2*r*Mathf.Cos(((j+1) * 2*Mathf.PI) / (2*n)), 2*r*Mathf.Sin(((j+1) * 2*Mathf.PI) / (2*n))), Quaternion.identity, Center.transform));
            Nodes[n+j].tag = "Layer2";
        }

        // Connect all the 1st layer nodes to the cell center.
        // Note the central springs will be used for outer layer nodes as well later.
        // They will be the ones that connect towards the inner layer(towards center)
        foreach(GameObject node in Nodes)
        {
            SpringJoint2D central_spring = node.AddComponent<SpringJoint2D>() as SpringJoint2D;
            central_spring.frequency = 5;
            if(node.tag == "Layer1")
                central_spring.connectedBody = Center.GetComponent<Rigidbody2D>();
        }

        // Connect all first layer nodes to their neighbors with a spring connector
        for (int i = 0; i < n; i++) 
        {
            SpringJoint2D neighbor_spring_1 = Nodes[i].AddComponent<SpringJoint2D>() as SpringJoint2D;
            SpringJoint2D neighbor_spring_2 = Nodes[i].AddComponent<SpringJoint2D>() as SpringJoint2D;

            neighbor_spring_1.frequency = 3;
            neighbor_spring_2.frequency = 3;

            if(i == 0)
            {
                neighbor_spring_1.connectedBody = Nodes[n-1].GetComponent<Rigidbody2D>();
                neighbor_spring_2.connectedBody = Nodes[i+1].GetComponent<Rigidbody2D>();
            }
            
            else if(i == n-1)
            {
                neighbor_spring_1.connectedBody = Nodes[i-1].GetComponent<Rigidbody2D>();
                neighbor_spring_2.connectedBody = Nodes[0].GetComponent<Rigidbody2D>();
            }

            else
            {
                neighbor_spring_1.connectedBody = Nodes[i-1].GetComponent<Rigidbody2D>();
                neighbor_spring_2.connectedBody = Nodes[i+1].GetComponent<Rigidbody2D>();
            }
        }

        // Connect all first layer nodes to their neighbors with a distance connector
        for (int i = 0; i < n; i++) 
        {
            DistanceJoint2D neighbor_distance_1 = Nodes[i].AddComponent<DistanceJoint2D>() as DistanceJoint2D;
            DistanceJoint2D neighbor_distance_2 = Nodes[i].AddComponent<DistanceJoint2D>() as DistanceJoint2D;

            if(i == 0)
            {
                neighbor_distance_1.connectedBody = Nodes[n-1].GetComponent<Rigidbody2D>();
                neighbor_distance_2.connectedBody = Nodes[i+1].GetComponent<Rigidbody2D>();
            }

            else if(i == n-1)
            {
                neighbor_distance_1.connectedBody = Nodes[i-1].GetComponent<Rigidbody2D>();
                neighbor_distance_2.connectedBody = Nodes[0].GetComponent<Rigidbody2D>();
            }

            else
            {
                neighbor_distance_1.connectedBody = Nodes[i-1].GetComponent<Rigidbody2D>();
                neighbor_distance_2.connectedBody = Nodes[i+1].GetComponent<Rigidbody2D>();
            }
        }

        // Experimenting with this *not to be used*

        // Connect all first layer nodes to all other first layer nodes with a spring connector
        // for(int i = 0; i < n; i++) 
        // {
        //     for (int j =0; j < n && j != i; j++)
        //     {
        //         SpringJoint2D layer_spring = Nodes[i].AddComponent<SpringJoint2D>() as SpringJoint2D;
        //         layer_spring.frequency = 1;
        //         layer_spring.connectedBody = Nodes[j].GetComponent<Rigidbody2D>();
        //     }
        // }

        // Connect all 2nd layer nodes to respective first layer nodes.
        // The ones directly radially out will connect one to one with first layer nodes.
        // The ones between each first layer node will have two spring connectors connecting to both it is inbetween. 
        SpringJoint2D[] springs;
        for (int i = n, j=n; i < 3*n; i++) 
        {
            // Connect nodes directly out from first layer nodes to respective nodes
            if((i+1)%2 == 0)
            {
                Nodes[i].GetComponent<SpringJoint2D>().connectedBody = Nodes[j%n].GetComponent<Rigidbody2D>();
                j++;
                Nodes[i].GetComponent<SpringJoint2D>().frequency = 5;
            }
            // Now for the inbetween nodes
            else if(i==n)
            {
                Nodes[i].GetComponent<SpringJoint2D>().connectedBody = Nodes[j%n].GetComponent<Rigidbody2D>();
                Nodes[i].AddComponent<SpringJoint2D>().connectedBody = Nodes[n-1].GetComponent<Rigidbody2D>();
                springs = Nodes[i].GetComponents<SpringJoint2D>();
                foreach(SpringJoint2D spring in springs)
                {
                    spring.frequency = 5;
                }
            }
            else
            {
                Nodes[i].GetComponent<SpringJoint2D>().connectedBody = Nodes[j%n].GetComponent<Rigidbody2D>();
                Nodes[i].AddComponent<SpringJoint2D>().connectedBody = Nodes[(j%n)-1].GetComponent<Rigidbody2D>();
                springs = Nodes[i].GetComponents<SpringJoint2D>();
                foreach(SpringJoint2D spring in springs)
                {
                    spring.frequency = 5;
                }
            }
        }

// Connect all second layer nodes to their neighbors with a spring connector
        for (int i = n; i < 3*n; i++) 
        {
            SpringJoint2D neighbor_spring_1 = Nodes[i].AddComponent<SpringJoint2D>() as SpringJoint2D;
            SpringJoint2D neighbor_spring_2 = Nodes[i].AddComponent<SpringJoint2D>() as SpringJoint2D;

            neighbor_spring_1.frequency = 3;
            neighbor_spring_2.frequency = 3;

            if(i == n)
            {
                neighbor_spring_1.connectedBody = Nodes[n-1].GetComponent<Rigidbody2D>();
                neighbor_spring_2.connectedBody = Nodes[i+1].GetComponent<Rigidbody2D>();
            }
            
            else if(i == 3*n-1)
            {
                neighbor_spring_1.connectedBody = Nodes[i-1].GetComponent<Rigidbody2D>();
                neighbor_spring_2.connectedBody = Nodes[n].GetComponent<Rigidbody2D>();
            }

            else
            {
                neighbor_spring_1.connectedBody = Nodes[i-1].GetComponent<Rigidbody2D>();
                neighbor_spring_2.connectedBody = Nodes[i+1].GetComponent<Rigidbody2D>();
            }
        }

        // Connect all the 2nd layer nodes to the cell center.
        foreach(GameObject node in Nodes)
        {
            SpringJoint2D central_spring = node.AddComponent<SpringJoint2D>() as SpringJoint2D;
            central_spring.frequency = 5;
            if(node.tag == "Layer2")
            {
                central_spring.connectedBody = Center.GetComponent<Rigidbody2D>();
            }
            else
            {
                Destroy (central_spring);
            }

        }

        // Connect all second layer nodes to their neighbors with a distance connector
        for (int i = n; i < 3*n; i++) 
        {
            DistanceJoint2D neighbor_distance_1 = Nodes[i].AddComponent<DistanceJoint2D>() as DistanceJoint2D;
            DistanceJoint2D neighbor_distance_2 = Nodes[i].AddComponent<DistanceJoint2D>() as DistanceJoint2D;

            if(i == n)
            {
                neighbor_distance_1.connectedBody = Nodes[n-1].GetComponent<Rigidbody2D>();
                neighbor_distance_2.connectedBody = Nodes[i+1].GetComponent<Rigidbody2D>();
            }

            else if(i == 3*n-1)
            {
                neighbor_distance_1.connectedBody = Nodes[i-1].GetComponent<Rigidbody2D>();
                neighbor_distance_2.connectedBody = Nodes[n].GetComponent<Rigidbody2D>();
            }

            else
            {
                neighbor_distance_1.connectedBody = Nodes[i-1].GetComponent<Rigidbody2D>();
                neighbor_distance_2.connectedBody = Nodes[i+1].GetComponent<Rigidbody2D>();
            }
        }
        
        // Create the sprite that will visually show the cell
        var sprite = Resources.Load("Closed Sprite Shape") as GameObject;
        var skin = Instantiate(sprite, center_coord, Quaternion.identity, Center.transform);
        SpriteShapeController controller = skin.GetComponent<SpriteShapeController>();
        Spline spline = controller.spline;
        spline.Clear();

        var secondlayer = new List<Transform>();
        float tangentlength = (5f*(.4f*r))/(n);

        for (int i = n; i < 3*n; i++)
        {
            Quaternion rotation = Nodes[i].transform.rotation;
            float radius = Nodes[i].GetComponent<CircleCollider2D>().radius;
            Vector2 pos = Nodes[i].transform.localPosition;
            Vector2 pos_norm = pos.normalized;

            spline.InsertPointAt(i-n, (pos+radius*pos_norm));
            spline.SetTangentMode(i-n, ShapeTangentMode.Continuous);
            
            Vector2 _newRt = Vector2.Perpendicular(pos_norm)*tangentlength;
            Vector2 _newLt = Vector2.zero - (_newRt);

            spline.SetRightTangent(i-n, _newRt);
            spline.SetLeftTangent(i-n, _newLt);
            secondlayer.Add(Nodes[i].transform);
        }
        controller.RefreshSpriteShape();
        secondlayer.Add(Center.transform);

        Center.GetComponent<SoftBody>().spriteShape = controller;
        Center.GetComponent<SoftBody>().points = secondlayer;
        Center.GetComponent<SoftBody>().enabled = true;
    }

    #endregion privateMethods
}
