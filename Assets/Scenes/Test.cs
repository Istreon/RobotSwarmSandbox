using UnityEngine;
using System.IO;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string FolderPath = Application.dataPath + "/Results";
        if (Directory.Exists(FolderPath)) {
            Debug.Log(FolderPath + " existe");
        } else
        {
            Debug.LogError(FolderPath + " n'existe pas");
        }

        FolderPath = Application.dataPath + "/Zbeub";
        if (!Directory.Exists(FolderPath))
        {
            Debug.Log(FolderPath + " n'existe pas");
        }
        else
        {
            Debug.LogError(FolderPath + " existe");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
