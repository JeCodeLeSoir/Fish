using UnityEngine;

public class Test : MonoBehaviour
{
    QuadTree2D<object> quadTree2D;
    private void Start()
    {
        quadTree2D  = 
        new QuadTree2D<object>(new Bounds(Vector2.zero, Vector2.one * 10f), 1);
        //quadTree2D.Subdivide();
    }

    private void Update()
    {
        Vector2 p = new Vector2(transform.position.x, transform.position.y);
        
        quadTree2D?.Insert(p, null);
    }

    private void OnDrawGizmos()
    {
        quadTree2D?.OnDrawGizmos();
    }
}
