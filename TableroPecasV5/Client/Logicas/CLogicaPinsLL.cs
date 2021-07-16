using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaPinsLL : ComponentBase, IDisposable
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

    public async void Dispose()
		{
      if (PosicionMapa >= 0)
			{
        await CRutinas.LiberarMapaAsync(JSRuntime, PosicionMapa);
			}
		}

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
    public double Ancho = -1000;
    public double Alto = -1000;
    private double mNivelZoom = 0;
    private bool mbRedimensionar = true;
    public Int32 PosicionMapa = -1;
    public List<IconoPush> Iconos = null;

    [Inject]
    public IJSRuntime JSRuntime { get; set; }

    private async Task ObtenerDimensionesPantallaAsync()
		{
      string Posicion = await JSRuntime.InvokeAsync<string>("FuncionesJS.getRectangulo", Direccion);
      List<double> Valores = CRutinas.ListaAReales(Posicion);
      mAbscisa = Valores[0];
      mOrdenada = Valores[1];
      Ancho = Valores[2];
      Alto = Valores[3];
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
      mNivelZoom = CRutinas.UbicarNivelZoom(Ancho, Alto, AbscMax - AbscMin, OrdMax - OrdMin);
		}

    private void AjustarAgrupacionesIconos(IconoPush Icono, Int32 Posicion, double RangoLat, double RangoLng)
		{
      for (Int32 i = Posicion-1; i >= 0; i--)
      {
        IconoPush Referencia = Iconos[i];
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
        IconoPush Referencia = Iconos[i];
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
      Args[0] = PosicionMapa;
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
      catch (Exception)
      {
//        Rutinas.CRutinas.DesplegarMsg(new Exception(Punto.Lng.ToString() + " " + Punto.Lat.ToString()));
      }
    }

    private async Task DibujarPushpinsAsync()
    {
      object[] Args = new object[1];
      Args[0] = PosicionMapa;
      await JSRuntime.InvokeVoidAsync("LiberarPushpins", Args);

      foreach (IconoPush Punto in Iconos)
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

    private Int32 CompararPuntos(IconoPush I1, IconoPush I2)
		{
      if (I1.Lng == I2.Lng)
			{
        return I1.Lat.CompareTo(I2.Lat);
			}
      else
			{
        return I1.Lng.CompareTo(I2.Lng);
			}
		}

    private bool CoordenadasIguales(IconoPush I1, IconoPush I2)
		{
      return (Math.Abs(I1.Lat - I2.Lat) < mDifMinima) && (Math.Abs(I1.Lng - I2.Lng) < mDifMinima);
		}

    private double DeterminarDiferenciaMinima()
		{
      if (Iconos.Count < 10)
			{
        return 0;
			}

      double LtMin = double.MaxValue;
      double LtMax = double.MinValue;
      double LnMin = double.MaxValue;
      double LnMax = double.MinValue;
      foreach (IconoPush Punto in Iconos)
			{
        LtMin = Math.Min(Punto.Lat, LtMin);
        LtMax = Math.Max(Punto.Lat, LtMax);
        LnMin = Math.Min(Punto.Lng, LnMin);
        LnMax = Math.Max(Punto.Lng, LnMax);
      }

      return Math.Max(LtMax - LtMin, LnMax - LnMin) / 100;

    }

    double mDifMinima;

    private void CargarValores()
    {
      CColumnaBase ColDatos = ColumnaNombre(ColumnaDatos);
      CColumnaBase ColLat = ColumnaNombre(ColumnaLat);
      CColumnaBase ColLng = ColumnaNombre(ColumnaLng);

      if (Iconos == null)
      {
        Iconos = new List<IconoPush>();
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
            Iconos.Add(Icono);
          }
        }

        Iconos.Sort(CompararPuntos);

        if (Agrupar || Iconos.Count > 500)
				{
          mDifMinima = DeterminarDiferenciaMinima();
				}
        else
				{
          mDifMinima = 0.00001;
				}


        for (i = Iconos.Count - 1; i > 0; i--)
				{
          if (CoordenadasIguales(Iconos[i], Iconos[i - 1]))
					{
            Iconos[i - 1].Valor += Iconos[i].Valor;
            Iconos.RemoveAt(i);
					}
				}

      }
    }

    public bool ProcesandoViewChange { get; set; } = false;
    public double ZoomAnterior = -1;
    public double ZoomPendiente = -1;

    private System.Timers.Timer mTimer = null;

    private void FncProcesarViewChange(Int32 Posicion, double Zoom)
    {
      if (PosicionMapa == Posicion && Agrupar)
      {
        if (mTimer != null)
				{
          mTimer.Stop();
          mTimer.Dispose();
          mTimer = null;
				}

        if (ZoomAnterior != Zoom)
				{
          ZoomPendiente = Zoom;
          mTimer = new System.Timers.Timer();
          mTimer.Interval = 2000;
					mTimer.Elapsed += MTimer_Elapsed;
          mTimer.Start();
				}
      }
    }

    public bool Graficando { get; set; } = false;

    private void MTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if (!ProcesandoViewChange)
      {
        try
        {
          Graficando = true;
          StateHasChanged();
          ProcesandoViewChange = true;
          //Thread workerThread = new Thread(TareaEnThread);
          //workerThread.Start(this);
          _ = TareaEnThreadAsync(this);
        }
        catch (Exception ex)
				{
          CRutinas.DesplegarMsg(ex);
				}
      }
    }

    private static async Task TareaEnThreadAsync(object Prm)
    {
      CLogicaPinsLL Logica = (CLogicaPinsLL)Prm;
      try
      {
        Logica.ZoomAnterior = Logica.ZoomPendiente;
        if (Logica.Agrupar)
        {
          object[] Args = new object[1];
          Args[0] = Logica.PosicionMapa;
          string PosLocal = await Logica.JSRuntime.InvokeAsync<string>("ExtremosMapa", Args);
          List<double> Valores = CRutinas.ListaAReales(PosLocal);
          double RangoLng = Valores[2] - Valores[0];
          double RangoLat = Valores[3] - Valores[1];
          if (RangoLat <= 0 || RangoLng <= 0)
          {
            double LatMin = double.MaxValue;
            double LatMax = double.MinValue;
            double LngMin = double.MaxValue;
            double LngMax = double.MinValue;
            foreach (IconoPush Icono in Logica.Iconos)
            {
              LatMin = Math.Min(LatMin, Icono.Lat);
              LatMax = Math.Max(LatMax, Icono.Lat);
              LngMin = Math.Min(LngMin, Icono.Lng);
              LngMax = Math.Max(LngMax, Icono.Lng);
            }
            RangoLat = LatMax - LatMin;
            RangoLng = LngMax - LngMin;
          }
          double DeltaLat = 40 * RangoLat / Logica.Alto;
          double DeltaLng = 25 * RangoLng / Logica.Ancho;

          for (Int32 i = 0; i < Logica.Iconos.Count; i++)
          {
            Logica.AjustarAgrupacionesIconos(Logica.Iconos[i], i, DeltaLat, DeltaLng);
          }
        }

        await Logica.DibujarPushpinsAsync();


      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
      finally
      {
        Logica.ProcesandoViewChange = false;
        Logica.Graficando = false;
        Logica.StateHasChanged();
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
          if (PosicionMapa < 0)
          {

            CargarValores();

            UbicarCentro();

            object[] Args = new object[7];
            Args[0] = PosicionMapa;
            Args[1] = '#' + Direccion; // mProyecto.LatCentro;
            Args[2] = mOrdenadaCentro;
            Args[3] = mAbscisaCentro;
            Args[4] = mNivelZoom;
            Args[5] = true;
            Args[6] = false;
            try
            {
              string PosLocal = await JSRuntime.InvokeAsync<string>("loadMapRetPos", Args);
              PosicionMapa = Int32.Parse(PosLocal);
              CLogicaBingMaps.gAlHacerViewChange = FncProcesarViewChange;
              ProcesandoViewChange = true;
              await TareaEnThreadAsync(this);
            }
            catch (Exception ex)
            {
              CRutinas.DesplegarMsg(ex);
            }
          }
        }
				foreach (IconoPush Punto in Iconos)
				{
					await AgregarPushPinAsync(Punto);
				}
			}
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
      finally
			{
        await base.OnAfterRenderAsync(firstRender);
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
