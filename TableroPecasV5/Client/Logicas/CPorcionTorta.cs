using System;
using System.Collections.Generic;
using System.Linq;
using Blazor.Extensions.Canvas.Canvas2D;
using System.Threading.Tasks;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
  public class CPorcionTorta
  {
    public bool Seleccionado;
    public Int32 Codigo;
    public double AcumuladoAnterior;
    public double Valor;
    public string Texto;
    public string Texto2;
    public string ColorRelleno;
    public CDatosTorta Datos;

    public string Denominacion
    {
      get
      {
        return Texto;
      }
    }

		public async Task DibujarSobreCanvasAsync(double AnchoTotal, double AltoTotal, double AcumuladoTotal, Canvas2DContext Contexto)
		{
			double AbscCentro = AnchoTotal / 2;
			double OrdCentro = AltoTotal / 2;
			double Tamanio = CLogicaGrafico.FRACCION_REFERENCIAS * Math.Min(AbscCentro, OrdCentro);

			if (Seleccionado)
			{
				double AnguloMedio = 2 * Math.PI * (AcumuladoAnterior + Math.Abs(Valor) / 2) /
						AcumuladoTotal;
				AbscCentro += Tamanio * Math.Sin(AnguloMedio) / 4;
				OrdCentro -= Tamanio * Math.Cos(AnguloMedio) / 4;
			}

			await CrearSectorAsync(AbscCentro, OrdCentro, Tamanio, AcumuladoTotal, Contexto);

		}

		private double AnguloInicial(double AcumuladoTotal)
    {
      return 2 * Math.PI * AcumuladoAnterior / AcumuladoTotal;
    }

    public double AnguloCentral(double AcumuladoTotal)
    {
      //return 2 * Math.PI * (AcumuladoAnterior + Valor / 2) /
      //      Superior.Acumulado;
      return 2 * Math.PI * AcumuladoAnterior / AcumuladoTotal +
          Math.PI * Math.Abs(Valor) / AcumuladoTotal;
    }

    public CPunto PuntoContornoEtiqueta(double AcumuladoTotal, double Ancho, double Alto)
    {
      double AngCentro = AnguloCentral(AcumuladoTotal);
      double AbscCentro = Ancho / 2;
      double OrdCentro = Alto / 2;
      double Tamanio = 0.85 * Math.Min(AbscCentro, OrdCentro);

      return new CPunto(AbscCentro + Tamanio * Math.Sin(AngCentro),
          OrdCentro - Tamanio * Math.Cos(AngCentro));
    }

    private CPunto PuntoDesplazado(double AbscCentro, double OrdCentro, double Angulo, double Distancia)
    {
      return new CPunto(AbscCentro + Math.Sin(Angulo) * Distancia,
          OrdCentro - Math.Cos(Angulo) * Distancia);
    }

    public void ImponerContornoRectangular(double Abscisa, double Ordenada, double Ancho, double Alto)
    {
      Contorno.Clear();
      Contorno.Add(new CPunto(Abscisa, Ordenada));
      Contorno.Add(new CPunto(Abscisa + Ancho, Ordenada));
      Contorno.Add(new CPunto(Abscisa + Ancho, Ordenada + Alto));
      Contorno.Add(new CPunto(Abscisa, Ordenada + Alto));
    }

    //public double AnguloCentro()
    //{
    //  return 2 * Math.PI * AcumuladoAnterior /
    //      Superior.Acumulado +
    //      ((Valor == Superior.Acumulado ?
    //      2 * Math.PI - 0.0001 :
    //      2 * Math.PI * (AcumuladoAnterior + Valor) /
    //      Superior.Acumulado)) / 2;
    //}

    private List<CPunto> Contorno = new List<CPunto>();

    private CPunto ObtenerPuntoArco(double Angulo, double AbscC, double OrdC, double Tamanio)
    {
      return new CPunto(AbscC + Tamanio * Math.Sin(Angulo), OrdC - Tamanio * Math.Cos(Angulo));
    }

    private async Task CrearSectorAsync(double AbscCentro, double OrdCentro,
          double Tamanio, double AcumuladoTotal, Canvas2DContext Contexto)
    {

      Contorno.Clear();

      double AngIni = AnguloInicial(AcumuladoTotal);

      await Contexto.BeginPathAsync();
      await Contexto.SetLineWidthAsync(1);
      await Contexto.SetStrokeStyleAsync("black");
      float[] Saltos = new float[0];
      await Contexto.SetLineDashAsync(Saltos);
      await Contexto.SetFillStyleAsync(ColorRelleno);

      CPunto Inicio = PuntoDesplazado(AbscCentro, OrdCentro, AngIni, Tamanio / 2);
      await Contexto.MoveToAsync(Inicio.Abscisa, Inicio.Ordenada);
      Contorno.Add(Inicio);

      Inicio = PuntoDesplazado(AbscCentro, OrdCentro, AngIni, Tamanio);
      await Contexto.LineToAsync(Inicio.Abscisa, Inicio.Ordenada);
      Contorno.Add(Inicio);

      double AnguloH = (Math.Abs(Valor + AcumuladoAnterior) >= AcumuladoTotal ?
        2 * Math.PI - 0.0001 :
        2 * Math.PI * Math.Abs(AcumuladoAnterior + Valor) / AcumuladoTotal);

      await Contexto.ArcAsync(AbscCentro, OrdCentro, Tamanio, CRutinas.PonerEnRango(AngIni - Math.PI / 2),
        CRutinas.PonerEnRango(AnguloH - Math.PI / 2), false);
      double AnguloMax = (AngIni > AnguloH ? (AnguloH + 2 * Math.PI) : AnguloH);
      for (Int32 i = 1; i <= 5; i++)
      {
        Contorno.Add(ObtenerPuntoArco(AngIni + (AnguloMax - AngIni) * i / 5, AbscCentro, OrdCentro, Tamanio));
      }

      Inicio = PuntoDesplazado(AbscCentro, OrdCentro, AnguloH, Tamanio / 2);
      await Contexto.LineToAsync(Inicio.Abscisa, Inicio.Ordenada);
      Contorno.Add(Inicio);

      await Contexto.ArcAsync(AbscCentro, OrdCentro, Tamanio / 2, CRutinas.PonerEnRango(AnguloH - Math.PI / 2),
        CRutinas.PonerEnRango(AngIni - Math.PI / 2), true);
      AnguloMax -= 2 * Math.PI;
      for (Int32 i = 1; i <= 5; i++)
      {
        Contorno.Add(ObtenerPuntoArco(AnguloMax + (AngIni - AnguloMax) * i / 5, AbscCentro, OrdCentro, Tamanio / 2));
      }

      await Contexto.StrokeAsync();
      await Contexto.FillAsync();

      await Contexto.ClosePathAsync();

    }

    public bool PuntoEncima(double Abscisa, double Ordenada)
    {
      // Usa el contorno.
      Int32 Cantidad = 0;
      for (Int32 i = 0; i < Contorno.Count; i++)
      {
        Int32 Otro = (i == 0 ? Contorno.Count - 1 : i - 1);
        if (((Ordenada - Contorno[i].Ordenada) * (Ordenada - Contorno[Otro].Ordenada)) <= 0)
        {
          if (Ordenada == Contorno[i].Ordenada)
          {
            if (Abscisa >= Contorno[i].Abscisa || Abscisa >= Contorno[Otro].Abscisa)
            {
              Cantidad++;
            }
          }
          else
          {
            double AbscInterseccion = Contorno[Otro].Abscisa + (Contorno[i].Abscisa - Contorno[Otro].Abscisa) *
                (Ordenada - Contorno[Otro].Ordenada) / (Contorno[i].Ordenada - Contorno[Otro].Ordenada);
            if (AbscInterseccion <= Abscisa)
            {
              Cantidad++;
            }
          }
        }
      }
      return ((Cantidad % 2) != 0);
    }

    public void Seleccionar()
    {
      Seleccionado = true;
    }

  }

}
