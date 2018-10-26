using System.Runtime.Serialization;

namespace W8lessLabs.GraphAPI
{
    [DataContract]
    public class GraphResponse<T>
    {
        [DataMember(Name = "value")]
        public T Value { get; set; }
        [DataMember(Name = "count")]
        public int Count { get; set; }
        [DataMember(Name = "@odata.nextLink")]
        public string NextLink { get; set; }
        [DataMember(Name = "@odata.deltaLink")]
        public string DeltaLink { get; set; }
        [DataMember(Name = "@odata.context")]
        public string Context { get; set; }
    }
}
