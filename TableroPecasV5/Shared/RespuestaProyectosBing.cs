using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public class RespuestaProyectosBing	: Respuesta
	{
		//[JsonProperty("Proyectos")]
		public List<CMapaBingCN> Proyectos { get; set; }

		public RespuestaProyectosBing()
		{
			Proyectos = new List<CMapaBingCN>();
		}
	}
}
