using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Behaviour based on Heroes and Cowards behaviour from : http://www.netlogoweb.org/launch#http://www.netlogoweb.org/assets/modelslib/IABM%20Textbook/chapter%202/Heroes%20and%20Cowards.nlogo


[RequireComponent(typeof(MonaRobot))]
public class HeroesAndCowardsMona : MonoBehaviour
{
    enum AgentBehaviour
    {
        Hero,
        Coward
    }

    #region Serialized fields
    [SerializeField]
    private AgentBehaviour agentBehaviour = AgentBehaviour.Hero;

    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float moveSpeed = 1.0f;

    #endregion

    #region Private fields
    private List<Transform> colleagues;

    private Transform friend = null;
    private Transform enemy = null;

    private Rigidbody agentBody;

    private float lastRotation = 0.0f;

    #endregion

    #region MonoBehaviour Callbacks
    // Start is called before the first frame update
    void Start()
    {
        if (agentBody == null) agentBody = this.GetComponent<Rigidbody>();
        colleagues = new List<Transform>();
    }


    private void FixedUpdate()
    {
        if (agentBody == null) agentBody = this.GetComponent<Rigidbody>();
        CheckForColleagues();
        if (friend != null && enemy != null)
        {
            switch (agentBehaviour)
            {
                case AgentBehaviour.Hero:
                    ActBravely();
                    break;
                case AgentBehaviour.Coward:
                    ActCowardly();
                    break;
            }
        }
        else
        {
            //Cherche un ami et un ennemi
            SearchingForColleagues();
        }

        MoveForward();
    }

    
   
    #endregion

    #region Private Methods
    private void ActBravely()
    {

        float x = (friend.position.x + enemy.position.x) / 2.0f;
        float z = (friend.position.z + enemy.position.z) / 2.0f;
        Vector3 directionPoint = new Vector3(x, this.transform.position.y, z);
        this.transform.LookAt(directionPoint);
    }

    private void ActCowardly()
    {
        float x = friend.position.x + ((friend.position.x + enemy.position.x) / 2.0f);
        float z = friend.position.z + ((friend.position.z + enemy.position.z) / 2.0f);
        Vector3 directionPoint = new Vector3(x, this.transform.position.y, z);
        this.transform.LookAt(directionPoint);
    }

    private void SearchingForColleagues()
    {
        int nb = colleagues.Count;
        if (nb < 2)
        {
            if (nb == 1)
            {
                Vector3 dir = colleagues[0].position - this.transform.position;
                float val = Random.Range(-1.0f, 1.0f);
                float val2 = Random.Range(0.0f, 3.0f);
                lastRotation += val * val2;
                if (lastRotation > 60) lastRotation = 60.0f;
                if (lastRotation < -60) lastRotation = -60.0f;
                dir = Quaternion.AngleAxis(lastRotation, Vector3.up) * dir;
                Vector3 dirPoint = dir + this.transform.position;
                this.transform.LookAt(dirPoint);
            }
            else
            {
                float val = Random.Range(0, 500);
                if (val == 0)
                {
                    val = Random.Range(-3.0f, 3.0f);
                    lastRotation += val;
                    Vector3 dir = Quaternion.AngleAxis(lastRotation, Vector3.up) * this.transform.forward;
                    Vector3 dirPoint = dir + this.transform.position;
                    this.transform.LookAt(dirPoint);
                }
            }
        }
        else
        {
            int val = Random.Range(0, nb);
            friend = colleagues[val];
            enemy = colleagues[((val + 1) % nb)];
            //gameObject.GetComponent<Renderer>().material.color = Color.blue;
        }
    }


    private void MoveForward()
    {
        
        agentBody.velocity = this.transform.forward * moveSpeed;
    }

    private void CheckForColleagues()
    {
        List<GameObject> detectedList = this.GetComponent<MonaRobot>().GetDetectedMONA();
        if(detectedList!=null)
        {
            colleagues.Clear();
            foreach (GameObject g in detectedList)
            {
                colleagues.Add(g.transform);
            }
        }
        
    }

    #endregion
}
