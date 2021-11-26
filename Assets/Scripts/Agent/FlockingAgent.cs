using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingAgent : Agent
{
    #region Serialized Fields
    [Header("Zone size parameters")]
    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float repulsiveZoneSize = 1.0f;

    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float orientationZoneSize = 0.0f;

    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float attractionZoneSize = 0.0f;

    [Header("Other parameters")]
    private bool isTrigger = true;
    #endregion

    #region Private fields

    private List<GameObject> ruleZonesGO;
    private List<SphereCollider> ruleZones; //At 0 : repulsive, At 1 : orientation, At 2 : attractive

    List<GameObject> lR, lO, lA;
    #endregion

    #region MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        InitializeAgent(true);
        Collider [] list = this.GetComponents<Collider>();
        foreach(Collider c in list)
        {
            c.isTrigger = isTrigger; 
        }


        ruleZones = new List<SphereCollider>();
        ruleZonesGO = new List<GameObject>();

        

        for (int i=0; i<3; i++)
        {
            GameObject temp = new GameObject();
            temp.AddComponent<VisionZone>();
            temp.transform.parent = this.transform;
            temp.transform.localPosition = Vector3.zero;
            ruleZonesGO.Add(temp);

            SphereCollider newZone = temp.AddComponent<SphereCollider>();
            newZone.isTrigger = true;
            

            ruleZones.Add(newZone);
        }
    }

    // Update is called once per frame
    void Update()
    {

        CheckAgentsInZones();
        UpdateZoneSizes();
    }


    #endregion

    private void UpdateZoneSizes()
    {
        ruleZones[0].radius = repulsiveZoneSize;
        ruleZones[1].radius = repulsiveZoneSize + orientationZoneSize;
        ruleZones[2].radius = repulsiveZoneSize + orientationZoneSize + attractionZoneSize;
    }

    private void CheckAgentsInZones()
    {
        //Récupération des listes d'agents présents dans chaques zones
        lR = ruleZonesGO[0].GetComponent<VisionZone>().getAgentsInsideZone();
        lO = ruleZonesGO[1].GetComponent<VisionZone>().getAgentsInsideZone();
        lA = ruleZonesGO[2].GetComponent<VisionZone>().getAgentsInsideZone();

        //Suppression des itérations d'agents dans chaque zones
        foreach (GameObject g in lA)
        {
            if (lO.Contains(g)) lA.Remove(g);
        }

        foreach (GameObject g in lO)
        {
            if (lR.Contains(g)) lO.Remove(g);
        }
    }


}
