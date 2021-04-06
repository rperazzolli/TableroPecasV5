using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Blazorise;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Listas;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaPedirSubconsultas : ComponentBase
	{
		private List<CSubconsultaExt> mSubconsultas = new List<CSubconsultaExt>();
		private CSubconsultaExt mSubconsulta = null;
		public delegate void FncAjustarListaDependiente(string CampoSucio);

		public bool Aguardando { get; set; } = false;

		public Modal Dialogo { get; set; }

		[Inject]
		public NavigationManager Navegador { get; set; }

		public List<DatosPrmSubconsulta> Parametros { get; set; }

		[Parameter]
		public Int32 Codigo { get; set; }

		[Inject]
		public HttpClient Http { get; set; }

		public FncAjustarListaDependiente AlSeleccionar { get; set; }

		public async void Abrir()
		{
			CLogicaSubconsulta.gParametros = await ArmarListaPrmsAsync(null);
			if (AlSeleccionar == null)
			{
				Navegador.NavigateTo("Subconsulta/" + mSubconsulta.Codigo.ToString());
			}
			else
			{
				AlSeleccionar(CLogicaSubconsulta.gParametros);
			}
		}

		public void Ignorar()
		{
			if (AlSeleccionar == null)
			{
				Navegador.NavigateTo("Indicadores");
			}
			else
			{
				AlSeleccionar(null);
			}
		}


		public void Cerrando(Blazorise.ModalClosingEventArgs e)
		{
			switch (e.CloseReason)
			{
				case CloseReason.EscapeClosing:
				case CloseReason.FocusLostClosing:
					e.Cancel = true;
					break;
			}
		}

		private void AjustarCombosAsociados(string Nombre)
		{
			_ = IntentarRefrescarCombosPorCambiarPrmAsync(Nombre);
		}

		private void CrearParametros()
		{
			Parametros = new List<DatosPrmSubconsulta>();
			foreach (CParametroExt Prm in mSubconsulta.Parametros)
			{
				Parametros.Add(new DatosPrmSubconsulta(Prm, AjustarCombosAsociados));
			}
		}

		private DatosPrmSubconsulta DatosParaPrm(string Nombre)
		{
			return (from D in Parametros
							where D.Parametro.Nombre == Nombre
							select D).FirstOrDefault();
		}

		private const string SEP = ")$$(";

		private async Task<string> ArmarListaPrmsAsync(DatosPrmSubconsulta Datos)
		{
			CSubconsultaExt SubC = (Datos == null ? mSubconsulta :
					await ObtenerSubconsultaCodigoAsync(Datos.Parametro.CodigoSubconsulta));
			if (SubC == null)
			{
				throw new Exception("Falta SC para prm " + Datos.Parametro.Nombre);
			}
			List<DatosPrmSubconsulta> Asociados = (from P in SubC.Parametros
																						 select (from D in Parametros
																										 where D.Parametro.Nombre == P.Nombre
																										 select D).FirstOrDefault()).ToList();
			if (Asociados.Contains(null))
			{
				throw new Exception("Falta parámetro en subconsulta de prm " + Datos.Parametro.Nombre);
			}

			string Respuesta = Asociados.Count.ToString();
			foreach (DatosPrmSubconsulta Dato in Asociados)
			{
				Respuesta += SEP + Dato.Parametro.CodigoSubconsulta.ToString() + SEP +
						Dato.Parametro.Nombre + SEP + (Dato.Parametro.TieneQuery ? "S" : "N") +
						SEP + Dato.Parametro.Tipo + SEP + CRutinas.FechaATexto((DateTime)Dato.ValorFecha) +
						SEP + CRutinas.FloatVStr(Dato.ValorReal) + SEP + Dato.ValorEntero.ToString() +
						SEP + Dato.ValorTexto;
			}

			return "(" + Respuesta + ")";

		}

		private async Task CargarListaComboAsync(DatosPrmSubconsulta Datos)
		{
			try
			{
				string Prms = await ArmarListaPrmsAsync(Datos);
				RespuestaQuerySubconsultas Respuesta = await Http.GetFromJsonAsync<RespuestaQuerySubconsultas>(
						"api/SubConsultas/ListarValoresQuerySC?URL=" + Contenedores.CContenedorDatos.UrlBPI +
						"&Ticket=" + Contenedores.CContenedorDatos.Ticket +
						"&Codigo=" + Datos.Parametro.CodigoSubconsulta.ToString() +
						"&NombreCampo="+Datos.Parametro.Nombre+
						"&Prms=" + Prms);

				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MsgErr);
				}

				Datos.Lista.Clear();
				for (Int32 i = 0; i < Respuesta.Valores.Count; i++)
				{
					Datos.Lista.Add(new CListaDoble(i, Respuesta.Valores[i], Respuesta.Textos[i]));
				}


			}
			catch (Exception ex)
			{
				throw new Exception("Al intentar listar datos combo" + Environment.NewLine + ex.Message);
			}
		}

		private async Task<CSubconsultaExt> LeerSubconsultaAsync(Int32 Codigo)
		{
			try
			{
				RespuestaSubconsultas Respuesta = await Http.GetFromJsonAsync<RespuestaSubconsultas>(
						"api/SubConsultas/ObtenerParametrosConSC?URL=" + Contenedores.CContenedorDatos.UrlBPI +
						"&Ticket=" + Contenedores.CContenedorDatos.Ticket +
						"&Codigo=" + Codigo.ToString());

				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MsgErr);
				}

				if (Respuesta.Subconsultas==null || Respuesta.Subconsultas.Count < 1)
				{
					return null;
				}

				return Respuesta.Subconsultas[0];

			}
			catch (Exception ex)
			{
				throw new Exception("Al obtener prms SC" + Environment.NewLine + ex.Message);
			}
		}

		private async Task<CSubconsultaExt> ObtenerSubconsultaCodigoAsync(Int32 Codigo)
		{
			CSubconsultaExt Respuesta = Contenedores.CContenedorDatos.SubconsultaCodigo(Codigo);
			if (Respuesta == null)
			{
				Respuesta = (from C in mSubconsultas
										 where C.Codigo == Codigo
										 select C).FirstOrDefault();
				if (Respuesta == null)
				{
					Respuesta = await LeerSubconsultaAsync(Codigo);
					if (Respuesta != null)
					{
						mSubconsultas.Add(Respuesta);
					}
				}
			}
			return Respuesta;
		}

		private async Task IntentarRefrescarCombosAsync()
		{
			Aguardando = true;
			StateHasChanged();
			try
			{
				foreach (CParametroExt Prm in mSubconsulta.Parametros)
				{
					if (Prm.TieneQuery && Prm.CodigoSubconsulta >= 0)
					{
						CSubconsultaExt SubC = await ObtenerSubconsultaCodigoAsync(Prm.CodigoSubconsulta);
						if (SubC != null)
						{
							DatosPrmSubconsulta Datos = DatosParaPrm(Prm.Nombre);
							if (Datos != null)
							{
								await CargarListaComboAsync(Datos);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				CRutinas.DesplegarMsg(ex);
			}
			finally
			{
				Aguardando = false;
				StateHasChanged();
			}
		}

		private bool SubconsultaIncluyePrm(CSubconsultaExt SubC, string PrmName)
		{
			return (from P in SubC.Parametros
							where P.Nombre == PrmName
							select P).FirstOrDefault() != null;
		}

		private async Task IntentarRefrescarCombosPorCambiarPrmAsync(string PrmCambiado)
		{
			Aguardando = true;
			StateHasChanged();
			try
			{
				foreach (CParametroExt Prm in mSubconsulta.Parametros)
				{
					if (Prm.TieneQuery && Prm.CodigoSubconsulta >= 0)
					{
						CSubconsultaExt SubC = await ObtenerSubconsultaCodigoAsync(Prm.CodigoSubconsulta);
						if (SubC != null)
						{
							if (SubconsultaIncluyePrm(SubC, PrmCambiado))
							{
								DatosPrmSubconsulta Datos = DatosParaPrm(Prm.Nombre);
								await CargarListaComboAsync(Datos);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				CRutinas.DesplegarMsg(ex);
			}
			finally
			{
				Aguardando = false;
				StateHasChanged();
			}
		}

		public string Nombre
		{
			get
			{
				return mSubconsulta == null ? "" : mSubconsulta.Nombre;
			}
		}

		private bool HayCombos()
		{
			return (from P in mSubconsulta.Parametros
							where P.TieneQuery
							select P).FirstOrDefault() != null;
		}

		protected override Task OnAfterRenderAsync(bool firstRender)
		{
			if (mSubconsulta == null)
			{
				mSubconsulta = Contenedores.CContenedorDatos.SubconsultaCodigo(Codigo);
				if (mSubconsulta == null)
				{
					throw new Exception("No encuentra subconsulta");
				}
				CrearParametros();
				if (HayCombos())
				{
					_ = IntentarRefrescarCombosAsync();
				}
			}
			if (Dialogo != null && !Dialogo.Visible)
			{
				Dialogo.Show();
			}
			return base.OnAfterRenderAsync(firstRender);
		}

	}

	public class DatosPrmSubconsulta
	{
		public CParametroExt Parametro { get; set; }

		private string mszValorTexto = "";
		public string ValorTexto
		{
			get { return mszValorTexto; }
			set
			{
				if (value != mszValorTexto)
				{
					mszValorTexto = value;
					Ensuciar();
				}
			}
		}

		public bool EsEntero
		{
			get { return (Parametro.Tipo.Contains("INT", StringComparison.InvariantCultureIgnoreCase)); }
		}

		public bool EsReal
		{
			get { return (Parametro.Tipo.Contains("DOUBLE", StringComparison.InvariantCultureIgnoreCase) ||
					  Parametro.Tipo.Contains("FLOAT", StringComparison.InvariantCultureIgnoreCase)); }
		}

		public bool EsFecha
		{
			get { return (Parametro.Tipo.Contains("DATE", StringComparison.InvariantCultureIgnoreCase)); }
		}

		private DateTime mdtFecha = CRutinas.FechaInicioDia(DateTime.Now);
		public DateTime? ValorFecha
		{
			get { return (DateTime?)mdtFecha; }
			set
			{
				if (mdtFecha != (DateTime)value)
				{
					mdtFecha = (DateTime)value;
					Ensuciar();
				}
			}
		}

		private double mdValor = 0;
		public double ValorReal
		{
			get { return mdValor; }
			set
			{
				if (mdValor != value)
				{
					mdValor = value;
					Ensuciar();
				}
			}
		}

		private Int32 miValor = 0;
		public Int32 ValorEntero
		{
			get { return miValor; }
			set
			{
				if (miValor != value)
				{
					miValor = value;
					Ensuciar();
				}
			}
		}

		private void Ensuciar()
		{
			if (mFncAjuste != null)
			{
				mFncAjuste(Parametro.Nombre);
			}
		}

		public List<CListaDoble> Lista { get; set; } = null;

		private CLogicaPedirSubconsultas.FncAjustarListaDependiente mFncAjuste = null;

		public string ValorLista
		{
			get
			{
				if (EsEntero)
				{
					return ValorEntero.ToString();
				}
				else
				{
					if (EsReal)
					{
						return ValorReal.ToString();
					}
					else
					{
						if (EsFecha)
						{
							return ValorFecha.ToString();
						}
						else
						{
							return ValorTexto;
						}
					}
				}
			}
			set
			{
				if (EsEntero)
				{
					ValorEntero = Int32.Parse(value);
				}
				else
				{
					if (EsReal)
					{
						ValorReal = double.Parse(value);
					}
					else
					{
						if (EsFecha)
						{
							ValorFecha = DateTime.Parse(value);
						}
						else
						{
							ValorTexto = value;
						}
					}
				}
			}
		}

		public void CambioValor(ChangeEventArgs e)
		{
			string NuevoValor = e.Value.ToString();
			try
			{
				if (EsEntero)
				{
					ValorEntero = Int32.Parse(NuevoValor);
				}
				else
				{
					if (EsReal)
					{
						ValorReal = double.Parse(NuevoValor);
					}
					else
					{
						if (EsFecha)
						{
							ValorFecha = DateTime.Parse(NuevoValor);
						}
						else
						{
							ValorTexto = NuevoValor;
						}
					}
				}
			}
			catch (Exception ex)
			{
				CRutinas.DesplegarMsg(ex);
			}
		}

		public DatosPrmSubconsulta (CParametroExt Prm, CLogicaPedirSubconsultas.FncAjustarListaDependiente Fnc)
		{
			Parametro = Prm;
			Parametro.Tipo = Parametro.Tipo;
			mFncAjuste = Fnc;
			Lista = new List<CListaDoble>();
		}
	}
}
