using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
  public class CVinculoDetalleCN : CContenedorBaseSinDescripcion
  {

    //[JsonProperty("Posicion")]
    public string Posicion { get; set; }

    //[JsonProperty("ValorAsociado")]
    public string ValorAsociado { get; set; }

  }

  public class CVinculoIndicadorCN : CContenedorBaseSinDescripcion
  {

    //[JsonProperty("ClaseIndicador")]
    public ClaseElemento ClaseIndicador { get; set; } = ClaseElemento.NoDefinida;

    /// <summary>
    /// Clase de vínculo
    /// </summary>
    //[JsonProperty("ClaseVinculada")]
    public ClaseVinculo ClaseVinculada{ get; set; }

    //[JsonProperty("CodigoIndicador")]
    public int CodigoIndicador { get; set; } = -1;

    /// <summary>
    /// Cuando hay una capa asociada, es el código de la capa.
    /// </summary>
    //[JsonProperty("CodigoVinculado")]
    public int CodigoVinculado { get; set; } = -1;

    //[JsonProperty("ColumnaLat")]
    public string ColumnaLat { get; set; } = "";

    //[JsonProperty("ColumnaLng")]
    public string ColumnaLng { get; set; } = "";

    //[JsonProperty("NombreColumna")]
    public string NombreColumna { get; set; } = "";

    //[JsonProperty("Rango")]
    public double Rango { get; set; } = 0;

    //[JsonProperty("TipoColumna")]
    public ClaseVariable TipoColumna { get; set; } = ClaseVariable.NoDefinida;

  }

  public class CVinculoIndicadorCompletoCN
  {

    //[JsonProperty("Detalles")]
    public List<CVinculoDetalleCN> Detalles { get; set; }

    //[JsonProperty("Vinculo")]
    public CVinculoIndicadorCN Vinculo { get; set; } = new CVinculoIndicadorCN();

    public CVinculoIndicadorCompletoCN()
		{
      Detalles = new List<CVinculoDetalleCN>();
		}
  }

}
