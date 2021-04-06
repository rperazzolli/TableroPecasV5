using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using Blazorise;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Client.Contenedores;
using TableroPecasV5.Client.Listas;
using TableroPecasV5.Client.Plantillas;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaAgregarElem : ComponentBase
	{

		public Modal ModalGrafico { get; set; }
    public Modal ModalIndicador { get; set; }
    public Modal ModalTarjeta { get; set; }
    public Modal ModalMapa { get; set; }
    public Modal ModalVarios { get; set; }

    private CLogicaPedirSubconsultas mModalPrms = null;
    public CLogicaPedirSubconsultas ModalPrms
    {
      get { return mModalPrms; }
      set
      {
        if (mModalPrms != value)
        {
          mModalPrms = value;
          mModalPrms.AlSeleccionar = FncRetornarPrms;
        }
      }
    }

    private void FncRetornarPrms(string Valor)
		{
      PidiendoPrms = false;
      if (Valor != null)
			{
        mszParametros = Valor;
        AgregarVarios();
			}
      else
			{
        StateHasChanged();
			}
		}

    public bool PidiendoPrms { get; set; }

    public List<Listas.CListaTexto> ListaVarios { get; set; }

    private bool mMimicos = false;
    private bool mSubconsultas = false;

    public void PonerMimicos()
		{
      mMimicos = true;
      mSubconsultas = false;
      mszParametros = "";
      ListaVarios = (from L in CContenedorDatos.ListaMimicos
                     orderby L.Nombre
                     select new Listas.CListaTexto(L.Codigo, L.Nombre)).ToList();
      StateHasChanged();
		}

    public async void PonerSubconsultas()
    {
      mMimicos = false;
      mSubconsultas = true;
      mszParametros = "";
      await CContenedorDatos.CargarListaSubconsultasAsync(Http);
      ListaVarios = (from L in CContenedorDatos.gSubconsultas
                     orderby L.Nombre
                     select new Listas.CListaTexto(L.Codigo, L.Nombre)).ToList();
      StateHasChanged();
    }

    private CContenedorBlocks mContenedor = new CContenedorBlocks();

    private List<CElementoXML> mElementos = new List<CElementoXML>();

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

    public bool Cargando { get; set; } = false;

    public Int32 CodigoMapaAgregar { get; set; } = -1;
    public Int32 CodigoVariosAgregar { get; set; } = -1;
    public string NombreGraficoAgregar { get; set; } = null;
    public Int32 IndicadorAgregar { get; set; } = -1;
    public static string SinDatos { get { return "...."; } }

    public bool NoHayGrafico
    {
      get { return NombreGraficoAgregar == null || NombreGraficoAgregar == SinDatos || Posicion.Length == 0; }
    }

    public bool NoHayMapa
    {
      get { return CodigoMapaAgregar == -1 || Posicion.Length == 0; }
    }

    public bool NoHayVarios
    {
      get { return CodigoVariosAgregar == -1 || Posicion.Length == 0; }
    }

    public bool NoHayIndicador
    {
      get { return IndicadorAgregar < 0 || Posicion.Length == 0; }
    }

    public bool NoHayTarjeta
    {
      get
      {
        return TarjetaAgregar < 0 || Posicion.Length == 0 ||
        (DimensionAgregar > 0 && ElementoDimensionAgregar < 0);
      }
    }

    private void AjustarPosicionElemento(CGrafV2DatosContenedorBlock Elemento)
    {
      try
      {
        string[] Datos = Posicion.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        if (Datos.Length != 4)
        {
          throw new Exception("Posición incorrecta");
        }
        Elemento.Posicion = new Point(double.Parse(Datos[0]), double.Parse(Datos[1]));
        Elemento.Ancho = double.Parse(Datos[2]);
        Elemento.Alto = double.Parse(Datos[3]);
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    public void AgregarGrafico()
		{
      //
      CerrarModales();
      CElementoXML Elemento = (from E in CContenedorDatos.gElementosXML
                               where E.Clase == CClaseElementoXML.Grafico && E.Nombre == NombreGraficoAgregar
                               select E).FirstOrDefault();
      if (Elemento != null)
      {
        AjustarPosicionElemento(Elemento.GrafV2);
        if (!mElementos.Contains(Elemento))
        {
          mElementos.Add(Elemento);
        }
        AjustarRectangulos();
      }
    }

    public void AgregarMapa()
    {
      //
      CerrarModales();
      CDatosMapaLista Mapa = (from E in ListaMapas
                               where E.Codigo == CodigoMapaAgregar
                               select E).FirstOrDefault();
      if (Mapa != null)
      {
        CElementoXML Elemento = new CElementoXML()
        {
          Clase = CClaseElementoXML.Mapa,
          Nombre = NombreGraficoAgregar,
          GrafV2 = Mapa.CrearGrafV2()
        };
        AjustarPosicionElemento(Elemento.GrafV2);
        if (!mElementos.Contains(Elemento))
        {
          mElementos.Add(Elemento);
        }
        AjustarRectangulos();
      }
    }

    private bool ExigirPrms
		{
      get
			{
        CSubconsultaExt SubC = (from S in CContenedorDatos.gSubconsultas
                                where S.Codigo == CodigoVariosAgregar
                                select S).FirstOrDefault();
        return (SubC != null && SubC.Parametros.Count > 0);
			}
		}

    public Int32 SubconsultaEnEdicion { get; set; } = -1;

    public void AgregarVarios()
    {
      if (mSubconsultas)
      {
        if (ExigirPrms)
        {
          if (mszParametros == null)
          {
            PidiendoPrms = true;
            SubconsultaEnEdicion = CodigoVariosAgregar;
            StateHasChanged();
            return;
          }
        }
        else
				{
          mszParametros = "[0]$$";
				}
      }
      //
      CerrarModales();
      CDatosOtroLista Otro = new CDatosOtroLista()
      {
        Clase = (mMimicos ? CGrafV2DatosContenedorBlock.ClaseBlock.Mimico :
            CGrafV2DatosContenedorBlock.ClaseBlock.Subconsulta),
        ClaseIndicador = (mMimicos ? ClaseElemento.Mimico : ClaseElemento.SubConsulta),
        Codigo = CodigoVariosAgregar,
        CodigoElementoDimension = -1,
        Nombre = (from L in ListaVarios
                  where L.Codigo == CodigoVariosAgregar
                  select L.Descripcion).FirstOrDefault(),
        CodigoPropio = CodigoVariosAgregar,
        Parametros=mszParametros 
      };

      if (Otro != null)
      {
        CElementoXML Elemento = new CElementoXML()
        {
          Clase = CClaseElementoXML.Varios,
          Nombre = Otro.Nombre,
          GrafV2 = Otro.CrearGrafV2()
        };
        AjustarPosicionElemento(Elemento.GrafV2);
        if (!mElementos.Contains(Elemento))
        {
          mElementos.Add(Elemento);
        }
        AjustarRectangulos();
      }
    }

    private CGrafV2DatosContenedorBlock CrearGrafV2Indicador()
		{
      return new CGrafV2DatosContenedorBlock()
      {
        Indicador = IndicadorAgregar,
        Agrupacion = ModoAgruparDependiente.NoDefinido,
        AgrupIndep = ModoAgruparIndependiente.NoDefinido,
        Alto = 0,
        Ancho = 0,
        Clase = ClaseGrafico.NoDefinido,
        ClaseElemento = CGrafV2DatosContenedorBlock.ClaseBlock.Indicador,
        ClaseOrigen = ClaseElemento.Indicador,
        CodigoElementoDimension = -1,
        ColumnaAbscisas = "",
        ColumnaOrdenadas = "",
        ColumnaSexo = "",
        Filtro = new CPasoCondicionesBlock(),
        FiltrosBlock = new List<CPasoCondicionesBlock>(),
        Nombre = NombreIndicador,
        Posicion = new Point(0, 0),
        SaltoHistograma = 0,
        UsaFiltroPropio = true,
        Visible = true
      };
		}

    private CGrafV2DatosContenedorBlock CrearGrafV2Tarjeta()
    {
      return new CGrafV2DatosContenedorBlock()
      {
        Indicador = TarjetaAgregar,
        Agrupacion = ModoAgruparDependiente.NoDefinido,
        AgrupIndep = ModoAgruparIndependiente.NoDefinido,
        Alto = 0,
        Ancho = 0,
        Clase = ClaseGrafico.NoDefinido,
        ClaseElemento = CGrafV2DatosContenedorBlock.ClaseBlock.Ficha,
        ClaseOrigen = ClaseElemento.Indicador,
        CodigoDimension = DimensionAgregar,
        CodigoElementoDimension = ElementoDimensionAgregar,
        ColumnaAbscisas = "",
        ColumnaOrdenadas = "",
        ColumnaSexo = "",
        Filtro = new CPasoCondicionesBlock(),
        FiltrosBlock = new List<CPasoCondicionesBlock>(),
        Nombre = NombreTarjeta,
        Posicion = new Point(0, 0),
        SaltoHistograma = 0,
        UsaFiltroPropio = true,
        Visible = true
      };
    }

    private string mszParametros = "";

    public string IndicadorTendencia { get; set; }
    private string mszFiltroIndicador = "";
    public string FiltroIndicador
		{
      get { return mszFiltroIndicador; }
      set
			{
        if (mszFiltroIndicador != value)
				{
          mszFiltroIndicador = value;
          StateHasChanged();
				}
			}
		}

    public List<CDatoIndicador> ListaIndicadores
		{
      get
			{
        return (from I in CContenedorDatos.ListaIndicadores
                where mszFiltroIndicador.Length == 0 ||
                    I.Descripcion.IndexOf(mszFiltroIndicador, StringComparison.InvariantCultureIgnoreCase) >= 0
                select I).ToList();
			}
		}

    private string mszFiltroTarjetas = "";
    public string FiltroTarjetas
    {
      get { return mszFiltroTarjetas; }
      set
      {
        if (mszFiltroTarjetas != value)
        {
          mszFiltroTarjetas = value;
          StateHasChanged();
        }
      }
    }

    public Int32 TarjetaAgregar { get; set; }

    private Int32 mDimensionAgregar = -1;
    public Int32 DimensionAgregar
    {
      get { return mDimensionAgregar; }
      set
      {
        if (mDimensionAgregar != value)
        {
          mDimensionAgregar = value;
          _ = CargarElementosDimensionAsync(Http);
        }
      }
    }

    public List<CListaElementosDimension> ElementosDimension = null;

    public async Task CargarElementosDimensionAsync(HttpClient Cliente)
    {
      if (mDimensionAgregar < 0)
      {
        ElementosDimension = null;
      }
      else
      {
        Int32 DimensionLeida = mDimensionAgregar;
        try
        {
          RespuestaEntidades Respuesta = await Cliente.GetFromJsonAsync<RespuestaEntidades>(
              "api/Indicadores/GetSujetosEntidad?URL=" + CContenedorDatos.UrlBPI +
              "&Ticket=" + CContenedorDatos.Ticket +
              "&Entidad=" + mDimensionAgregar.ToString());
          if (!Respuesta.RespuestaOK)
          {
            throw new Exception(Respuesta.MsgErr);
          }
          else
          {
            if (mDimensionAgregar == DimensionLeida && ModalTarjeta != null && ModalTarjeta.Visible)
            {
              ElementosDimension = (from E in Respuesta.Entidades
                                    select new CListaElementosDimension()
                                    {
                                      CodigoDimension = E.Codigo,
                                      Descripcion = E.Descripcion,
                                      Elementos = new List<CEntidadCN>()
                                    }).ToList();
              StateHasChanged();
            }
          }
        }
        catch (Exception ex)
        {
          CRutinas.DesplegarMsg(ex);
        }
      }
    }

    public Int32 ElementoDimensionAgregar { get; set; }

    public List<CListaTexto> ListaTarjetas
    {
      get
      {
        List<CListaTexto> Lista = new List<CListaTexto>();
        foreach (CPuntoSala Sala in CContenedorDatos.EstructuraIndicadores.Salas)
				{
          foreach (CPuntoSolapa Solapa in Sala.Solapas)
					{
            foreach (CPuntoPregunta Pregunta in Solapa.Preguntas)
						{
              if (mszFiltroTarjetas.Length==0 ||
                  Pregunta.Pregunta.Pregunta.IndexOf(mszFiltroTarjetas, StringComparison.CurrentCultureIgnoreCase) >= 0)
							{
                Lista.Add(new CListaTexto(Pregunta.Pregunta.Codigo,
                  Pregunta.Pregunta.Pregunta + " <" + Sala.Sala.Nombre + ">"));
							}
						}
					}
				}
        return Lista;
      }
    }

    [Inject]
    public HttpClient Http { get; set; }

    public List<CListaElementosDimension> Dimensiones = null;

    private async Task CargarDimensionesAsync()
		{
      Dimensiones = await CContenedorDatos.ObtenerListaDimensionesAsync(Http);
      if (ModalTarjeta!=null && ModalTarjeta.Visible)
			{
        StateHasChanged();
			}
		}

    protected override Task OnInitializedAsync()
		{
      _ = CargarDimensionesAsync();
			return base.OnInitializedAsync();
		}

    private string NombreIndicador
    {
      get
      {
        CDatoIndicador Elemento = (from L in ListaIndicadores
                                   where L.Codigo == IndicadorAgregar
                                   select L).FirstOrDefault();
        return (mbIndicador ? "Indicador " : "Tendencia ") + (Elemento == null ? "--" : Elemento.Descripcion);
      }
    }

    private CElementoXML CrearElementoIndicador()
		{
      return new CElementoXML()
      {
        Clase = (mbIndicador? CClaseElementoXML.Indicador:CClaseElementoXML.Tendencia),
        Nombre = NombreIndicador,
        GrafV2 = CrearGrafV2Indicador()
      };
		}

    private string NombreTarjeta
		{
      get
			{
        CListaTexto Elemento = (from L in ListaTarjetas
                  where L.Codigo == TarjetaAgregar
                  select L).FirstOrDefault();
        return "Tarjeta " + (Elemento == null ? "--" : Elemento.Descripcion);
			}
		}

    private CElementoXML CrearElementoTarjeta()
    {
      return new CElementoXML()
      {
        Clase = CClaseElementoXML.Tarjeta,
        Nombre = NombreTarjeta,
        GrafV2 = CrearGrafV2Tarjeta()
      };
    }

    public List<CRect> Rectangulos { get; set; } = new List<CRect>();

    private void AjustarRectangulos()
		{
      Rectangulos = (from E in mElementos
                     select E.Rectangulo).ToList();
      StateHasChanged();
    }

    [CascadingParameter]
    public CLogicaSalaReunion SalaReunion { get; set; }

    [Parameter]
    public string XML { get; set; }

    private void AgregarElementoAXML(XElement Inicio, CGrafV2DatosContenedorBlock Elemento)
		{
      Inicio.Add(Elemento.CrearElemento());
		}

    private string ObtenerXML()
		{
      // Cargar el XML.
      XDocument Documento = XDocument.Parse(XML);

      XElement Inicio = (XElement)Documento.FirstNode;

      // Agregar los elementos.
      foreach (CElementoXML Elemento in mElementos)
			{
        AgregarElementoAXML(Inicio, Elemento.GrafV2);
			}

      System.Text.StringBuilder Escritor = new System.Text.StringBuilder();
      Escritor.Append(Documento);
      return Escritor.ToString();
    }

    public void Registrar()
		{
      SalaReunion.CerrarEditarXML(ObtenerXML());
		}

    public void AgregarIndicador()
    {
      //
      CerrarModales();
      CElementoXML Elemento = CrearElementoIndicador();
      if (Elemento != null)
      {
        AjustarPosicionElemento(Elemento.GrafV2);
        if (mbIndicador)
        {
          Elemento.GrafV2.Ancho = (CContenedorDatos.SiempreTendencia ? 350 : 110);
          Elemento.GrafV2.Alto = (CContenedorDatos.SiempreTendencia ? 180 : 60);
        }
        if (!mElementos.Contains(Elemento))
        {
          mElementos.Add(Elemento);
        }
        AjustarRectangulos();
      }
    }

    public void AgregarTarjeta()
    {
      //
      CerrarModales();
      CElementoXML Elemento = CrearElementoTarjeta();
      if (Elemento != null)
      {
        AjustarPosicionElemento(Elemento.GrafV2);
        Elemento.GrafV2.Ancho = CLogicaTarjeta.ANCHO_TARJETA;
        Elemento.GrafV2.Alto = CLogicaTarjeta.ALTO_TARJETA;
        if (!mElementos.Contains(Elemento))
        {
          mElementos.Add(Elemento);
        }
        AjustarRectangulos();
      }
    }

    public List<CElementoXML> ListaGraficos
		{
      get
			{
        return (from G in CContenedorDatos.gElementosXML
                where G.Clase == CClaseElementoXML.Grafico
                orderby G.Nombre
                select G).ToList();
			}
		}

    private void CerrarModal(Modal Dialogo)
		{
      if (Dialogo!=null && Dialogo.Visible)
			{
        Dialogo.Hide();
			}
		}

    private void CerrarModales()
    {
      CerrarModal(ModalGrafico);
      CerrarModal(ModalIndicador);
      CerrarModal(ModalTarjeta);
      CerrarModal(ModalMapa);
      CerrarModal(ModalVarios);

      mModalEnEdicion = null;
    }

    public void AgregarElementos(Int32 Opcion)
		{
      switch (Opcion)
			{
        case 3:
          AgregarModalGraficos();
          break;
			}
		}

    public bool Posicionando { get; set; }

    public string Posicion { get; set; } = "";

    private void ImponerPosicion(double Abscisa, double Ordenada, double Ancho, double Alto)
		{
      Posicion = Abscisa.ToString() + "; " + Ordenada.ToString() + "; " + Ancho.ToString() + "; " + Alto.ToString();
      Posicionando = false;
      StateHasChanged();
      mModalEnEdicion = mModalAReponer;
      mModalAReponer.Show();
		}

    private CLogicaDefinirRectangulos mDefinidor = null;
    public CLogicaDefinirRectangulos DefinidorPosiciones
    {
      get { return mDefinidor; }
      set
      {
        if (mDefinidor != value)
        {
          mDefinidor = value;
          mDefinidor.AlRetornar = ImponerPosicion;
        }
      }
    }

    public void ObtenerPosicion()
		{
      if (mModalEnEdicion != null)
			{
        mModalAReponer = mModalEnEdicion;
        CerrarModales();
        Posicionando = true;
        StateHasChanged();
			}
		}

    private Modal mModalEnEdicion = null;
    private Modal mModalAReponer = null;

    public void AgregarModalGraficos()
		{
      Posicion = "";
      CerrarModales();
      mModalEnEdicion = ModalGrafico;
      NombreGraficoAgregar = "";
      Posicion = "";
      ModalGrafico.Show();
		}

    public void AgregarModalVarios()
    {
      Posicion = "";
      CerrarModales();
      mModalEnEdicion = ModalVarios;
      CodigoVariosAgregar = -1;
      Posicion = "";
      mMimicos = true;
      mSubconsultas = false;
      mszParametros = "";
      ModalVarios.Show();
    }


    public List<CDatosMapaLista> ListaMapas { get; set; } = null;

    private async Task LlenarListaMapasAsync()
    {
      try
      {
        RespuestaCapasWSS Respuesta = await Http.GetFromJsonAsync<RespuestaCapasWSS>(
            "api/Proyectos/ListarCapasWSS?URL=" + CContenedorDatos.UrlBPI +
            "&Ticket=" + CContenedorDatos.Ticket +
            "&Clase=-1&Codigo=-1");
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MsgErr);
        }
        else
        {
          ListaMapas = (from C in Respuesta.Capas
                        select new CDatosMapaLista(C, null)).ToList();
        }

        RespuestaGraficosVarios RespGr = await Http.GetFromJsonAsync<RespuestaGraficosVarios>(
            "api/Proyectos/ListarGraficosIndicadores?URL=" + CContenedorDatos.UrlBPI +
            "&Ticket=" + CContenedorDatos.Ticket +
            "&Indicadores="+CRutinas.EnterosALista((from I in CContenedorDatos.ListaIndicadores
                                                    select I.Codigo).Distinct().ToList()));
        if (!RespGr.RespuestaOK)
        {
          throw new Exception(RespGr.MsgErr);
        }
        else
        {
          foreach (CGraficoCompletoCN G in RespGr.Graficos)
          {
            if (G.Graficos.Count == 1 && G.Graficos[0].ClaseDeGrafico == ClaseGrafico.SobreGIS)
            {
              ListaMapas.Add(new CDatosMapaLista(G));
            }
          }
        }

        foreach (CMapaBingCN Mapa in CContenedorDatos.ListaMapas)
				{
          ListaMapas.Add(new CDatosMapaLista(Mapa));
				}

        ListaMapas = (from M in ListaMapas
                      orderby M.Nombre
                      select M).ToList();

        Cargando = false;

        StateHasChanged();

      }
      catch (Exception ex)
      {
        ListaMapas = null;
        CRutinas.DesplegarMsg(ex);
      }
    }

    public void AgregarModalMapas()
    {
      Posicion = "";
      CerrarModales();
      mModalEnEdicion = ModalMapa;
      CodigoMapaAgregar = -1;
      Posicion = "";
      if (ListaMapas == null)
      {
        Cargando = true;
        _ = LlenarListaMapasAsync();
      }
      ModalMapa.Show();
    }

    public void AgregarModalTarjeta()
    {
      Posicion = "";
      CerrarModales();
      mModalEnEdicion = ModalTarjeta;
      TarjetaAgregar = -1;
      DimensionAgregar = -1;
      ElementoDimensionAgregar = -1;
      Posicion = "";
      ModalTarjeta.Show();
    }
    private bool mbIndicador = false;

    public void AgregarModalIndicador(bool bIndicador)
    {
      Posicion = "";
      CerrarModales();
      mbIndicador = bIndicador;
      IndicadorTendencia = (bIndicador ? "Indicador" : "Tendencia");
      mszFiltroIndicador = "";
      mModalEnEdicion = ModalIndicador;
      IndicadorAgregar = -1;
      Posicion = "";
      ModalIndicador.Show();
    }

    public void CerrarVentanaGrafico()
		{
      ModalGrafico.Hide();
		}

    public void CerrarVentanaVarios()
    {
      ModalVarios.Hide();
    }

    public void CerrarVentanaIndicador()
    {
      ModalIndicador.Hide();
    }

    public void CerrarVentanaTarjeta()
    {
      ModalTarjeta.Hide();
    }

    public void CerrarVentanaMapa()
    {
      ModalMapa.Hide();
    }

  }
}
