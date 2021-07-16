using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaPaginaMimico: ComponentBase
  {
    [Inject]
    IJSRuntime JSRuntime { get; set; }

    private Int32 mCodigo = -1;

    [Parameter]
    public Int32 Codigo
    {
      get { return mCodigo; }
      set
      {
        if (mCodigo != value)
        {
          mCodigo = value;
          StateHasChanged();
        }
      }
    }

    private double AltoNecesario
    {
      get
      {
        if (Mimico == null)
        {
          return Contenedores.CContenedorDatos.AltoPantalla - 45;
        }
        else
        {
          return Mimico.AltoNecesario;
        }
      }
    }

    public string AnchoNecesario
    {
      get
      {
        if (Mimico == null)
        {
          return "100%";
        }
        else
        {
          return Mimico.AnchoNecesario.ToString() + "px";
        }
      }
    }

    private CMimico mMimico = null;
    public CMimico Mimico
    {
      get
      {
        return mMimico;
      }
      set
      {
        if (mMimico != value)
        {
          if (mMimico != null)
          {
            mMimico.AlRefrescarContenido -= MMimico_AlRefrescarContenido;
          }
          mMimico = value;
          if (mMimico != null)
          {
            mMimico.AlRefrescarContenido += MMimico_AlRefrescarContenido;
          }
        }
      }
    }

    private void MMimico_AlRefrescarContenido(object Referencia)
    {
      StateHasChanged();
    }

    public string EstiloMimico
    {
      get
      {
        if (Mimico != null && Mimico.HayImagen)
        {
          return "width: " + Mimico.AnchoPantallaCompleta.ToString() + "px; top: 0px; left: 0px; height: " +
              Mimico.AltoPantallaCompleta.ToString() +
              "px; overflow-x: visible; overflow-y: hidden; text-align: left; position: absolute;";
        }
        else
        {
          return "width: 100%; top: 0px; left: 0px; height: 100%; overflow: auto; text-align: left; position: absolute;";
        }
      }
    }


  }
}
