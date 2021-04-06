using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Clases
{
  public class CListaElementosDimension
  {
    public Int32 CodigoDimension;
    public string Descripcion;
    public List<CEntidadCN> Elementos;

    public CListaElementosDimension()
    {
      CodigoDimension = -1;
      Descripcion = "";
      Elementos = new List<CEntidadCN>();
    }

  }

}
