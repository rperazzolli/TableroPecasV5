using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
  public class RespuestaMimico : Respuesta
  {

    //[JsonProperty("Imagen")]
    public CImagenCN Imagen { get; set; }

    //[JsonProperty("Mimico")]
    public CMimicoCN Mimico { get; set; }

    //[JsonProperty("Proceso")]
    public CProcesoGraficoCN Proceso { get; set; }

    public RespuestaMimico()
		{
      Mimico = new CMimicoCN();
      Imagen = new CImagenCN();
      Proceso = new CProcesoGraficoCN();
		}

  }



}
