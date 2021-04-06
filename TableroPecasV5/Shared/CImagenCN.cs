using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
  public class CImagenCN : CBaseElementoMimicoCN
  {

    //[JsonProperty("Ancho")]
    public Int32 Ancho { get; set; }

    //[JsonProperty("Alto")]
    public Int32 Alto { get; set; }

    //[JsonProperty("UrlImagen")]
    public string UrlImagen{ get; set; }

    public CImagenCN()
		{
      UrlImagen = "";
      Ancho = 0;
      Alto = 0;
		}

  }

}
