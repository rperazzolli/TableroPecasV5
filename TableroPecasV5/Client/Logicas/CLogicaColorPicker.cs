using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaColorPicker	: ComponentBase
	{
		[Parameter]
		public bool Abierto { get; set; } = false;

		[Parameter]
		public Int32 Azul { get; set; } = 255;

		[Parameter]
		public Int32 Rojo { get; set; } = 255;

		[Parameter]
		public Int32 Verde { get; set; } = 255;

		public string ColorRGB
		{
			get
			{
				return "rgb( " + Rojo.ToString() + ", " + Verde.ToString() + ", " + Azul.ToString() + ")";
			}
		}

		private BECanvas mCanvasColores = null;

		public BECanvas CanvasColores
		{
			get { return mCanvasColores; }
			set { mCanvasColores = value; }
		}
		public void CambiarAbierto()
		{
			Abierto = !Abierto;
		}

		public string EstiloGlobal
		{
			get
			{
				return "width: 350px; height: " + (Abierto ? 350 : 30).ToString() + ";";
			}
		}

		public string EstiloZonaColor
		{
			get
			{
				return "width: 25px; height: 20px; position: absolute; margin-left: 5px; margin-top: 5px; background: " +
					ColorRGB + ";";
			}
		}

		private Canvas2DContext mContexto = null;
		private static readonly int[] COLORES_8 = {0, 36, 73, 109, 145, 182, 218, 255 };
		private static readonly int[] COLORES_4 = { 0, 85, 170, 255 };

		private async Task DibujarColoresAsync()
		{
			mContexto = await Blazor.Extensions.CanvasContextExtensions.CreateCanvas2DAsync(mCanvasColores);
			await mContexto.BeginBatchAsync();

			for (Int32 iRojo = 0; iRojo < 8; iRojo++)
			{
				for (Int32 iVerde = 0; iVerde < 8; iVerde++)
				{
					for (Int32 iAzul = 0; iAzul < 4; iAzul++)
					{
						Int32 Posicion = 32 * iAzul + 8 * iVerde + iRojo;
						Int32 Columna = Posicion % 16;
						Int32 Fila = (Posicion - Columna) / 16;
						string Color = "rgb( " + COLORES_8[iRojo].ToString() + ", " +
							COLORES_8[iVerde].ToString() + ", " + COLORES_4[iAzul].ToString() + ")";
						await mContexto.SetFillStyleAsync(Color);
						await mContexto.FillRectAsync(20 * Columna, 15 * Fila, 20, 15);
						if (Rojo==COLORES_8[iRojo] && Verde==COLORES_8[iVerde] && Azul == COLORES_4[iAzul])
						{
							await mContexto.SetStrokeStyleAsync("black");
							await mContexto.StrokeRectAsync(20 * Columna, 15 * Fila, 20, 15);
						}
					}
				}
			}

			await mContexto.EndBatchAsync();

		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (Abierto && mCanvasColores != null)
			{
				await DibujarColoresAsync();
			}
		}

	}
}
