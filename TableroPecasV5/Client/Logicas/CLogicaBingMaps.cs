using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Blazorise;
using System.Net.Http;
using System.Net.Http.Json;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaBingMaps : ComponentBase, IDisposable
  {
    public delegate void FncEventoViewChange(Int32 Posicion, double Zoom);
    public delegate void FncEventoClick(double Lat, double Lng);

    public static FncEventoViewChange gAlHacerViewChange { get; set; }
    public static FncEventoClick gAlHacerClick { get; set; }

    [Inject]
    IJSRuntime JSRuntime { get; set; }

    public static List<CLogicaBingMaps> gPunteros = new List<CLogicaBingMaps>();
    public CLogicaBingMaps()
    {
      gPunteros.Add(this);
    }

    private Int32 mCodigo;


    [Parameter]
    public Int32 Codigo
    {
      get { return mCodigo; }
      set
      {
        if (mCodigo != value)
        {
          mCodigo = value;
          if (mCodigo > 0)
					{
            _ = CargarProyectoBingAsync();
          }
        }
      }
    }

    private List<CCapaWFSCN> mListaCapasWFS = null;
    public List<CCapaWFSCN> ListaCapasWFS
    {
      get { return (Selector.ListaCapasWFS == null ? mListaCapasWFS : Selector.ListaCapasWFS); }
    }

    public List<CCapaWFSCN> ListaCapasWFSFiltradas { get; set; } = null;

    public List<CCapaWISCompletaCN> ListaCapasWIS
		{
      get { return Selector.ListaCapasWIS; }
		}

    public List<CCapaWISCompletaCN> ListaCapasWISFiltradas { get; set; } = null;

    private async Task LeerCapasWFSAsync(bool UnicamenteWFS)
		{
      RespuestaCapasGIS Respuesta = await Contenedores.CContenedorDatos.LeerCapasWFSAsync(Http, UnicamenteWFS, true);
      if (!Respuesta.RespuestaOK)
			{
        CRutinas.DesplegarMsg(Respuesta.MsgErr);
			}
      else
			{
        mListaCapasWFS = Respuesta.CapasWFS;
			}
		}

    public void Refrescar()
		{
      StateHasChanged();
		}

    public CLogicaSelCapasWFS Selector { get; set; }

    public bool AgregandoCapas { get; set; } = false;
    public bool AgregandoCapasWIS { get; set; } = false;

    public async void CerrarDefinicionCapas()
		{
      AgregandoCapas = false;
      await Selector.LeerCapasAsync(true);
      StateHasChanged();
		}

    public async void CerrarDefinicionWIS()
    {
      AgregandoCapasWIS = false;
      await Selector.LeerCapasAsync(true);
      StateHasChanged();
      ModalCapas.Show();
    }

    public async void CerrarEdicionCapas()
		{
      CerrarMenu();
      await LeerCapasWFSAsync(true);
      StateHasChanged();
		}

    [Parameter]
    public double Abscisa { get; set; } = -999;

    [Parameter]
    public double Ordenada { get; set; } = -999;

    [Parameter]
    public double Ancho { get; set; } = -999;

    [Parameter]
    public double Alto { get; set; } = -999;

    [Parameter]
    public Int32 NivelFlotante { get; set; }

    [Parameter]
    public string Direccion { get; set; } = "";

    private static Int32 gCodigoMapa = 0;

    protected override Task OnInitializedAsync()
		{
      gCodigoMapa++;
      Direccion = "ContenedorMapa" + gCodigoMapa.ToString();
			return base.OnInitializedAsync();
		}

		public async void Dispose()
    {
      try
      {
        object[] Args = new object[1];
        Args[0] = mPosicionBingMap;
        await JSRuntime.InvokeAsync<string>("LiberarMap", Args);
        gPunteros.Remove(this);
      }
      catch (Exception)
      {
        // oculta el mensaje al cerrar.
      }
    }

    private bool mbConfigurando = false;
    public bool Configurando
		{
      get { return mbConfigurando; }
      set
			{
        if (mbConfigurando != value)
				{
          mbConfigurando = value;
          StateHasChanged();
				}
			}
		}

    private bool mbEtiquetas = false;
    public bool Etiquetas
    {
      get { return mbEtiquetas; }
      set
      {
        if (mbEtiquetas != value)
        {
          mbEtiquetas = value;
          StateHasChanged();
        }
      }
    }


    public string EstiloMapa
    {
      get
      {
        if (Abscisa < -998)
        {
          return "width: 100%; bottom: 0px; left: 0px; height: " +
            (Contenedores.CContenedorDatos.AltoPantalla - 0).ToString() + "px; ";
        }
        else
        {
          return "width: " + Math.Floor(Ancho).ToString() + "px; left: " + Math.Floor(Abscisa).ToString() + "px; top: " +
              Math.Floor(Ordenada).ToString() + "px; height: " +
              Math.Floor(Alto - 0).ToString() +
              "px; overflow: hidden; background-color: white; position: absolute;";
        }
      }
    }

    public string EstiloMapaComponente
    {
      get
      {
        if (Ancho < 1)
        {
          return "width: 100%; left: 0px; top: 40px; height: calc(100% - 40px); overflow: hidden; background-color: white; position: absolute;";
        }
        else
        {
          return "width: " + Math.Floor(Ancho).ToString() + "px; left: " + Math.Floor(Abscisa).ToString() + "px; top: " +
              Math.Floor(Ordenada).ToString() + "px; height: " +
              Math.Floor(Alto - 25).ToString() +
              "px; overflow: hidden; background-color: white; position: absolute;";
        }
      }
    }

    //private double LatCentro = -40;
    //private double LngCentro = -70;
    //private double NivelZoom = 9;

    //    private Int32 mPixelsAlto = -1;
    //private List<CPuntoTextoColor> mPuntos = null;
    //private void CrearPuntos()
    //{
    //  mPuntos = new List<CPuntoTextoColor>();
    //  mPuntos.Add(new CPuntoTextoColor(-72, -40, "blue", "1"));
    //  mPuntos.Add(new CPuntoTextoColor(-71, -41, "red", "2"));
    //  mPuntos.Add(new CPuntoTextoColor(-71.5, -40.5, "yellow", "3"));
    //  mPuntos.Add(new CPuntoTextoColor(-71.25, -40, "green", "4"));
    //}

    private async Task AgregarPushPinAsync(CPuntoTextoColor Punto)
    {
      object[] Args = new object[7];
      Args[0] = mPosicionBingMap;
      Args[1] = Punto.Abscisa;
      Args[2] = Punto.Ordenada;
      Args[3] = Punto.Color;
      Args[4] = Punto.Texto;
      Args[5] = "";
      Args[6] = "";
      try
      {
        await JSRuntime.InvokeAsync<Task>("AgregarPushpin", Args);
      }
      catch (Exception ex)
      {
        Rutinas.CRutinas.DesplegarMsg(ex);
      }
    }

    private CProyectoBing mProyecto = null;

    public CProyectoBing Proyecto
    {
      get { return mProyecto; }
      set { mProyecto = value; }
    }

    [Inject]
    public HttpClient Http { get; set; }

    private string ObtenerColorIndicador(Int32 Codigo, Int32 Elemento)
    {
      List<CInformacionAlarmaCN> Alarmas = (from A in Contenedores.CContenedorDatos.gAlarmasIndicador
                                            orderby A.Periodo
                                            where A.CodigoIndicador == Codigo && (Elemento < 0 || Elemento == A.ElementoDimension)
                                            select A).ToList();
      if (Alarmas.Count > 0)
      {
        return Alarmas.Last().Color.ToUpper();
      }
      else
      {
        return "";
      }
    }

    private string ObtenerColorPregunta(Int32 Codigo)
    {
      string Respuesta = "";
      foreach (CPuntoSala Sala in Contenedores.CContenedorDatos.EstructuraIndicadores.Salas)
      {
        foreach (CPuntoSolapa Solapa in Sala.Solapas)
        {
          foreach (CPuntoPregunta PregLocal in Solapa.Preguntas)
          {
            if (PregLocal.Pregunta.Codigo == Codigo) {
              foreach (CPreguntaIndicadorCN PregInd in PregLocal.Indicadores) {
                Respuesta = CRutinas.ColorMasCritico(Respuesta, ObtenerColorIndicador(PregInd.Indicador, PregInd.ElementoDimension));
              }
            }
          }
        }
      }
      return Respuesta;
    }

    private string ObtenerColorSalaReunion(Int32 Codigo)
    {
      string Respuesta = "";
      foreach (CPuntoSala Sala in Contenedores.CContenedorDatos.EstructuraIndicadores.Salas)
      {
        if (Sala.Sala.Codigo == Codigo)
        {
          foreach (CPuntoSolapa Solapa in Sala.Solapas)
          {
            foreach (CPuntoPregunta PregLocal in Solapa.Preguntas)
            {
              foreach (CPreguntaIndicadorCN PregInd in PregLocal.Indicadores)
              {
                Respuesta = CRutinas.ColorMasCritico(Respuesta, ObtenerColorIndicador(PregInd.Indicador, PregInd.ElementoDimension));
              }
            }
          }
        }
      }
      return Respuesta;
    }

    private async Task CargarProyectoBingAsync()
    {

      RespuestaProyectoBing Respuesta = await Http.GetFromJsonAsync<RespuestaProyectoBing>(
          "api/Proyectos/CargarProyecto?URL=" + Contenedores.CContenedorDatos.UrlBPI +
          "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
          "&Codigo=" + Codigo.ToString());
      if (!Respuesta.RespuestaOK)
      {
        throw new Exception("Al leer mapa " + Respuesta.MsgErr);
      }

      mCodigoLeido = Codigo;

      ReubicarCentro = true;

      mProyecto = new CProyectoBing();
      mProyecto.Proyecto = Respuesta.Proyecto;

      mProyecto.CapasCompletas = new List<CCapaComodin>();

      foreach (CDatosCapaComodin Dato in Respuesta.CapasCompletas)
      {
        mProyecto.CapasCompletas.Add(await ConvertirDatosCapaACapaAsync(Dato, Respuesta.Proyecto.Preguntas));
      }

      foreach (CElementoPreguntasWISCN Pregunta in mProyecto.Proyecto.Preguntas)
      {
        if (Pregunta.Color.Length == 0)
        {
          switch (Pregunta.ClaseWIS)
          {
            case ClaseCapa.Bing:
              foreach (CPreguntaPreguntaWISCN Elemento in Pregunta.Contenidos)
              {
                string Color = "";
                switch (Elemento.Clase)
                {
                  case ClaseDetalle.Indicador:
                    Color = ObtenerColorIndicador(Elemento.CodigoElemento, Elemento.CodigoElementoDimension);
                    break;
                  case ClaseDetalle.Pregunta:
                    Color = ObtenerColorPregunta(Elemento.CodigoElemento);
                    break;
                  case ClaseDetalle.SalaReunion:
                    Color = ObtenerColorSalaReunion(Elemento.CodigoElemento);
                    break;
                }
                if (Color != "")
                {
                  Elemento.Color = Color;
                  Pregunta.Color = CRutinas.ColorMasCritico(Pregunta.Color, Color);
                }
              }
              break;
          }
        }
      }

      StateHasChanged();

    }

    private async Task<CCapaComodin> ConvertirDatosCapaACapaAsync(CDatosCapaComodin Datos, List<CElementoPreguntasWISCN> Preguntas)
    {
      CCapaComodin Respuesta = new CCapaComodin();
      Respuesta.CapaWFS = Datos.CapaWFS;
      Respuesta.CapaWIS = Datos.CapaWIS;
      Respuesta.CapaWMS = Datos.CapaWMS;
      Respuesta.Clase = Datos.Clase;
      Respuesta.ColorWFS = Datos.ColorWFS;
      Respuesta.Opacidad = Datos.Opacidad;
      Respuesta.Preguntas = Datos.Preguntas;
      Respuesta.Pushpins = Datos.Pushpins;
      await Respuesta.AjustarColoresElementosCapaAsync();
      return Respuesta;
    }

    public string NombreMapaNuevo { get; set; } = "";
    private static Int32 mCodigoNuevo = -1;

    private async Task LimpiarContenidoMapaAsync()
		{
      if (mPosicionBingMap >= 0)
			{
        object[] Args = new object[1];
        Args[0] = mPosicionBingMap;
        try
        {
          string PosLocal = await JSRuntime.InvokeAsync<string>("LiberarPushpins", Args);
        }
        catch (Exception ex)
        {
          CRutinas.DesplegarMsg(ex);
        }
      }
    }

    private async Task RegistrarProyecto(CProyectoBing Proyecto)
		{
      var Respuesta = await Http.PostAsJsonAsync<CMapaBingCN>(
            "api/Proyectos/InsertarProyectoBing?URL=" +
            Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Contenedores.CContenedorDatos.Ticket, Proyecto.Proyecto);
      if (!Respuesta.IsSuccessStatusCode)
      {
        throw new Exception(Respuesta.ReasonPhrase);
      }

      RespuestaCodigos RespuestaCodigos = await Respuesta.Content.ReadFromJsonAsync<RespuestaCodigos>();
      if (!RespuestaCodigos.RespuestaOK)
      {
        throw new Exception(RespuestaCodigos.MsgErr);
      }

      Proyecto.Proyecto.Codigo = RespuestaCodigos.ParesDeCodigosMimicos[0].Actual;

    }

    public async void CrearProyecto()
		{
      if (NombreMapaNuevo!=null && NombreMapaNuevo.Trim().Length != 0)
			{
        await LimpiarContenidoMapaAsync();
        // Crear el proyecto.
        CProyectoBing Proyecto = new CProyectoBing();
        Proyecto.Proyecto.Codigo = mCodigoNuevo--;
        Proyecto.Proyecto.AbscisaCentro = -27;
        Proyecto.Proyecto.OrdenadaCentro = -40;
        Proyecto.Proyecto.NivelZoom = 7;
        Proyecto.Proyecto.Descripcion = NombreMapaNuevo.Trim();
        // Registrarlo.
        await RegistrarProyecto(Proyecto);

        // Entrar a editarlo.
        Codigo = Proyecto.Proyecto.Codigo;
        StateHasChanged();
        CerrarMenu();
			}
		}

    public Modal ModalMenu { get; set; }
    public Modal ModalCapas { get; set; }
    public Modal ModalNewProyecto { get; set; }

    public List<CPreguntaPreguntaWISCN> Preguntas { get; set; }

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

    private void NavegarASala(Int32 Codigo)
    {
      NavigationManager.NavigateTo("SalaReunion/" + Codigo.ToString(), false);
    }

    public string PreguntaElegida { get; set; }

    public void CerrarMenu()
    {
      if (ModalMenu.Visible)
      {
        ModalMenu.Hide();
      }
      if (ModalNewProyecto.Visible)
			{
        ModalNewProyecto.Hide();
			}
      if (ModalCapas.Visible)
			{
        ModalCapas.Hide();
			}
    }

    public string EstiloPregunta(CPreguntaPreguntaWISCN Pregunta)
    {
      return "background-color: " + CRutinas.ColorAclarado(Pregunta.Color) + ";";
    }

    public string CodigoPregunta(CPreguntaPreguntaWISCN Pregunta)
    {
      switch (Pregunta.Clase)
      {
        case ClaseDetalle.Indicador: return "I-" + Pregunta.CodigoElemento.ToString();
        case ClaseDetalle.Pregunta: return "P-" + Pregunta.CodigoElemento.ToString();
        case ClaseDetalle.SalaReunion: return "S-" + Pregunta.CodigoElemento.ToString();
        default: return "-1";
      }
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

    public string TextoPregunta(CPreguntaPreguntaWISCN Pregunta)
    {
      switch (Pregunta.Clase)
      {
        case ClaseDetalle.Indicador:
          CDatoIndicador Indicador = Contenedores.CContenedorDatos.IndicadorDesdeCodigo(Pregunta.CodigoElemento);
          return "Ind " + (Indicador == null ? "--" : Indicador.Descripcion);
        case ClaseDetalle.Pregunta:
          CPreguntaCN PreguntaLocal = Contenedores.CContenedorDatos.UbicarPregunta(Pregunta.CodigoElemento);
          return "Ficha " + (PreguntaLocal == null ? "--" : PreguntaLocal.Pregunta);
        case ClaseDetalle.SalaReunion:
          CSalaCN SalaLocal = Contenedores.CContenedorDatos.UbicarSala(Pregunta.CodigoElemento);
          return "Sala " + (SalaLocal == null ? "--" : SalaLocal.Nombre);
        default:
          return "--";
      }
    }

    public void AbrirMenu()
    {
      if (ModalMenu != null)
      {
        ModalMenu.Show();
      }
    }

    private Int32 mPosicionBingMap = -1;

    public delegate void FncEventoPoligono(string Referencia);
    public static event FncEventoPoligono AlRecibirEventoPoligono;

    private static void ProcesarMensajeClick(string Referencia)
		{
      AlRecibirEventoPoligono?.Invoke(Referencia);
		}

    [JSInvokable]
    public static Task<string> AbrirMenuBingMapsAsync(string Referencia)
    {
      try
      {
        if (Referencia.StartsWith("$$"))
				{
          ProcesarMensajeClick(Referencia);
				}
        string[] Elementos = Referencia.Split(';');
        if (Elementos.Length >= 1)
        {
          Int32 CodProy;
          if (Int32.TryParse(Elementos[0], out CodProy))
          {
            CLogicaBingMaps gPuntero = (from P in CLogicaBingMaps.gPunteros
                                        where P.Proyecto != null && P.Proyecto.Proyecto.Codigo == CodProy
                                        select P).FirstOrDefault();
            if (gPuntero != null)
            {
              gPuntero.Preguntas = new List<CPreguntaPreguntaWISCN>();
              if (Elementos.Length == 2)
              {
                foreach (CCapaComodin Capa in gPuntero.Proyecto.CapasCompletas)
                {
                  if (Capa.Clase == ClaseCapa.WIS)
                  {
                    foreach (CElementoPreguntasWISCN Pregunta in Capa.Preguntas)
                    {
                      if (Pregunta.CodigoArea == Elementos[1])
                      {
                        gPuntero.Preguntas = Pregunta.Contenidos;
                        break;
                      }
                    }
                    break;
                  }
                }
              }
              else
              {
                if (Elementos[1] == "P")
                {
                  foreach (CElementoPreguntasWISCN Elemento in gPuntero.Proyecto.Proyecto.Preguntas)
                  {
                    if (Elemento.Codigo == Int32.Parse(Elementos[2]))
                    {
                      gPuntero.Preguntas.AddRange(Elemento.Contenidos);
                    }
                  }
                }
              }

              if (gPuntero.Preguntas.Count > 0)
              {
                gPuntero.PreguntaElegida = null;
                foreach (CPreguntaPreguntaWISCN Pregunta in gPuntero.Preguntas)
                {
                  if (Pregunta.Color.Length == 0)
                  {
                    Task<ColorBandera> Tarea = CRutinas.ObtenerColorBanderaPreguntaPreguntaAsync(null, Pregunta);
                    Tarea.Wait();
                    Pregunta.Color = CRutinas.ColorBanderaATextoEspaniol(Tarea.Result);
                  }
                }
                if (gPuntero.Preguntas.Count == 1)
                {
                  gPuntero.PreguntaElegida = gPuntero.CodigoPregunta(gPuntero.Preguntas[0]);
                  gPuntero.Moverse();
                }
                else
                {
                  gPuntero.AbrirMenu();
                }
              }
            }
          }
        }
        return Task.FromResult("");
      }
      catch (Exception)
      {
        return Task.FromResult("No funcionó");
      }
    }

    [JSInvokable]
    public static Task<string> RefrescarZoomAsync(string Referencia)
    {
      try
      {
        Int32 Pos = Referencia.IndexOf(";");
        Int32 Posicion = Int32.Parse(Referencia.Substring(0,Pos));
        double Zoom = CRutinas.StrVFloat(Referencia.Substring(Pos + 1));
        if (gAlHacerViewChange != null)
				{
          gAlHacerViewChange(Posicion, Zoom);
				}
        return Task.FromResult("");
      }
      catch (Exception)
      {
        return Task.FromResult("No funcionó");
      }
    }

    [JSInvokable]
    public static Task<string> ClickEnMapaAsync(string Referencia)
    {
      try
      {
        if (gAlHacerClick != null)
				{
          string[] Coordenadas = Referencia.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
          if (Coordenadas.Length == 2)
					{
            gAlHacerClick(CRutinas.StrVFloat(Coordenadas[0]), CRutinas.StrVFloat(Coordenadas[1]));
					}
				}
        return Task.FromResult("");
      }
      catch (Exception)
      {
        return Task.FromResult("No funcionó");
      }
    }

    private void FncProcesarClick(double Lat, double Lng)
		{
      if (Lat != Lng)
			{
        return;
			}
    }

    public void AbrirEdicionWIS()
    {
      ModalCapas.Hide();
      AgregandoCapasWIS = true;
      StateHasChanged();
    }

    public void AbrirEdicionProveedores()
		{
      ModalCapas.Hide();
      AgregandoCapas = true;
      StateHasChanged();
		}

    public void CerrarEditarProveedores()
		{
      AgregandoCapas = false;
      ModalCapas.Show();
      StateHasChanged();
		}

    public void HacerReportes()
		{

		}

    public void AgregarCapas()
    {
      CerrarMenu();
      Selector.Contenedor = this;
      ModalCapas.Show();
    }

    public void Publicar()
		{

		}
                
    public void Renombrar()
		{

		}
                   
    public void AmpliarEscala()
		{

		}
                           
    public void ReducirEscala()
		{

		}
                              
    public void AgregarElemento()
		{

		}
                                 
    public void Grabar()
		{

		}
                                    
    public void Borrar()
		{

		}
                                       
    public void EditarMetas()
		{

		}
                                          
    public void MostrarAyuda()
		{

		}

    public void RefrescarCapas()
		{

		}

    private Int32 mCodigoLeido = -1;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      if (Codigo <= 0)
      {
        if (Codigo < 0)
        {
          if (!ModalNewProyecto.Visible)
          {
            ModalNewProyecto.Show();
          }
        }
        return;
      }
      try
      {
        if (ReubicarCentro)
        {
          mProyecto.UbicarCentro(Abscisa < -998 ? Contenedores.CContenedorDatos.AnchoPantalla : Ancho,
              Abscisa < -998 ? (Contenedores.CContenedorDatos.AltoPantalla - 45) : (Alto - 45));
          ReubicarCentro = false;
        }

        if (mProyecto != null && mPosicionBingMap < 0 && false)
        {
          object[] Args = new object[7];
          Args[0] = mPosicionBingMap;
          Args[1] = '#' + Direccion; // mProyecto.LatCentro;
          Args[2] = mProyecto.LatCentro;
          Args[3] = mProyecto.LngCentro;
          Args[4] = mProyecto.NivelZoom;
          Args[5] = false;
          Args[6] = false;
          try
          {
            string PosLocal = await JSRuntime.InvokeAsync<string>("loadMapRetPos", Args);
            mPosicionBingMap = Int32.Parse(PosLocal);
          }
          catch (Exception ex)
          {
            CRutinas.DesplegarMsg(ex);
          }
        }

        if (mProyecto != null && mPosicionBingMap >= 0 && false)
        {
          try
          {
            await mProyecto.DibujarAsync(JSRuntime, mPosicionBingMap);
          }
          catch (Exception ex)
          {
            CRutinas.DesplegarMsg(ex);
          }
        }
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
      await base.OnAfterRenderAsync(firstRender);
    }

    public bool ReubicarCentro { get; set; } = false;

    public void ImponerAncho(double R)
    {
      Ancho = R;
    }

    public void ImponerAlto(double R)
    {
      Alto = R;
    }

  }
}
