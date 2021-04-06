using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
  public enum PosicionadorGIS
  {
    NoCorresponde = -1,
    PinesXY = 1,
    TortasXY = 2,
    Vinculo = 3
  }

  public class CGraficoCompletoCN
	{
    //[JsonProperty("Adicional")]
    public string Adicional {get; set; }

    //[JsonProperty("Codigo")]
    public int Codigo {get; set; }

    //[JsonProperty("CodigoSC")]
    public int CodigoSC {get; set; }

    //[JsonProperty("Descripcion")]
    public string Descripcion {get; set; }

		//[JsonProperty("Graficos")]
		public List<CGraficoCN> Graficos { get; set; }

		//[JsonProperty("Indicador")]
		public int Indicador {get; set; }

    //[JsonProperty("Orden")]
    public int OrdenGraficacion {get; set; }

    //[JsonProperty("ParamSC")]
    public string ParamSC {get; set; }

    //[JsonProperty("Posicionador")]
    public PosicionadorGIS Posicionador {get; set; }

    //[JsonProperty("Vinculo1")]
    public string Vinculo1 {get; set; }

    //[JsonProperty("Vinculo2")]
    public string Vinculo2 {get; set; }
  }

  public class RespuestaGraficosVarios : Respuesta
	{
		//[JsonProperty("Vinculo")]
		public List<CGraficoCompletoCN> Graficos { get; set; }
	}
}
