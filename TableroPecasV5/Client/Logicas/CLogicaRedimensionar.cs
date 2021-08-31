using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.Model;


namespace TableroPecasV5.Client.Logicas
{
    public class CLogicaRedimensionar : ComponentBase
    {

    protected BECanvas mCanvas = null;

    [Parameter]
    public bool MostrarLineas { get; set; } = true;

    public void ClickSobreRedimensionar()
		{

		}

    public BECanvas CanvasPropia
    {
      get { return mCanvas; }
      set { mCanvas = value; }
    }

    protected Canvas2DContext mContexto;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {

      if (MostrarLineas)
      {

        if (CanvasPropia == null)
        {
          await base.OnAfterRenderAsync(firstRender);
          return;
        }

        try
        {
          mContexto = await Blazor.Extensions.CanvasContextExtensions.CreateCanvas2DAsync(CanvasPropia);
          //
          await mContexto.BeginBatchAsync();

          try
          {

            await mContexto.ClearRectAsync(0, 0, 10, 10);
            await mContexto.BeginPathAsync();
            await mContexto.SetStrokeStyleAsync("gray");
            await mContexto.SetLineWidthAsync(1);
            await mContexto.MoveToAsync(10, 0);
            await mContexto.LineToAsync(0, 10);
            await mContexto.MoveToAsync(10, 7);
            await mContexto.LineToAsync(7, 10);
            await mContexto.MoveToAsync(10, 4);
            await mContexto.LineToAsync(4, 10);
            await mContexto.StrokeAsync();
            await mContexto.ClosePathAsync();

          }
          finally
          {
            await mContexto.EndBatchAsync();
          }

        }
        catch (Exception ex)
        {
          Rutinas.CRutinas.DesplegarMsg(ex);
        }
        finally
        {
          await base.OnAfterRenderAsync(firstRender);
        }
      }
      else
			{
        await base.OnAfterRenderAsync(firstRender);
			}
    }

  }
}
