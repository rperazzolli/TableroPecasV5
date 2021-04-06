using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableroPecasV5.Client.Clases
{
  public enum ColorBandera
  {
    NoCorresponde,
    SinDatos,
    Azul,
    Rojo,
    Amarillo,
    Verde,
    Blanco // cuando hay indicadores sin color.
  }

  public class CParValores
  {
    public string CodigoElemento { get; set; }
    public double ValorElemento { get; set; }
    public Int32 Cantidad { get; set; }
    public ColorBandera ColorElemento { get; set; }
    public string ColorImpuesto { get; set; }

    public CParValores(string Cod0)
    {
      CodigoElemento = Cod0;
      ValorElemento = 0;
      Cantidad = 0;
      ColorElemento = ColorBandera.NoCorresponde;
      ColorImpuesto = "transparent";
    }

  }

}
