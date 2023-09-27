[System.Serializable]
public class Exp2AnticipationAnswer
{
    public string filename;
    public bool fracture;
    public float height;

    public Exp2AnticipationAnswer(string filename, bool fracture, float height)
    {
        this.filename = filename;
        this.fracture = fracture;
        this.height = height;
    }
}
