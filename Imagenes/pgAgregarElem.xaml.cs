using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using SilverFlow.Controls;
using System.ComponentModel;
using System.Xml.Linq;
using IndicadoresV2.Componentes;
using IndicadoresV2.Datos;
using IndicadoresV2.WCFBPI;

namespace IndicadoresV2.Paginas
{
  public partial class pgAgregarElem : Page, INotifyPropertyChanged
  {

    public event PropertyChangedEventHandler PropertyChanged;

    public pgAgregarElem()
    {
      InitializeComponent();
      DataContext = this;
    }

    public CSolapaCN Solapa { get; set; }
    public CPreguntaCN Pregunta { get; set; }

    public CContenedorBlocks VentanaBase { get; set; }

    private void OnPropertyChanged(string Propiedad)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(Propiedad));
      }
    }

    private string mszNombreOpcion = "";
    public string NombreOpcion
    {
      get { return mszNombreOpcion; }
      set
      {
        if (mszNombreOpcion != value)
        {
          mszNombreOpcion = value;
          OnPropertyChanged("NombreOpcion");
        }
      }
    }

    private string mszNombreGrafico = "";
    public string NombreGrafico
    {
      get { return mszNombreGrafico; }
      set
      {
        if (mszNombreGrafico != value)
        {
          mszNombreGrafico = value;
          OnPropertyChanged("NombreGrafico");
        }
      }
    }

    //private string mszNombreMapa = "";
    //public string NombreMapa
    //{
    //  get { return mszNombreMapa; }
    //  set
    //  {
    //    if (mszNombreMapa != value)
    //    {
    //      mszNombreMapa = value;
    //      OnPropertyChanged("NombreMapa");
    //    }
    //  }
    //}

    private ObservableCollection<CDatoIndicador> mListaIndicadores = new ObservableCollection<CDatoIndicador>();
    public ObservableCollection<CDatoIndicador> ListaIndicadores
    {
      get { return mListaIndicadores; }
      set
      {
        if (mListaIndicadores != value)
        {
          mListaIndicadores = value;
          OnPropertyChanged("ListaIndicadores");
        }
      }
    }

    private ObservableCollection<CDatosGrafLista> mListaGraficos = new ObservableCollection<CDatosGrafLista>();
    public ObservableCollection<CDatosGrafLista> ListaGraficos
    {
      get { return mListaGraficos; }
      set
      {
        if (mListaGraficos != value)
        {
          mListaGraficos = value;
          OnPropertyChanged("ListaGraficos");
        }
      }
    }

    private ObservableCollection<CDatosMapaLista> mListaMapas = new ObservableCollection<CDatosMapaLista>();
    public ObservableCollection<CDatosMapaLista> ListaMapas
    {
      get { return mListaMapas; }
      set
      {
        if (mListaMapas != value)
        {
          mListaMapas = value;
          OnPropertyChanged("ListaMapas");
        }
      }
    }

    private ObservableCollection<CDatosOtroLista> mListaOtros = new ObservableCollection<CDatosOtroLista>();
    public ObservableCollection<CDatosOtroLista> ListaOtros
    {
      get { return mListaOtros; }
      set
      {
        if (mListaOtros != value)
        {
          mListaOtros = value;
          OnPropertyChanged("ListaOtros");
        }
      }
    }

    private Visibility mVerIndicador = Visibility.Collapsed;
    public Visibility VerIndicador
    {
      get { return mVerIndicador; }
      set
      {
        if (mVerIndicador != value)
        {
          mVerIndicador = value;
          if (value == Visibility.Visible)
          {
            VerGraficos = Visibility.Collapsed;
            VerMapas = Visibility.Collapsed;
            VerOtros = Visibility.Collapsed;
          }
          OnPropertyChanged("VerIndicador");
        }
      }
    }

    private Visibility mVerGraficos = Visibility.Collapsed;
    public Visibility VerGraficos
    {
      get { return mVerGraficos; }
      set
      {
        if (mVerGraficos != value)
        {
          mVerGraficos = value;
          if (value == Visibility.Visible)
          {
            VerIndicador = Visibility.Collapsed;
            VerMapas = Visibility.Collapsed;
            VerOtros = Visibility.Collapsed;
          }
          else
          {
            AguardandoGraf = Visibility.Collapsed;
          }
          OnPropertyChanged("VerGraficos");
        }
      }
    }

    private Visibility mVerMapas = Visibility.Collapsed;
    public Visibility VerMapas
    {
      get { return mVerMapas; }
      set
      {
        if (mVerMapas != value)
        {
          mVerMapas = value;
          if (value == Visibility.Visible)
          {
            VerIndicador = Visibility.Collapsed;
            VerGraficos = Visibility.Collapsed;
            VerOtros = Visibility.Collapsed;
          }
          else
          {
            AguardandoMapas = Visibility.Collapsed;
          }
          OnPropertyChanged("VerMapas");
        }
      }
    }

    private Visibility mVerOtros = Visibility.Collapsed;
    public Visibility VerOtros
    {
      get { return mVerOtros; }
      set
      {
        if (mVerOtros != value)
        {
          mVerOtros = value;
          if (value == Visibility.Visible)
          {
            VerIndicador = Visibility.Collapsed;
            VerGraficos = Visibility.Collapsed;
            VerMapas = Visibility.Collapsed;
          }
          else
          {
            AguardandoOtros = Visibility.Collapsed;
          }
          OnPropertyChanged("VerOtros");
        }
      }
    }

    private Visibility mVerNombreGrafico = Visibility.Collapsed;
    public Visibility VerNombreGrafico
    {
      get { return mVerNombreGrafico; }
      set
      {
        if (mVerNombreGrafico != value)
        {
          mVerNombreGrafico = value;
          if (value == Visibility.Visible)
          {
            VerMuestraFiltro = Visibility.Collapsed;
          }
          OnPropertyChanged("VerNombreGrafico");
        }
      }
    }

    private bool mbOcultarFiltro = false;
    public bool OcultarFiltro
    {
      get { return mbOcultarFiltro; }
      set
      {
        if (mbOcultarFiltro != value)
        {
          mbOcultarFiltro = value;
          OnPropertyChanged("OcultarFiltro");
        }
      }
    }

    private Visibility mVerMuestraFiltro = Visibility.Collapsed;
    public Visibility VerMuestraFiltro
    {
      get { return mVerMuestraFiltro; }
      set
      {
        if (mVerMuestraFiltro != value)
        {
          mVerMuestraFiltro = value;
          if (value == Visibility.Visible)
          {
            VerNombreGrafico = Visibility.Collapsed;
          }
          OnPropertyChanged("VerMuestraFiltro");
        }
      }
    }

    private Visibility mVerFiltrosMapa = Visibility.Collapsed;
    public Visibility VerFiltrosMapa
    {
      get { return mVerFiltrosMapa; }
      set
      {
        if (mVerFiltrosMapa != value)
        {
          mVerFiltrosMapa = value;
          OnPropertyChanged("VerFiltrosMapa");
        }
      }
    }

    private Visibility mVerAguarda = Visibility.Collapsed;
    public Visibility VerAguarda
    {
      get { return mVerAguarda; }
      set
      {
        if (mVerAguarda != value)
        {
          mVerAguarda = value;
          OnPropertyChanged("VerAguarda");
        }
      }
    }

    private Visibility mAguardaGraficos = Visibility.Collapsed;
    public Visibility AguardandoGraf
    {
      get { return mAguardaGraficos; }
      set
      {
        if (mAguardaGraficos != value)
        {
          mAguardaGraficos = value;
          OnPropertyChanged("AguardandoGraf");
        }
      }
    }

    private Visibility mAguardaMapas = Visibility.Collapsed;
    public Visibility AguardandoMapas
    {
      get { return mAguardaMapas; }
      set
      {
        if (mAguardaMapas != value)
        {
          mAguardaMapas = value;
          OnPropertyChanged("AguardandoMapas");
        }
      }
    }

    private Visibility mAguardaOtros = Visibility.Collapsed;
    public Visibility AguardandoOtros
    {
      get { return mAguardaOtros; }
      set
      {
        if (mAguardaOtros != value)
        {
          mAguardaOtros = value;
          OnPropertyChanged("AguardandoOtros");
        }
      }
    }

    private CDatoIndicador mIndicadorSeleccionado = null;
    public CDatoIndicador IndicadorSeleccionado
    {
      get { return mIndicadorSeleccionado; }
      set
      {
        if (mIndicadorSeleccionado != value)
        {
          mIndicadorSeleccionado = value;
          HayIndicador = (value != null);
          OnPropertyChanged("IndicadorSeleccionado");
        }
      }
    }

    private CDatosGrafLista mGraficoSeleccionado = null;
    public CDatosGrafLista GraficoSeleccionado
    {
      get { return mGraficoSeleccionado; }
      set
      {
        if (mGraficoSeleccionado != value)
        {
          mGraficoSeleccionado = value;
          HayGrafico = (value != null);
          if (HayGrafico)
          {
            NombreGrafico = mGraficoSeleccionado.Nombre;
            FiltroPropio = false;
            VerMuestraFiltro = CRutinas.PonerVisible(mGraficoSeleccionado.Clase == ClaseGrafico.NoDefinido);
            OcultarFiltro = false;
            VerNombreGrafico = CRutinas.PonerVisible(mGraficoSeleccionado.Clase != ClaseGrafico.NoDefinido);

          }
          OnPropertyChanged("GraficoSeleccionado");
        }
      }
    }

    private CDatosMapaLista mMapaSeleccionado = null;
    public CDatosMapaLista MapaSeleccionado
    {
      get { return mMapaSeleccionado; }
      set
      {
        if (mMapaSeleccionado != value)
        {
          mMapaSeleccionado = value;
          //NombreMapa = (mMapaSeleccionado == null ? "" : mMapaSeleccionado.Nombre);
          FiltroPropio = false;
          VerFiltrosMapa = CRutinas.PonerVisible(mMapaSeleccionado != null && mMapaSeleccionado.FiltrosBlock != null);
          HayMapa = (mMapaSeleccionado != null);
          OnPropertyChanged("MapaSeleccionado");
        }
      }
    }

    private CDatosOtroLista mOtroSeleccionado = null;
    public CDatosOtroLista OtroSeleccionado
    {
      get { return mOtroSeleccionado; }
      set
      {
        if (mOtroSeleccionado != value)
        {
          mOtroSeleccionado = value;
          //NombreOtro = (mOtroSeleccionado == null ? "" : mOtroSeleccionado.Nombre);
          FiltroPropio = false;
          HayOtro = (mOtroSeleccionado != null);
          OnPropertyChanged("OtroSeleccionado");
        }
      }
    }

    private bool mbHayIndicador = false;
    public bool HayIndicador
    {
      get { return mbHayIndicador; }
      set
      {
        if (mbHayIndicador != value)
        {
          mbHayIndicador = value;
          OnPropertyChanged("HayIndicador");
        }
      }
    }

    private bool mbHayGrafico = false;
    public bool HayGrafico
    {
      get { return mbHayGrafico; }
      set
      {
        if (mbHayGrafico != value)
        {
          mbHayGrafico = value;
          OnPropertyChanged("HayGrafico");
        }
      }
    }

    private bool mbHayMapa = false;
    public bool HayMapa
    {
      get { return mbHayMapa; }
      set
      {
        if (mbHayMapa != value)
        {
          mbHayMapa = value;
          OnPropertyChanged("HayMapa");
        }
      }
    }

    private bool mbHayOtro = false;
    public bool HayOtro
    {
      get { return mbHayOtro; }
      set
      {
        if (mbHayOtro != value)
        {
          mbHayOtro = value;
          OnPropertyChanged("HayOtro");
        }
      }
    }

    private bool mbFiltroPropio = false;
    public bool FiltroPropio
    {
      get { return mbFiltroPropio; }
      set
      {
        if (mbFiltroPropio != value)
        {
          mbFiltroPropio = value;
          OnPropertyChanged("FiltroPropio");
        }
      }
    }

    // Se ejecuta cuando el usuario navega a esta página.
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
    }

    private void CargarImagenes()
    {
      ImgIndic.Source = CRutinas.ImgIndMan;
      ImgTend.Source = CRutinas.ImgTendencia;
      ImgGraf.Source = CRutinas.ImgGrTorta;
      ImgBing.Source = CRutinasMimico.ImgBing;
      ImgMenu.Source = CRutinas.ImgAbrirMenu;
      ImgBuscarIndic.Source = CRutinas.ImgBuscar;
      ImgRegistrar.Source = CRutinas.ImgRegistrar;
      ImgBuscarGrafico.Source = CRutinas.ImgBuscar;
      ImgFTorta.Source = CRutinas.ImgGrTorta;
      ImgFBarras.Source = CRutinas.ImgGrBarras;
      ImgFBarrasH.Source = CRutinas.ImgBarrasH;
      ImgFHisto.Source = CRutinas.ImgGrHistograma;
      ImgFPareto.Source = CRutinas.ImgGrPareto;
      ImgFPiramide.Source = CRutinas.ImgPiramide;
      ImgFCalor.Source = CRutinas.ImgGrTorta;
      ImgFControl.Source = CRutinasMimico.ImgBing;
      ImgFGradiente.Source = CRutinas.ImgIdera;
      ImgBuscarMapa.Source = CRutinas.ImgBuscar;
      ImgFMimico.Source = CRutinasMimico.ImgXPDL;
      ImgFGrilla.Source = CRutinasMimico.ImgGrilla;
      ImgFSubC.Source = CRutinas.ImgTendencia;
      ImgBuscarOtro.Source = CRutinas.ImgBuscar;
      CRutinas.PonerConjuntoEnCanvas(cvConjuntos);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
      CargarImagenes();
    }

    private void CerrarVentanaEdicion()
    {
      ((Grid)this.Parent).Children.Remove(this);
    }

    private Int32 mClaseGraficoSeleccionado = -1;

    private bool ClaseCumpleFiltro(ClaseGrafico Clase)
    {
      switch (Clase)
      {
        case ClaseGrafico.Torta:
          return (mClaseGraficoSeleccionado == 1 || mClaseGraficoSeleccionado < 0);
        case ClaseGrafico.Barras:
          return (mClaseGraficoSeleccionado == 2 || mClaseGraficoSeleccionado < 0);
        case ClaseGrafico.Pareto:
          return (mClaseGraficoSeleccionado == 3 || mClaseGraficoSeleccionado < 0);
        case ClaseGrafico.BarrasH:
          return (mClaseGraficoSeleccionado == 4 || mClaseGraficoSeleccionado < 0);
        case ClaseGrafico.Histograma:
          return (mClaseGraficoSeleccionado == 5 || mClaseGraficoSeleccionado < 0);
        case ClaseGrafico.Piramide:
          return (mClaseGraficoSeleccionado == 6 || mClaseGraficoSeleccionado < 0);
        case ClaseGrafico.NoDefinido:
          return (mClaseGraficoSeleccionado == 7 || mClaseGraficoSeleccionado < 0);
        default:
          return false;
      }
    }

    private List<CDatosGrafLista> mListaGraficosTotal = null;
    private void CrearListaGraficos()
    {
      mClaseGraficoSeleccionado=-1;

      mListaGraficosTotal = new List<CDatosGrafLista>();

      foreach (CPaginaNavegador Pagina in MainPage.PunteroMainPage.HistoriaPaginas)
      {
        PagTarjeta PagTrj = Pagina as PagTarjeta;
        if (PagTrj != null)
        {
          foreach (Graficos.CGrafV2DatosContenedorBlock Graf in PagTrj.ListarDatosGraficos())
          {
            mListaGraficosTotal.Add(new CDatosGrafLista(Graf));
          }

          foreach (Graficos.CGrafV2DatosContenedorBlock Filtro in PagTrj.ListarDatosConjuntos())
          {
            mListaGraficosTotal.Add(new CDatosGrafLista(Filtro));
          }

        }
      }
    }

    private List<Graficos.CPasoCondicionesBlock> ExtraerFiltrosIndicador(Int32 Codigo)
    {
      if (Codigo < 0)
      {
        return null;
      }

      foreach (CPaginaNavegador Pagina in MainPage.PunteroMainPage.HistoriaPaginas)
      {
        PagTarjeta PagTrj = Pagina as PagTarjeta;
        if (PagTrj != null)
        {
          List<Graficos.CPasoCondicionesBlock> Lista = PagTrj.ExtraerFiltrosIndicador(Codigo);
          if (Lista != null && Lista.Count > 0)
          {
            return Lista;
          }
        }
      }

      return null;

    }

    private List<CGraficoCompletoCN> mGraficosDefinidos = null;
    private List<CDatosMapaLista> mListaMapasTotal = null;
    private void CrearListaMapas()
    {
      mClaseMapaSeleccionado = -1;

      if (mListaMapasTotal == null)
      {
        WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteBPI();
        try
        {
          Cliente.ListarCapasWSSCompleted +=
          new EventHandler<ListarCapasWSSCompletedEventArgs>(
            Cliente_ListarCapasWSSCompleted);
          Cliente.ListarCapasWSSAsync(CRutinas.Ticket, ClaseElemento.NoDefinida, -1);
          AguardandoMapas = Visibility.Visible;
        }
        catch (Exception ex)
        {
          CRutinas.DesplegarError(ex);
        }
        finally
        {
          Cliente.CloseAsync();
        }
      }
      else
      {
        LeerListaGraficos();
      }
    }

    private void LeerListaGraficos()
    {

      if (mGraficosDefinidos == null)
      {
        WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteBPI();
        try
        {
          Cliente.ListarGraficosIndicadoresCompleted += Cliente_ListarGraficosIndicadoresCompleted;
          Cliente.ListarGraficosIndicadoresAsync(CRutinas.Ticket, new List<Int32>());
          AguardandoMapas = Visibility.Visible;
        }
        catch (Exception ex)
        {
          CRutinas.DesplegarError(ex);
        }
        finally
        {
          Cliente.CloseAsync();
        }
      }
      else
      {
        CompletarCargaListaMapas();
      }
    }

    private void Cliente_ListarGraficosIndicadoresCompleted(object sender, ListarGraficosIndicadoresCompletedEventArgs e)
    {
      try
      {
        WCFBPI.CRespuestaGraficosVarios Respuesta = e.Result;
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MensajeError);
        }

        mGraficosDefinidos = Respuesta.Graficos;

        CompletarCargaListaMapas();

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarError(ex);
      }
      finally
      {
        AguardandoMapas = Visibility.Collapsed;
      }
    }

    void Cliente_ListarCapasWSSCompleted(object sender, ListarCapasWSSCompletedEventArgs e)
    {
      try
      {
        WCFBPI.CRespuestaCapasWSS Respuesta = e.Result;
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MensajeError);
        }

        mListaMapasTotal = new List<CDatosMapaLista>();
        foreach (CCapaWSSCN Capa in Respuesta.Capas)
        {
          mListaMapasTotal.Add(new CDatosMapaLista(Capa,
              ExtraerFiltrosIndicador(Capa.Clase == ClaseElemento.Indicador ? Capa.CodigoElemento : -1)));
        }

        LeerListaGraficos();

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarError(ex);
      }
      finally
      {
        AguardandoMapas = Visibility.Collapsed;
      }
    }

    private void CompletarCargaListaMapas()
    {

      foreach (CMapaBingCN Mapa in Datos.CCcontenedorDatos.ContenedorUnico.ProyectosBing)
      {
        mListaMapasTotal.Add(new CDatosMapaLista(Mapa));
      }

      foreach (CPaginaNavegador Pagina in MainPage.PunteroMainPage.HistoriaPaginas)
      {
        PagGISDatos Pag = Pagina as PagGISDatos;
        if (Pag != null)
        {
          if (Pag.cbCalientes.IsChecked == true && Pag.Capas.Count == 1 && Pag.Indicador != null)
          {
            mListaMapasTotal.Add(new CDatosMapaLista(Pag.Capas[0], Pag.Indicador.Codigo, ExtraerFiltrosIndicador(Pag.Indicador.Codigo)));
          }
        }
      }

      foreach (CGraficoCompletoCN Grafico in mGraficosDefinidos)
      {
        if (Grafico.Graficos.Count==1 && Grafico.Graficos[0].ClaseDeGrafico == ClaseGrafico.SobreGIS)
        {
          mListaMapasTotal.Add(new CDatosMapaLista(Grafico));
        }
      }


      mListaMapasTotal.Sort(delegate(CDatosMapaLista D1, CDatosMapaLista D2)
      {
        return D1.Nombre.CompareTo(D2.Nombre);
      });

      CmdFiltroMapa_Click(null, null);

    }

    private List<CDatosOtroLista> mListaOtrosTotal = null;
    private void CrearListaOtros()
    {

      mClaseOtroSeleccionado=-1;
      mListaOtrosTotal=new List<CDatosOtroLista>();

      foreach (CElementoMimicoCN Mimico in Datos.CCcontenedorDatos.ContenedorUnico.Mimicos)
      {
        mListaOtrosTotal.Add(new CDatosOtroLista(Mimico));
      }

      foreach (CElementoIndicador Indi in CCcontenedorDatos.ContenedorUnico.IndicadoresDefinidos)
      {
        mListaOtrosTotal.Add(new CDatosOtroLista(Indi));
      }

      foreach (CSubconsultaExt SubC in CCcontenedorDatos.ContenedorUnico.Subconsultas)
      {
        if (SubC.Parametros.Count == 0)
        {
          mListaOtrosTotal.Add(new CDatosOtroLista(SubC));
        }
      }

      mListaOtrosTotal.Sort(delegate(CDatosOtroLista D1, CDatosOtroLista D2)
      {
        return D1.Nombre.CompareTo(D2.Nombre);
      });

      CmdFiltroOtros_Click(null, null);

    }

    private void FiltrarGraficos()
    {
      ListaGraficos.Clear();
      string Texto = tbFiltroGrafico.Text.Trim();
      List<string> ListaPalabras = (Texto.Length == 0 ? new List<string>() : Pantallas.frmEditIndicador.ExtraerListaPalabras(Texto));

      foreach (CDatosGrafLista Dato in mListaGraficosTotal)
      {
        if (ClaseCumpleFiltro(Dato.Clase) && (Texto.Length == 0 || Pantallas.frmEditIndicador.CumpleCondicionTexto(Dato.Nombre, ListaPalabras)))
        {
          ListaGraficos.Add(Dato);
        }
      }
    }

    private void CopiarCapasWFS()
    {
      foreach (FloatingWindow Ventana in VentanaBase.Contenedor.FloatingWindows)
      {
        Paginas.CDatosMapaLista DatosMapa = (Ventana.Tag as Paginas.CDatosMapaLista);
        if (DatosMapa != null)
        {
          Graficos.VentanaAguarda VentAg = (from UIElement Elemento in Ventana.HostPanel.Children
                                            where Elemento is Graficos.VentanaAguarda
                                            select (Graficos.VentanaAguarda)Elemento).FirstOrDefault();
          if (VentAg != null)
          {
            PagBaseGIS Pantalla = VentAg.frContenedor.Content as PagBaseGIS;
            if (Pantalla != null)
            {
              DatosMapa.CapasWFS.Clear();
              foreach (Rutinas.CCapaComodin Capa in Pantalla.ProyectoBing.CapasCompletas)
              {
                if (Capa.Clase == ClaseCapa.WFS)
                {
                  DatosMapa.CapasWFS.Add(new CCapaWFSMapa(Capa.CodigoCapa,
                      Capa.ColorWFS.A, Capa.ColorWFS.R, Capa.ColorWFS.G, Capa.ColorWFS.B));
                }
              }
            }
          }
        }
      }
    }

    private void RegistrarSolapa()
    {
      CopiarCapasWFS();
      Solapa.Block = VentanaBase.ObtenerXML();
      WCFBPIClient Cliente = CRutinas.ObtenerClienteBPI();
      try
      {
        Cliente.RegistrarSolapaCompleted += Cliente_RegistrarSolapaCompleted;
        Cliente.RegistrarSolapaAsync(CRutinas.Ticket, Solapa);
        VerAguarda = Visibility.Visible;
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarError(ex);
      }
      finally
      {
        Cliente.CloseAsync();
      }

    }

    void Cliente_RegistrarSolapaCompleted(object sender, RegistrarSolapaCompletedEventArgs e)
    {
      VerAguarda = Visibility.Collapsed;
      try
      {
        CRespuestaEntero Respuesta = e.Result;
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MensajeError);
        }
        Solapa.Codigo = Respuesta.CodigoAsociado;
        CerrarVentanaEdicion();
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarError(ex);
      }
    }

    private void RegistrarTarjeta()
    {
      Pregunta.Block = VentanaBase.ObtenerXML();
      WCFBPIClient Cliente = CRutinas.ObtenerClienteBPI();
      try
      {
        Cliente.RegistrarPreguntaCompleted += Cliente_RegistrarPreguntaCompleted;
        Cliente.RegistrarPreguntaAsync(CRutinas.Ticket, Pregunta);
        VerAguarda = Visibility.Visible;
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarError(ex);
      }
      finally
      {
        Cliente.CloseAsync();
      }

    }

    void Cliente_RegistrarPreguntaCompleted(object sender, RegistrarPreguntaCompletedEventArgs e)
    {
      VerAguarda = Visibility.Collapsed;
      try
      {
        CRespuestaEntero Respuesta = e.Result;
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MensajeError);
        }
        Pregunta.Codigo = Respuesta.CodigoAsociado;
        CerrarVentanaEdicion();
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarError(ex);
      }
    }

    private Int32 mOpcion = -1;
    private void COpcionAgregarElemento_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (sender != null && sender is COpcionAgregarElemento)
      {
        COpcionAgregarElemento Elemento = (COpcionAgregarElemento)sender;
        mOpcion = Elemento.CodigoOpcion;
        switch (mOpcion)
        {
          case 1: // Agregar indicador.
          case 3: // Agregar tendencias.
            NombreOpcion = "Agregar " + (mOpcion == 1 ? "indicador" : "gráfico evolutivo");
            VerIndicador = Visibility.Visible;
            tbFiltroIndicador.Text = "";
            IndicadorSeleccionado = null;
            CmdFiltroIndic_Click(null, null);
            break;
          case 2: // Agregar tarjeta.
            if (Solapa == null || Solapa.Codigo < 0)
            {
              MessageBox.Show("No está editando una solapa o aún no la registró");
            }
            else
            {
              Pantallas.frmEditBoton Editor = new Pantallas.frmEditBoton();
              CPreguntaCN Pregunta = new CPreguntaCN();
              Pregunta.Codigo = -1;
              Pregunta.Block = "";
              Pregunta.Dimension = -1;
              Pregunta.ElementoDimension = -1;
              Pregunta.Orden = -1;
              Pregunta.Pregunta = "";
              Pregunta.Solapa = Solapa.Codigo;
              Pregunta.Block = "";
              Editor.DatosFicha = Pregunta;
              Editor.Closed += Editor_Closed;
              Editor.Show();
            }
            break;
          case 4:
            // buscar los graficos en frmcreargrafv2, gListaGraficos.
            mClaseGraficoSeleccionado = -1;
            VerGraficos = Visibility.Visible;
            CrearListaGraficos();
            tbFiltroGrafico.Text = "";
            GraficoSeleccionado = null;
            CmdFiltroGrafico_Click(null, null);
            break;
          case 5:
            mClaseMapaSeleccionado = -1;
            VerMapas = Visibility.Visible;
            tbFiltroMapa.Text = "";
            MapaSeleccionado = null;
            CrearListaMapas();
            break;
          case 10:
            mClaseOtroSeleccionado = -1;
            VerOtros = Visibility.Visible;
            tbFiltroOtros.Text = "";
            OtroSeleccionado = null;
            CrearListaOtros();
            break;
          case -1:
            if (Solapa != null)
            {
              RegistrarSolapa();
            }
            else
            {
              RegistrarTarjeta();
            }
            break;
        }
      }
    }

    void Editor_Closed(object sender, EventArgs e)
    {
      Pantallas.frmEditBoton Editor = (sender as Pantallas.frmEditBoton);
      if (Editor != null && Editor.DialogResult == true)
      {
        CRutinasMimico.EstIndicadores.PreguntasSueltas.Add(new CPuntoPregunta()
        {
          Pregunta = Editor.DatosFicha
        });
        VentanaBase.AgregarFicha(Editor.DatosFicha);
      }
    }

    private void CmdFiltroIndic_Click(object sender, RoutedEventArgs e)
    {
      string Texto = tbFiltroIndicador.Text.Trim();

      ListaIndicadores.Clear();

      if (Texto.Length == 0)
      {
        foreach (CDatoIndicador Dato in from I in CCcontenedorDatos.ContenedorUnico.IndicadoresDefinidos
                                        orderby I.Datos.Descripcion
                                        select I.Datos)
        {
          ListaIndicadores.Add(Dato);
        }
      }
      else
      {
        List<string> ListaPalabras = Pantallas.frmEditIndicador.ExtraerListaPalabras(Texto);
        foreach (CDatoIndicador Dato in from I in CCcontenedorDatos.ContenedorUnico.IndicadoresDefinidos
                                        where Pantallas.frmEditIndicador.CumpleCondicion(I, ListaPalabras, true)
                                        orderby I.Datos.Descripcion
                                        select I.Datos)
        {
          ListaIndicadores.Add(Dato);
        }
      }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      mOpcion = -1;
      VerIndicador = Visibility.Collapsed;
    }

    private void CmdOKIndic_Click(object sender, RoutedEventArgs e)
    {
      // Crear el indicador en la posicion que corresponde.
      if (VentanaBase != null)
      {
        foreach (CDatoIndicador Indicador in lbIndic.SelectedItems)
        {
          switch (mOpcion)
          {
            case 1:
              VentanaBase.AgregarIndicador(Indicador, -1, true);
              break;
            case 3:
              VentanaBase.AgregarTendencia(Indicador, -1);
              break;
            case -1:
              Grid Grilla = (Grid)this.Parent;
              Grilla.Children.Remove(this);
              break;
          }
        }
        Button_Click(null, null);
      }
    }

    private void AgregarGraficoSeleccionado()
    {
      AguardandoGraf = Visibility.Visible;
      try
      {
        VentanaBase.AlCompletarPedido = FncCerrarPedido;
        switch (GraficoSeleccionado.Datos.ClaseElemento)
        {
          case Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Grafico:
            GraficoSeleccionado.Datos.Nombre = NombreGrafico;
            GraficoSeleccionado.Datos.UsaFiltroPropio = FiltroPropio;
            VentanaBase.AgregarGrafico(GraficoSeleccionado);
            break;
          case Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Conjunto:
            GraficoSeleccionado.Datos.Visible = !OcultarFiltro;
            VentanaBase.AgregarConjunto(GraficoSeleccionado);
            break;
        }
      }
      finally
      {
        VerGraficos = Visibility.Collapsed;
        AguardandoGraf = Visibility.Collapsed;
      }
    }

    private void FncCerrarPedido()
    {
      //VerGraficos=Visibility.Collapsed;
      //VerMapas=Visibility.Collapsed;
      //VerOtros=Visibility.Collapsed;
      //AguardandoGraf = Visibility.Collapsed;
      //AguardandoMapas=Visibility.Collapsed;
      //AguardandoOtros=Visibility.Collapsed;
    }

    private void CmdOKGraf_Click(object sender, RoutedEventArgs e)
    {
      // Buscar el elemento a graficar.
      AgregarGraficoSeleccionado();
    }

    private void CmdFiltroGrafico_Click(object sender, RoutedEventArgs e)
    {
      FiltrarGraficos();
    }

    private void ImgFTorta_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (sender != null)
      {
        if (sender is FrameworkElement)
        {
          Int32 CodigoClase = Int32.Parse((string)((FrameworkElement)sender).Tag);
          mClaseGraficoSeleccionado = (CodigoClase == mClaseGraficoSeleccionado ? -1 : CodigoClase);
          FiltrarGraficos();
        }
      }
    }

    private void ButtonGraf_Click(object sender, RoutedEventArgs e)
    {
      VerGraficos = Visibility.Collapsed;
      VerMapas = Visibility.Collapsed;
      VerOtros = Visibility.Collapsed;
    }

    private void CmdOKMapa_Click(object sender, RoutedEventArgs e)
    {
      AguardandoMapas = Visibility.Visible;
      try
      {
        VentanaBase.AlCompletarPedido = FncCerrarPedido;
        GraficoSeleccionado = null;
        MapaSeleccionado.Nombre = MapaSeleccionado.Nombre;
        MapaSeleccionado.UsaFiltroPropio = FiltroPropio;
        VentanaBase.AgregarMapa(MapaSeleccionado);
        VerMapas = Visibility.Collapsed;
      }
      finally
      {
        AguardandoMapas = Visibility.Collapsed;
      }
    }

    private bool CumpleFiltroMapa(Graficos.CGrafV2DatosContenedorBlock.ClaseBlock Clase)
    {
      if (mClaseMapaSeleccionado < 0)
      {
        return true;
      }
      else
      {
        switch (Clase)
        {
          case Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.MapaCalor: return (mClaseMapaSeleccionado == 1);
          case Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.MapaControl: return (mClaseMapaSeleccionado == 2);
          case Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.MapaGradientes: return (mClaseMapaSeleccionado == 3);
          default: return false;
        }
      }
    }

    private Int32 mClaseOtroSeleccionado = -1;
    private bool CumpleFiltroOtro(Graficos.CGrafV2DatosContenedorBlock.ClaseBlock Clase)
    {
      if (mClaseOtroSeleccionado < 0)
      {
        return true;
      }
      else
      {
        switch (Clase)
        {
          case Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Mimico: return (mClaseOtroSeleccionado == 1);
          case Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Grilla: return (mClaseOtroSeleccionado == 2);
          case Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Consulta: return (mClaseOtroSeleccionado == 3);
          default: return false;
        }
      }
    }

    private void FiltrarMapas()
    {
      ListaMapas.Clear();
      string Texto = tbFiltroMapa.Text.Trim();
      List<string> ListaPalabras = (Texto.Length == 0 ? new List<string>() : Pantallas.frmEditIndicador.ExtraerListaPalabras(Texto));

      foreach (CDatosMapaLista Dato in mListaMapasTotal)
      {
        if (CumpleFiltroMapa(Dato.Clase) && (Texto.Length == 0 || Pantallas.frmEditIndicador.CumpleCondicionTexto(Dato.Nombre, ListaPalabras)))
        {
          ListaMapas.Add(Dato);
        }
      }
    }

    private void FiltrarOtros()
    {
      ListaOtros.Clear();
      string Texto = tbFiltroOtros.Text.Trim();
      List<string> ListaPalabras = (Texto.Length == 0 ? new List<string>() : Pantallas.frmEditIndicador.ExtraerListaPalabras(Texto));

//      qwqw();
      foreach (CDatosOtroLista Dato in mListaOtrosTotal)
      {
        if (CumpleFiltroOtro(Dato.Clase) && (Texto.Length == 0 || Pantallas.frmEditIndicador.CumpleCondicionTexto(Dato.Nombre, ListaPalabras)))
        {
          ListaOtros.Add(Dato);
        }
      }
    }

    private void CmdFiltroMapa_Click(object sender, RoutedEventArgs e)
    {
      FiltrarMapas();
    }

    private Int32 mClaseMapaSeleccionado = -1;
    private void ImgFCalor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (sender != null)
      {
        if (sender is FrameworkElement)
        {
          Int32 CodigoClase = Int32.Parse((string)((FrameworkElement)sender).Tag);
          mClaseMapaSeleccionado = (CodigoClase == mClaseMapaSeleccionado ? -1 : CodigoClase);
          FiltrarMapas();
        }
      }
    }

    private void ImgFMimico_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (sender != null)
      {
        if (sender is FrameworkElement)
        {
          Int32 CodigoClase = Int32.Parse((string)((FrameworkElement)sender).Tag);
          mClaseOtroSeleccionado = (CodigoClase == mClaseOtroSeleccionado ? -1 : CodigoClase);
          FiltrarOtros();
        }
      }
    }

    private void CmdOKOtro_Click(object sender, RoutedEventArgs e)
    {
      AguardandoOtros = Visibility.Visible;
      try
      {
        VentanaBase.AlCompletarPedido = FncCerrarPedido;
        switch (OtroSeleccionado.Clase)
        {
          case Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Mimico:
            VentanaBase.AgregarMimico(OtroSeleccionado);
            break;
          case Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Grilla:
            VentanaBase.AgregarGrilla(OtroSeleccionado);
            break;
          case Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Consulta:
            VentanaBase.AgregarSubConsulta(OtroSeleccionado);
            break;
        }
      }
      finally
      {
        VerOtros = Visibility.Collapsed;
        AguardandoOtros = Visibility.Collapsed;
      }
    }

    private void CmdFiltroOtros_Click(object sender, RoutedEventArgs e)
    {
      FiltrarOtros();
    }

  }

  public class CDatosGrafLista
  {
    public static Int32 gCodigo = 0;
    public Int32 Codigo { get; set; }
    public ImageSource Imagen { get { return CRutinas.ImagenDesdeClaseGrafico(Clase, Datos.ClaseElemento); } }
    public ClaseGrafico Clase { get { return Datos.Clase; } }
    public string Nombre { get; set; }
    public Graficos.CGrafV2DatosContenedorBlock Datos { get; set; }

    public CDatosGrafLista(Graficos.CGrafV2DatosContenedorBlock Grafico)
    {
      gCodigo++;
      Codigo = gCodigo;
      Datos = Grafico;
      Nombre = Grafico.Nombre;
    }

  }

  public class CDatosMapaLista
  {
    public Int32 Codigo { get; set; }
    public Int32 CodigoPropio { get; set; }
    public Graficos.CGrafV2DatosContenedorBlock.ClaseBlock Clase { get; set; }
    public ImageSource Imagen { get { return CRutinas.ImagenDesdeClaseMapa(Clase); } }
    public string Nombre { get; set; }
    public ClaseElemento ClaseIndicador { get; set; }
    public Int32 CodigoIndicador { get; set; }
    public Int32 CodigoElementoDimension { get; set; }
    public string ColumnaDatos { get; set; }
    public string ColumnaLat { get; set; }
    public string ColumnaLng { get; set; }
    public bool UsaFiltroPropio { get; set; }
    public List<Graficos.CPasoCondicionesBlock> FiltrosBlock { get; set; }
    public Point Posicion { get; set; }
    public double Ancho { get; set; }
    public double Alto { get; set; }
    public double EscalaFuente { get; set; } = 0;
    public bool MuestraFondo { get; set; } = true;
    public bool MuestraNombres { get; set; } = true;
    public bool MuestraValores { get; set; } = true;
    public bool OcultaCeros { get; set; } = false;
    public List<CCapaWFSMapa> CapasWFS { get; set; } = new List<CCapaWFSMapa>();

    public CDatosMapaLista()
    {
      FncComun();
      Clase = Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.NoDefinida;
      CodigoPropio = -1;
      Nombre = "";
      ClaseIndicador = ClaseElemento.NoDefinida;
      CodigoIndicador = -1;
      FiltrosBlock = new List<Graficos.CPasoCondicionesBlock>();
    }

    public CDatosMapaLista(CCapaWSSCN Capa, List<Graficos.CPasoCondicionesBlock> Filtros)
    {
      Clase = Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.MapaGradientes;
      FncComun();
      CodigoPropio = Capa.Codigo;
      Nombre = Capa.Nombre;
      ClaseIndicador = Capa.Clase;
      CodigoIndicador = Capa.CodigoElemento;
      FiltrosBlock = Filtros;
      switch (Capa.Clase)
      {
        case ClaseElemento.Indicador:
          CDatoIndicador Indi = CCcontenedorDatos.ContenedorUnico.IndicadorDesdeCodigo(Capa.CodigoElemento);
          if (Indi != null)
          {
            Nombre += " <" + Indi.Descripcion + ">";
          }
          break;
        case ClaseElemento.SubConsulta:
          CSubconsultaExt SC = CCcontenedorDatos.ContenedorUnico.SubconsultaCodigo(Capa.CodigoElemento);
          if (SC != null)
          {
            Nombre += " <" + SC.Nombre + ">";
          }
          break;
      }
    }

    private List<CCapaWFSMapa> ExtraerCapas(string Texto)
    {
      List<CCapaWFSMapa> Respuesta = new List<CCapaWFSMapa>();
      if (Texto.Length > 0)
      {
        string[] Datos = Texto.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string Dato in Datos)
        {
          Respuesta.Add(new CCapaWFSMapa(Dato));
        }
      }
      return Respuesta;
    }

    public CDatosMapaLista(CGraficoCompletoCN Grafico)
    {
      if (Grafico.Graficos.Count != 1)
      {
        return;
      }

      if (Grafico.Graficos[0].CampoSexo.StartsWith(CRutinas.CTE_ES_CAPA_CALOR))
      {
        Clase = Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.MapaCalor;
      }
      else
      {
        Clase = Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Pines;
      }

      if (Grafico.Vinculo2.IndexOf(CRutinas.CTE_SEPARADOR) >= 0)
      {
        Int32 Pos = Grafico.Vinculo2.IndexOf(CRutinas.CTE_SEPARADOR);
        string Resto = Grafico.Vinculo2.Substring(Pos + CRutinas.CTE_SEPARADOR.Length);
        CapasWFS = ExtraerCapas(Resto);
      }
      FncComun();
      CodigoPropio = Grafico.Codigo;
      Nombre = Grafico.Descripcion;
      ClaseIndicador = (Grafico.Indicador > 0 ? ClaseElemento.Indicador : ClaseElemento.SubConsulta);
      CodigoIndicador = (Grafico.Indicador > 0 ? Grafico.Indicador : Grafico.CodigoSC);
      string ValSexo = Grafico.Graficos[0].CampoSexo;
      Int32 Pos1 = ValSexo.IndexOf(CRutinas.CTE_SEPARADOR);
      if (Pos1 < 0)
      {
        return;
      }
      ValSexo = ValSexo.Substring(Pos1 + CRutinas.CTE_SEPARADOR.Length);
      Pos1 = ValSexo.IndexOf(CRutinas.CTE_SEPARADOR);
      if (Pos1 < 0)
      {
        return;
      }
      ColumnaLng = ValSexo.Substring(0, Pos1);
      ColumnaLat = ValSexo.Substring(Pos1 + CRutinas.CTE_SEPARADOR.Length);
      ColumnaDatos = Grafico.Graficos[0].CampoOrdenadas;
      FiltrosBlock = new List<Graficos.CPasoCondicionesBlock>();
      switch (ClaseIndicador)
      {
        case ClaseElemento.Indicador:
          CDatoIndicador Indi = CCcontenedorDatos.ContenedorUnico.IndicadorDesdeCodigo(CodigoIndicador);
          if (Indi != null)
          {
            Nombre += " <" + Indi.Descripcion + ">";
          }
          break;
        case ClaseElemento.SubConsulta:
          CSubconsultaExt SC = CCcontenedorDatos.ContenedorUnico.SubconsultaCodigo(CodigoIndicador);
          if (SC != null)
          {
            Nombre += " <" + SC.Nombre + ">";
          }
          break;
      }
    }

    private void FncComun()
    {
      CDatosGrafLista.gCodigo++;
      Codigo = CDatosGrafLista.gCodigo;
      CodigoElementoDimension = -1;
      Posicion = new Point(-1000, -1000);
      Ancho = -1;
      Alto = -1;
    }

    public CDatosMapaLista(CMapaBingCN Capa)
    {
      Clase = Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.MapaControl;
      FncComun();
      CodigoPropio = Capa.Codigo;
      Nombre = Capa.Descripcion;
      ClaseIndicador = ClaseElemento.NoDefinida;
      CodigoIndicador = -1;
      FiltrosBlock = null;
    }

    public CDatosMapaLista(CCapaChinches Capa, Int32 Indicador, List<Graficos.CPasoCondicionesBlock> Filtros)
    {
      Clase = Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.MapaCalor;
      FncComun();
      CodigoPropio = -1;
      ColumnaLat = Capa.ColumnaLat;
      ColumnaLng = Capa.ColumnaLong;
      ColumnaDatos = Capa.ColumnaValor;
      ClaseIndicador = ClaseElemento.Indicador;
      CodigoIndicador = Indicador;
      Nombre = Capa.ColumnaValor + " <" + Capa.NombreCapa + ">";
      FiltrosBlock = Filtros;
    }

    private string ExtraerCapasWSF()
    {
      string Respuesta = "";
      foreach (CCapaWFSMapa Capa in CapasWFS)
      {
        Respuesta += (Respuesta.Length == 0 ? "" : "/") + Capa.Texto;
      }
      return Respuesta;
    }

    public void AgregarAXML(XElement Superior, FloatingWindow Ventana)
    {
      XElement Contenido = new XElement(CRutinas.CTE_ELEMENTO);

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CLASE_0, ((Int32)Clase).ToString());

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_NOMBRE, Nombre);

      CRutinas.AgregarAtributosPosicion(Contenido, Ventana.Position.X, Ventana.Position.Y,
          Ventana.Width, Ventana.Height);

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CODIGO_2, CodigoPropio.ToString());
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CLASE_ELEMENTO, ((Int32)ClaseIndicador).ToString());
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_INDICADOR, CodigoIndicador.ToString());
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_ELEMENTO_DIMENSION, CodigoElementoDimension.ToString());

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_COLUMNA, ColumnaDatos);
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_COLUMNA_ABSC, ColumnaLat);
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_COLUMNA_ORD, ColumnaLng);

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CAPAS_WSF, ExtraerCapasWSF());

      Graficos.VentanaAguarda VentAg = (from UIElement Elemento in Ventana.HostPanel.Children
                               where Elemento is Graficos.VentanaAguarda
                               select (Graficos.VentanaAguarda)Elemento).FirstOrDefault();
      if (VentAg != null)
      {
        pgBingWSS PantallaWSS = VentAg.frContenedor.Content as pgBingWSS;
        if (PantallaWSS != null)
        {
          EscalaFuente = PantallaWSS.slZoomFuentes.Value;
          MuestraFondo = PantallaWSS.OpcionPosicion(0);
          MuestraNombres = PantallaWSS.OpcionPosicion(1);
          MuestraValores = PantallaWSS.OpcionPosicion(2);
          OcultaCeros = PantallaWSS.OpcionPosicion(3);
        }
      }

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_ESCALA_TEXTO, CRutinas.FloatVStr(EscalaFuente));
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_MUESTRA_FONDO, CRutinas.BoolToStr(MuestraFondo));
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_MUESTRA_NOMBRES, CRutinas.BoolToStr(MuestraNombres));
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_MUESTRA_VALORES, CRutinas.BoolToStr(MuestraValores));
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_OCULTA_CEROS, CRutinas.BoolToStr(OcultaCeros));

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_FILTRO_PROPIO, (UsaFiltroPropio ? "Y" : "N"));

      if (FiltrosBlock != null && FiltrosBlock.Count > 0)
      {
        XElement Filtros = new XElement(CRutinas.CTE_FILTROS);
        foreach (Graficos.CPasoCondicionesBlock FilLocal in FiltrosBlock)
        {
          FilLocal.AgregarAXML(Filtros);
        }
        Contenido.Add(Filtros);
      }

      Superior.Add(Contenido);

    }

    public void CargarDesdeXML(XElement Elemento)
    {

      Clase = (Graficos.CGrafV2DatosContenedorBlock.ClaseBlock)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CLASE_0);

      Nombre = CRutinas.ExtraerAtributo(Elemento, CRutinas.CTE_NOMBRE);

      double AnchoLocal;
      double AltoLocal;
      Posicion = Componentes.CContenedorBlocks.ObtenerPosicion(Elemento, out AnchoLocal, out AltoLocal);
      Ancho = AnchoLocal;
      Alto = AltoLocal;

      CodigoPropio = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CODIGO_2);
      ClaseIndicador = (ClaseElemento)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CLASE_ELEMENTO);
      CodigoIndicador = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_INDICADOR);
      CodigoElementoDimension = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_ELEMENTO_DIMENSION);

      ColumnaDatos = CRutinas.ExtraerAtributo(Elemento, CRutinas.CTE_COLUMNA);
      ColumnaLat = CRutinas.ExtraerAtributo(Elemento, CRutinas.CTE_COLUMNA_ABSC);
      ColumnaLng = CRutinas.ExtraerAtributo(Elemento, CRutinas.CTE_COLUMNA_ORD);

      CapasWFS = ExtraerCapas(CRutinas.ExtraerAtributo(Elemento, CRutinas.CTE_CAPAS_WSF));

      EscalaFuente= CRutinas.ExtraerAtributoDoble(Elemento, CRutinas.CTE_ESCALA_TEXTO,0);
      MuestraFondo= CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_MUESTRA_FONDO, true);
      MuestraNombres= CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_MUESTRA_NOMBRES, true);
      MuestraValores=CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_MUESTRA_VALORES, true);
      OcultaCeros = CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_OCULTA_CEROS, false);

      UsaFiltroPropio = CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_FILTRO_PROPIO);

      XElement ElFiltro = Elemento.Element(CRutinas.CTE_FILTROS);
      if (ElFiltro != null)
      {
        foreach (XElement ElF2 in ElFiltro.Elements(CRutinas.CTE_FILTRO))
        {
          Graficos.CPasoCondicionesBlock F = new Graficos.CPasoCondicionesBlock();
          F.CargarDesdeXML(ElF2);
          FiltrosBlock.Add(F);
        }
      }

    }
  }

  public class CCapaWFSMapa
  {
    public Int32 Capa { get; set; }
    public Int32 A { get; set; }
    public Int32 R { get; set; }
    public Int32 G { get; set; }
    public Int32 B { get; set; }

    public CCapaWFSMapa(Int32 Capa0, Int32 A0, Int32 R0, Int32 G0, Int32 B0)
    {
      Capa = Capa0;
      A = A0;
      R = R0;
      G = G0;
      B = B0;
    }

    public CCapaWFSMapa(string Datos)
    {
      string[] Valores = Datos.Split(new char[] { ';' });
      Capa = Int32.Parse(Valores[0]);
      A = Int32.Parse(Valores[1]);
      R = Int32.Parse(Valores[2]);
      G = Int32.Parse(Valores[3]);
      B = Int32.Parse(Valores[4]);
    }

    public string Texto
    {
      get
      {
        return Capa.ToString() + ";" + A.ToString() + ";" + R.ToString() + ";" +
            G.ToString() + ";" + B.ToString();
      }
    }
  }

  public class CDatosOtroLista
  {
    public Int32 Codigo { get; set; }
    public Int32 CodigoPropio { get; set; }
    public Graficos.CGrafV2DatosContenedorBlock.ClaseBlock Clase { get; set; }
    public ImageSource Imagen { get { return CRutinas.ImagenDesdeClaseMapa(Clase); } }
    public string Nombre { get; set; }
    public ClaseElemento ClaseIndicador { get; set; }
    public Int32 CodigoElementoDimension { get; set; }
    public Point Posicion { get; set; }
    public double Ancho { get; set; }
    public double Alto { get; set; }

    public CDatosOtroLista()
    {
      FncComun();
      Clase = Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.NoDefinida;
      CodigoPropio = -1;
      Nombre = "";
      ClaseIndicador = ClaseElemento.NoDefinida;
      CodigoElementoDimension = -1;
    }

    public CDatosOtroLista(CElementoIndicador IndicadorGrilla)
    {
      Clase = Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Grilla;
      FncComun();
      CodigoPropio = IndicadorGrilla.Datos.Codigo;
      Nombre = IndicadorGrilla.Datos.Descripcion;
      ClaseIndicador = ClaseElemento.Indicador;
      CodigoElementoDimension=-1;
    }

    public CDatosOtroLista(CSubconsultaExt SubConsulta)
    {
      Clase = Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Consulta;
      FncComun();
      CodigoPropio = SubConsulta.Codigo;
      Nombre = "<SC> " + SubConsulta.Nombre;
      ClaseIndicador = ClaseElemento.SubConsulta;
      CodigoElementoDimension = -1;
    }

    private void FncComun()
    {
      CDatosGrafLista.gCodigo++;
      Codigo = CDatosGrafLista.gCodigo;
      CodigoElementoDimension = -1;
      Posicion = new Point(-1000, -1000);
      Ancho = -1;
      Alto = -1;
    }

    public CDatosOtroLista(CElementoMimicoCN Mimico)
    {
      Clase = Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Mimico;
      FncComun();
      CodigoPropio = Mimico.Codigo;
      Nombre = Mimico.Nombre;
      ClaseIndicador = ClaseElemento.NoDefinida;
      CodigoElementoDimension=-1;
    }

    public void AgregarAXML(XElement Superior, FloatingWindow Ventana)
    {
      XElement Contenido = new XElement(CRutinas.CTE_ELEMENTO);

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CLASE_0, ((Int32)Clase).ToString());

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_NOMBRE, Nombre);
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CODIGO, CodigoPropio.ToString());

      CRutinas.AgregarAtributosPosicion(Contenido, Ventana.Position.X, Ventana.Position.Y,
          Ventana.Width, Ventana.Height);

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CLASE_ELEMENTO,((Int32)ClaseIndicador).ToString());
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_ELEMENTO_DIMENSION, CodigoElementoDimension.ToString());

      Superior.Add(Contenido);

    }

    public void CargarDesdeXML(XElement Elemento)
    {

      Clase = (Graficos.CGrafV2DatosContenedorBlock.ClaseBlock)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CLASE_0);

      Nombre = CRutinas.ExtraerAtributo(Elemento, CRutinas.CTE_NOMBRE);
      CodigoPropio = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CODIGO);

      double AnchoLocal;
      double AltoLocal;
      Posicion = Componentes.CContenedorBlocks.ObtenerPosicion(Elemento, out AnchoLocal, out AltoLocal);
      Ancho = AnchoLocal;
      Alto = AltoLocal;

      ClaseIndicador = (ClaseElemento)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CLASE_ELEMENTO);
      CodigoElementoDimension = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_ELEMENTO_DIMENSION);

    }

  }

}
