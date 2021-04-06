using System;
using System.Collections.Generic;
using System.Text;

namespace TableroPecasV5.Shared
{
	public class CPuntoPregunta
	{
		//[JsonProperty("Indicadores")]
		public List<CPreguntaIndicadorCN> Indicadores { get; set; }

		//[JsonProperty("Pregunta")]
		public CPreguntaCN Pregunta { get; set; }

		public CPuntoPregunta()
		{
			Pregunta = new CPreguntaCN();
			Indicadores = new List<CPreguntaIndicadorCN>();
		}

	}
	public class CPreguntaIndicadorCN
	{

		//[JsonProperty("Dimension")]
		public int Dimension { get; set; }

		//[JsonProperty("ElementoDimension")]
		public int ElementoDimension { get; set; }

		//[JsonProperty("Indicador")]
		public int Indicador { get; set; }

		//[JsonProperty("Orden")]
		public int Orden { get; set; }

		//[JsonProperty("Pregunta")]
		public int Pregunta { get; set; }

	}

  public class CPreguntaCN
  {

		//[JsonProperty("Block")]
		public string Block { get; set; }

		//[JsonProperty("Codigo")]
		public int Codigo { get; set; }

		//[JsonProperty("Dimension")]
		public int Dimension { get; set; }

		//[JsonProperty("ElementoDimension")]
		public int ElementoDimension { get; set; }

		//[JsonProperty("Orden")]
		public int Orden { get; set; }

		//[JsonProperty("Pregunta")]
		public string Pregunta { get; set; }

		//[JsonProperty("Solapa")]
		public int Solapa { get; set; }

  }



}
