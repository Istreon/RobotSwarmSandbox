using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public class SwarmClipTools
{
    public static LogClip LoadClip(string filePath)
    {
        if (File.Exists(filePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);
            LogClip clip = (LogClip)bf.Deserialize(file);
            file.Close();

            return clip;
        }
        else
        {
            return null;
        }
    }

    public static void SaveClip(LogClip clip, string filePath)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filePath, FileMode.OpenOrCreate);
        //Debug.Log(Application.persistentDataPath);
        bf.Serialize(file, clip);
        file.Close();
    }
}
