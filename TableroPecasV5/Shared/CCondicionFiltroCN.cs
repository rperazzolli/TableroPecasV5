using System;
using System.Collections.Generic;
using System.Text;

namespace TableroPecasV5.Shared
{
	public class CCondicionFiltroCN
	{
    //[JsonProperty("BlockCnd")]
    public int BlockCondiciones { get; set; }

    //[JsonProperty("CampoCnd")]
    public string CampoCondicion { get; set; }

    //[JsonProperty("CodigoGr")]
    public int CodigoGrafico { get; set; }

    //[JsonProperty("DebeCumplirTodas")]
    public bool DebeCumplirTodasEnBlock { get; set; }

    //[JsonProperty("Incluye")]
    public bool IncluyeALasQueCumplen { get; set; }

    //[JsonProperty("ModoFiltrar")]
    public ModoFiltrar ModoDeFiltrar { get; set; }

    //[JsonProperty("OrdenEval")]
    public int OrdenEvaluacion { get; set; }

    //[JsonProperty("Paso")]
    public int Paso { get; set; }

    //[JsonProperty("VMax")]
    public double ValorMaximo { get; set; }

    //[JsonProperty("VMin")]
    public double ValorMinimo { get; set; }

    //[JsonProperty("VTexto")]
    public string ValorTexto { get; set; }
  }
}
