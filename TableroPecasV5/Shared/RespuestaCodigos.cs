using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
  public class ParCodigos
  {

    //[JsonProperty("Actual")]
    public int Actual { get; set; }

    //[JsonProperty("Anterior")]
    public int Anterior { get; set; }

  }

    public class RespuestaCodigos : Respuesta
	{
    //[JsonProperty("CodigosMimicos")]
    public List<ParCodigos> ParesDeCodigosMimicos { get; set; }

    //[JsonProperty("CodigosPreguntas")]
    public List<ParCodigos> ParesDeCodigosPreguntas { get; set; }

    //[JsonProperty("CodigosWFS")]
    public List<ParCodigos> ParesDeCodigosWFS { get; set; }

    public RespuestaCodigos()
		{
      ParesDeCodigosMimicos = new List<ParCodigos>();
      ParesDeCodigosPreguntas = new List<ParCodigos>();
      ParesDeCodigosWFS = new List<ParCodigos>();
		}
  }
}
