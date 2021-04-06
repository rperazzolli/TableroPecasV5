using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableroPecasV5.Client.Clases
{
  public class CPuntoCalor
  {
    public double Abscisa;
    public double Ordenada;
    public float Valor;

    public CPuntoCalor(double Absc, double Ord, float Valor0)
    {
      Abscisa = Absc;
      Ordenada = Ord;
      Valor = Valor0;
    }
  }

}
