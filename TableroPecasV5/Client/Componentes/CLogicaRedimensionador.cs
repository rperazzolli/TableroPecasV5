using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace TableroPecasV5.Client.Componentes
{
  public class CLogicaRedimensionador: ComponentBase
  {

    public delegate void FncNotificar(string Texto);
    public event FncNotificar AlIniciarRedimensionado;
    public event FncNotificar AlCerrarRedimensionado;
    public event FncNotificar AlRedimensionar;

    [Parameter]
    public string Identificacion { get; set; } = "";

    [Parameter]
    public Int32 MargenSuperior { get; set; } = 0;

    public BECanvas CanvasGrafico { get; set; }

    [Inject]
    private IJSRuntime Runtime { get; set; }

    private void AgregarEventoJS()
    {
 //     Runtime.InvokeAsync<Int32>("FuncionesJS.AgregarEventoMouseDown", Identificacion);
    }

    private Canvas2DContext mContexto;

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
      if (firstRender)
      {
        AgregarEventoJS();
      }

      mContexto = await Blazor.Extensions.CanvasContextExtensions.CreateCanvas2DAsync(CanvasGrafico);

      await mContexto.BeginBatchAsync();

      try
      {

        await mContexto.ClearRectAsync(0, 0, 10, 10);
        await mContexto.SetFillStyleAsync("red");
        await mContexto.FillRectAsync(0, 0, 10, 10);

        await mContexto.BeginPathAsync();
        await mContexto.SetStrokeStyleAsync("gray");
        await mContexto.SetLineWidthAsync(1);
        await mContexto.MoveToAsync(10, 0);
        await mContexto.LineToAsync(0, 10);
        await mContexto.MoveToAsync(10, 5);
        await mContexto.LineToAsync(5, 10);

        await mContexto.ClosePathAsync();

      }
      catch (Exception)
      {
        //
      }
      finally
      {
        await mContexto.EndBatchAsync();
      }

    }

    private bool mbMouseDown = false;

    public void IniciarRedimensionado()
    {
      mbMouseDown = true;
      if (AlIniciarRedimensionado != null)
      {
        //AlIniciarRedimensionado(e.ClientX.ToString() + ";" + e.ClientY.ToString() + ";" +
        //  e.ScreenX.ToString() + ";" + e.ScreenY.ToString());
      }
    }

    public void CerrarRedimensionado(MouseEventArgs e)
    {
      if (mbMouseDown)
      {
        mbMouseDown = false;
        if (AlCerrarRedimensionado != null)
        {
          AlCerrarRedimensionado(Identificacion);
        }
      }
    }

    public void Redimensionar(MouseEventArgs e)
    {
      if (mbMouseDown)
      {
        if (AlRedimensionar != null)
        {
          AlRedimensionar(e.ScreenX.ToString() + ";" + e.ScreenY.ToString());
        }
      }
    }

  }
}
