using System;
using System.Collections.Generic;
using System.Text;

namespace TableroPecasV5.Shared
{
	public class CBlockPlantilla
	{
    public List<CComentarioPasoCN> Comentarios { get; set; }

    public DateTime FechaImpuesta { get; set; }

    public double FraccionAbscisas { get; set; }

    public double FraccionAncho { get; set; }

    public double FraccionOrdenadas { get; set; }

    public double LatMaxima { get; set; }

    public double LatMinima { get; set; }

    public double LngMaxima { get; set; }

    public double LngMinima { get; set; }

    public string Nombre { get; set; }

    public List<CPasoGraficoCN> Pasos { get; set; }

    public CBlockPlantilla()
		{
      Comentarios = new List<CComentarioPasoCN>();
      Pasos = new List<CPasoGraficoCN>();
		}

  }

  public class CComentarioPasoCN
  {

    public ClaseComentario Clase { get; set; }

    public string Comentario { get; set; }

  }

  public class CPasoGraficoCN
  {

    public List<CCondicionFiltradorCN> CondicionesFiltrador { get; set; }

    public List<CGraficoCN> Graficos { get; set; }

    public CPasoGraficoCN()
		{
      CondicionesFiltrador = new List<CCondicionFiltradorCN>();
      Graficos = new List<CGraficoCN>();
		}

  }

  public class CCondicionFiltradorCN
  {

    public int Coincidencias { get; set; }

    public string NombreColumna { get; set; }
    public string NombreColumnaAND { get; set; }

    public double RangoMaximo { get; set; }

    public double RangoMinimo { get; set; }
    public List<string> ValoresImpuestos { get; set; }

    public List<Int32> IndicesValoresImpuestos { get; set; }

    public CCondicionFiltradorCN()
		{
      ValoresImpuestos = new List<string>();
		}

  }


}
