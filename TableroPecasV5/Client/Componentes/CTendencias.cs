using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using Blazor.Extensions.Canvas;
using System.Windows;
using Blazor.Extensions.Canvas.Model;
using Blazor.Extensions.Canvas.Canvas2D;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Componentes
{
  public class CTendencias
  {

    public ObservableCollection<Clases.CCurvaTendencia> Curvas { get; set; } = new ObservableCollection<Clases.CCurvaTendencia>();

    public Int32 PuntoSeleccionado
    {
      get { return mPosValorSeleccionado; }
      set { mPosValorSeleccionado = value; }
    }

    public bool MostrarReferencias { get; set; } = false;

    public bool MostrarEtiquetas { get; set; } = false;

    public Int32 MesesAgrupacionBarras { get; set; } = -1;

    public DateTime FechaMinima { get; set; }

    public DateTime FechaMaxima { get; set; }

    public double DimensionCaracter { get; set; } = -1;

    public double Ancho { get; set; }
    public double AnchoContenedor { get; set; }
    public double Alto { get; set; }

    private double mMinimo;
    private double mMaximo;
    private double mMinimoDerecha;
    private double mMaximoDerecha;
    private DateTime mFechaMinima;
    private DateTime mFechaMaxima;
    private Rutinas.CRutinas.SaltoEscalaFechas mSalto;

    private void ObtenerExtremosBarras()
    {
      if (Periodos != null && Periodos.Count > 0)
      {
        mMaximo = (from V in Periodos
                   select (from D in V.Datos
                           select D.Valor).Max()).Max();
        mMinimo = (from V in Periodos
                   select (from D in V.Datos
                           select D.Valor).Max()).Min();
        bool HayNeg = (mMinimo < 0);
        if (mMinimo > 0)
        {
          mMinimo = 0;
        }
        Rutinas.CRutinas.AjustarExtremosEscala(ref mMinimo, ref mMaximo);
        if (mMinimo < 0 && !HayNeg)
        {
          mMaximo -= mMinimo;
          mMinimo = 0;
        }
        mFechaMinima = (from P in Periodos
                        select P.Desde).Min();
        mFechaMaxima = (from P in Periodos
                        select P.Desde).Max();
        if (MesesAgrupacionBarras == 12)
        {
          mSalto = Rutinas.CRutinas.SaltoEscalaFechas.Anios;
        }
        else
        {
          mSalto = Rutinas.CRutinas.SaltoEscalaFechas.Meses;
        }
      }
    }

      private void ObtenerExtremosEscalas()
    {
      mMinimo = double.MaxValue;
      mMaximo = double.MinValue;
      mMinimoDerecha = double.MaxValue;
      mMaximoDerecha = double.MinValue;
      mFechaMinima = FechaMinima;
      mFechaMaxima = FechaMaxima;

      if (MesesAgrupacionBarras > 0)
      {
        ObtenerExtremosBarras();
        return;
      }

      foreach (Clases.CCurvaTendencia Curva in Curvas)
      {
        if (Curva.IndicadorBase || !Curva.EscalaDerecha)
        {
          foreach (CInformacionAlarmaCN Dato in Curva.Alarmas)
          {
            mMinimo = Math.Min(mMinimo, Dato.Valor);
            mMaximo = Math.Max(mMaximo, Dato.Valor);
          }
        }
        else
        {
          foreach (CInformacionAlarmaCN Dato in Curva.Alarmas)
          {
            mMinimoDerecha = Math.Min(mMinimoDerecha, Dato.Valor);
            mMaximoDerecha = Math.Max(mMaximoDerecha, Dato.Valor);
          }
        }
        if (Curva.Alarmas.Count > 0)
        {
          if (Curva.Alarmas[0].FechaInicial < mFechaMinima)
          {
            mFechaMinima = Curva.Alarmas[0].FechaInicial;
          }
          if (Curva.Alarmas.Last().FechaInicial > mFechaMaxima)
          {
            mFechaMaxima = Curva.Alarmas.Last().FechaInicial;
          }
        }
      }

      Rutinas.CRutinas.AjustarExtremosEscala(ref mMinimo, ref mMaximo);
      if (mMaximoDerecha >= mMinimoDerecha)
      {
        Rutinas.CRutinas.AjustarExtremosEscala(ref mMinimoDerecha, ref mMaximoDerecha);
      }

      mSalto = Rutinas.CRutinas.AjustarExtremosFechas(ref mFechaMinima, ref mFechaMaxima);

    }

    private double mAnchoEscala;
    private double mAnchoEscalaDerecha;
    private double mAbscisaGrafico;
    private double mOrdenadaGrafico;
    private double mAnchoGrafico;
    private double mAltoGrafico;
    private double mOrdenadaFechas;
    private double mAnchoFechas;
    private Int32 mColumnasReferencia;
    private Int32 mFilasReferencia;

    private async Task<double> DeterminarAnchoEscalaAsync(double Minimo, double Maximo, Canvas2DContext Contexto)
    {
      string Fmt = Rutinas.CRutinas.FormatoPorSalto(Minimo, Maximo);
      TextMetrics MedidasMin = await Contexto.MeasureTextAsync(Minimo.ToString(Fmt));
      TextMetrics MedidasMax= await Contexto.MeasureTextAsync(Maximo.ToString(Fmt));

      TextMetrics M = await Contexto.MeasureTextAsync("H");
      DimensionCaracter = M.Width;

      return Math.Max(MedidasMax.Width, MedidasMin.Width) + 2 * DimensionCaracter;

    }

    private const double SEP_REFERENCIAS_V = 4;

    private double AnchoNecesarioBarras
    {
      get
      {
        if (Periodos==null || Periodos.Count == 0)
        {
          return 0;
        }
        return AbscisaPeriodo(Periodos.Count - 1) +
          Curvas.Count * ANCHO_BARRA + (Curvas.Count - 1) * SEP_ENTRE_BARRAS;
      }
    }

    private async Task DeterminarDimensionesAsync(Canvas2DContext Contexto, double AnchoEscala)
    {

      AjustarHayBandas();

      // Obtener valores escalas.
      ObtenerExtremosEscalas();

      mAnchoEscala = await DeterminarAnchoEscalaAsync(mMinimo, mMaximo, Contexto);
      if (AnchoEscala > 0)
      {
        mAnchoEscala = AnchoEscala;
      }

      mFechaMaxima = FechaMaxima;
      if (mFechaMinima > FechaMinima)
      {
        mFechaMinima = FechaMinima;
      }

      Rutinas.CRutinas.SaltoEscalaFechas Salto = Rutinas.CRutinas.AjustarExtremosFechas(ref mFechaMinima, ref mFechaMaxima);
      string FmtFecha = Rutinas.CRutinas.FormatoFechaPorSalto(Salto);
      string TextoFechaParaAncho = Rutinas.CRutinas.TextoFechaPorSalto(Salto);
      TextMetrics Dimension = await Contexto.MeasureTextAsync(TextoFechaParaAncho);
      mAnchoFechas = Dimension.Width;
      mAnchoEscala = Math.Max(mAnchoEscala, mAnchoFechas / 2 + 1);

      if (mMaximoDerecha > mMinimoDerecha)
      {
        mAnchoEscalaDerecha = await DeterminarAnchoEscalaAsync(mMinimoDerecha, mMaximoDerecha, Contexto);
      }
      else
      {
        mAnchoEscalaDerecha = 0;
      }

      mAnchoEscalaDerecha = Math.Max(mAnchoEscalaDerecha, mAnchoFechas / 2 + 1);

      if (Curvas.Count > 1 && MostrarReferencias)
      {
        if (Curvas.Count <= 3)
        {
          mFilasReferencia = 1;
          mColumnasReferencia = Curvas.Count;
        }
        else
        {
          if (Curvas.Count == 4)
          {
            mFilasReferencia = 2;
            mColumnasReferencia = 2;
          }
          else
          {
            mColumnasReferencia = 3;
            mFilasReferencia = ((Curvas.Count % 3) == 0 ? (Curvas.Count / 3) : ((Curvas.Count + 3 - Curvas.Count % 3) / 3));
          }
        }
      }
      else
      {
        mFilasReferencia = 0;
        mColumnasReferencia = 0;
      }

      mAbscisaGrafico = mAnchoEscala + DimensionCaracter;
      mOrdenadaGrafico = DimensionCaracter / 2 + mFilasReferencia * (DimensionCaracter + SEP_REFERENCIAS_V) +
          (mFilasReferencia > 0 ? SEP_REFERENCIAS_V : 0);
      mAnchoGrafico = Ancho - mAbscisaGrafico - mAnchoEscalaDerecha - DimensionCaracter;
      mAltoGrafico = Alto - 3 * DimensionCaracter - mOrdenadaGrafico;
      // Si hay scroll, reducir el alto.
      if (MesesAgrupacionBarras > 0 &&
          (AnchoNecesarioBarras + mAbscisaGrafico + mAnchoEscalaDerecha + DimensionCaracter) > AnchoContenedor)
      {
        mAltoGrafico -= 17;
      }
      mOrdenadaFechas = mOrdenadaGrafico + mAltoGrafico + 1.5 * DimensionCaracter;

    }

    private async Task DibujarEscalaAsync(Canvas2DContext Contexto)
    {
      await Contexto.SetFillStyleAsync("black");
      string Fmt = Rutinas.CRutinas.FormatoPorSalto(mMinimo, mMaximo);
      for (Int32 i = 0; i <= 5; i++)
      {
        double Valor = mMaximo + (mMinimo - mMaximo) * i / 5;
        string Aa = Valor.ToString(Fmt);
        TextMetrics Dimensiones = await Contexto.MeasureTextAsync(Aa);
        await Contexto.FillTextAsync(Aa, mAbscisaGrafico - DimensionCaracter / 2 - Dimensiones.Width,
           OrdenadaValor(Valor) + DimensionCaracter / 2);
      }
    }

    private async Task DibujarEscalaDerechaAsync(Canvas2DContext Contexto)
    {
      if (mMaximoDerecha >= mMinimoDerecha)
      {
        await Contexto.SetFillStyleAsync("black");
        string Fmt = Rutinas.CRutinas.FormatoPorSalto(mMinimoDerecha, mMaximoDerecha);
        for (Int32 i = 0; i <= 5; i++)
        {
          double Valor = mMaximoDerecha + (mMinimoDerecha - mMaximoDerecha) * i / 5;
          string Aa = Valor.ToString(Fmt);
          await Contexto.FillTextAsync(Aa, mAbscisaGrafico + mAnchoGrafico + DimensionCaracter / 2,
             OrdenadaValor(Valor, true) + DimensionCaracter / 2);
        }
      }
    }

    //private async Task DibujarZonasLlenasAsync(Canvas2DContext Dibujador)
    //{
    //}

    private async Task DibujarGrillaAsync(Canvas2DContext Contexto)
    {
      await Contexto.BeginPathAsync();
      await Contexto.SetLineWidthAsync(1);
      await Contexto.SetStrokeStyleAsync("black");
      float[] Saltos = new float[0];
      await Contexto.SetLineDashAsync(Saltos);
      await Contexto.MoveToAsync(mAbscisaGrafico, mOrdenadaGrafico);
      await Contexto.LineToAsync(mAbscisaGrafico + mAnchoGrafico, mOrdenadaGrafico);
      await Contexto.LineToAsync(mAbscisaGrafico + mAnchoGrafico, mOrdenadaGrafico + mAltoGrafico);
      await Contexto.LineToAsync(mAbscisaGrafico, mOrdenadaGrafico + mAltoGrafico);
      await Contexto.LineToAsync(mAbscisaGrafico, mOrdenadaGrafico);
      await Contexto.StrokeAsync();
      await Contexto.ClosePathAsync();

      await Contexto.BeginPathAsync();
      await Contexto.SetLineWidthAsync(1);
      await Contexto.SetStrokeStyleAsync(mbHayBandas ? "#000000" : "#c0c0c0");
      await Contexto.SetLineDashAsync(new float[] { 2, 1 });
      for (Int32 i = 1; i < 5; i++)
      {
        double Ord00 = mOrdenadaGrafico + mAltoGrafico * i / 5;
        await Contexto.MoveToAsync(mAbscisaGrafico, Ord00);
        await Contexto.LineToAsync(mAbscisaGrafico + mAnchoGrafico, Ord00);
      }
      await Contexto.StrokeAsync();

      await Contexto.ClosePathAsync();

    }

    private const Int32 CANT_FECHAS = 9;
    private async Task DibujarEscalaTAsync(Canvas2DContext Contexto)
    {
      // Obtener Fechas extremas.
      mFechaMaxima = FechaMaxima;
      if (mFechaMinima > FechaMinima)
      {
        mFechaMinima = FechaMinima;
      }

      Rutinas.CRutinas.AjustarExtremosFechas(ref mFechaMinima, ref mFechaMaxima);
      Rutinas.CRutinas.SaltoEscalaFechas Salto = Rutinas.CRutinas.SaltoDesdeFrecuencia(Curvas[0].Indicador.Frecuencia);
      string FmtFecha = Rutinas.CRutinas.FormatoFechaPorSalto(Salto);
      string TextoFechaParaAncho = Rutinas.CRutinas.TextoFechaPorSalto(Salto);

      TextMetrics Dimension = await Contexto.MeasureTextAsync(TextoFechaParaAncho);

      await Contexto.SetFillStyleAsync("black");

      List<double> Abscisas = new List<double>();
      List<double> Fechas = (from A in Curvas[0].Alarmas
                             select A.FechaInicial.ToOADate()).ToList();
      for (Int32 i = 1; i < Curvas.Count; i++)
      {
        Fechas.AddRange((from A in Curvas[i].Alarmas
                         select A.FechaInicial.ToOADate()).ToList());
      }
      Fechas = (from F in Fechas
                orderby F
                select F).Distinct().ToList();

      double AbscSup = 0;
      double AbscInf = 0;
      bool bAbajo = true;
      foreach (double Fecha in Fechas)
      {
        if (Fecha == Fechas.Last() && mMaximoDerecha >= mMinimoDerecha)
        {
          bAbajo = true; // fuerza a que el valor de la derecha se imprima abajo.
        }

        double AbscFecha = mAbscisaGrafico + mAnchoGrafico * (Fecha - mFechaMinima.ToOADate()) /
            (mFechaMaxima.ToOADate() - mFechaMinima.ToOADate());
        double AbscAnt = (bAbajo ? AbscInf : AbscSup);
        if ((AbscAnt + Dimension.Width / 2) < AbscFecha)
        {
          Abscisas.Add(AbscFecha);
          string Aa = DateTime.FromOADate(Fecha).ToString(FmtFecha);
          TextMetrics DimensionFecha = await Contexto.MeasureTextAsync(Aa);
          await Contexto.FillTextAsync(Aa, AbscFecha - DimensionFecha.Width / 2, mOrdenadaFechas + (bAbajo ? DimensionCaracter : 0));
          if (bAbajo)
          {
            AbscInf = AbscFecha + DimensionFecha.Width / 2;
          }
          else
          {
            AbscSup = AbscFecha + DimensionFecha.Width / 2;
          }
          bAbajo = !bAbajo;
        }
      }

      await Contexto.BeginPathAsync();

      await Contexto.SetLineWidthAsync(1);
      await Contexto.SetStrokeStyleAsync(mbHayBandas ? "#000000" : "#c0c0c0");
      await Contexto.SetLineDashAsync(new float[] { 2, 1 });

      foreach (double Absc in Abscisas)
      {
        if (Absc != mAbscisaGrafico && Absc != (mAbscisaGrafico + mAnchoGrafico))
        {
          await Contexto.MoveToAsync(Absc, mOrdenadaGrafico);
          await Contexto.LineToAsync(Absc, mOrdenadaGrafico + mAltoGrafico);
        }
      }

      await Contexto.StrokeAsync();

      await Contexto.ClosePathAsync();

    }

    private double AbscisaPeriodo(Int32 Pos)
    {
      return mAbscisaGrafico +
          SEP_GRUPOS_BARRAS + Pos * (Curvas.Count * (ANCHO_BARRA + SEP_ENTRE_BARRAS) - SEP_ENTRE_BARRAS + SEP_GRUPOS_BARRAS);
    }

    public const double ANCHO_BARRA = 15;
    public const double SEP_GRUPOS_BARRAS = 15;
    public const double SEP_ENTRE_BARRAS = 2;
    public const double ANCHO_PREVISTO_ESCALA = 90;

    private async Task DibujarEscalaTBarrasAsync(Canvas2DContext Contexto)
    {
      string FmtFecha = Rutinas.CRutinas.FormatoFechaPorSalto(mSalto);
      string TextoFechaParaAncho = Rutinas.CRutinas.TextoFechaPorSalto(mSalto);

      TextMetrics Dimension = await Contexto.MeasureTextAsync(TextoFechaParaAncho);

      await Contexto.SetFillStyleAsync("black");

      Int32 Pos = 0;
      foreach (Clases.CPeriodoT Periodo in Periodos)
      {
        double AbscFecha = AbscisaPeriodo(Pos++) + (Curvas.Count * (ANCHO_BARRA + SEP_ENTRE_BARRAS) - SEP_ENTRE_BARRAS) / 2;
        string Aa = Periodo.Desde.ToString(FmtFecha);
        TextMetrics DimensionFecha = await Contexto.MeasureTextAsync(Aa);
        await Contexto.FillTextAsync(Aa, AbscFecha - DimensionFecha.Width / 2, mOrdenadaFechas);
      }
    }


    private void AjustarHayBandas()
    {
      mbHayBandas = false;
      mbEscalaCreciente = true;
      mbEscalaDerCreciente = true;
      if (Curvas != null)
      {
        foreach (Clases.CCurvaTendencia Curva in Curvas)
        {
          foreach (CInformacionAlarmaCN Punto in Curva.Alarmas)
          {
            if (Punto.Sobresaliente != Punto.Minimo)
            {
              if (Curva.IndicadorBase)
              {
                mbHayBandas = true;
              }
              if (Curva.IndicadorBase || !Curva.EscalaDerecha)
              {
                mbEscalaCreciente = (Punto.Sobresaliente >= Punto.Minimo);
              }
              else
              {
                mbEscalaDerCreciente = (Punto.Sobresaliente >= Punto.Minimo);
              }
              break;
            }
          }
        }
      }
    }

    public double OrdenadaValor(double Valor, bool EscalaDerecha = false)
    {
      if (EscalaDerecha)
      {
        return mOrdenadaGrafico + mAltoGrafico * (mbEscalaDerCreciente ? (mMaximoDerecha - Valor) : (Valor - mMinimoDerecha)) /
            (mMaximoDerecha - mMinimoDerecha);
      }
      else
      {
        return mOrdenadaGrafico + mAltoGrafico *
          (mbEscalaCreciente || MesesAgrupacionBarras > 0 ? (mMaximo - Valor) : (Valor - mMinimo)) / (mMaximo - mMinimo);
      }
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

    public double AbscisaFecha(DateTime Fecha)
    {
      if (mFechaMinima.Year > 1800 && mFechaMaxima > mFechaMinima)
      {
        return mAbscisaGrafico + mAnchoGrafico *
          (Fecha.ToOADate() - mFechaMinima.ToOADate()) /
          (mFechaMaxima.ToOADate() - mFechaMinima.ToOADate());
      }
      else
      {
        return mAbscisaGrafico + mAnchoGrafico / 2;
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
      Desde = (Curvas[0].Alarmas.Count == 1 ? 0 : double.NaN);
      Hasta = double.NaN;
      OrdIni = (Curvas[0].Alarmas.Count == 1 ?
          OrdenadaBanda(Curvas[0].Alarmas[0], Posicion) : double.NaN);
      if (!bInvertido) // recorre el listado hacia adelante.
      {
        foreach (CInformacionAlarmaCN Dato in Curvas[0].Alarmas)
        {
          ProcesarElemento(Dato, ref Lista,
              ref Desde, ref Hasta, ref OrdIni, Posicion);
        }
      }
      else
      {
        for (Int32 i = Curvas[0].Alarmas.Count - 1; i >= 0; i--)
        {
          ProcesarElemento(Curvas[0].Alarmas[i], ref Lista,
              ref Desde, ref Hasta, ref OrdIni, Posicion);
        }
      }
    }

    public static string ColorZonaAzul(byte Opacidad = 255)
    {
      //      return Color.FromArgb(Opacidad, 212, 232, 255);
      return "#c0c7ff"; // Color.FromArgb(Opacidad, 192, 199, 255);
    }

    public static string ColorZonaVerde()
    {
      return "#a7daa7"; // Color.FromArgb(Opacidad, 167, 218, 167);
    }

    public static string ColorZonaAmarilla()
    {
      return "#ffff74"; // Color.FromArgb(Opacidad, 255, 255, 119); // 228, 233,116);
    }

    public static string ColorZonaRoja()
    {
      return "#ff7d7d"; // Color.FromArgb(Opacidad, 255, 125, 125); // 179, 179);
    }

    public static async Task AgregarZonaPuntosAsync(Canvas2DContext Contexto, List<CPunto> Puntos,
          string ColorFondo, double OrdenadaMinima, double OrdenadaMaxima)
    {
      await Contexto.BeginPathAsync();
      await Contexto.SetFillStyleAsync(ColorFondo);

      foreach (CPunto Punto in Puntos)
      {
        Punto.Ordenada = Math.Min(OrdenadaMaxima, Math.Max(Punto.Ordenada, OrdenadaMinima));
      }

      await Contexto.MoveToAsync(Puntos[0].Abscisa, Puntos[0].Ordenada);

      foreach (CPunto Punto in Puntos)
      {
        await Contexto.LineToAsync(Punto.Abscisa, Punto.Ordenada);
      }

      if (Puntos.Last().Abscisa!=Puntos[0].Abscisa || Puntos.Last().Ordenada != Puntos[0].Ordenada)
      {
        await Contexto.LineToAsync(Puntos.Last().Abscisa, Puntos.Last().Ordenada);
      }

      await Contexto.FillAsync();

      await Contexto.ClosePathAsync();

    }

    private async Task DibujarShapeInferiorAsync(Canvas2DContext Contexto)
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

        await AgregarZonaPuntosAsync(Contexto, Puntos, mbEscalaCreciente ? ColorZonaRoja() : ColorZonaAzul(),
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

        await AgregarZonaPuntosAsync(Contexto, Puntos,
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

        await AgregarZonaPuntosAsync(Contexto, Puntos, mbEscalaCreciente ? ColorZonaAzul() : ColorZonaRoja(),
             mOrdenadaGrafico, mOrdenadaGrafico + mAltoGrafico);

      }

    }

    private bool mbHayBandas;
    private bool mbEscalaCreciente;
    private bool mbEscalaDerCreciente;

    private async Task DibujarZonasLlenasAsync(Canvas2DContext Contexto)
    {

      if (!mbHayBandas)
      {
        return;
      }

      await DibujarShapeInferiorAsync(Contexto);
      if (!mbEscalaCreciente)
      {
        await DibujarShapeIntermedioAsync(Contexto, 3, 2,
            ColorZonaVerde());
        await DibujarShapeIntermedioAsync(Contexto, 2, 1,
            ColorZonaAmarilla());
      }
      else
      {
        await DibujarShapeIntermedioAsync(Contexto, 1, 2,
            ColorZonaAmarilla());
        await DibujarShapeIntermedioAsync(Contexto, 2, 3,
            ColorZonaVerde());
      }
      await DibujarShapeSuperiorAsync(Contexto);
    }

    private List<CPunto> mPuntosValoresTendencia = null;
    private Int32 mPosValorSeleccionado = -1;

    private async Task AjustarLineaCurvaAsync(Canvas2DContext Contexto, Clases.CCurvaTendencia Curva, string ColorCurva)
    {
      await Contexto.SetStrokeStyleAsync(ColorCurva);
      float[] Saltos;
      if (Curva.IndicadorBase)
      {
        Saltos = new float[0];
      }
      else
      {
        Saltos = new float[] { 2, 2 };
      }
      await Contexto.SetLineDashAsync(Saltos);

      await Contexto.SetLineWidthAsync(Curva.IndicadorBase ? 3 : 2);

    }

    public List<CPunto> PuntosTendencia { get { return mPuntosValoresTendencia; } }

    public const double SEMIDIAMETRO = 4;
    public const double SEMIDIAMETRO2 = 16;

    private async Task DibujarCurvaAsync(Canvas2DContext Contexto, Clases.CCurvaTendencia Curva, string ColorCurva)
    {
      if (Curva.Alarmas.Count > 0)
      {

        bool EscalaDerecha = (!Curva.IndicadorBase && Curva.EscalaDerecha);

        await Contexto.BeginPathAsync();

        await AjustarLineaCurvaAsync(Contexto, Curva, ColorCurva);

        bool bPrimerPunto = true;

        foreach (CInformacionAlarmaCN Dato in Curva.Alarmas)
        {
          if (bPrimerPunto)
          {
            await Contexto.MoveToAsync(AbscisaFecha(Dato.FechaInicial), OrdenadaValor(Dato.Valor, EscalaDerecha));
            bPrimerPunto = false;
          }
          else
          {
            await Contexto.LineToAsync(AbscisaFecha(Dato.FechaInicial), OrdenadaValor(Dato.Valor, EscalaDerecha));
          }
        }

        await Contexto.StrokeAsync();

        await Contexto.ClosePathAsync();

      }

      // Si es la curva principal, agrega los puntos.
      if (Curva.IndicadorBase)
      {
        // Por defecto elige el ultimo valor.
        if (mPosValorSeleccionado < 0 || mPosValorSeleccionado >= Curva.Alarmas.Count)
        {
          mPosValorSeleccionado = Curva.Alarmas.Count - 1;
        }

        mPuntosValoresTendencia = new List<CPunto>();
        Int32 Pos = 0;
        foreach (CInformacionAlarmaCN Dato in Curva.Alarmas)
        {
          await Contexto.BeginPathAsync();
          await Contexto.SetStrokeStyleAsync("#000000");
          await Contexto.SetLineWidthAsync(1);
          CPunto Punto = new CPunto(AbscisaFecha(Dato.FechaInicial), OrdenadaValor(Dato.Valor, false));
          mPuntosValoresTendencia.Add(Punto);
          if (Pos == mPosValorSeleccionado)
          {
            await Contexto.SetFillStyleAsync("#ff0000");
          }
          else
          {
            await Contexto.SetFillStyleAsync("#c0c0c0");
          }
          await Contexto.ArcAsync(Punto.Abscisa, Punto.Ordenada, SEMIDIAMETRO, 0, 2 * Math.PI);
          await Contexto.FillAsync();
          await Contexto.StrokeAsync();
          await Contexto.ClosePathAsync();
          Pos++;
        }
      }

      // Si corresponde agrega las etiquetas.
      if (MostrarEtiquetas)
      {
        foreach (CInformacionAlarmaCN Dato in Curva.Alarmas)
        {
          string Texto = Rutinas.CRutinas.ValorATexto(Dato.Valor, Curva.Indicador.Decimales);
          TextMetrics Dimensiones = await Contexto.MeasureTextAsync(Texto);
          double AnchoRect = Dimensiones.Width + 4;
          double AltoRect = DimensionCaracter + 4;
          double Abscisa = Math.Max(mAbscisaGrafico,
              Math.Min(mAbscisaGrafico + mAnchoGrafico - AnchoRect, AbscisaFecha(Dato.FechaInicial) - AnchoRect / 2));
          double Ordenada = OrdenadaValor(Dato.Valor, Curva.EscalaDerecha) + 4;
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

    private const double SEP_REFERENCIAS_H = 10;
    private async Task DibujarReferenciasAsync(Canvas2DContext Contexto)
    {

      Int32 Pos = 0;
      foreach (Clases.CCurvaTendencia Curva in Curvas)
      {
        double Abscisa = (Pos % mColumnasReferencia) * AnchoContenedor / mColumnasReferencia + SEP_REFERENCIAS_H;
        double Ordenada = (SEP_REFERENCIAS_V + DimensionCaracter) *
            (Pos + mColumnasReferencia - Pos % mColumnasReferencia) / mColumnasReferencia;
        string Color = Rutinas.CRutinas.ColorOrden(Pos);
        double AnchoDisponible = Ancho / mColumnasReferencia - 2 * SEP_REFERENCIAS_H;
        await Contexto.SetFillStyleAsync(Color);
        if (MesesAgrupacionBarras < 1)
        {
          await Contexto.BeginPathAsync();
          await AjustarLineaCurvaAsync(Contexto, Curva, Color);
          await Contexto.MoveToAsync(Abscisa, Ordenada - DimensionCaracter / 2);
          await Contexto.LineToAsync(Abscisa + 2 * DimensionCaracter, Ordenada - DimensionCaracter / 2);
          await Contexto.StrokeAsync();
          await Contexto.ClosePathAsync();
        }
        else
        {
          await Contexto.FillRectAsync(Abscisa, Ordenada - DimensionCaracter, 2 * DimensionCaracter, DimensionCaracter);
        }
        await Contexto.FillTextAsync(Curva.Indicador.Descripcion, Abscisa + 2.5 * DimensionCaracter, Ordenada,
            AnchoDisponible - 2.5 * DimensionCaracter - SEP_REFERENCIAS_H);
        Pos++;
      }

    }

    private async Task PonerEtiquetaBarraAsync(Canvas2DContext Contexto,
          double Abscisa, double Ordenada, double Valor, Int32 Decimales)
    {
      string Texto = Rutinas.CRutinas.ValorATexto(Valor, Decimales);
      await Contexto.SaveAsync();
      await Contexto.TranslateAsync(Abscisa + ANCHO_BARRA - (ANCHO_BARRA - DimensionCaracter) / 2, Ordenada);
      await Contexto.RotateAsync(4.712f); // 270); // (float)(1.5*Math.PI));
      await Contexto.SetFillStyleAsync("#000000");
      await Contexto.FillTextAsync(Texto, 0, 0);
      await Contexto.RestoreAsync();
    }

    private async Task DibujarBarrasAsync(Canvas2DContext Contexto)
    {

      Int32 Pos = 0;
      foreach (Clases.CPeriodoT Periodo in Periodos)
      {
        double Abscisa = AbscisaPeriodo(Pos);
        Int32 PosCurva = 0;
        foreach (Clases.CCurvaTendencia Curva in Curvas)
        {
          Clases.DatosPeriodo Datos = (from D in Periodo.Datos
                                       where D.Orden == PosCurva
                                       select D).FirstOrDefault();
          if (Datos != null)
          {
            await Contexto.SetFillStyleAsync(Rutinas.CRutinas.ColorOrden(PosCurva));
            double Ordenada = OrdenadaValor(Datos.Valor, false);
            double OrdEtiquetas = 0;
            if (Datos.Valor < 0 && mMaximo > 0)
            {
              double Ord0 = OrdenadaValor(0, false);
              OrdEtiquetas = Ord0;
              await Contexto.FillRectAsync(Abscisa, Ord0,
                  ANCHO_BARRA, Ordenada - Ord0);
            }
            else
            {
              if (Datos.Valor >= 0 && mMinimo < 0)
              {
                double Ord0 = OrdenadaValor(0, false);
                OrdEtiquetas = Ordenada;
                await Contexto.FillRectAsync(Abscisa, Ordenada,
                    ANCHO_BARRA, Ord0 - Ordenada);
              }
              else
              {
                OrdEtiquetas = Ordenada;
                await Contexto.FillRectAsync(Abscisa, Ordenada,
                    ANCHO_BARRA, mOrdenadaGrafico + mAltoGrafico - Ordenada);
              }
            }

            if (MostrarEtiquetas)
            {
              await PonerEtiquetaBarraAsync(Contexto, Abscisa, OrdEtiquetas, Datos.Valor, Curva.Indicador.Decimales);
            }

          }
          Abscisa += ANCHO_BARRA + SEP_ENTRE_BARRAS;
          PosCurva++;
        }
        Pos++;
      }

    }

    private Clases.CPeriodoT ObtenerPeriodo(DateTime Fecha, ref List<Clases.CPeriodoT> Periodos)
    {
      switch (MesesAgrupacionBarras)
      {
        case 12:
          DateTime Inicio = Rutinas.CRutinas.FechaInicioAnio(Fecha);
          foreach (Clases.CPeriodoT Periodo in Periodos)
          {
            if (Periodo.Desde.Year == Inicio.Year)
            {
              return Periodo;
            }
          }
          Clases.CPeriodoT PeriodoL = new Clases.CPeriodoT()
          {
            Desde = Rutinas.CRutinas.FechaInicioAnio(Fecha),
            Hasta = Rutinas.CRutinas.FechaInicioAnio(Fecha).AddYears(1).AddSeconds(-1)
          };
          Periodos.Add(PeriodoL);
          return PeriodoL;
        default:
          foreach (Clases.CPeriodoT Periodo in Periodos)
          {
            if (Periodo.Desde<=Fecha && Periodo.Hasta >= Fecha)
            {
              return Periodo;
            }
          }
          DateTime FechaInicio = Rutinas.CRutinas.FechaInicioMes(Fecha);
          Int32 Mes = FechaInicio.Month - (FechaInicio.Month - 1) % MesesAgrupacionBarras;
          Clases.CPeriodoT PeriodoL2 = new Clases.CPeriodoT()
          {
            Desde = new DateTime(FechaInicio.Year, Mes, 1, 0, 0, 0),
            Hasta = new DateTime(FechaInicio.Year, Mes + MesesAgrupacionBarras, 1, 0, 0, 0).AddSeconds(-1)
          };
          Periodos.Add(PeriodoL2);
          return PeriodoL2;
      }
    }

    //private List<Clases.CPeriodoT> ObtenerDatosPorPeriodo()
    //{
    //  List<Clases.CPeriodoT> Respuesta = new List<Clases.CPeriodoT>();
    //  Int32 Pos = 0;
    //  foreach (Clases.CCurvaTendencia Curva in Curvas)
    //  {
    //    foreach (CInformacionAlarmaCN Dato in Curva.Alarmas)
    //    {
    //      Clases.CPeriodoT Punto = ObtenerPeriodo(Dato.FechaInicial, ref Respuesta);
    //      Punto.AgregarValor(Pos, Dato.Valor);
    //    }
    //    Pos++;
    //  }

    //  Respuesta.Sort(delegate (Clases.CPeriodoT P1, Clases.CPeriodoT P2)
    //  {
    //    return P1.Desde.CompareTo(P2.Desde);
    //  });

    //  return Respuesta;
    //}

    private async Task DibujarTendenciaUnicaAsync(Canvas2DContext Contexto)
    {
      AjustarHayBandas();
      await DibujarEscalaAsync(Contexto);
      await DibujarZonasLlenasAsync(Contexto);
      await DibujarEscalaTAsync(Contexto);
      await DibujarGrillaAsync(Contexto);
      await DibujarCurvaAsync(Contexto, Curvas[0], Rutinas.CRutinas.ColorOrden(0));
    }

    private async Task DibujarTendenciaMultipleAsync(Canvas2DContext Contexto)
    {
      if (MostrarReferencias)
      {
        await DibujarReferenciasAsync(Contexto);
      }
      await DibujarEscalaAsync(Contexto);
      await DibujarEscalaDerechaAsync(Contexto);
      await DibujarEscalaTAsync(Contexto);
      await DibujarGrillaAsync(Contexto);
      Int32 Pos = 0;
      foreach (Clases.CCurvaTendencia Curva in Curvas)
      {
        await DibujarCurvaAsync(Contexto, Curva, Rutinas.CRutinas.ColorOrden(Pos++));
      }
    }

    public List<Clases.CPeriodoT> Periodos { get; set; }

    private async Task DibujarBarrasAgrupadasAsync(Canvas2DContext Dibujador)
    {
      await DeterminarDimensionesAsync(Dibujador, -1);
      await DibujarEscalaAsync(Dibujador);
      await DibujarEscalaTBarrasAsync(Dibujador);
      await DibujarGrillaAsync(Dibujador);
      await DibujarBarrasAsync(Dibujador);
      if (MostrarReferencias)
      {
        await DibujarReferenciasAsync(Dibujador);
      }
    }

    public async Task<double> DibujarTendenciasAsync(Canvas2DContext Contexto, double AnchoEscala)
    {

      await Contexto.ClearRectAsync(0, 0, Ancho, Alto);

      await Contexto.SetFillStyleAsync("white");
      
      await Contexto.FillRectAsync(0, 0, Ancho, Alto);

      await Contexto.SetFontAsync("10px serif");

      await DeterminarDimensionesAsync(Contexto, AnchoEscala);

      if (Curvas == null || Curvas.Count == 1)
      {
        await DibujarTendenciaUnicaAsync(Contexto);
      }
      else
      {
        if (MesesAgrupacionBarras < 0)
        {
          await DibujarTendenciaMultipleAsync(Contexto);
        }
        else
        {
          await DibujarBarrasAgrupadasAsync(Contexto);
        }
      }
      return mAnchoEscala;
    }

   }
}
