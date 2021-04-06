using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Client.Clases;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaDefinirRectangulos : ComponentBase
	{

    public delegate void FncRetornarCoordenadas(double Abscisa, double Ordenada, double Ancho, double Alto);

    public FncRetornarCoordenadas AlRetornar { get; set; }
    public bool MouseAbajo { get; set; } = false;
    public double Abscisa { get; set; } = -1;
    public double Ordenada { get; set; } = -1;
    public double Ancho { get; set; } = -1;
    public double Alto { get; set; } = -1;
    public static List<CRect> Rectangulos { get; set; } = new List<CRect>();

		public Task PointerDown(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
      MouseAbajo = true;
      Abscisa = e.OffsetX;
      Ordenada = e.OffsetY;
      Ancho = 0;
      Alto = 0;
      StateHasChanged();
      return Task.CompletedTask;
    }

    public Task PointerUp(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
      MouseAbajo = false;
      if (AlRetornar != null)
			{
        AlRetornar(Abscisa, Ordenada, Ancho, Alto);
			}
      return Task.CompletedTask;
    }

    public Task PointerMove(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
      if (MouseAbajo)
      {
        if (e.OffsetX < Abscisa)
        {
          Ancho += Abscisa - e.OffsetX;
          Abscisa = e.OffsetX;
        }
        else
        {
          Ancho = e.OffsetX - Abscisa;
        }
        if (e.OffsetY < Ordenada)
        {
          Alto += Ordenada - e.OffsetY;
          Ordenada = e.OffsetY;
        }
        else
        {
          Alto = e.OffsetY - Ordenada;
        }
        StateHasChanged();
      }
      return Task.CompletedTask;
    }

	}

}
