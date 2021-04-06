using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Blazor.Extensions.Canvas;

namespace PruebaRef.Client
{
	public class CLogicaElemento : ComponentBase
	{
		private string mValor = "Antes";
		public string Valor
		{
			get { return mValor; }
			set
			{
				if (value != mValor)
				{
					mValor = value;
					StateHasChanged();
				}
			}
		}

		private BECanvas mCanvas = null;
		public BECanvas CanvasPropia
		{
			get { return mCanvas; }
			set { mCanvas = value; }
		}

		private Blazor.Extensions.Canvas.Canvas2D.Canvas2DContext mContexto = null;

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			try {
				if (CanvasPropia == null)
				{
					return;
				}
				mContexto = await Blazor.Extensions.CanvasContextExtensions.CreateCanvas2DAsync(CanvasPropia);
				await mContexto.BeginBatchAsync();
        try
        {
          double AnchoCanvas = 90;
          double AltoCanvas = 50;
          await mContexto.ClearRectAsync(0, 0, AnchoCanvas, AltoCanvas);
          await mContexto.SetFillStyleAsync("red");
          await mContexto.FillRectAsync(0, 0, AnchoCanvas, AltoCanvas);

        }
        finally
        {
          await mContexto.EndBatchAsync();
        }

      }
      catch (Exception ex)
			{
				Valor = ex.Message;
			}
	  }
	}
}
