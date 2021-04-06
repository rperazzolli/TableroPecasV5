using System;
using System.Collections.Generic;
using System.Text;

namespace TableroPecasV5.Shared
{
  public class CComentarioCN
  {
    public List<string> Archivos { get; set; }

    public ClaseComentario Clase { get; set; }

    public ClaseElemento ClaseOrigen { get; set; }

    public int CodigoOrigen { get; set; }

    public string Contenido { get; set; }

    public System.DateTime Fecha { get; set; }

    public List<string> Links { get; set; }

    public int Orden { get; set; }

    public int Periodo { get; set; }

    public int Propietario { get; set; }

    public int SubCodigoOrigen { get; set; }

    public string Vinculo { get; set; }

    public CComentarioCN()
    {
      Archivos = new List<string>();
      Links = new List<string>();
    }


  }
}
