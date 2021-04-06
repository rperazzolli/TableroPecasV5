using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.Model;
using System.Drawing;
using System.IO;
using System.Web;
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaTab : ComponentBase, IDisposable
  {

    public delegate void FncRefrescar(Int32 CodSolapa);

    public event FncRefrescar AlRefrescar;

    public static string gFuente = "12px serif";

    [Inject]
    public IJSRuntime JSRuntime { get; set; }

    private const double MARGEN_EXTERIOR = 5;
    private const double PADDING = 4;

    private bool mbBloquear = false;

    public void Dispose()
		{
      mbBloquear = true;
		}

    [Parameter]
    public long AnchoTotal { get; set; }

    [Parameter]
    public long AltoTotal { get; set; }

    [Parameter]
    public Int32 Codigo { get; set; }

    [Parameter]
    public string Texto { get; set; }

    [Parameter]
    public Int32 CodigoPrimeraSolapa { get; set; }

    public BECanvas CanvasTab;
    private Canvas2DContext mContexto;

    private bool mbSeleccionado = false;
    private bool mbEncima = false;

    public bool Seleccionada
    {
      get { return mbSeleccionado; }
      set { mbSeleccionado = value; }
    }

    public void EstaEncima(bool Encima)
    {
      mbEncima = Encima;
      if (!mbSeleccionado)
      {
        StateHasChanged();
      }
    }

    public void Redibujar()
		{
      StateHasChanged();
		}

    public void Seleccionar(bool SeleccionarElemento)
    {
      if (mbSeleccionado != SeleccionarElemento || Codigo < 0)
      {
        mbSeleccionado = SeleccionarElemento;
        if (mbSeleccionado && AlRefrescar != null)
        {
          AlRefrescar(Codigo);
        }
        else
        {
          StateHasChanged();
        }
      }
    }

    public string Estilo
    {
      get
      {
        return "width: " + AnchoTotal.ToString() + "px; Height: " + AltoTotal.ToString() +
            "px; margin-top: 0px; margin-left: 0px; padding: 0px;";
      }
    }

    private const double RAD_RECTANGULO = 4;

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
      if (CanvasTab != null && !mbBloquear)
      {

        mContexto = await Blazor.Extensions.CanvasContextExtensions.CreateCanvas2DAsync(CanvasTab);

        await mContexto.BeginBatchAsync();

        try
        {

          await mContexto.ClearRectAsync(0, 0, AnchoTotal, AltoTotal);

          await mContexto.SetFontAsync(gFuente);

          if (mbEncima || (mbSeleccionado && Codigo > 0))
          {
            await mContexto.BeginPathAsync();
            await mContexto.MoveToAsync(RAD_RECTANGULO, 0);
            await mContexto.LineToAsync(AnchoTotal - RAD_RECTANGULO, 0);
            await mContexto.ArcToAsync(AnchoTotal, 0, AnchoTotal, RAD_RECTANGULO, RAD_RECTANGULO);
            await mContexto.LineToAsync(AnchoTotal, AltoTotal);
            await mContexto.LineToAsync(0, AltoTotal);
            await mContexto.LineToAsync(0, RAD_RECTANGULO);
            await mContexto.ArcToAsync(0, 0, RAD_RECTANGULO, 0, RAD_RECTANGULO);
            await mContexto.ClosePathAsync();
            if (mbSeleccionado)
            {
              await mContexto.SetFillStyleAsync("white");
              await mContexto.FillAsync();
            }
            else
            {
              if (mbEncima)
              {
                await mContexto.SetStrokeStyleAsync("white");
                await mContexto.SetLineWidthAsync(1);
                await mContexto.StrokeAsync();
              }
            }
          }

          await mContexto.SetFillStyleAsync("#000000");
          await mContexto.SetTextBaselineAsync(TextBaseline.Middle);
          await mContexto.SetTextAlignAsync(TextAlign.Center);
          await mContexto.FillTextAsync(Texto, AnchoTotal / 2, AltoTotal / 2, AnchoTotal);

        }
        catch (Exception ex)
				{
          CRutinas.DesplegarMsg(ex);
				}
        finally
        {
          await mContexto.EndBatchAsync();
        }
      }
    }
  }

}

