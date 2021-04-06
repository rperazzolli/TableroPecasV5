using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Shared;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaLogin: ComponentBase
  {

    public CLogicaLogin()
		{
		}

    private string mszUsuario = "";
    public string Usuario
    {
      get { return mszUsuario; }
      set
      {
        if (value != mszUsuario)
        {
          mszUsuario = value;
          Deshabilitado = (mszUsuario.Trim().Length == 0);
          StateHasChanged();                                                        
        }
      }
    }

    private string mszClave = "";
    public string Clave
    {
      get { return mszClave; }
      set { mszClave = value; }
    }

    private string mszMensajeLogin = "";
    public string MensajeLogin
    {
      get { return mszMensajeLogin; }
      set
      {
        if (mszMensajeLogin != value)
        {
          mszMensajeLogin = value;
          StateHasChanged();
        }
      }
    }

    [Inject]
    NavigationManager NavigationManager { get; set; }

    private bool mbDeshabilitado = false;
    public bool Deshabilitado
    {
      get { return mbDeshabilitado; }
      set { mbDeshabilitado = value; }
    }

    public bool Leyendo { get; set; }

    public void FncResize(object Aa)
    {

    }

    [Inject]
    public HttpClient Http { get; set; }

    [Inject]
    public NavigationManager Navegador { get; set; }

    public async void LoginUsuarioAsync()
    {
      if (mszUsuario.Length > 0)
      {
        try
        {
          MensajeLogin = "Login en marcha";
          StateHasChanged();
          CDatosLogin Datos = await Http.GetFromJsonAsync<CDatosLogin>(
            "api/Login/GetTicket?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Usuario=" + mszUsuario +
            "&Clave=" + mszClave);
          if (!Datos.RespuestaOK)
          {
            throw new Exception(Datos.MsgErr);
          }
          else
          {
            if (Datos.Ticket.Length == 0)
						{
              throw new Exception("Usuario o clave incorrecto");
						}
            Datos.SiempreTendencia = Datos.SiempreTendencia;
            Contenedores.CContenedorDatos.EsAdministrador = true; // Datos.Administrador;
            Contenedores.CContenedorDatos.Ticket = Datos.Ticket;
            Contenedores.CContenedorDatos.Usuario = Usuario;
            Contenedores.CContenedorDatos.Clave = Clave;
            Contenedores.CContenedorDatos.CodigoUsuario = Datos.CodigoUsuario;
            Contenedores.CContenedorDatos.TendenciasEnTarjeta = Datos.TendenciasEnTarjeta;
            Contenedores.CContenedorDatos.DesciendeEnRojo = Datos.DesciendeEnRojo;
            Contenedores.CContenedorDatos.SiempreTendencia = Datos.SiempreTendencia;
            Contenedores.CContenedorDatos.PoneEtiquetas = Datos.PoneEtiquetas;
            Contenedores.CContenedorDatos.RespetaSentido = Datos.RespetaSentido;
            Contenedores.CContenedorDatos.ImprimirPDF = Datos.ImprimirPDF;
            MensajeLogin = "Inicializando variables. Aguarde unos segundos.";
            StateHasChanged();
            MensajeLogin = await Contenedores.CContenedorDatos.InicializarDatosAsync(Http);
            StateHasChanged();
            NavigationManager.NavigateTo("Indicadores");
      //      CLogicaMenu.Refrescar();
      //      if (Logicas.CLogicaMenu.gPuntero!=null && Contenedores.CContenedorDatos.HabilitarIndicadores)
						//{
      //        Logicas.CLogicaMenu.gPuntero.OpcionMenuIndicadores();
						//}
          }
        }
        catch (Exception ex)
        {
          MensajeLogin = Rutinas.CRutinas.TextoMsg(ex);
        }
      }
    }
  }
}
