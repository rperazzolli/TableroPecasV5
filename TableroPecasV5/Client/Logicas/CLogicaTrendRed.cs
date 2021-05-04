using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using Blazor.Extensions;
using Blazor.Extensions.Canvas;
using System.Net.Http;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.Model;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaTrendRed : CLogicaReloj, IDisposable
  {
    enum PasoEscalaH
    {
      Minutos,
      Horas,
      Dias,
      Meses,
      Anios,
      NoDefinido
    }

    public const double ANCHO_TREND_RED = 350;
    public const double ALTO_TREND_RED = 180;

    [CascadingParameter]
    public CLogicaIndicador Pagina { get; set; }

    public CLogicaTrendRed()
    {
      Ancho = ANCHO_TREND_RED;
      Alto = ALTO_TREND_RED;
    }

    private bool mbCerrado = false;

    public new void Dispose()
    {
      mbCerrado = true;
    }

    public new long AnchoCanvas
    {
      get { return (long)ANCHO_TREND_RED; }
    }

    public new long AltoCanvas
    {
      get { return (long)ALTO_TREND_RED; }
    }

    //private async Task PonerNombreAsync()
    //{
    //  await mContexto.SetFontAsync("12px serif");
    //  TextMetrics Medida = await mContexto.MeasureTextAsync("H");
    //  double AltoCaracter = Medida.Width;
    //  mAltoTitulo = AltoCaracter + 8;
    //  string Msg = Indicador.Descripcion;
    //  while (true)
    //  {
    //    Medida = await mContexto.MeasureTextAsync(Msg);
    //    if (Medida.Width <= Ancho)
    //{
    //      break;
    //}
    //    Msg = Msg.Substring(0, Msg.Length - 1);
    //  }
    //  await mContexto.SetFillStyleAsync("black");
    //  await mContexto.FillTextAsync(Msg, (Ancho - Medida.Width) / 2, 2);
    //}

    private double mMinimo = double.MaxValue;
    private double mMaximo = double.MinValue;
    private double mAnchoEscalaV;
    private double mAnchoGrafico;
    private double mAltoEscalaH;
    //    private double mAltoTitulo;
    private double mAltoEscalaV;
    private DateTime mFechaMinima;
    private DateTime mFechaMaxima;

    private async Task DeterminarAnchoEscalaVAsync()
    {
      if (mMinimo > mMaximo)
      {
        mMinimo = (from A in Alarmas
                   select A.Valor).Min();
        mMaximo = (from A in Alarmas
                   select A.Valor).Max();
        CRutinas.RedondearExtremos(ref mMinimo, ref mMaximo);

        if (!Indicador.EscalaCreciente && Contenedores.CContenedorDatos.RespetaSentido)
        {
          double ValMax = Math.Min(mMaximo, mMinimo);
          mMinimo = Math.Max(mMinimo, mMaximo);
          mMaximo = ValMax;
        }

      }

      List<string> Textos = new List<string>();
      List<double> Anchos = new List<double>();
      for (Int32 i = 0; i <= 5; i++)
      {
        Textos.Add(CRutinas.ValorATexto(mMaximo + (mMinimo - mMaximo) * i / 5, Indicador.Decimales));
      }

      mAnchoEscalaV = 0;
      foreach (string Texto in Textos)
      {
        TextMetrics Medida = await mContexto.MeasureTextAsync(Texto);
        Anchos.Add(Medida.Width);
        mAnchoEscalaV = Math.Max(mAnchoEscalaV, Medida.Width);
      }

      mAnchoEscalaV += 4;

    }

    private async Task AgregarEscalaVerticalAsync(double AltoCaracter)
    {
      await mContexto.SetFillStyleAsync("#000000");
      for (Int32 i = 0; i <= 5; i++)
      {
        string Texto = CRutinas.ValorATexto(mMaximo + (mMinimo - mMaximo) * i / 5, Indicador.Decimales);
        TextMetrics Medida = await mContexto.MeasureTextAsync(Texto);
        await mContexto.FillTextAsync(Texto, mAnchoEscalaV - Medida.Width - 2,
            mOrdenadaGrafico + mAltoGrafico * i / 5 - AltoCaracter / 2);
      }
    }

    private PasoEscalaH mPaso;
    private Int32 mCantidadH;
    private Int32 mMesInicial;

    private Int32 DeterminarCantidadPaso(Int32 Ancho, double Rango, double AnchoGrafico, double AltoCaracter)
    {
      return (Int32)Math.Floor(Rango / (AnchoGrafico /
            (1.25 * (double)Ancho * AltoCaracter))) + 1;
    }

    private void DeterminarPasoCantidad(double AnchoGrafico, double AltoCaracter)
    {

      double DifDias = mFechaMaxima.ToOADate() - mFechaMinima.ToOADate();

      if (Indicador.Frecuencia == CRutinas.FREC_ANUAL)
      {
        mPaso = PasoEscalaH.Anios;
        if (DifDias < 730)
        {
          mFechaMaxima = mFechaMaxima.AddYears(1);
          DifDias = mFechaMaxima.ToOADate() - mFechaMinima.ToOADate();
        }
        mCantidadH = DeterminarCantidadPaso(4, DifDias / 366, AnchoGrafico, AltoCaracter);
      }
      else
      {
        switch (Indicador.Frecuencia)
        {
          case CRutinas.FREC_MINUTOS:
            mPaso = PasoEscalaH.Minutos;
            mCantidadH = DeterminarCantidadPaso(14,
                DifDias * 1440, AnchoGrafico, AltoCaracter);
            break;
          case CRutinas.FREC_HORARIA:
            mPaso = PasoEscalaH.Horas;
            mCantidadH = DeterminarCantidadPaso(11, DifDias * 24, AnchoGrafico, AltoCaracter);
            break;
          case CRutinas.FREC_DIARIA:
          case CRutinas.FREC_SEMANAL:
            mPaso = PasoEscalaH.Dias;
            mCantidadH = DeterminarCantidadPaso(5, DifDias, AnchoGrafico, AltoCaracter);
            break;
          case CRutinas.FREC_MENSUAL:
          case CRutinas.FREC_BIMESTRAL:
          case CRutinas.FREC_TRIMESTRAL:
          case CRutinas.FREC_CUATRIMESTRAL:
          case CRutinas.FREC_SEMESTRAL:
          case CRutinas.FREC_MENSUAL_AC:
            mPaso = PasoEscalaH.Meses;
            mCantidadH = DeterminarCantidadPaso(5, DifDias / 31, AnchoGrafico, AltoCaracter);
            break;
          case CRutinas.FREC_ANUAL:
            mPaso = PasoEscalaH.Anios;
            mMesInicial = 1;
            mCantidadH = DeterminarCantidadPaso(4, DifDias / 366, AnchoGrafico, AltoCaracter);
            break;
        }
      }

      mCantidadH = 1;

      switch (Indicador.Frecuencia)
      {
        case CRutinas.FREC_ANUAL:
          switch (mPaso)
          {
            case PasoEscalaH.Meses:
              if (mCantidadH < 12)
              {
                mPaso = PasoEscalaH.Anios;
                mCantidadH = 1;
              }
              break;
            case PasoEscalaH.Dias:
              if (mCantidadH < 365)
              {
                mPaso = PasoEscalaH.Anios;
                mCantidadH = 1;
              }
              break;
          }
          break;
        case CRutinas.FREC_SEMESTRAL:
          if (mPaso == PasoEscalaH.Dias && mCantidadH < 180)
          {
            mPaso = PasoEscalaH.Meses;
            mCantidadH = 6;
          }
          break;
        case CRutinas.FREC_CUATRIMESTRAL:
          if (mPaso == PasoEscalaH.Dias && mCantidadH < 120)
          {
            mPaso = PasoEscalaH.Meses;
            mCantidadH = 4;
          }
          break;
        case CRutinas.FREC_TRIMESTRAL:
          if (mPaso == PasoEscalaH.Dias && mCantidadH < 90)
          {
            mPaso = PasoEscalaH.Meses;
            mCantidadH = 3;
          }
          break;
        case CRutinas.FREC_BIMESTRAL:
          if (mPaso == PasoEscalaH.Dias && mCantidadH < 60)
          {
            mPaso = PasoEscalaH.Meses;
            mCantidadH = 2;
          }
          break;
        case CRutinas.FREC_MENSUAL:
        case CRutinas.FREC_MENSUAL_AC:
          if (mPaso == PasoEscalaH.Dias && mCantidadH < 30)
          {
            mPaso = PasoEscalaH.Meses;
            mCantidadH = 1;
          }
          break;
      }

    }

    public double AbscisaFecha(DateTime Fecha)
    {
      if (mFechaMinima.Year > 1800)
      {
        return mAnchoEscalaV + mAnchoGrafico *
          (Fecha.ToOADate() - mFechaMinima.ToOADate()) /
          (mFechaMaxima.ToOADate() - mFechaMinima.ToOADate());
      }
      else
      {
        return 0;
      }
    }

    public string FormatoFecha(DateTime Fecha)
    {
      if (Indicador.Frecuencia == CRutinas.FREC_ANUAL)
      {
        if (mMesInicial == 1) return string.Format("{0:yyyy}", Fecha);
        else return string.Format("{0:MM/yy}", Fecha);
      }
      else
      {
        if (Indicador.Frecuencia == CRutinas.FREC_SEMANAL)
        {
          return string.Format("{0:dd/MM}", Fecha);
        }
        switch (mPaso)
        {
          case PasoEscalaH.Minutos:
            return string.Format("{0:dd/MM/yy hh:mm}", Fecha);
          case PasoEscalaH.Horas:
            return string.Format("{0:dd/MM/yy hh}", Fecha);
          case PasoEscalaH.Dias:
            return string.Format("{0:dd/MM/yy}", Fecha);
          case PasoEscalaH.Meses:
            return string.Format("{0:MM/yy}", Fecha);
          default:
            if (Fecha.Month == 12 && Fecha.Day == 31)
            {
              Fecha = Fecha.AddDays(1);
            }
            return (mMesInicial == 1 ?
              string.Format("{0:yyyy}", Fecha) :
              string.Format("{0:MM/yy}", Fecha));
        }
      }
    }

    public string FormatoFechaFicticio()
    {
      if (Indicador.Frecuencia == CRutinas.FREC_ANUAL)
      {
        if (mMesInicial == 1) return "HHHH";
        else return "HH/HH";
      }
      else
      {
        if (Indicador.Frecuencia == CRutinas.FREC_SEMANAL)
        {
          return "HH/HH";
        }
        switch (mPaso)
        {
          case PasoEscalaH.Minutos:
            return "HH/HH/HH HH:HH";
          case PasoEscalaH.Horas:
            return "HH/HH/HH HH";
          case PasoEscalaH.Dias:
            return "HH/HH/HH";
          case PasoEscalaH.Meses:
            return "HH/HH";
          default:
            return (mMesInicial == 1 ? "HHHH" : "HH/HH");
        }
      }
    }

    private async Task PonerTextoEscalaFechasGiradosAsync(DateTime Fecha, double AltoCaracter)
    {
      string Texto = FormatoFecha(Fecha);
      TextMetrics Medida = await mContexto.MeasureTextAsync(Texto);
      double Abscisa = AbscisaFecha(Fecha) + AltoCaracter / 2;
      double Ordenada = mAltoEscalaV + 2 + Medida.Width;
      await mContexto.FillTextAsync(Texto, -Ordenada, Abscisa);
    }

    private async Task DeterminarAltoEscalaHAsync(double AltoCaracter)
    {

      mAnchoGrafico = Ancho - mAnchoEscalaV;
      mFechaMinima = Alarmas[0].FechaInicial;
      mFechaMaxima = Alarmas.Last().FechaInicial;

      if (mFechaMaxima == mFechaMinima)
      {
        mFechaMinima = mFechaMinima.AddDays(-1);
        mFechaMaxima = mFechaMaxima.AddDays(1);
      }

      DeterminarPasoCantidad(Ancho - mAnchoEscalaV, AltoCaracter);

      TextMetrics Medida = await mContexto.MeasureTextAsync(FormatoFechaFicticio());

      mAltoEscalaH = 4 + Medida.Width;

    }

    private async Task AgregarEscalaHorizontalAsync(double AltoCaracter)
    {
      await mContexto.RotateAsync((float)(-Math.PI / 2));
      foreach (DateTime Fecha in (from A in Alarmas
                                  select A.FechaInicial).ToList())
      {
        await PonerTextoEscalaFechasGiradosAsync(Fecha, AltoCaracter);
      }
      await mContexto.SetTransformAsync(1, 0, 0, 1, 0, 0);
    }

    public double OrdenadaValor(double Valor)
    {
        return mOrdenadaGrafico + mAltoGrafico *
          (mbEscalaCreciente ? (mMaximo - Valor) : (Valor - mMinimo)) / (mMaximo - mMinimo);
    }

    public double OrdenadaBanda(CInformacionAlarmaCN Punto, Int32 Banda)
    {
      switch (Banda)
      {
        case 1:
          return OrdenadaValor(Punto.Minimo);
        case 2:
          return OrdenadaValor(Punto.Satisfactorio);
        case 3:
          return OrdenadaValor(Punto.Sobresaliente);
        default:
          return 0;
      }
    }

    private void ProcesarElemento(CInformacionAlarmaCN Dato,
        ref List<CPunto> Lista,
        ref double Desde, ref double Hasta,
        ref double OrdIni,
        Int32 Posicion)
    {
      Hasta = AbscisaFecha(Dato.FechaInicial);
      if (double.IsNaN(Desde))
      {
        Desde = Hasta;
        OrdIni = OrdenadaBanda(Dato, Posicion);
      }
      Lista.Add(new CPunto(Hasta, OrdenadaBanda(Dato, Posicion)));
    }

    private void SumarPuntos(Int32 Posicion,
          ref List<CPunto> Lista,
          ref double Desde, ref double Hasta,
          ref double OrdIni,
          bool bInvertido)
    {
      Desde = (Alarmas.Count == 1 ? 0 : double.NaN);
      Hasta = double.NaN;
      OrdIni = (Alarmas.Count == 1 ?
          OrdenadaBanda(Alarmas[0], Posicion) : double.NaN);
      if (!bInvertido) // recorre el listado hacia adelante.
      {
        foreach (CInformacionAlarmaCN Dato in Alarmas)
        {
          ProcesarElemento(Dato, ref Lista,
              ref Desde, ref Hasta, ref OrdIni, Posicion);
        }
      }
      else
      {
        for (Int32 i = Alarmas.Count - 1; i >= 0; i--)
        {
          ProcesarElemento(Alarmas[i], ref Lista,
              ref Desde, ref Hasta, ref OrdIni, Posicion);
        }
      }
    }

    private async Task DibujarShapeInferiorAsync()
    {
      List<CPunto> Puntos = new List<CPunto>();

      double Desde = 0;
      double Hasta = 0;
      double OrdIni = 0;
      SumarPuntos(mbEscalaCreciente ? 1 : 3, ref Puntos, ref Desde,
          ref Hasta, ref OrdIni, false);

      // Agrega las lineas para cerrar el grafico en la parte inferior.
      if (Puntos.Count > 0)
      {
        Puntos.Add(new CPunto(Hasta, mOrdenadaGrafico + mAltoGrafico));
        Puntos.Add(new CPunto(Desde, mOrdenadaGrafico + mAltoGrafico));

        await CTendencias.AgregarZonaPuntosAsync(mContexto, Puntos,
            mbEscalaCreciente ? CTendencias.ColorZonaRoja() : CTendencias.ColorZonaAzul(),
             mOrdenadaGrafico, mOrdenadaGrafico + mAltoGrafico);

      }
    }

    private async Task DibujarShapeIntermedioAsync(Canvas2DContext Contexto, Int32 Banda1, Int32 Banda2,
        string ColorBlock)
    {

      List<CPunto> Puntos = new List<CPunto>();

      double Desde = 0;
      double Hasta = 0;
      double OrdIni = 0;
      SumarPuntos(Banda2, ref Puntos, ref Desde, ref Hasta, ref OrdIni, false);
      double Desde2 = 0;
      double Ord2 = 0;
      SumarPuntos(Banda1, ref Puntos, ref Desde2, ref Hasta, ref Ord2, true);

      if (Puntos.Count > 0)
      {
        Puntos.Add(new CPunto(Desde, Ord2));

        await CTendencias.AgregarZonaPuntosAsync(Contexto, Puntos,
          ColorBlock, mOrdenadaGrafico, mOrdenadaGrafico + mAltoGrafico);

      }
    }

    private async Task DibujarShapeSuperiorAsync(Canvas2DContext Contexto)
    {

      List<CPunto> Puntos = new List<CPunto>();

      double Desde = 0;
      double Hasta = 0;
      double OrdIni = 0;
      SumarPuntos(mbEscalaCreciente ? 3 : 1, ref Puntos, ref Desde,
          ref Hasta, ref OrdIni, false);

      if (Puntos.Count > 0)
      {
        Puntos.Add(new CPunto(Hasta, mOrdenadaGrafico));
        Puntos.Add(new CPunto(Desde, mOrdenadaGrafico));

        await CTendencias.AgregarZonaPuntosAsync(Contexto, Puntos,
            mbEscalaCreciente ? CTendencias.ColorZonaAzul() : CTendencias.ColorZonaRoja(),
             mOrdenadaGrafico, mOrdenadaGrafico + mAltoGrafico);

      }

    }

    private double mOrdenadaGrafico;
    private double mAltoGrafico;
    private async Task DeterminarDimensionesEscalasAsync(double AltoCaracter)
		{
      await DeterminarAnchoEscalaVAsync();
      await DeterminarAltoEscalaHAsync(AltoCaracter);
      mAltoGrafico = Alto - mOrdenadaGrafico - mAltoEscalaH;
      mAltoEscalaV = Alto - mAltoEscalaH;
      mAnchoGrafico = Ancho - mAnchoEscalaV - AltoCaracter/2 - 2;
		}

    private bool mbHayBandas;
    private bool mbEscalaCreciente;
    private async Task DibujarZonasLlenasAsync()
    {
      mbHayBandas = false;
      foreach (CInformacionAlarmaCN Punto in Alarmas)
      {
        if (Punto.Sobresaliente != Punto.Minimo)
        {
          mbEscalaCreciente = (Punto.Sobresaliente >= Punto.Minimo);
          mbHayBandas = true;
          break;
        }
      }

      if (!mbHayBandas)
      {
        return;
      }

      await DibujarShapeInferiorAsync();

      if (!Indicador.EscalaCreciente && !Contenedores.CContenedorDatos.RespetaSentido)
      {
        await DibujarShapeIntermedioAsync(mContexto, 3, 2,
            CTendencias.ColorZonaVerde());
        await DibujarShapeIntermedioAsync(mContexto, 2, 1,
            CTendencias.ColorZonaAmarilla());
      }
      else
      {
        await DibujarShapeIntermedioAsync(mContexto, 1, 2,
            CTendencias.ColorZonaAmarilla());
        await DibujarShapeIntermedioAsync(mContexto, 2, 3,
            CTendencias.ColorZonaVerde());
      }

      await DibujarShapeSuperiorAsync(mContexto);

    }

    private async Task DibujarGrillaAsync(Canvas2DContext Contexto)
    {
      await Contexto.BeginPathAsync();
      await Contexto.SetLineWidthAsync(1);
      await Contexto.SetStrokeStyleAsync("black");
      float[] Saltos = new float[0];
      await Contexto.SetLineDashAsync(Saltos);
      await Contexto.MoveToAsync(mAnchoEscalaV, mOrdenadaGrafico);
      await Contexto.LineToAsync(mAnchoEscalaV + mAnchoGrafico, mOrdenadaGrafico);
      await Contexto.LineToAsync(mAnchoEscalaV + mAnchoGrafico, mOrdenadaGrafico + mAltoGrafico);
      await Contexto.LineToAsync(mAnchoEscalaV, mOrdenadaGrafico + mAltoGrafico);
      await Contexto.LineToAsync(mAnchoEscalaV, mOrdenadaGrafico);
      await Contexto.StrokeAsync();
      await Contexto.ClosePathAsync();

      await Contexto.BeginPathAsync();
      await Contexto.SetLineWidthAsync(1);
      await Contexto.SetStrokeStyleAsync(mbHayBandas ? "#000000" : "#c0c0c0");
      await Contexto.SetLineDashAsync(new float[] { 2, 1 });
      for (Int32 i = 1; i < 5; i++)
      {
        double Ord00 = mOrdenadaGrafico + mAltoGrafico * i / 5;
        await Contexto.MoveToAsync(mAnchoEscalaV, Ord00);
        await Contexto.LineToAsync(mAnchoEscalaV + mAnchoGrafico, Ord00);
      }
      await Contexto.StrokeAsync();

      await Contexto.ClosePathAsync();

    }

    private async Task DibujarCurvaAsync(Canvas2DContext Contexto, double AltoCaracter)
    {
      if (Alarmas.Count > 0)
      {

        await Contexto.BeginPathAsync();

        bool bPrimerPunto = true;

        foreach (CInformacionAlarmaCN Dato in Alarmas)
        {
          if (bPrimerPunto)
          {
            await Contexto.MoveToAsync(AbscisaFecha(Dato.FechaInicial), OrdenadaValor(Dato.Valor));
            bPrimerPunto = false;
          }
          else
          {
            await Contexto.LineToAsync(AbscisaFecha(Dato.FechaInicial), OrdenadaValor(Dato.Valor));
          }
        }

        await Contexto.SetStrokeStyleAsync("#000000");

        await Contexto.StrokeAsync();

        await Contexto.ClosePathAsync();

      }

      Int32 Pos = 0;
      foreach (CInformacionAlarmaCN Dato in Alarmas)
      {
        await Contexto.BeginPathAsync();
        await Contexto.SetStrokeStyleAsync("#000000");
        await Contexto.SetLineWidthAsync(1);
        CPunto Punto = new CPunto(AbscisaFecha(Dato.FechaInicial), OrdenadaValor(Dato.Valor));
        await Contexto.SetFillStyleAsync("#c0c0c0");
        await Contexto.ArcAsync(Punto.Abscisa, Punto.Ordenada, CTendencias.SEMIDIAMETRO, 0, 2 * Math.PI);
        await Contexto.FillAsync();
        await Contexto.StrokeAsync();
        await Contexto.ClosePathAsync();
        Pos++;
      }

      // Si corresponde agrega las etiquetas.
      if (Contenedores.CContenedorDatos.PoneEtiquetas)
      {
        foreach (CInformacionAlarmaCN Dato in Alarmas)
        {
          string Texto = Rutinas.CRutinas.ValorATexto(Dato.Valor, Indicador.Decimales);
          TextMetrics Dimensiones = await Contexto.MeasureTextAsync(Texto);
          double AnchoRect = Dimensiones.Width + 4;
          double AltoRect = AltoCaracter + 4;
          double Abscisa = Math.Max(mAnchoEscalaV,
              Math.Min(mAnchoEscalaV + mAnchoGrafico - AnchoRect, AbscisaFecha(Dato.FechaInicial) - AnchoRect / 2));
          double Ordenada = OrdenadaValor(Dato.Valor) + 4;
          if (Ordenada > (mOrdenadaGrafico + mAltoGrafico))
          {
            Ordenada -= (AltoRect + 8);
          }
          // Dibujar el rectangulo.
          await Contexto.SetFillStyleAsync("#ffffff");
          await Contexto.FillRectAsync(Abscisa, Ordenada, AnchoRect, AltoRect);
          await Contexto.SetFillStyleAsync("#000000");
          await Contexto.FillTextAsync(Texto, Abscisa + 2, Ordenada + AltoRect - 2);
        }

      }
    }

    [Inject]
    public HttpClient Http { get; set; }

    private async Task LeerAlarmasAsync()
		{
      Alarmas = await Contenedores.CContenedorDatos.ObtenerAlarmasIndicadorAsync(Http, Indicador,
          CodigoElementoDimension, true);
      mbLeyo = true;
      StateHasChanged();
      if (Pagina != null)
			{
        Pagina.HayAlarmaReducida = true;
        Pagina.Refrescar();
			}
		}

    private bool mbLeyo = false;
    private bool mbLeyendo = false;
    private bool mbDibujando = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {

      if (CanvasReloj == null || mbCerrado)
      {
        return;
      }

      try
      {
        mContexto = await Blazor.Extensions.CanvasContextExtensions.CreateCanvas2DAsync(CanvasReloj);

        if (Alarmas!=null && Alarmas.Count > 1)
				{
          mbLeyo = true;
				}
        //
        await mContexto.BeginBatchAsync();

        try
        {

          await mContexto.ClearRectAsync(0, 0, AnchoCanvas, AltoCanvas);
          await mContexto.SetFillStyleAsync("white");
          await mContexto.FillRectAsync(0, 0, AnchoCanvas, AltoCanvas);

          double AltoCaracter;

          if (!mbLeyo)
          {

            await mContexto.SetFontAsync("11px serif");
            string Msg = "Aguarde por favor";
            TextMetrics Medida = await mContexto.MeasureTextAsync("H");
            AltoCaracter = Medida.Width;
            mAltoEscalaH = AltoCaracter + 4;
            Medida = await mContexto.MeasureTextAsync(Msg);
            await mContexto.SetFillStyleAsync("black");
            await mContexto.FillTextAsync(Msg, (Ancho - Medida.Width) / 2, (Alto + AltoCaracter) / 2);
          }
          else
          {
            if (!mbDibujando)
            {
              mbDibujando = true;
              try
              {
                //            await PonerNombreAsync();
                await mContexto.SetFillStyleAsync("#000000");
                await mContexto.SetFontAsync("11px serif");

                if (Alarmas.Count > 0)
                {
                  TextMetrics Medida = await mContexto.MeasureTextAsync("H");
                  AltoCaracter = Medida.Width;

                  mOrdenadaGrafico = AltoCaracter / 2;

                  await DeterminarDimensionesEscalasAsync(AltoCaracter);

                  await DibujarZonasLlenasAsync();

                  await DibujarGrillaAsync(mContexto);

                  await AgregarEscalaVerticalAsync(AltoCaracter);

                  await AgregarEscalaHorizontalAsync(AltoCaracter);

                  await DibujarCurvaAsync(mContexto, AltoCaracter);

                }
              }
              finally
              {
                mbDibujando = false;
              }
            }

          }

        }
        finally
        {
          await mContexto.EndBatchAsync();
          if (!mbLeyo && !mbLeyendo)
					{
            mbLeyendo = true;
            await LeerAlarmasAsync();
          }
        }

      }
      catch (Exception ex)
      {
        Rutinas.CRutinas.DesplegarMsg(ex);
      }
    }

  }
}
