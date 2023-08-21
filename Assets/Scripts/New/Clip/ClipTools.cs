using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class ClipTools
{
    #region Methods - Load and Save

    /// <summary>
    /// This method load a clip from a specific file format (.dat), containing a <see cref="SwarmClip"/> instance.
    /// </summary>
    /// <param name="filePath"> A <see cref="string"/> value corresponding to the absolute path of the file to load</param>
    /// <returns>
    /// A <see cref="SwarmClip"/> instance from the file, null otherwise. 
    /// </returns>
    public static SwarmClip LoadClip(string filePath)
    {
        if (File.Exists(filePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);
            SwarmClip clip = null;
            try
            {
                clip = (SwarmClip)bf.Deserialize(file);
            }
            catch (Exception)
            {
                return null;
            }

            file.Close();
            return clip;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// This method save a <see cref="SwarmClip"/> into a .dat file.
    /// </summary>
    /// <param name="clip"> The <see cref="SwarmClip"/> to save.</param>
    /// <param name="filePath"> The absolute path of the file that will contain the clip.</param>
    public static void SaveClip(SwarmClip clip, string filePath)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filePath, FileMode.OpenOrCreate);
        bf.Serialize(file, clip);
        file.Close();
    }
    #endregion

}
