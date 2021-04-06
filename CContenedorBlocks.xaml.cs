using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Windows.Threading;
using SilverFlow.Controls;
using IndicadoresV2.WCFBPI;
using IndicadoresV2.Graficos;
using IndicadoresV2.Datos;

namespace IndicadoresV2.Componentes
{
  /// <summary>
  /// Es el componente que hace toda la funcionalidad del despliegue y registro de los blocks en las solapas y tarjetas.
  /// </summary>
  public partial class CContenedorBlocks : UserControl, IDisposable
  {
    public delegate void FncCompletar();

    public FncCompletar AlCompletarPedido { get; set; }

    private DispatcherTimer mTimer = null;

    public CContenedorBlocks()
    {
      InitializeComponent();
      CrearTimer();
    }

    public void CrearTimer()
    {
      if (mTimer == null)
      {
        mTimer = new DispatcherTimer();
        mTimer.Tick += FuncionTimer;
        mTimer.Interval = new TimeSpan(1, 0, 0);
        mTimer.Start();
      }
    }

    private void FuncionTimer(object sender, EventArgs e)
    {
      // Hay que refrescar el contenido de frContenido.
      foreach (FloatingWindow Ventana in Contenedor.FloatingWindows)
      {
        if (Ventana is frmReloj)
        {
          ((frmReloj)Ventana).ForzarRelecturaDatos();
        }
      }
    }

    public void Dispose()
    {
      Liberar();
    }

    public void Liberar()
    {
      Contenedor.CloseAllWindows();
    }

    public CSolapaCN Solapa { get; set; }
    public CPreguntaCN Pregunta { get; set; }

    private frmReloj BuscarReloj(Int32 Codigo, Int32 CodigoElementoDimension)
    {
      foreach (FloatingWindow Ventana in Contenedor.FloatingWindows)
      {
        if (Ventana is frmReloj)
        {
          frmReloj Reloj = (frmReloj)Ventana;
          if (Reloj.Indicador.Codigo == Codigo && Reloj.CodigoElementoDimension == CodigoElementoDimension)
          {
            return Reloj;
          }
        }
      }
      return null;
    }

    public static string XMLLimpia()
    {
      XDocument Documento = new XDocument(
        new XElement(CRutinas.CTE_INICIO));
      System.Text.StringBuilder Escritor = new System.Text.StringBuilder();
      Escritor.Append(Documento);
      return Escritor.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Indicador"></param>
    /// <param name="CodigoElementoDimension"></param>
    public frmReloj AgregarIndicador(CDatoIndicador Indicador, Int32 CodigoElementoDimension, bool Visible,
        double Abscisa = -1000, double Ordenada = -1000)
    {

      frmReloj Reloj = BuscarReloj(Indicador.Codigo, CodigoElementoDimension);
      if (Reloj != null)
      {
        if (Reloj.Visibility == Visibility.Visible)
        {
          return Reloj;
        }
      }
      else
      {
        Reloj = new frmReloj();
      }

      Point Posicion;
      if (Math.Abs(Abscisa + 1000) > 0.001 && Math.Abs(Ordenada + 1000) > 0.001)
      {
        Posicion = new Point(Abscisa, Ordenada);
      }
      else
      {
        Posicion = UbicarPosicionMasUsable(Contenedor);
        // Centra el reloj.
        Posicion.X += (AnchoVentana - Paginas.PagTarjeta.ANCHO_RELOJ) / 2;
        Posicion.Y += (AltoVentana - Paginas.PagTarjeta.ALTO_RELOJ) / 2;
      }

      Reloj.ContenedorDatos = Datos.CCcontenedorDatos.ContenedorUnico;
      if (Visible)
      {
        Reloj.TopMost = true;
      }
      Contenedor.Add(Reloj);
      //      Reloj.Visibility = CRutinas.PonerVisible(Visible);
      Reloj.Indicador = Indicador;
      Reloj.CodigoElementoDimension = CodigoElementoDimension;
      Reloj.ShowInIconbar = false;
      Reloj.VentanaBlocks = this;
      Reloj.AbscisaBase = Posicion.X;
      Reloj.OrdenadaBase = Posicion.Y;
      Reloj.CmdEditar.Visibility = Datos.CCcontenedorDatos.VisibilidadUsuario;
      Reloj.Show(Posicion.X, Posicion.Y);
      if (!Visible)
      {
        Reloj.Visibility = Visibility.Collapsed;
      }

      return Reloj;

    }

    private double AnchoVentana
    {
      get { return AnchoHost / 3 - BANDA; }
    }

    private double AltoVentana
    {
      get { return AltoHost / 2 - BANDA; }
    }

    public void AgregarTendencia(CDatoIndicador Indicador, Int32 CodigoElementoDimension, double Abscisa=-1000, double Ordenada=-1000,
        double Ancho=-1, double Alto=-1, string Adicionales="")
    {
      // Crea el elemento a incluir.
      Point Posicion = (Ancho > 0 && Alto > 0 ? new Point(Abscisa, Ordenada) : UbicarPosicionMasUsable(Contenedor));

      frmReloj Reloj = BuscarReloj(Indicador.Codigo, CodigoElementoDimension);
      if (Reloj == null)
      {
        Reloj = AgregarIndicador(Indicador, CodigoElementoDimension, false);
      }
      frmTendencia2 Tendencia = new frmTendencia2();
      Reloj.HayVentanasDependientes = true;
      if (Adicionales.Length > 0)
      {
        try
        {
          Tendencia.MostrarReferencias = (Adicionales.Substring(0, 1) == "S");
          Tendencia.MostrarBarras = (Adicionales.Substring(1, 1) == "S");
          Tendencia.MostrarAcumulado = (Adicionales.Substring(2, 1) == "S");
          Tendencia.MostrarPromedio = (Adicionales.Substring(3, 1) == "S");
          Tendencia.MostrarEvolutivo = (Adicionales.Substring(4, 1) == "S");
          Adicionales = Adicionales.Substring(5);
          string[] DatosLocales = Adicionales.Split(CRutinas.CTE_SEPARADOR.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
          Int32 CantAdic = Int32.Parse(DatosLocales[0]);
          Int32 Pos = 1;
          for (Int32 i = 0; i < CantAdic && Pos < DatosLocales.Length; i++)
          {
            Int32 CodIndi = Int32.Parse(DatosLocales[Pos++]);
            Int32 CodElem = Int32.Parse(DatosLocales[Pos++]);
            ElementoComparacion Elemento = new ElementoComparacion(Datos.CCcontenedorDatos.ContenedorUnico.IndicadorDesdeCodigo(CodIndi),
              CodElem,
              (DatosLocales[Pos] == "S"), null);
            Tendencia.IndicadoresComparativos.Add(Elemento);
            Pos++;
          }
        }
        catch (Exception)
        {
          //
        }
      }
      Tendencia.RelojCreador = Reloj;
      Tendencia.ContenedorDatos = Datos.CCcontenedorDatos.ContenedorUnico;
      Tendencia.tbNombre.Text = Indicador.Descripcion;
      Contenedor.Add(Tendencia);
      if (Ancho > 0 && Alto > 0)
      {
        Tendencia.Width = Ancho;
        Tendencia.Height = Alto;
      }
      else
      {
        Tendencia.Width = AnchoVentana;
        Tendencia.Height = AltoVentana;
      }
      Tendencia.Show(Posicion.X, Posicion.Y);
    }

    private Graficos.VentanaAguarda CrearVentanaAguarda(object Referencia, bool EliminarHeader=false)
    {
      Graficos.VentanaAguarda VentanaAguarda = new Graficos.VentanaAguarda()
      {
        Tag = Referencia,
        SinHeader=EliminarHeader
      };

      Point Posicion = UbicarPosicionMasUsable(Contenedor);

      VentanaAguarda.Width = AnchoVentana;
      VentanaAguarda.Height = AltoVentana;

      Contenedor.Add(VentanaAguarda);

      VentanaAguarda.Show(Posicion.X, Posicion.Y);

      return VentanaAguarda;

    }

    private Graficos.VentanaAguarda CrearVentanaAguardaPosicion(object Referencia, Point Posicion, double Ancho, double Alto,
          bool EliminarHeader = false)
    {
      Graficos.VentanaAguarda VentanaAguarda = new Graficos.VentanaAguarda()
      {
        Tag = Referencia,
        SinHeader = EliminarHeader
      };

      VentanaAguarda.Width = Ancho;
      VentanaAguarda.Height = Alto;

      Contenedor.Add(VentanaAguarda);

      VentanaAguarda.Show(Posicion.X, Posicion.Y);

      return VentanaAguarda;

    }

    private VentanaAguarda UbicarVentanaAguarda(object Referencia)
    {
      foreach (FloatingWindow Ventana in Contenedor.FloatingWindows)
      {
        if (Ventana is VentanaAguarda && Ventana.Tag == Referencia)
        {
          return Ventana as VentanaAguarda;
        }
        //if (Ventana is VentanaAguarda && Ventana.Tag != null)
        //{
        //  Int32 CodigoLocal = (Ventana.Tag is Paginas.CDatosGrafLista ? ((Paginas.CDatosGrafLista)Ventana.Tag).Codigo :
        //    (Ventana.Tag is Paginas.CDatosMapaLista ? ((Paginas.CDatosMapaLista)Ventana.Tag).CodigoPropio :
        //    (Ventana.Tag is Paginas.CDatosOtroLista ? ((Paginas.CDatosOtroLista)Ventana.Tag).CodigoPropio : -1)));
        //  if (CodigoLocal == Codigo)
        //  {
        //    return Ventana as VentanaAguarda;
        //  }
        //}
      }
      return null;
    }

    private VentanaAguarda UbicarVentanaAguardaOtros(CGrafV2DatosContenedorBlock.ClaseBlock Clase, Int32 Codigo)
    {
      foreach (FloatingWindow Ventana in Contenedor.FloatingWindows)
      {
        if (Ventana is VentanaAguarda && Ventana.Tag != null)
        {
          if (Ventana.Tag is Paginas.CDatosOtroLista)
          {
            Paginas.CDatosOtroLista DatosOtro = (Paginas.CDatosOtroLista)Ventana.Tag;
            if (DatosOtro.Clase==Clase && DatosOtro.CodigoPropio == Codigo)
            {
              return Ventana as VentanaAguarda;
            }
          }
        }
      }
      return null;
    }

    private Point PosicionDesdeVentanaAguarda(object Referencia) //Int32 Codigo)
    {
      VentanaAguarda Ventana=UbicarVentanaAguarda(Referencia); //Codigo);
      if (Ventana != null)
      {
        Contenedor.Remove(Ventana);
        return Ventana.Position;
      }
      return new Point(0, 0);
    }

    private List<Paginas.CDatosGrafLista> mListaDatosGraf = new List<Paginas.CDatosGrafLista>();
    public void AgregarGrafico(Paginas.CDatosGrafLista DatosGraf)
    {
      Graficos.CGrafV2DatosContenedorBlock Datos = DatosGraf.Datos;

      frmReloj Reloj = BuscarReloj(Datos.Indicador, Datos.CodigoElementoDimension);
      if (Reloj == null)
      {
        CDatoIndicador DefIndicador = CCcontenedorDatos.ContenedorUnico.IndicadorDesdeCodigo(Datos.Indicador);
        if (DefIndicador == null)
        {
          MessageBox.Show("No encuentra indicador " + Datos.Indicador.ToString());
          return;
        }
        Reloj = AgregarIndicador(DefIndicador, Datos.CodigoElementoDimension, false);
      }

      mListaDatosGraf.Add(DatosGraf);

      if (Reloj.ProveedorComprimido() == null)
      {
        if (DatosGraf.Datos.Ancho > 0)
        {
          CrearVentanaAguardaPosicion(DatosGraf, Datos.Posicion, Datos.Ancho, Datos.Alto);
        }
        else
        {
          CrearVentanaAguarda(DatosGraf);
        }
        Reloj.AgregarCompromisoBlockAlLeerDatos(DatosGraf.Codigo);
      }
      else
      {
        CompletarCreacionGrafico(Reloj, DatosGraf.Codigo, false);
      }

    }

    public void AgregarConjunto(Paginas.CDatosGrafLista DatosGraf)
    {
      Graficos.CGrafV2DatosContenedorBlock Datos = DatosGraf.Datos;

      frmReloj Reloj = BuscarReloj(Datos.Indicador, Datos.CodigoElementoDimension);
      if (Reloj == null)
      {
        CDatoIndicador DefIndicador = CCcontenedorDatos.ContenedorUnico.IndicadorDesdeCodigo(Datos.Indicador);
        if (DefIndicador == null)
        {
          MessageBox.Show("No encuentra indicador " + Datos.Indicador.ToString());
          return;
        }
        Reloj = AgregarIndicador(DefIndicador, Datos.CodigoElementoDimension, false);
      }

      mListaDatosGraf.Add(DatosGraf);

      if (Reloj.ProveedorComprimido() == null)
      {
        if (DatosGraf.Datos.Ancho < 0)
        {
          CrearVentanaAguarda(DatosGraf);
        }
        else
        {
          CrearVentanaAguardaPosicion(DatosGraf, DatosGraf.Datos.Posicion, DatosGraf.Datos.Ancho, DatosGraf.Datos.Alto);
        }
        Reloj.AgregarCompromisoBlockAlLeerDatos(DatosGraf.Codigo);
      }
      else
      {
        CompletarCreacionGrafico(Reloj, DatosGraf.Codigo, false);
      }

    }

    private List<Paginas.CDatosMapaLista> mListaDatosMapa = new List<Paginas.CDatosMapaLista>();
    public void AgregarMapa(Paginas.CDatosMapaLista Datos)
    {
      if (Datos.ClaseIndicador == ClaseElemento.Indicador && Datos.CodigoIndicador > 0)
      {

        frmReloj Reloj = BuscarReloj(Datos.CodigoIndicador, Datos.CodigoElementoDimension);
        if (Reloj == null)
        {
          CDatoIndicador DefIndicador = CCcontenedorDatos.ContenedorUnico.IndicadorDesdeCodigo(Datos.CodigoIndicador);
          if (DefIndicador == null)
          {
            MessageBox.Show("No encuentra indicador " + Datos.CodigoIndicador.ToString());
            return;
          }
          Reloj = AgregarIndicador(DefIndicador, Datos.CodigoElementoDimension, false);
        }

        mListaDatosMapa.Add(Datos);

        if (Reloj.ProveedorComprimido() == null)
        {
          if (Datos.Ancho < 0)
          {
            CrearVentanaAguarda(Datos);
          }
          else
          {
            CrearVentanaAguardaPosicion(Datos, Datos.Posicion, Datos.Ancho, Datos.Alto);
          }
          Reloj.AgregarCompromisoBlockAlLeerDatos(Datos.Codigo);
        }
        else
        {
          CompletarCreacionMapa(Reloj, Datos.Codigo);
        }
      }
      else
      {
        mListaDatosMapa.Add(Datos);
        CompletarCreacionMapa(null, Datos.Codigo);
      }

    }

    private List<Paginas.CDatosOtroLista> mListaDatosOtros = new List<Paginas.CDatosOtroLista>();
    public void AgregarMimico(Paginas.CDatosOtroLista Datos)
    {
      VentanaAguarda Ventana = (Datos.Ancho < 0 ?
        CrearVentanaAguarda(Datos) :
        CrearVentanaAguardaPosicion(Datos, Datos.Posicion, Datos.Ancho, Datos.Alto));
      Paginas.PagMimico PagMim = new Paginas.PagMimico();
      PagMim.DesdeInforme = true;
      PagMim.Clase = Componentes.ClaseElementoMenu.Mimico;
      PagMim.Codigo = Datos.CodigoPropio;
      PagMim.Contenedor = MainPage.Contenedor;
      PagMim.Superior = MainPage.PunteroMainPage;
      Ventana.frContenedor.Content = PagMim;
      Ventana.SP.Visibility = Visibility.Collapsed;
    }

    public void AgregarGrilla(Paginas.CDatosOtroLista Datos)
    {

      frmReloj Reloj = BuscarReloj(Datos.CodigoPropio, Datos.CodigoElementoDimension);
      if (Reloj == null)
      {
        CDatoIndicador DefIndicador = CCcontenedorDatos.ContenedorUnico.IndicadorDesdeCodigo(Datos.CodigoPropio);
        if (DefIndicador == null)
        {
          MessageBox.Show("No encuentra indicador " + Datos.CodigoPropio.ToString());
          return;
        }
        Reloj = AgregarIndicador(DefIndicador, Datos.CodigoElementoDimension, false);
      }

      mListaDatosOtros.Add(Datos);

      if (Reloj.ProveedorComprimido() == null)
      {
        if (Datos.Ancho > 0)
        {
          CrearVentanaAguardaPosicion(Datos, Datos.Posicion, Datos.Ancho, Datos.Alto);
        }
        else
        {
          CrearVentanaAguarda(Datos);
        }
        Reloj.AgregarCompromisoBlockAlLeerDatos(Datos.Codigo);
      }
      else
      {
        CompletarCreacionOtro(Reloj, Datos.Codigo);
      }

    }

    private frmReloj BuscarRelojFicticio(WCFBPI.ClaseElemento ClaseOrg,
          Int32 CodigoOrg, Int32 CodigoElemento)
    {
      foreach (FloatingWindow Ventana in Contenedor.FloatingWindows)
      {
        if (Ventana is frmReloj)
        {
          frmReloj Reloj = (frmReloj)Ventana;
          if (Reloj.ClaseOrigen == ClaseOrg && Reloj.CodigoOrigen == CodigoOrg &&
              Reloj.CodigoElementoDimension == CodigoElemento)
          {
            return Reloj;
          }
        }
      }
      return null;
    }

    private frmReloj CrearRelojFicticio(WCFBPI.ClaseElemento ClaseOrg,
          Int32 CodigoOrg, CProveedorComprimido Proveedor, string NombreFicticio,
          List<WCFBPI.CParametroExt> PrmSC)
    {
      frmReloj Reloj = BuscarRelojFicticio(ClaseOrg, CodigoOrg, -1);
      if (Reloj == null)
      {
        Reloj = new Graficos.frmReloj();
        Reloj.PuedeAgruparPeriodos = false;
        Reloj.ClaseOrigen = ClaseOrg;
        Reloj.CodigoOrigen = CodigoOrg;
        Reloj.ContenedorDatos = Datos.CCcontenedorDatos.ContenedorUnico;
        Contenedor.Add(Reloj);
        Reloj.Indicador = CRutinas.CrearIndicadorParaDataset(NombreFicticio);
        Graficos.BlockDatosZip Block = new Graficos.BlockDatosZip(CodigoOrg);
        Block.Periodo = -1;
        Block.FechaHora = DateTime.Now;
        Block.Proveedor = Proveedor;
        Reloj.ImponerBlock(Block);
        Reloj.CodigoElementoDimension = -1;
        Reloj.ShowInIconbar = false;
        Reloj.VentanaBlocks = this;
        Reloj.AbscisaBase = 0;
        Reloj.OrdenadaBase = 0;
        Reloj.CmdEditar.Visibility = Visibility.Collapsed;
        Reloj.Show(0, 0);
        Reloj.Visibility = Visibility.Collapsed;
      }

      return Reloj;

    }

    public void AgregarSubConsulta(Paginas.CDatosOtroLista Datos)
    {

      if (Datos.Posicion.X > -100)
      {
        CrearVentanaAguardaPosicion(Datos, Datos.Posicion, Datos.Ancho, Datos.Alto);
      }
      else
      {
        CrearVentanaAguarda(Datos);
      }

      // Leer el detalle y despues poner en la pantalla.
      WCFBPIClient Cliente = CRutinas.ObtenerClienteBPI();
      try
      {
        Cliente.DetalleSubconsultaCompleted += Cliente_DetalleSubconsultaCompleted;
        Cliente.DetalleSubconsultaAsync(CRutinas.Ticket, Datos.CodigoPropio, new List<WCFBPI.CParametroExt>(), "", -1, Guid.NewGuid().ToString(), false);
        mListaDatosOtros.Add(Datos);
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

    private void RefrescarPedido(string GUID)
    {
      WCFBPIClient Cliente = CRutinas.ObtenerClienteBPI();
      try
      {
        Cliente.RefrescarPedidoDetalleSubconsultaBinCompleted += Cliente_RefrescarPedidoDetalleSubconsultaBinCompleted;
        Cliente.RefrescarPedidoDetalleSubconsultaBinAsync(CRutinas.Ticket, GUID, true);
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

    private void Cliente_RefrescarPedidoDetalleSubconsultaBinCompleted(object sender, RefrescarPedidoDetalleSubconsultaBinCompletedEventArgs e)
    {
      try
      {
        ProcesarRespuestaSC(e.Result);
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarError(ex);
      }
    }

    private void ProcesarDataset(Int32 CodigoSC, byte[] Datos)
    {
      CProveedorComprimido ProvDatos = new IndicadoresV2.Datos.CProveedorComprimido(ClaseElemento.Consulta, CodigoSC);
      ProvDatos.ProcesarDatasetBinarioUnicode(Datos, false);
      frmReloj Reloj = CrearRelojFicticio(ClaseElemento.Consulta, CodigoSC, ProvDatos,
          "SC_" + CodigoSC.ToString(), new List<CParametroExt>());

      Reloj.CmdGrilla_Click(null, null);

      // Reposicionar, poner TAG y cerrar aguarda.
      Graficos.VentanaAguarda VentAg = UbicarVentanaAguardaOtros(CGrafV2DatosContenedorBlock.ClaseBlock.Consulta, CodigoSC);
      if (VentAg != null)
      {
        Graficos.frmContenedorFiltros Filtros = Reloj.UbicarContenedorFiltros();
        if (Filtros != null)
        {
          Filtros.Tag = VentAg.Tag;
          Filtros.Position = VentAg.Position;
          VentAg.Close();
        }
      }
    }

    private void ProcesarRespuestaSC(CRespuestaDatasetBinSC Respuesta)
    {
      try
      {
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MensajeError);
        }
        switch (Respuesta.Situacion)
        {
          case SituacionPedido.EnMarcha:
            RefrescarPedido(Respuesta.GUID);
            break;
          case SituacionPedido.Abortado:
            return;
          default:
            ProcesarDataset(Respuesta.CodigoSC, Respuesta.Datos);
            break;
        }
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarError(ex);
      }
    }

    private void Cliente_DetalleSubconsultaCompleted(object sender, DetalleSubconsultaCompletedEventArgs e)
    {
      try
      {
        ProcesarRespuestaSC(e.Result);
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarError(ex);
      }
    }

    public void ModificarBoton(WCFBPI.CPreguntaCN Boton)
    {
      VentanaAguarda Ventana = FichaDesdeCodigo(Boton.Codigo);
      if (Ventana != null)
      {
        BotonPanel.Ficha Ficha = Ventana.bdFicha.Child as BotonPanel.Ficha;
        if (Ficha != null)
        {
          Ficha.Pregunta = Boton;
          Datos.CCcontenedorDatos.ContenedorUnico.AjustarElementosDimensionIndicadoresBoton(Boton);
        }
      }
    }

    public void EliminarBoton(WCFBPI.CPreguntaCN Boton)
    {
      VentanaAguarda Ventana = FichaDesdeCodigo(Boton.Codigo);
      if (Ventana != null)
      {
        Contenedor.Remove(Ventana);
      }
    }

    public VentanaAguarda FichaDesdeCodigo(Int32 Codigo)
    {
      foreach (FloatingWindow F in Contenedor.FloatingWindows)
      {
        VentanaAguarda Ventana = (F as VentanaAguarda);
        if (Ventana != null && Ventana.bdFicha.Child != null)
        {
          BotonPanel.Ficha Ficha = Ventana.bdFicha.Child as BotonPanel.Ficha;
          if (Ficha != null && Ficha.Codigo == Codigo)
          {
            return Ventana;
          }
        }
      }
      return null;
    }

    public void AgregarFicha(CPreguntaCN Ficha, double Abscisa = -1000, double Ordenada = -1000, double Ancho = -1, double Alto = -1)
    {
      BotonPanel.Ficha Tarjeta = new BotonPanel.Ficha();
      Tarjeta.Indicadores.Clear();
      Tarjeta.IndicadoresEnPregunta.Clear();
      Tarjeta.DatosAlarma.Clear();
      Tarjeta.Factor = 1;
      Tarjeta.PaginaSala = null;
      Tarjeta.ContenedorBlocks = this;
      Tarjeta.Pregunta = Ficha;
      Tarjeta.Codigo = Ficha.Codigo;
      Tarjeta.TextoPregunta = Ficha.Pregunta;
      if (Ficha.Block.Length > 0)
      {
        Tarjeta.AgregarIndicadoresATimer();
      }
      VentanaAguarda Ventana = (Ancho < 0 ? CrearVentanaAguarda(Ficha, true) :
          CrearVentanaAguardaPosicion(Ficha, new Point(Abscisa, Ordenada), Ancho, Alto, true));
      Ventana.BorderThickness = new Thickness(0);
      Ventana.BorderBrush = new SolidColorBrush(Colors.Transparent);
      Ventana.Background = new SolidColorBrush(Colors.Transparent);
      Ventana.LayoutRoot.Background = new SolidColorBrush(Colors.Transparent);
      Ventana.ShowMaximizeButton = false;
      Ventana.ShowCloseButton = false;
      Ventana.ShowMinimizeButton = false;
      Ventana.ShowRestoreButton = false;
      Ventana.bdFicha.Visibility = Visibility.Visible;
      Ventana.frContenedor.Visibility = Visibility.Collapsed;
      Ventana.SP.Visibility = Visibility.Collapsed;
      Point Punto = UbicarPosicionMasUsable(Contenedor);
      double DeltaH = (AnchoVentana - Paginas.PagSalaReunion.ANCHO_BOTON_RELOJ) / 2;
      double DeltaV = (AltoVentana - Paginas.PagSalaReunion.ALTO_BOTON_RELOJ) / 2;
      Ventana.Width = Paginas.PagSalaReunion.ANCHO_BOTON_RELOJ;
      Ventana.Height = Paginas.PagSalaReunion.ALTO_BOTON_RELOJ;
      Ventana.bdFicha.Child = Tarjeta;
      Ventana.Tag = Ficha;
      Tarjeta.VentanaContenedoraBlocks = Ventana;

      CPuntoPregunta PuntoPreg = Datos.CCcontenedorDatos.ContenedorUnico.PuntoPreguntaDesdeCodigo(Ficha.Codigo);

      if (PuntoPreg != null)
      {
        if (PuntoPreg.Indicadores == null)
        {
          PuntoPreg.Indicadores = new List<WCFBPI.CPreguntaIndicadorCN>();
        }
        foreach (WCFBPI.CPreguntaIndicadorCN Indicador in PuntoPreg.Indicadores)
        {
          Tarjeta.IndicadoresEnPregunta.Add(Indicador);
          WCFBPI.CDatoIndicador DatoIndicador =
              Datos.CCcontenedorDatos.ContenedorUnico.IndicadorDesdeCodigo(Indicador.Indicador);
          if (DatoIndicador != null)
          {
            Tarjeta.Indicadores.Add(DatoIndicador);
            WCFBPI.CInformacionAlarmaCN DatosAlarma =
                Datos.CCcontenedorDatos.ContenedorUnico.DatosAlarmaIndicador(Indicador.Indicador,
                    Indicador.Dimension, Indicador.ElementoDimension);
            if (DatosAlarma != null)
            {
              Tarjeta.DatosAlarma.Add(DatosAlarma);
            }
            else
            {
              WCFBPI.CInformacionAlarmaCN Alarma =
                  new WCFBPI.CInformacionAlarmaCN();
              Alarma.Color = "GRIS";
              Tarjeta.DatosAlarma.Add(Alarma);
            }
          }
        }
      }


      Tarjeta.FuncionClick = SeleccionoFicha;

      Tarjeta.AjustarColorAlarma();

      Tarjeta.PosicionarProximaAlarma();

      Tarjeta.DispararAnimaciones();

      Tarjeta.CmdEdit.Visibility = Datos.CCcontenedorDatos.VisibilidadUsuario;

      Ventana.Position = new Point(Ventana.Position.X + DeltaH, Ventana.Position.Y + DeltaV);

    }

    private void SeleccionoFicha(BotonPanel.Ficha Ficha)
    {
      IndicadoresV2.Paginas.PagTarjeta Pagina = new IndicadoresV2.Paginas.PagTarjeta();
      Pagina.Metas = null;
      Pagina.PaginaGIS = null;
      Pagina.PaginaAnterior = null;
      Pagina.PaginaSalaReunion = null;
      Pagina.ContenedorDatos = CCcontenedorDatos.ContenedorUnico;
      //      Pagina.Pregunta = mContenedor.PreguntaDesdeCodigo(Pregunta.Codigo);
      Pagina.Detalle = Paginas.PagSalaReunion.CrearDetallePregunta(Ficha);
      Pagina.Superior = MainPage.PunteroMainPage;
      Pagina.Superior.MostrarPagina(Pagina);
    }

    private List<CFiltradorStep> ExtraerFiltros(List<Graficos.CPasoCondicionesBlock> Filtros, List<CColumnaBase> Columnas)
    {
      List<CFiltradorStep> Respuesta = new List<CFiltradorStep>();
      foreach (Graficos.CPasoCondicionesBlock Paso in Filtros)
      {
        Respuesta.Add(ExtraerFiltradorStepDesdePasoCondiciones(Paso, Columnas));
      }
      return Respuesta;
    }

    private CFiltradorStep ExtraerFiltradorStepDesdePasoCondiciones(CPasoCondicionesBlock Cnd, List<CColumnaBase> Columnas)
    {
      CFiltradorStep Paso = new CFiltradorStep();
      Paso.CumplirTodas = false;
      foreach (Graficos.CGrupoCondicionesBlock Grupo in Cnd.Grupos)
      {
        CCondiciones CndsLocal = new CCondiciones();
        CndsLocal.TodasLasCondiciones = false;
        CndsLocal.IncluyeCondiciones = true;
        foreach (Graficos.CCondicionBlock CndBlock in Grupo.Condiciones)
        {
          Datos.CCondicion CndLocal = new CCondicion();
          CColumnaBase Columna = (from C in Columnas
                                  where C.Nombre == CndBlock.Columna
                                  select C).FirstOrDefault();
          if (Columna != null)
          {
            CndLocal.Clase = Columna.Clase;
            CndLocal.Modo = CndBlock.Modo;
            CndLocal.ValorIgual = CndBlock.Valor;
            CndLocal.ColumnaCondicion = Columna.Orden;
            switch (CndLocal.Modo)
            {
              case ModoFiltrar.Igual:
                break;
              default:
                if (Columna.Clase == ClaseVariable.Fecha)
                {
                  CndLocal.ValorMinimo = CRutinas.DecodificarFechaHora(CndBlock.Valor).ToOADate();
                  CndLocal.ValorMaximo = CRutinas.DecodificarFechaHora(CndBlock.ValorMaximo).ToOADate();
                }
                else
                {
                  CndLocal.ValorMinimo = CRutinas.StrVFloat(CndBlock.Valor);
                  CndLocal.ValorMaximo = CRutinas.StrVFloat(CndBlock.ValorMaximo);
                }
                break;
            }
            CndLocal.AjustarIndices(Columnas);
            CndsLocal.AgregarCondicion(CndLocal);
          }
        }
        Paso.AgregarCondicion(CndsLocal);
      }
      return Paso;
    }

    private CFiltradorStep ExtraerFiltro(Graficos.CGrafV2DatosContenedorBlock Datos, List<CColumnaBase> Columnas)
    {
      if (Datos.Filtro != null)
      {
        if (Datos.Filtro.Grupos.Count != 1 || Datos.Filtro.Grupos[0].Condiciones.Count == 0 ||
            Datos.Filtro.Grupos[0].Condiciones[0].Columna != Datos.ColumnaAbscisas)
        {
          return ExtraerFiltradorStepDesdePasoCondiciones(Datos.Filtro, Columnas);
        }
      }
      return null;
    }

    private List<Paginas.CDatosGrafLista> mListaDatosGraficosRefresco = new List<Paginas.CDatosGrafLista>();

    public void CompletarCreacionGrafico(frmReloj Reloj, Int32 CodigoGraf, bool BuscaAguarda = true)
    {
      try
      {

        

        Paginas.CDatosMapaLista DatosMapaVentana = (from L in mListaDatosMapaVentanas
                                             where L.Codigo == CodigoGraf
                                             select L).FirstOrDefault();
        if (DatosMapaVentana != null)
        {
          RefrescarDatosMapa(Reloj, CodigoGraf);
          return;
        }

        Paginas.CDatosMapaLista DatosMapa = (from L in mListaDatosMapa
                                             where L.Codigo == CodigoGraf
                                             select L).FirstOrDefault();
        if (DatosMapa != null)
        {
          CompletarCreacionMapa(Reloj, CodigoGraf);
        }
        else
        {
          Paginas.CDatosOtroLista DatosOtro = (from L in mListaDatosOtros
                                               where L.Codigo == CodigoGraf
                                               select L).FirstOrDefault();
          if (DatosOtro != null)
          {
            CompletarCreacionOtro(Reloj, CodigoGraf);
          }
          else
          {
            Paginas.CDatosGrafLista DatosGrafVentana = (from L in mListaDatosGraficosRefresco
                                                        where L.Codigo == CodigoGraf
                                                        select L).FirstOrDefault();
            if (DatosGrafVentana != null)
            {
              RefrescarDatosMapa(Reloj, CodigoGraf);
              return;
            }

            // Crea el elemento a incluir.
            Paginas.CDatosGrafLista DatosLista = (from L in mListaDatosGraf
                                                  where L.Codigo == CodigoGraf
                                                  select L).FirstOrDefault();
            if (DatosLista != null)
            {
              mListaDatosGraf.Remove(DatosLista);
              mListaDatosGraficosRefresco.Add(DatosLista);

              Point Posicion = (BuscaAguarda ? PosicionDesdeVentanaAguarda(DatosLista) : // CodigoGraf) :
                (DatosLista.Datos.Ancho < 0 ? UbicarPosicionMasUsable(Contenedor) : DatosLista.Datos.Posicion));

              double AnchoLocal = (DatosLista.Datos.Ancho < 0 ? AnchoVentana : DatosLista.Datos.Ancho);
              double AltoLocal = (DatosLista.Datos.Alto < 0 ? AltoVentana : DatosLista.Datos.Alto);

              Graficos.CGrafV2DatosContenedorBlock Datos = DatosLista.Datos;

              switch (Datos.ClaseElemento)
              {
                case CGrafV2DatosContenedorBlock.ClaseBlock.Grafico:

                  frmCrearGraf Graf0 = new frmCrearGraf();
                  Graf0.Tag = Datos;
                  Graf0.NombreImpuesto = Datos.Nombre;
                  Graf0.ContenedorBlocks = this;

                  if (Datos.UsaFiltroPropio)
                  {
                    Graf0.FiltrosBlocks = ExtraerFiltros(Datos.FiltrosBlock, Reloj.Columnas);
                  }
                  else
                  {
                    Graf0.FiltrosBlocks = null;
                  }
                  Graf0.Reloj = Reloj;
                  Contenedor.Add(Graf0);
                  Graf0.SerieAbscisas = Reloj.ProveedorComprimido().ColumnaNombre(Datos.ColumnaAbscisas);
                  Graf0.SerieOrdenadas = Reloj.ProveedorComprimido().ColumnaNombre(Datos.ColumnaOrdenadas);
                  Graf0.SerieSexo = Reloj.ProveedorComprimido().ColumnaNombre(Datos.ColumnaSexo);
                  Graf0.BarrasH = (Datos.Clase == ClaseGrafico.BarrasH);
                  Graf0.ClaseDelGrafico = (Datos.Clase == ClaseGrafico.BarrasH ? ClaseGrafico.Barras : Datos.Clase);
                  Graf0.ModoAgrIndep = Datos.AgrupIndep;
                  Graf0.ModoAgrDep = Datos.Agrupacion;
                  Graf0.CargarGrafico = true;
                  Graf0.IniciarCerrada = true;
                  Graf0.tbNombre.Text = Datos.Nombre;
                  Reloj.HayVentanasDependientes = true;

                  if (Datos.UsaFiltroPropio)
                  {
                    Graf0.ActualizarDatosGrafico(false);
                  }

                  Contenedor.Add(Graf0);
                  Graf0.Width = AnchoLocal;
                  Graf0.Height = AltoLocal;
                  Graf0.Show(Posicion.X, Posicion.Y);
                  break;

                case CGrafV2DatosContenedorBlock.ClaseBlock.Conjunto:
                  CProveedorComprimido Proveedor = Reloj.ProveedorComprimido();
                  Proveedor.AjustarDependientes = Reloj.FncRefrescarDatosDataset;
                  frmFiltroFlotante Filtro = new frmFiltroFlotante();
                  Filtro.DesdeBlocks = true;
                  Filtro.Tag = Datos;
                  IndicadoresV2.Componentes.CFiltrador FiltroProv = Proveedor.FiltroParaColumna(Datos.ColumnaAbscisas);
                  if (FiltroProv == null)
                  {
                    Filtro.Filtro.Filtrador.Columna = Proveedor.ColumnaNombre(Datos.ColumnaAbscisas);
                    Proveedor.Filtros.Add(Filtro.Filtro.Filtrador);
                  }
                  else
                  {
                    Filtro.Filtro.Filtrador = FiltroProv;
                  }
                  Filtro.Filtro.Proveedor = Proveedor;
                  Filtro.Reloj = Reloj;
                  Contenedor.Add(Filtro);

                  Filtro.Filtro.NoAgrandar = true;

                  Filtro.Filtro.ModoDeAgrupar = Datos.Agrupacion;

                  Filtro.Filtro.Filtrador.CrearInformacionFilas();

                  Filtro.Filtro.DesdeBlock = true;

                  Filtro.Width = AnchoLocal;
                  Filtro.Height = AltoLocal;
                  Filtro.Visibility = CRutinas.PonerVisible(Datos.Visible);
                  Filtro.Show(Posicion.X, Posicion.Y);

                  if (Datos.Filtro != null && Datos.Filtro.Grupos.Count > 0)
                  {
                    Filtro.Filtro.Filtrador.CantidadNecesaria = Datos.Filtro.Grupos[0].CantidadMinima;
                    foreach (CCondicionBlock Cnd in Datos.Filtro.Grupos[0].Condiciones)
                    {
                      switch (Datos.Filtro.Grupos[0].Condiciones[0].Modo)
                      {
                        case ModoFiltrar.Igual:
                          Filtro.Filtro.ImponerValorSeleccionadoBlock(Cnd.Valor);
                          break;
                        case ModoFiltrar.PorRango:
                          Filtro.Filtro.ImponerRangoBlock(Cnd.Valor, Cnd.ValorMaximo);
                          break;
                      }
                    }
                  }

                  Filtro.Filtro.InicializarDesdeBlock();

                  if (Datos.ColumnaOrdenadas.Length > 0)
                  {
                    Filtro.Filtro.ImponerColumnaValor(Proveedor.ColumnaNombre(Datos.ColumnaOrdenadas));
                  }


                  break;
              }

            }

            if (AlCompletarPedido != null)
            {
              AlCompletarPedido();
            }
          }
        }

      }
      catch (Exception ex)
      {
        MessageBox.Show("Al completar creacion");
        CRutinas.DesplegarError(ex);
      }

    }

    public IndicadoresV2.WCFBPI.CGraficoCN ArmarDatosGrafico(Paginas.CDatosMapaLista Datos)
    {
      IndicadoresV2.WCFBPI.CGraficoCN Respuesta = new WCFBPI.CGraficoCN();
      Respuesta.ClaseDeGrafico = WCFBPI.ClaseGrafico.Torta;
      Respuesta.Abscisas = 0;
      Respuesta.Ordenadas = 0;
      Respuesta.Ancho = 100;
      Respuesta.Alto = 100;
      Respuesta.CampoAbscisas = Datos.ColumnaLng;
      Respuesta.CampoOrdenadas = Datos.ColumnaDatos;
      Respuesta.AgrupacionAbscisas = WCFBPI.ModoAgruparIndependiente.Todos;
      Respuesta.AgrupacionOrdenadas = WCFBPI.ModoAgruparDependiente.Acumulado;

      Respuesta.CondicionesFiltro = new List<WCFBPI.CCondicionFiltroCN>();

      return Respuesta;
    }

    private List<CLineaComprimida> FiltrarDatosInicialesBlock(List<CColumnaBase> ColsIni, List<CLineaComprimida> LinsIni,
          List<CFiltradorStep> FiltrosBlocks)
    {
      if (FiltrosBlocks != null && FiltrosBlocks.Count > 0)
      {
        for (Int32 i = 0; i < FiltrosBlocks.Count; i++)
        {
          FiltrosBlocks[i].FiltrarDatos((i == 0 ? LinsIni : FiltrosBlocks[i - 1].Datos), ColsIni);
        }
        return FiltrosBlocks.Last().Datos;
      }
      else
      {
        return LinsIni;
      }
    }

    private List<Paginas.CDatosMapaLista> mListaDatosMapaVentanas = new List<Paginas.CDatosMapaLista>();

    private frmCrearGraf UbicarVentanaGraf(Paginas.CDatosGrafLista Datos)
    {
      foreach (FloatingWindow Ventana in Contenedor.FloatingWindows)
      {
        if (Ventana is frmCrearGraf && Ventana.Tag == Datos.Datos)
        {
          return (frmCrearGraf)Ventana;
        }
      }
      return null;
    }

    private void RefrescarDatosMapa(frmReloj Reloj, Int32 CodigoMapa)
    {

      try
      {
        Paginas.CDatosMapaLista DatosMapa = (from L in mListaDatosMapaVentanas
                                             where L.Codigo == CodigoMapa
                                             select L).FirstOrDefault();
        if (DatosMapa != null)
        {

          // Crea el elemento a incluir.
          VentanaAguarda Ventana = UbicarVentanaAguarda(DatosMapa); // CodigoMapa);
          if (Ventana == null)
          {
            return;
          }

          Ventana.SP.Visibility = Visibility.Collapsed;

          switch (DatosMapa.Clase)
          {
            case CGrafV2DatosContenedorBlock.ClaseBlock.MapaControl:

              Paginas.PagBing PgBing = (Paginas.PagBing)Ventana.frContenedor.Content;
              PgBing.Redibujar();
              break;

            case CGrafV2DatosContenedorBlock.ClaseBlock.Pines:
            case CGrafV2DatosContenedorBlock.ClaseBlock.MapaCalor:

              Paginas.PagGISDatos PagGIS = (Paginas.PagGISDatos)Ventana.frContenedor.Content;
              PagGIS.DesdeSolapa = true;
              PagGIS.ProveedorCreador = Reloj.ProveedorComprimido();
              PagGIS.Vinculo = CRutinas.CrearVinculoIndicadorCompleto();
              PagGIS.FiltrosBlocks = (DatosMapa.UsaFiltroPropio ?
                ExtraerFiltros(DatosMapa.FiltrosBlock, PagGIS.ProveedorCreador.Columnas) : null);
              WCFBPI.CGraficoCN Grafico = ArmarDatosGrafico(DatosMapa);
              Grafico.CampoOrdenadas = DatosMapa.ColumnaDatos;
              List<CLineaComprimida> LineasPropias = FiltrarDatosInicialesBlock(PagGIS.ProveedorCreador.Columnas,
                    PagGIS.ProveedorCreador.Datos, PagGIS.FiltrosBlocks);
              PagGIS.Capas.Clear();
              PagGIS.CrearPushPinLatLong(Reloj.Indicador, DatosMapa.CodigoElementoDimension,
                    DatosMapa.ColumnaLat, DatosMapa.ColumnaLng, Grafico,
                    double.MinValue, double.MinValue, double.MaxValue, PagGIS.ProveedorCreador.Columnas,
                    LineasPropias, null, false, true, true,
                    (DatosMapa.Clase == CGrafV2DatosContenedorBlock.ClaseBlock.MapaCalor));
              PagGIS.Capas[0].NombreCapa = DatosMapa.Nombre;
              PagGIS.RefrescarGraficos(true, true);
              break;

            case CGrafV2DatosContenedorBlock.ClaseBlock.MapaGradientes:
              Paginas.pgBingWSS Pagina = (Paginas.pgBingWSS)Ventana.frContenedor.Content;
              Pagina.Proveedor.ImponerDataset(Reloj.ProveedorComprimido().Columnas, Reloj.ProveedorComprimido().Datos);
              Pagina.EliminarParesValores();
              Pagina.PonerEnPantalla();
              break;

          }
        }
        else
        {
          Paginas.CDatosGrafLista DatosLista = (from L in mListaDatosGraficosRefresco
                                                where L.Codigo == CodigoMapa
                                                select L).FirstOrDefault();
          if (DatosLista != null)
          {

            Graficos.CGrafV2DatosContenedorBlock Datos = DatosLista.Datos;

            switch (Datos.ClaseElemento)
            {
              case CGrafV2DatosContenedorBlock.ClaseBlock.Grafico:

                frmCrearGraf Graf0 = UbicarVentanaGraf(DatosLista);
                if (Graf0 != null)
                {
                  Paginas.PagTorta pgTorta = (Paginas.PagTorta)Graf0.frGrafico.Content;
                  if (pgTorta != null)
                  {
                    pgTorta.FiltroPasos.AjustarDatosIniciales(Reloj.ProveedorComprimido().Columnas,
                        Reloj.ProveedorComprimido().Datos);
                    pgTorta.RefrescarDatosDesdeProveedor();
                    pgTorta.DibujarGrafico();

                  }
                }
                break;

              //case CGrafV2DatosContenedorBlock.ClaseBlock.Conjunto:
              //  CProveedorComprimido Proveedor = Reloj.ProveedorComprimido();
              //  Proveedor.AjustarDependientes = Reloj.FncRefrescarDatosDataset;
              //  frmFiltroFlotante Filtro = new frmFiltroFlotante();
              //  Filtro.DesdeBlocks = true;
              //  Filtro.Tag = Datos;
              //  IndicadoresV2.Componentes.CFiltrador FiltroProv = Proveedor.FiltroParaColumna(Datos.ColumnaAbscisas);
              //  if (FiltroProv == null)
              //  {
              //    Filtro.Filtro.Filtrador.Columna = Proveedor.ColumnaNombre(Datos.ColumnaAbscisas);
              //    Proveedor.Filtros.Add(Filtro.Filtro.Filtrador);
              //  }
              //  else
              //  {
              //    Filtro.Filtro.Filtrador = FiltroProv;
              //  }
              //  Filtro.Filtro.Proveedor = Proveedor;
              //  Filtro.Reloj = Reloj;
              //  Contenedor.Add(Filtro);

              //  Filtro.Filtro.NoAgrandar = true;

              //  Filtro.Filtro.ModoDeAgrupar = Datos.Agrupacion;

              //  Filtro.Filtro.Filtrador.CrearInformacionFilas();

              //  Filtro.Filtro.DesdeBlock = true;

              //  Filtro.Width = AnchoLocal;
              //  Filtro.Height = AltoLocal;
              //  Filtro.Visibility = CRutinas.PonerVisible(Datos.Visible);
              //  Filtro.Show(Posicion.X, Posicion.Y);

              //  if (Datos.Filtro != null && Datos.Filtro.Grupos.Count > 0)
              //  {
              //    Filtro.Filtro.Filtrador.CantidadNecesaria = Datos.Filtro.Grupos[0].CantidadMinima;
              //    foreach (CCondicionBlock Cnd in Datos.Filtro.Grupos[0].Condiciones)
              //    {
              //      switch (Datos.Filtro.Grupos[0].Condiciones[0].Modo)
              //      {
              //        case ModoFiltrar.Igual:
              //          Filtro.Filtro.ImponerValorSeleccionadoBlock(Cnd.Valor);
              //          break;
              //        case ModoFiltrar.PorRango:
              //          Filtro.Filtro.ImponerRangoBlock(Cnd.Valor, Cnd.ValorMaximo);
              //          break;
              //      }
              //    }
              //  }

              //  Filtro.Filtro.InicializarDesdeBlock();

              //  if (Datos.ColumnaOrdenadas.Length > 0)
              //  {
              //    Filtro.Filtro.ImponerColumnaValor(Proveedor.ColumnaNombre(Datos.ColumnaOrdenadas));
              //  }


              //  break;
            }

          }
        }
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarError(ex);
      }
    }

    public void CompletarCreacionMapa(frmReloj Reloj, Int32 CodigoMapa)
    {
      // Si está refrescando por el timer.
      try
      {

//        ActualizarDatosVentanas(Reloj);qwqw

        Paginas.CDatosMapaLista DatosLista = (from L in mListaDatosMapa
                                              where L.Codigo == CodigoMapa
                                              select L).FirstOrDefault();
        if (DatosLista != null)
        {

          // Crea el elemento a incluir.
          VentanaAguarda Ventana = UbicarVentanaAguarda(DatosLista); // CodigoMapa);
          if (Ventana == null)
          {
            if (DatosLista.Ancho > 0)
            {
              Ventana = CrearVentanaAguardaPosicion(DatosLista, DatosLista.Posicion, DatosLista.Ancho, DatosLista.Alto);
            }
            else
            {
              Ventana = CrearVentanaAguarda(DatosLista);
            }
          }

          Ventana.Tag = DatosLista;

          Ventana.SP.Visibility = Visibility.Collapsed;

          mListaDatosMapa.Remove(DatosLista);
          mListaDatosMapaVentanas.Add(DatosLista);

          switch (DatosLista.Clase)
          {
            case CGrafV2DatosContenedorBlock.ClaseBlock.MapaControl:

              Paginas.PagBing PgBing = new Paginas.PagBing();
              PgBing.CodigoBing = DatosLista.CodigoPropio;
              PgBing.Superior = MainPage.PunteroMainPage;
              Ventana.frContenedor.Content = PgBing;
              PgBing.Redibujar();
              break;

            case CGrafV2DatosContenedorBlock.ClaseBlock.Pines:
            case CGrafV2DatosContenedorBlock.ClaseBlock.MapaCalor:

              Paginas.PagGISDatos PagGIS = new Paginas.PagGISDatos();
              PagGIS.ClaseParaRelacion = ClaseElemento.NoDefinida;
              PagGIS.CodigoParaRelacion = -1;
              PagGIS.ProveedorCreador = Reloj.ProveedorComprimido();
              PagGIS.Indicador = Reloj.Indicador;
              PagGIS.NombreColumnaGIS = "";
              PagGIS.Superior = MainPage.PunteroMainPage;
              PagGIS.Vinculo = CRutinas.CrearVinculoIndicadorCompleto();
              PagGIS.MimicoTraza = null;
              PagGIS.FiltrosBlocks = (DatosLista.UsaFiltroPropio ?
                ExtraerFiltros(DatosLista.FiltrosBlock, PagGIS.ProveedorCreador.Columnas) : null);
              PagGIS.DesdeBlocks = true;
              WCFBPI.CGraficoCN Grafico = ArmarDatosGrafico(DatosLista);
              Grafico.CampoOrdenadas = DatosLista.ColumnaDatos;
              List<CLineaComprimida> LineasPropias = FiltrarDatosInicialesBlock(PagGIS.ProveedorCreador.Columnas,
                    PagGIS.ProveedorCreador.Datos, PagGIS.FiltrosBlocks);
              PagGIS.CrearPushPinLatLong(Reloj.Indicador, DatosLista.CodigoElementoDimension,
                    DatosLista.ColumnaLat, DatosLista.ColumnaLng, Grafico,
                    double.MinValue, double.MinValue, double.MaxValue, PagGIS.ProveedorCreador.Columnas,
                    LineasPropias, null, false, true, true,
                    (DatosLista.Clase == CGrafV2DatosContenedorBlock.ClaseBlock.MapaCalor));
              PagGIS.Capas[0].NombreCapa = DatosLista.Nombre;
              PagGIS.ProyectoBing.AgregarCapasWFS(DatosLista.CapasWFS);
              Ventana.frContenedor.Content = PagGIS;
              break;

            case CGrafV2DatosContenedorBlock.ClaseBlock.MapaGradientes:
              Paginas.pgBingWSS Pagina = new Paginas.pgBingWSS();
              Pagina.CodigoCapaImpuesto = DatosLista.CodigoPropio;
              Pagina.DatosGuardados = DatosLista;
              Pagina.ClaseParaRelacion = DatosLista.ClaseIndicador; // ClaseElemento.NoDefinida;
              Pagina.CodigoParaRelacion = DatosLista.CodigoPropio; // -1;
              Pagina.Indicador = (Reloj == null ? null : Reloj.Indicador);
              Pagina.Proveedor = new Datos.CProveedorComprimido(ClaseElemento.NoDefinida, -1);
              switch (DatosLista.ClaseIndicador)
              {
                case ClaseElemento.Indicador:
                  Pagina.Proveedor.ImponerDataset(Reloj.ProveedorComprimido().Columnas, Reloj.ProveedorComprimido().Datos);
                  Pagina.Superior = MainPage.PunteroMainPage;
                  Pagina.Reloj = Reloj;
                  Ventana.frContenedor.Content = Pagina;
                  break;
                default:
                  MessageBox.Show("Únicamente soporta mapas gradiente desde indicador");
                  return;
              }
              break;

          }

        }

        if (AlCompletarPedido != null)
        {
          AlCompletarPedido();
        }

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarError("Al completar mapa ", ex);
      }

    }

    public void CompletarCreacionOtro(frmReloj Reloj, Int32 CodigoOtro)
    {
      try
      {
        Paginas.CDatosOtroLista DatosLista = (from L in mListaDatosOtros
                                              where L.Codigo == CodigoOtro
                                              select L).FirstOrDefault();
        if (DatosLista != null)
        {

          // Crea el elemento a incluir.
          VentanaAguarda Ventana = UbicarVentanaAguarda(DatosLista); // CodigoOtro);
          if (Ventana == null)
          {
            Ventana = (DatosLista.Ancho < 0 ?
              CrearVentanaAguarda(DatosLista) :
              CrearVentanaAguardaPosicion(DatosLista, DatosLista.Posicion, DatosLista.Ancho, DatosLista.Alto));
          }

          mListaDatosOtros.Remove(DatosLista);

          // a diferencia de los otros, en los que agrega una page al frame, aca agrega la grilla al floatwindow.

          Grilla.frmGrilla Grilla = new Grilla.frmGrilla();
          Grilla.Tag = DatosLista;
          Grilla.Reloj = Reloj;
          Grilla.CrearDatosDesdeReloj(Reloj);
          Contenedor.Add(Grilla);
          Grilla.Width = Ventana.Width;
          Grilla.Height = Ventana.Height;
          Grilla.Show(Ventana.Position);

          Contenedor.Remove(Ventana);

        }


        if (AlCompletarPedido != null)
        {
          AlCompletarPedido();
        }

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarError("Al crear otro", ex);
      }

    }

    public static Point ObtenerPosicion(XElement Elemento, out double Ancho, out double Alto)
    {
      double Absc=CRutinas.StrVFloat(Elemento.Attribute(CRutinas.CTE_ABSCISA).Value);
      double Ord = CRutinas.StrVFloat(Elemento.Attribute(CRutinas.CTE_ORDENADA).Value);
      Ancho = CRutinas.StrVFloat(Elemento.Attribute(CRutinas.CTE_ANCHO).Value);
      Alto = CRutinas.StrVFloat(Elemento.Attribute(CRutinas.CTE_ALTO).Value);
      return new Point(Absc, Ord);
    }

    private void CargarIndicador(XElement Elemento)
    {

      Int32 CodigoIndicador=Int32.Parse( Elemento.Attribute(CRutinas.CTE_CODIGO).Value);
      Int32 CodigoElementoDimension = Int32.Parse(Elemento.Attribute(CRutinas.CTE_ELEMENTO_DIMENSION).Value);

      double Ancho;
      double Alto;
      Point Posicion = ObtenerPosicion(Elemento, out Ancho, out Alto);

      CDatoIndicador Datos=CCcontenedorDatos.ContenedorUnico.IndicadorDesdeCodigo(CodigoIndicador);

      if (Datos != null)
      {
        System.Xml.Linq.XAttribute Atr = Elemento.Attribute(CRutinas.CTE_VISIBLE);
        bool Visible = (Atr == null ? true : (Atr.Value != "N"));
        AgregarIndicador(Datos, CodigoElementoDimension, Visible, Posicion.X, Posicion.Y);
      }

    }

    private void CargarTendencia(XElement Elemento)
    {

      Int32 CodigoIndicador = Int32.Parse(Elemento.Attribute(CRutinas.CTE_CODIGO).Value);
      Int32 CodigoElementoDimension = Int32.Parse(Elemento.Attribute(CRutinas.CTE_ELEMENTO_DIMENSION).Value);

      double Ancho;
      double Alto;
      Point Posicion = ObtenerPosicion(Elemento, out Ancho, out Alto);

      CDatoIndicador Datos = CCcontenedorDatos.ContenedorUnico.IndicadorDesdeCodigo(CodigoIndicador);

      if (Datos != null)
      {
        XAttribute Atr = Elemento.Attribute(CRutinas.CTE_CONDICIONES);
        string Adicionales = (Atr == null ? "" : Elemento.Attribute(CRutinas.CTE_CONDICIONES).Value);

        Atr = Elemento.Attribute(CRutinas.CTE_VISIBLE);
        bool Visible = (Atr == null ? true : (Atr.Value != "N"));
        AgregarTendencia(Datos, CodigoElementoDimension, Posicion.X, Posicion.Y, Ancho, Alto, Adicionales);
      }

    }

    private void CargarFicha(XElement Elemento)
    {

      Int32 CodigoFicha = Int32.Parse(Elemento.Attribute(CRutinas.CTE_CODIGO).Value);

      CPreguntaCN DatosFicha = CCcontenedorDatos.ContenedorUnico.PreguntaDesdeCodigo(CodigoFicha);
      if (DatosFicha != null)
      {
        double Ancho;
        double Alto;
        Point Posicion = ObtenerPosicion(Elemento, out Ancho, out Alto);
        AgregarFicha(DatosFicha, Posicion.X, Posicion.Y, Ancho, Alto);
      }

    }

    private void CargarGrafico(XElement Elemento)
    {

      Graficos.CGrafV2DatosContenedorBlock Graf = new CGrafV2DatosContenedorBlock();

      Graf.ClaseElemento = (CGrafV2DatosContenedorBlock.ClaseBlock)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CLASE_0);
      Graf.Clase = (ClaseGrafico)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CLASE);

      Graf.Nombre = Elemento.Attribute(CRutinas.CTE_NOMBRE).Value;

      double Ancho;
      double Alto;
      Graf.Posicion = ObtenerPosicion(Elemento, out Ancho, out Alto);
      Graf.Ancho = Ancho;
      Graf.Alto = Alto;

      Graf.Indicador = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_INDICADOR);
      Graf.CodigoElementoDimension = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_ELEMENTO_DIMENSION);

      Graf.AgrupIndep = (ModoAgruparIndependiente)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_AGRUPACION_IND);
      Graf.Agrupacion = (ModoAgruparDependiente)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_AGRUPACION_DEP);

      Graf.ColumnaAbscisas = Elemento.Attribute(CRutinas.CTE_COLUMNA_ABSC).Value;
      Graf.ColumnaOrdenadas = Elemento.Attribute(CRutinas.CTE_COLUMNA_ORD).Value;
      Graf.ColumnaSexo = Elemento.Attribute(CRutinas.CTE_COLUMNA_SEXO).Value;

      Graf.SaltoHistograma = CRutinas.StrVFloat(Elemento.Attribute(CRutinas.CTE_SALTO_HISTOGRAMA).Value);

      Graf.Visible = CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_VISIBLE);
      Graf.UsaFiltroPropio = CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_FILTRO_PROPIO);

      XElement ElFiltro = Elemento.Element(CRutinas.CTE_FILTRO);
      if (ElFiltro != null)
      {
        Graf.Filtro = new CPasoCondicionesBlock();
        Graf.Filtro.CargarDesdeXML(ElFiltro);
      }

      ElFiltro = Elemento.Element(CRutinas.CTE_FILTROS);
      if (ElFiltro != null)
      {
        XElement Filtros = new XElement(CRutinas.CTE_FILTROS);
        foreach (XElement ElFiltroLocal in ElFiltro.Elements(CRutinas.CTE_FILTRO))
        {
          CPasoCondicionesBlock FLocal = new CPasoCondicionesBlock();
          FLocal.CargarDesdeXML(ElFiltroLocal);
          Graf.FiltrosBlock.Add(FLocal);
        }
      }

      switch (Graf.ClaseElemento)
      {
        case CGrafV2DatosContenedorBlock.ClaseBlock.Conjunto:
          AgregarConjunto(new Paginas.CDatosGrafLista(Graf));
          break;
        default:
          AgregarGrafico(new Paginas.CDatosGrafLista(Graf));
          break;
      }

    }

    private void CargarMapa(XElement Elemento)
    {

      Paginas.CDatosMapaLista Datos = new Paginas.CDatosMapaLista();
      Datos.CargarDesdeXML(Elemento);

      AgregarMapa(Datos);

    }

    private void CargarOtroLista(XElement Elemento)
    {

      try
      {

        Paginas.CDatosOtroLista Datos = new Paginas.CDatosOtroLista();
        Datos.CargarDesdeXML(Elemento);

        switch (Datos.Clase)
        {
          case CGrafV2DatosContenedorBlock.ClaseBlock.Grilla:
            AgregarGrilla(Datos);
            break;
          case CGrafV2DatosContenedorBlock.ClaseBlock.Mimico:
            AgregarMimico(Datos);
            break;
          case CGrafV2DatosContenedorBlock.ClaseBlock.Consulta:
            AgregarSubConsulta(Datos);
            break;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Al cargar otro desde lista");
        throw ex;
      }

    }

    public bool ACargar { get; set; }

    public void CargarXML(string XML)
    {
      try
      {
//        Contenedor.CloseAllWindows();
        XDocument Documento = XDocument.Parse(XML);
        XElement Inicio = Documento.Element(CRutinas.CTE_INICIO);
        // Mantener una lista de los datos cargados.
        foreach (XElement Elemento in Inicio.Elements(CRutinas.CTE_ELEMENTO))
        {
          XAttribute AtrClase = Elemento.Attribute(CRutinas.CTE_CLASE_0);
          switch ((Graficos.CGrafV2DatosContenedorBlock.ClaseBlock)Int32.Parse(AtrClase.Value))
          {
            case CGrafV2DatosContenedorBlock.ClaseBlock.Indicador:
              CargarIndicador(Elemento);
              break;
            case CGrafV2DatosContenedorBlock.ClaseBlock.Tendencia:
              CargarTendencia(Elemento);
              break;
            case CGrafV2DatosContenedorBlock.ClaseBlock.Grafico:
            case CGrafV2DatosContenedorBlock.ClaseBlock.Conjunto:
              CargarGrafico(Elemento);
              break;
            case CGrafV2DatosContenedorBlock.ClaseBlock.Grilla:
            case CGrafV2DatosContenedorBlock.ClaseBlock.Mimico:
            case CGrafV2DatosContenedorBlock.ClaseBlock.Consulta:
              CargarOtroLista(Elemento);
              break;
            case CGrafV2DatosContenedorBlock.ClaseBlock.MapaCalor:
            case CGrafV2DatosContenedorBlock.ClaseBlock.Pines:
            case CGrafV2DatosContenedorBlock.ClaseBlock.MapaControl:
            case CGrafV2DatosContenedorBlock.ClaseBlock.MapaGradientes:
              CargarMapa(Elemento);
              break;
            case CGrafV2DatosContenedorBlock.ClaseBlock.Ficha:
              CargarFicha(Elemento);
              break;

          }
        }

        ACargar = false;
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarError("Al crear desde XML", ex);
      }
    }

    public string ObtenerXML()
    {
      XElement Inicio=new XElement(CRutinas.CTE_INICIO);
      XDocument Documento = new XDocument(Inicio);
      // Mantener una lista de los datos cargados.
      foreach (FloatingWindow Ventana in Contenedor.FloatingWindows)
      {
        Graficos.CGrafV2DatosContenedorBlock DatosGraf = (Ventana.Tag as Graficos.CGrafV2DatosContenedorBlock);
        if (DatosGraf != null)
        {
          if (DatosGraf.ClaseElemento == CGrafV2DatosContenedorBlock.ClaseBlock.Conjunto)
          {
            frmFiltroFlotante Filtro = Ventana as frmFiltroFlotante;
            DatosGraf = Filtro.Filtro.ObtenerDatosBlock();
          }
          else
          {
            frmCrearGraf Graf = Ventana as frmCrearGraf;
            DatosGraf = Graf.DatosGraficoBlock(true);
          }
          DatosGraf.AgregarAXML(Inicio, Ventana);
        }
        else
        {
          Paginas.CDatosMapaLista DatosMapa = (Ventana.Tag as Paginas.CDatosMapaLista);
          if (DatosMapa != null)
          {
            DatosMapa.AgregarAXML(Inicio, Ventana);
          }
          else
          {
            CPreguntaCN Pregunta = (Ventana.Tag as CPreguntaCN);
            if (Pregunta != null)
            {
              AgregarPreguntaAXML(Inicio, Pregunta, Ventana);
            }
            else
            {
              Paginas.CDatosOtroLista OtrosLista = (Ventana.Tag as Paginas.CDatosOtroLista);
              if (OtrosLista != null)
              {
                OtrosLista.AgregarAXML(Inicio, Ventana);
              }
              else
              {
                Graficos.frmReloj Reloj = (Ventana as Graficos.frmReloj);
                if (Reloj != null)
                {
                  if (Reloj.Visibility == Visibility.Visible)
                  {
                    AgregarRelojAXML(Inicio, Reloj);
                  }
                }
                else
                {
                  Graficos.frmTendencia2 Tendencia = (Ventana as Graficos.frmTendencia2);
                  if (Tendencia != null)
                  {
                    AgregarTendenciaAXML(Inicio, Tendencia);
                  }
                }
              }
            }
          }
        }
      }

      System.Text.StringBuilder Escritor = new System.Text.StringBuilder();
      Escritor.Append(Documento);
      return Escritor.ToString();

    }

    private void AgregarPreguntaAXML(XElement Inicio, CPreguntaCN Pregunta, FloatingWindow Ventana)
    {
      XElement Contenido = new XElement(CRutinas.CTE_ELEMENTO);

      CRutinas.AgregarAtributo(Contenido,CRutinas.CTE_CLASE_0,((Int32)Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Ficha).ToString());
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CODIGO, Pregunta.Codigo.ToString());

      CRutinas.AgregarAtributosPosicion(Contenido, Ventana.Position.X, Ventana.Position.Y,
          Ventana.Width, Ventana.Height);

      //CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CODIGO_2, Pregunta.Dimension.ToString());
      //CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_ELEMENTO_DIMENSION, Pregunta.ElementoDimension.ToString());

      Inicio.Add(Contenido);

    }

    private void AgregarRelojAXML(XElement Inicio, Graficos.frmReloj Ventana)
    {
      XElement Contenido = new XElement(CRutinas.CTE_ELEMENTO);

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CLASE_0, ((Int32)Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Indicador).ToString());

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_NOMBRE, Ventana.Indicador.Descripcion);
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CODIGO, Ventana.Indicador.Codigo.ToString());

      CRutinas.AgregarAtributosPosicion(Contenido, Ventana.Position.X, Ventana.Position.Y,
          Ventana.Width, Ventana.Height);

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CODIGO_2, Ventana.Indicador.Dimension.ToString());
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_ELEMENTO_DIMENSION, Ventana.CodigoElementoDimension.ToString());

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_VISIBLE, (Ventana.Visibility == Visibility.Visible ? "S" : "N"));

      Inicio.Add(Contenido);

    }

    private void AgregarTendenciaAXML(XElement Inicio, Graficos.frmTendencia2 Ventana)
    {
      XElement Contenido = new XElement(CRutinas.CTE_ELEMENTO);

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CLASE_0, ((Int32)Graficos.CGrafV2DatosContenedorBlock.ClaseBlock.Tendencia).ToString());

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_NOMBRE, Ventana.Indicador.Descripcion);
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CODIGO, Ventana.Indicador.Codigo.ToString());

      CRutinas.AgregarAtributosPosicion(Contenido, Ventana.Position.X, Ventana.Position.Y,
          Ventana.Width, Ventana.Height);

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CODIGO_2, Ventana.Indicador.Dimension.ToString());
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_ELEMENTO_DIMENSION, Ventana.RelojCreador.CodigoElementoDimension.ToString());

      // Agrega otros indicadores.
      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_CONDICIONES, Ventana.ExtraerIndicadoresAsociados());

      CRutinas.AgregarAtributo(Contenido, CRutinas.CTE_VISIBLE, (Ventana.Visibility == Visibility.Visible ? "S" : "N"));

      Inicio.Add(Contenido);

    }

    private double PorcentajeInterseccion(Rect Rect0, Rect RectBase)
    {
      return Math.Max(0,
        Math.Max(0, (Math.Min(Rect0.X + Rect0.Width, RectBase.X + RectBase.Width) - Math.Max(Rect0.X, RectBase.X))) *
        Math.Max(0, (Math.Min(Rect0.Y + Rect0.Height, RectBase.Y + RectBase.Height) - Math.Max(Rect0.Y, RectBase.Y))) /
        (RectBase.Width * RectBase.Height));
    }

    private double DeterminarPorcentaje(FloatingWindowHost Host, Rect Rectangulo)
    {
      double Suma = 0;
      foreach (FloatingWindow Ventana in Host.FloatingWindows)
      {
        if (Ventana.Visibility == Visibility.Visible)
        {
          Suma += PorcentajeInterseccion(new Rect()
          {
            X = Ventana.Position.X,
            Y = Ventana.Position.Y,
            Width = CRutinas.Ancho(Ventana),
            Height = CRutinas.Alto(Ventana)
          }, Rectangulo);
        }
      }
      return Suma;
    }

    private double AcumularOcupado(Int32 Columna, Int32 Fila, double[,] Acumulados)
    {
      double Acumulado = 0;
      for (Int32 Columna0 = Columna; Columna0 < Columna + 3; Columna0++)
      {
        for (Int32 Fila0 = Fila; Fila0 < Fila + 2; Fila0++)
        {
          Acumulado += Acumulados[Fila0, Columna0];
        }
      }
      return Acumulado;
    }

    private double AnchoHost
    {
      get { return CRutinas.AnchoContenedor(Contenedor.CanvasBase); }
    }

    private double AltoHost
    {
      get { return CRutinas.AltoContenedor(Contenedor.CanvasBase); }
    }

    private const double BANDA = 2;
    public Point UbicarPosicionMasUsable(FloatingWindowHost Host)
    {
      // rectangulos (9x4). Se requieren 3x2.
      double[,] PorcentajesOcupados = new double[4,9];

      double AnchoTotal = AnchoHost;
      double AltoTotal = AltoHost;

      Point Respuesta = new Point(-1000, -1000);

      if (AnchoTotal <= 0 || AltoTotal <= 0)
      {
        return Respuesta;
      }

      for (Int32 Columna = 0; Columna < 9; Columna++)
      {
        for (Int32 Fila = 0; Fila < 4; Fila++)
        {
          Rect Rectangulo = new Rect()
          {
            X = AnchoTotal * Columna / 9,
            Y = AltoTotal * Fila / 4,
            Width = AnchoTotal / 9,
            Height = AltoTotal / 4
          };
          PorcentajesOcupados[Fila,Columna]=DeterminarPorcentaje(Host, Rectangulo);
        }
      }

      // Ahora busca el mas bajo.
      double Minimo = double.MaxValue;
      for (Int32 Fila = 0; Fila < 3; Fila++)
      {
        for (Int32 Columna = 0; Columna < 7; Columna++)
        {
          double Local = AcumularOcupado(Columna, Fila, PorcentajesOcupados);
          if (Local < (Minimo - 0.01))
          {
            Minimo = Local;
            Respuesta = new Point(BANDA + AnchoTotal * Columna / 9, BANDA + Fila * AltoTotal / 4);
          }
        }
      }

      return Respuesta;

    }


    public void ActualizarDatosVentanas(frmReloj Reloj)
    {

      CProveedorComprimido Prov = Reloj.ProveedorComprimido();
      if (Prov != null)
      {
        Reloj.ColumnasDataset = Reloj.ProveedorComprimido().Columnas;
        if (Reloj.ColumnasDataset == null)
        {
          return;
        }
      }
      else
      {
        return;
      }

      foreach (UIElement Ventana in Contenedor.HostPanel.Children)
      {
        if (Ventana is Grilla.frmGrilla)
        {
          Grilla.frmGrilla GrillaDatos = (Grilla.frmGrilla)Ventana;
          if (GrillaDatos.Reloj == Reloj)
          {
            GrillaDatos.CrearDatosDesdeReloj(Reloj);
          }
        }
        if (Ventana is Graficos.frmCrearGraf)
        {
          ((Graficos.frmCrearGraf)Ventana).ActualizarDatosGrafico(true);
        }
        if (Ventana is IndicadoresV2.Graficos.frmTendencia2)
        {
          frmTendencia2 Tendencia = (frmTendencia2)Ventana;
          if (Tendencia.RelojCreador == Reloj)
          {
            Reloj.FiltrarBlocksDatosPorAsociacion(Reloj.ProveedorComprimido());
            Tendencia.RedibujarLineaFiltrada(Reloj.BlocksDatos);
          }
        }
      }

    }

    //public override void OnApplyTemplate()
    //{
    //  base.OnApplyTemplate();
    //  Contenedor.HostPanel.Loaded += HostPanel_Loaded;
    //}

    //void HostPanel_Loaded(object sender, RoutedEventArgs e)
    //{
    //  if (ACargar)
    //  {
    //    if (Solapa != null)
    //    {
    //      CargarXML(Solapa.Block);
    //    }
    //    else
    //    {
    //      CargarXML(Pregunta.Block);
    //    }
    //    Contenedor.HostPanel.Loaded -= HostPanel_Loaded;
    //  }
    //}

    private void Contenedor_Loaded(object sender, RoutedEventArgs e)
    {
      if (Contenedor.HostPanel == null)
      {
        Contenedor.FuncionLoadHostPanel = FncAgregarEventoLoaded;
      }
      else
      {
        FncAgregarEventoLoaded(Contenedor);
      }
    }

    private void FncAgregarEventoLoaded(FloatingWindowHost Host)
    {
      if (ACargar)
      {
        if (Solapa != null)
        {
          CargarXML(Solapa.Block);
        }
        else
        {
          CargarXML(Pregunta.Block);
        }
//        Contenedor.HostPanel.Loaded -= HostPanel_Loaded;
      }
    }

  }
}
