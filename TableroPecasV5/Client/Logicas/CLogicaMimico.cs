using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.Model;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using TableroPecasV5.Shared;
using Blazorise;
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaMimico : CBaseGrafico
  {

    public event FncEventoRefresco AlRefrescarContenido;

    public CLogicaMimico()
    {
      AnchoCanvas = 0;
      AltoCanvas = 0;
    }

    private bool mbEditandoReporte = false;
    public bool EditandoReporte
		{
      get { return mbEditandoReporte; }
      set
			{
        if (value != mbEditandoReporte)
				{
          mbEditandoReporte = value;
          StateHasChanged();
				}
			}
		}

    [Inject]
    public IJSRuntime JSRuntime { get; set; }

    [Parameter]
    public double DefasajeVertical { get; set; } = 0;

    [Parameter]
    public Plantillas.CLinkContenedorFiltros Filtros { get; set; }

    private bool mbConfigurando = false;
    public bool Configurando
    {
      get { return mbConfigurando; }
      set
      {
        if (mbConfigurando != value)
        {
          mbConfigurando = value;
        }
      }
    }

    private bool mbAjustar = false;
    public bool Ajustar
    {
      get { return mbAjustar; }
      set
      {
        if (mbAjustar != value)
        {
          mbAjustar = value;
        }
      }
    }

    public void ImponerContenido()
		{
      mszFileDialogo = "";
      ModalContenido.Show();
		}

    public bool Posicionando { get; set; } = false;

    private CLogicaDefinirRectangulos mDefinidor = null;
    public CLogicaDefinirRectangulos DefinidorRectangulos
    {
      get { return mDefinidor; }
      set
      {
        if (mDefinidor != value)
        {
          mDefinidor = value;
          mDefinidor.AlRetornar = ImponerPosicion;
        }
      }
    }

    private string mszFiltro = "";
    public string Filtro
		{
       get
			{
        return mszFiltro;
			}
      set
			{
        if (mszFiltro != value)
				{
          mszFiltro = value;
          FiltrarLista();
				}
			}
		}

    private Int32 mElementoSeleccionado = -1;
    public Int32 ElementoSeleccionado
    {
      get { return mElementoSeleccionado; }
      set
      {
        mElementoSeleccionado = value;
        if (mElementoSeleccionado > 0)
        {
          AgregarOEliminarElemento();
          StateHasChanged();
        }
      }
    }

    public void AlCambiarSeleccionElementos()
		{
      StateHasChanged();
		}

    private string NombreElemento(ClaseDetalle Clase, Int32 Codigo)
		{
      switch (Clase)
			{
        case ClaseDetalle.Indicador:
          CDatoIndicador Indicador = Contenedores.CContenedorDatos.IndicadorDesdeCodigo(Codigo);
          return (Indicador == null ? "--" : Indicador.Descripcion);
        case ClaseDetalle.Pregunta:
          CPreguntaCN Pregunta = Contenedores.CContenedorDatos.PreguntaDesdeCodigo(Codigo);
          return (Pregunta == null ? "--" : Pregunta.Pregunta);
        case ClaseDetalle.SalaReunion:
          CSalaCN Sala = Contenedores.CContenedorDatos.UbicarSala(Codigo);
          return (Sala == null ? "--" : Sala.Nombre);
        case ClaseDetalle.Mimico:
          CElementoMimicoCN Mimico = Contenedores.CContenedorDatos.MimicoDesdeCodigo(Codigo);
          return (Mimico == null ? "" : Mimico.Nombre);
        default:
          return "--";
			}
      //Listas.CListaTexto Elemento = (from L in mElementosFiltrados
      //                               where L.Codigo == Codigo
      //                               select L).FirstOrDefault();
      //return (Elemento == null ? "--" : Elemento.Descripcion);
		}

    public void AgregarOEliminarElemento()
    {
      ClaseDetalle Clase = (ClaseDetalle)Categoria;
      ElementoIncluido Existente = (from E in ElementosIncluidos
                                    where E.Clase == Clase && E.Codigo == mElementoSeleccionado
                                    select E).FirstOrDefault();
      if (Existente == null)
      {
        ElementosIncluidos.Add(new ElementoIncluido()
        {
          Clase = Clase,
          Codigo = ElementoSeleccionado,
          Text = NombreElemento(Clase, ElementoSeleccionado)
        });
      }
      else
      {
        ElementosIncluidos.Remove(Existente);
      }
      StateHasChanged();
    }

    public string NombreDelElemento { get; set; } = "";

    public string VinculoDelElemento { get; set; } = "";

    public List<ElementoIncluido> ElementosIncluidos { get; set; } = new List<ElementoIncluido>();

    private List<CDetallePreguntaCN> ObtenerPreguntasEntrada(Int32 CodigoElemento)
    {
      return (from ElementoIncluido E in ElementosIncluidos
              select new CDetallePreguntaCN()
              {
                ClaseDeDetalle = E.Clase,
                ClaseEntidad = (Int32)ClaseElemento.Mimico,
                Codigo = E.Codigo,
                CodigoEntidad = Mimico.MimicoPropio.Codigo,
                CodigoPregunta = CodigoElemento,
                Color = ColorBandera.SinDatos
              }).ToList();
    }

    public void CrearEntrada()
		{
      CerrarMenu();
      if (mCodigoPeguntaEnEdicion == Int32.MinValue)
      {
        Int32 CodigoElemento = gCodigoCorrelativo--;
        List<CDetallePreguntaCN> ListaPreguntas = ObtenerPreguntasEntrada(CodigoElemento);
        CElementoPreguntasCN Punto = new CElementoPreguntasCN()
        {
          Abscisa = mAbscisaEnEdicion / FactorEscala,
          Alto = mAltoEnEdicion / FactorEscala,
          Ancho = mAnchoEnEdicion / FactorEscala,
          ClaseEntidad = (Int32)ClaseElemento.Mimico,
          Codigo = CodigoElemento,
          CodigoEntidad = Mimico.MimicoPropio.Codigo,
          MimicoBase = Mimico.MimicoPropio.MimicoBase,
          Nombre = NombreDelElemento,
          Ordenada = mOrdenadaEnEdicion / FactorEscala,
          PreguntasAsociadas = ListaPreguntas,
          Vinculo = VinculoDelElemento
        };

        Mimico.GruposDePreguntasDelMimico.Add(Punto);
      }
      else
			{
        CElementoPreguntasCN Elemento = (from P in Mimico.GruposDePreguntasDelMimico
                                         where P.Codigo == mCodigoPeguntaEnEdicion
                                         select P).FirstOrDefault();
        Elemento.Nombre = NombreDelElemento;
        Elemento.PreguntasAsociadas = ObtenerPreguntasEntrada(mCodigoPeguntaEnEdicion);
			}

      StateHasChanged();

		}

    private List<Listas.CListaTexto> mListaElementosBruta;

    private void FiltrarLista()
    {
      if (Filtro.Length == 0)
      {
        mElementosFiltrados = new List<Listas.CListaTexto>();
        mElementosFiltrados.AddRange(mListaElementosBruta);
      }
      else
      {
        mElementosFiltrados = (from E in mListaElementosBruta
                               where E.Codigo < 0 ||
                               E.Descripcion.IndexOf(Filtro, StringComparison.InvariantCultureIgnoreCase) >= 0
                               select E).ToList();
      }
      StateHasChanged();
    }

    private List<Listas.CListaTexto> mElementosFiltrados = new List<Listas.CListaTexto>();

    public List<Listas.CListaTexto> ElementosFiltrados
    {
      get { return mElementosFiltrados; }
      set
      {
        if (mElementosFiltrados != value)
        {
          mElementosFiltrados = value;
        }
      }
    }

    private void ActualizarListas()
		{
      switch (mCategoria)
			{
        case (Int32)ClaseDetalle.Indicador:
          mListaElementosBruta = (from CDatoIndicador I in Contenedores.CContenedorDatos.ListaIndicadores
                                  orderby I.Descripcion
                                  select new Listas.CListaTexto(I.Codigo, I.Descripcion)).ToList();
          break;
        case (Int32)ClaseDetalle.Pregunta:
          mListaElementosBruta = (from CPreguntaCN P in Contenedores.CContenedorDatos.ListaPreguntas
                                  orderby P.Pregunta
                                  select new Listas.CListaTexto(P.Codigo, P.Pregunta)).ToList();
          break;
        case (Int32)ClaseDetalle.SalaReunion:
          mListaElementosBruta = (from Listas.CListaTexto L in Contenedores.CContenedorDatos.ListaSalasReunion
                                  orderby L.Descripcion
                                  select L).ToList();
          break;
        case (Int32)ClaseDetalle.Mimico:
          mListaElementosBruta = (from CElementoMimicoCN M in Contenedores.CContenedorDatos.ListaMimicos
                                  orderby M.Nombre
                                  select new Listas.CListaTexto(M.Codigo, M.Nombre)).ToList();
          break;
        default:
          mListaElementosBruta = new List<Listas.CListaTexto>();
          break;
      }
      mListaElementosBruta.Insert(0, new Listas.CListaTexto()
      {
        Codigo = -1,
        Descripcion = "No corresponde"
      });
      FiltrarLista();
    }

    private Int32 mCategoria = -1;
    public Int32 Categoria
    {
      get { return mCategoria; }
      set
      {
        if (mCategoria != value)
        {
          mCategoria = value;
          ActualizarListas();
        }
      }
    }

    public Listas.CListaTexto[] Categorias
		{
      get
			{
        return new Listas.CListaTexto[]
        {
          new Listas.CListaTexto()
          {
            Codigo=(Int32)ClaseDetalle.NoDefinido,
            Descripcion="No corresponde"
          },
          new Listas.CListaTexto()
          {
            Codigo=(Int32)ClaseDetalle.Indicador,
            Descripcion="Indicadores"
          },
          new Listas.CListaTexto()
					{
            Codigo=(Int32)ClaseDetalle.Pregunta,
            Descripcion="Ficha"
          },
          new Listas.CListaTexto()
          {
            Codigo=(Int32)ClaseDetalle.SalaReunion,
            Descripcion="Sala de reuniones"
          },
          new Listas.CListaTexto()
          {
            Codigo=(Int32)ClaseDetalle.Mimico,
            Descripcion="Mímicos"
          }
        };
			}
		}

    private double mAbscisaEnEdicion;
    private double mOrdenadaEnEdicion;
    private double mAnchoEnEdicion;
    private double mAltoEnEdicion;

    private void ImponerPosicion(double Abscisa, double Ordenada, double Ancho, double Alto)
		{
      Posicionando = false;
      mAbscisaEnEdicion = Abscisa;
      mOrdenadaEnEdicion = Ordenada;
      mAnchoEnEdicion = Ancho;
      mAltoEnEdicion = Alto;
      StateHasChanged();
      NombreDelElemento = "";
      VinculoDelElemento = "";
      ElementosIncluidos.Clear();
      if (ModalElementos!=null && !ModalElementos.Visible)
			{
        ModalElementos.Show();
			}
		}

    public void AgregarElemento()
    {
      if (HayImagen)
			{
        mCodigoPeguntaEnEdicion = Int32.MinValue;
        Categoria = -1;
        NombreDelElemento = "";
        ElementoSeleccionado = -1;
        Posicionando = true;
        StateHasChanged();
			}
    }

    public void CfgReporte()
    {
      EditandoReporte = true;
    }

    private async Task<List<CLineaReporte>> ObtenerIndicadoresTareaAsync(string CodigoTarea, DateTime Fecha)
    {
      List<CLineaReporte> Respuesta = new List<CLineaReporte>();
      foreach (CElementoPreguntasCN Grupo in (from P in Mimico.GruposDePreguntasDelMimico
                                              where P.Vinculo == CodigoTarea
                                              select P).ToList())
      {
        foreach (CDetallePreguntaCN Detalle in Grupo.PreguntasAsociadas)
        {
          if (Detalle.ClaseDeDetalle == ClaseDetalle.Indicador)
          {
            CDatoIndicador Indicador = Contenedores.CContenedorDatos.IndicadorDesdeCodigo(Detalle.Codigo);
            if (Indicador != null)
            {
              CInformacionAlarmaCN Datos = await Contenedores.CContenedorDatos.DatosAlarmaIndicadorAsync(
                  Http, Detalle.Codigo, Indicador.Dimension, Detalle.CodigoEntidad);
              if (Datos != null)
              {
                Respuesta.Add(new CLineaReporte()
                {
                  Indicador = Indicador,
                  DatosIndicador = Datos
                });
              }
            }
          }
        }
      }

      return Respuesta;
    }

    private async Task AgregarTareasAReporteAsync(List<CJerarquiaCN> Jerarquias, string Superior, Int32 Tab,
      List<CLineaReporte> Lineas, DateTime Fecha)
    {
      if (Jerarquias.Count > 0)
      {
        foreach (CJerarquiaCN Jerarquia in (from J in Jerarquias
                                            where J.Superior == Superior
                                            orderby J.Orden
                                            select J).ToList())
        {
          CTareaGraficaCN Tarea = (from T in mProceso.Tareas
                                   where T.Codigo == Jerarquia.Inferior
                                   select T).FirstOrDefault();
          if (Tarea != null)
          {
            Lineas.Add(new CLineaReporte()
            {
              Referencia = Tarea.Descripcion,
              TabReferencia = Tab
            });
            Lineas.AddRange(await ObtenerIndicadoresTareaAsync(Jerarquia.Inferior, Fecha));
            await AgregarTareasAReporteAsync(Jerarquias, Jerarquia.Inferior, Tab + 10, Lineas, Fecha);
          }
        }
      }
      else
      {
        foreach (CTareaGraficaCN Tarea in (from T in mProceso.Tareas
                                           orderby T.Descripcion
                                           select T).ToList())
        {
          Lineas.Add(new CLineaReporte()
          {
            Referencia = Tarea.Descripcion,
            TabReferencia = Tab
          });
          Lineas.AddRange(await ObtenerIndicadoresTareaAsync(Tarea.Codigo, Fecha));
        }
      }
    }

    private async Task<List<CJerarquiaCN>> LeerJerarquiasAsync()
    {
      try
      {
        RespuestaJerarquias Respuesta = await Http.GetFromJsonAsync<RespuestaJerarquias>(
            "api/Jerarquias/LeerJerarquias?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
            "&Mimico=" + Mimico.ToString());
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MsgErr);
        }

        return Respuesta.Jerarquias;

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
        return new List<CJerarquiaCN>();
      }
    }

    public async void VerReporte()
    {
      // Leer las jerarquias.
      List<CJerarquiaCN> Jerarquias = await LeerJerarquiasAsync();
      List<CLineaReporte> LineasReporte = new List<CLineaReporte>();
      await AgregarTareasAReporteAsync(Jerarquias, "", 0, LineasReporte, DateTime.Now);

    }

    public List<CTareaGraficaCN> TareasEnProceso
		{
      get { return (mProceso == null ? new List<CTareaGraficaCN>() : mProceso.Tareas); }
		}

    public async void Registrar()
    {
      if (Mimico != null)
			{
        await RegistrarMimicoAsync();
			}
    }

    public async void Borrar()
    {
      var Respuesta = await Http.DeleteAsync("api/Mimicos/BorrarMimico?URL=" +
              Contenedores.CContenedorDatos.UrlBPI +
              "&Ticket=" + Contenedores.CContenedorDatos.Ticket);
      if (!Respuesta.IsSuccessStatusCode)
      {
        throw new Exception(Respuesta.ReasonPhrase);
      }

      Respuesta RespuestaCtrl = await Respuesta.Content.ReadFromJsonAsync<Respuesta>();
      if (!RespuestaCtrl.RespuestaOK)
      {
        throw new Exception(RespuestaCtrl.MsgErr);
      }

      CElementoMimicoCN Anterior = (from M in Contenedores.CContenedorDatos.ListaMimicos
                                    where M.Codigo == Mimico.MimicoPropio.Codigo
                                    select M).FirstOrDefault();
      if (Anterior != null)
      {
        Contenedores.CContenedorDatos.ListaMimicos.Remove(Anterior);
      }

      NavigationManager.NavigateTo("Indicadores");

    }

    public bool NoHayArchivo
		{
      get { return mszFileDialogo.Length == 0; }
		}

    public void AjustarContenido()
		{
      if (mszFileDialogo.Length > 0)
			{
        mszFileDialogo = mszFileDialogo.ToUpper();
        FileInfo FInfo = new FileInfo(mszFileDialogo);
        switch (FInfo.Extension)
				{
          case ".JPEG":
          case ".JPG":
          case ".PNG":
          case ".GIF":
            HayImagen = true;
            HayProceso = false;
            break;
          case ".XPDL":
            HayProceso = true;
            HayImagen = false;
            break;
          default:
            return;
				}
        CerrarMenu();
        StateHasChanged();
			}
		}

    private string mszFileDialogo = "";
    private byte[] mContenidoArchivo = new byte[0];
    public async void ImponerArchivo(FileChangedEventArgs e)
		{
      if (e.Files.Length == 1)
			{
        mszFileDialogo = e.Files[0].Name;
        using (MemoryStream Archivo = new MemoryStream())
				{
          await e.Files[0].WriteToStreamAsync(Archivo);
          Archivo.Seek(0L, SeekOrigin.Begin);
          using (BinaryReader Lector = new BinaryReader(Archivo))
					{
//            mContenidoArchivo = new byte[(int)Archivo.Length];
            mContenidoArchivo = Lector.ReadBytes((int)Archivo.Length);
					}
				}
        StateHasChanged();
			}
		}

    public Int32 AltoMenu
		{
      get { return (Contenedores.CContenedorDatos.EsAdministrador ? 28 : 0); }
		}

    public string EstiloMimico
    {
      get
      {
        if (HayImagen)
        {
          return "height: calc(100% - "+AltoMenu.ToString()+"px); width: 100%; text-align: left; margin-left: 0px; margin-top: " +
            AltoMenu.ToString() + "px; overflow-y: hidden; " +
            "overflow-x: hidden; background: white; position: absolute;"; // + (HayImagen ? "hidden" : "auto") + ";";
        }
        else
        {
          return "height: " + Math.Floor(AltoImpuesto > 0 ? AltoImpuesto : AltoNecesario).ToString() +
            "px; width: " + Math.Floor(AnchoImpuesto > 0 ? AnchoImpuesto : AnchoNecesario).ToString() +
            "px; text-align: left; margin-left: 0px; margin-top: " + AltoMenu.ToString() +
            "px; overflow-y: hidden; overflow-x: hidden; background: white; position: absolute;"; // + (HayImagen ? "hidden" : "auto") + ";";
        }
      }
    }

    public string EstiloContenido
    {
      get
      {
        if (HayImagen)
        {
          return "height: " + Escalar(mAltoImg).ToString() +
            "px; width: " + Escalar(mAnchoImg).ToString() +
            "px; margin-top: " + AltoMenu.ToString() +
            "px; text-align: left; margin-left: 0px; margin-top: 0px; overflow: hidden; position: absolute;";
        }
        else
        {
          return "height: 100%" + //AltoNecesario.ToString() +
            "; width: 100%" + //AnchoNecesario.ToString() +
            "; margin-top: " + AltoMenu.ToString() +
            "px; text-align: left; margin-left: 0px; overflow: auto; background: transparent; position: absolute;";
        }
      }
    }

    [Parameter]
    public double FactorEscala { get; set; } = 1;

    [Parameter]
    public double AnchoImpuesto { get; set; } = -1;

    [Parameter]
    public double AltoImpuesto { get; set; } = -1;

    [Parameter]
    public long AnchoCanvas { get; set; }

    [Parameter]
    public long AltoCanvas { get; set; }

    public string Escala
    {
      get
      {
        return "scale(" + CRutinas.FloatVStr(FactorEscala) + ", " + CRutinas.FloatVStr(FactorEscala) + ");";
      }
    }

    public BECanvas CanvasGrafico;
    private Canvas2DContext mContexto;

    public CMimicoCN Mimico { get; set; } = null;
    private CProcesoGraficoCN mProceso = null;
    private CImagenCN mImagen = null;

    public bool HayProceso { get; set; } = false;

    public bool HayImagen { get; set; } = false;
    private Int32 mAnchoImg = 100;
    private Int32 mAltoImg = 100;

    public string ContenidoImagen { get; set; }

    public double AnchoPantallaCompleta
    {
      get { return Math.Floor(Contenedores.CContenedorDatos.AnchoPantalla - 4d); }
    }

    public double AltoPantallaCompleta
    {
      get { return Math.Floor(Contenedores.CContenedorDatos.AltoPantalla - 5d); }
    }

    public void AjustarFactorEscala(double AnchoAhora, double AltoAhora)
    {
      AnchoImpuesto = AnchoAhora;
      AltoImpuesto = AltoAhora;
      AjustarFactorEscala();
    }

    public void AjustarFactorEscala()
    {
      if (HayImagen)
      {
        if (AnchoImpuesto > 0)
        {
          FactorEscala = Math.Min(1, Math.Min(AnchoImpuesto / AnchoNecesario, AltoImpuesto / AltoNecesario));
        }
        else
        {
          FactorEscala = Math.Min(1, Math.Min(
              AnchoPantallaCompleta / AnchoNecesario,
              AltoPantallaCompleta / AltoNecesario));
        }
      }
    }


    private void IncorporarDatosImagen()
    {
      try
      {
        ContenidoImagen = mImagen.UrlImagen;
        mAnchoImg = mImagen.Ancho;
        mAltoImg = mImagen.Alto;
        HayImagen = true;
        AjustarFactorEscala();
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    public string EstiloImagen
    {
      get
      {
        return "width: " + Escalar(mAnchoImg).ToString() + "px; height: " +
      Escalar(mAltoImg).ToString() + "px; overflow-x: hidden; overflow-y: hidden;";
      }
    }

    //public string EstiloDivCanvas
    //{
    //  get
    //  {
    //    return "position: absolute; margin: 0px; width: " + AnchoCanvas.ToString() + "px; Height: " + AltoCanvas.ToString() + "px;";
    //  }
    //}

    private void AjustarDimensionesProceso()
    {
      double AbscMax = double.MinValue;
      double OrdMax = double.MinValue;

      foreach (CTareaGraficaCN Tarea in mProceso.Tareas)
      {
        AbscMax = Math.Max(AbscMax, Tarea.Abscisa + Tarea.Ancho);
        OrdMax = Math.Max(OrdMax, Tarea.Ordenada + Tarea.Alto);
      }

      foreach (CRomboGraficoCN Rombo in mProceso.Rombos)
      {
        AbscMax = Math.Max(AbscMax, Rombo.Abscisa + Rombo.Ancho);
        OrdMax = Math.Max(OrdMax, Rombo.Ordenada + Rombo.Alto);
      }

      foreach (CPoolGraficoCN Pool in mProceso.Pools)
      {
        foreach (CLineaGraficaCN Linea in Pool.Lineas)
        {
          AbscMax = Math.Max(AbscMax, Linea.Abscisa + Linea.Ancho);
          OrdMax = Math.Max(OrdMax, Linea.Ordenada + Linea.Alto);
        }
      }
      foreach (CFlechaGraficaCN Flecha in mProceso.Flechas)
      {
        foreach (CPuntoGraficoCN Punto in Flecha.Puntos)
        {
          AbscMax = Math.Max(AbscMax, Punto.Abscisa);
          OrdMax = Math.Max(OrdMax, Punto.Ordenada);
        }
      }

      AnchoCanvas = (Int32)Math.Floor(AbscMax + 1);
      AltoCanvas = (Int32)Math.Floor(OrdMax + 1);
    }

    [Inject]
    public HttpClient Http { get; set; }

    private async Task LeerDatosMimicoAsync()
    {
      try
      {
        RespuestaMimico Respuesta = await Http.GetFromJsonAsync<RespuestaMimico>(
            "api/Mimicos/LeerMimico?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
            "&CodigoUnico=" + CodigoUnico.ToString());
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MsgErr);
        }
        Mimico = Respuesta.Mimico;
        mProceso = Respuesta.Proceso;
        mImagen = Respuesta.Imagen;
        if (mImagen.UrlImagen.Length > 0)
        {
          AnchoCanvas = 0;
          AltoCanvas = 0;
          IncorporarDatosImagen();
        }
        else
        {
          if (mProceso != null)
          {
            HayImagen = false;
            FactorEscala = 1;
            mAnchoImg = 0;
            mAltoImg = 0;
            AjustarDimensionesProceso();
            HayProceso = true;
          }
        }
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    private void AjustarPosicionPregunta(CElementoPreguntasCN Pregunta)
    {
      CTareaGraficaCN Tarea = (from T in mProceso.Tareas
                               where T.Codigo == Pregunta.Vinculo
                               select T).FirstOrDefault();
      if (Tarea != null)
      {
        Pregunta.Abscisa = Tarea.Abscisa;
        Pregunta.Ordenada = Tarea.Ordenada;
        Pregunta.Ancho = Tarea.Ancho;
        Pregunta.Alto = Tarea.Alto;
      }
      else
      {
        CRomboGraficoCN Rombo = (from R in mProceso.Rombos
                                 where R.Codigo == Pregunta.Vinculo
                                 select R).FirstOrDefault();
        if (Rombo != null)
        {
          Pregunta.Abscisa = Rombo.Abscisa;
          Pregunta.Ordenada = Rombo.Ordenada;
          Pregunta.Ancho = Rombo.Ancho;
          Pregunta.Alto = Rombo.Alto;
        }
      }
    }

    private double Escalar(double R)
    {
      return Math.Floor(FactorEscala * R);
    }

    public string Estilo(CElementoPreguntasCN Pregunta)
    {
      if (Pregunta.Vinculo != null && Pregunta.Vinculo.Length > 0 && Pregunta.Abscisa < 0.001 && Pregunta.Ordenada < 0.001)
      {
        AjustarPosicionPregunta(Pregunta);
      }

      return "width: " + Escalar(Pregunta.Ancho).ToString() + "px; height: " + Escalar(Pregunta.Alto).ToString() +
        "px; position: absolute; left: " + Escalar(Pregunta.Abscisa).ToString() + "px; top: " +
        (Escalar(Pregunta.Ordenada) + DefasajeVertical).ToString() +
        "px; cursor: pointer; background-color: transparent; overflow: hidden;";

    }

    public string PreguntaElegida { get; set; } = "";

    [Inject]
    NavigationManager NavigationManager { get; set; }

    private void NavegarAIndicador(Int32 Codigo)
    {
      NavigationManager.NavigateTo("DetalleIndicador/" + Codigo.ToString(), false);
    }

    private void NavegarAFicha(Int32 Codigo)
    {
      NavigationManager.NavigateTo("Tarjeta/" + Codigo.ToString(), false);
    }

    private void NavegarAMimico(Int32 Codigo)
    {
      NavigationManager.NavigateTo("Mimicos/" + Codigo.ToString(), false);
    }

    private void NavegarASala(Int32 Codigo)
    {
      NavigationManager.NavigateTo("SalaReunion/" + Codigo.ToString(), false);
    }

    public void Moverse()
    {
      if (PreguntaElegida != null && PreguntaElegida.Length > 0)
      {
        if (PreguntaElegida != "-1")
        {
          switch (PreguntaElegida.Substring(0, 1))
          {
            case "I":
              NavegarAIndicador(Int32.Parse(PreguntaElegida.Substring(2)));
              break;
            case "P":
              NavegarAFicha(Int32.Parse(PreguntaElegida.Substring(2)));
              break;
            case "S":
              NavegarASala(Int32.Parse(PreguntaElegida.Substring(2)));
              break;
          }
        }
        CerrarMenu();
      }
    }

    public void CerrarMenu()
    {
      if (ModalMenu != null && ModalMenu.Visible)
      {
        ModalMenu.Hide();
      }

      if (ModalContenido != null && ModalContenido.Visible)
      {
        ModalContenido.Hide();
      }

      if (ModalDefinir != null && ModalDefinir.Visible)
      {
        ModalDefinir.Hide();
      }

      if (ModalElementos != null && ModalElementos.Visible)
      {
        ModalElementos.Hide();
      }

    }

    public Int32 Comite { get; set; } = -1;

    public string Denominacion { get; set; } = "";

    public bool NoHayMimico
    {
      get
      {
        return (Denominacion.Length == 0 && Comite > 0 && mContenidoArchivo.Length > 0);
      }
    }

    public bool NoHayElementos
    {
      get
      {
        return ElementosIncluidos.Count == 0;
      }
    }

    private static Int32 gCodigoCorrelativo = -1;

    public async void CrearMimico()
    {
      CerrarMenu();
      Mimico = new CMimicoCN();
      Int32 CodigoPropio = gCodigoCorrelativo--;
      Mimico.MimicoPropio = new CElementoMimicoCN()
      {
        Abscisa = 0,
        Alto = 100,
        Ancho = 100,
        Codigo = CodigoPropio,
        Comite = Comite,
        MimicoBase = CodigoPropio,
        Nombre = Denominacion,
        Ordenada = 0,
        Vinculo = ""
      };
      Mimico.ImagenesBinarias.Add(new CImagenBinariaCN()
      {
        Codigo = gCodigoCorrelativo--,
        Mimico = CodigoPropio,
        DatosSucios = true,
        Imagen = mContenidoArchivo
      });

      // Registra.
      await RegistrarMimicoAsync();

    }

    private async Task RegistrarMimicoAsync()
    {

      var Respuesta = await Http.PostAsJsonAsync<CMimicoCN>("api/Mimicos/InsertarMimico?URL=" +
              Contenedores.CContenedorDatos.UrlBPI +
              "&Ticket=" + Contenedores.CContenedorDatos.Ticket, Mimico);
      if (!Respuesta.IsSuccessStatusCode)
      {
        throw new Exception(Respuesta.ReasonPhrase);
      }

      RespuestaCodigos RespuestaCodigo = await Respuesta.Content.ReadFromJsonAsync<RespuestaCodigos>();
      if (!RespuestaCodigo.RespuestaOK)
      {
        throw new Exception(RespuestaCodigo.MsgErr);
      }

      CodigoUnico = (Mimico.MimicoPropio.Codigo > 0 ? Mimico.MimicoPropio.Codigo :
          CRutinas.TraducirCodigo(Mimico.MimicoPropio.Codigo, RespuestaCodigo.ParesDeCodigosMimicos));

      // Lee.
      await LeerDatosMimicoAsync();

      Mimico.MimicoPropio.Codigo = CodigoUnico;
      Mimico.MimicoPropio.MimicoBase = CodigoUnico;

      CElementoMimicoCN Anterior = (from M in Contenedores.CContenedorDatos.ListaMimicos
                                    where M.Codigo == CodigoUnico
                                    select M).FirstOrDefault();
      if (Anterior != null)
			{
        Contenedores.CContenedorDatos.ListaMimicos.Remove(Anterior);
      }

      Contenedores.CContenedorDatos.ListaMimicos.Add(Mimico.MimicoPropio);

      StateHasChanged();

    }

    public string EstiloPregunta(CDetallePreguntaCN Pregunta)
    {
      return "background-color: " + CRutinas.ColorAclarado(Pregunta.Color) + ";";
    }

    public string CodigoPregunta(CDetallePreguntaCN Pregunta)
    {
      switch (Pregunta.ClaseDeDetalle)
      {
        case ClaseDetalle.Indicador: return "I-" + Pregunta.Codigo.ToString();
        case ClaseDetalle.Pregunta: return "P-" + Pregunta.Codigo.ToString();
        case ClaseDetalle.SalaReunion: return "S-" + Pregunta.Codigo.ToString();
        default: return "-1";
      }
    }

    public string TextoPregunta(CDetallePreguntaCN Pregunta)
    {
      switch (Pregunta.ClaseDeDetalle)
      {
        case ClaseDetalle.Indicador:
          CDatoIndicador Indicador = Contenedores.CContenedorDatos.IndicadorDesdeCodigo(Pregunta.Codigo);
          return "Ind " + (Indicador == null ? "--" : Indicador.Descripcion);
        case ClaseDetalle.Pregunta:
          CPreguntaCN PreguntaLocal = Contenedores.CContenedorDatos.UbicarPregunta(Pregunta.Codigo);
          return "Ficha " + (PreguntaLocal == null ? "--" : PreguntaLocal.Pregunta);
        case ClaseDetalle.SalaReunion:
          CSalaCN SalaLocal = Contenedores.CContenedorDatos.UbicarSala(Pregunta.Codigo);
          return "Sala " + (SalaLocal == null ? "--" : SalaLocal.Nombre);
        default:
          return "--";
      }
    }

    //public void ClickEnArea(string Vinculo)
    //{
    //  if (Vinculo.Length == 0)
    //  {
    //    return;
    //  }

    //  // Armar preguntas.
    //  foreach (CElementoPreguntasCN Elemento in Mimico.GruposDePreguntasDelMimico)
    //  {
    //    if (Elemento.Vinculo == Vinculo || (Elemento.Vinculo.Length == 0 && Elemento.Nombre == Vinculo))
    //    {
    //      Preguntas = Elemento.PreguntasAsociadas;
    //      if (Preguntas.Count == 1)
    //      {
    //        if (Preguntas[0].ClaseDeDetalle == ClaseDetalle.Mimico)
    //        {
    //          NavegarAMimico(Preguntas[0].Codigo);
    //          return;
    //        }
    //        PreguntaElegida = CodigoPregunta(Preguntas[0]);
    //        Moverse();
    //      }
    //      if (ModalMenu != null)
    //      {
    //        PreguntaElegida = "";
    //        ModalMenu.Show();
    //      }
    //    }
    //  }
    //}

    private Int32 mCodigoPeguntaEnEdicion = Int32.MinValue;

    public void ClickEnArea(Int32 Codigo)
    {

      CElementoPreguntasCN Elemento = (from E in Mimico.GruposDePreguntasDelMimico
                                       where E.Codigo == Codigo
                                       select E).FirstOrDefault();
      if (Elemento != null)
      {

        if (Configurando)
        {
          Categoria = -1;
          NombreDelElemento = Elemento.Nombre;
          mAbscisaEnEdicion = Elemento.Abscisa;
          mOrdenadaEnEdicion = Elemento.Ordenada;
          mAnchoEnEdicion = Elemento.Ancho;
          mAltoEnEdicion = Elemento.Alto;
          ElementoSeleccionado = -1;
          mCodigoPeguntaEnEdicion = Elemento.Codigo;
          VinculoDelElemento = Elemento.Vinculo;
          ElementosIncluidos = (from P in Elemento.PreguntasAsociadas
                                select new ElementoIncluido()
                                {
                                  Clase = P.ClaseDeDetalle,
                                  Codigo = P.Codigo,
                                  Text = NombreElemento(P.ClaseDeDetalle, P.Codigo)
                                }).ToList();
          ModalElementos.Show();
        }
        else
        {
          // Armar preguntas.
          Preguntas = Elemento.PreguntasAsociadas;
          if (Preguntas.Count == 1)
          {
            if (Preguntas[0].ClaseDeDetalle == ClaseDetalle.Mimico)
            {
              NavegarAMimico(Preguntas[0].Codigo);
              return;
            }
            PreguntaElegida = CodigoPregunta(Preguntas[0]);
            Moverse();
          }
          if (ModalMenu != null)
          {
            PreguntaElegida = "";
            ModalMenu.Show();
          }
        }
      }
    }

    public Modal ModalMenu { get; set; }

    public Modal ModalContenido { get; set; }

    private Modal mModalDefinir = null;
    public Modal ModalDefinir
    {
      get { return mModalDefinir; }
      set
      {
        if (mModalDefinir != value)
        {
          mModalDefinir = value;
          if (CodigoUnico<0 && !mModalDefinir.Visible)
					{
            mModalDefinir.Show();
					}
        }
      }
    }

    public Modal ModalElementos { get; set; }

    public List<CDetallePreguntaCN> Preguntas { get; set; }

    private double mDimensionCaracter;

    //private async Task<List<string>> ObtenerLineasEnBlockAsync(Canvas2DContext Contexto, string Texto, double Ancho, double Alto)
    //{
    //  List<string> Respuesta = new List<string>();
    //  string[] Palabras = Texto.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    //  string LineaAnterior = "";
    //  for (Int32 i = 0; i < Palabras.Length; i++)
    //  {
    //    string Linea = LineaAnterior + (LineaAnterior.Length > 0 ? " " : "") + Palabras[i];
    //    TextMetrics Medida = await Contexto.MeasureTextAsync(Linea);
    //    if (Medida.Width < Ancho)
    //    {
    //      LineaAnterior = Linea;
    //    }
    //    else
    //    {
    //      if (LineaAnterior.Length == 0)
    //      {
    //        Respuesta.Add(Palabras[i]);
    //        LineaAnterior = "";
    //      }
    //      else
    //      {
    //        Respuesta.Add(LineaAnterior);
    //        LineaAnterior = Palabras[i];
    //      }
    //    }
    //  }
    //  if (LineaAnterior.Length > 0)
    //  {
    //    Respuesta.Add(LineaAnterior);
    //  }
    //  return Respuesta;
    //}

    private async Task PonerEtiquetaVerticalAsync(Canvas2DContext Contexto,
          double Abscisa, double Ordenada, double Ancho, double Alto, string Texto)
    {
      await Contexto.SaveAsync();
      TextMetrics Medidas = await Contexto.MeasureTextAsync(Texto);
      await Contexto.TranslateAsync(Abscisa + (Ancho + mDimensionCaracter) / 2, Ordenada + (Alto + Medidas.Width) / 2);
      await Contexto.RotateAsync(4.712f); // 270); // (float)(1.5*Math.PI));
      await Contexto.FillTextAsync(Texto, 0, 0);
      await Contexto.RestoreAsync();
    }

    //private async Task PonerEtiquetaHorizontalAsync(Canvas2DContext Contexto,
    //      double Abscisa, double Ordenada, double Ancho, double Alto, string Texto)
    //{
    //  TextMetrics Medidas = await Contexto.MeasureTextAsync(Texto);
    //  List<string> Lineas = new List<string>();
    //  if (Medidas.Width < Ancho)
    //  {
    //    Lineas.Add(Texto);
    //  }
    //  else
    //  {
    //    Lineas = await ObtenerLineasEnBlockAsync(Contexto, Texto, Ancho, Alto);
    //  }
    //  Int32 LineasMax = Math.Min(Lineas.Count, (Int32)Math.Floor(Alto / (mDimensionCaracter + 2)));
    //  double Ord0 = Ordenada + (Alto - LineasMax * mDimensionCaracter + (LineasMax - 1) * 2) / 2 + mDimensionCaracter;
    //  for (Int32 i = 0; i < LineasMax; i++)
    //  {
    //    TextMetrics Medida = await Contexto.MeasureTextAsync(Lineas[i]);
    //    await Contexto.FillTextAsync(Lineas[i], Abscisa + (Ancho - Medida.Width) / 2, Ord0);
    //    Ord0 += mDimensionCaracter + 2;
    //  }
    //}

    private async Task DibujarRectanguloAsync(Canvas2DContext Contexto, double AbscMin, double OrdMin, double Ancho, double Alto,
        string Texto = "", string ColorFondo = "", bool TextoVertical = false)
    {
      if (ColorFondo.Length > 0)
      {
        await Contexto.SaveAsync();
        await Contexto.SetFillStyleAsync(ColorFondo);
        await Contexto.FillRectAsync(AbscMin, OrdMin, Ancho, Alto);
        await Contexto.RestoreAsync();
      }
      await Contexto.StrokeRectAsync(AbscMin, OrdMin, Ancho, Alto);
      if (Texto.Length > 0)
      {
        if (TextoVertical)
        {
          await PonerEtiquetaVerticalAsync(Contexto, AbscMin, OrdMin, Ancho, Alto, Texto);
        }
        else
        {
          await CRutinas.PonerEtiquetaHorizontalAsync(Contexto, AbscMin, OrdMin, Ancho, Alto, Texto, mDimensionCaracter);
        }
      }
    }

    private async Task DibujarFlechaAsync(Canvas2DContext Contexto, CFlechaGraficaCN Flecha)
    {
      if (Flecha.Puntos.Count > 1)
      {
        await Contexto.BeginPathAsync();
        try
        {
          await Contexto.MoveToAsync(Flecha.Puntos[0].Abscisa, Flecha.Puntos[0].Ordenada);
          for (Int32 i = 1; i < Flecha.Puntos.Count; i++)
          {
            await Contexto.LineToAsync(Flecha.Puntos[i].Abscisa, Flecha.Puntos[i].Ordenada);
          }
          await Contexto.SetStrokeStyleAsync("gray");
          await Contexto.StrokeAsync();
        }
        finally
        {
          await Contexto.ClosePathAsync();
        }
      }
    }

    private async Task DibujarRomboAsync(Canvas2DContext Contexto, double AbscMin, double OrdMin, double Ancho, double Alto,
        string ColorFondo = "")
    {
      await Contexto.BeginPathAsync();
      try
      {
        await Contexto.MoveToAsync(AbscMin + Ancho / 2, OrdMin);
        await Contexto.LineToAsync(AbscMin + Ancho, OrdMin + Alto / 2);
        await Contexto.LineToAsync(AbscMin + Ancho / 2, OrdMin + Alto);
        await Contexto.LineToAsync(AbscMin, OrdMin + Alto / 2);
        await Contexto.LineToAsync(AbscMin + Ancho / 2, OrdMin);
        if (ColorFondo.Length > 0)
        {
          await Contexto.SetFillStyleAsync(ColorFondo);
          await Contexto.FillAsync();
        }
        await Contexto.StrokeAsync();
      }
      finally
      {
        await Contexto.ClosePathAsync();
      }
    }

    public double AnchoNecesario
    {
      get { return Math.Max(mAnchoImg, AnchoCanvas) + 10; }
    }

    public double AltoNecesario
    {
      get { return Math.Max(mAltoImg, AltoCanvas) + 10; }
    }

    private Int32 mCodigoLeido = -1;

    private async Task<ColorBandera> ColorTareaAsync(CTareaGraficaCN Tarea)
    {
      if (Mimico != null)
      {
        foreach (CElementoPreguntasCN Pregunta in Mimico.GruposDePreguntasDelMimico)
        {
          if (Pregunta.Vinculo == Tarea.Codigo)
          {
            return await CRutinas.ObtenerColorBanderaAsync(Http, Pregunta.PreguntasAsociadas);
          }
        }
      }
      return ColorBandera.SinDatos;
    }

    public string EstiloDivCanvas
		{
      get
			{
        return "width: " + AnchoCanvas.ToString() + "px; height: " + AltoCanvas.ToString() +
          "px; position: absolute; background: transparent; margin-left: 0px; margin-top: 0px;";
			}
		}

    public void UbicarElemento(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
      if (HayProceso)
      {
        CTareaGraficaCN TareaRefe = null;
        foreach (CTareaGraficaCN Tarea in mProceso.Tareas)
        {
          if (e.OffsetX >= Tarea.Abscisa && e.OffsetX < (Tarea.Abscisa + Tarea.Ancho) &&
            e.OffsetY >= Tarea.Ordenada && e.OffsetY < (Tarea.Ordenada + Tarea.Alto))
          {
            TareaRefe = Tarea;
            break;
          }
        }
        if (TareaRefe != null)
        {
          CElementoPreguntasCN Elemento = (from E in Mimico.GruposDePreguntasDelMimico
                                           where E.Vinculo == TareaRefe.Codigo
                                           select E).FirstOrDefault();
          if (Configurando && Elemento == null)
          {
            Posicionando = false;
            mAbscisaEnEdicion = TareaRefe.Abscisa;
            mOrdenadaEnEdicion = TareaRefe.Ordenada;
            mAnchoEnEdicion = TareaRefe.Ancho;
            mAltoEnEdicion = TareaRefe.Alto;
            NombreDelElemento = TareaRefe.Descripcion;
            VinculoDelElemento = TareaRefe.Codigo;
            ElementosIncluidos.Clear();
            if (ModalElementos != null && !ModalElementos.Visible)
            {
              ModalElementos.Show();
            }
          }
          else
          {
            if (Elemento != null)
            {
              ClickEnArea(Elemento.Codigo);
            }
          }
        }
      }
    }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
      if (Mimico == null || mCodigoLeido != CodigoUnico)
      {
        mCodigoLeido = CodigoUnico;
        if (CodigoUnico > 0)
        {
          await LeerDatosMimicoAsync();
        }
        else
				{
          Mimico = new CMimicoCN();
          Mimico.MimicoPropio.Codigo = -1;
          mProceso = null;
          mImagen = new CImagenCN()
          {
            Ancho = 0,
            Alto = 0,
            UrlImagen = "",
            Codigo = -1,
            MimicoBase = -1
          };
          HayImagen = false;
          HayProceso = false;
        }
        if (Mimico != null)
        {
          if (AlRefrescarContenido != null)
          {
            AlRefrescarContenido(null);
          }
          StateHasChanged();
        }
      }
      else
      {
        if (HayProceso)
        {
          try
          {

            mContexto = await Blazor.Extensions.CanvasContextExtensions.CreateCanvas2DAsync(CanvasGrafico);

            await mContexto.BeginBatchAsync();

            try
            {

              await mContexto.ClearRectAsync(0, 0, AnchoCanvas, AltoCanvas);
              await mContexto.SetFillStyleAsync("white");
              await mContexto.FillRectAsync(0, 0, AnchoCanvas, AltoCanvas);

              await mContexto.SetFontAsync("10px serif");
              TextMetrics Medida = await mContexto.MeasureTextAsync("H");
              mDimensionCaracter = Medida.Width + 2;

              await mContexto.SetLineWidthAsync(1);
              await mContexto.SetStrokeStyleAsync("black");
              await mContexto.SetFillStyleAsync("#000000");

              foreach (CPoolGraficoCN Pool in mProceso.Pools)
              {
                if (Pool.Lineas.Count > 0)
                {
                  // Dibujar la zona del pool (con el nombre vertical).
                  double OrdMin = Pool.Lineas[0].Ordenada;
                  double OrdMax = Pool.Lineas.Last().Ordenada + Pool.Lineas.Last().Alto;
                  await DibujarRectanguloAsync(mContexto, 0, OrdMin, Pool.Lineas[0].Abscisa / 2, OrdMax, Pool.Descripcion,
                    "", true);
                  foreach (CLineaGraficaCN Linea in Pool.Lineas)
                  {
                    await DibujarRectanguloAsync(mContexto, Linea.Abscisa / 2, Linea.Ordenada, Linea.Abscisa / 2, Linea.Alto,
                        Linea.Descripcion, "", true);
                    await DibujarRectanguloAsync(mContexto, Linea.Abscisa, Linea.Ordenada, Linea.Ancho, Linea.Alto);
                  }
                }
              }

              foreach (CFlechaGraficaCN Flecha in mProceso.Flechas)
              {
                await DibujarFlechaAsync(mContexto, Flecha);
              }

              await mContexto.SetFontAsync("10px serif");

              foreach (CTareaGraficaCN Tarea in mProceso.Tareas)
              {
                await DibujarRectanguloAsync(mContexto, Tarea.Abscisa, Tarea.Ordenada, Tarea.Ancho, Tarea.Alto,
                    Tarea.Descripcion, CRutinas.ColorAclarado(await ColorTareaAsync(Tarea)));
              }

              foreach (CRomboGraficoCN Rombo in mProceso.Rombos)
              {
                await DibujarRomboAsync(mContexto, Rombo.Abscisa, Rombo.Ordenada, Rombo.Ancho, Rombo.Alto,
                    "lightgray");
              }

            }
            finally
            {
              await mContexto.EndBatchAsync();
            }

          }
          catch (Exception ex)
          {
            CRutinas.DesplegarMsg(ex);
          }
        }
      }
    }
  }

  public class ElementoIncluido
	{
    public ClaseDetalle Clase { get; set; }
    public Int32 Codigo { get; set; }
    public string Text { get; set; }

	}

}
