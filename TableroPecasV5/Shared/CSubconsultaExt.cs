using System;
using System.Collections.Generic;
using System.Text;

namespace TableroPecasV5.Shared
{
  public class CSubconsultaExt
  {
    //[JsonProperty("Codigo")]
    public int Codigo { get; set; }

    //[JsonProperty("Descripcion")]
    public string Descripcion { get; set; }

    //[JsonProperty("Nombre")]
    public string Nombre { get; set; }

    //[JsonProperty("Parametros")]
    public List<CParametroExt> Parametros { get; set; }

    public CSubconsultaExt()
    {
      Parametros = new List<CParametroExt>();
    }
  }
}
