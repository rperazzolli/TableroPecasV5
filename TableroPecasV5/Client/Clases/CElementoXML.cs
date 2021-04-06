using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Client.Plantillas;

namespace TableroPecasV5.Client.Clases
{
	public enum CClaseElementoXML
	{
		Grafico = 1,
		Indicador = 2,
		Tendencia = 3,
		Tarjeta = 4,
		Mapa = 5,
		Varios = 6
	}

	public class CElementoXML
	{
		public CClaseElementoXML Clase { get; set; }
		public string Nombre { get; set; }
		public CGrafV2DatosContenedorBlock GrafV2 { get; set; }

		public CRect Rectangulo
		{
			get
			{
				return new CRect()
				{
					Abscisa = GrafV2.Posicion.X,
					Ordenada = GrafV2.Posicion.Y,
					Ancho = GrafV2.Ancho,
					Alto = GrafV2.Alto
				};
			}
		}
	}
}
