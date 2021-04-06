using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
  public enum StatusEstructura
  {
    EnEdicion = 1,
    Publicado = 4,
    NoDefinido = -1
  }

  public class CCamposComunesCN
  {

    //[JsonProperty("CodigoE")]
    public string CodigoExterno { get; set; } = "";

    //[JsonProperty("FReg")]
    public System.DateTime FechaRegistro { get; set; } = new DateTime(1900, 1, 1);

    //[JsonProperty("H")]
    public List<int> Hijos { get; set; } = new List<int>();

    //[JsonProperty("IdOrg")]
    public int IDOrganigrama { get; set; } = -1;

    //[JsonProperty("Reg")]
    public int Registrador { get; set; } = -1;

    //[JsonProperty("Stat")]
    public StatusEstructura Status { get; set; } = StatusEstructura.NoDefinido;

    //[JsonProperty("Desde")]
    public System.DateTime ValidezDesde { get; set; } = new DateTime(1900, 1, 1);

    //[JsonProperty("Hasta")]
    public System.DateTime ValidezHasta { get; set; } = new DateTime(1900, 1, 1);
  }

    public class CGrupoPuestosCN : CCamposComunesCN
  {

    //[JsonProperty("Codigo")]
    public int Codigo { get; set; } = -1;

    //[JsonProperty("Descripcion")]
    public string Descripcion { get; set; } = "";

    //[JsonProperty("Objetivo")]
    public string Objetivo { get; set; } = "";
  }

    public class RespuestaComites : Respuesta
	{
    //[JsonProperty("Grupos")]
    public List<CGrupoPuestosCN> GrupoPuestos { get; set; } = new List<CGrupoPuestosCN>();

  }
}
