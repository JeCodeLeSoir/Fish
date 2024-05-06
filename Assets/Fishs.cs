using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Fishs : MonoBehaviour
{
    static Fishs instance;

    [SerializeField] ParticleSystem _particleSystem;
    [SerializeField] int count;
    [SerializeField] Transform test;

    Vector3 minViewport;
    Vector3 maxViewport;

    List<Fish> FishList = new List<Fish>();

    [Range(0, 10)]
    public float maxSpeed = 1f;

    [Range(0.1f, 10)]
    public float neighborhoodRadius = 3f;

    [Range(0, 3)]
    public float separationAmount = 1f;

    [Range(0, 3)]
    public float cohesionAmount = 1f;

    [Range(0, 3)]
    public float alignmentAmount = 1f;

    public class Fish
    {
 
        public ParticleSystem.Particle particle;
        
        public Vector2 position;
        public float size = 1f / 5f;
        public float rotation;
 
        private Vector2 velocity;

        public float maxSpeed = 1f;
        public float neighborhoodRadius = 1f;
        public float separationAmount = 1f;
        public float cohesionAmount = 1f;
        public float alignmentAmount = 1f;

        public void SetNeighbors(List<Fish> fishs)
        {
            m_neighbors = fishs;
        }

        private List<Fish> m_neighbors = new();

        public bool Alpha { get; internal set; }

        private void LookDir(Vector2 velocity)
        {
            rotation = -Vector2.SignedAngle(Vector2.up, velocity);
        }

        private void LookPos(Vector2 pos)
        {
            rotation = -Vector2.SignedAngle(Vector2.up, pos);
        }

        public Fish()
        {
            particle = new ParticleSystem.Particle();
        }

        public void Ini()
        {
            particle.startColor = Alpha ? Color.red : Color.white;
            particle.startSize = Alpha ? size * 2 : size;
             
            this.velocity = Up();
            speed = maxSpeed;
        }

        bool dirChange = false;
        public void ResetDirChange() => dirChange = false;

        float speed;
        float changeSpeedTime = 0;


        float Range(System.Random random, float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }

        internal void Update(float delta)
        {
            maxSpeed = instance.maxSpeed;
            neighborhoodRadius = instance.neighborhoodRadius;
            separationAmount = instance.separationAmount;
            cohesionAmount = instance.cohesionAmount;
            alignmentAmount = instance.alignmentAmount;

            System.Random random = new System.Random();
            
            if (changeSpeedTime> 1.5f)
            {
                speed = Range(random, 0.1f, maxSpeed);
                changeSpeedTime = 0;
            }
            else
            {
                changeSpeedTime += delta;
            }

            if (!dirChange)
            {
                if (!(this.position.x > instance.minViewport.x && this.position.x < instance.maxViewport.x &&
                this.position.y > instance.minViewport.y && this.position.y < instance.maxViewport.y))
                {
                    this.velocity = (-this.velocity.normalized) + (
                      (float) random.NextDouble() > 1 ? Left() : Right());
                    this.velocity.Normalize();
                    
                    dirChange = true;
                    instance.ResetDirChange(this, 0.1f);
                }
            }

            if ((this.position.x > instance.minViewport.x && this.position.x < instance.maxViewport.x &&
                this.position.y > instance.minViewport.y && this.position.y < instance.maxViewport.y))
            {
                var avoidance = Avoidance(m_neighbors);
                var alignment = Cohesion(m_neighbors);
                var cohesion = Alignment(m_neighbors);

                velocity = alignmentAmount * alignment + cohesionAmount * cohesion + separationAmount * avoidance;

                if (velocity.magnitude < 0.01f)
                    velocity = this.Up();
            }

            velocity = LimitMagnitude(velocity, maxSpeed);

            this.position += velocity * (delta * speed);
             
            LookDir(this.velocity);
            
            UpdateParticle();
        }

        public float frameRate = 10.0f;
        
        public void UpdateParticle()
        {
            particle.position = position;
            particle.rotation = rotation;
        }
 
        private Vector2 Cohesion(IEnumerable<Fish> list)
        {
            var sumPositions = Vector2.zero;
            
            foreach (var boid in list)
            {
                sumPositions += boid.position;
            }

            var average = sumPositions / list.Count();
            var direction = average - position;

            return direction.normalized;
        }

        private Vector2 Alignment(IEnumerable<Fish> list)
        {
            var velocity = Vector2.zero;

            foreach (var boid in list)
            {
                velocity += boid.velocity;
            }

            velocity /= list.Count();

            return velocity.normalized;
        }

        private Vector2 Avoidance(IEnumerable<Fish> list)
        {

            var direction = Vector2.zero;

            foreach (var boid in list)
            {
                var difference = this.position - boid.position;
                direction += difference.normalized / difference.magnitude;
            }


            direction /= list.Count();

            return direction.normalized;
        }

        private Vector2 LimitMagnitude(Vector2 baseVector, float maxMagnitude)
        {
            if (baseVector.sqrMagnitude > maxMagnitude * maxMagnitude)
            {
                baseVector = baseVector.normalized * maxMagnitude;
            }
            return baseVector;
        }
         
        private Vector2 Up()
        {
            float radianAngle = Mathf.Deg2Rad * rotation;
           
            float x = Mathf.Sin(radianAngle);
            float y = Mathf.Cos(radianAngle);

            return new Vector2(x, y);
        }

        private Vector2 Down()
        {
            return -Up();
        }

        private Vector2 Left()
        {
            float radianAngle = Mathf.Deg2Rad * rotation;
 
            float x = Mathf.Cos(radianAngle);
            float y = Mathf.Sin(radianAngle);

            return new Vector2(-x, y);
        }

        private Vector2 Right()
        {
            return -Left();
        }
    }

    IEnumerator cResetDirChange(Fish fish, float timer) {
        
        yield return new WaitForSeconds(timer);
        
        fish.ResetDirChange();
    }

    Queue<Action> actions = new();
    public void ResetDirChange(Fish fish, float timer)
    {
        actions.Enqueue(() => StartCoroutine(cResetDirChange(fish, timer)));
    }

    private void Start()
    {
        instance = this;

        minViewport = Camera.main.ViewportToWorldPoint(Vector3.zero);
        maxViewport = Camera.main.ViewportToWorldPoint(Vector3.one);

        for (int i = 0; i < count; i++)
        {
            var randX = UnityEngine.Random.Range(minViewport.x, maxViewport.x);
            var randY = UnityEngine.Random.Range(minViewport.y, maxViewport.y);
            var randR = UnityEngine.Random.Range(-90, 90);

            var f = new Fish();

            f.Alpha = UnityEngine.Random.Range(1, count+3) > count;
            f.position = new Vector3(randX, randY, 0);
            f.rotation = randR;

            f.Ini();

            FishList.Add(f);
        }
 
    }
 
    QuadTree2D<Fish> quadTree2D;

    private void Update()
    {
        for (int i = 0; i < actions.Count; i++)
            actions.Dequeue()?.Invoke();
        
        var delta = Time.deltaTime;

        quadTree2D = new QuadTree2D<Fish>(
            new Bounds(Vector2.zero, new Vector2(100, 100)), 4);

        for (int i = 0; i < FishList.Count; i++)
        {
            var fish = FishList[i];
            quadTree2D.Insert(fish.position, fish);
        }

        Parallel.For(0, FishList.Count, (i) =>
        {
            var fish = FishList[i];
            fish.Update(delta);

            //quadTree2D.Insert(fish.position, fish);
        });

        //for (int i = 0; i < FishList.Count; i++)
        Parallel.For(0, FishList.Count, (i) =>
            {
                var fish = FishList[i];

                Bounds b = new Bounds(fish.position,
                    new Vector2(neighborhoodRadius, neighborhoodRadius));

                List<Fish> neighbors = quadTree2D.Query(b).Select((e) =>
                {
                    return e.Value;
                }).ToList();

                if (neighbors.Contains(fish))
                    neighbors.Remove(fish);

                fish.SetNeighbors(neighbors);
            });
 
        _particleSystem.SetParticles(ToParticles(FishList));
    }

    private void OnDrawGizmos()
    {
        if (quadTree2D != null)
            quadTree2D.OnDrawGizmos();
    }

    private ParticleSystem.Particle[] ToParticles(List<Fish> fishList)
    {
        var particles = new ParticleSystem.Particle[fishList.Count];

        for (int i = 0; i < fishList.Count; i++)
        {
            particles[i] = fishList[i].particle;
        }

        return particles;
    }
}
