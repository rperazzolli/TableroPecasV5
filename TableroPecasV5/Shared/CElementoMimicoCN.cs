using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
  public class CBaseElementoMimicoCN : CContenedorBaseSinDescripcion
  {

    //[JsonProperty("MimicoBase")]
    public int MimicoBase{ get; set; }

  }

  public class CBaseElementoRectCN : CBaseElementoMimicoCN
  {

    //[JsonProperty("Abscisa")]
    public double Abscisa{ get; set; }

    //[JsonProperty("Alto")]
    public double Alto{ get; set; }

    //[JsonProperty("Ancho")]
    public double Ancho{ get; set; }

    //[JsonProperty("Nombre")]
    public string Nombre{ get; set; }

    //[JsonProperty("Ordenada")]
    public double Ordenada{ get; set; }

  }

  public class CElementoMimicoCN : CBaseElementoRectCN
  {

    //[JsonProperty("Comite")]
    public int Comite { get; set; }

    //[JsonProperty("Vinculo")]
    public string Vinculo { get; set; }

  }

}
