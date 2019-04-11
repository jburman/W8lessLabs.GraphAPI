module Json

open Newtonsoft.Json
open W8lessLabs.GraphAPI

type JsonSerializer() =
   interface IJsonSerializer with
      member this.Serialize(value) = JsonConvert.SerializeObject(value)
      member this.Deserialize(value) : 'T = JsonConvert.DeserializeObject<'T>(value)
