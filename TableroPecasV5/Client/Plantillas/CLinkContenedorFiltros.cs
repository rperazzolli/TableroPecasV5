using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Logicas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Plantillas
{
  public class CLinkContenedorFiltros

  {
    public Int32 Periodo { get; set; }
    public CFiltrador Filtrador { get; set; }
    public ClaseElemento Clase { get; set; }
    public CDatoIndicador Indicador { get; set; }
    public Int32 CodigoElemento { get; set; }
    public CLogicaFiltroTextos.FncEventoRefresco FncRefresco { get; set; }
    public CLogicaFiltroTextos.FncEventoTextoBool FncCerrar { get; set; }
    public CFiltrador.FncEventoEnteros FncAjustarListasValor { get; set; }
    public static Int32 gCodigoUnico = 1;

    public List<CLinkBase> ComponentesAsociados { get; set; }

    public async Task CargarDatosFiltradorAsync(HttpClient Http, CContenedorBlocks Contenedor,
        ClaseElemento Clase, string Prms)
    {
      if (Filtrador == null)
      {
        CProveedorComprimido Proveedor = await Contenedor.CrearProveedorAsync(Http, Indicador.Codigo,
            Indicador.Dimension, CodigoElemento, Clase, Prms);
        if (Proveedor != null)
        {
          Filtrador = new CFiltrador();
          Filtrador.Proveedor = Proveedor;
          foreach (CLinkBase Link in ComponentesAsociados)
          {
            if (Link is CLinkOtro)
            {
              CLinkOtro Lnk = (CLinkOtro)Link;
              if (Lnk.ComponenteGrilla != null)
              {
                Lnk.ComponenteGrilla.Proveedor = Proveedor;
              }
            }
            if (Link is CLinkGraficoCnt)
            {
              CLinkGraficoCnt Graf = (CLinkGraficoCnt)Link;
              if (Graf.ComponentePropio != null)
              {
                Graf.ImponerProveedor(Proveedor);
              }
            }

            if (Link is CLinkMapa)
            {
              CLinkMapa Mapa = (CLinkMapa)Link;
              if (Mapa.ComponenteCalor != null || Mapa.Componente != null ||
                  Mapa.ComponenteGradiente != null || Mapa.ComponenteMapa != null)
              {
                Mapa.ImponerProveedor(Proveedor);
              }
            }

            if (Link is CLinkOtro)
						{
              CLinkOtro Otro = (CLinkOtro)Link;
              if (Otro.Datos.Clase==CGrafV2DatosContenedorBlock.ClaseBlock.Mimico)
							{
                Otro.ImponerProveedor(Proveedor);
							}
						}
          }
          Proveedor.InformarDependientes();
        }
      }
    }

    public CLinkContenedorFiltros()
    {
      Filtrador = null;
      ComponentesAsociados = new List<CLinkBase>();
    }

  }

}
