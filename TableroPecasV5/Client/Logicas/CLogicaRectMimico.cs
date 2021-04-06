using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaRectMimico : ComponentBase
	{

		[Parameter]
		public Int32 CodigoPropio { get; set; } = -1;

		[Parameter]
		public bool Configurando { get; set; } = false;

		[Parameter]
		public bool EsProceso { get; set; } = false;

		[Parameter]
		public double Abscisa { get; set; }

		[Parameter]
		public double Ordenada { get; set; }

		private double mAncho;

		[Parameter]
		public double Ancho
		{
			get { return mAncho; }
			set
			{
				mAncho = value;
			}
		}

		public double AnchoPantalla
		{
			get { return Math.Floor(Factor * Ancho); }
		}

		public double AltoPantalla
		{
			get { return Math.Floor(Factor * Alto); }
		}

		private double mAlto;
		[Parameter]
		public double Alto
		{
			get { return mAlto; }
			set
			{
				mAlto = value;
			}
		}

		[Parameter]
		public string Texto { get; set; }

		[Parameter]
		public EventCallback<Int32> AlHacerClick { get; set; }

		[Parameter]
		public double Factor { get; set; }

		public bool Adentro
		{
			get { return mbAdentro; }
		}

		public double Ancho1
		{
			get { return Math.Floor(Factor * Ancho) - 1; }
		}

		public double Alto1
		{
			get
			{
				return Math.Floor(Factor * Alto) - 1;
			}
		}

		private bool mbAdentro = false;

		public string EstiloRectangulo
		{
			get
			{
				return "width: " + AnchoPantalla.ToString() + "px; height: " +
					AltoPantalla.ToString() +
					"px; position: absolute; padding: 0px;";
			}
		}

		public async void FuncionClick()
		{
			await AlHacerClick.InvokeAsync(CodigoPropio);
		}

		public void Entrando ()
		{
			if (!mbAdentro)
			{
				mbAdentro = true;
				StateHasChanged();
			}
		}

		public void Saliendo()
		{
			if (mbAdentro)
			{
				mbAdentro = false;
				StateHasChanged();
			}
		}

		protected override Task OnAfterRenderAsync(bool firstRender)
		{
			return base.OnAfterRenderAsync(firstRender);
		}
	}
}
