using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Clases;

namespace TableroPecasV5.Client.Logicas
{
  public class CMapaCalorRN
  {

    public enum ModoUnir
    {
      Inicio = 1,
      Cierre = 2,
      No = -1
    }

    public CRespuestaCalor Respuesta { get; set; }
    public List<CPuntoCalor> Puntos { get; set; }

    public CMapaCalorRN()
    {
      Puntos = new List<CPuntoCalor>();
      Respuesta = new CRespuestaCalor();
    }

    private double ObtenerDistanciaEfecto()
    {
      //double DistBase = Math.Sqrt(4 * Respuesta.PixelsAncho * Respuesta.PixelsAlto / Puntos.Count); // para que haya una media de 4 puntos.
      double DimMedia = Math.Sqrt(Respuesta.PixelsAncho * Respuesta.PixelsAlto);
      return Respuesta.FactorDistancia * DimMedia / 10; // Math.Min(DimMedia / 2, Math.Max(DimMedia / 10, DistBase));
    }

    //    private List<float> mCantidadValores;
    private void CrearVectorDatos(bool Acumulado)
    {

      Respuesta.Valores.Clear();
      Respuesta.Abscisas = new List<double>();
      Respuesta.Ordenadas = new List<double>();
      double SaltoH = (Respuesta.AbscisaMaxima - Respuesta.AbscisaMinima) / Respuesta.SegmentosH;
      double SaltoV = (Respuesta.OrdenadaMaxima - Respuesta.OrdenadaMinima) / Respuesta.SegmentosV;

      for (Int32 i = 0; i <= Respuesta.SegmentosV; i++)
      {
        double Ord0 = Respuesta.OrdenadaMinima + SaltoV * i;
        for (Int32 ii = 0; ii <= Respuesta.SegmentosH; ii++)
        {
          Respuesta.Abscisas.Add(Respuesta.AbscisaMinima + ii * SaltoH);
          Respuesta.Ordenadas.Add(Ord0);
          Respuesta.Valores.Add(0);
        }
      }

      //if (!Acumulado)
      //{
      //  mCantidadValores = new List<float>();
      //  for (Int32 i = 0; i < Respuesta.SegmentosH * Respuesta.SegmentosV; i++)
      //  {
      //    mCantidadValores.Add(0);
      //  }
      //}
    }

    private Int32 ObtenerPosicion(double Valor, double Minimo, double Maximo, Int32 Divisiones)
    {
      return (Int32)Math.Floor((double)(Divisiones - 1) * (Valor - Minimo) / (Maximo - Minimo) + 0.5);
    }

    private void UbicarPosicionGrilla(double Abscisa, double Ordenada, out Int32 Columna, out Int32 Fila)
    {
      Columna = ObtenerPosicion(Abscisa, Respuesta.AbscisaMinima, Respuesta.AbscisaMaxima, Respuesta.SegmentosH);
      Fila = ObtenerPosicion(Ordenada, Respuesta.OrdenadaMinima, Respuesta.OrdenadaMaxima, Respuesta.SegmentosV);
    }

    private void DeterminarRangoFilasPunto(CPuntoCalor Punto, double SaltoAbscisas, double SaltoOrdenadas,
          out Int32 FilaMinima, out Int32 FilaMaxima,
          out Int32 ColumnaMinima, out Int32 ColumnaMaxima)
    {
      double Salto = 4 * Math.Sqrt(SaltoAbscisas * SaltoOrdenadas);
      UbicarPosicionGrilla(Punto.Abscisa - Salto, Punto.Ordenada - Salto, out ColumnaMinima, out FilaMinima);
      UbicarPosicionGrilla(Punto.Abscisa + Salto, Punto.Ordenada + Salto, out ColumnaMaxima, out FilaMaxima);
      ColumnaMinima = Math.Min(Math.Max(ColumnaMinima, 1), Respuesta.SegmentosH - 1);
      ColumnaMaxima = Math.Max(1, Math.Min(ColumnaMaxima, Respuesta.SegmentosH - 1));
      FilaMinima = Math.Min(Math.Max(FilaMinima, 1), Respuesta.SegmentosV - 1);
      FilaMaxima = Math.Max(1, Math.Min(FilaMaxima, Respuesta.SegmentosV - 1));
    }

    private double PixelAbscisa(double Abscisa)
    {
      return (Abscisa - Respuesta.AbscisaMinima) * Respuesta.PixelsAncho / (Respuesta.AbscisaMaxima - Respuesta.AbscisaMinima);
    }

    private double PixelOrdenada(double Ordenada)
    {
      return (Ordenada - Respuesta.OrdenadaMinima) * Respuesta.PixelsAlto / (Respuesta.OrdenadaMaxima - Respuesta.OrdenadaMinima);
    }

    private double AbscisaCentroColumna(Int32 Columna, double Pixels)
    {
      return ((double)Columna + 0.5) * Pixels / (double)Respuesta.SegmentosH;
    }

    private double OrdenadaCentroColumna(Int32 Fila, double Pixels)
    {
      return ((double)Fila + 0.5) * Pixels / (double)Respuesta.SegmentosV;
    }

    private double ObtenerDistanciaCentroBlock(CPuntoCalor Punto, Int32 Columna, Int32 Fila)
    {
      double DistH = PixelAbscisa(Punto.Abscisa) - AbscisaCentroColumna(Columna, Respuesta.PixelsAncho);
      double DistV = PixelOrdenada(Punto.Ordenada) - OrdenadaCentroColumna(Fila, Respuesta.PixelsAlto);
      return Math.Sqrt(DistH * DistH + DistV * DistV);
    }

    private double DeterminarPesoPunto(CPuntoCalor Punto, Int32 Columna, Int32 Fila,
        double Distancia)
    {
      double DistPunto = ObtenerDistanciaCentroBlock(Punto, Columna, Fila);
      if (DistPunto >= Distancia)
      {
        return 0;
      }

      return Math.Cos(Math.PI * DistPunto / (2 * Distancia));

    }

    private const Int32 SALTOS_ESCALA = 255;
    private float AjustarValor(float Valor, List<float> Escala)
    {
      Int32 iMin = 0;
      Int32 iMax = Escala.Count - 1;
      Int32 i = 0;
      do
      {
        i = (iMax + iMin) / 2;
        if (i == 0)
        {
          i++;
        }
        if (Escala[i - 1] >= Valor)
        {
          iMax = i - 1;
        }
        else
        {
          if (Escala[i] < Valor)
          {
            iMin = i + 1;
          }
          else
          {
            // fuerza salida.
            iMin = i - 1;
            iMax = i;
          }
        }
      } while ((iMax - iMin) > 1);

      float Respuesta = (iMax == 0 ? 0 : ((float)(iMax - 1) + (Valor - Escala[iMax - 1]) / (Escala[iMax] - Escala[iMax - 1])) /
          (float)(Escala.Count - 1));
      return (Respuesta > 0.001 ? Respuesta : 0.001f);

    }

    private double RedondearDistancia(double DistanciaPixels)
    {
      float Distancia = (float)(6387137 * Math.PI * DistanciaPixels * (Respuesta.AbscisaMaxima - Respuesta.AbscisaMinima) /
          (180 * Respuesta.PixelsAncho));
      double Minimo = 0;
      double Maximo = Distancia;
      Rutinas.CRutinas.AjustarExtremosEscala(ref Minimo, ref Maximo, true);
      Respuesta.DistanciaEscala = 0;
      for (Int32 i = 5; i < 10; i++)
      {
        double Valor = Maximo * (double)i / 10;
        if (Math.Abs(Valor - Distancia) < Math.Abs(Distancia - Respuesta.DistanciaEscala))
        {
          Respuesta.DistanciaEscala = (float)Valor;
        }
      }
      return DistanciaPixels * Respuesta.DistanciaEscala / Distancia;
    }

    private const Int32 CANT_ESCALA = 8;

    private void CrearEscala()
    {

      double Minimo = Respuesta.Minimo; // *FactorCorreccion;
      double Maximo = Respuesta.Maximo; // *FactorCorreccion;
      Rutinas.CRutinas.AjustarExtremosEscala(ref Minimo, ref Maximo, true);
      Respuesta.ValoresEscala.Clear();

      List<float> ValoresColores = new List<float>();
      for (Int32 i = 0; i <= CANT_ESCALA; i++)
      {
        ValoresColores.Add(Respuesta.Minimo + i * (Respuesta.Maximo - Respuesta.Minimo) / CANT_ESCALA);
      }
      for (Int32 i = 0; i < ValoresColores.Count; i++)
      {
        Respuesta.ValoresEscala.Add(ValoresColores[i]);
      }

      //for (Int32 i = 0; i <= 10; i++)
      //{
      //  float Valor = (float)(Minimo + (Maximo - Minimo) * (double)i / 10);
      //  if (Valor >= Respuesta.Minimo && Valor <= Respuesta.Maximo)
      //  {
      //    for (Int32 ii = 0; ii < 4; ii++)
      //    {
      //      if (Math.Abs(Valor - ValoresColores[ii]) < Math.Abs(Respuesta.ValoresEscala[ii] - ValoresColores[ii]))
      //      {
      //        Respuesta.ValoresEscala[ii] = Valor;
      //      }
      //    }
      //  }
      //}

    }

    public string DeterminarMapa()
    {

      try
      {

        if (Puntos == null || Puntos.Count == 0)
        {
          return "Sin datos";
        }

        if (Respuesta.AbscisaMinima >= Respuesta.AbscisaMaxima || Respuesta.OrdenadaMinima == Respuesta.OrdenadaMaxima ||
          Respuesta.SegmentosH < 0 || Respuesta.SegmentosV < 0 || Respuesta.PixelsAncho <= 0 || Respuesta.PixelsAlto <= 0)
        {
          return "Datos incorrectos";
        }

        Respuesta.SegmentosH = (Respuesta.SegmentosHCfg > 0 ? Respuesta.SegmentosHCfg : (Int32)Respuesta.PixelsAncho);
        Respuesta.SegmentosV = (Respuesta.SegmentosVCfg > 0 ? Respuesta.SegmentosVCfg : (Int32)Respuesta.PixelsAlto);

        // Obtener la distancia sobre la que hay efecto (en pixels).
        double Distancia = RedondearDistancia(ObtenerDistanciaEfecto());

        // Obtener salto en abscisas y ordenadas (en coordenadas geograficas).
        double SaltoAbscisas = (Respuesta.AbscisaMaxima - Respuesta.AbscisaMinima) / Respuesta.SegmentosH;
        double SaltoOrdenadas = (Respuesta.OrdenadaMaxima - Respuesta.OrdenadaMinima) / Respuesta.SegmentosV;

        double Revancha = Math.Max(SaltoAbscisas, SaltoOrdenadas);
        Respuesta.AbscisaMinima -= SaltoAbscisas;
        Respuesta.AbscisaMaxima += SaltoAbscisas;
        Respuesta.OrdenadaMinima -= SaltoOrdenadas;
        Respuesta.OrdenadaMaxima += SaltoOrdenadas;
        Respuesta.SegmentosH += 2;
        Respuesta.SegmentosV += 2;

        CrearVectorDatos(Respuesta.Acumulado);

        // Poner el peso de los distintos puntos.
        Int32 ColMinima;
        Int32 ColMaxima;
        Int32 FilaMinima;
        Int32 FilaMaxima;
        foreach (CPuntoCalor Punto in Puntos)
        {
          DeterminarRangoFilasPunto(Punto, SaltoAbscisas, SaltoOrdenadas, out FilaMinima, out FilaMaxima, out ColMinima, out ColMaxima);
          for (Int32 Columna = ColMinima; Columna <= ColMaxima; Columna++)
          {
            for (Int32 Fila = FilaMinima; Fila <= FilaMaxima; Fila++)
            {
              double Peso = DeterminarPesoPunto(Punto, Columna, Fila, Distancia);
              if (Peso > 0.0001)
              {
                Respuesta.Valores[Fila * (Respuesta.SegmentosH + 1) + Columna] += Punto.Valor * (float)Peso;
                //if (!Respuesta.Acumulado)
                //{
                //  mCantidadValores[Fila * Respuesta.SegmentosH + Columna] += (float)Peso;
                //}
              }
            }
          }
        }

        // Buscar el maximo y minimo.
        Respuesta.Maximo = (from V in Respuesta.Valores
                            select V).Max();
        Respuesta.Minimo = (from V in Respuesta.Valores
                            select V).Min();
        if (Respuesta.Minimo >= Respuesta.Maximo)
        {
          return "Sin valores o valor uniforme";
        }

        // Traducir a 0..1.
        for (Int32 i = 0; i < Respuesta.Valores.Count; i++)
        {
          if (Respuesta.Maximo > Respuesta.Minimo)
          {
            Respuesta.Valores[i] = (Respuesta.Valores[i] - Respuesta.Minimo) / (Respuesta.Maximo - Respuesta.Minimo);
          }
          else
          {
            Respuesta.Valores[i] = (float)0.5;
          }
        }

        if (Respuesta.Cuantiles && Respuesta.Maximo > Respuesta.Minimo)
        {
          List<float> Valores = (from V in Respuesta.Valores
                                 where V > 0
                                 select V).ToList();
          if (Valores.Count > SALTOS_ESCALA)
          {
            Valores.Sort();
            List<float> Escala = new List<float>();
            Escala.Add(Valores[0]);
            for (Int32 i = 1; i < SALTOS_ESCALA; i++)
            {
              Escala.Add(Valores[(Valores.Count * i) / SALTOS_ESCALA]);
            }
            Escala.Add(Valores[Valores.Count - 1]);

            // Elimina duplicados.
            for (Int32 i = Escala.Count - 1; i > 0; i--)
            {
              if (Escala[i] == Escala[i - 1])
              {
                Escala.RemoveAt(i);
              }
            }

            for (Int32 i = 0; i < Respuesta.Valores.Count; i++)
            {
              if (Respuesta.Valores[i] > 0)
              {
                try
                {
                  Respuesta.Valores[i] = AjustarValor(Respuesta.Valores[i], Escala);
                }
                catch (Exception)
                {
                  Respuesta.Valores[i] = AjustarValor(Respuesta.Valores[i], Escala);
                }
              }
            }
          }

        }

        CrearEscala();

        return "";
      }
      catch (Exception ex)
      {
        return ex.Message;
      }
    }

    private double ObtenerFraccion(double Val1, double Val2, double Buscado)
    {
      if (((Val1 - Buscado) * (Val2 - Buscado)) <= 0)
      {
        return (Buscado - Val1) / (Val2 - Val1);
      }
      else
      {
        return -1;
      }
    }

    private CPunto PuntoSobreBorde(CPunto P1, CPunto P2, double Fraccion)
    {
      return new CPunto(P1.Abscisa + (P2.Abscisa - P1.Abscisa) * Fraccion, P1.Ordenada + (P2.Ordenada - P1.Ordenada) * Fraccion);
    }
    private void AgregarValoresTriangulo(ref CCurvasIgualValor Curvas, CPunto P1, CPunto P2, CPunto P3,
        double V1, double V2, double V3, double ValorEscala)
    {
      if (V1 < ValorEscala && V2 < ValorEscala && V3 < ValorEscala)
      {
        return;
      }
      if (V1 > ValorEscala && V2 > ValorEscala && V3 > ValorEscala)
      {
        return;
      }

      var Fraccion112 = ObtenerFraccion(V1, V2, ValorEscala);
      var Fraccion123 = ObtenerFraccion(V2, V3, ValorEscala);
      var Fraccion113 = ObtenerFraccion(V1, V3, ValorEscala);

      List<CPunto> PuntosLocales = new List<CPunto>();

      if (Fraccion112 >= 0 || Fraccion123 >= 0 || Fraccion113 >= 0)
      {
        // no intersecta el valor Escala1.
        if (Fraccion112 >= 0 && Fraccion123 >= 0)
        {
          PuntosLocales.Add(PuntoSobreBorde(P1, P2, Fraccion112));
          PuntosLocales.Add(PuntoSobreBorde(P2, P3, Fraccion123));
        }
        else
        {
          if (Fraccion112 >= 0 && Fraccion113 >= 0)
          {
            PuntosLocales.Add(PuntoSobreBorde(P1, P2, Fraccion112));
            PuntosLocales.Add(PuntoSobreBorde(P1, P3, Fraccion113));
          }
          else
          {
            if (Fraccion123 >= 0 && Fraccion113 >= 0)
            {
              PuntosLocales.Add(PuntoSobreBorde(P2, P3, Fraccion123));
              PuntosLocales.Add(PuntoSobreBorde(P1, P3, Fraccion113));
            }
          }
        }
      }

      if (PuntosLocales.Count == 2)
      {
        //        CPunto PRefe = new CPunto(-68.23286730769232, -38.96809);
        if (PuntosLocales[0].PuntoIgual(PuntosLocales[1]))
        {
          Curvas.AgregarNuevoTramo(PuntosLocales, ValorEscala);
          return;
        }
        Curvas.AgregarNuevoTramo(PuntosLocales, ValorEscala);
      }

    }

    private void AgregarValoresRectangulo(ref CCurvasIgualValor Curva,
      CPunto P1, CPunto P2, CPunto P3, CPunto P4, double V1, double V2, double V3, double V4, double Escala)
    {
      if (V1 <= Escala && V2 <= Escala && V3 <= Escala && V4 <= Escala)
      {
        return;
      }
      if (V1 >= Escala && V2 >= Escala && V3 >= Escala && V4 >= Escala)
      {
        return;
      }

      CPunto PuntoCentro = new CPunto((P1.Abscisa + P2.Abscisa + P3.Abscisa + P4.Abscisa) / 4,
            (P1.Ordenada + P2.Ordenada + P3.Ordenada + P4.Ordenada) / 4);
      var ValCentro = SacarValorDeEscala((float)(V1 + V2 + V3 + V4) / 4);

      AgregarValoresTriangulo(ref Curva, P1, P2, PuntoCentro, V1, V2, ValCentro, Escala);
      AgregarValoresTriangulo(ref Curva, P2, P4, PuntoCentro, V2, V4, ValCentro, Escala);
      AgregarValoresTriangulo(ref Curva, P4, P3, PuntoCentro, V4, V3, ValCentro, Escala);
      AgregarValoresTriangulo(ref Curva, P3, P1, PuntoCentro, V3, V1, ValCentro, Escala);

    }

    private List<float> mValorEscala01;

    private float SacarValorDeEscala(float Valor)
    {
      foreach (float Escala in mValorEscala01)
      {
        if (Valor == Escala)
        {
          return Valor + 0.000001f;
        }
      }
      return Valor;
    }

    private void ArmarCurvaIgualValor(ref CCurvasIgualValor Curva)
    {
      double SaltoH = (Respuesta.AbscisaMaxima - Respuesta.AbscisaMinima) / Respuesta.SegmentosH;
      double SaltoV = (Respuesta.OrdenadaMaxima - Respuesta.OrdenadaMinima) / Respuesta.SegmentosV;

      float FactorEscala = (Respuesta.ValoresEscala.Count == 0 ? 1 : (Respuesta.ValoresEscala.Last() <= 0 ? 1 : 1 / Respuesta.ValoresEscala.Last()));
      mValorEscala01 = (from E in Respuesta.ValoresEscala
                                 select E * FactorEscala).ToList();

      for (Int32 i = 0; i < Respuesta.Valores.Count; i++)
      {
        Respuesta.Valores[i] = SacarValorDeEscala(Respuesta.Valores[i]);
      }

      for (Int32 Fila = 0; Fila < Respuesta.SegmentosV; Fila++)
      {
        double Ordenada = Respuesta.OrdenadaMinima + SaltoV * Fila;
        for (Int32 Columna = 0; Columna < Respuesta.SegmentosH; Columna++)
        {
          try
          {
            CPunto P1 = new CPunto(Respuesta.AbscisaMinima + SaltoH * Columna, Ordenada);
            CPunto P2 = new CPunto(P1.Abscisa + SaltoH, Ordenada);
            CPunto P3 = new CPunto(P1.Abscisa, Ordenada + SaltoV);
            CPunto P4 = new CPunto(P1.Abscisa + SaltoH, P2.Ordenada + SaltoV);
            Int32 Pos = Fila * (Respuesta.SegmentosH + 1) + Columna;
            foreach (float ValorEscala in Respuesta.ValoresEscala)
            {
              float ValorBuscado = ValorEscala * FactorEscala;
              if (ValorBuscado > 0.005)
              {
                //if (ValorBuscado == 0)
                //{
                //  ValorBuscado = 0.0002f;
                //}
                AgregarValoresRectangulo(ref Curva, P1, P2, P3, P4, Respuesta.Valores[Pos], Respuesta.Valores[Pos + 1],
                    Respuesta.Valores[Pos + Respuesta.SegmentosH + 1], Respuesta.Valores[Pos + Respuesta.SegmentosH + 2],
                    ValorBuscado);
              }
            }
          }
          catch (Exception)
          {
            CPunto P1 = new CPunto(Respuesta.AbscisaMinima + SaltoH * Columna, Ordenada);
            CPunto P2 = new CPunto(P1.Abscisa + SaltoH, Ordenada);
            CPunto P3 = new CPunto(P1.Abscisa, Ordenada + SaltoV);
            CPunto P4 = new CPunto(P1.Abscisa + SaltoH, P2.Ordenada + SaltoV);
            Int32 Pos = Fila * (Respuesta.SegmentosH + 1) + Columna;
            foreach (float ValorEscala in Respuesta.ValoresEscala)
            {
              AgregarValoresRectangulo(ref Curva, P1, P2, P3, P4, Respuesta.Valores[Pos], Respuesta.Valores[Pos + 1],
                  Respuesta.Valores[Pos + Respuesta.SegmentosH + 1], Respuesta.Valores[Pos + Respuesta.SegmentosH + 2], ValorEscala * FactorEscala);
            }
          }
        }

      }
    }

    private void AjustarDireccionContenidoCurvas()
    {
      for (Int32 i = 0; i < Curvas.Curvas.Count; i++)
      {
        foreach (CLineaIgualValor Linea in Curvas.Curvas[i].Lineas)
        {
          Linea.AjustarSentidoDeGiro();
          if (i < (Curvas.Curvas.Count - 1))
          {
            foreach (CLineaIgualValor Linea2 in Curvas.Curvas[i + 1].Lineas)
            {
              if (Linea.LineaAdentro(Linea2))
              {
                Linea.LineaContenida = Linea2;
                break;
              }
            }
          }
        }
      }
    }

    public CCurvasIgualValor Curvas = null;

    public void DeterminarCurvas()
    {
      Curvas = new CCurvasIgualValor();
      ArmarCurvaIgualValor(ref Curvas);
      AjustarDireccionContenidoCurvas();
    }

    private string InterpolarRGBA(Int32 R1, Int32 G1, Int32 B1, float A1, Int32 R2, Int32 G2, Int32 B2, float A2, float Fraccion)
    {
      Int32 R = R1 + (Int32)Math.Floor((float)(R2 - R1) * Fraccion + 0.49);
      Int32 G = G1 + (Int32)Math.Floor((float)(G2 - G1) * Fraccion + 0.49);
      Int32 B = B1 + (Int32)Math.Floor((float)(B2 - B1) * Fraccion + 0.49);
      float A = A1 + (float)(A2 - A1) * Fraccion;
      return "rgba(" + R.ToString() + ", " + G.ToString() + ", " + B.ToString() + ", " + Rutinas.CRutinas.FloatVStr(A) + ")";
    }

    private string ColorDesdeValorEscala(float Valor)
    {
      if (Valor <= 0.333333)
      {
        // Entre azul y verde.
        //        return "rgba(128,255,128,0.5);";
        return InterpolarRGBA(0, 0, 255, 0, 0, 255, 0, 0.33f, 3 * Valor);
      }
      else
      {
        if (Valor <= 0.666666)
        {
          return InterpolarRGBA(0, 255, 0, 0.33f, 255, 255, 0, 0.66f, 3f * (Valor-0.333333f));
        }
        else
        {
          return InterpolarRGBA(255, 255, 0, 0.66f, 255, 0, 0, 0.8f, 3f * (Valor - 0.666667f));
        }
      }
    }

    private void ExtraerAbscisasLinea(CLineaIgualValor Linea, out double[] Abscisas, out double[] Ordenadas)
    {
      Abscisas = new double[Linea.Segmentos.Count + 2];
      Ordenadas = new double[Linea.Segmentos.Count + 2];
      Abscisas[0] = Linea.Segmentos[0].PuntoD.Abscisa;
      Ordenadas[0] = Linea.Segmentos[0].PuntoD.Ordenada;
      Int32 Pos = 1;
      foreach (CSegmentoIgualValor Segm in Linea.Segmentos)
      {
        Abscisas[Pos] = Segm.PuntoH.Abscisa;
        Ordenadas[Pos] = Segm.PuntoH.Ordenada;
        Pos++;
      }
      Abscisas[Pos] = Abscisas[0];
      Ordenadas[Pos] = Ordenadas[0];
    }

    private async Task DibujarAreaColorAsync(IJSRuntime JSRuntime, CLineaIgualValor Externa, CLineaIgualValor Interna, string Color, Int32 Posicion)
    {
      if (Externa.Segmentos != null && Externa.Segmentos.Count > 1)
      {
        double[] Abscisas;
        double[] Ordenadas;
        ExtraerAbscisasLinea(Externa, out Abscisas, out Ordenadas);
        if (Interna == null)
        {
          object[] Args = new object[10];
          Args[0] = Posicion;
          Args[1] = Abscisas;
          Args[2] = Ordenadas;
          Args[3] = -999;
          Args[4] = -999;
          Args[5] = Color;
          Args[6] = "";
          Args[7] = "";
          Args[8] = "";
          Args[9] = 0;
          await JSRuntime.InvokeAsync<Task>("DibujarPoligono", Args);
        }
        else
        {
          double[] AbscisasAdentro;
          double[] OrdenadasAdentro;
          ExtraerAbscisasLinea(Interna, out AbscisasAdentro, out OrdenadasAdentro);
          object[] Args = new object[9];
          Args[0] = Posicion;
          Args[1] = Abscisas;
          Args[2] = Ordenadas;
          Args[3] = AbscisasAdentro;
          Args[4] = OrdenadasAdentro;
          Args[5] = Color;
          Args[6] = "";
          Args[7] = "";
          Args[8] = "";
          await JSRuntime.InvokeAsync<Task>("DibujarPoligonoHueco", Args);
        }
      }
    }

    private async Task DibujarRectanguloExtremoAsync(IJSRuntime JSRuntime, Int32 Posicion)
    {
      double[] Abscisas = new double[5];
      double[] Ordenadas = new double[5];
      Abscisas[0] = Respuesta.AbscisaMinima;
      Ordenadas[0] = Respuesta.OrdenadaMinima;
      Abscisas[1] = Respuesta.AbscisaMaxima;
      Ordenadas[1] = Respuesta.OrdenadaMinima;
      Abscisas[2] = Respuesta.AbscisaMaxima;
      Ordenadas[2] = Respuesta.OrdenadaMaxima;
      Abscisas[3] = Respuesta.AbscisaMinima;
      Ordenadas[3] = Respuesta.OrdenadaMaxima;
      Abscisas[4] = Respuesta.AbscisaMinima;
      Ordenadas[4] = Respuesta.OrdenadaMinima;

      object[] Args = new object[9];
      Args[0] = Posicion;
      Args[1] = Abscisas;
      Args[2] = Ordenadas;
      Args[3] = -999;
      Args[4] = -999;
      Args[5] = "red";
      Args[6] = "";
      Args[7] = "";
      Args[8] = "";
      Args[9] = 0;
      await JSRuntime.InvokeAsync<Task>("DibujarPoligono", Args);
    }

    public async Task DibujarCurvasAsync(IJSRuntime JSRuntime, Int32 Posicion)
    {
//      await DibujarRectanguloExtremoAsync(JSRuntime, Posicion);

      for (Int32 i = 0; i < Curvas.Curvas.Count; i++)
      {
        CCurvaIgualValor Curva = Curvas.Curvas[i];
        if (Curva.Valor > 0.05)
        {
          string Color = (i == (Curvas.Curvas.Count - 1) ? ColorDesdeValorEscala((float)Curva.Valor) :
              ColorDesdeValorEscala((float)(Curva.Valor + Curvas.Curvas[i + 1].Valor) / 2f));
          foreach (CLineaIgualValor Linea in Curva.Lineas)
          {
            await DibujarAreaColorAsync(JSRuntime, Linea, Linea.LineaContenida, Color, Posicion);
          }
        }
      }
    }

  }

  public class CSegmentoIgualValor
  {
    public CPunto PuntoD { get; set; }
    public CPunto PuntoH { get; set; }

    public CSegmentoIgualValor(CPunto P1, CPunto P2)
    {
      PuntoD = P1;
      PuntoH = P2;
    }

  }

  public class CLineaIgualValor
  {
    public List<CSegmentoIgualValor> Segmentos { get; set; }
    public CLineaIgualValor LineaContenida { get; set; } = null;

    public CLineaIgualValor()
    {
      Segmentos = new List<CSegmentoIgualValor>();
    }

    public void PonerPrimerSegmento(CPunto P1, CPunto P2)
    {
      Segmentos.Add(new CSegmentoIgualValor(P1, P2));
    }

    public bool AgregarSegmento(CPunto P1, CPunto P2)
    {
      if (Segmentos.Count > 0)
      {
        if (Segmentos[0].PuntoD.PuntoIgual(P1))
        {
          Segmentos.Insert(0, new CSegmentoIgualValor(P2, P1));
          return true;
        }
        else
        {
          if (Segmentos[0].PuntoD.PuntoIgual(P2))
          {
            Segmentos.Insert(0, new CSegmentoIgualValor(P1, P2));
            return true;
          }
          else
          {
            if (Segmentos.Last().PuntoH.PuntoIgual(P1))
            {
              Segmentos.Add(new CSegmentoIgualValor(P1, P2));
              return true;
            }
            else
            {
              if (Segmentos.Last().PuntoH.PuntoIgual(P2))
              {
                Segmentos.Add(new CSegmentoIgualValor(P2, P1));
                return true;
              }
            }
          }
        }
      }
      return false;
    }

    public CMapaCalorRN.ModoUnir VerificarPosibleUnion(CPunto P1, CPunto P2)
    {
      if (Segmentos.Count > 0)
      {
        if (Segmentos[0].PuntoD.PuntoIgual(P1) || Segmentos[0].PuntoD.PuntoIgual(P2))
        {
          return CMapaCalorRN.ModoUnir.Inicio;
        }
        else
        {
          if (Segmentos.Last().PuntoD.PuntoIgual(P1) || Segmentos.Last().PuntoD.PuntoIgual(P2))
          {
            return CMapaCalorRN.ModoUnir.Cierre;
          }
        }
      }
      return CMapaCalorRN.ModoUnir.No;
    }

    private void UnirConOtraCurva(CLineaIgualValor Otra, bool DesdeInicio, bool OtraDesdeInicio)
    {
      if (DesdeInicio == OtraDesdeInicio)
      {
        Otra.InvertirDireccion();
        OtraDesdeInicio = !OtraDesdeInicio;
      }
      if (DesdeInicio)
      {
        Int32 Pos = 0;
        foreach (CSegmentoIgualValor S in Otra.Segmentos)
        {
          Segmentos.Insert(Pos++, S);
        }
      }
      else
      {
        foreach (CSegmentoIgualValor S in Otra.Segmentos)
        {
          Segmentos.Add(S);
        }
      }

      Otra.Segmentos.Clear();

    }

    public bool CurvaCerrada
    {
      get
      {
        return (Segmentos.Count > 2 && Segmentos[0].PuntoD.PuntoIgual(Segmentos.Last().PuntoH));
      }
    }

    private double AreaBajoSegmento(CPunto P1, CPunto P2)
    {
      return (P2.Abscisa - P1.Abscisa) * (P2.Ordenada + P1.Ordenada) / 2;
    }

    private bool SentidoHorario
    {
      get
      {
        if (Segmentos.Count < 3)
        {
          return true;
        }
        else
        {
          double Area = 0;
          foreach (CSegmentoIgualValor S in Segmentos)
          {
            Area += AreaBajoSegmento(S.PuntoD, S.PuntoH);
          }
          Area += AreaBajoSegmento(Segmentos.Last().PuntoH, Segmentos[0].PuntoD);
          return (Area <= 0); // menor porque ordenadas van hacia abajo.
        }
      }
    }

    public void InvertirDireccion()
    {
      List<CSegmentoIgualValor> NuevaLista = new List<CSegmentoIgualValor>();
      for (Int32 i = Segmentos.Count - 1; i >= 0; i--)
      {
        NuevaLista.Add(new CSegmentoIgualValor(Segmentos[i].PuntoH, Segmentos[i].PuntoD));
      }
      Segmentos = NuevaLista;
    }

    public void AjustarSentidoDeGiro()
    {
      if (!SentidoHorario)
      {
        InvertirDireccion();
      }
    }

    public bool UnirConCurva(CLineaIgualValor Otra)
    {
      if (Otra.Segmentos.Count > 0 && Segmentos.Count > 0)
      {
        if (Segmentos[0].PuntoD.PuntoIgual(Otra.Segmentos.Last().PuntoH))
        {
          UnirConOtraCurva(Otra, true, false);
          return true;
        }
        else
        {
          if (Segmentos[0].PuntoD.PuntoIgual(Otra.Segmentos[0].PuntoD))
          {
            UnirConOtraCurva(Otra, true, true);
            return true;
          }
          else
          {
            if (Segmentos.Last().PuntoH.PuntoIgual(Otra.Segmentos[0].PuntoD))
            {
              UnirConOtraCurva(Otra, false, true);
              return true;
            }
            else
            {
              if (Segmentos.Last().PuntoH.PuntoIgual(Otra.Segmentos.Last().PuntoH))
              {
                UnirConOtraCurva(Otra, false, false);
                return true;
              }
            }
          }
        }
      }
      return false;
    }

    private bool IntersectaPorIzquierda(CSegmentoIgualValor Segm, CPunto Punto)
    {
      if ((Segm.PuntoH.Ordenada - Punto.Ordenada) * (Segm.PuntoD.Ordenada - Punto.Ordenada) <= 0)
      {
        if (Segm.PuntoD.Ordenada == Segm.PuntoH.Ordenada)
        {
          return (Punto.Abscisa > Segm.PuntoD.Abscisa && Punto.Abscisa > Segm.PuntoH.Abscisa);
        }
        else
        {
          double AbscisaSegm = Segm.PuntoD.Abscisa + (Segm.PuntoH.Ordenada - Punto.Ordenada) *
                (Segm.PuntoH.Abscisa - Segm.PuntoD.Abscisa) /
                (Segm.PuntoH.Ordenada - Segm.PuntoD.Ordenada);
          return (AbscisaSegm < Punto.Abscisa);
        }
      }
      else
      {
        return false;
      }
    }

    private bool PuntoAdentro(CPunto Punto)
    {
      if (Segmentos.Count<3 || !CurvaCerrada)
      {
        return false;
      }
      Int32 Suma = 0;
      foreach (CSegmentoIgualValor Segm in Segmentos)
      {
        if (IntersectaPorIzquierda(Segm, Punto))
        {
          Suma++;
        }
      }
      if (IntersectaPorIzquierda(new CSegmentoIgualValor(Segmentos.Last().PuntoH, Segmentos[0].PuntoD), Punto))
      {
        Suma++;
      }

      return ((Suma % 2) != 0);

    }

    public bool LineaAdentro(CLineaIgualValor Otra)
    {
      if (Otra.Segmentos.Count>2 && Otra.CurvaCerrada)
      {
        return PuntoAdentro(Otra.Segmentos[0].PuntoD);
      }
      else
      {
        return false;
      }
    }



  }

  public class CCurvaIgualValor
  {

    public double Valor { get; set; }
    public CCurvaIgualValor CurvaInterior { get; set; }

    public List<CLineaIgualValor> Lineas { get; set; }
    public CCurvaIgualValor()
    {
      Lineas = new List<CLineaIgualValor>();
    }

    public void AgregarSegmento(CPunto P1, CPunto P2)
    {
      foreach (CLineaIgualValor Linea0 in Lineas)
      {
        if (Linea0.AgregarSegmento(P1, P2))
        {
          foreach (CLineaIgualValor Linea2 in Lineas)
          {
            if (Linea2 != Linea0)
            {
              if (Linea0.UnirConCurva(Linea2))
              {
                Lineas.Remove(Linea2);
                break;
              }
            }
          }
          return;
        }
      }
      CLineaIgualValor Linea = new CLineaIgualValor();
      Linea.PonerPrimerSegmento(P1, P2);
      Lineas.Add(Linea);
    }

  }

  public class CCurvasIgualValor
  {
    public List<CCurvaIgualValor> Curvas { get; set; }

    public CCurvasIgualValor()
    {
      Curvas = new List<CCurvaIgualValor>();
    }

    public void AgregarNuevoTramo(List<CPunto> Puntos, double Valor)
    {
      // Ubicar curva.
      CCurvaIgualValor Curva = (from C in Curvas
                                where Math.Abs(Valor - C.Valor) < 0.00001
                                select C).FirstOrDefault();
      if (Curva == null)
      {
        Curva = new CCurvaIgualValor()
        {
          Valor = Valor
        };
        Curvas.Add(Curva);
      }

      for (Int32 i = 1; i < Puntos.Count; i++)
      {
        Curva.AgregarSegmento(Puntos[i - 1], Puntos[i]);
      }
    }

  }
}
