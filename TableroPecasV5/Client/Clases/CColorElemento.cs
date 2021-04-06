using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Clases
{
  public class CColorElemento
  {
    public ClaseDetalle Clase { get; set; }
    public Int32 Codigo { get; set; }
    public Int32 Dimension { get; set; }
    public Int32 ElementoDimension { get; set; }
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
