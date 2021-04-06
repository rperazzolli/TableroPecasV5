using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;

namespace TableroPecasV5.Shared
{
	public class CDatosLogin
	{
		//[JsonProperty("Usuario")]
		public string Usuario { get; set; }

		//[JsonProperty("Ticket")]
		public string Ticket { get; set; }

		//[JsonProperty("Administrador")]
		public bool Administrador { get; set; }

		//[JsonProperty("DesciendeEnRojo")]
		public bool DesciendeEnRojo { get; set; }

		//[JsonProperty("ImprimirPDF")]
		public string ImprimirPDF { get; set; }

		//[JsonProperty("MaximizaTendencias")]
		public bool MaximizaTendencias { get; set; }

		//[JsonProperty("Minutos")]
		public Int32 Minutos { get; set; }

		//[JsonProperty("PoneEtiquetas")]
		public bool PoneEtiquetas { get; set; }

		//[JsonProperty("RespetaSentido")]
		public bool RespetaSentido { get; set; }

		//[JsonProperty("SiempreTendencia")]
		public bool SiempreTendencia { get; set; }

		//[JsonProperty("TendenciaEnTarjeta")]
		public bool TendenciasEnTarjeta { get; set; }

		//[JsonProperty("MsgErr")]
		public string MsgErr { get; set; }

		//[JsonProperty("RespuestaOK")]
		public bool RespuestaOK { get; set; }

		//[JsonProperty("CodigoUsuario")]
		public Int32 CodigoUsuario { get; set; }

		public CDatosLogin()
		{
			Ticket = "";
			MsgErr = "";
			RespuestaOK = true;
		}
	}
}
