using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Behaviour based on Heroes and Cowards behaviour from : http://www.netlogoweb.org/launch#http://www.netlogoweb.org/assets/modelslib/IABM%20Textbook/chapter%202/Heroes%20and%20Cowards.nlogo

public class HeroesAndCowardsAgent : MonoBehaviour
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
    [Range(1.0f, 50.0f)]
    private float visionSize=5.0f;

    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float moveSpeed = 1.0f;
    #endregion

    #region Private fields
    private List<Transform> colleagues;

    private Transform friend=null;
    private Transform enemy=null;

    private SphereCollider visionZone;

    private Rigidbody agentBody;

    private float lastRotation = 0.0f;

    #endregion

    #region MonoBehaviour Callbacks
    // Start is called before the first frame update
    void Start()
    {
        agentBody= this.GetComponent<Rigidbody>();
        visionZone = gameObject.AddComponent<SphereCollider>();
        visionZone.isTrigger = true;

        colleagues = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (friend!=null && enemy!=null)
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
        } else
        {
            //Cherche un ami et un ennemi
            SearchingForColleagues();
        }

        MoveForward();
        UpdateVisionZoneSize();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<HeroesAndCowardsAgent>() != null)
        {
            if (friend != null || enemy != null) return;

            Transform temp = other.gameObject.transform;
            //Check if the transform is not already in the colleague list
            bool alreadyHere = false;

            foreach (Transform t in colleagues)
            {
                if (Transform.ReferenceEquals(t, temp))
                {
                    alreadyHere = true;
                }
            }

            if (!alreadyHere)
            {
                    
                colleagues.Add(temp);
            }
        }
    }
    #endregion

    #region Private Methods
    private void ActBravely()
    {
        float x = (friend.position.x + enemy.position.x) / 2.0f;
        float z = (friend.position.z + enemy.position.z) / 2.0f;
        Vector3 directionPoint = new Vector3(x, 0.5f, z);
        this.transform.LookAt(directionPoint);
    }

    private void ActCowardly()
    {
        float x = friend.position.x + ((friend.position.x + enemy.position.x) / 2.0f);
        float z = friend.position.z + ((friend.position.z + enemy.position.z) / 2.0f);
        Vector3 directionPoint = new Vector3(x, 0.5f, z);
        this.transform.LookAt(directionPoint);
    }

    private void SearchingForColleagues()
    {
        int nb = colleagues.Count;
        if (nb < 2)
        {
            if(nb==1)
            {
                Vector3 dir=colleagues[0].position - this.transform.position;
                float val = Random.Range(-1.0f, 1.0f);
                float val2 = Random.Range(0.0f, 3.0f);
                lastRotation += val*val2;
                if (lastRotation > 60) lastRotation = 60.0f;
                if (lastRotation < -60) lastRotation = -60.0f;
                dir = Quaternion.AngleAxis(lastRotation, Vector3.up) * dir;
                Vector3 dirPoint = dir + this.transform.position;
                this.transform.LookAt(dirPoint);
            } else
            {
                float val = Random.Range(0, 500);
                if(val==0)
                {
                    val = Random.Range(-2.0f, 2.0f);
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
            gameObject.GetComponent<Renderer>().material.color = Color.blue;
        }
    }

   
    private void UpdateVisionZoneSize()
    {
        visionZone.radius = visionSize;
    }
   

    private void MoveForward()
    {
        //this.transform.position += this.transform.forward * moveSpeed * Time.deltaTime;
        agentBody.velocity= this.transform.forward * moveSpeed ;
    }




   



    #endregion
}
