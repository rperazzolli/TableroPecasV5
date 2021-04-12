using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaPinsLL : ComponentBase
	{
		[Parameter]
		public List<CLineaComprimida> Lineas { get; set; }

    [Parameter]
    public List<CColumnaBase> Columnas { get; set; }

    [Parameter]
		public string ColumnaDatos { get; set; }

		[Parameter]
		public string ColumnaLat { get; set; }

		[Parameter]
		public string ColumnaLng { get; set; }

		[Parameter]
		public bool Agrupar { get; set; } = true;

		[Parameter]
		public string Direccion { get; set; } = "";

		protected override Task OnInitializedAsync()
		{
			mCodigoMapa = gCodigoMapa++;
			Direccion = "PinsLL" + mCodigoMapa.ToString();
			return base.OnInitializedAsync();
		}

		public static Int32 gCodigoMapa = 0;
		private Int32 mCodigoMapa;
    private double mAbscisa = -1000;
    private double mOrdenada = -1000;
    private double mAbscisaCentro = -1000;
    private double mOrdenadaCentro = -1000;
    private double mAncho = -1000;
    private double mAlto = -1000;
    private double mNivelZoom = 0;
    private bool mbRedimensionar = true;
    private Int32 mPosicionMapa = -1;
    private List<IconoPush> mIconos = null;

    [Inject]
    public IJSRuntime JSRuntime { get; set; }

    private async Task ObtenerDimensionesPantallaAsync()
		{
      string Posicion = await JSRuntime.InvokeAsync<string>("FuncionesJS.getRectangulo", Direccion);
      List<double> Valores = CRutinas.ListaAReales(Posicion);
      mAbscisa = Valores[0];
      mOrdenada = Valores[1];
      mAncho = Valores[2];
      mAlto = Valores[3];
    }

    private CColumnaBase ColumnaNombre(string Nombre)
		{
      return (from C in Columnas
              where C.Nombre == Nombre
              select C).FirstOrDefault();
    }

    private void UbicarExtremosEscala(string Columna, out double Minimo, out double Maximo)
    {
      Minimo = double.MaxValue;
      Maximo = double.MinValue;
      CColumnaBase ColDatos = ColumnaNombre(Columna);
      if (ColDatos == null)
      {
        throw new Exception("No encuentra " + Columna);
      }
      if (ColDatos is CColumnaReal ColReal)
      {
        for (Int32 i = 0; i < ColReal.Valores.Count; i++)
        {
          if (Math.Abs((double)ColReal.Valores[i]) <= 180 && ((double)ColReal.Valores[i]) != 0)
          {
            Minimo = (double)ColReal.Valores[i];
            break;
          }
        }
        for (Int32 i = ColReal.Valores.Count - 1; i >= 0; i--)
        {
          if (Math.Abs((double)ColReal.Valores[i]) <= 180 && ((double)ColReal.Valores[i]) != 0)
          {
            Maximo = (double)ColReal.Valores[i];
            break;
          }
        }
      }
    }

    private void UbicarCentro()
		{
      double AbscMin, OrdMin, AbscMax, OrdMax;
      UbicarExtremosEscala(ColumnaLng, out AbscMin, out AbscMax);
      UbicarExtremosEscala(ColumnaLat, out OrdMin, out OrdMax);
      if (AbscMax<AbscMin || OrdMax < OrdMin)
			{
        throw new Exception("Coordenadas extremas incorrectas");
			}
      mAbscisaCentro = (AbscMin + AbscMax) / 2;
      mOrdenadaCentro = (OrdMin + OrdMax) / 2;
      mNivelZoom = CRutinas.UbicarNivelZoom(mAncho, mAlto, AbscMax - AbscMin, OrdMax - OrdMin);
		}

    private void AjustarAgrupacionesIconos(IconoPush Icono, Int32 Posicion, double RangoLat, double RangoLng)
		{
      for (Int32 i = Posicion-1; i >= 0; i--)
      {
        IconoPush Referencia = mIconos[i];
        if (Math.Abs(Referencia.Lng - Icono.Lng) > RangoLng)
				{
          break;
				}
        if (Referencia.LineaContenedor < 0)
        {
          if (Math.Abs(Referencia.Lat - Icono.Lat) < RangoLat)
          {
            Icono.LineaContenedor = i;
            Referencia.ValorGrafico += Icono.Valor;
            Referencia.Agrupado = true;
            return;
          }
        }
      }
      for (Int32 i = 0; i < Posicion; i++)
      {
        IconoPush Referencia = mIconos[i];
        if (Referencia.LineaContenedor < 0)
        {
          if (Math.Abs(Referencia.Lat-Icono.Lat)<RangoLat &&
						Math.Abs(Referencia.Lng - Icono.Lng) < RangoLng)
					{
            Icono.LineaContenedor = i;
            Referencia.ValorGrafico += Icono.Valor;
            Referencia.Agrupado = true;
            return;
					}
        }
			}

      Icono.LineaContenedor = -1;
      Icono.ValorGrafico = Icono.Valor;
      Icono.Agrupado = false;

		}

    private async Task AgregarPushPinAsync(IconoPush Punto)
    {
      object[] Args = new object[7];
      Args[0] = mPosicionMapa;
      Args[1] = Punto.Lng;
      Args[2] = Punto.Lat;
      Args[3] = (Punto.Agrupado ? "lightblue" : "lightgray");
      Args[4] = Punto.ValorGrafico.ToString();
      Args[5] = "";
      Args[6] = "";
      try
      {
        await JSRuntime.InvokeAsync<Task>((Punto.Agrupado ? "AgregarPushpinGrande" : "AgregarPushpin"), Args);
      }
      catch (Exception ex)
      {
//        Rutinas.CRutinas.DesplegarMsg(new Exception(Punto.Lng.ToString() + " " + Punto.Lat.ToString()));
      }
    }

    private async Task DibujarPushpinsAsync()
    {
      object[] Args = new object[1];
      Args[0] = mPosicionMapa;
      await JSRuntime.InvokeVoidAsync("LiberarPushpins", Args);

      foreach (IconoPush Punto in mIconos)
			{
        if (Punto.LineaContenedor < 0)
				{
          await AgregarPushPinAsync(Punto);
				}
			}

    }

    public bool DatosCompletos
		{
      get { return Lineas != null; }
		}

    public bool SinDatos
		{
      get { return (Lineas == null || Lineas.Count == 0); }
		}

    private void CargarValores()
    {
      CColumnaBase ColDatos = ColumnaNombre(ColumnaDatos);
      CColumnaBase ColLat = ColumnaNombre(ColumnaLat);
      CColumnaBase ColLng = ColumnaNombre(ColumnaLng);

      if (mIconos == null)
      {
        mIconos = new List<IconoPush>();
        Int32 i = 0;
        foreach (CLineaComprimida Linea in Lineas)
        {
          IconoPush Icono = new IconoPush()
          {
            Valor = ColDatos.ValorReal(Linea.Codigos[ColDatos.Orden], true),
            Lat = ColLat.ValorReal(Linea.Codigos[ColLat.Orden]),
            Lng = ColLng.ValorReal(Linea.Codigos[ColLng.Orden])
          };
          if (Math.Abs(Icono.Lat) <= 180 && Math.Abs(Icono.Lng) <= 180)
          {
            Icono.LineaDatos = i++;
            Icono.ValorGrafico = Icono.Valor;
            mIconos.Add(Icono);
          }
        }

        mIconos.Sort(delegate (IconoPush I1, IconoPush I2)
        {
          return I1.Lng.CompareTo(I2.Lng);
        });

      }
    }

    private async void FncProcesarViewChange(Int32 Posicion)
    {
      if (mPosicionMapa >= 0)
      {
      }

      object[] Args = new object[1];
      Args[0] = mPosicionMapa;
      try
      {
        string PosLocal = await JSRuntime.InvokeAsync<string>("ExtremosMapa", Args);
        List<double> Valores = CRutinas.ListaAReales(PosLocal);
        double RangoLng = Valores[2] - Valores[0];
        double RangoLat = Valores[3] - Valores[1];
        if (RangoLat <= 0 || RangoLng <= 0)
        {
          double LatMin = double.MaxValue;
          double LatMax = double.MinValue;
          double LngMin = double.MaxValue;
          double LngMax = double.MinValue;
          foreach (IconoPush Icono in mIconos)
          {
            LatMin = Math.Min(LatMin, Icono.Lat);
            LatMax = Math.Max(LatMax, Icono.Lat);
            LngMin = Math.Min(LngMin, Icono.Lng);
            LngMax = Math.Max(LngMax, Icono.Lng);
          }
          RangoLat = LatMax - LatMin;
          RangoLng = LngMax - LngMin;
        }
        double DeltaLat = 40 * RangoLat / mAlto;
        double DeltaLng = 25 * RangoLng / mAncho;

        for (Int32 i = 0; i < mIconos.Count; i++)
        {
          AjustarAgrupacionesIconos(mIconos[i], i, DeltaLat, DeltaLng);
        }

        await DibujarPushpinsAsync();

        StateHasChanged();

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      try
      {
        if (mAbscisa < -999)
        {
          await ObtenerDimensionesPantallaAsync();
          StateHasChanged();
        }
        else
        {
          if (mPosicionMapa < 0)
          {

            CargarValores();

            UbicarCentro();

            object[] Args = new object[7];
            Args[0] = mPosicionMapa;
            Args[1] = '#' + Direccion; // mProyecto.LatCentro;
            Args[2] = mOrdenadaCentro;
            Args[3] = mAbscisaCentro;
            Args[4] = mNivelZoom;
            Args[5] = true;
            Args[6] = false;
            try
            {
              string PosLocal = await JSRuntime.InvokeAsync<string>("loadMapRetPos", Args);
              mPosicionMapa = Int32.Parse(PosLocal);
              CLogicaBingMaps.gAlHacerViewChange = FncProcesarViewChange;
              FncProcesarViewChange(mPosicionMapa);
            }
            catch (Exception ex)
            {
              CRutinas.DesplegarMsg(ex);
            }
          }
        }
        //foreach (CPuntoTextoColor Punto in mPuntos)
        //{
        //	await AgregarPushPinAsync(Punto);
        //}
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

  }

  public class IconoPush
	{
    public double Valor { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public double ValorGrafico { get; set; }
    public Int32 LineaDatos { get; set; }
    public Int32 LineaContenedor { get; set; }
    public bool Agrupado { get; set; } = false;
	}
}
