using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableroPecasV5.Client.Listas
{
  public class CListaTexto
  {
    public Int32 Codigo { get; set; }
    public string Descripcion { get; set; }

    public CListaTexto()
    {
      Codigo = -1;
      Descripcion = "";
    }

    public CListaTexto(Int32 Cod0, string Texto0)
    {
      Codigo = Cod0;
      Descripcion = Texto0;
    }
  }

  public class CTextoTexto
  {
    public string Codigo { get; set; }
    public string Descripcion { get; set; }
    public object Referencia { get; set; } = null;

    public CTextoTexto()
    {
      Codigo = "";
      Descripcion = "";
    }

    public CTextoTexto(string Cod0, string Texto0)
    {
      Codigo = Cod0;
      Descripcion = Texto0;
    }
  }

}
