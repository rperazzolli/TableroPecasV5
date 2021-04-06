using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaMenu : LayoutComponentBase
  {

    public static CLogicaMenu gPuntero = null;

    public CLogicaMenu()
    {
      gPuntero = this;
      Rutinas.CRutinas.gFncInformarUsuario = MostrarMsg;
    }

    private void MostrarMsg(string Msg1, string Msg2)
    {
      LineaMsg1 = Msg1;
      LineaMsg2 = Msg2;
      HayMsg = true;
      StateHasChanged();
    }

    public void OpcionMenuIndicadores(Int32 Codigo)
    {
      if (Codigo < 0)
      {
        return;
      }
    }

    public bool HayMsg { get; set; } = false;
    public string LineaMsg1 { get; set; } = "";
    public string LineaMsg2 { get; set; } = "";

    public void OcultarMsg()
    {
      LineaMsg1 = "";
      LineaMsg2 = "";
      HayMsg = false;
    }

    [Inject]
    NavigationManager NavigationManager { get; set; }

    [Inject]
    public IJSRuntime JSRuntime { get; set; }

    private static double gDimensionCaracter = -1;

    private async Task DeterminarAnchoOpcionesAsync(Int32 CodSala)
    {
      List<string> gOpciones = (from CSolapaCN Solapa in Contenedores.CContenedorDatos.SolapasEnSala(CodSala)
                                select Solapa.Nombre).ToList();
      object[] Args = new object[3];
      Args[0] = 12;
      Args[1] = "serif";
      string Aa = "";
      foreach (string Opcion in gOpciones)
      {
        if (Opcion.Length > Aa.Length)
        {
          Aa = Opcion;
        }
      }
      Args[2] = Aa.Length;

      double R = await JSRuntime.InvokeAsync<double>("FuncionesJS.ObtenerDimensiones", Args);
      Contenedores.CContenedorDatos.AnchoOpcionSolapa = (long)Math.Floor(R + 0.5);


      if (gDimensionCaracter < 0)
      {
        Args[0] = "H";
        Args[2] = 12;
        gDimensionCaracter = await JSRuntime.InvokeAsync<double>("FuncionesJS.ObtenerDimensionTexto", Args);
      }

      Contenedores.CContenedorDatos.AltoOpcionSolapa = (long)(Math.Floor(gDimensionCaracter + 0.5) + 18);

    }

    public void OpcionMenuIndicadores()
    {
      NavigationManager.NavigateTo("Indicadores", false);
    }

    public async void OpcionMenuSalas(Int32 Codigo)
    {
      await DeterminarAnchoOpcionesAsync(Codigo);
      NavigationManager.NavigateTo("SalaReunion/" + Codigo.ToString(), false);
    }

    public void OpcionMenuMapas(Int32 Codigo)
    {
      NavigationManager.NavigateTo("BingMaps/" + Codigo.ToString(), false);
      if (Codigo < 0)
      {
        return;
      }
    }

    public void OpcionMenuMimicos(Int32 Codigo)
    {
      NavigationManager.NavigateTo("Mimicos/" + Codigo.ToString(), false);
      if (Codigo < 0)
      {
        return;
      }
    }

    public static void Refrescar()
    {
      gPuntero.StateHasChanged();
    }

  }
}
