using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaPaginaTarjeta: ComponentBase
  {

    protected override async Task OnAfterRenderAsync(bool firstRender)
		{
      await base.OnAfterRenderAsync(firstRender);
		}


		private Int32 mCodigo = -1;

		public CPreguntaCN Pregunta { get; set; } = null;
		public List<CPreguntaIndicadorConAlarmas> PreguntasIndicadores { get; set; } = null;
		public CDatoIndicador Indicador(CPreguntaIndicadorCN Pregunta)
		{
			return Contenedores.CContenedorDatos.IndicadorDesdeCodigo(Pregunta.Indicador);
		}

		public bool IndicadoresEnTarjeta
		{
			get
			{
				return (PreguntasIndicadores != null && PreguntasIndicadores.Count > 0);
			}
		}

		private List<CInformacionAlarmaCN> AlarmasParaIndicador(Int32 Indicador, Int32 Dimension)
		{
			return (from A in Contenedores.CContenedorDatos.gAlarmasIndicador
																							where A.CodigoIndicador == Indicador && A.ElementoDimension == Dimension
																							orderby A.Periodo
																							select A).ToList();
		}

		[Parameter]
		public Int32 Codigo
		{
			get { return mCodigo; }
			set
			{
				if (mCodigo != value)
				{
					mCodigo = value;
					if (mCodigo >= 0)
					{
						Pregunta = Contenedores.CContenedorDatos.UbicarPregunta(mCodigo);
						if (Pregunta == null)
						{
							Pregunta = new CPreguntaCN()
							{
								Codigo = mCodigo,
								Pregunta = "...."
							};
						}

						PreguntasIndicadores = new List<CPreguntaIndicadorConAlarmas>();
						foreach (CPreguntaIndicadorCN IndicadorLocal in Contenedores.CContenedorDatos.UbicarIndicadoresEnPregunta(mCodigo))
						{
							PreguntasIndicadores.Add(new CPreguntaIndicadorConAlarmas()
							{
								Indicador = IndicadorLocal,
								Alarmas = AlarmasParaIndicador(IndicadorLocal.Indicador, IndicadorLocal.ElementoDimension)
							});
						}
					}
					StateHasChanged();
				}
			}
		}

		public CDatoIndicador IndicadorComponente(Int32 Codigo)
		{
			return Contenedores.CContenedorDatos.IndicadorDesdeCodigo(Codigo);
		}

		[Inject]
		NavigationManager NavigationManager { get; set; }

		public void MoverseAIndicador(CPreguntaIndicadorCN Pregunta)
		{
			NavigationManager.NavigateTo("DetalleIndicador/" + Pregunta.Indicador.ToString(), false);
		}

		public string EstiloCompleto
		{
			get
			{
				return "width: 100%; height: " + AltoNecesario.ToString() + "px; display: block; poition: relative; overflow: hidden; padding: 0px;";
				//return "width: 100%; height: 100%; display: block; overflow: hidden; padding: 0px; background-color: blue;";
			}
		}

		Int32 mCantColumnas = -1;
		Int32 mSeparacion = -1;

		public const Int32 ANCHO_RELOJ_COMPLETO = 260;
		public const Int32 ALTO_RELOJ_COMPLETO = 200;

		public string EstiloReloj
		{
			get
			{
				return "width: " + ANCHO_ELEMENTO.ToString() + "px; height: " + ALTO_ELEMENTO.ToString() +
						"px; left: 0px; top: 0px; position: absolute; text-align: center; z-index: 1;";
			}
		}

		private Int32 AltoNecesario
		{
			get
			{
				if (mSeparacion < 0)
				{
					mCantColumnas = (Int32)Math.Floor((double)Contenedores.CContenedorDatos.AnchoPantalla /
								(double)(ANCHO_ELEMENTO + 20));
					mCantColumnas = Math.Max(mCantColumnas, 1);
					mSeparacion = (Int32)Math.Floor((double)(Contenedores.CContenedorDatos.AnchoPantalla -
								mCantColumnas * ANCHO_ELEMENTO) / (mCantColumnas + 1));
					mSeparacion = Math.Max(mSeparacion, 1);
				}
				if (PreguntasIndicadores == null)
				{
					return mSeparacion * 2 + (Int32)ALTO_ELEMENTO; // CLogicaRelojCompleto.ALTO_RELOJ_COMPLETO;
				}
				Int32 Filas = PreguntasIndicadores.Count;
				Filas = ((Filas % mCantColumnas) == 0 ? Filas : Filas + mCantColumnas - Filas % mCantColumnas);
				return mSeparacion + (mSeparacion + (Int32)ALTO_ELEMENTO + mSeparacion) * (Filas / mCantColumnas);
			}
		}

		private void DeterminarColumnas (double AnchoElemento)
		{
			mCantColumnas = (Int32)Math.Floor((double)Contenedores.CContenedorDatos.AnchoPantalla /
			(double)(AnchoElemento + 20));
			mCantColumnas = Math.Max(mCantColumnas, 1);
			mSeparacion = (Int32)Math.Floor((double)(Contenedores.CContenedorDatos.AnchoPantalla -
						mCantColumnas * AnchoElemento) / (mCantColumnas + 1));
			mSeparacion = Math.Max(mSeparacion, 1);
		}

		private static double ANCHO_ELEMENTO
		{
			get
			{
				return (double)(Contenedores.CContenedorDatos.SiempreTendencia ? (CLogicaTrendRed.ANCHO_TREND_RED + 20) :
						(double)CLogicaRelojCompleto.ANCHO_RELOJ_COMPLETO);
			}
		}

		private static double ALTO_ELEMENTO
		{
			get
			{
				return (double)(Contenedores.CContenedorDatos.SiempreTendencia ? (110 + CLogicaTrendRed.ALTO_TREND_RED) :
						(double)CLogicaRelojCompleto.ALTO_RELOJ_COMPLETO);
			}
		}

		public string EstiloIndicador(CPreguntaIndicadorCN Indicador)
		{
			Int32 Posicion = -1;
			foreach (CPreguntaIndicadorConAlarmas Indi in PreguntasIndicadores)
			{
				Posicion++;
				if (Indi.Indicador.Indicador == Indicador.Indicador && Indi.Indicador.ElementoDimension == Indicador.ElementoDimension)
				{
					break;
				}
			}

			if (mSeparacion < 0)
			{
				DeterminarColumnas(ANCHO_ELEMENTO);
			}

			Int32 Columna = Posicion % mCantColumnas;
			Int32 Fila = (Posicion - Columna) / mCantColumnas;

			return "width: " + ANCHO_ELEMENTO.ToString() + "px; height: " +	ALTO_ELEMENTO.ToString() +
				"px; position: absolute; left: " +
				(Columna * ANCHO_ELEMENTO + (Columna + 1) * mSeparacion).ToString() +
				"px; top: " + (Fila * ALTO_ELEMENTO + (Fila + 1) * mSeparacion).ToString() +
				"px;";
		}

	}

  public class UnionIndicadorTab
  {
    public static CLogicaTab.FncRefrescar FncRefrescar { get; set; }
    public CDatoIndicador Indicador { get; set; }

    public Int32 Abscisa { get; set; }
    public Int32 Ordenada { get; set; }

    private CLogicaRelojCompleto mTab = null;
    public CLogicaRelojCompleto TabLocal
    {
      get { return mTab; }
      set
      {
        if (value != mTab)
        {
          if (mTab != null)
          {
//            mTab.AlRefrescar -= FncRefrescar;
          }
          mTab = value;
        }
        if (mTab != null)
        {
//          mTab.AlRefrescar += FncRefrescar;
        }
      }
    }
  }

	public class CPreguntaIndicadorConAlarmas
	{
		private static Int32 gCodigoUnico = 0;
		public Int32 CodigoUnico { get; set; }

		private CDatoIndicador mDatosIndicador = null;

		public CDatoIndicador DatosIndicador { get { return mDatosIndicador; } }

		private CPreguntaIndicadorCN mIndicador = null;
		public CPreguntaIndicadorCN Indicador
		{
			get { return mIndicador; }
			set
			{
				mIndicador = value;
				if (mIndicador != null)
				{
					mDatosIndicador = Contenedores.CContenedorDatos.IndicadorDesdeCodigo(Indicador.Indicador);
				}
				else
				{
					mDatosIndicador = null;
				}
			}
		}

		public List<CInformacionAlarmaCN> Alarmas { get; set; }

		[Parameter]
		public CLogicaReloj Reloj { get; set; }
		public string NombreIndicador
		{
			get
			{
				return (mDatosIndicador == null ? "--" : mDatosIndicador.Descripcion);
			}
		}

		public string Unidades
		{
			get { return (mDatosIndicador == null ? "" : mDatosIndicador.Unidades); }
		}

		public string ValorIndicador
		{
			get
			{
				return (Alarmas == null || Alarmas.Count == 0 || mDatosIndicador == null ?
							"--" : Alarmas.Last().Valor.ToString(Rutinas.CRutinas.FormatoDecimales(mDatosIndicador.Decimales)));
			}
		}

		public CPreguntaIndicadorConAlarmas()
		{
			CodigoUnico = gCodigoUnico++;
		}
	}

}
