using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class QuadTree2D<T>
{
    QuadTree2D<T> northwest;
    QuadTree2D<T> northeast;
    QuadTree2D<T> southwest;
    QuadTree2D<T> southeast;

    private int capacity = 4;
    private Bounds bounds;
    private bool divided;

    Dictionary<Vector3, T> points = new();

    public QuadTree2D(Bounds bounds, int capacity)
    {
        this.bounds = bounds;
        this.capacity = capacity;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(this.bounds.center, this.bounds.size);

        if (divided)
        {
            northwest.OnDrawGizmos();
            northeast.OnDrawGizmos();
            southwest.OnDrawGizmos();
            southeast.OnDrawGizmos();
        }
    }
    
    public bool Insert(Vector3 point, T data)
    {
        if (!this.bounds.Contains(point))
            return false;

        if (points.Count < capacity)
        {
            points.Add(point, data);
            return true;
        }
        
        if (!divided)
            Subdivide();

        return this.northwest.Insert(point, data)
          || this.northeast.Insert(point, data)
          || this.southwest.Insert(point, data)
          || this.southeast.Insert(point, data);
    }

    public void Subdivide()
    {
        divided = true;

        Bounds northwest_bounds = new Bounds(
            new Vector2(
                this.bounds.center.x - this.bounds.size.x/ 4,
                this.bounds.center.y + this.bounds.size.y / 4
            ),
            new Vector2(
               this.bounds.size.x / 2,
               this.bounds.size.y / 2
            )
        );

        northwest = new QuadTree2D<T>(northwest_bounds, this.capacity);

        Bounds northeast_bounds = new Bounds(
            new Vector2(
                this.bounds.center.x + this.bounds.size.x / 4,
                this.bounds.center.y - this.bounds.size.y / 4
            ),
            new Vector2(
                this.bounds.size.x / 2,
                this.bounds.size.y / 2
            )
        );

        northeast = new QuadTree2D<T>(northeast_bounds, this.capacity);

        Bounds southwest_bounds = new Bounds(
            new Vector2(
                this.bounds.center.x - this.bounds.size.x / 4,
                this.bounds.center.y - this.bounds.size.y / 4
            ),
            new Vector2(
                this.bounds.size.x / 2,
                this.bounds.size.y / 2
            )
        );

        southwest = new QuadTree2D<T>(southwest_bounds, this.capacity);

        Bounds southeast_bounds = new Bounds(
            new Vector2(
               this.bounds.center.x + this.bounds.size.y / 4,
               this.bounds.center.y + this.bounds.size.y / 4
            ),
            new Vector2(
                this.bounds.size.x / 2,
                this.bounds.size.y / 2
            )
        );

        southeast = new QuadTree2D<T>(southeast_bounds, this.capacity);
    }
    
    public Dictionary<Vector3, T>
        Query(Bounds range, Dictionary<Vector3, T> found =  null)
    {
        if (found == null)
        {
            found = new Dictionary<Vector3, T>();
        }

        if (!range.Intersects(this.bounds))
        {
            return found;
        }

        if (this.divided)
        {
            this.northwest.Query(range, found);
            this.northeast.Query(range, found);
            this.southwest.Query(range, found);
            this.southeast.Query(range, found);
            
            return found;
        }

        foreach (var p in this.points) {
            if (range.Contains(p.Key))
            {
                if (!found.ContainsKey(p.Key))
                {
                    found.Add(p.Key, p.Value);
                }
            }
        }
 
        return found;
    }

}
