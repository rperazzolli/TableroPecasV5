using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Logicas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Plantillas
{
  public class CLinkReloj : CLinkBase
  {

    public List<CInformacionAlarmaCN> Alarmas { get; set; } = null;

    public ClaseElemento Clase { get; set; } = ClaseElemento.Indicador;
    public string ParametrosSC { get; set; } = "";

    public CFiltrador Filtrador { get; set; }
    public bool HayVentanasDependientes { get; set; } = false;
    public bool Visible { get; set; } = true;
    public CLogicaFiltroTextos.FncEventoRefresco FncRefresco { get; set; }
    public CLogicaFiltroTextos.FncEventoTextoBool FncCerrar { get; set; }
    public CFiltrador.FncEventoEnteros FncAjustarListasValor { get; set; }
    public static Int32 gCodigoUnico = 1;

    public Logicas.CLogicaRelojCompleto ComponentePropio
    {
      get { return (Logicas.CLogicaRelojCompleto)Componente; }
      set { Componente = value; }
    }

    public Logicas.CLogicaTrendRed ComponenteTendRed
    {
      get { return (Logicas.CLogicaTrendRed)Componente; }
      set { Componente = value; }
    }

    private List<Int32> mCompromisosGraficosBlockPendientes = new List<int>();
    private void AgregarCompromisoBlock(Int32 Codigo)
		{
      if (!mCompromisosGraficosBlockPendientes.Contains(Codigo))
      {
        mCompromisosGraficosBlockPendientes.Add(Codigo);
      }
		}

   // private async Task LeerDatosUltimoPeriodoAsync(HttpClient Http)
   // {
   //   CInformacionAlarmaCN UltAlarma;
   //   if (Alarmas == null || Alarmas.Count == 0)
   //   {
   //     UltAlarma = await Contenedores.CContenedorDatos.DatosAlarmaIndicadorAsync(Http, Indicador.Codigo,
   //        Indicador.Dimension, CodigoElemento);
   //   }
   //   else
   //   {
   //     UltAlarma = Alarmas.Last();
   //   }

   //   RespuestaDatasetBin Respuesta = await Contenedores.CContenedorDatos.LeerDetalleDatasetAsync(Http,
   //       Indicador.Codigo, Indicador.Dimension, CodigoElemento, UltAlarma.Periodo);
   //   if (Respuesta != null)
			//{
   //     ProcesarRespuestaDataset(Respuesta);
			//}

   // }

   // private bool mbEstaLeyendoUltimoPeriodo = false;

  //  public void AgregarCompromisoBlockAlLeerDatos(HttpClient Http, Int32 Codigo)
		//{
  //    AgregarCompromisoBlock(Codigo);
  //    if (!mbEstaLeyendoUltimoPeriodo)
		//	{
  //      mbEstaLeyendoUltimoPeriodo = true;
  //      _ = LeerDatosUltimoPeriodoAsync(Http);
		//	}
		//}

    public Datos.CProveedorComprimido Proveedor { get; set; } = null;

    public async Task CargarDatosProveedorAsync(HttpClient Http, CContenedorBlocks Blocks)
    {
      bool HayTendencias = (from T in Blocks.Tendencias
                            where T.Reloj == this
                            select T).FirstOrDefault() != null;

      bool HayGraficos = (from G in Blocks.Graficos
                          where G.Reloj == this
                          select G).FirstOrDefault() != null;

      bool HayGrillas = (from G in Blocks.Grillas
                         where G.Indicador != null && G.Indicador.Codigo == Indicador.Codigo
                         select G).FirstOrDefault() != null;

        CLinkContenedorFiltros Filtro = (from F in Blocks.ContenedoresFiltros
                                         where F.Indicador.Codigo == Indicador.Codigo &&
                                           F.CodigoElemento == CodigoElemento
                                         select F).FirstOrDefault();

      // ubicar el periodo.
      if ((HayTendencias || HayGraficos || HayGrillas) && Alarmas == null)
			{
        Alarmas = await Contenedores.CContenedorDatos.ObtenerAlarmasIndicadorAsync(
            Http, Indicador, CodigoElemento, false);
        if (Alarmas != null)
				{
          // si hay una tendencia, imponerla.
          foreach (CLinkTendencia Tendencia in Blocks.Tendencias)
					{
            if (Tendencia.Reloj == this)
						{
              Tendencia.FncRefrescar();
						}
					}
				}
			}

      if (HayGraficos || HayGrillas)
      {
        if (Filtro == null)
        {
          Filtro = await Blocks.AgregarContenedorFiltrosAsync(Http, Indicador,
              CodigoElemento, Clase, ParametrosSC);
        }
      }

      if (Filtro != null)
      {
        // El filtro tiene apuntados los dependientes, con lo que
        // no hace falta enviar los eventos desde el proveedor (creo).
        await Filtro.CargarDatosFiltradorAsync(Http, Blocks, Clase, ParametrosSC);
        //Proveedor = new CProveedorComprimido(ClaseElemento.Indicador, Indicador.Codigo);

        //// Crear las relaciones.
        //foreach (CLinkGraficoCnt Grafico in Blocks.Graficos)
        //{
        //  if (Grafico.Reloj == this)
        //  {
        //    Proveedor.AlAjustarDependientes += Grafico.FncRefrescarGrafico;
        //  }
        //}

      }

      // Leer los datos.
    }

    public CLinkReloj()
    {
      PosicionUnica = gCodigoUnico++;
      Componente = null;
      Filtrador = null;
    }

  }

}
