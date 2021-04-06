using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaReporteIndicadores : ComponentBase
	{

		[CascadingParameter]
		public CLogicaMimico Contenedor { get; set; }

		[Parameter]
		public string NombreReporte { get; set; }

		[Parameter]
		public List<CLineaReporte> LineasReporte { get; set; }

		public void Cerrar()
		{
			if (Contenedor != null)
			{
				Contenedor.MostrandoReporte = false;
			}
		}

		public List<LineaImpresion> LineasImpresion { get; set; }

		protected override Task OnParametersSetAsync()
		{
			if (LineasReporte != null)
			{
				LineasImpresion = (from L in LineasReporte
													 select new LineaImpresion(L)).ToList();
			}
			return base.OnParametersSetAsync();
		}
	}

	public class LineaImpresion
	{

		public string Estilo
		{
			get { return "color: " + ColorLinea + ";"; }
		}

		public string Nombre { get; set; } = "";
		public string Indicador { get; set; } = "";
		public string Periodo { get; set; } = "";
		public string Unidades { get; set; } = "";
		public string Valor { get; set; } = "";
		public string Imagen { get; set; } = "";
		public string Minimo { get; set; } = "";
		public string Satisfactorio { get; set; } = "";
		public string Sobresaliente { get; set; } = "";

		public string UrlImagen
		{
			get
			{
				return "Imagenes/" + Imagen + ".png";
			}
		}

		public string ColorLinea { get; set; }

		public LineaImpresion(CLineaReporte Linea)
		{
			if (Linea.Indicador == null)
			{
				ColorLinea = "black";
				Nombre = Linea.Referencia;
			}
			else
			{
				ColorLinea = CRutinas.ColorEquivalente(CRutinas.ColorAlarma(Linea.DatosIndicador));
				Indicador =Linea.Indicador.Descripcion;
				if (Linea.DatosIndicador != null)
				{
					Periodo =Linea.DatosIndicador.FechaInicial.ToString(
							CRutinas.FormatoFechaPorFrecuencia(Linea.Indicador.Frecuencia));
					Valor = Linea.DatosIndicador.Valor.ToString(CRutinas.FormatoDecimales(Linea.Indicador.Decimales));
					Unidades = Linea.Indicador.Unidades;
					Imagen=CRutinas.ImagenDesdeTendencia(Linea.DatosIndicador);
					if (Linea.DatosIndicador.Minimo != Linea.DatosIndicador.Sobresaliente)
					{
						Minimo =Linea.DatosIndicador.Minimo.ToString(CRutinas.FormatoDecimales(Linea.Indicador.Decimales));
						Satisfactorio =Linea.DatosIndicador.Satisfactorio.ToString(CRutinas.FormatoDecimales(Linea.Indicador.Decimales));
						Sobresaliente = Linea.DatosIndicador.Sobresaliente.ToString(CRutinas.FormatoDecimales(Linea.Indicador.Decimales));
					}
				}
			}

		}

	}
}
