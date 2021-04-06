using System;
using System.Collections.Generic;
using System.Text;

namespace TableroPecasV5.Shared
{
	public class CEntidadCN	: CContenedorBase
	{
	}

	public class CContenedorBase : CContenedorBaseConVersion
	{
		//[JsonProperty("Descripcion")]
		public string Descripcion { get; set; }
	}

	public class CContenedorBaseConVersion : CContenedorBaseSinVersion
	{
		//[JsonProperty("Version")]
		public Int32 Version { get; set; }
	}

	public class CContenedorBaseSinVersion
	{
		//[JsonProperty("Codigo")]
		public Int32 Codigo { get; set; }
	}
}
