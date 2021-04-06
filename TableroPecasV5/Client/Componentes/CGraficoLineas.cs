using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Extensions.Canvas.Model;
using Blazor.Extensions.Canvas.Canvas2D;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Componentes
{
  public class CGraficoLineas
  {

    private List<CDatosPuntoReal> mPuntos;
    private double mMinimo;
    private double mMaximo;
    private double mMinimoAbsc;
    private double mMaximoAbsc;
    private double mAnchoEscala;
    private double mDimensionCaracter;
    private double mAnchoGrafico;
    private double mAnchoComponente;
    private double mAltoComponente;
    private double mAbscisaGrafico;
    private double mOrdenadaGrafico;
    private double mAltoGrafico;

    public async Task HacerGraficoLineasAsync(Canvas2DContext Contexto, double AnchoComponente, double AltoComponente)
    {
      if (mPuntos != null && mPuntos.Count > 0)
      {
        // Limpiar.
        double AnchoLimpiar = Math.Max(AnchoComponente, mAnchoGrafico + mAnchoEscala + 1.5 * mDimensionCaracter);
        await Contexto.ClearRectAsync(0, 0, AnchoLimpiar, AltoComponente);
        await Contexto.SetFillStyleAsync("white");
        await Contexto.FillRectAsync(0, 0, AnchoLimpiar, AltoComponente);

        mAnchoComponente = AnchoComponente;
        mAltoComponente = AltoComponente;

        ObtenerDimensionesGrafico();
        await DibujarEscalaAsync(Contexto);
        await DibujarEscalaAbscisasAsync(Contexto);
        await DibujarGrillaAsync(Contexto);
        await DibujarPuntosAsync(Contexto);
      }
    }

    private double AbscisaValor(double Valor)
    {
      return mAbscisaGrafico + mAnchoGrafico * (Valor - mMinimoAbsc) / (mMaximoAbsc - mMinimoAbsc);
    }

    private async Task DibujarPuntosAsync(Canvas2DContext Contexto)
    {
      await Contexto.BeginPathAsync();

      await Contexto.SetLineWidthAsync(2);
      await Contexto.SetStrokeStyleAsync("gray");
      float[] Saltos = new float[0];
      await Contexto.SetLineDashAsync(Saltos);

      bool bPrimerPunto = true;

      foreach (CDatosPuntoReal Punto in mPuntos)
      {
        if (bPrimerPunto)
        {
          await Contexto.MoveToAsync(AbscisaValor(Punto.Abscisa), OrdenadaValor(Punto.Valor));
          bPrimerPunto = false;
        }
        else
        {
          await Contexto.LineToAsync(AbscisaValor(Punto.Abscisa), OrdenadaValor(Punto.Valor));
        }
      }

      await Contexto.StrokeAsync();

      await Contexto.ClosePathAsync();


      foreach (CDatosPuntoReal Punto in mPuntos)
      {
        await Contexto.BeginPathAsync();
        await Contexto.SetStrokeStyleAsync("#000000");
        await Contexto.SetFillStyleAsync("#c0c0c0");
        await Contexto.SetLineWidthAsync(1);
        await Contexto.ArcAsync(AbscisaValor(Punto.Abscisa), OrdenadaValor(Punto.Valor), 3, 0, 2 * Math.PI);
        await Contexto.FillAsync();
        await Contexto.StrokeAsync();
        await Contexto.ClosePathAsync();
      }

    }

    public double OrdenadaValor(double Valor)
    {
      return mOrdenadaGrafico + mAltoGrafico * (mMaximo - Valor) / (mMaximo - mMinimo);
    }

    private async Task DibujarEscalaAsync(Canvas2DContext Contexto)
    {
      await Contexto.SetFillStyleAsync("black");
      string Fmt = Rutinas.CRutinas.FormatoPorSalto(mMinimo, mMaximo);
      for (Int32 i = 0; i <= 5; i++)
      {
        double Valor = mMaximo - (mMaximo - mMinimo) * i / 5;
        string Aa = Valor.ToString(Fmt);
        TextMetrics Dimensiones = await Contexto.MeasureTextAsync(Aa);
        await Contexto.FillTextAsync(Aa, mAbscisaGrafico - mDimensionCaracter / 2 - Dimensiones.Width,
           OrdenadaValor(Valor) + mDimensionCaracter / 2);
      }
    }

    private async Task DibujarEscalaAbscisasAsync(Canvas2DContext Contexto)
    {
      for (Int32 i = 0; i <= 5; i++)
      {
        double Valor = mMinimoAbsc + (mMaximoAbsc - mMinimoAbsc) * i / 5;
        string Aa = CRutinas.ValorATexto(Valor, 3);
        TextMetrics Dimensiones = await Contexto.MeasureTextAsync(Aa);
        await Contexto.FillTextAsync(Aa,
          (i == 5 ? (mAbscisaGrafico + mAnchoGrafico - Dimensiones.Width) :
              (mAbscisaGrafico + mAnchoGrafico * i / 5 - Dimensiones.Width / 2)),
          mOrdenadaGrafico + mAltoGrafico + ((i % 2) == 0 ? (3.5 * mDimensionCaracter + 2) : (1.5 * mDimensionCaracter)));
      }
    }

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
      await Contexto.SetStrokeStyleAsync("#c0c0c0");

      await Contexto.SetLineDashAsync(new float[] { 2, 1 });
      for (Int32 i = 1; i < 5; i++)
      {
        double Ord00 = mOrdenadaGrafico + mAltoGrafico * i / 5;
        await Contexto.MoveToAsync(mAbscisaGrafico, Ord00);
        await Contexto.LineToAsync(mAbscisaGrafico + mAnchoGrafico, Ord00);
      }

      for (Int32 i = 1; i < 5; i++)
      {
        double Absc0 = mAbscisaGrafico + mAnchoGrafico * i / 5;
        await Contexto.MoveToAsync(Absc0, mOrdenadaGrafico);
        await Contexto.LineToAsync(Absc0, mOrdenadaGrafico + mAltoGrafico);
      }

      await Contexto.StrokeAsync();

      await Contexto.ClosePathAsync();

    }

    private void ObtenerDimensionesGrafico()
    {
      mAbscisaGrafico = mAnchoEscala + mDimensionCaracter;
      mOrdenadaGrafico = mDimensionCaracter / 2;
      //      mAnchoGrafico = mAnchoComponente - mAbscisaGrafico - mDimensionCaracter;
      mAltoGrafico = mAltoComponente - 4.5 * mDimensionCaracter - 2;
    }

    private string FmtTexto(long AltoTotal)
    {
      return (AltoTotal > 400 ? "12" : "10") + "px serif";
    }

    private void ObtenerExtremosEscala()
    {
      mMinimo = mMinimoAbsc= double.MaxValue;
      mMaximo =mMaximoAbsc= double.MinValue;

      foreach (CDatosPuntoReal Punto in mPuntos)
      {
        mMinimo = Math.Min(mMinimo, Punto.Valor);
        mMaximo = Math.Max(mMaximo, Punto.Valor);
        mMinimoAbsc = Math.Min(mMinimoAbsc, Punto.Abscisa);
        mMaximoAbsc = Math.Max(mMaximoAbsc, Punto.Abscisa);
      }

      CRutinas.AjustarExtremosEscala(ref mMinimo, ref mMaximo);
      CRutinas.AjustarExtremosEscala(ref mMinimoAbsc, ref mMaximoAbsc);

    }

    private async Task<double> DeterminarAnchoEscalaAsync(Canvas2DContext Contexto)
    {
      TextMetrics M = await Contexto.MeasureTextAsync("H");
      mDimensionCaracter = M.Width;

      string Fmt = Rutinas.CRutinas.FormatoPorSalto(mMinimo, mMaximo);
      TextMetrics MedidasMax = await Contexto.MeasureTextAsync(mMaximo.ToString(Fmt));
      TextMetrics MedidasMin = await Contexto.MeasureTextAsync(mMinimo.ToString(Fmt));

      return Math.Max(MedidasMax.Width, MedidasMin.Width);

    }

    public async Task<long> DeterminarAnchoNecesarioAsync(Canvas2DContext Contexto, List<CDatosPuntoReal> Puntos,
        long AnchoTotal, long AltoTotal)
    {
      if (Puntos != null && Puntos.Count > 0)
      {
        mPuntos = Puntos;
        await Contexto.SetFontAsync(FmtTexto(AltoTotal));
        ObtenerExtremosEscala();
        mAnchoEscala = await DeterminarAnchoEscalaAsync(Contexto);
        mAnchoGrafico = AnchoTotal - mAnchoEscala - 1.5 * mDimensionCaracter;
        return AnchoTotal;
      }
      else
      {
        return -1;
      }
    }


  }
}
