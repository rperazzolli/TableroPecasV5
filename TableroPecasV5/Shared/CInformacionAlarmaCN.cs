using System;
using System.Collections.Generic;
using System.Text;

namespace TableroPecasV5.Shared
{
	public class CInformacionAlarmaCN
	{

      public int CodigoIndicador { get; set; }

      public string Color { get; set; }

      public List<CComentarioCN> Comentarios { get; set; }

      public bool DatosParaFecha { get; set; }

      public int Dimension { get; set; }

      public int ElementoDimension { get; set; }

      public System.DateTime FechaDesde { get; set; }

      public System.DateTime FechaFinal { get; set; }

      public System.DateTime FechaHasta { get; set; }

      public System.DateTime FechaInicial { get; set; }

      public List<int> Instancias { get; set; }

      public double Minimo { get; set; }

      public int Periodo { get; set; }

      public double Satisfactorio { get; set; }

      public string Sentido { get; set; }

      public double Sobresaliente { get; set; }

      public double Tendencia { get; set; }

      public double Valor { get; set; }

      public double ValorAnterior { get; set; }

    public CInformacionAlarmaCN()
		{
      Comentarios = new List<CComentarioCN>();
      Instancias = new List<int>();
		}


  }
}
