using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
  public class RespuestaQuerySubconsultas : Respuesta
  {
    //[JsonProperty("Campo")]
    public string Campo { get; set; }

    //[JsonProperty("Textos")]
    public List<string> Textos { get; set; }

    //[JsonProperty("Valores")]
    public List<string> Valores { get; set; }

  }
}
