using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableroPecasV5.Client.Clases
{
  public class CPunto
  {
    public double Abscisa { get; set; }
    public double Ordenada { get; set; }

    public CPunto()
    {
      Abscisa = 0;
      Ordenada = 0;
    }

    public override string ToString()
    {
      return Abscisa.ToString() + " ; " + Ordenada.ToString();
    }

    public CPunto(double Absc, double Ord)
    {
      Abscisa = Absc;
      Ordenada = Ord;
    }

    public bool PuntoIgual(CPunto Otro)
    {
      return ((Math.Abs(Otro.Abscisa - Abscisa) + Math.Abs(Otro.Ordenada - Ordenada)) < 0.0000000000001);
    }
  }
}
