using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SwarmClipTools
{
    #region Methods
    /**
     * This method load a clip from a specific file type (.dat)
     * Parameters : 
     * -filePath (string) : is the path (absolute) of the file to load
     * 
     * Return value :
     * -Return the loaded clip
     * -If the filepath is wrong, return null
     * */
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


    /**
     * This method save a clip into a .dat file
     * Parameters :
     * -clip (LogClip) :  the clip to save
     * -filePath (string) : the name of the file and its absolute path
     * 
     * Return value :
     * -There is no return value
     **/
    public static void SaveClip(LogClip clip, string filePath)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filePath, FileMode.OpenOrCreate);
        bf.Serialize(file, clip);
        file.Close();
    }
    #endregion
}
