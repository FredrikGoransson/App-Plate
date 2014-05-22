namespace AppPlate.Core.Serialization
{
    public interface IDocumentSerializer
    {
        TObject DeserializeObject<TObject>(string serializedObject);
        string SerializeObject<TObject>(TObject objectForSerialization);
    }

    public class JsonDocumentSerializer : IDocumentSerializer
    {
        public TObject DeserializeObject<TObject>(string serializedObject)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TObject>(serializedObject);
        }

        public string SerializeObject<TObject>(TObject objectForSerialization)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(objectForSerialization);
        }
    }
}