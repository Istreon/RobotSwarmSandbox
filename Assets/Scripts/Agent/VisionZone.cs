using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionZone : MonoBehaviour
{

    private List<GameObject> agentsInside;
    // Start is called before the first frame update
    void Start()
    {
        agentsInside = new List<GameObject>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        agentsInside.Clear();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Agent>()!=null)
        {
            //Debug.Log("ok");
            agentsInside.Add(other.gameObject);
        }
    }

    public List<GameObject> getAgentsInsideZone()
    {
        return agentsInside;
    }
}
