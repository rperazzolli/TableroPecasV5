using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Json;
using Blazorise;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Client.Plantillas;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaSalaReunion : ComponentBase
  {

    public CLogicaSalaReunion()
    {
      Componentes.CBaseGrafico.BordeSuperior = 100;
      UnionSolapaTab.FncRefrescar = FncRefrescarSolapa;
      if (Contenedores.CContenedorDatos.EsAdministrador)
			{
        CrearSolapaCrear();
        CrearSolapaEditar();
			}
//      CLinkGrafico.FncRefrescarGraficos = AlRefrescarGrafico;
      gPuntero = this;
    }

    private void CrearSolapaCrear()
		{
      CSolapaCN Solapa = new CSolapaCN()
      {
        Codigo = -1,
        Nombre = "+"
      };
      SolapaCrear = new UnionSolapaTab()
      {
        Solapa = Solapa
      };
		}

    private void CrearSolapaEditar()
    {
      CSolapaCN Solapa = new CSolapaCN()
      {
        Codigo = -2,
        Nombre = "Editar"
      };
      SolapaEditar = new UnionSolapaTab()
      {
        Solapa = Solapa
      };
    }

    public void AlRefrescarGrafico(string Nombre, bool Abre)
    {
      StateHasChanged();
    }

    public static CLogicaSalaReunion gPuntero = null;

    private Int32 mCodigo = -1;

    [Parameter]
    public Int32 Codigo
    {
      get { return mCodigo; }
      set
      {
        //if (mCodigo != value)
        //{
          mCodigo = value;
          if (mCodigo >= 0)
          {
            Sala = Contenedores.CContenedorDatos.UbicarSala(mCodigo);
            if (Sala == null)
            {
              Sala = new CSalaCN()
              {
                Codigo = mCodigo,
                Nombre = "...."
              };
            }
            Solapas = (from S in Contenedores.CContenedorDatos.SolapasEnSala(mCodigo)
                       select new UnionSolapaTab() { Solapa = S }).ToList();
            PreguntasEnSolapa = new List<UnionPreguntaTab>();
            if (Solapas.Count > 0)
						{
              FncRefrescarSolapa(Solapas[0].Solapa.Codigo);
						}
          }
          StateHasChanged();
        //}
      }
    }

    public void CerrarEditarXML(string XML)
		{
      EditarSolapa = false;
      if (mSolapaEnEdicion != null)
			{
        mSolapaEnEdicion.Solapa.Block = XML;
			}
      AjustarListaPreguntas(true);
		}

    [JSInvokable]
    public static Task<string> ProcesarPedidoRefrescoAsync(string Nombre)
    {
      string Msg = "";
      try
      {
        if (gPuntero != null)
        {
        }
      }
      catch (Exception ex)
      {
        Msg = ex.Message;
      }
      return Task.FromResult(Msg);
    }

    //public bool PrimeraSolapa(CSolapaCN Solapa)
    //{
    //  return (Solapa == null || Solapas == null || Solapas.Count == 0 ? false : Solapa == Solapas.First().Solapa);
    //}

    [Inject]
    NavigationManager NavigationManager { get; set; }

    public void MoverseAFicha(Int32 Codigo)
    {
      NavigationManager.NavigateTo("Tarjeta/" + Codigo.ToString(), false);
    }

    public string EstiloCompleto
    {
      get
      {
        return "width: " + (Contenedores.CContenedorDatos.AnchoPantalla - 12).ToString()+
          "px; height: "+ Contenedores.CContenedorDatos.AltoPantalla.ToString()+
          "px; display: block; overflow: hidden; padding: 0px; position: absolute;";
      }
    }

    public string EstiloSuperior
    {
      get
      {
        return "width: 100%; height: "+ (Contenedores.CContenedorDatos.AltoOpcionSolapa + 5).ToString()+
            "px; position: absolute; display: inline-block; overflow: hidden; margin-top: 0px; margin-left: 5px;";
      }
    }

    public static string EstiloTarjeta(UnionPreguntaTab Pregunta)
    {
      return "width: " + CLogicaTarjeta.ANCHO_TARJETA.ToString() + "px; height: " + CLogicaTarjeta.ALTO_TARJETA.ToString() +
        "px; position: absolute; margin-left: " + Math.Floor(Pregunta.Abscisa + 0.5).ToString() +
        "px; margin-top: " + Math.Floor(Pregunta.Ordenada + 0.5).ToString() + "px; background-color: lightgray;";
    }

    public string EstiloElemento
    {
      get
      {
        return "width: " + Contenedores.CContenedorDatos.AnchoOpcionSolapa.ToString() + "px; height: " +
          (Contenedores.CContenedorDatos.AltoOpcionSolapa + 5).ToString() +
          "px; display: inline-block; overflow: hidden; margin-top: 5px;";
      }
    }

    public string EstiloAgregarSolapa
    {
      get
      {
        return "width: 25px; height: " +
          (Contenedores.CContenedorDatos.AltoOpcionSolapa + 5).ToString() +
          "px; display: inline-block; overflow: hidden; margin-top: 5px;";
      }
    }

    public string EstiloEditarSolapa
    {
      get
      {
        return "width: 40px; height: " +
          (Contenedores.CContenedorDatos.AltoOpcionSolapa + 5).ToString() +
          "px; display: inline-block; overflow: hidden; margin-top: 5px;";
      }
    }

    private Int32 mCantColumnas=-1;
    private double mSeparacion;

    private void DeterminarColumnas()
    {
      mCantColumnas = Math.Max(1,
          (Int32)Math.Floor((Contenedores.CContenedorDatos.AnchoPantalla - 25) / (double)(CLogicaTarjeta.ANCHO_TARJETA + 25)));
      mSeparacion = (Contenedores.CContenedorDatos.AnchoPantalla - mCantColumnas * CLogicaTarjeta.ANCHO_TARJETA) / (mCantColumnas + 1);
    }

    private List<CPreguntaCN> mPreguntas = null;
    private string mszXML = "";
    public string XMLContenedor
    {
      get { return mszXML; }
    }

    public List<UnionPreguntaTab> PreguntasEnSolapa { get; set; } = null;

    private bool HaySolapaSeleccionada()
    {
      foreach (UnionSolapaTab Solapa in Solapas)
      {
        if (Solapa.TabLocal.Seleccionada)
        {
          return true;
        }
      }
      return false;
    }

    //  private async Task ObtenerProveedoresFaltantesAsync()
    //{
    //   if (BlocksEnPantalla!=null && BlocksEnPantalla.Graficos != null)
    //{
    //     foreach(CLinkGraficoCnt Grafico in BlocksEnPantalla.Graficos)
    //	{
    //       if (Grafico.Filtros==null && Grafico.Datos.Datos.UsaFiltroPropio)
    //		{
    //         await Grafico.ObtenerProveedorPropioAsync(Http);
    //       }
    //     }

    //}

    //}

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      //   if (firstRender)
      //{
      //     await ObtenerProveedoresFaltantesAsync();
      //}
      if (Solapas != null && Solapas.Count > 0 &&
          !HaySolapaSeleccionada())
      {
        Solapas[0].TabLocal.Seleccionar(true);
        StateHasChanged();
      }
      else
      {
        await base.OnAfterRenderAsync(firstRender);
      }
    }

    [Inject]
    public HttpClient Http { get; set; }

    private void AjustarListaPreguntas(bool Refrescar = false)
    {
      if (mPreguntas == null && Sala != null)
      {
        // Buscar la solapa.
        CSolapaCN Solapa = Contenedores.CContenedorDatos.SolapaDesdeCodigo(Sala.Codigo, mCodigoSolapa);
        if (Solapa != null && Solapa.Block != null && Solapa.Block.Length > 2)
        {
          PreguntasEnSolapa = null;
          mszXML = Solapa.Block;
          if (Refrescar)
          {
            StateHasChanged();
          }
        }
        else
        {
          mszXML = "";
          mPreguntas = Contenedores.CContenedorDatos.PreguntasEnSolapa(Sala.Codigo, mCodigoSolapa);
          PreguntasEnSolapa = (from P in mPreguntas
                               select new UnionPreguntaTab()
                               {
                                 Pregunta0 = P,
                                 Abscisa0 = AbscisaPregunta(P.Codigo),
                                 Ordenada0 = OrdenadaPregunta(P.Codigo)
                               }).ToList();
        }
      }
      else
      {
        PreguntasEnSolapa = null;
        mszXML = "";
      }
    }

    private Int32 PosicionPregunta(Int32 Pregunta)
    {
      if (mPreguntas == null)
      {
        return -1;
      }
      for (Int32 i = 0; i < mPreguntas.Count; i++)
      {
        if (mPreguntas[i].Codigo == Pregunta)
        {
          return i;
        }
      }
      return -1;
    }

    public Int32 AbscisaPregunta (Int32 Pregunta)
    {
      Int32 Posicion = PosicionPregunta(Pregunta);
      if (Posicion < 0)
      {
        return -1000;
      }
      if (mCantColumnas < 0)
      {
        DeterminarColumnas();
      }
      Int32 Columna = Posicion % mCantColumnas;
      Int32 Fila = (Posicion - Columna) / mCantColumnas;
      return (Int32)Math.Floor(mSeparacion + Columna * (mSeparacion + CLogicaTarjeta.ANCHO_TARJETA) + 0.5);
    }

    public Int32 OrdenadaPregunta(Int32 Pregunta)
    {
      Int32 Posicion = PosicionPregunta(Pregunta);
      if (Posicion < 0)
      {
        return -1000;
      }
      if (mCantColumnas < 0)
      {
        DeterminarColumnas();
      }
      Int32 Columna = Posicion % mCantColumnas;
      Int32 Fila = (Posicion - Columna) / mCantColumnas;
      return (Int32)Math.Floor(25.5 + Fila * (CLogicaTarjeta.ALTO_TARJETA + 25));
    }

    public Int32 CodigoPrimeraSolapa
		{
      get
			{
        return (Solapas.Count > 0 ? Solapas[0].Solapa.Codigo : -1);
			}
		}

    private Int32 mCodigoSolapa = -1;

    public Modal ModalCrearSolapa { get; set; }

    private string mszNombre = "";

    public string NombreSolapaNueva
    {
      get { return mszNombre; }
      set
      {
        if (mszNombre != value)
        {
          mszNombre = value;
          AjustarHabilitado();
        }
      }
    }


    public string ColorSolapaNueva { get; set; } = "#ffffff";

    public bool MultiInstrumentosSolapaNueva { get; set; } = false;

    public void CerrandoCrearSolapa(Blazorise.ModalClosingEventArgs e)
    {
      switch (e.CloseReason)
      {
        case CloseReason.EscapeClosing:
        case CloseReason.FocusLostClosing:
          e.Cancel = true;
          break;
      }
    }

    private void AjustarHabilitado()
    {
      CrearSolapaDesHabilitado = mszNombre.Trim().Length == 0;
    }

    public bool CrearSolapaDesHabilitado { get; set; } = true;

    private void ExtraerComponentesColor(out Int32 Rojo, out Int32 Verde, out Int32 Azul)
    {
      try
      {
        string Aa = ColorSolapaNueva.Substring(1);
        Rojo = Int32.Parse(Aa.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        Verde = Int32.Parse(Aa.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        Azul = Int32.Parse(Aa.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
      }
      catch (Exception)
      {
        throw new Exception("Color incorrecto <" + ColorSolapaNueva + ">");
      }
    }

    public void SalirSolapa()
		{
      ModalCrearSolapa.Hide();
		}

    public async void RegistrarSolapa()
    {
      try
      {

        ExtraerComponentesColor(out Int32 Rojo, out Int32 Verde, out Int32 Azul);

        CSolapaCN Solapa = new CSolapaCN()
        {
          Codigo = -1,
          Dimension = -1,
          ElementoDimension = -1,
          Nombre = mszNombre.Trim(),
          Orden = Contenedores.CContenedorDatos.ObtenerOrdenProximaSolapa(Codigo),
          Sala = Codigo,
          Rojo = Rojo,
          Verde = Verde,
          Azul = Azul,
          Block = (MultiInstrumentosSolapaNueva ? Plantillas.CContenedorBlocks.XMLLimpia() : "")
        };

        var Respuesta = await Http.PostAsJsonAsync<CSolapaCN>("api/Comites/InsertarSolapa?URL=" +
                Contenedores.CContenedorDatos.UrlBPI +
                "&Ticket=" + Contenedores.CContenedorDatos.Ticket, Solapa);
        if (!Respuesta.IsSuccessStatusCode)
        {
          throw new Exception(Respuesta.ReasonPhrase);
        }

        RespuestaEnteros RespuestaCodigo = await Respuesta.Content.ReadFromJsonAsync<RespuestaEnteros>();
        if (!RespuestaCodigo.RespuestaOK)
        {
          throw new Exception(RespuestaCodigo.MsgErr);
        }

        Solapa.Codigo = RespuestaCodigo.Codigos[0];

        CPuntoSala Punto = Contenedores.CContenedorDatos.UbicarPuntoSala(Codigo);
        if (Punto != null)
        {
          Punto.Solapas.Add(new CPuntoSolapa()
          {
            Solapa = Solapa,
            Preguntas = new List<CPuntoPregunta>()
          });
        }

        Solapas.Add(new UnionSolapaTab()
        {
          Solapa = Solapa
        });

        await CLogicaMainMenu.gPuntero.DeterminarAnchoOpcionesAsync(Codigo);

        ModalCrearSolapa.Hide();
        StateHasChanged();

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    public string XMLSolapaSeleccionada
		{
      get
			{
        return (mSolapaEnEdicion == null ? CContenedorBlocks.XMLLimpia() : mSolapaEnEdicion.Solapa.Block);
			}
		}

    public bool EditarSolapa { get; set; } = false;
    public bool EditarXML { get; set; } = false;
    private UnionSolapaTab mSolapaEnEdicion = null;

    public void FncRefrescarSolapa(Int32 Codigo)
    {
      switch (Codigo)
      {
        case -1:
          if (ModalCrearSolapa != null)
          {
            NombreSolapaNueva = "";
            MultiInstrumentosSolapaNueva = false;
            ModalCrearSolapa.Show();
          }
          break;
        case -2:
          EditarSolapa = true;
          StateHasChanged();
          break;
        default:
          mPreguntas = null;
          mszXML = "";
          EditarSolapa = false;
          mCodigoSolapa = Codigo;
          foreach (UnionSolapaTab Solapa in Solapas)
          {
            if (Solapa.TabLocal != null)
            {
              Solapa.TabLocal.Seleccionar(Solapa.Solapa.Codigo == Codigo);
            }
          }
          if (SolapaCrear != null && SolapaCrear.TabLocal != null)
          {
            SolapaCrear.TabLocal.Seleccionar(false);
          }
          if (SolapaEditar != null && SolapaEditar.TabLocal != null)
          {
            SolapaEditar.TabLocal.Seleccionar(false);
          }
          mSolapaEnEdicion = (from S in Solapas
                                            where S.Solapa.Codigo == Codigo
                                            select S).FirstOrDefault();
          EditarXML = (Contenedores.CContenedorDatos.EsAdministrador &&
            mSolapaEnEdicion != null && mSolapaEnEdicion.Solapa.Block.Length > 0);
          mPreguntas = null;
          AjustarListaPreguntas();
          StateHasChanged();
          break;
      }
    }

    public CSalaCN Sala { get; set; }

    public UnionSolapaTab SolapaCrear { get; set; }
    public UnionSolapaTab SolapaEditar { get; set; }

    public List<UnionSolapaTab> Solapas { get; set; }

    public string NombreSolapa(Int32 Codigo)
    {
      return "SOLAPA_" + Codigo.ToString();
    }
  }

  public class UnionSolapaTab
  {
    public static CLogicaTab.FncRefrescar FncRefrescar { get; set; }
    public CSolapaCN Solapa { get; set; }

    private CLogicaTab mTab = null;
    public CLogicaTab TabLocal
    {
      get { return mTab; }
      set
      {
        if (value != mTab)
        {
          if (mTab != null)
          {
            mTab.AlRefrescar -= FncRefrescar;
          }
          mTab = value;
        }
        if (mTab != null)
        {
          mTab.AlRefrescar += FncRefrescar;
          mTab.Seleccionada = false; // (mTab.Codigo == mTab.CodigoPrimeraSolapa);
        }
      }
    }
  }

  public class UnionPreguntaTab
  {
    public static CLogicaTab.FncRefrescar FncRefrescar { get; set; }

    public CPreguntaCN Pregunta0
    {
      set { Pregunta = value; }
    }

    public Int32 Abscisa0
    {
      set { Abscisa = value; }
    }

    public Int32 Ordenada0
    {
      set { Ordenada = value; }
    }

    [Parameter]
    public CPreguntaCN Pregunta { get; set; }

    [Parameter]
    public Int32 Abscisa { get; set; } = 0;

    [Parameter]
    public Int32 Ordenada { get; set; } = 0;

    private CLogicaTarjeta mTab = null;
    public CLogicaTarjeta TabLocal
    {
      get { return mTab; }
      set
      {
        if (value != mTab)
        {
          if (mTab != null)
          {
//            mTab.AlRefrescar -= FncRefrescar;
          }
          mTab = value;
        }
        if (mTab != null)
        {
          //          mTab.AlRefrescar += FncRefrescar;
          mTab.LimpiarIndicadores();
        }
      }
    }
  }

}
