using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feeler : MonoBehaviour
{

    private List<GameObject> obstacles;
    // Start is called before the first frame update
    void Start()
    {
        obstacles = new List<GameObject>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        obstacles.Clear();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Obstacle>() != null)
        {
            obstacles.Add(other.gameObject);
        }
    }

    public List<GameObject> getObstacles()
    {
        return obstacles;
    }
}
