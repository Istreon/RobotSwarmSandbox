using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MonaRobot : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private Mesh fieldOfViewMesh;

    [SerializeField]
    private GameObject mona3DModel;

    [SerializeField]
    private bool isKinematic = false;
    #endregion

    #region Private field
    private GameObject monaChild;

    private const float robotDiameter = 0.065f;   //Diameter of a MONA robot (unit : meters)
    private const float irSensorRange = 0.4f;     //Range of the 5 IR sensor of the MONA robot (unit : meters)
    private const float maxSpeed = 0.05f;         //Max speed of the MONA robot (unit : meters/second)
    private const int viewingAngle = 140;         //Viewing angle of the IR sensor of the MONA robot. (unit : degree)

    private Vector3 originalScale;
    private Vector3 monaOriginalScale;

    private MeshCollider fieldOfView;


    private List<GameObject> detectedMONAs;

    #endregion



    #region MonoBehaviour Callbacks
    // Start is called before the first frame update
    void Start()
    {
        detectedMONAs = new List<GameObject>();

        //Creation of 5 IR sensors
        //(idealement, c'est mieux de creer 5 capteurs (chacun ayant 45 degrés d'angle), mais le coup en performance est bcp trop grand))
        //Then, we are creating a simulated 5 IR sensors by only one fieldOfView
        fieldOfView = gameObject.AddComponent<MeshCollider>();
        fieldOfView.sharedMesh = fieldOfViewMesh;
        fieldOfView.convex = true;
        fieldOfView.isTrigger = true;


        //Instantiate a mona robot gameobject.
        monaChild = Instantiate(mona3DModel);
        monaChild.transform.parent = this.transform;
        monaChild.transform.localPosition = Vector3.zero;
        monaChild.transform.localRotation = Quaternion.identity;


        originalScale = this.transform.localScale;              //Will be use to calculate the robot size when we change the custom mesh collider size
        monaOriginalScale = monaChild.transform.localScale;     //We want to keep the same mona size, only change the fieldOfViewRange


        Rigidbody temp = this.gameObject.GetComponent<Rigidbody>();
        temp.isKinematic = isKinematic;
        if (!temp.isKinematic)
        {
            temp.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }

    }

    // Update is called once per frame
    void Update()
    {
        UpdateMonaModelScale();
    }

    private void LateUpdate()
    {
        detectedMONAs.Clear();
    }


    
    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<MonaRobot>() != null)
        {
            //It's an Agent
            GameObject temp = other.gameObject;
            /*
            
            //Check if the transform is not already in the colleague list
            bool alreadyHere = false;
            
            foreach (GameObject t in detectedMONAs)
            {
                if (GameObject.ReferenceEquals(t, temp))
                {
                    alreadyHere = true;
                }
            }
            */

            if (!detectedMONAs.Contains(temp))
            {
                detectedMONAs.Add(temp);
            }
            
        } else
        {
            //C'est un obstacle
            
        }
    }
    #endregion


    #region Private methods
    private void UpdateMonaModelScale() //Used to keep the same mona model size when we change the fieldOfView (MeshColliderSize) by changing the scale of this gameObject
    {
        Vector3 temp = new Vector3(originalScale.x / this.transform.localScale.x, originalScale.y / this.transform.localScale.y, originalScale.z / this.transform.localScale.z);
        temp = new Vector3(temp.x * monaOriginalScale.x, temp.y * monaOriginalScale.y, temp.z * monaOriginalScale.z);
        monaChild.transform.localScale = temp;
    }

    #endregion

    #region Public methods
    public List<GameObject> GetDetectedMONA()
    {
        return detectedMONAs;
    }

    #endregion
}
