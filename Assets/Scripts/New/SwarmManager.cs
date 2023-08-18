using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmManager : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private GameObject mapPrefab;
    #endregion

    private SwarmData swarm;

    private GameObject map;
    // Start is called before the first frame update
    void Start()
    {
        //Instantiation of the map
        map = Instantiate(mapPrefab);
        map.transform.parent = null;

        /*SwarmParameters parameters = FindObjectOfType<SwarmParameters>();
        if (parameters == null) {
            Debug.LogError("ParameterManager is missing in the scene", this);
            parameters = new SwarmParameters();
        }
        swarm.SetParameters(parameters);*/
    }

    // Update is called once per frame
    void Update()
    {
        //Pour chaque agent
        //Mise à 0 de l'accéleration

        //Mets à jour la position des agents (en utilisant le manager de mouvement)

        //Corrige la position des agents avec le manager de map

        //Pour chaque agent
        //Application du comportement sur les agents

        //Pour chaque agent
        //Calcul de l'acceleration

        //Calcul de la vitesse

        //Reduction de la vitesse en fonction de la vitesse maximale

        //Affichage

        UpdateMap();
    }

    #region Methods - Map
    private void UpdateMap()
    {
        if (swarm.GetParameters() == null)
        {
            Debug.Log("meeeh");
            return;
        }
        float x = swarm.GetParameters().GetMapSizeX();
        float z = swarm.GetParameters().GetMapSizeZ();
        map.transform.position = new Vector3(x / 2.0f, 0.0f, z / 2.0f);
        map.transform.localScale = new Vector3(x, 1.0f, z);
    }

    public Vector3 CorrectPosition(Vector3 position, float objectRadius)
    {
        float mapSizeX = swarm.GetParameters().GetMapSizeX();
        float mapSizeZ = swarm.GetParameters().GetMapSizeZ();
        float x = 0.0f;
        float z = 0.0f;
        Vector3 temp = this.transform.position;
        if (position.x > mapSizeX - objectRadius)
        {
            x = mapSizeX - objectRadius;
        }
        if (position.x < objectRadius)
        {
            x = objectRadius;
        }

        if (position.z > mapSizeZ - objectRadius)
        {
            z = mapSizeZ - objectRadius;
        }
        if (position.z < objectRadius)
        {
            z = objectRadius;
        }
        Vector3 newPosition = new Vector3(x, 0.0f, z);

        return newPosition;
    }
    #endregion
}
