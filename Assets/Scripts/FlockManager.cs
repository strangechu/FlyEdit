using UnityEngine;
using System.Collections;

public class FlockManager : MonoBehaviour
{

    public Boid boidPrefab;
    public int numberOfBoids;
    public float alignmentWeight;
    public float cohesionWeight;
    public float separationWeight;
    public float targetWeight;
    public GameObject target_object;
    public int randomSeed = 5566;

    private Boid[] boids;

    public Boid getBoid(int i)
    {
        if (i >= numberOfBoids)
            return null;
        return boids[i];
    }

    void Start ()
    {
        Random.InitState(randomSeed);
        boids = new Boid[numberOfBoids];

        for (int i = 0; i < numberOfBoids; i++)
        {
            boids[i] = Instantiate(boidPrefab, transform.position, Quaternion.identity) as Boid;
            boids[i].transform.parent = transform;
        }
	}
	
	
	void FixedUpdate ()
    {
        for (int i = 0; i < numberOfBoids; i++)
        {
            Boid boid = boids[i];
            if (boid != null && boid.thisRigidbody != null)
            {
                Vector3 alignment = align(boid) * alignmentWeight;
                alignment = alignment * Time.deltaTime;
                Vector3 cohesion = cohere(boid) * cohesionWeight * Time.deltaTime;
                Vector3 separation = separate(boid) * separationWeight * Time.deltaTime;
                Vector3 target = follow(boid) * targetWeight * Time.deltaTime;
                if (boid.debug)
                {
                    boid.showAlignmentDebug(alignment);
                    boid.showCohesionDebug(cohesion);
                    boid.showSeparationDebug(separation);
                }

                if (Vector3.Distance(boid.transform.position, target_object.transform.position) < 10.0f)
                    target = Vector3.zero;

                boid.thisRigidbody.velocity += (alignment + cohesion + separation + target) * 0.4f;

                //boid.thisRigidbody.AddForce(align(boid) * alignmentWeight);
                //boid.thisRigidbody.AddForce(cohere(boid) * cohesionWeight);
                //boid.thisRigidbody.AddForce(separate(boid) * separationWeight);
            }
        }
	}

    private Vector3 align(Boid boid)
    {
        Vector3 velocity = Vector3.zero;
        int count = 0;
        for (int i = 0; i < numberOfBoids; i++)
        {
            float distance = Vector3.Distance(boids[i].transform.localPosition, boid.transform.localPosition);
			if (distance > 0 && distance < boid.neighborRadius)
			{
				velocity += boids[i].thisRigidbody.velocity;
				count++;
			}
        }
        if (count > 0)
        {
            Vector3 ret = (velocity / (numberOfBoids - 1)).normalized;
            return ret;
        }
        else
        {
            return Vector3.zero;
        }
    }

    private Vector3 cohere (Boid boid)
    {
        Vector3 centerOfMass = Vector3.zero;
        int count = 0;
        for (int i = 0; i < numberOfBoids; i++)
        {
            float distance = Vector3.Distance(boids[i].transform.localPosition, boid.transform.localPosition);
			if (distance > 0 && distance < boid.neighborRadius)
			{
				centerOfMass += boids[i].transform.localPosition;
				count++;
			}
        }
        if (count > 0)
        {
            return ((centerOfMass / (numberOfBoids - 1)) - boid.transform.localPosition).normalized;
        }
        else
        {
            return Vector3.zero;
        }
    }

    private Vector3 separate (Boid boid)
    {
        Vector3 velocity = Vector3.zero;
        int count = 0;
        for (int i = 0; i < numberOfBoids; i++)
        {
            float distance = Vector3.Distance(boids[i].transform.localPosition, boid.transform.localPosition);
			if (distance > 0 && distance < boid.desiredSeparation)
			{
				velocity -= (boids[i].transform.localPosition - boid.transform.localPosition).normalized / distance;
				count++;
			}
        }
        if (count > 0)
        {
            return (velocity / (numberOfBoids - 1)).normalized;
        }
        else
        {
            return Vector3.zero;
        }
    }

    private Vector3 follow (Boid boid)
    {
        Vector3 velocity = Vector3.zero;
        return (target_object.transform.position - boid.transform.position).normalized;
    }
}
