using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using TableroPecasV5.Client.Contenedores;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Listas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaVinculadorWFS : ComponentBase
	{
		[Inject]
		public HttpClient Http { get; set; }

		public CVinculoIndicadorCompletoCN Vinculador { get; set; }

		[Parameter]
		public Int32 Indicador { get; set; }

		[Parameter]
		public Datos.CColumnaBase ColumnaVinculo { get; set; }

		[Parameter]
		public ClaseElemento ClaseIndicador { get; set; } = ClaseElemento.Indicador;

		public List<CTextoTexto> ListaElementosCapa = new List<CTextoTexto>();

		public List<CCapaWFSCN> ListaCapas { get; set; } = null;

		private async Task LeerCapasAsync()
		{
			RespuestaCapasGIS Respuesta = await CContenedorDatos.LeerCapasWFSAsync(Http, true, true);
			if (Respuesta != null)
			{
				ListaCapas = Respuesta.CapasWFS;
				await LeerVinculoAsync();
			}
			else
			{
				StateHasChanged();
			}
		}

		private CVinculoIndicadorCompletoCN CrearVinculador()
		{
			Int32 CodAgregado = -1;
      CVinculoIndicadorCompletoCN Respuesta = new CVinculoIndicadorCompletoCN();
			Respuesta.Vinculo.Codigo = Vinculador.Vinculo.Codigo;
			Respuesta.Vinculo.ClaseIndicador = ClaseIndicador;
			Respuesta.Vinculo.ClaseVinculada = ClaseVinculo.Areas;
			Respuesta.Vinculo.CodigoIndicador = Indicador;
			Respuesta.Vinculo.CodigoVinculado = mCodigoCapa;
			Respuesta.Vinculo.ColumnaLat = "";
			Respuesta.Vinculo.ColumnaLng = "";
			Respuesta.Vinculo.NombreColumna = ColumnaVinculo.Nombre;
			Respuesta.Vinculo.Rango = 0;
			Respuesta.Vinculo.TipoColumna = ColumnaVinculo.Clase;
			foreach (CElementoVinculador Elemento in ListaElementos)
			{
				if (Elemento.Vinculo.Posicion.Length > 0)
				{
					CVinculoDetalleCN Vinculo = (from D in Respuesta.Detalles
																			 where D.ValorAsociado == Elemento.Vinculo.Posicion
																			 select D).FirstOrDefault();
					if (Vinculo == null)
					{
						Vinculo = new CVinculoDetalleCN()
						{
							Codigo = CodAgregado--,
							ValorAsociado = Elemento.Vinculo.ValorAsociado
						};
						Respuesta.Detalles.Add(Vinculo);
					}
					Vinculo.Posicion = Elemento.Vinculo.Posicion;
				}
			}
			return Respuesta;
		}

		private bool ModificoVinculo(CVinculoIndicadorCompletoCN Ahora)
		{
			if (Vinculador == null)
			{
				return true;
			}
			if (Vinculador.Vinculo.Codigo!=Ahora.Vinculo.Codigo ||
					Vinculador.Vinculo.CodigoVinculado != Ahora.Vinculo.CodigoVinculado)
			{
				return true;
			}

			if (Vinculador.Detalles.Count != Ahora.Detalles.Count)
			{
				return true;
			}

			foreach (CVinculoDetalleCN Detalle in Vinculador.Detalles)
			{
				CVinculoDetalleCN DetAhora = (from D in Ahora.Detalles
																			where D.ValorAsociado == Detalle.ValorAsociado
																			select D).FirstOrDefault();
				if (DetAhora==null || DetAhora.Posicion != Detalle.Posicion)
				{
					return true;
				}
			}
			return false;
		}

		public void AsociarPorCodigo()
		{
			if (ListaElementos != null)
			{
				foreach (CElementoVinculador Linea in ListaElementos)
				{
					CTextoTexto Asociado = (from L in Linea.Linea.Elementos
														 where L.Codigo == Linea.Vinculo.ValorAsociado
														 select L).FirstOrDefault();
					if (Asociado!=null && Asociado.Codigo.Length > 0)
					{
						Linea.Linea.ElementoAsociado = Asociado.Codigo;
					}
				}
			}
		}

		public void AsociarPorNombre()
		{
			if (ListaElementos != null)
			{
				foreach (CElementoVinculador Linea in ListaElementos)
				{
					CTextoTexto Asociado = (from L in Linea.Linea.Elementos
																	where L.Descripcion == Linea.Vinculo.ValorAsociado
																	select L).FirstOrDefault();
					if (Asociado != null && Asociado.Codigo.Length > 0)
					{
						Linea.Linea.ElementoAsociado = Asociado.Codigo;
					}
				}
			}
		}

		private bool mbRegistrando = false;

		public void Registrar()
		{
			CVinculoIndicadorCompletoCN VinculoLocal = CrearVinculador();
			Int32 Codigo = Vinculador.Vinculo.Codigo;
			if (ModificoVinculo(VinculoLocal))
			{
				HayBoton = false;
				HayMensaje = true;
				Mensaje = "Registrando";
				mbRegistrando = true;
				StateHasChanged();
			}
			else
			{
				if (AlResponder != null)
				{
					AlResponder(VinculoLocal);
				}
			}
		}

		private async Task RegistrarVinculoAsync()
		{
			mbRegistrando = false;
			CVinculoIndicadorCompletoCN VinculoLocal = CrearVinculador();
			Int32 Codigo = Vinculador.Vinculo.Codigo;
			Codigo = await CContenedorDatos.RegistrarVinculoAsync(Http, VinculoLocal);
			HayMensaje = false;
			StateHasChanged();
			if (Codigo > 0)
			{
				if (AlResponder != null)
				{
					AlResponder(VinculoLocal);
				}
			}
			else
			{
				Rutinas.CRutinas.DesplegarMsg(new Exception("No pudo registrar correctamente"));
			}
		}

		public bool AgregandoCapas { get; set; } = false;

		public async void CerrarEditarProveedoresWFS()
		{
			AgregandoCapas = false;
			await LeerCapasAsync();
			StateHasChanged();
		}

		public void AgregarCapa()
		{
			AgregandoCapas = true;
			StateHasChanged();
		}

		private Int32 mCodigoCapa { get; set; } = -1;
		public Int32 CapaSeleccionada
		{
			get { return mCodigoCapa; }
			set
			{
				if (value != mCodigoCapa && value > 0)
				{
					mCodigoCapa = value;
					_ = ImponerCapaAsync();
				}
			}
		}

		public void Cerrar()
		{
			if (AlResponder != null)
			{
				AlResponder(null);
			}
		}

		public List<CElementoVinculador> ListaElementos { get; set; } = null;

		public bool HayMensaje { get; set; } = true;
		public string Mensaje { get; set; } = "Procesando";
		public bool HayBoton { get; set; } = false;

		public void CerrarMsg()
		{
			HayBoton = false;
			HayMensaje = false;
			StateHasChanged();
		}

		public string EstiloBotones
		{
			get
			{
				return "width: 100%; height: 40px; text-align: right; position: relative; margin-top: 5px;";
			}
		}

		public CCapaWFSCN Capa { get; set; } = null;

		[Parameter]
		public CLogicaVinculadorCoordenadas.FncRespondeVinculo AlResponder { get; set; } = null;

		private async Task ImponerCapaAsync()
		{
			if (mCodigoCapa > 0)
			{
				Mensaje = "Leyendo detalles capa";
				HayMensaje = true;
				StateHasChanged();
				RespuestaCapaWFS Respuesta = await Contenedores.CContenedorDatos.LeerCapaWFSAsync(Http, mCodigoCapa, false);
				if (Respuesta.RespuestaOK)
				{
					ArmarListaElementosCapa(Respuesta.Capa);
					ArmarListaElementos();
					Capa = Respuesta.Capa;
			    HayMensaje = false;
				}
				else
				{
					HayBoton = true;
					Mensaje = Respuesta.MsgErr;
				}
			}
			else
			{
				HayMensaje = false;
				ArmarListaElementos();
				ListaElementosCapa.Clear();
			}
			StateHasChanged();
		}

		public bool SinElementos
		{
			get
			{
				return ListaElementos == null;
			}
		}

		private CVinculoDetalleCN ObtenerVinculo(string Elemento)
		{
			CVinculoDetalleCN Respuesta = (from D in Vinculador.Detalles
																		 where D.ValorAsociado == Elemento
																		 select D).FirstOrDefault();
			if (Respuesta == null)
			{
				Respuesta = new CVinculoDetalleCN()
				{
					Codigo = -1,
					Posicion = "",
					ValorAsociado = Elemento
				};
			}

			return Respuesta;

		}

		private void ArmarListaElementos()
		{
			ListaElementos = new List<CElementoVinculador>();
			foreach (string Valor in ColumnaVinculo.ListaValores)
			{
				ListaElementos.Add(new CElementoVinculador()
				{
					Vinculo = ObtenerVinculo(Valor)
				});
			}
		}

		public static string TruncarTexto(string Texto)
		{
			return (Texto.Length > 15 ? Texto.Substring(Texto.Length - 15) : Texto);
		}

		private void ArmarListaElementosCapa(CCapaWFSCN Capa)
		{
			ListaElementosCapa.Clear();
			ListaElementosCapa.Add(new CTextoTexto()
			{
				Codigo = "",
				Descripcion = "No corresponde"
			});
			foreach (CPuntoWFSCN Punto in Capa.Puntos)
			{
				ListaElementosCapa.Add(new CTextoTexto()
				{
					Codigo = TruncarTexto(Punto.Codigo),
					Descripcion = Punto.Nombre
				});
			}
			foreach (CLineaWFSCN Linea in Capa.Lineas)
			{
				ListaElementosCapa.Add(new CTextoTexto()
				{
					Codigo = TruncarTexto(Linea.Codigo),
					Descripcion = Linea.Nombre
				});
			}
			foreach (CAreaWFSCN Area in Capa.Areas)
			{
				ListaElementosCapa.Add(new CTextoTexto()
				{
					Codigo = TruncarTexto(Area.Codigo),
					Descripcion = Area.Nombre
				});
			}
		}

		private async Task LeerVinculoAsync()
		{
			HayMensaje = true;
			Mensaje = "Leyendo vínculos";
			StateHasChanged();
			Vinculador = await CContenedorDatos.LeerVinculoAsync(Http,
					ClaseIndicador, Indicador, ColumnaVinculo.Nombre);
			if (Vinculador != null)
			{
				mCodigoCapa = Vinculador.Vinculo.CodigoVinculado;
				await ImponerCapaAsync();
			}
			else
			{
				StateHasChanged();
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				_ = LeerCapasAsync();
				return;
			}
			if (mbRegistrando)
			{
				await RegistrarVinculoAsync();
			}
			await base.OnAfterRenderAsync(firstRender);
		}

	}
}
