using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Plantillas
{
  public class CDatosGrafLista
  {
    public static Int32 gCodigo = 0;
    public Int32 Codigo { get; set; }
    public ClaseGrafico Clase { get { return Datos.Clase; } }
    public string Nombre { get; set; }
    public CGrafV2DatosContenedorBlock Datos { get; set; }

    public CDatosGrafLista(CGrafV2DatosContenedorBlock Grafico)
    {
      gCodigo++;
      Codigo = gCodigo;
      Datos = Grafico;
      Nombre = Grafico.Nombre;
    }

  }

}
