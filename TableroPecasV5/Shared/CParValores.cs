using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
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
    //[JsonProperty("CodigoElemento")]
    public string CodigoElemento { get; set; }

    //[JsonProperty("ValorElemento")]
    public double ValorElemento { get; set; }

    //[JsonProperty("Cantidad")]
    public Int32 Cantidad { get; set; }

    //[JsonProperty("ColorElemento")]
    public ColorBandera ColorElemento { get; set; }

    //[JsonProperty("ColorImpuesto")]
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
