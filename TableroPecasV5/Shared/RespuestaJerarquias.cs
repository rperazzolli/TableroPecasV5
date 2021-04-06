using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public class RespuestaJerarquias : Respuesta
	{
    //[JsonProperty("Jerarq")]
    public List<CJerarquiaCN> Jerarquias { get; set; } = new List<CJerarquiaCN>();
	}

  public class CJerarquiaCN
  {

    //[JsonProperty("Inf")]
    public string Inferior { get; set;}

    //[JsonProperty("Mimico")]
    public int Mimico { get; set;}

    //[JsonProperty("Ord")]
    public int Orden { get; set;}

    //[JsonProperty("Sup")]
    public string Superior { get; set;}

  }

}
