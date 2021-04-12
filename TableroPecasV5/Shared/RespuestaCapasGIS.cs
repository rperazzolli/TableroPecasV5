using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public class RespuestaCapasGIS : Respuesta
	{
    //[JsonProperty("WFS")]
    public List<CCapaWFSCN> CapasWFS { get; set; } = new List<CCapaWFSCN>();

    //[JsonProperty("WIS")]
    public List<CCapaWISCompletaCN> CapasWIS { get; set; } = new List<CCapaWISCompletaCN>();

    //[JsonProperty("WMS")]
    public List<CCapaWMSCN> CapasWMS { get; set; } = new List<CCapaWMSCN>();
  }

}
