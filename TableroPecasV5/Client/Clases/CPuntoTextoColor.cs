using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Clases
{
  public class CPuntoTextoColor : CPunto
  {
    public string Texto { get; set; }
    public string Color { get; set; }

    public CPuntoTextoColor()
    {
      Texto = "";
      Color = "gray";
    }

    public CPuntoTextoColor(double Absc, double Ord, string Color0, string Texto0 = "")
    {
      Abscisa = Absc;
      Ordenada = Ord;
      Color = Color0;
      Texto = Texto0;
    }
  }
}
