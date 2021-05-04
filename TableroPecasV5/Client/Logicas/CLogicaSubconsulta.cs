using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaSubconsulta : CLogicaIndicador
	{

		public static string gParametros = "";

		public CLogicaSubconsulta()
		{
			mAlarmasImpuestas = new List<CInformacionAlarmaCN>();
			mAlarmasImpuestas.Add(new CInformacionAlarmaCN()
			{
				CodigoIndicador = Codigo,
				Color = "gray",
				Comentarios = new List<CComentarioCN>(),
				FechaInicial = DateTime.Now,
				Periodo = -1
			});
		}

		public string Prms { get; set; }

		public override string NombreIndicador
		{
			get
			{
				CSubconsultaExt SubC = Contenedores.CContenedorDatos.SubconsultaCodigo(Codigo);
				return (SubC == null ? "Indefinida" : SubC.Nombre);
			}
		}

		private async Task LeerSubconsultaAsync()
		{
			try
			{
				AguardandoFiltros = true;
				StateHasChanged();
				RespuestaDatasetBin Respuesta = await Http.GetFromJsonAsync<RespuestaDatasetBin>(
						"api/SubConsultas/LeerDataset?URL=" + Contenedores.CContenedorDatos.UrlBPI +
						"&Ticket=" + Contenedores.CContenedorDatos.Ticket +
						"&Codigo=" + Codigo.ToString() +
						"&Prms=" + Prms);

				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MsgErr);
				}

				HayFiltroDatos = true;

				ProcesarRespuestaDataset(Respuesta);

				AguardandoFiltros = false;

			}
			catch (Exception ex)
			{
				HayFiltroDatos = false;

				CRutinas.DesplegarMsg(
						new Exception("Al intentar acceder a información de alarmas" + Environment.NewLine + ex.Message));
			}
			finally
			{
				StateHasChanged();
			}
		}

		protected override Task OnAfterRenderAsync(bool firstRender)
		{
  		gPuntero = this;
	  	gPuntero.ClaseOrigen = ClaseElemento.SubConsulta;
			gPuntero.CodigoOrigen = Codigo;
			gPuntero.HayFiltroDatos = true;
			Prms = gParametros;

			if (!mbLeyo && Codigo >= 0 && Prms != null)
			{
				mbLeyo = true;
				_ = LeerSubconsultaAsync();
			}
			return base.OnAfterRenderAsync(firstRender);
		}
	}
}
