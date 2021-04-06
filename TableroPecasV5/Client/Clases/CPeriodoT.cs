using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableroPecasV5.Client.Clases
{
  public class CPeriodoT
  {
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public List<DatosPeriodo> Datos { get; set; }

    public CPeriodoT()
    {
      Datos = new List<DatosPeriodo>();
    }

    public void AgregarValor(Int32 Orden, double Valor)
    {
      foreach (DatosPeriodo D in Datos)
      {
        if (D.Orden == Orden)
        {
          D.Valor += Valor;
          return;
        }
      }
      Datos.Add(new DatosPeriodo()
      {
        Orden = Orden,
        Valor = Valor
      });
    }
  }

  public class DatosPeriodo
  {
    public Int32 Orden { get; set; }
    public double Valor { get; set; }
  }
}
