using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.Model;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaMapaCalor : ComponentBase, IDisposable
  {

    [Inject]
    public IJSRuntime JSRuntime { get; set; }

    private Int32 mPosicionMapaCalor = -1;

    public CLogicaMapaCalor()
		{
      mPosicionMapaCalor = -1;
		}

    [Parameter]
    public Plantillas.CLinkContenedorFiltros Filtros { get; set; }
    public async void Dispose()
    {
      object[] Args = new object[1];
      Args[0] = mPosicionMapaCalor;
      try
      {
        await JSRuntime.InvokeAsync<string>("LiberarMap", Args);
        //       await JSRuntime.InvokeAsync<string>("LiberarMapaCalor", Args);
      }
      catch (Exception)
      {
        // Ignora lo que puede ocurrir porque al cerrar puede dar una excepcion.
      }
    }

		protected override Task OnInitializedAsync()
		{
      mCodigoMapa = gCodigoMapa++;
      Direccion = "MapaCalor" + mCodigoMapa.ToString();
			return base.OnInitializedAsync();
		}

		public static Int32 gCodigoMapa = 0;
    private Int32 mCodigoMapa;

    [Parameter]
    public CDatoIndicador Indicador { get; set; }

    [Parameter]
    public Int32 CodigoElementoDimension { get; set; }

    [Parameter]
    public Int32 CodigoCapa { get; set; }

    [Parameter]
    public string Direccion { get; set; }

    [Parameter]
    public double Abscisa { get; set; } = -999;

    [Parameter]
    public double Ordenada { get; set; } = -999;

    [Parameter]
    public double Ancho { get; set; } = -999;

    [Parameter]
    public double Alto { get; set; } = -999;

    [Parameter]
    public string ColumnaLat { get; set; }

    [Parameter]
    public string ColumnaLng { get; set; }

    [Parameter]
    public string ColumnaValor { get; set; }

    [Parameter]
    public Int32 NivelFlotante { get; set; }

    [Parameter]
    public Plantillas.CLinkMapa Datos { get; set; }

    public bool DatosCompletos { get; set; } = false;

    public bool SinDatos { get; set; } = false;

    public string Estilo
    {
      get
      {
        return "width: " + Math.Floor(Ancho).ToString() + "px; left: 0px; top: 0px; height: " +
            Math.Floor(Alto - 0).ToString() +
            "px; overflow: hidden; position: absolute;";
      }
    }

    public string EstiloAguarda
    {
      get
      {
        return "width: " + Math.Floor(Ancho).ToString() + "px; left: 0px; top: 0px; height: " +
            Math.Floor(Alto - 0).ToString() +
            "px; overflow: hidden; background-color: #BEC9E7; position: absolute; text-align: center;";
      }
    }

    //public string NombreReferencias
    //{
    //  get
    //  {
    //    return "MAPA_CALOR_" + CodigoCapa.ToString();
    //  }
    //}

    private CProveedorComprimido mProveedor = null;
    public CProveedorComprimido Proveedor
    {
      get { return mProveedor; }
      set
      {
        if (mProveedor != value)
        {
          if (mProveedor != null)
          {
            mProveedor.AlAjustarDependientes -= FncAjustarVentana;
          }
          mProveedor = value;
          if (mProveedor != null)
          {
            mProveedor.AlAjustarDependientes += FncAjustarVentana;
          }
        }
      }
    }

    public List<CColumnaBase> ColumnasDataset { get { return Proveedor.Columnas; } }

    public List<CLineaComprimida> DatosSinFiltroPropio { get; set; } = null;

    public List<CLineaComprimida> DatosFiltrados { get; set; } = null;

    private void FncAjustarVentana(object sender)
    {
      StateHasChanged();
    }

    private HttpClient mHttp = null;
    [Inject]
    public HttpClient Http
    {
      get { return mHttp; }
      set
      {
        if (value != mHttp)
        {
          mHttp = value;
        }
      }
    }

    private async Task LeerDatosNecesariosAsync()
    {
      try
      {
        if (Proveedor == null && Indicador == null)
        {
          SinDatos = true;
          return;
        }

        // leer datos indicador.
        if (Proveedor == null && Indicador != null)
        {
          RespuestaDatasetBin Respuesta = await Contenedores.CContenedorDatos.LeerDetalleDatasetAsync(
              Http, Indicador.Codigo, Indicador.Dimension, CodigoElementoDimension,-1);
          if (!Respuesta.RespuestaOK)
					{
            throw new Exception(Respuesta.MsgErr);
					}
          byte[] Datos = Respuesta.Datos;
          if (Datos.Length == 0)
          {
            SinDatos = true;
            return;
          }
          else
          {
            Proveedor = new CProveedorComprimido(ClaseElemento.NoDefinida, -1);
            Proveedor.ProcesarDatasetBinario(Datos, false);
            DatosFiltrados = Proveedor.Datos;
          }
        }

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    private bool mbReubicarCentro = false;
    public bool ReubicarCentro
    {
      get { return mbReubicarCentro; }
      set
      {
        if (mbReubicarCentro != value)
        {
          mbReubicarCentro = value;
          if (mbReubicarCentro)
          {
            StateHasChanged();
          }
        }
      }
    }

    private List<CPuntoCalor> mPuntosCalor = null;

    private void AjustarPuntosCalor()
    {
      mPuntosCalor = new List<CPuntoCalor>();
      Int32 PosColLat = Proveedor.ColumnaNombre(ColumnaLat).Orden;
      Int32 PosColLng = Proveedor.ColumnaNombre(ColumnaLng).Orden;
      Int32 PosColValor = (ColumnaValor.Length > 0 ? Proveedor.ColumnaNombre(ColumnaValor).Orden : -1);
      foreach (CLineaComprimida Linea in DatosFiltrados)
      {
        if (Linea.Vigente)
        {
          double Lat = Proveedor.ExtraerDatoReal(Linea, PosColLat);
          double Long = Proveedor.ExtraerDatoReal(Linea, PosColLng);
          float Valor = (PosColValor >= 0 ? (float)Proveedor.ExtraerDatoReal(Linea, PosColValor) : 1);
          mPuntosCalor.Add(new CPuntoCalor(Long, Lat, Valor));
        }
      }
    }

    [Parameter]
    public Plantillas.CGrafV2DatosContenedorBlock.ClaseBlock Clase { get; set; }

    [Parameter]
    public ClaseElemento ClaseOrigen { get; set; }

    private CRespuestaCalor mRespuestaCalor = null;

    private double mLatMin, mLatMax;
    private double mLngMin, mLngMax;

    public static double DeterminarDiscretizacion(double Ancho, double Alto, double Elementos)
    {
      return Math.Sqrt((Ancho * Alto) / Elementos);
    }

    [CascadingParameter]
    public Logicas.CLogicaSalaReunion PaginaSala { get; set; }

    private List<CPuntoTextoColor> mPushpins = null;

    private static string ObtenerColor(double Valor, double Minimo, double Satisfactorio, double Sobresaliente)
    {
      if (Minimo == Sobresaliente)
      {
        return "lightgray";
      }
      else
      {
        if (Sobresaliente > Minimo)
        {
          if (Valor < Minimo)
          {
            return "red";
          }
          else
          {
            if (Valor < Satisfactorio)
            {
              return "yellow";
            }
            else
            {
              return (Valor < Sobresaliente ? "green" : "blue");
            }
          }
        }
        else
        {
          if (Valor < Minimo)
          {
            return "blue";
          }
          else
          {
            if (Valor < Satisfactorio)
            {
              return "green";
            }
            else
						{
              return (Valor < Sobresaliente ? "yellow" : "red");
						}
          }
        }
      }
    }

    private bool ColorMasCritico(string C1, string C2)
		{
      if (C2 == "lightgray")
			{
        return C1 != "lightgray";
			}
      if (C1=="red")
			{
        return C2 != "red";
			}
      else
			{
        if (C1 == "yellow")
				{
          return C2 == "green" || C2 == "blue";
				}
        else
				{
          if (C1 == "green")
					{
            return C2 == "blue";
					}
          else
					{
            return false;
					}
				}
			}
		}

    private string AgruparTextos(string T1, string T2)
    {
      try
      {
        Int32 Pos = T1.IndexOf("<");
        Int32 Cantidad = 1;
        double R1;
        if (Pos > 0)
        {
          R1 = CRutinas.StrVFloatNaN(T1.Substring(0, Pos - 1));
          Cantidad = Int32.Parse(T1.Substring(Pos + 1, T1.Length - Pos - 2));
        }
        else
        {
          R1 = CRutinas.StrVFloatNaN(T1);
        }
        double R2 = CRutinas.StrVFloatNaN(T2);
        Cantidad++;
        if (!double.IsNaN(R1) && !double.IsNaN(R2))
        {
          return (R1 + R2).ToString() + " <" + Cantidad.ToString() + ">";
        }
        else {
          if (Pos > 0)
          {
            return T1.Substring(0, Pos + 1) + Cantidad.ToString() + ">";
          }
          else
					{
            return T1.Trim() + " <" + Cantidad.ToString() + ">";
					}
        }
      }
      catch (Exception)
      {
        return T1;
      }
    }

    private async void CrearPines()
    {

      double Minimo = 0;
      double Satisfactorio = 0;
      double Sobresaliente = 0;

      // Buscar los datos del indicador.
      CInformacionAlarmaCN Alar = await Contenedores.CContenedorDatos.DatosAlarmaIndicadorAsync(Http, Indicador.Codigo,
            Indicador.Dimension, CodigoElementoDimension);
      if (Alar != null)
      {
        Minimo = Alar.Minimo;
        Satisfactorio = Alar.Satisfactorio;
        Sobresaliente = Alar.Sobresaliente;
      }

      mPushpins = new List<CPuntoTextoColor>();
      Int32 PosColLat = Proveedor.ColumnaNombre(ColumnaLat).Orden;
      Int32 PosColLng = Proveedor.ColumnaNombre(ColumnaLng).Orden;
      Int32 PosColValor = (ColumnaValor.Length > 0 ? Proveedor.ColumnaNombre(ColumnaValor).Orden : -1);
      foreach (CLineaComprimida Linea in DatosFiltrados)
      {
        if (Linea.Vigente)
        {
          double Lat = Proveedor.ExtraerDatoReal(Linea, PosColLat);
          double Long = Proveedor.ExtraerDatoReal(Linea, PosColLng);
          float Valor = (PosColValor >= 0 ? (float)Proveedor.ExtraerDatoReal(Linea, PosColValor) : 1);
          mPushpins.Add(new CPuntoTextoColor()
          {
            Abscisa = Long,
            Ordenada = Lat,
            Color = ObtenerColor(Valor, Minimo, Satisfactorio, Sobresaliente),
            Texto = Valor.ToString()
          });
        }
      }

      mLatMin = (from P in mPushpins
                 select P.Ordenada).Min();
      mLatMax = (from P in mPushpins
                 select P.Ordenada).Max();
      mLngMin = (from P in mPushpins
                 select P.Abscisa).Min();
      mLngMax = (from P in mPushpins
                 select P.Abscisa).Max();

      // Unificar.
      mPushpins = (from P in mPushpins
                   orderby P.Abscisa, P.Ordenada
                   select P).ToList();

      List<CPuntoTextoColor> Comprimidos = new List<CPuntoTextoColor>();
      double AbscAnt = -1000;
      double OrdAnt = -1000;
      foreach (CPuntoTextoColor Punto in mPushpins)
			{
        if (Punto.Abscisa == AbscAnt && Punto.Ordenada == OrdAnt)
        {
          CPuntoTextoColor PuntoRefe = Comprimidos.Last();
          if (ColorMasCritico(Punto.Color, PuntoRefe.Color))
          {
            PuntoRefe.Color = Punto.Color;
          }
          try
          {
            PuntoRefe.Texto = AgruparTextos(PuntoRefe.Texto, Punto.Texto);
          }
          catch (Exception)
          {
            //
          }
        }
        else
        {
          AbscAnt = Punto.Abscisa;
          OrdAnt = Punto.Ordenada;
          Comprimidos.Add(Punto);
        }
			}

      mPushpins = Comprimidos;

    }

    private CMapaCalorRN mCreador;

    private void CrearMapaCalor()
    {

      if (mPuntosCalor == null)
      {
        AjustarPuntosCalor();
      }

      mLatMin = (from P in mPuntosCalor
                 select P.Ordenada).Min();
      mLatMax = (from P in mPuntosCalor
                 select P.Ordenada).Max();
      mLngMin = (from P in mPuntosCalor
                 select P.Abscisa).Min();
      mLngMax = (from P in mPuntosCalor
                 select P.Abscisa).Max();


      mCreador = new CMapaCalorRN();
      mCreador.Puntos = mPuntosCalor;
      if (mRespuestaCalor == null)
      {
        mRespuestaCalor = new CRespuestaCalor();
        mRespuestaCalor.Aplanado = false;
        mRespuestaCalor.Empuntado = false;
        mRespuestaCalor.Acumulado = true;
        mRespuestaCalor.FactorDistancia = 1;
      }
      mRespuestaCalor.PixelsAncho = mLngMax - mLngMin;
      mRespuestaCalor.PixelsAlto = mLatMax - mLatMin;
      if (mRespuestaCalor.PixelsAncho < 0.00001)
      {
        mRespuestaCalor.Valores.Clear();
        return;
      }
      if (mRespuestaCalor.SegmentosHCfg < 0)
      {
        double DimElemento = DeterminarDiscretizacion(
              mRespuestaCalor.PixelsAncho, mRespuestaCalor.PixelsAlto, 5000);
        mRespuestaCalor.SegmentosHCfg = (Int32)Math.Floor(mRespuestaCalor.PixelsAncho / DimElemento);
        mRespuestaCalor.SegmentosVCfg = (Int32)Math.Floor(mRespuestaCalor.PixelsAlto / DimElemento);
      }

      mRespuestaCalor.AbscisaMinima = mLngMin;
      mRespuestaCalor.AbscisaMaxima = mLngMax;
      mRespuestaCalor.OrdenadaMinima = mLatMin;
      mRespuestaCalor.OrdenadaMaxima = mLatMax;

      mCreador.Respuesta = mRespuestaCalor;
      string Msg = mCreador.DeterminarMapa();
      if (Msg.Length > 0)
      {
        //MessageBox.Show("No puede determinar mapa de calor" + Environment.NewLine +
        //  Msg);
        mRespuestaCalor.Valores.Clear();
      }

      mCreador.DeterminarCurvas();
    }

    private bool mbGraficando = false;
    private object OBJ_LOCK = new object();
    private double mLatCentro, mLngCentro;
    private Int32 mNivelZoom;

    private void FiltrarConFiltrosPropios()
		{
      DatosFiltrados = (Proveedor == null ? new List<CLineaComprimida>() : Proveedor.Datos);
   //   if (Proveedor == null)
			//{
   //     Proveedor = new CProveedorComprimido(ClaseElemento.Bing, -1, ColumnasDataset, DatosFiltrados);
			//}
		}

    private async Task AgregarPushPinAsync(CPuntoTextoColor Punto)
    {
      object[] Args = new object[7];
      Args[0] = mPosicionMapaCalor;
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

    private async Task AgregarPinesAsync()
		{
      foreach (CPuntoTextoColor Pushpin in mPushpins)
			{
        await AgregarPushPinAsync(Pushpin);
			}
		}

    public void Refrescar()
		{
      StateHasChanged();
		}

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
      bool bRedibujar = false;
      lock (OBJ_LOCK)
      {
        if (mbGraficando)
        {
          return;
        }
        else
        {
          mbGraficando = true;
        }
      }
      try
      {
        if (!DatosCompletos)
        {
          if (Proveedor == null)
          {
            if (Filtros != null && Filtros.Filtrador != null)
            {
              Datos.ImponerProveedor(Filtros.Filtrador.Proveedor, false);
            }
            else
            {
              return;
            }
          }
          FiltrarConFiltrosPropios();
          // determinar valores escala.
          if (Clase == Plantillas.CGrafV2DatosContenedorBlock.ClaseBlock.Pines)
          {
            CrearPines();
          }
          else
          {
            CrearMapaCalor();
          }
          CRutinas.UbicarCentro(Abscisa < -998 ? Contenedores.CContenedorDatos.AnchoPantalla : Ancho,
              Abscisa < -998 ? (Contenedores.CContenedorDatos.AltoPantalla - 45) : (Alto - 25),
              mLatMin, mLatMax, mLngMin, mLngMax, out mLatCentro, out mLngCentro, out mNivelZoom);

          DatosCompletos = true;
          bRedibujar = true;
          StateHasChanged();
          return;
        }
        else
        {
          if (ReubicarCentro)
          {
            CRutinas.UbicarCentro(Abscisa < -998 ? Contenedores.CContenedorDatos.AnchoPantalla : Ancho,
                Abscisa < -998 ? (Contenedores.CContenedorDatos.AltoPantalla - 45) : (Alto - 25),
                mLatMin, mLatMax, mLngMin, mLngMax, out mLatCentro, out mLngCentro, out mNivelZoom);
            ReubicarCentro = false;
          }
          object[] Args = new object[7];
          Args[0] = mPosicionMapaCalor;
          Args[1] = '#' + Direccion;
          Args[2] = (mLatMax + mLatMin) / 2;
          Args[3] = (mLngMax + mLngMin) / 2;
          Args[4] = mNivelZoom;
          Args[5] = false;
          Args[6] = false;
          try
          {
            if (mPosicionMapaCalor < 0)
            {
              string CodigoMapa = await JSRuntime.InvokeAsync<string>("loadMapRetPos", Args);
              mPosicionMapaCalor = Int32.Parse(CodigoMapa);
            }
            if (Clase == Plantillas.CGrafV2DatosContenedorBlock.ClaseBlock.Pines)
            {
              await AgregarPinesAsync();
            }
            else
            {
              await mCreador.DibujarCurvasAsync(JSRuntime, mPosicionMapaCalor);
            }
            //  object[] Args = new object[14];
            //Args[0] = mPosicionMapaCalor;
            //Args[1] = '#' + Direccion;
            //Args[2] = mRespuestaCalor.SegmentosV;
            //Args[3] = mRespuestaCalor.SegmentosH;
            //Args[4] = mRespuestaCalor.Abscisas.ToArray();
            //Args[5] = mRespuestaCalor.Ordenadas.ToArray();
            //Args[6] = mRespuestaCalor.Valores.ToArray();
            //Args[7] = (mLatMax+mLatMin)/2;
            //Args[8] = (mLngMax+mLngMin)/2;
            //Args[9] = mNivelZoom;
            //Args[10] = mRespuestaCalor.ValoresEscala.ToArray();
            //Args[11] = -1;
            //Args[12] = -1;
            //Args[13] = -1;
            //try
            //{
            //  string CodigoMapa = await JSRuntime.InvokeAsync<string>("GetMapCalor", Args);
            //}
            //catch (Exception ex)
            //{
            //  CRutinas.DesplegarMsg(ex);
            //}
            //}
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
      finally
      {
        mbGraficando = false;
        if (bRedibujar)
				{
          StateHasChanged();
				}
      }
    }


  }
}
