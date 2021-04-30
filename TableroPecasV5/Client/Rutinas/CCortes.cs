using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Linq;

namespace TableroPecasV5.Client.Rutinas
{
  public class CCortes
  {

    private static void CorrerLimiteHaciaAtras(List<CBlockCorte> Bloques, Int32 Pos, List<double> Valores,
          CEncuentro Encuentro,
          bool Imponer = false)
    {
      if (Bloques[Pos].PosD == Bloques[Pos].PosH)
      {
        Encuentro.EMCAnteriorRetrocediendo = double.MaxValue;
        Encuentro.EMCPosteriorRetrocediendo = double.MaxValue;
      }
      else
      {
        Int32 PosRefe = Bloques[Pos].PosH;
        Int32 CantValores = 1;
        for (Int32 i = PosRefe - 1; i >= 0; i--)
        {
          if (DatosIguales(Valores[i], Valores[PosRefe]))
          {
            CantValores++;
          }
          else
          {
            break;
          }
        }

        double EMC1, EMC2;

        Bloques[Pos].ProbarEliminarElementos(Valores[PosRefe], CantValores, false,
            out EMC1, Imponer);
        Encuentro.EMCAnteriorRetrocediendo = EMC1;
        Bloques[Pos + 1].ProbarAgregarElementos(Valores[PosRefe], CantValores, true,
            out EMC2, Imponer);
        Encuentro.EMCPosteriorRetrocediendo = EMC2;
      }
    }

    private static void CorrerLimiteHaciaAdelante(List<CBlockCorte> Bloques, Int32 Pos, List<double> Valores,
          CEncuentro Encuentro,
          bool Imponer = false)
    {
      if (Bloques[Pos].PosD == Bloques[Pos].PosH)
      {
        Encuentro.EMCAnteriorAdelantando = double.MaxValue;
        Encuentro.EMCPosteriorAdelantando = double.MaxValue;
      }
      else
      {
        Int32 PosRefe = Bloques[Pos + 1].PosD;
        Int32 CantValores = 1;
        for (Int32 i = PosRefe + 1; i < Valores.Count; i++)
        {
          if (DatosIguales(Valores[i], Valores[PosRefe]))
          {
            CantValores++;
          }
          else
          {
            break;
          }
        }

        double EMC1, EMC2;
        Bloques[Pos].ProbarAgregarElementos(Valores[PosRefe], CantValores, false,
            out EMC1, Imponer);
        Encuentro.EMCAnteriorAdelantando = EMC1;
        Bloques[Pos + 1].ProbarEliminarElementos(Valores[PosRefe], CantValores, true,
            out EMC2, Imponer);
        Encuentro.EMCPosteriorAdelantando = EMC2;
      }
    }

    private static void ProcesarSaltoEnEncuentro(List<CBlockCorte> Bloques, Int32 Pos,
        List<double> Valores, CEncuentro Encuentro)
    {
      CorrerLimiteHaciaAtras(Bloques, Pos, Valores, Encuentro);
      CorrerLimiteHaciaAdelante(Bloques, Pos, Valores, Encuentro);
    }

    private static double EliminarElementosAnteriores(List<CBlockCorte> Bloques, Int32 Pos, List<double> Valores,
          bool Imponer = false)
    {
      if (Bloques[Pos].PosD == Bloques[Pos].PosH)
      {
        return double.MaxValue;
      }

      Int32 PosRefe = Bloques[Pos].PosH;
      Int32 CantValores = 1;
      for (Int32 i = PosRefe - 1; i >= 0; i--)
      {
        if (DatosIguales(Valores[i], Valores[PosRefe]))
        {
          CantValores++;
        }
        else
        {
          break;
        }
      }

      double EMC1, EMC2;
      Bloques[Pos].ProbarEliminarElementos(Valores[PosRefe], CantValores, false,
          out EMC1, Imponer);
      Bloques[Pos + 1].ProbarAgregarElementos(Valores[PosRefe], CantValores, true,
          out EMC2, Imponer);
      return EMC1 + EMC2;
    }

    private static double AgregarElementosAnteriores(List<CBlockCorte> Bloques, Int32 Pos, List<double> Valores,
          bool Imponer = false)
    {
      if (Bloques[Pos].PosD == Bloques[Pos].PosH)
      {
        return double.MaxValue;
      }

      Int32 PosRefe = Bloques[Pos + 1].PosD;
      Int32 CantValores = 1;
      for (Int32 i = PosRefe + 1; i < Valores.Count; i++)
      {
        if (DatosIguales(Valores[i], Valores[PosRefe]))
        {
          CantValores++;
        }
        else
        {
          break;
        }
      }

      double EMC1, EMC2;
      Bloques[Pos].ProbarAgregarElementos(Valores[PosRefe], CantValores, false,
          out EMC1, Imponer);
      Bloques[Pos + 1].ProbarEliminarElementos(Valores[PosRefe], CantValores, true,
          out EMC2, Imponer);
      return EMC1 + EMC2;
    }

    public static List<double> DeterminarRangosMinimizandoEMCGlobal(List<double> Valores, Int32 Segmentos)
    {

      Valores.Sort();

      List<double> ValoresUnicos = (from V in Valores
                                    select V).Distinct().ToList();
      if (ValoresUnicos.Count <= Segmentos)
      {
        throw new Exception("Los valores son insuficientes");
      }

      ValoresUnicos.Sort();

      double Factor = (double)ValoresUnicos.Count / (double)(Segmentos + 1);
      List<CBlockCorte> Bloques = new List<CBlockCorte>();
      Int32 Pos0 = 0;
      for (Int32 i = 1; i <= Segmentos; i++)
      {
        Int32 Pos1 = Math.Min((Int32)Math.Floor(Factor * i + 0.5), Valores.Count - 1);
        Int32 Pos2 = -1;
        for (Int32 ii = Pos0 + 1; ii < Valores.Count; ii++)
        {
          if (Valores[ii] == ValoresUnicos[Pos1])
          {
            Pos2 = ii;
            for (Int32 ii2 = ii + 1; ii2 < Valores.Count; ii2++)
            {
              if (!DatosIguales(Valores[ii2], Valores[ii]))
              {
                break;
              }
              else
              {
                Pos2 = ii2; // si hay valores repetidos, corta en el ultimo.
              }
            }
            break;
          }
        }
        Bloques.Add(new CBlockCorte()
        {
          PosD = Pos0,
          PosH = Pos2
        });
        Pos0 = Pos2 + 1;
      }

      Bloques[Bloques.Count - 1].PosH = Valores.Count - 1;

      foreach (CBlockCorte Block in Bloques)
      {
        Block.DeterminarEMC(Valores);
      }

      Int32 iMax = Bloques.Count - 1;
      List<double> EMCAntes = new List<double>();
      List<double> EMCDespues = new List<double>();

      for (Int32 i = 0; i < iMax; i++)
      {
        EMCAntes.Add(Bloques[i].EMC + Bloques[i + 1].EMC);
        EMCAntes.Add(Bloques[i].EMC + Bloques[i + 1].EMC);
      }

      for (Int32 i = 0; i < iMax; i++)
      {
        EMCDespues.Add(EliminarElementosAnteriores(Bloques, i, Valores, false));
        EMCDespues.Add(AgregarElementosAnteriores(Bloques, i, Valores, false));
      }

      while (true)
      {
        Int32 Pos = -1;
        double Maximo = 0;
        for (Int32 i = 0; i < EMCDespues.Count; i++)
        {
          if ((EMCAntes[i] - EMCDespues[i]) > Maximo)
          {
            Maximo = EMCAntes[i] - EMCDespues[i];
            Pos = i;
          }
        }

        if (Pos >= 0 && Maximo > 0.0001)
        {

          double Antes = (from B in Bloques
                          select B.EMC).Sum();
          // Redeterminar el valor en los bloques.
          Int32 Incremento = (Pos % 2);
          Int32 PosOtro = Pos + (Incremento > 0 ? -1 : 1);
          Int32 PosVct = (Pos - Incremento) / 2;
          switch (Incremento)
          {
            case 0: // elimina a la izq.
              EMCAntes[Pos] = EliminarElementosAnteriores(Bloques, PosVct, Valores, true);
              break;
            default:
              EMCAntes[Pos] = AgregarElementosAnteriores(Bloques, PosVct, Valores, true);
              break;
          }
          EMCAntes[PosOtro] = EMCAntes[Pos];
          Int32 PosIzq = Math.Min(Pos, PosOtro);
          Int32 PosDer = Math.Max(Pos, PosOtro);
          EMCDespues[PosIzq] = EliminarElementosAnteriores(Bloques, PosVct, Valores, false);
          EMCDespues[PosDer] = AgregarElementosAnteriores(Bloques, PosVct, Valores, false);

          double Despues = (from B in Bloques
                            select B.EMC).Sum();
        }
        else
        {
          break;
        }
      }

      List<double> Respuesta = new List<double>();
      Respuesta.Add(Valores[0]);
      foreach (CBlockCorte Block in Bloques)
      {
        Respuesta.Add(Valores[Block.PosH]);
      }
      return Respuesta;
    }

    private static double DeterminarEMC(List<double> EMCs)
    {
      if (EMCs.Count < 2)
      {
        return 0;
      }
      else
      {
        double S = 0;
        double S2 = 0;
        foreach (double V in EMCs)
        {
          S += V;
          S2 += S * S;
        }
        return Math.Sqrt((S2 - S * S / EMCs.Count) / EMCs.Count);
      }
    }

    private double DeterminarEMCGlobal(List<double> EMCs, Int32 Pos, CEncuentro Encuentro, bool HaciaAtras)
    {
      if (EMCs.Count < 2)
      {
        return 0;
      }
      else
      {
        List<double> Provisorios = new List<double>();
        Provisorios.AddRange(EMCs);
        if (HaciaAtras)
        {
          Provisorios[Pos] = Encuentro.EMCAnteriorRetrocediendo;
          Provisorios[Pos + 1] = Encuentro.EMCPosteriorRetrocediendo;
        }
        else
        {
          Provisorios[Pos] = Encuentro.EMCAnteriorAdelantando;
          Provisorios[Pos + 1] = Encuentro.EMCPosteriorAdelantando;
        }

        return DeterminarEMC(Provisorios);

      }
    }

    private static bool DatosIguales(double R1, double R2)
    {
      return Math.Abs(R1 - R2) < 0.000000001;
    }
  }

  public class CEncuentro
  {
    public double EMCAnteriorRetrocediendo { get; set; }
    public double EMCAnteriorAdelantando { get; set; }
    public double EMCPosteriorRetrocediendo { get; set; }
    public double EMCPosteriorAdelantando { get; set; }
  }

  public class CBlockCorte
  {
    public Int32 PosD { get; set; }
    public Int32 PosH { get; set; }
    public double EMC2 { get; set; }
    public double EMC { get; set; }
    public double Suma { get; set; }
    public double Suma2 { get; set; }
    public double Cantidad { get; set; }

    public void DeterminarEMC(List<double> Valores)
    {
      Cantidad = PosH - PosD + 1;
      Suma = 0;
      Suma2 = 0;
      for (Int32 i = PosD; i <= PosH; i++)
      {
        Suma += Valores[i];
        Suma2 += Valores[i] * Valores[i];
      }
      EMC = Math.Sqrt((Suma2 - Suma * Suma / Cantidad) / Cantidad);
    }

    public void ProbarAgregarElementos(double Valor, Int32 CantValores, bool DesdeInicio, out double EMCLocal, bool Imponer = false)
    {
      double SumaLocal = Suma + Valor * CantValores;
      double MediaLocal = SumaLocal / (Cantidad + CantValores);
      double Suma2Local = Suma2 + CantValores * Valor * Valor;
      double EMC2Local = (Suma2Local - (Cantidad + CantValores) * MediaLocal * MediaLocal) /
          (Cantidad + CantValores);
      EMCLocal = Math.Sqrt(EMC2Local);
      if (Imponer)
      {
        if (DesdeInicio)
        {
          PosD -= CantValores;
        }
        else
        {
          PosH += CantValores;
        }
        Suma = SumaLocal;
        Suma2 = Suma2Local;
        EMC2 = EMC2Local;
        EMC = EMCLocal;
        Cantidad += CantValores;
      }
    }

    public void ProbarEliminarElementos(double Valor, Int32 CantValores, bool DesdeInicio, out double EMCLocal, bool Imponer = false)
    {
      if (Cantidad <= CantValores)
      {
        EMCLocal = double.MaxValue;
      }
      else
      {
        double SumaLocal = Suma - Valor * CantValores;
        double MediaLocal = SumaLocal / (Cantidad - CantValores);
        double Suma2Local = Suma2 - CantValores * Valor * Valor;
        double EMC2Local = (Suma2Local - (Cantidad - CantValores) * MediaLocal * MediaLocal) /
            (Cantidad - CantValores);
        EMCLocal = Math.Sqrt(EMC2Local);
        if (Imponer)
        {
          if (DesdeInicio)
          {
            PosD += CantValores;
          }
          else
          {
            PosH -= CantValores;
          }
          Suma = SumaLocal;
          Suma2 = Suma2Local;
          EMC2 = EMC2Local;
          EMC = EMCLocal;
          Cantidad -= CantValores;
        }
      }
    }

  }
}
