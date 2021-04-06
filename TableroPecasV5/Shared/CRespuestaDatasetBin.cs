using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public enum SituacionPedido
	{
		EnMarcha = 0,
		Abortado = 1,
		Completado = 2
	}

	public class RespuestaDatasetBin	: Respuesta
	{

		//[JsonProperty("Situacion")]
		public SituacionPedido Situacion { get; set; }

		//[JsonProperty("GUID")]
		public string GUID { get; set; }

		//[JsonProperty("ClaseOrigen")]
		public ClaseElemento ClaseOrigen { get; set; }

		//[JsonProperty("CodigoOrigen")]
		public Int32 CodigoOrigen { get; set; }

		//[JsonProperty("Datos")]
		public byte[] Datos { get; set; }

		//[JsonProperty("Periodo")]
		public Int32 Periodo { get; set; }

		//[JsonProperty("Zipeado")]
		public bool Zipeado { get; set; }

		public RespuestaDatasetBin()
		{
			Situacion = SituacionPedido.EnMarcha;
			GUID = "";
			ClaseOrigen = ClaseElemento.NoDefinida;
			CodigoOrigen = -1;
			Datos = new byte[0];
			Periodo = -1;
			Zipeado = false;
		}

	}
}
