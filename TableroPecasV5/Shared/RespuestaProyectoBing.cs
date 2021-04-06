using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{

  public class RespuestaProyectoBing : Respuesta
  {

    public RespuestaProyectoBing()
    {
      Proyecto = null;
      CapasCompletas = new List<CDatosCapaComodin>();
    }

    //[JsonProperty("Proyecto")]
    public CMapaBingCN Proyecto { get; set; }

    //[JsonProperty("CapasCompletas")]
    public List<CDatosCapaComodin> CapasCompletas { get; set; }

    //[JsonProperty("LatCentro")]
    public double LatCentro { get; set; } = 0;

    //[JsonProperty("LngCentro")]
    public double LngCentro { get; set; } = 0;

    //[JsonProperty("NivelZoom")]
    public double NivelZoom { get; set; } = 1;

  }

}
