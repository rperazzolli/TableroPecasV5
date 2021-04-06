using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableroPecasV5.Client.Listas
{
  public class CListaDoble : CListaTexto
  {
    public string Detalle { get; set; }

    public CListaDoble(Int32 Cod0, string Linea1, string Linea2)
    {
      Codigo = Cod0;
      Descripcion = Linea1;
      Detalle = Linea2;
    }
  }
}
