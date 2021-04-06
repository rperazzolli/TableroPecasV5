using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Contenedores;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaCfgJerarquias : ComponentBase
  {

    [CascadingParameter]
    public CLogicaMimico Contenedor { get; set; }

    [Parameter]
    public Int32 Mimico { get; set; }

    [Parameter]
    public List<CTareaGraficaCN> Tareas { get; set; }

    public void Cerrar()
    {
      if (Contenedor != null)
      {
        Contenedor.EditandoReporte = false;
      }
    }

    private LineaTV mLineaEnArrastre = null;

    public void IniciarDragLinea(LineaTV Linea)
    {
      mLineaEnArrastre = Linea;
    }

    private void SaltearSiEsSuperior(LineaTV Receptor, LineaTV Desplazado)
		{
      if (Receptor.Superior == Desplazado)
			{
        Receptor.Jerarquia.Superior = Desplazado.Jerarquia.Superior;
			}
      else
			{
        if (Receptor.Jerarquia.Superior.Length>0 && Receptor.Superior != null)
				{
          SaltearSiEsSuperior(Receptor.Superior, Desplazado);
				}
			}
		}

    public void RecibirDrop(LineaTV Linea)
    {
      if (mLineaEnArrastre != null)
      {
        SaltearSiEsSuperior(Linea, mLineaEnArrastre);
        mLineaEnArrastre.Superior = Linea;
        mLineaEnArrastre.Jerarquia.Superior = Linea.Jerarquia.Inferior;
        Linea.TieneDependientes = true;
        OrdenarLineas();
        StateHasChanged();
      }
    }

    public string EstiloBloque
    {
      get
      {
        return "position: absolute; margin-left: " + (CContenedorDatos.AnchoPantalla / 5).ToString() +
          "px; margin-top: " + (CContenedorDatos.AltoPantalla / 10).ToString() +
          "px; width: " + (6 * CContenedorDatos.AnchoPantalla / 10).ToString() +
          "px; height: " + (6 * CContenedorDatos.AltoPantalla / 10).ToString() +
          "px; background: white; overflow: auto; padding: 0px;";

      }
    }

    private void AgregarDependientes(List<LineaTV> Ordenadas, LineaTV Linea, double Tabulacion)
    {
      List<LineaTV> Dependientes = (from L in Lineas
                                    where L.Jerarquia.Superior == Linea.Jerarquia.Inferior
                                    orderby L.Nombre
                                    select L).ToList();
      Linea.TieneDependientes = (Dependientes.Count > 0);
      foreach (LineaTV LineaLocal in Dependientes)
      {
        LineaLocal.Tabulacion = Tabulacion;
        Ordenadas.Add(LineaLocal);
        if (LineaLocal.TieneDependientes)
        {
          AgregarDependientes(Ordenadas, LineaLocal, Tabulacion + 25);
        }
      }
    }

    private void OrdenarLineas()
		{
      List<LineaTV> Ordenadas = new List<LineaTV>();
      foreach (LineaTV Linea in (from L in Lineas
                                 where L.Jerarquia.Superior.Length==0
                                 orderby L.Nombre
                                 select L).ToList())
			{
        Linea.Tabulacion = 0;
        Ordenadas.Add(Linea);
        if (Linea.TieneDependientes)
				{
          AgregarDependientes(Ordenadas, Linea, 25);
				}
			}
      Lineas = Ordenadas;
		}

    public string EstiloSubBloque
		{
      get
			{
        return "padding: 5px; margin-left: 5px; margin-right: 5px; margin-bottom: 5px; box-sizing: border-box; -moz-box-sizing: border-box;" +
            "-webkit-box-sizing: border-box; border: 2px solid gray; height: " +
            (6 * CContenedorDatos.AltoPantalla / 10 - 71).ToString() + "px; margin-top: 35px;";
			}
		}

    public async void RegistrarReporte()
		{
      Int32 Orden = 0;
      foreach (LineaTV Linea in Lineas)
			{
        Linea.Jerarquia.Orden = Orden++;
			}

      try
      {
        var Respuesta = await Http.PostAsJsonAsync<List<CJerarquiaCN>>("api/Jerarquias/RegistrarJerarquias?URL=" +
                Contenedores.CContenedorDatos.UrlBPI +
                "&Ticket=" + Contenedores.CContenedorDatos.Ticket+
                "&Mimico="+Mimico.ToString(), (from L in Lineas
                                               select L.Jerarquia).ToList());
        if (!Respuesta.IsSuccessStatusCode)
        {
          throw new Exception(Respuesta.ReasonPhrase);
        }

        Respuesta RespuestaWS = await Respuesta.Content.ReadFromJsonAsync<Respuesta>();
        if (!RespuestaWS.RespuestaOK)
        {
          throw new Exception(RespuestaWS.MsgErr);
        }


        Contenedor.EditandoReporte = false;

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    public string EstiloMasMenos(LineaTV Linea)
    {
      return "width: 25px; text-align: center; margin-left: " + Linea.Tabulacion.ToString() +
          "px; cursor: pointer; display: inline-block;";
    }

    public string EstiloLinea(LineaTV Linea)
		{
      return "margin-left: " + Linea.Tabulacion.ToString() + "px;";
		}

    public void AbrirCerrar(LineaTV Linea)
		{
      Linea.Abierto = !Linea.Abierto;
      StateHasChanged();
		}

    public List<LineaTV> Lineas { get; set; }

    [Inject]
    public HttpClient Http { get; set; }

    private void AgregarTareaBase(CTareaGraficaCN Tarea)
    {

      Lineas.Add(new LineaTV()
      {
        Nombre = Tarea.Descripcion,
        Jerarquia = new CJerarquiaCN()
        {
          Inferior = Tarea.Codigo,
          Superior = "",
          Mimico = Mimico,
          Orden = -1
        }
      });
    }

    private bool AgregarTareaJerarquia(CTareaGraficaCN Tarea, CJerarquiaCN Jerarquia)
		{
      LineaTV LineaSuperior = (from L in Lineas
                               where L.Jerarquia.Inferior == Jerarquia.Superior
                               select L).FirstOrDefault();
      if (LineaSuperior == null)
			{
        return true;
			}
      else
			{
        Lineas.Add(new LineaTV()
        {
          Nombre = Tarea.Descripcion,
          Jerarquia = Jerarquia,
          Superior = LineaSuperior
        });
        LineaSuperior.TieneDependientes = true;
        return false;
			}
		}

    private void AjustarOrdenTabulacion(string Superior, double Tabulacion)
		{
      Int32 Orden = 0;
      foreach (LineaTV Linea in (from L in Lineas
                                 where L.Jerarquia.Superior==Superior
                                 select L).ToList())
			{
        Linea.Jerarquia.Orden = Orden++;
        Linea.Tabulacion = Tabulacion;
        if (Linea.TieneDependientes)
				{
          AjustarOrdenTabulacion(Linea.Jerarquia.Inferior, Tabulacion + 25);
				}
			}
		}

    private void ArmarLineas(List<CJerarquiaCN> Jerarquias)
    {
      Lineas = new List<LineaTV>();
      bool HayPendientes = true;
      while (HayPendientes)
      {
        HayPendientes = false;
        foreach (CTareaGraficaCN Tarea in (from T in Tareas
                                           orderby T.Descripcion
                                           select T).ToList())
        {
          // Las ya incluidas no se procesan.
          if ((from L in Lineas
               where L.Jerarquia.Inferior == Tarea.Codigo
               select L).FirstOrDefault() != null)
          {
            continue;
          }

          // Busca jerarquia ya registrada.
          CJerarquiaCN Jerarquia = (from J in Jerarquias
                                    where J.Inferior == Tarea.Codigo
                                    select J).FirstOrDefault();
          if (Jerarquia == null || Jerarquia.Superior.Length == 0)
          {
            // No hay. Va al nivel 0.
            AgregarTareaBase(Tarea);
          }
          else
          {
            HayPendientes |= AgregarTareaJerarquia(Tarea, Jerarquia);
          }
        }
      }

      // determinar orden y tabulacion.
      AjustarOrdenTabulacion("", 0);

    }

    private async Task LeerJerarquiasAsync()
    {
      try
      {
        RespuestaJerarquias Respuesta = await Http.GetFromJsonAsync<RespuestaJerarquias>(
            "api/Jerarquias/LeerJerarquias?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
            "&Mimico=" + Mimico.ToString());
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MsgErr);
        }

        ArmarLineas(Respuesta.Jerarquias);

        OrdenarLineas();

        StateHasChanged();

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
		{
      if (Lineas == null)
			{
        _ = LeerJerarquiasAsync();
			}
			return base.OnAfterRenderAsync(firstRender);
		}
	}

  public class LineaTV
  {
    public string Nombre { get; set; }
    public CJerarquiaCN Jerarquia { get; set; }
    public bool Abierto { get; set; } = false;
    public string TextoMasMenos { get { return Abierto ? "-" : "+"; } }
    public double Tabulacion { get; set; } = 0;
    public LineaTV Superior { get; set; } = null;
    public bool TieneDependientes { get; set; } = false;
  }
}
