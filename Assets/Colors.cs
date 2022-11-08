using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colors : MonoBehaviour
{
    private List<Color> _colors = new List<Color>();
    // Start is called before the first frame update
    void Start()
    {
        _colors = new List<Color>();
    }

    public void addColor(Color color)
    {
        _colors.Add(color);
    }

    public bool alreadyExists(Color color)
    {
        List<Color> colors = _colors;

        if (_colors.Contains(color))
        {
            return true;
        }
        else
        {
            return false;   
        }
    }
}
