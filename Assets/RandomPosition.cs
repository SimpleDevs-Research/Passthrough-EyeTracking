using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPosition : MonoBehaviour
{
    public Renderer plane;
    public Camera camera;
    private GameObject[] objectsToRandomize;
    // Start is called before the first frame update
    void Start()
    {

        List<GameObject> foundObjects = new List<GameObject>();
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            if (obj.layer == LayerMask.NameToLayer("Randomize"))
            {
                foundObjects.Add(obj);
            }
        }
        objectsToRandomize = foundObjects.ToArray();
        RandomizePositions();

        /* float length = plane.bounds.size.x;
        float width = plane.bounds.size.z;
        float upperHeight = plane.bounds.center.y + plane.bounds.extents.y;

        Renderer boundingBox = GetComponent<Renderer>();

        transform.position = new Vector3(Random.Range(plane.bounds.min.x, plane.bounds.max.x), upperHeight + boundingBox.bounds.extents.y, Random.Range(plane.bounds.min.z, plane.bounds.max.z)); */
        /* Transform.position.y = upperHeight + boundingBox.bounds.extents.y;

        Transform.position.x = Random.Range(plane.bounds.min.x, plane.bounds.max.x);
        Transform.position.z = Random.Range(plane.bounds.min.z, plane.bounds.max.z); */
        
    }

    void RandomizePositions()
    {
        float upperHeight = plane.bounds.max.y;

        List<Bounds> placedBounds = new List<Bounds>();

        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            if (obj.layer == LayerMask.NameToLayer("Sphere"))
            {
                placedBounds.Add(obj.GetComponent<Renderer>().bounds);
                break;
            }
        }

        foreach (GameObject obj in objectsToRandomize)
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

            bounds = obj.GetComponent<Renderer>().bounds;

            if (renderers.Length > 0)
            {
                foreach (Renderer renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            bounds.extents *= 1.3f;

            bool placed = false;
            int attempts = 0;

            while (!placed && attempts < 100)
            {
                Vector3 randomPosition = new Vector3(
                    Random.Range(plane.bounds.min.x + bounds.extents.x, plane.bounds.max.x - bounds.extents.x),
                    upperHeight + (bounds.extents.y / 1.3f),
                    Random.Range(plane.bounds.min.z + bounds.extents.z, plane.bounds.max.z - bounds.extents.z)
                );

                while (Vector3.Distance(randomPosition + new Vector3(bounds.extents.x, 0, 0), camera.transform.position) <= 0.5f
                || Vector3.Distance(randomPosition - new Vector3(bounds.extents.x, 0, 0), camera.transform.position) <= 0.5f
                || Vector3.Distance(randomPosition + new Vector3(0, 0, bounds.extents.z), camera.transform.position) <= 0.5f
                || Vector3.Distance(randomPosition - new Vector3(0, 0, bounds.extents.z), camera.transform.position) <= 0.5f)
                {
                    randomPosition = new Vector3(
                        Random.Range(plane.bounds.min.x + bounds.extents.x, plane.bounds.max.x - bounds.extents.x),
                        upperHeight + (bounds.extents.y / 1.3f),
                        Random.Range(plane.bounds.min.z + bounds.extents.z, plane.bounds.max.z - bounds.extents.z)
                    );
                    attempts++;
                }
                
                bounds.center = randomPosition;
                bool intersects = false;

                foreach (var placedBound in placedBounds)
                {
                    if (placedBound.Intersects(bounds))
                    {
                        intersects = true;
                        break;
                    }
                }

                if (!intersects)
                {
                    obj.transform.position = randomPosition;
                    placedBounds.Add(bounds);
                    placed = true;
                }

                attempts++;
            }

            if (!placed)
            {
                Debug.Log("Max attempts reached for object " + obj.name);
            }
        }

        foreach (GameObject obj in objectsToRandomize)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            Bounds bounds = obj.GetComponent<Renderer>().bounds;
            if (renderers.Length > 0)
            {
                foreach (Renderer renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
            
            if ((bounds.min.y - upperHeight) > 0.1f)
            {
                obj.transform.position = new Vector3(obj.transform.position.x, upperHeight, obj.transform.position.z);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
