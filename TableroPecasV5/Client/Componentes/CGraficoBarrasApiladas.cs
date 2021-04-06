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
  public class CGraficoBarrasApiladas
  {

    private double mDimensionCaracter;
    private double mDimensionCaracterDer;
    private double mAltoComponente;
    private double mAltoComponenteDer;
    private double mDimensionReferenciaMax;
    private double mDimensionReferenciaMaxDer;
    private double mAnchoValores;
    private bool mbPuedeGraficar = false;
    private List<double> mDimensionReferencias;
    public const Int32 ALTO_COLUMNA = 25;
    public const Int32 ALTO_ENTRE_COLUMNAS = 10;
    private const Int32 SEP_COLUMNA = 5;
    private const Int32 SEP_SUB_FILAS = 2;

    private List<DatosBarraApilada> mDatos;
    public List<ElementoPila> mSubDatos;
    private bool mbScroll = false;
    private double mAnchoTexto;
    private double mAnchoBarras;

    public static Int32 AltoNecesario(Int32 Cantidad)
    {
      return Cantidad * (ALTO_COLUMNA + ALTO_ENTRE_COLUMNAS) + ALTO_ENTRE_COLUMNAS;
    }

    public static Int32 AltoNecesario(List<DatosBarraApilada> Datos)
    {
      Int32 Respuesta = 0;
      foreach (DatosBarraApilada Dato in Datos)
      {
        Respuesta += 2 * ALTO_ENTRE_COLUMNAS + ALTO_COLUMNA + Dato.Elementos.Count * (ALTO_COLUMNA + SEP_SUB_FILAS);
      }
      return Respuesta;
    }

    private async Task ImprimirTextoAsync(Canvas2DContext Contexto, string Texto, double Abscisa, double Ordenada,
        double Disponible, double Necesario, bool Principal)
    {
      string TextoLocal = TruncarTexto(Texto, Necesario, Disponible);
      await Contexto.SetFillStyleAsync((Principal ? "#000000" : "gray"));
      if (TextoLocal.Length < Texto.Length)
      {
        await Contexto.FillTextAsync(TextoLocal, Abscisa, Ordenada + (ALTO_COLUMNA + mDimensionCaracter) / 2, Disponible);
      }
      else
      {
        await Contexto.FillTextAsync(TextoLocal, Abscisa, Ordenada + (ALTO_COLUMNA + mDimensionCaracter) / 2);
      }

    }

    private async Task<double> DibujarBarraDetalladaAsync(Canvas2DContext Contexto, DatosBarraApilada Dato, Int32 Pos,
        double Ordenada, double Maximo)
    {
      if (Maximo > 0 && Dato.ValorTotal>=0)
      {
        Ordenada += ALTO_ENTRE_COLUMNAS;
        // Texto.
        await ImprimirTextoAsync(Contexto, Dato.Nombre, SEP_COLUMNA, Ordenada, mAnchoTexto, mDimensionReferencias[Pos], true);

        // Barra.
        double AbscisaBarra = 2 * SEP_COLUMNA + mAnchoTexto;
        await Contexto.SetFillStyleAsync("green");
        double Ancho = mAnchoBarras * Dato.ValorTotal / Maximo;
        await Contexto.FillRectAsync(AbscisaBarra, Ordenada, Ancho, ALTO_COLUMNA);

        // Valor.
        string Texto = CRutinas.ValorATexto(Dato.ValorTotal, 3);
        AbscisaBarra += Ancho+ SEP_COLUMNA;
        await Contexto.FillTextAsync(Texto, AbscisaBarra, Ordenada + (ALTO_COLUMNA + mDimensionCaracter) / 2);

        Ordenada += ALTO_COLUMNA + SEP_SUB_FILAS;

        // Dependientes.
        await Contexto.SetFillStyleAsync("gray");
        foreach (DatosElementoPila Elemento in Dato.Elementos)
        {
          Ancho = mAnchoBarras * Elemento.Valor / Maximo;
          if (Ancho > 0)
          {
            ElementoPila ElDatos = (from E in mSubDatos
                                    where E.Nombre == Elemento.Nombre
                                    select E).FirstOrDefault();
            if (ElDatos != null)
            {
              await ImprimirTextoAsync(Contexto, Elemento.Nombre, 2 * SEP_COLUMNA,
                  Ordenada, mAnchoTexto - SEP_COLUMNA, ElDatos.Ancho, false);
              string ColorElemento = ElDatos.Pincel;
              await Contexto.SetFillStyleAsync(ColorElemento);
              await Contexto.FillRectAsync(2 * SEP_COLUMNA + mAnchoTexto, Ordenada, Ancho, ALTO_COLUMNA);
              await ImprimirTextoAsync(Contexto, CRutinas.ValorATexto(Elemento.Valor, 3), 3 * SEP_COLUMNA + mAnchoTexto + Ancho,
                 Ordenada, 999, 0, false);
              Ordenada += ALTO_COLUMNA + SEP_SUB_FILAS;
            }
          }
        }

        return Ordenada + ALTO_ENTRE_COLUMNAS;

      }
      else
      {
        return Ordenada;
      }
    }

    private async Task DibujarBarrasDetalladasAsync(Canvas2DContext Contexto)
    {
      double Ordenada = 0;
      Int32 Pos = 0;
      double AcumMax = Math.Max(0, (from D in mDatos
                                    select D.ValorTotal).Max());
      foreach (DatosBarraApilada Dato in mDatos)
      {
        Ordenada= await DibujarBarraDetalladaAsync(Contexto, Dato, Pos, Ordenada, AcumMax);
      }
    }

    public async Task HacerGraficoBarrasAsync(Canvas2DContext Contexto, Canvas2DContext ContextoDer,
          double AnchoComponente, double AltoComponente, List<DatosBarraApilada> Apiladas,
          List<ElementoPila> Pilas, bool Detallado)
    {
      mDatos = Apiladas;
      mSubDatos = Pilas;
      if (mDatos != null && mDatos.Count > 0)
      {
        // Limpiar.
        // Determinar ancho columnas.

        mDimensionCaracter = await DeterminarAltoCaracterAsync(Contexto,
            Math.Min(AnchoComponente * (Detallado ? 1 : 0.67), AltoComponente));

        mAltoComponente = DeterminarAltoComponente(Detallado);
        if (!Detallado)
        {
          mAltoComponenteDer = await DeterminarAltoComponenteDerAsync(ContextoDer, Math.Min(AnchoComponente * 0.33, AltoComponente));
        }
        // Ajusta alto.
        //mAltoComponente = mDatos.Count * (mAltoColumna + ALTO_ENTRE_COLUMNAS);
        //mAltoComponenteDer = mSubDatos.Count * (mAltoColumna + ALTO_ENTRE_COLUMNAS);

        ImponerColoresDer();

        await ObtenerDimensionesReferenciasAsync(Contexto);
        if (Detallado)
        {
          mDimensionReferenciaMax = Math.Max(mDimensionReferenciaMax, mDimensionReferenciaMaxDer);
        }

        // Obtener Dimensiones columnas y subcolumnas.
        ObtenerDimensionesColumnas(AnchoComponente, AltoComponente, Detallado);

        // Limpiar el fondo del grafico.
        if (Detallado)
        {
          await LimpiarFondoAsync(Contexto, AnchoComponente, mAltoComponente);
        }
        else
        {
          await LimpiarFondoAsync(Contexto, AnchoComponente * 0.67, mAltoComponente);
          await LimpiarFondoAsync(ContextoDer, AnchoComponente * 0.33, mAltoComponenteDer);
        }

        if (mbPuedeGraficar)
        {
          if (Detallado)
          {
            await DibujarBarrasDetalladasAsync(Contexto);
          }
          else
          {

            // Zona de barras.
            Int32 Pos = 0;
            double AcumMax = Math.Max(0, (from D in mDatos
                                          select D.ValorTotal).Max());
            foreach (DatosBarraApilada Dato in mDatos)
            {
              await DibujarBarraAsync(Contexto, Dato, Pos, AcumMax);
              Pos++;
            }

            Pos = 0;
            foreach (ElementoPila Dato in mSubDatos)
            {
              await DibujarReferenciaAsync(ContextoDer, Dato, Pos);
              Pos++;
            }
          }

        }

      }
    }

    private void ImponerColoresDer()
    {
      Int32 Pos = 0;
      foreach (ElementoPila Dato in mSubDatos)
      {
        Dato.Pincel = CRutinas.ColorOrden(Pos++);
      }
    }

    private async Task LimpiarFondoAsync(Canvas2DContext Contexto, double Ancho, double Alto)
    {
      await Contexto.ClearRectAsync(0, 0, Ancho, Alto);
      await Contexto.SetFillStyleAsync("white");
      await Contexto.FillRectAsync(0, 0, Ancho, Alto);
    }

    private string TruncarTexto(string Texto, double Largo, double Disponible)
    {
      if (Disponible >= Largo)
      {
        return Texto;
      }
      else
      {
        return Texto.Substring(0, (Int32)Math.Floor(Disponible * Texto.Length / Largo));
      }
    }

    private double OrdenadaPosicion(Int32 Pos)
    {
      return Pos * (ALTO_COLUMNA + ALTO_ENTRE_COLUMNAS) + ALTO_ENTRE_COLUMNAS;
    }

    private async Task DibujarBarraAsync(Canvas2DContext Contexto, DatosBarraApilada Dato, Int32 Pos, double Maximo)
    {
      if (Maximo > 0) {
        double Ordenada = OrdenadaPosicion(Pos);
        // Texto.
        string Texto = TruncarTexto(Dato.Nombre, mDimensionReferencias[Pos], mAnchoTexto);
        await Contexto.SetFillStyleAsync("#000000");
        if (Texto.Length < Dato.Nombre.Length)
        {
          await Contexto.FillTextAsync(Texto, SEP_COLUMNA, Ordenada + (ALTO_COLUMNA + mDimensionCaracter) / 2, mAnchoTexto);
        }
        else
        {
          await Contexto.FillTextAsync(Texto, SEP_COLUMNA, Ordenada + (ALTO_COLUMNA + mDimensionCaracter) / 2);
        }

        // Barra.
        double AbscisaBarra = 2 * SEP_COLUMNA + mAnchoTexto;
        foreach (DatosElementoPila Elemento in Dato.Elementos)
        {
          double Ancho = mAnchoBarras * Elemento.Valor / Maximo;
          if (Ancho > 0)
          {
            string ColorElemento = (from S in mSubDatos
                                    where S.Nombre == Elemento.Nombre
                                    select S.Pincel).FirstOrDefault();
            if (ColorElemento.Length == 0)
            {
              ColorElemento = "white";
            }
            await Contexto.SetFillStyleAsync(ColorElemento);
            await Contexto.FillRectAsync(AbscisaBarra, Ordenada, Ancho, ALTO_COLUMNA);
            AbscisaBarra += Ancho;
          }
        }

        // Valor.
        Texto = CRutinas.ValorATexto(Dato.ValorTotal, 3);
        AbscisaBarra += SEP_COLUMNA;
        await Contexto.SetFillStyleAsync("#000000");
        await Contexto.FillTextAsync(Texto, AbscisaBarra, Ordenada + (ALTO_COLUMNA + mDimensionCaracter) / 2);
      }
    }

    private async Task DibujarReferenciaAsync(Canvas2DContext Contexto, ElementoPila Dato, Int32 Pos)
    {
      double Ordenada = OrdenadaPosicion(Pos);
      // Rectangulo.
      await Contexto.SetFillStyleAsync(Dato.Pincel);
      await Contexto.FillRectAsync(SEP_COLUMNA, Ordenada, ALTO_COLUMNA, ALTO_COLUMNA);
      // Texto.
      await Contexto.SetFillStyleAsync("#000000");
      await Contexto.FillTextAsync(Dato.Nombre, 2 * SEP_COLUMNA + ALTO_COLUMNA, Ordenada + (ALTO_COLUMNA - mDimensionCaracter) / 2);
    }

    private void ObtenerDimensionesColumnas(double AnchoComponente, double AltoComponente, bool Detallado)
    {
      mbScroll = (mAltoComponente > AltoComponente);
      double AnchoDisponible = (Detallado ? 1 : 0.67) * AnchoComponente - (mbScroll ? CRutinas.ALTO_BARRA_SCROLL : 0);
      double AnchoNeto = AnchoDisponible - mAnchoValores - 4 * SEP_COLUMNA;
      if (AnchoNeto < 50)
      {
        mbPuedeGraficar = false;
      }
      else
      {
        mbPuedeGraficar = true;
        mAnchoTexto = Math.Min(AnchoNeto / 2, mDimensionReferenciaMax);
        mAnchoBarras = AnchoNeto - mAnchoTexto;
      }
    }

    private string FmtTexto(double AltoTotal)
    {
      return (AltoTotal > 400 ? "12" : "10") + "px serif";
    }

    private async Task<double> DeterminarAltoCaracterAsync(Canvas2DContext Contexto, double DimensionMinima)
    {
      await Contexto.SetFontAsync(FmtTexto(DimensionMinima));
      return (await Contexto.MeasureTextAsync("H")).Width;
    }

    private double DeterminarAltoComponente(bool Detallado)
    {
      if (Detallado)
      {
        return AltoNecesario(mDatos);
      }
      else
      {
        return AltoNecesario(mDatos.Count);
      }
    }

    private async Task<double> DeterminarAltoComponenteDerAsync(Canvas2DContext Contexto, double DimensionMinima)
    {
      await Contexto.SetFontAsync(FmtTexto(DimensionMinima));
      mDimensionCaracterDer = (await Contexto.MeasureTextAsync("H")).Width;
      return mSubDatos.Count *  (Math.Max(mDimensionCaracterDer, ALTO_COLUMNA) + ALTO_ENTRE_COLUMNAS);
    }

    private async Task ObtenerDimensionesReferenciasAsync(Canvas2DContext Contexto)
    {
      mDimensionReferencias = new List<double>();
      mAnchoValores = 0;
      foreach (DatosBarraApilada Dato in mDatos)
      {
        TextMetrics Medida = await Contexto.MeasureTextAsync(Dato.Nombre);
        mDimensionReferencias.Add(Medida.Width);
        mAnchoValores = Math.Max(mAnchoValores, (await Contexto.MeasureTextAsync(CRutinas.ValorATexto(Dato.ValorTotal, 3))).Width);
      }
      mDimensionReferenciaMax = (from M in mDimensionReferencias
                                 select M).Max();

      // Ahora las secundarias.
      foreach (ElementoPila Elemento in mSubDatos)
      {
        TextMetrics Medida = await Contexto.MeasureTextAsync(Elemento.Nombre);
        Elemento.Ancho = Medida.Width;
      }
      mDimensionReferenciaMaxDer = (from M in mSubDatos
                                 select M.Ancho).Max();

    }


  }

}
