[System.Serializable]
public class Exp2AnticipationAnswer
{
    public string filename;
    public int visualisation;
    public bool fracture;
    public float height;

    public Exp2AnticipationAnswer(string filename, int visualisation, bool fracture, float height)
    {
        this.filename = filename;
        this.visualisation = visualisation;
        this.fracture = fracture;
        this.height = height;
    }
}
