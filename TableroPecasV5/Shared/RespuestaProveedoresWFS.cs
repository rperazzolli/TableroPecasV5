using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{

	public class CProveedorWFSCN
	{
    public int Codigo {get; set;}

    public string Descripcion {get; set;}

    public string DireccionFA {get; set;}

    public string DireccionURL {get; set;}

    public System.DateTime FechaRefresco {get; set;}

  }

  public class RespuestaProveedoresWFS : Respuesta
	{

		//[JsonProperty("Prov")]
		public List<CProveedorWFSCN> Proveedores { get; set; }

	}
}
