using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Shared
{
  public class CColorElemento
  {
    //[JsonProperty("Clase")]
    public ClaseDetalle Clase { get; set; }

    //[JsonProperty("Codigo")]
    public Int32 Codigo { get; set; }

    //[JsonProperty("Dimension")]
    public Int32 Dimension { get; set; }

    //[JsonProperty("ElementoDimension")]
    public Int32 ElementoDimension { get; set; }

    //[JsonProperty("ColorElemento")]
    public ColoresParaPreguntas ColorElemento { get; set; }

    public CColorElemento()
    {
      Clase = ClaseDetalle.NoDefinido;
      Codigo = -1;
      Dimension = -1;
      ElementoDimension = -1;
      ColorElemento = ColoresParaPreguntas.Gris;
    }

  }

}
