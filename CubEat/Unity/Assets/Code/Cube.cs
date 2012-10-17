using UnityEngine;
using System.Collections;

public class Cube : MonoBehaviour
{
    public int Layer;// { get; set; }
    public int NumberOnLayer;// { get; set; }

    private bool _isEmpty = true;
    public bool IsEmitting;// { get; set; }
    public bool IsInPlayedTime;// { get; set; }
    public bool IsOnMeasure;// { get; set; }

    public bool IsEmpty 
    {
        get
        {
            return _isEmpty;
        }
        set
        {
            _isEmpty = value;
            if (_isEmpty)
                this.gameObject.renderer.material.color = Color.blue;
            else
                this.gameObject.renderer.material.color = Color.white;
        }
    }



    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnMouseDown()
    {
    }

    public void OnMouseUp()
    {
        IsEmpty = !IsEmpty;
    }
}
