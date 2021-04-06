using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Clases
{
	public class CLineaReporte
	{
		public Int32 TabReferencia { get; set; } = 0;
		public string Referencia { get; set; } = "";
		public CDatoIndicador Indicador { get; set; }
		public CInformacionAlarmaCN DatosIndicador { get; set; }

		public CLineaReporte() { }

		public CLineaReporte (string Texto, Int32 Tab, CDatoIndicador Indic, CInformacionAlarmaCN Datos)
		{
			TabReferencia = Tab;
			Referencia = Texto;
			Indicador = Indic;
			DatosIndicador = Datos;
		}

		public CLineaReporte(Int32 CodIndicador, Int32 CodDimension)
		{
			Indicador = Contenedores.CContenedorDatos.IndicadorDesdeCodigo(CodIndicador);
			if (Indicador != null)
			{
				DatosIndicador = Contenedores.CContenedorDatos.AlarmaIndicadorDesdeGlobal(CodIndicador, CodDimension);
			}
		}
	}
}
