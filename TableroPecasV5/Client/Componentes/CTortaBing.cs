using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Componentes
{
	public class CTortaBing
	{
		public delegate void FncMouseSector(string Texto);

		public string CodigoTorta { get; set; }
		public CPosicionWFSCN Centro { get; set; }
		public double Lado { get; set; } = 1;
		public List<CGajoTorta> Gajos { get; set; }
		public double Acumulado { get; set; }

		//public async Task ResaltarSectorAsync(IJSRuntime JSRuntime, Int32 PosicionMapa, string Texto, double FactorAbsc)
		//{
		//	foreach (CGajoTorta Gajo in Gajos)
		//	{
		//		if (Gajo.Texto == Texto)
		//		{
		//			if (!Gajo.Resaltado)
		//			{
		//				Gajo.Resaltado = true;
		//				await EliminarPoligonoAsync(JSRuntime, Gajo.Texto, PosicionMapa);
		//				await DibujarGajoAsync(JSRuntime, Gajo, PosicionMapa, FactorAbsc);
		//			}
		//		}
		//		else
		//		{
		//			if (!Gajo.Resaltado)
		//			{
		//				Gajo.Resaltado = false;
		//				await EliminarPoligonoAsync(JSRuntime, Gajo.Texto, PosicionMapa);
		//				await DibujarGajoAsync(JSRuntime, Gajo, PosicionMapa, FactorAbsc);
		//			}
		//		}
		//	}
		//}

		public void SeleccionarGajo(string Referencia)
		{
			if (Gajos != null)
			{
				foreach (CGajoTorta Gajo in Gajos)
				{
					Gajo.Resaltado = Gajo.Referencia == Referencia;
				}
			}
		}

		public void DeterminarAcumulado()
		{
			Acumulado = 0;
			if (Gajos != null)
			{
				foreach (CGajoTorta Gajo in Gajos)
				{
					Acumulado += Math.Abs(Gajo.Valor);
				}
			}
		}

		public void SumarValor(string Texto, string Referencia, double Valor)
		{
			CGajoTorta Gajo = (from G in Gajos
												 where G.Referencia == Referencia
												 select G).FirstOrDefault();
			if (Gajo == null)
			{
				Gajo = new CGajoTorta()
				{
					Referencia = Referencia,
					Resaltado = false,
					Texto = Texto,
					Valor = 0
				};
				Gajos.Add(Gajo);
			}
			Gajo.Valor += Valor;
		}

		private const double SALTO_ANGULO = Math.PI / 90;

		private void AgregarPunto(double Distancia, double Angulo, List<double> Abscisas, List<double> Ordenadas,
				double FactAbsc)
		{
			Abscisas.Add(Centro.X + Distancia * Math.Cos(Angulo) * FactAbsc);
			Ordenadas.Add(Centro.Y + Distancia * Math.Sin(Angulo));
		}

		private async Task EliminarPoligonoAsync(IJSRuntime JSRuntime, string Referencia, Int32 Posicion)
		{
			object[] Args = new object[2];
			Args[0] = Posicion;
			Args[1] = "$$" + Posicion.ToString() + "$$" + CodigoTorta + "$$" + Referencia;
			await JSRuntime.InvokeAsync<Task>("EliminarPoligono", Args);
		}

		private async Task DibujarGajoAsync(IJSRuntime JSRuntime, CGajoTorta Gajo, Int32 Posicion, double FactAbsc,
			  double FactorEscala)
		{
			double DeltaAngulo = 2 * Math.PI * (mbValorUnitario ? 1 : Math.Abs(Gajo.Valor)) / Acumulado;
			List<double> Abscisas = new List<double>();
			List<double> Ordenadas = new List<double>();
			double LadoLocal = FactorEscala * (Gajo.Resaltado ? 1.5 * Lado : Lado);
			double AnguloMedio = Gajo.Angulo + DeltaAngulo / 2;
			double Saltos = Math.Max(2, DeltaAngulo / SALTO_ANGULO);

			AgregarPunto(LadoLocal / 2, Gajo.Angulo, Abscisas, Ordenadas, FactAbsc);
			AgregarPunto(LadoLocal, Gajo.Angulo, Abscisas, Ordenadas, FactAbsc);

			for (double i = 0; i <= Saltos; i++)
			{
				AgregarPunto(LadoLocal, Gajo.Angulo + i * DeltaAngulo / Saltos, Abscisas, Ordenadas, FactAbsc);
			}

			AgregarPunto(LadoLocal, Gajo.Angulo + DeltaAngulo, Abscisas, Ordenadas, FactAbsc);

			for (double i = Saltos; i >= 0; i--)
			{
				AgregarPunto(LadoLocal / 2, Gajo.Angulo + i * DeltaAngulo / Saltos, Abscisas, Ordenadas, FactAbsc);
			}

			double AbscCentro = Centro.X + FactAbsc* LadoLocal * 0.75 * Math.Cos(Gajo.Angulo + DeltaAngulo / 2);
			double OrdCentro = Centro.Y + LadoLocal * 0.75 * Math.Sin(Gajo.Angulo + DeltaAngulo / 2);

			object[] Args = new object[10];
			Args[0] = Posicion;
			Args[1] = Abscisas.ToArray();
			Args[2] = Ordenadas.ToArray();
			Args[3] = AbscCentro;
			Args[4] = OrdCentro;
			Args[5] = Gajo.Color;
			Args[6] = Gajo.Texto;
			Args[7] = Gajo.Valor;
			Args[8] = "$$" + Posicion.ToString() + "$$" + CodigoTorta + "$$" + Gajo.Texto;
			Args[9] = 1;
			await JSRuntime.InvokeAsync<Task>("DibujarPoligono", Args);
		}

		public async Task GraficarSobreMapaAsync(IJSRuntime JSRuntime, Int32 PosicionMapa, double FactorAbsc,
			  double FactorEscala)
		{
			if (Gajos != null && Gajos.Count > 0)
			{
				Gajos = (from G in Gajos
								 where G.Valor != 0
								 select G).ToList();
				AjustarAngulos();
				foreach (CGajoTorta Gajo in Gajos)
				{
					await DibujarGajoAsync(JSRuntime, Gajo, PosicionMapa, FactorAbsc, FactorEscala);
				}
			}
		}

		private bool mbValorUnitario = false;

		private void AjustarAngulos()
		{
			Acumulado = (from G in Gajos
													select Math.Abs(G.Valor)).Sum();

			if (Acumulado == 0)
			{
				mbValorUnitario = true;
				Acumulado = Gajos.Count;
			}
			else
			{
				mbValorUnitario = false;
			}

			Gajos.Sort(delegate (CGajoTorta G1, CGajoTorta G2)
			{
				return Math.Abs(G1.Valor.CompareTo(Math.Abs(G2.Valor)));
			});

			double Acum = 0;
			foreach (CGajoTorta Gajo in Gajos)
			{
				Gajo.Angulo = 2 * Math.PI * Acum / Acumulado;
				Acum += (mbValorUnitario ? 1 : Math.Abs(Gajo.Valor));
			}
		}

	}

	public class CGajoTorta
	{
		public double Angulo { get; set; }
		public double Valor { get; set; }
		public string Texto { get; set; }
		public string Referencia { get; set; }
		public string Color { get; set; }
		public bool Resaltado { get; set; } = false;

	}
}
