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

		public async Task ResaltarSectorAsync(IJSRuntime JSRuntime, Int32 PosicionMapa, string Texto)
		{
			foreach (CGajoTorta Gajo in Gajos)
			{
				if (Gajo.Texto == Texto)
				{
					if (!Gajo.Resaltado)
					{
						Gajo.Resaltado = true;
						await EliminarPoligonoAsync(JSRuntime, Gajo.Texto, PosicionMapa);
						await DibujarGajoAsync(JSRuntime, Gajo, PosicionMapa);
					}
				}
				else
				{
					if (!Gajo.Resaltado)
					{
						Gajo.Resaltado = false;
						await EliminarPoligonoAsync(JSRuntime, Gajo.Texto, PosicionMapa);
						await DibujarGajoAsync(JSRuntime, Gajo, PosicionMapa);
					}
				}
			}
		}

		private const double SALTO_ANGULO = Math.PI / 90;

		private void AgregarPunto(double Distancia, double Angulo, List<double> Abscisas, List<double> Ordenadas)
		{
			Abscisas.Add(Centro.X + Distancia * Math.Cos(Angulo));
			Ordenadas.Add(Centro.Y + Distancia * Math.Sin(Angulo));
		}

		private void DesplazarPorResaltado(double Angulo, List<double> Abscisas, List<double> Ordenadas,
			  ref double AbscCentro, ref double OrdCentro)
		{
			double DespAbsc = Lado * Math.Cos(Angulo) / 4;
			double DespOrd = Lado * Math.Sin(Angulo) / 4;
			AbscCentro += DespAbsc;
			OrdCentro += DespOrd;
			for (Int32 i = 0; i < Abscisas.Count; i++)
			{
				Abscisas[i] += DespAbsc;
				Ordenadas[i] += DespOrd;
			}
		}

		private async Task EliminarPoligonoAsync(IJSRuntime JSRuntime, string Referencia, Int32 Posicion)
		{
			object[] Args = new object[2];
			Args[0] = Posicion;
			Args[1] = "$$" + Posicion.ToString() + "$$" + CodigoTorta + "$$" + Referencia;
			await JSRuntime.InvokeAsync<Task>("EliminarPoligono", Args);
		}

		private async Task DibujarGajoAsync(IJSRuntime JSRuntime, CGajoTorta Gajo, Int32 Posicion)
		{
			double DeltaAngulo = 2 * Math.PI * (mbValorUnitario ? 1 : Math.Abs(Gajo.Valor)) / Acumulado;
			List<double> Abscisas = new List<double>();
			List<double> Ordenadas = new List<double>();
			double AnguloMedio = Gajo.Angulo + DeltaAngulo / 2;
			double Saltos = Math.Max(2, DeltaAngulo / SALTO_ANGULO);

			AgregarPunto(Lado / 2, Gajo.Angulo, Abscisas, Ordenadas);
			AgregarPunto(Lado, Gajo.Angulo, Abscisas, Ordenadas);

			for (double i = 0; i <= Saltos; i++)
			{
				AgregarPunto(Lado, Gajo.Angulo + i * DeltaAngulo / Saltos, Abscisas, Ordenadas);
			}

			AgregarPunto(Lado, Gajo.Angulo + DeltaAngulo, Abscisas, Ordenadas);

			for (double i = Saltos; i >= 0; i--)
			{
				AgregarPunto(Lado / 2, Gajo.Angulo + i * DeltaAngulo / Saltos, Abscisas, Ordenadas);
			}

			double AbscCentro = Centro.X + Lado * 0.75 * Math.Cos(Gajo.Angulo + DeltaAngulo / 2);
			double OrdCentro = Centro.Y + Lado * 0.75 * Math.Sin(Gajo.Angulo + DeltaAngulo / 2);

			if (Gajo.Resaltado)
			{
				DesplazarPorResaltado(Gajo.Angulo + DeltaAngulo / 2, Abscisas, Ordenadas, ref AbscCentro, ref OrdCentro);
			}

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

		public async Task GraficarSobreMapaAsync(IJSRuntime JSRuntime, Int32 PosicionMapa)
		{
			if (Gajos != null && Gajos.Count > 0)
			{
				AjustarAngulos();
				foreach (CGajoTorta Gajo in Gajos)
				{
					await DibujarGajoAsync(JSRuntime, Gajo, PosicionMapa);
				}
			}
		}

		private double Acumulado;
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
		public string Color { get; set; }
		public bool Resaltado { get; set; } = false;

	}
}
