using UnityEngine;

public class DetectDests : MonoBehaviour
{   // Update is called once per frame
    [SerializeField] private LayerMask tileLayer; // Highly recommended to avoid hitting yourself!

    [SerializeField] private GameObject destUI;

    [SerializeField] private GameObject destObject;


    void Awake()
    {
        destUI = GameObject.FindWithTag("DestUI");
        destUI.SetActive(false);
    }

    void Update()
    {
        // 1. Run the physics check exactly ONCE this frame and save the answer
        bool isOverDestination = DetectedDest();

        // 2. Use a simple if/else structure
        if (isOverDestination)
        {        
            // Actual UI logic will go here later
            destUI.SetActive(true);
        }
        else
        {
            // Actual UI logic will go here later
            destUI.SetActive(false);
        }
    }        

    bool DetectedDest()
    {
        // 1. Create the downward ray
        Ray ray = new Ray(transform.position, Vector3.down);
        
        // 2. Cast the ray with a specific maximum distance and layer mask
        if (Physics.Raycast(ray, out RaycastHit hit, tileLayer))
        {
            Debug.Log($"[RAYCAST HIT] Object: {hit.collider.gameObject.name} | Tag: {hit.collider.tag}");
            destObject = hit.collider.gameObject;
            // 3. Directly return whether the hit object has the correct tag
            return hit.collider.CompareTag("Dest");
        }
        
        // If the ray didn't hit anything at all
        destObject = null;
        return false;
    }
}
