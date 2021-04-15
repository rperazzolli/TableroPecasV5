using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Blazor.Extensions.Canvas;
using System.Net.Http;
using System.Net.Http.Json;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.Model;
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Client.Plantillas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Rutinas
{
  public class CRutinas
  {

    public enum ClaseBlock
    {
      Grafico = 1,
      Conjunto = 2,
      Comparativo = 3,
      MapaCalor = 4,
      MapaGradientes = 5,
      MapaControl = 6,
      Mimico = 7,
      Grilla = 8,
      Indicador = 9,
      Tendencia = 10,
      Ficha = 11,
      NoDefinida = -1
    }

    public const Int32 ELEMENTO_TODOS = -1000;

    public const string CTE_OTROS = "Otros";

    public const string COLOR_BLANCO = "BLANCO";
    public const string COLOR_GRIS = "GRIS";
    public const string COLOR_AZUL = "AZUL";
    public const string COLOR_ROJO = "ROJO";
    public const string COLOR_VERDE = "VERDE";
    public const string COLOR_AMARILLO = "AMARILLO";

    public const string FREC_MINUTOS = "Minutos";
    public const string FREC_HORARIA = "Horaria";
    public const string FREC_DIARIA = "Diaria";
    public const string FREC_SEMANAL = "Semanal";
    public const string FREC_MENSUAL = "Mensual";
    public const string FREC_MENSUAL_AC = "Mensual-Acumulado";
    public const string FREC_BIMESTRAL = "Bimestral";
    public const string FREC_TRIMESTRAL = "Trimestral";
    public const string FREC_CUATRIMESTRAL = "Cuatrimestral";
    public const string FREC_SEMESTRAL = "Semestral"; // Unicamente se usa en indicadores manuales.
    public const string FREC_ANUAL = "Anual";

    public const bool TendenciasEnTarjeta = false;

    public const double ANCHO_COLUMNA_H = 25;
    public const double ANCHO_SUB_COLUMNA_H = 10;
    public const Int32 ALTO_BARRA_SCROLL = 17;

    public delegate void FncMsgAlerta(string Texto1, string Texto2);
    public delegate void FncRefrescar();
    public static string gMsgUsuario { get; set; } = "";

    public static Int32 AnchoPantalla = 800;
    public static Int32 AltoPantalla = 600;

    public static FncMsgAlerta gFncInformarUsuario { get; set; } = null;

    public enum SaltoEscalaFechas
    {
      NoCorresponde = 0,
      Horas = 1,
      Dias = 2,
      Meses = 3,
      Anios = 4
    }

    public async static Task LiberarMapaAsync(Microsoft.JSInterop.IJSRuntime JSRuntime, Int32 Posicion)
    {
      object[] Args = new object[1];
      Args[0] = Posicion;
      try
      {
        string Retorno = await JSRuntime.InvokeAsync<string>("LiberarMap", Args);
        if (Retorno.Length > 0)
        {
          throw new Exception(Retorno);
        }
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }

    }

    public async static Task<Logicas.Rectangulo> ObtenerExtremosMapaAsync(Microsoft.JSInterop.IJSRuntime JSRuntime, Int32 Posicion)
    {
      object[] Args = new object[1];
      Args[0] = Posicion;
      try
      {
        string Retorno = await JSRuntime.InvokeAsync<string>("ExtremosMapa", Args);
        Logicas.Rectangulo Rect = new Logicas.Rectangulo(Retorno);
        Rect.width = Math.Abs(Rect.width - Rect.left);
        Rect.height = Math.Abs(Rect.height - Rect.top);
        return Rect;
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
        return new Logicas.Rectangulo("0;0;1;1");
      }

    }

    public async static Task LimpiarContenidoMapaAsync(Microsoft.JSInterop.IJSRuntime JSRuntime, Int32 Posicion)
    {
      object[] Args = new object[1];
      Args[0] = Posicion;
      try
      {
        string Retorno = await JSRuntime.InvokeAsync<string>("LiberarPushpins", Args);
        if (Retorno.Length > 0)
        {
          throw new Exception(Retorno);
        }
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }

    }

    public static async Task<CPosicionWFSCN> ObtenerDimensionesPantallaAsync(
        Microsoft.JSInterop.IJSRuntime JSRuntime, string Direccion)
    {
      object[] Args = new object[1];
      Args[0] = Direccion;
      string Posicion = await JSRuntime.InvokeAsync<string>("FuncionesJS.getRectangulo", Args);
      List<double> Valores = ListaAReales(Posicion);
      return new CPosicionWFSCN()
      {
        X = Valores[2],
        Y = Valores[3]
      };
    }

    public static void ExtraerCoordenadasPosicion(string Posicion, out double Lng, out double Lat)
    {
      try
      {
        string[] Coord = Posicion.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        Lng = StrVFloat(Coord[0]);
        Lat = StrVFloat(Coord[1]);
      }
      catch (Exception)
      {
        Lng = -1000;
        Lat = -1000;
      }
    }

    public static string ColorAlarma(CInformacionAlarmaCN Datos)
		{
      if (Datos == null)
			{
        return COLOR_GRIS;
			}
      else
			{
        if (Datos.Minimo == Datos.Sobresaliente)
				{
          return COLOR_GRIS;
				}
        else
				{
          if (Datos.Sobresaliente > Datos.Minimo)
          {
            if (Datos.Valor < Datos.Minimo)
            {
              return COLOR_ROJO;
            }
            else
            {
              if (Datos.Valor < Datos.Satisfactorio)
              {
                return COLOR_AMARILLO;
              }
              else
              {
                return (Datos.Valor < Datos.Sobresaliente ? COLOR_VERDE : COLOR_AZUL);
              }
            }
          }
          else
          {
            if (Datos.Valor > Datos.Minimo)
            {
              return COLOR_ROJO;
            }
            else
            {
              if (Datos.Valor > Datos.Satisfactorio)
              {
                return COLOR_AMARILLO;
              }
              else
              {
                return (Datos.Valor > Datos.Sobresaliente ? COLOR_VERDE : COLOR_AZUL);
              }
            }
          }
        }
			}
		}

    public static string FechaATexto(DateTime Fecha)
    {
      return Fecha.ToString("yyyyMMddHHmmss");
    }

    public static bool ContenidoImagenEsProceso(byte[] Bytes)
		{
      if (Bytes==null || Bytes.Length < 25)
			{
        return false;
			}

      string Texto = System.Text.Encoding.UTF8.GetString(Bytes, 0, 25).Trim().ToLower();
      if (!Texto.Substring(0,2).Contains("<"))
			{
        return false;
			}

      while (!Texto.StartsWith("<"))
			{
        Texto = Texto.Substring(1);
			}

      if (Texto.StartsWith("<?xml"))
			{
        return true;
			}

      return Texto.Contains("package");

		}

    public static string ColorMasCritico(string Color1, string Color2)
    {
      switch (Color1)
      {
        case "":
          return Color2;
        case COLOR_GRIS:
          switch (Color2)
          {
            case "":
              return Color1;
            default:
              return Color2;
          }
        case COLOR_ROJO:
          return Color1;
        case COLOR_AMARILLO:
          switch (Color2)
          {
            case COLOR_ROJO:
              return Color2;
            default:
              return Color1;
          }
        case COLOR_VERDE:
          switch (Color2)
          {
            case COLOR_ROJO:
            case COLOR_AMARILLO:
              return Color2;
            default:
              return Color1;
          }
        case COLOR_AZUL:
          switch (Color2)
          {
            case COLOR_ROJO:
            case COLOR_AMARILLO:
            case COLOR_VERDE:
              return Color2;
            default:
              return Color1;
          }
        default:
          return Color2;
      }
    }

    public static ColorBandera ColorBanderaMasCritico(ColorBandera C1, ColorBandera C2)
    {
      if (C2 == ColorBandera.NoCorresponde || C2 == ColorBandera.SinDatos)
      {
        return (C1 == ColorBandera.NoCorresponde ? C2 : C1);
      }

      switch (C1)
      {
        case ColorBandera.NoCorresponde:
        case ColorBandera.SinDatos:
          return C2;
        case ColorBandera.Azul:
          return C2;
        case ColorBandera.Verde:
          return (C2 == ColorBandera.Azul ? C1 : C2);
        case ColorBandera.Amarillo:
          return (C2 == ColorBandera.Azul ||
                C2 == ColorBandera.Verde ? C1 : C2);
        case ColorBandera.Rojo:
          return C1;
        default:
          return C2;
      }
    }

    public static string ColorEquivalente(string Color)
    {
      switch (Color)
      {
        case COLOR_AZUL: return "blue";
        case COLOR_VERDE: return "green";
        case COLOR_AMARILLO: return "yellow";
        case COLOR_ROJO: return "red";
        case COLOR_GRIS: return "gray";
        default: return "white";
      }
    }

    public static string ColorAclarado(string Color)
    {
      switch (Color)
      {
        case COLOR_AZUL: return "lightblue";
        case COLOR_VERDE: return "lightgreen";
        case COLOR_AMARILLO: return "yellow";
        case COLOR_ROJO: return "red";
        case COLOR_GRIS: return "lightgray";
        default: return "white";
      }
    }

    public static string ColorAclarado(ColorBandera Color)
    {
      switch (Color)
      {
        case ColorBandera.Azul: return "lightblue";
        case ColorBandera.Verde: return "lightgreen";
        case ColorBandera.Amarillo: return "yellow";
        case ColorBandera.Rojo: return "red";
        default: return "lightgray";
      }
    }

    public static bool EstaComprendido(CDetallePreguntaIcono Detalle,
          List<CPreguntaPreguntaWISCN> Preguntas)
    {
      if (Detalle == null)
      {
        return true; // si no hay un elemento seleccionado, usa el menu completo.
      }

      foreach (CPreguntaPreguntaWISCN Dato in Preguntas)
      {
        if (Dato.Codigo == Detalle.Detalle.Codigo &&
          Dato.Clase == Detalle.Detalle.Clase)
        {
          return true;
        }
        else
        {
          if (Dato.Clase == ClaseDetalle.Pregunta &&
            Detalle.Detalle.Clase == ClaseDetalle.Indicador &&
            Contenedores.CContenedorDatos.IndicadorEnPregunta(Detalle.Detalle.Codigo, Dato.CodigoPregunta,
                Detalle.Detalle.CodigoElementoDimension))
          {
            return true;
          }
        }
      }
      return false;
    }

    public async static Task<List<CEntradaIndicador>> IndicadoresEnContenidosAsync(HttpClient Http,
          List<CPreguntaPreguntaWISCN> Contenidos)
    {
      List<CEntradaIndicador> Respuesta = new List<CEntradaIndicador>();
      if (Contenidos != null)
      {
        foreach (CPreguntaPreguntaWISCN Entrada in Contenidos)
        {
          foreach (CEntradaIndicador Dato in await IndicadoresEnContenidoAsync(Http, Entrada))
          {
            if (!IndicadorEnLista(Respuesta, Dato.CodigoIndicador, Dato.CodigoElementoDimension))
            {
              Respuesta.Add(Dato);
            }
          }
        }
      }
      return Respuesta;
    }

    public async static Task<List<CEntradaIndicador>> IndicadoresEnContenidosAsync(HttpClient Http,
          List<CDetallePreguntaCN> Contenidos)
    {
      List<CEntradaIndicador> Respuesta = new List<CEntradaIndicador>();
      if (Contenidos != null)
      {
        foreach (CDetallePreguntaCN Entrada in Contenidos)
        {
          foreach (CEntradaIndicador Dato in await IndicadoresEnContenidoAsync(Http, Entrada))
          {
            if (!IndicadorEnLista(Respuesta, Dato.CodigoIndicador, Dato.CodigoElementoDimension))
            {
              Respuesta.Add(Dato);
            }
          }
        }
      }
      return Respuesta;
    }

    private static void AcoplarListaIndicadores(List<CEntradaIndicador> Respuesta, List<CEntradaIndicador> Lista)
    {
      foreach (CEntradaIndicador DatoIndi in Lista)
      {
        if (!IndicadorEnLista(Respuesta, DatoIndi.CodigoIndicador, DatoIndi.CodigoElementoDimension))
        {
          Respuesta.Add(DatoIndi);
        }
      }
    }


    public async static Task<List<CEntradaIndicador>> IndicadoresEnContenidoAsync(HttpClient Http, CPreguntaPreguntaWISCN Entrada)
    {
      List<CEntradaIndicador> Respuesta = new List<CEntradaIndicador>();
      if (Entrada.Clase == ClaseDetalle.Indicador)
      {
        if (Entrada.CodigoDimension >= 0 &&
            Entrada.CodigoDimension == Contenedores.CContenedorDatos.DimensionIndicador(Entrada.CodigoElemento))
        {
          foreach (CEntradaIndicador DatoIndi in await ListaIndicadoresDimensionAsync(Http, Entrada.CodigoElemento,
            Entrada.CodigoDimension, Entrada.CodigoElementoDimension))
          {
            if (!IndicadorEnLista(Respuesta, DatoIndi.CodigoIndicador, DatoIndi.CodigoElementoDimension))
            {
              Respuesta.Add(DatoIndi);
            }
          }
        }
        else
        {
          if (!IndicadorEnLista(Respuesta, Entrada.CodigoElemento, Entrada.CodigoElementoDimension))
          {
            Respuesta.Add(new CEntradaIndicador(Entrada.CodigoElemento, Entrada.CodigoElementoDimension));
          }
        }
      }
      else
      {
        if (Entrada.Clase == ClaseDetalle.Pregunta)
        {
          List<CEntradaIndicador> ListaLocal =
              await Contenedores.CContenedorDatos.CodigosIndicadoresPreguntaAsync(Http, Entrada.CodigoElemento);
          foreach (CEntradaIndicador Elem in ListaLocal)
          {
            if (Entrada.CodigoDimension < 0 || Elem.CodigoElementoDimension >= 0)
            {
              if (!IndicadorEnLista(Respuesta, Elem.CodigoIndicador, Elem.CodigoElementoDimension))
              {
                Respuesta.Add(Elem);
              }
            }
            else
            {
              foreach (CEntradaIndicador DatoIndi in await ListaIndicadoresDimensionAsync(Http, Elem.CodigoIndicador,
                Entrada.CodigoDimension, Elem.CodigoElementoDimension))
              {
                if (!IndicadorEnLista(Respuesta, DatoIndi.CodigoIndicador, DatoIndi.CodigoElementoDimension))
                {
                  Respuesta.Add(DatoIndi);
                }
              }
            }
          }
        }
      }
      return Respuesta;
    }

    public async static Task<List<CEntradaIndicador>> IndicadoresEnContenidoAsync(HttpClient Http,
          CDetallePreguntaCN Entrada)
    {
      List<CEntradaIndicador> Respuesta = new List<CEntradaIndicador>();
      switch (Entrada.ClaseDeDetalle)
      {
        case ClaseDetalle.Indicador:
          if (!IndicadorEnLista(Respuesta, Entrada.Codigo, -1))
          {
            Respuesta.Add(new CEntradaIndicador(Entrada.Codigo, -1));
          }
          break;
        case ClaseDetalle.Pregunta:
          List<CEntradaIndicador> ListaLocal =
              await Contenedores.CContenedorDatos.CodigosIndicadoresPreguntaAsync(Http, Entrada.Codigo);
          AcoplarListaIndicadores(Respuesta, ListaLocal);
          break;
        case ClaseDetalle.SalaReunion:
          List<CEntradaIndicador> ListaLocal2 =
              await Contenedores.CContenedorDatos.CodigosIndicadoresSalaReunionAsync(Http, Entrada.Codigo);
          AcoplarListaIndicadores(Respuesta, ListaLocal2);
          break;
      }
      return Respuesta;
    }

    private static CPuntoWFSCN CopiarPuntoWFS(CPuntoWFSCN Punto)
    {
      CPuntoWFSCN Respuesta = new CPuntoWFSCN();
      Respuesta.Codigo = Punto.Codigo;
      Respuesta.Nombre = Punto.Nombre;
      Respuesta.Punto = CopiarPunto(Punto.Punto);
      return Respuesta;
    }

    private static List<CPuntoWFSCN> CopiarPuntosWFS(List<CPuntoWFSCN> Puntos)
    {
      List<CPuntoWFSCN> Respuesta = new List<CPuntoWFSCN>();
      foreach (CPuntoWFSCN Punto in Puntos)
      {
        Respuesta.Add(CopiarPuntoWFS(Punto));
      }
      return Respuesta;
    }

    private static CValorDimensionCN CopiarValorDimension(CValorDimensionCN Punto)
    {
      CValorDimensionCN Respuesta = new CValorDimensionCN();
      Respuesta.Dimension = Punto.Dimension;
      Respuesta.Valor = Punto.Valor;
      return Respuesta;
    }

    private static List<CValorDimensionCN> CopiarDimensiones(List<CValorDimensionCN> Dimensiones)
    {
      List<CValorDimensionCN> Respuesta = new List<CValorDimensionCN>();
      foreach (CValorDimensionCN Valor in Dimensiones)
      {
        Respuesta.Add(CopiarValorDimension(Valor));
      }
      return Respuesta;
    }

    private static CPosicionWFSCN CopiarPunto(CPosicionWFSCN Punto)
    {
      CPosicionWFSCN Respuesta = new CPosicionWFSCN();
      Respuesta.X = Punto.X;
      Respuesta.Y = Punto.Y;
      return Respuesta;
    }

    private static List<CPosicionWFSCN> CopiarContorno(List<CPosicionWFSCN> Contorno)
    {
      List<CPosicionWFSCN> Respuesta = new List<CPosicionWFSCN>();
      foreach (CPosicionWFSCN Punto in Contorno)
      {
        Respuesta.Add(CopiarPunto(Punto));
      }
      return Respuesta;
    }

    private static CLineaWFSCN CopiarLineaWFS(CLineaWFSCN Linea)
    {
      CLineaWFSCN Respuesta = new CLineaWFSCN();
      Respuesta.Codigo = Linea.Codigo;
      Respuesta.Nombre = Linea.Nombre;
      Respuesta.Centro = CopiarPunto(Linea.Centro);
      Respuesta.Contorno = CopiarContorno(Linea.Contorno);
      return Respuesta;
    }

    private static List<CLineaWFSCN> CopiarLineasWFS(List<CLineaWFSCN> Lineas)
    {
      List<CLineaWFSCN> Respuesta = new List<CLineaWFSCN>();
      foreach (CLineaWFSCN Linea in Lineas)
      {
        Respuesta.Add(CopiarLineaWFS(Linea));
      }
      return Respuesta;
    }

    private static CAreaWFSCN CopiarArea(CAreaWFSCN Area)
    {
      CAreaWFSCN Respuesta = new CAreaWFSCN();
      Respuesta.Codigo = Area.Codigo;
      Respuesta.Area = Area.Area;
      Respuesta.Nombre = Area.Nombre;
      Respuesta.Centro = CopiarPunto(Area.Centro);
      Respuesta.Contorno = CopiarContorno(Area.Contorno);
      Respuesta.Dimensiones = CopiarDimensiones(Area.Dimensiones);
      return Respuesta;
    }

    private static List<CAreaWFSCN> CopiarAreasWFS(List<CAreaWFSCN> Areas)
    {
      List<CAreaWFSCN> Respuesta = new List<CAreaWFSCN>();
      if (Areas != null)
      {
        foreach (CAreaWFSCN Area in Areas)
        {
          Respuesta.Add(CopiarArea(Area));
        }
      }
      return Respuesta;
    }

    public static CCapaWFSCN CopiarCapaWFS(CCapaWFSCN Layer)
    {
      CCapaWFSCN Copiado = new CCapaWFSCN();
      Copiado.Codigo = Layer.Codigo;
      Copiado.Areas = CopiarAreasWFS(Layer.Areas);
      Copiado.Puntos = CopiarPuntosWFS(Layer.Puntos);
      Copiado.Descripcion = Layer.Descripcion;
      Copiado.Capa = Layer.Capa;
      Copiado.CodigoProveedor = Layer.CodigoProveedor;
      Copiado.Detalle = Layer.Detalle;
      Copiado.DireccionURL = Layer.DireccionURL;
      Copiado.Elemento = Layer.Elemento;
      Copiado.FechaRefresco = Layer.FechaRefresco;
      Copiado.GuardaCompactada = Layer.GuardaCompactada;
      Copiado.Lineas = CopiarLineasWFS(Layer.Lineas);
      Copiado.NombreCampoCodigo = Layer.NombreCampoCodigo;
      Copiado.NombreCampoDatos = Layer.NombreCampoDatos;
      Copiado.NombreElemento = Layer.NombreElemento;
      Copiado.PuntosMaximosContorno = Layer.PuntosMaximosContorno;
      Copiado.Version = Layer.Version;
      return Copiado;
    }

    public static CCapaWISCN CopiarCapaWIS(CCapaWISCN Layer)
    {
      CCapaWISCN Copiado = new CCapaWISCN();
      Copiado.Codigo = Layer.Codigo;
      Copiado.CodigoWFS = Layer.CodigoWFS;
      Copiado.Descripcion = Layer.Descripcion;
      return Copiado;
    }

    public static CMapaBingCN CrearMapaBing()
    {
      CMapaBingCN Respuesta = new CMapaBingCN();
      Respuesta.AbscisaCentro = -1000;
      Respuesta.Capas = new List<CCapaBingCN>();
      Respuesta.Codigo = -1;
      Respuesta.Descripcion = "";
      Respuesta.NivelZoom = 7;
      Respuesta.OrdenadaCentro = -1000;
      Respuesta.Preguntas = new List<CElementoPreguntasWISCN>();
      return Respuesta;
    }

    public static async Task<ColorBandera> ObtenerColorBanderaPreguntaPreguntaAsync(HttpClient Http, CPreguntaPreguntaWISCN Pregunta)
		{
      ColorBandera RespuestaLocal = ColorBandera.SinDatos;
      List<CEntradaIndicador> IndicadoresMenu = await IndicadoresEnContenidoAsync(Http, Pregunta);
      foreach (CEntradaIndicador Elemento in IndicadoresMenu)
      {
        ColorBandera ColorLocal =
              await ObtenerColorIndicadorAsync(Http,
                  Elemento.CodigoIndicador, Elemento.CodigoElementoDimension);
        RespuestaLocal = CRutinas.ColorBanderaMasCritico(RespuestaLocal, ColorLocal);
      }
      return RespuestaLocal;
    }

    /// <summary>
    /// Determina el color de una chinche o area caliente, basado en el indicador
    /// seleccionado y las entradas del menu con dimension.
    /// </summary>
    /// <param name="Detalle"></param>
    /// <param name="Contenidos"></param>
    /// <returns></returns>
    public async static Task<ColorBandera> ObtenerColorBanderaAsync(HttpClient Http,
          List<CPreguntaPreguntaWISCN> Contenidos)
    {

      ColorBandera Respuesta = ColorBandera.SinDatos;

      if (Contenidos != null)
      {
        foreach (CPreguntaPreguntaWISCN Pregunta in Contenidos)
        {
          ColorBandera ColorLocal = await ObtenerColorBanderaPreguntaPreguntaAsync(Http, Pregunta);
          Pregunta.Color = ColorBanderaATextoEspaniol(ColorLocal);
          Respuesta = CRutinas.ColorBanderaMasCritico(Respuesta, ColorLocal);
        }
      }
      return Respuesta;
    }

    /// <summary>
    /// Determina el color de una chinche o area caliente, basado en el indicador
    /// seleccionado y las entradas del menu con dimension.
    /// </summary>
    /// <param name="Detalle"></param>
    /// <param name="Contenidos"></param>
    /// <returns></returns>
    public async static Task<ColorBandera> ObtenerColorBanderaAsync(HttpClient Http,
          CDetallePreguntaIcono Detalle,
          List<CPreguntaPreguntaWISCN> Contenidos)
    {

      ColorBandera Respuesta = ColorBandera.SinDatos;

      List<CEntradaIndicador> IndicadoresMenu = await IndicadoresEnContenidosAsync(Http, Contenidos);
      foreach (CEntradaIndicador Elemento in IndicadoresMenu)
      {
        if (Detalle == null || Elemento.CodigoIndicador == Detalle.Detalle.Codigo)
        {
          ColorBandera ColorLocal = await ObtenerColorIndicadorAsync(Http,
                    Elemento.CodigoIndicador, Elemento.CodigoElementoDimension);
          if (Respuesta == ColorBandera.SinDatos ||
              Respuesta == ColorBandera.Blanco)
          {
            Respuesta = ColorLocal;
          }
          switch (ColorLocal)
          {
            case ColorBandera.Verde:
              if (Respuesta == ColorBandera.Azul)
              {
                Respuesta = ColorBandera.Verde;
              }
              break;
            case ColorBandera.Amarillo:
              if (Respuesta == ColorBandera.Azul ||
                    Respuesta == ColorBandera.Verde)
              {
                Respuesta = ColorBandera.Amarillo;
              }
              break;
            case ColorBandera.Rojo:
              Respuesta = ColorBandera.Rojo;
              break;
          }
        }
      }
      return Respuesta;
    }

    public async static Task<ColorBandera> ObtenerColorBanderaAsync(HttpClient Http,
          List<CDetallePreguntaCN> Contenidos)
    {

      ColorBandera Respuesta = ColorBandera.SinDatos;

      foreach (CDetallePreguntaCN Detalle in Contenidos)
			{
        if (Detalle.Color == ColorBandera.NoCorresponde)
        {
          List<CEntradaIndicador> IndicadoresMenu = await IndicadoresEnContenidoAsync(Http, Detalle);
          foreach (CEntradaIndicador Elemento in IndicadoresMenu)
          {
            ColorBandera ColorLocal = await ObtenerColorIndicadorAsync(Http,
                    Elemento.CodigoIndicador, Elemento.CodigoElementoDimension);
            Detalle.Color = CRutinas.ColorBanderaMasCritico(Detalle.Color, ColorLocal);
          }
        }
        Respuesta = CRutinas.ColorBanderaMasCritico(Respuesta, Detalle.Color);
      }

      return Respuesta;
    }

    private const string SEPARADOR = "$$";
    private const string SEPARADOR_CON_PAR = "]$$[";

    public static string ListaPrmsATexto(List<CParametroExt> Prms)
		{
      string Respuesta = "["+Prms.Count.ToString() + "]"+SEPARADOR;
      foreach (CParametroExt Prm in Prms)
			{
        Respuesta += "[" + Prm.CodigoSubconsulta.ToString() + SEPARADOR_CON_PAR +
            Prm.Nombre + SEPARADOR_CON_PAR + (Prm.TieneQuery ? "Y" : "N") + SEPARADOR_CON_PAR +
            Prm.Tipo + SEPARADOR_CON_PAR + FechaATexto(Prm.ValorDateTime) + SEPARADOR_CON_PAR +
            FloatVStr(Prm.ValorFloat) + SEPARADOR_CON_PAR + Prm.ValorInteger.ToString() + SEPARADOR_CON_PAR +
            Prm.ValorString + "]" + SEPARADOR;
			}
      return Respuesta;
		}

    public static Int32 UbicarNivelZoom(double Ancho, double Alto, double RangoLng, double RangoLat)
		{
      double Relacion1 = RangoLat * 650 / Alto;
      double Relacion2 = RangoLng * 1280 / Ancho;
      double Salto = Math.Max(Relacion1, Relacion2);
      if (Salto == 0)
      {
        return 10;
      }
      else
      {
        Salto *= Math.Pow(2, 7);
        for (Int32 i = 15; i > 1; i--)
        {
          if (Salto < 1.5)
          {
            return i;
          }
          Salto /= 2;
        }
        return 1;
      }
    }

    public static List<double> ListaAReales(string Lista)
    {
      string[] Datos = Lista.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      List<double> Valores = new List<double>();
      foreach (string Valor in Datos)
			{
        Valores.Add(StrVFloat(Valor));
			}
      return Valores;
    }

    public static CElementoPreguntasWISCN CrearElementoPreguntas(CPunto Posicion)
    {
      CElementoPreguntasWISCN Respuesta = new CElementoPreguntasWISCN();
      Respuesta.Codigo = -1;
      Respuesta.Nombre = "";
      Respuesta.CodigoArea = "";
      Respuesta.Dimension = -1;
      Respuesta.ElementoDimension = -1;
      Respuesta.Contenidos = new List<CPreguntaPreguntaWISCN>();
      Respuesta.Abscisa = (Posicion == null ? -1000 : Posicion.Abscisa);
      Respuesta.Ordenada = (Posicion == null ? -1000 : Posicion.Ordenada);
      return Respuesta;
    }

    public static string TextoMsg(Exception ex)
		{
      return (ex.InnerException == null ? ex.Message : ex.InnerException.ToString());
		}

    public static void UbicarCentro(double AnchoPixels, double AltoPixels, double LatMin, double LatMax,
        double LngMin, double LngMax, out double LatCentro, out double LngCentro, out Int32 NivelZoom)
    {
        LatCentro = (LatMin + LatMax) / 2;
        LngCentro = (LngMin + LngMax) / 2;

        double Relacion1 = (LatMax - LatMin) * 650 / AltoPixels;
        double Relacion2 = (LngMax - LngMin) * 1280 / AnchoPixels;
        double Salto = Math.Max(Relacion1, Relacion2);
        if (Salto == 0)
        {
          NivelZoom = 10;
        }
        else
        {
          Salto *= Math.Pow(2, 7);
          for (Int32 i = 15; i > 1; i--)
          {
            if (Salto < 1.5)
            {
              NivelZoom = i;
              return;
            }
            Salto /= 2;
          }
          NivelZoom = 1;
        }

      }



    //public static Point PuntoIncorrecto()
    //{
    //  return new Point(-1000, -1000);
    //}

    //private static Point PuntoDesdeCoordenadas(string Valor)
    //{
    //  if (Valor.Contains(" "))
    //  {
    //    try
    //    {
    //      string[] Valores = Valor.Trim().Split(' ');
    //      return new Point(CRutinas.StrVFloat(Valores[0]), CRutinas.StrVFloat(Valores[1]));
    //    }
    //    catch (Exception)
    //    {
    //      //
    //    }
    //  }
    //  return null;

    //}

    //public static Point PosicionValor(string Valor, CCapaWFSCN Capa)
    //{
    //  Point PuntoCoordenadas = PuntoDesdeCoordenadas(Valor);
    //  if (PuntoCoordenadas != null && !double.IsNaN(PuntoCoordenadas.X) && !double.IsNaN(PuntoCoordenadas.Y))
    //  {
    //    return PuntoCoordenadas;
    //  }
    //  else
    //  {
    //    foreach CAreaWFSCN Area in Capa.Areas)
    //    {
    //      if (Area.Codigo.Equals(Valor, StringComparison.OrdinalIgnoreCase))
    //      {
    //        return new Point(Area.Centro.X, Area.Centro.Y);
    //      }
    //    }

    //    foreach CPuntoWFSCN Punto in Capa.Puntos)
    //    {
    //      if (Punto.Codigo.Equals(Valor, StringComparison.OrdinalIgnoreCase))
    //      {
    //        return new Plantillas.Point(Punto.Punto.X, Punto.Punto.Y);
    //      }
    //    }
    //  }

    //  return PuntoIncorrecto();

    //}

    //private static bool TramoIntersectaPorIzq(CPosicionWFSCN P1, CPosicionWFSCN P2, Point Punto)
    //{
    //  if (P1.X > Punto.X && P2.X >= Punto.X)
    //  {
    //    return false;
    //  }
    //  else
    //  {
    //    if ((P1.Y > Punto.Y && P2.Y >= Punto.Y) || (P1.Y < Punto.Y && P2.Y <= Punto.Y))
    //    {
    //      return false;
    //    }
    //  }
    //  if (P1.Y == P2.Y)
    //  {
    //    return (Punto.Y == P1.Y && P1.X <= Punto.X);
    //  }
    //  else
    //  {
    //    double AbscRefe = P1.X + (P2.X - P1.X) * (Punto.Y - P1.Y) / (P2.Y - P1.Y);
    //    return (AbscRefe <= Punto.X);
    //  }
    //}

    //public static bool AreaContienePunto(CAreaWFSCN Area, Point Punto)
    //{
    //  // el criterio es hacer una linea horizontal desde el infinito y verificar cuantas veces corta
    //  // al contorno.
    //  Int32 Cantidad = 0;
    //  for (Int32 i = 1; i < Area.Contorno.Count; i++)
    //  {
    //    if (TramoIntersectaPorIzq(Area.Contorno[i - 1], Area.Contorno[i], Punto))
    //    {
    //      Cantidad++;
    //    }
    //  }
    //  return ((Cantidad % 2) != 0);
    //}

    //public static bool PoligonoContienePunto(List<CPosicionWFSCN> Contorno, Point Punto)
    //{
    //  // el criterio es hacer una linea horizontal desde el infinito y verificar cuantas veces corta
    //  // al contorno.
    //  Int32 Cantidad = 0;
    //  for (Int32 i = 1; i < Contorno.Count; i++)
    //  {
    //    if (TramoIntersectaPorIzq(Contorno[i - 1], Contorno[i], Punto))
    //    {
    //      Cantidad++;
    //    }
    //  }
    //  return ((Cantidad % 2) != 0);
    //}

    //public static CAreaWFSCN AreaCentroPunto(CCapaWFSCN Capa, Point Punto)
    //{
    //  double DistMin = double.MaxValue;
    //  CAreaWFSCN Respuesta = null;
    //  foreach (CAreaWFSCN Area in Capa.Areas)
    //  {
    //    double DistArea = (Area.Centro.X - Punto.X) * (Area.Centro.X - Punto.X) +
    //        (Area.Centro.Y - Punto.Y) * (Area.Centro.Y - Punto.Y);
    //    if (DistArea < DistMin)
    //    {
    //      DistMin = DistArea;
    //      Respuesta = Area;
    //    }
    //  }
    //  return (DistMin < 0.000001 ? Respuesta : null);
    //}

    //public static CAreaWFSCN AreaContenedoraPunto(CCapaWFSCN Capa, Point Punto)
    //{
    //  foreach (CAreaWFSCN Area in Capa.Areas)
    //  {
    //    if (AreaContienePunto(Area, Punto))
    //    {
    //      return Area;
    //    }
    //  }
    //  return null;
    //}

    //public static double DistanciaCuadradaEntrePuntos(CPosicionWFSCN P1, Point P2)
    //{
    //  return (P1.X - P2.X) * (P1.X - P2.X) + (P1.Y - P2.Y) * (P1.Y - P2.Y);
    //}

    //public static string DeterminarAreaContenedora(Point Punto, CCapaWFSCN CapaWFS, string ValorResto,
    //    bool PorCodigo = false, bool UsaCentro = false)
    //{
    //  CAreaWFSCN Area = (UsaCentro ? AreaCentroPunto(CapaWFS, Punto) :
    //      AreaContenedoraPunto(CapaWFS, Punto));
    //  if (Area == null && UsaCentro)
    //  {
    //    Area = AreaContenedoraPunto(CapaWFS, Punto);
    //  }
    //  return (Area == null ? ValorResto : (PorCodigo ? Area.Codigo : Area.Nombre));
    //}

    //public static CPuntoWFSCN PuntoMasCercano(CCapaWFSCN Capa, Point Punto, double Rango)
    //{
    //  double RangoRefe = Rango * 180 / (6378137.0 * Math.PI);
    //  RangoRefe = RangoRefe * RangoRefe;
    //  double DistMinima = 1000000000;
    //  CPuntoWFSCN PuntoMasCercano = null;
    //  foreach (CPuntoWFSCN PuntoWFS in Capa.Puntos)
    //  {
    //    double DistPunto = DistanciaCuadradaEntrePuntos(PuntoWFS.Punto, Punto);
    //    if (DistPunto < DistMinima)
    //    {
    //      DistMinima = DistPunto;
    //      PuntoMasCercano = PuntoWFS;
    //    }
    //  }
    //  if (DistMinima <= RangoRefe)
    //  {
    //    return PuntoMasCercano;
    //  }
    //  else
    //  {
    //    return null;
    //  }
    //}

    //public static string DeterminarPuntoMasCercano(Point Punto, CCapaWFSCN CapaWFS,
    //    double Rango, string ValorResto, bool PorCodigo = false)
    //{
    //  CPuntoWFSCN PuntoWFS = PuntoMasCercano(CapaWFS, Punto, Rango);
    //  return (PuntoWFS == null ? ValorResto : (PorCodigo ? PuntoWFS.Codigo : PuntoWFS.Nombre));
    //}

    //public static string TextoPunto(Point Punto, CCapaWFSCN CapaWFS, double Rango,
    //    string ValorResto, bool PorCodigo = false, bool UsaCentro = false)
    //{
    //  switch (CapaWFS.Elemento)
    //  {
    //    case ElementoWFS.Superficie:
    //      return DeterminarAreaContenedora(Punto, CapaWFS, ValorResto, PorCodigo, UsaCentro);
    //    case ElementoWFS.Punto:
    //      return DeterminarPuntoMasCercano(Punto, CapaWFS, Rango, ValorResto, PorCodigo);
    //    default:
    //      return ValorResto;
    //  }
    //}

    public const string ADENTRO = "SI";
    public const string AFUERA = "NO";

    //public static List<string> ExtraerListaElementosWFS(CCapaWFSCN CapaWFS, bool PorCodigo = false)
    //{
    //  List<string> Respuesta = new List<string>();
    //  foreach CAreaWFSCN Area in CapaWFS.Areas)
    //  {
    //    Respuesta.Add(PorCodigo ? Area.Codigo.ToUpper() : Area.Nombre.ToUpper());
    //  }
    //  foreach CPuntoWFSCN Punto in CapaWFS.Puntos)
    //  {
    //    Respuesta.Add(PorCodigo ? Punto.Codigo.ToUpper() : Punto.Nombre.ToUpper());
    //  }
    //  return Respuesta;
    //}

    //public static List<CElementoPosicion> ExtraerListaElementosPosicionWFS(CCapaWFSCN CapaWFS)
    //{
    //  List<CElementoPosicion> Respuesta = new List<CElementoPosicion>();
    //  foreach CAreaWFSCN Area in CapaWFS.Areas)
    //  {
    //    Respuesta.Add(new CElementoPosicion(Area.Nombre.ToUpper(), Area.Centro));
    //  }
    //  foreach CPuntoWFSCN Punto in CapaWFS.Puntos)
    //  {
    //    Respuesta.Add(new CElementoPosicion(Punto.Nombre.ToUpper(), Punto.Punto));
    //  }
    //  return Respuesta;
    //}

    //public static bool PuntoEnCirculos(Point Punto,
    //    List<CPosicionWFSCN> Contorno, double Distancia2)
    //{
    //  foreach (CPosicionWFSCN Posicion in Contorno)
    //  {
    //    if (DistanciaCuadradaEntrePuntos(Posicion, Punto) <= Distancia2)
    //    {
    //      return true;
    //    }
    //  }
    //  return false;
    //}

//    public static async Task<byte[]> LeerDetalleIndicadorAsync(Int32 Indicador, Int32 Dimension, Int32 CodigoElemento)
//    {
//      WCFBPIClient Cliente = ObtenerClienteBPI();
//      try
//      {
//        CRespuestaInformacionAlarmaVarias RespTend = await Cliente.HistoriaAlarmaConDimensionAsync(Contenedores.CContenedorDatos.Ticket,
//              Indicador, Dimension, CodigoElemento, DateTime.Now);
//        if (!RespTend.RespuestaOK)
//        {
//          throw new Exception(RespTend.MensajeError);
//        }
//        if (RespTend.Instancias.Count == 0)
//        {
//          return new byte[0];
//        }

//        string szGUID = new Guid().ToString();
//        CRespuestaDatasetBin Respuesta = await Cliente.ObtenerDetalleIndicadorBinAsync(Contenedores.CContenedorDatos.Ticket,
//            szGUID, Indicador, RespTend.Instancias.Last().Periodo, Dimension, CodigoElemento, false);
//        bool bContinuar = true;
//        while (bContinuar)
//        {
//          if (!Respuesta.RespuestaOK)
//          {
//            throw new Exception("Al obtener detalle SC " + Respuesta.MensajeError);
//          }
//          switch (Respuesta.Situacion)
//          {
//            case SituacionPedido.EnMarcha:
//              Respuesta = await Cliente.RefrescarPedidoDetalleIndicadorBinAsync(Contenedores.CContenedorDatos.Ticket,
//                  szGUID, true);
//              break;
//            default:
//              // hay datos.
//              bContinuar = false;
//              break;

//          }
//        }
//        // Si hay datos arma un dataset y lo retorna.
//        if (Respuesta.Situacion == SituacionPedido.Completado)
//        {
//          return Respuesta.Datos;
//        }
//        else
//        {
//          return new byte[0];
//        }

//      }
//      catch (Exception ex)
//      {
//        DesplegarMsg(ex);
//        return new byte[0];
//      }
//      finally
//      {
//        await Cliente.CloseAsync();
//      }
//    }

//    public static object OBJ_GLOBAL = new object();
////    private static bool gbHabilitado = true;

//    public static async Task<byte[]> LeerSubconsultaAsync(Int32 Codigo, List<CParametroExt> Prms)
//    {
//      WCFBPIClient Cliente = ObtenerClienteBPI();
//      try
//      {
//        string szGUID = new Guid().ToString();
//        CRespuestaDatasetBinSC Respuesta = await Cliente.DetalleSubconsultaAsync(Contenedores.CContenedorDatos.Ticket,
//            Codigo, Prms, "", -1, szGUID, false);
//        bool bContinuar = true;
//        while (bContinuar)
//        {
//          if (!Respuesta.RespuestaOK)
//          {
//            throw new Exception("Al obtener detalle SC " + Respuesta.MensajeError);
//          }
//          switch (Respuesta.Situacion)
//          {
//            case SituacionPedido.EnMarcha:
//              Respuesta = await Cliente.RefrescarPedidoDetalleSubconsultaBinAsync(Contenedores.CContenedorDatos.Ticket,
//                  szGUID, true);
//              break;
//            default:
//              // hay datos.
//              bContinuar = false;
//              break;

//          }
//        }
//        // Si hay datos arma un dataset y lo retorna.
//        if (Respuesta.Situacion == SituacionPedido.Completado)
//        {
//          return Respuesta.Datos;
//        }
//        else
//        {
//          return new byte[0];
//        }

//      }
//      catch (Exception ex)
//      {
//        DesplegarMsg(ex);
//        return new byte[0];
//      }
//      finally
//      {
//        await Cliente.CloseAsync();
//      }
//    }

    private static async Task<List<string>> ObtenerLineasEnBlockAsync(Canvas2DContext Contexto, string Texto, double Ancho, double Alto)
    {
      List<string> Respuesta = new List<string>();
      string[] Palabras = Texto.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
      string LineaAnterior = "";
      for (Int32 i = 0; i < Palabras.Length; i++)
      {
        string Linea = LineaAnterior + (LineaAnterior.Length > 0 ? " " : "") + Palabras[i];
        TextMetrics Medida = await Contexto.MeasureTextAsync(Linea);
        if (Medida.Width < Ancho)
        {
          LineaAnterior = Linea;
        }
        else
        {
          if (LineaAnterior.Length == 0)
          {
            Respuesta.Add(Palabras[i]);
            LineaAnterior = "";
          }
          else
          {
            Respuesta.Add(LineaAnterior);
            LineaAnterior = Palabras[i];
          }
        }
      }
      if (LineaAnterior.Length > 0)
      {
        Respuesta.Add(LineaAnterior);
      }
      return Respuesta;
    }

    public static async Task PonerEtiquetaHorizontalAsync(Canvas2DContext Contexto,
          double Abscisa, double Ordenada, double Ancho, double Alto, string Texto, double DimensionCaracter)
    {
      TextMetrics Medidas = await Contexto.MeasureTextAsync(Texto);
      List<string> Lineas = new List<string>();
      if (Medidas.Width < Ancho)
      {
        Lineas.Add(Texto);
      }
      else
      {
        Lineas = await ObtenerLineasEnBlockAsync(Contexto, Texto, Ancho, Alto);
      }
      Int32 LineasMax = Math.Min(Lineas.Count, (Int32)Math.Floor(Alto / (DimensionCaracter + 2)));
      double Ord0 = Ordenada + (Alto - LineasMax * DimensionCaracter + (LineasMax - 1) * 2) / 2 + DimensionCaracter;
      for (Int32 i = 0; i < LineasMax; i++)
      {
        TextMetrics Medida = await Contexto.MeasureTextAsync(Lineas[i]);
        await Contexto.FillTextAsync(Lineas[i], Abscisa + (Ancho - Medida.Width) / 2, Ord0);
        Ord0 += DimensionCaracter + 2;
      }
    }

    public static Int32 TraducirCodigo(Int32 Anterior, List<ParCodigos> Pares)
    {
      ParCodigos Par = (from P in Pares
                        where P.Anterior == Anterior
                        select P).FirstOrDefault();
      return (Par == null ? -1000 : Par.Actual);
    }

    public static Int32 CodigoColorAlarma(string Color)
    {
      switch (Color.ToUpper())
      {
        case COLOR_ROJO:
          return 4;
        case COLOR_AMARILLO:
          return 3;
        case COLOR_VERDE:
          return 2;
        case COLOR_AZUL:
          return 1;
        default:
          return -1;
      }
    }

    public static string ColorDesdeCodigo(Int32 Codigo)
    {
      switch (Codigo)
      {
        case 4:
          return "#CC4726";
        case 3:
          return "#F19E36";
        case 2:
          return "#489432";
        case 1:
          return "#5683E8";
        default:
          return "gray";
      }
    }

    public static string ColorBanderaATexto(ColorBandera Color, bool Puro)
    {
      if (Puro)
      {
        switch (Color)
        {
          case ColorBandera.Blanco:
            return "white";
          case ColorBandera.Azul:
            return "blue"; // "lightblue";
          case ColorBandera.Verde:
            return "green";
          case ColorBandera.Amarillo:
            return "yellow";
          case ColorBandera.Rojo:
            return "red";
          default:
            return "lightgray";
        }
      }
      else
      {
        switch (Color)
        {
          case ColorBandera.Blanco:
            return "rgba(255 ,255, 255, 0.5)";
          case ColorBandera.Azul:
            return "rgba(128, 128, 255, 0.5)"; // "lightblue";
          case ColorBandera.Verde:
            return "rgba(128, 255, 128, 0.5)";
          case ColorBandera.Amarillo:
            return "rgba(255, 255, 128, 0.5)";
          case ColorBandera.Rojo:
            return "rgba(255, 128, 128, 0.5)";
          default:
            return "rgba(128, 128, 128, 0.5)";
        }
      }
    }

    public static string ColorBanderaATextoEspaniol(ColorBandera Color)
    {
      switch (Color)
      {
        case ColorBandera.Blanco:
          return COLOR_BLANCO;
        case ColorBandera.Azul:
          return COLOR_AZUL; // "lightblue";
        case ColorBandera.Verde:
          return COLOR_VERDE;
        case ColorBandera.Amarillo:
          return COLOR_AMARILLO;
        case ColorBandera.Rojo:
          return COLOR_ROJO;
        default:
          return COLOR_GRIS;
      }
    }

    /// <summary>
    /// Determina el color de una chinche o area caliente, basado en el indicador
    /// seleccionado y las entradas del menu con dimension.
    /// </summary>
    /// <param name="Detalle"></param>
    /// <param name="Contenidos"></param>
    /// <returns></returns>
    //public async static Task<ColorBandera> ObtenerColorBanderaAsync(
    //      List<CPreguntaPreguntaWISCN> Contenidos)
    //{

    //  ColorBandera Respuesta = ColorBandera.SinDatos;

    //  List<CEntradaIndicador> IndicadoresMenu = IndicadoresEnContenidos(Contenidos);
    //  foreach (CEntradaIndicador Elemento in IndicadoresMenu)
    //  {
    //    ColorBandera ColorLocal =
    //          await ObtenerColorIndicadorAsync(
    //              Elemento.CodigoIndicador, Elemento.CodigoElementoDimension);
    //    if (Respuesta == ColorBandera.SinDatos ||
    //        Respuesta == ColorBandera.Blanco)
    //    {
    //      Respuesta = ColorLocal;
    //    }
    //    switch (ColorLocal)
    //    {
    //      case ColorBandera.Verde:
    //        if (Respuesta == ColorBandera.Azul)
    //        {
    //          Respuesta = ColorBandera.Verde;
    //        }
    //        break;
    //      case ColorBandera.Amarillo:
    //        if (Respuesta == ColorBandera.Azul ||
    //              Respuesta == ColorBandera.Verde)
    //        {
    //          Respuesta = ColorBandera.Amarillo;
    //        }
    //        break;
    //      case ColorBandera.Rojo:
    //        Respuesta = ColorBandera.Rojo;
    //        break;
    //    }
    //  }
    //  return Respuesta;
    //}

    //public static CElementoPreguntasWISCN CrearElementoPreguntas(CPunto Posicion)
    //{
    //  CElementoPreguntasWISCN Respuesta = new CElementoPreguntasWISCN();
    //  Respuesta.Codigo = -1;
    //  Respuesta.Nombre = "";
    //  Respuesta.CodigoArea = "";
    //  Respuesta.Dimension = -1;
    //  Respuesta.ElementoDimension = -1;
    //  Respuesta.Contenidos = new List<CPreguntaPreguntaWISCN>();
    //  Respuesta.Abscisa = (Posicion == null ? -1000 : Posicion.Abscisa);
    //  Respuesta.Ordenada = (Posicion == null ? -1000 : Posicion.Ordenada);
    //  return Respuesta;
    //}

    public class CColoresPregunta
    {
      public string CodigoElemento { get; set; }
      public ColoresParaPreguntas ColorPregunta { get; set; }
      public List<CColorElemento> Preguntas { get; set; }

      public CColoresPregunta()
      {
        CodigoElemento = "";
        ColorPregunta = ColoresParaPreguntas.Gris;
        Preguntas = new List<CColorElemento>();
      }

      public static ColoresParaPreguntas CompararColores(ColoresParaPreguntas C1,
            ColoresParaPreguntas C2)
      {
        if (C2 == ColoresParaPreguntas.Gris)
        {
          return C1;
        }

        switch (C1)
        {
          case ColoresParaPreguntas.Gris:
            return C2; // ColoresParaPreguntas.Gris;
          case ColoresParaPreguntas.Verde:
            return (C2 == ColoresParaPreguntas.Azul ? C1 : C2);
          case ColoresParaPreguntas.Amarillo:
            switch (C2)
            {
              case ColoresParaPreguntas.Azul:
              case ColoresParaPreguntas.Verde:
                return C1;
              default:
                return C2;
            }
          case ColoresParaPreguntas.Rojo:
            return C1; // (C2 == ColoresParaPreguntas.Gris ? C2 : C1);
          case ColoresParaPreguntas.Azul: return C2;
          default:
            return ColoresParaPreguntas.Gris;
        }
      }

      public void AjustarColor()
      {

        ColorPregunta = (Preguntas.Count == 0 ? ColoresParaPreguntas.Gris : Preguntas[0].ColorElemento);

        foreach (CColorElemento ColorLocal in Preguntas)
        {
          ColorPregunta = CompararColores(ColorLocal.ColorElemento, ColorPregunta);
        }
      }

    }

    public static void InformarUsuario(string Msg1, string Msg2)
    {
      if (gFncInformarUsuario != null)
      {
        gFncInformarUsuario(Msg1, Msg2);
      }
    }

    public static void InformarUsuario(string Msg1, Exception ex)
    {
      if (gFncInformarUsuario != null)
      {
        gFncInformarUsuario(Msg1, MostrarMensajeError(ex));
      }
    }

    public static DateTime FechaGraficarPeriodo(string Frecuencia, CInformacionAlarmaCN Periodo)
    {
      switch (Frecuencia)
      {
        case FREC_ANUAL:
          return Periodo.FechaInicial; //FechaFinal.AddDays(-1);
        case FREC_SEMANAL:
          return Periodo.FechaFinal.AddDays(-1);
        default:
          return Periodo.FechaInicial;
      }
    }

    public static void DesplegarMsg(Exception ex)
    {
      Contenedores.CContenedorDatos.MostrarMensaje(ex.InnerException == null ? ex.Message : ex.InnerException.ToString());
    }

    public static void DesplegarMsg(string Msg)
    {
      Contenedores.CContenedorDatos.MostrarMensaje(Msg);
    }

    public static string ColorSecuencia(Int32 Secuencia, byte Opacidad = 255)
    {
      switch (Secuencia % 6)
      {
        case 0: return "#263e6a"; // Color.FromArgb(Opacidad, 38, 62, 106);
        case 1: return "#335b9c"; // Color.FromArgb(Opacidad, 52, 91, 156);
        case 2: return "#4c78c7"; // Color.FromArgb(Opacidad, 76, 120, 199);
        case 3: return "#94aedf"; // Color.FromArgb(Opacidad, 148, 174, 223);
        case 4: return "#b1dde8"; // Color.FromArgb(Opacidad, 177, 221, 232);
        default: return "#dce5f4"; // Color.FromArgb(Opacidad, 220, 229, 244);
      }

    }

    public static Int32 PasosMaximos = 30;
    public static List<string> ArmarListaColores(Int32 Cantidad)
    {
      List<string> Respuesta = new List<string>();
//      Random Semilla = new Random(0); //DateTime.Now.Millisecond);
      for (Int32 i = 0; i < Cantidad; i++)
      {
        Respuesta.Add(ColorSecuencia(i));
      }
      return Respuesta;
    }

    //public static WCFBPIClient ObtenerClienteBPI(int Minutos = 5)
    //{
    //  string Direccion = TableroPecasV5.Properties.Resources.UrlWCF;

    //  System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();

    //  binding.MaxBufferSize = 100000000;
    //  binding.MaxReceivedMessageSize = 100000000;
    //  binding.CloseTimeout = new TimeSpan(0, Minutos, 0);
    //  binding.OpenTimeout = new TimeSpan(0, Minutos, 0);
    //  binding.SendTimeout = new TimeSpan(0, Minutos, 0);
    //  binding.ReceiveTimeout = new TimeSpan(0, Minutos, 0);

    //  return new WCFBPIClient(
    //                   binding,
    //                   new System.ServiceModel.EndpointAddress(
    //                   Direccion));
    //}

    //public static WCFEstructura.WcfEstructuraClient ObtenerClienteEstructura()
    //{
    //  System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();

    //  binding.MaxBufferSize = 100000000;
    //  binding.MaxReceivedMessageSize = 100000000;
    //  binding.CloseTimeout = new TimeSpan(0, 2, 0);
    //  binding.OpenTimeout = new TimeSpan(0, 2, 0);
    //  binding.SendTimeout = new TimeSpan(0, 2, 0);
    //  binding.ReceiveTimeout = new TimeSpan(0, 2, 0);

    //  WCFEstructura.WcfEstructuraClient Respuesta = new WCFEstructura.WcfEstructuraClient();

    //  Respuesta.Endpoint.Address = new System.ServiceModel.EndpointAddress("http://localhost/WCFEstructura/WCF_Estructura.svc");

    //  return Respuesta;
    //}

    public static void AjustarExtremosEscala(ref double Minimo, ref double Maximo, bool bMinimo0 = false)
    {
      if (Maximo <= (Minimo + 0.0000001))
      {
        Minimo = Math.Floor(Minimo);
        Maximo = ((Minimo + 1) >= Maximo ? Minimo + 1 : Minimo + 2);
        return;
      }
      double Factor = 1;
      double Salto = 1.1 * (Maximo - Minimo);
      while (Salto > 10)
      {
        Salto /= 10;
        Factor *= 10;
      }
      while (Salto <= 1)
      {
        Salto *= 10;
        Factor /= 10;
      }
      // ahora salto>1 && Salto<=10.
      if (Salto != Math.Floor(Salto))
      {
        if (Factor >= 10)
        {
          Salto = Math.Floor(Salto * 2 + 1) / 2;
        }
        else
        {
          Salto = Math.Floor(Salto) + 1;
        }
      }
      Salto *= Factor / 10;

      if (bMinimo0 && Minimo >= -0.00001)
      {
        Minimo = 0;
        Maximo = 10 * Salto;
      }
      else
      {
        double Media = Salto * Math.Floor((Minimo + Maximo) / (2 * Salto) + 0.5);
        double MinimoLocal = Media - 5 * Salto;
        while (MinimoLocal > Minimo)
        {
          MinimoLocal -= Salto;
        }
        if (Minimo >= 0 && MinimoLocal < 0)
        {
          Minimo = 0;
        }
        else
        {
          Minimo = MinimoLocal;
        }
        Maximo = Minimo + 10 * Salto;
      }
    }

    public static Int32 DiaEnLaSemana(DateTime Fecha)
    {
      switch (Fecha.DayOfWeek)
      {
        case DayOfWeek.Sunday:
          return 0;
        case DayOfWeek.Monday:
          return 1;
        case DayOfWeek.Tuesday:
          return 2;
        case DayOfWeek.Wednesday:
          return 3;
        case DayOfWeek.Thursday:
          return 4;
        case DayOfWeek.Friday:
          return 5;
        case DayOfWeek.Saturday:
          return 6;
        default:
          return 0; // para el compilador.
      }
    }

    public static string BoolToStr(bool B)
		{
      return (B ? "Y" : "N");
		}

    public static bool StrToBool(String A)
		{
      return (A == "Y");
		}

    public static double BuscarValorEscala(double Valor)
    {
      if (Valor <= 0)
      {
        return 0.1;
      }
      double Dif = 1.25 * Valor;
      double Pot = 1;

      while (Dif < 1)
      {
        Dif *= 10;
        Pot /= 10;
      }

      while (Dif > 10)
      {
        Dif /= 10;
        Pot *= 10;
      }

      double Cte = 1;
      if (Dif < 1.25)
      {
        Cte = 1.25;
      }
      else
      {
        if (Dif < 1.5)
        {
          Cte = 1.5;
        }
        else
        {
          if (Dif < 2)
          {
            Cte = 2;
          }
          else
          {
            if (Dif < 2.5)
            {
              Cte = 2.5;
            }
            else
            {
              Cte = Math.Floor(Dif);
              if (Cte < Dif)
              {
                Cte += 1;
              }
            }
          }
        }
      }

      return Cte * Pot;

    }

    public static string FormatoMes(DateTime Fecha)
    {
      return EnteroLargo(Fecha.Month, 2) + "/" +
          EnteroLargo(Fecha.Year % 100, 2);
    }

    public static DateTime FechaInicialSemana(DateTime Fecha)
    {
      return Fecha.AddDays(-DiaEnLaSemana(Fecha));
    }

    public static DateTime FechaInicioDia(DateTime Fecha, bool IncluyeDia = false)
    {
      if (IncluyeDia)
      {
        return new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 23, 59, 59);
      }
      else
      {
        return new DateTime(Fecha.Year, Fecha.Month, Fecha.Day, 0, 0, 0);
      }
    }

    public static DateTime FechaInicioMes(DateTime Fecha)
    {
      return new DateTime(Fecha.Year, Fecha.Month, 1, 0, 0, 0);
    }

    public static DateTime FechaInicioAnio(DateTime Fecha)
    {
      return new DateTime(Fecha.Year, 1, 1, 0, 0, 0);
    }

    public static SaltoEscalaFechas SaltoDesdeFrecuencia(string Frecuencia)
    {
      switch (Frecuencia)
      {
        case FREC_ANUAL: return SaltoEscalaFechas.Anios;
        //case FREC_MENSUAL:
        //case FREC_MENSUAL_AC:
        //case FREC_BIMESTRAL:
        //case FREC_TRIMESTRAL:
        //case FREC_CUATRIMESTRAL:
        //case FREC_SEMESTRAL:
        //  return SaltoEscalaFechas.Meses;
        case FREC_DIARIA:
        case FREC_SEMANAL:
          return SaltoEscalaFechas.Dias;
        case FREC_HORARIA:
        case FREC_MINUTOS:
          return SaltoEscalaFechas.Horas;
        default:
          return SaltoEscalaFechas.Meses;
      }
    }

    public static SaltoEscalaFechas AjustarExtremosFechas(ref DateTime FechaMinima, ref DateTime FechaMaxima)
    {
      if (FechaMinima > FechaMaxima)
      {
        DateTime FRefe = FechaMaxima;
        FechaMaxima = FechaMinima;
        FechaMinima = FRefe;
      }
      FechaMinima = FechaInicioDia(FechaMinima);
      if (FechaMaxima != FechaInicioDia(FechaMaxima))
      {
        FechaMaxima = FechaInicioDia(FechaMaxima).AddDays(1);
      }

      double SaltoDias = FechaMaxima.ToOADate() - FechaMinima.ToOADate();

      // Si estan en el mismo mes, usar los rangos extremos que estan.
      if (SaltoDias < 2)
      {
        return SaltoEscalaFechas.Horas;
      }

      if (SaltoDias <= 60)
      {
        return SaltoEscalaFechas.Dias;
      }

      // hasta 24 meses redondea los meses.
      if (SaltoDias <= 720)
      {
        FechaMinima = FechaInicioMes(FechaMinima);
        if (FechaMaxima != FechaInicioMes(FechaMaxima))
        {
          FechaMaxima = FechaInicioMes(FechaMaxima).AddMonths(1);
        }
        return SaltoEscalaFechas.Meses;
      }

      FechaMinima = FechaInicioAnio(FechaMinima);
      if (FechaMaxima != FechaInicioAnio(FechaMaxima))
      {
        FechaMaxima = FechaInicioAnio(FechaMaxima).AddYears(1);
      }
      return SaltoEscalaFechas.Anios;

    }

    public static string FormatoValoresDobles(double R)
    {
      if (Math.Abs(R) >= 1000)
      {
        return "###,###,##0"; // "{0:0}";
      }
      else
      {
        if (Math.Abs(R) >= 100)
        {
          return "##0.0"; // "{0:0.0}";
        }
        else
        {
          if (Math.Abs(R) >= 10)
          {
            return "#0.00"; // "{0:0.00}";
          }
          else
          {
            return "0.000"; // "{0:0.000}";
          }
        }
      }
    }

    public static string FormatoDecimales(Int32 Decimales)
    {
      return "###,###,###,###,##0" + (Decimales > 0 ? ("." + new string('0', Decimales)) : "");
    }

    public static string ValorATexto(double Valor, Int32 Decimales = -1)
    {
      if (double.IsNaN(Valor))
      {
        return "";
      }
      else
      {
        string Texto = (Decimales >= 0 ? Valor.ToString(FormatoDecimales(Decimales)) :
            Valor.ToString(FormatoValoresDobles(Valor)));
        if (Decimales > 0 &&
            Texto.IndexOf(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) >= 0)
        {
          while (Texto.EndsWith("0"))
          {
            Texto = Texto.Substring(0, Texto.Length - 1);
          }
          if (Texto.EndsWith(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
          {
            Texto = Texto.Substring(0, Texto.Length - 1);
          }
        }
        return Texto;
      }
    }

    public static string ColorOrden(Int32 Orden)
    {
      switch (Orden % 6)
      {
        case 0: return "#263e6a"; // Color.FromArgb(Opacidad, 38, 62, 106);
        case 1: return "#345b9f"; // Color.FromArgb(Opacidad, 52, 91, 156);
        case 2: return "#4c78c7"; // Color.FromArgb(Opacidad, 76, 120, 199);
        case 3: return "#94aedf"; // Color.FromArgb(Opacidad, 148, 174, 223);
        case 4: return "#b1dde8"; // Color.FromArgb(Opacidad, 177, 221, 232);
        default: return "#dce5f4"; // Color.FromArgb(Opacidad, 220, 229, 244);
      }
    }

    private const string BASE_FMT = "###,###,###,###,##0";
    public static string FormatoPorSalto(double Minimo, double Maximo)
    {
      double Salto = Math.Abs(Maximo - Minimo) / 5;
      string AA = Salto.ToString("#####0.000");
      AA = AA.Substring(AA.Length - 3);
      if (AA == "000")
      {
        return BASE_FMT;
      }
      else
      {
        if (AA.Substring(2, 1) == "0")
        {
          if (AA.Substring(1, 1) == "0")
          {
            return BASE_FMT + ".0";
          }
          else
          {
            return BASE_FMT + ".00";
          }
        }
        else
        {
          return BASE_FMT + ".000";
        }
      }
    }

    public static string FormatoFechaPorSalto(SaltoEscalaFechas Salto)
    {
      switch (Salto)
      {
        case SaltoEscalaFechas.Anios: return "yyyy";
        case SaltoEscalaFechas.Meses: return "MM/yyyy";
        case SaltoEscalaFechas.Horas: return "dd/MM/yy HH";
        default: return "dd/MM/yy";
      }
    }

    public static string FormatoFechaPorFrecuencia(string Frecuencia)
    {
      switch (Frecuencia)
      {
        case FREC_ANUAL: return "yyyy";
        case FREC_MENSUAL:
        case FREC_MENSUAL_AC:
        case FREC_BIMESTRAL:
        case FREC_TRIMESTRAL:
        case FREC_CUATRIMESTRAL:
        case FREC_SEMESTRAL: return "MM/yyyy";
        case FREC_DIARIA:
        case FREC_SEMANAL: return "dd/MM/yyyy";
        case FREC_HORARIA: return "dd/MM/yyyy HH";
        case FREC_MINUTOS: return "dd/MM/yyyy HH:mm";
        default: return "dd/MM/yyyy";
      }
    }

    public static string ImagenDesdeTendencia(CInformacionAlarmaCN Datos)
    {
      switch (Datos.Color)
      {
        case CRutinas.COLOR_ROJO:
          if (Datos.Valor > Datos.ValorAnterior)
          {
            return "rojo_crec";
          }
          else
          {
            return (Datos.Valor == Datos.ValorAnterior ? "rojo_igu" : "rojo_dec");
          }
        case COLOR_AMARILLO:
          if (Datos.Valor > Datos.ValorAnterior)
          {
            return "am_crec";
          }
          else
          {
            return (Datos.Valor == Datos.ValorAnterior ? "am_igu" : "am_dec");
          }
        case COLOR_VERDE:
          if (Datos.Valor > Datos.ValorAnterior)
          {
            return "ver_crec";
          }
          else
          {
            return (Datos.Valor == Datos.ValorAnterior ? "ver_igu" : "ver_dec");
          }
        case COLOR_AZUL:
          if (Datos.Valor > Datos.ValorAnterior)
          {
            return "azu_crec";
          }
          else
          {
            return (Datos.Valor == Datos.ValorAnterior ? "azu_igu" : "azu_dec");
          }
        default:
          return "am_igu";
      }
    }

    public static string TextoFechaPorSalto(SaltoEscalaFechas Salto)
    {
      switch (Salto)
      {
        case SaltoEscalaFechas.Anios: return "HHHH";
        case SaltoEscalaFechas.Meses: return "HH/HHHH";
        case SaltoEscalaFechas.Horas: return "HH/HH/HH HH";
        default: return "HH/HH/HH";
      }
    }

    public static DateTime DecodificarFecha(string Valor)
    {
      bool bHayDatos = true;
      return DecodificarFecha(Valor, ref bHayDatos);
    }

    public static bool EsFechaCodificada(string Texto)
    {
      const string FechaPattern = @"^[0-9]{14}";
      const string FechaPattern8 = @"^[0-9]{8}";

      return ((Texto.Length == 14 &&
            Regex.IsMatch(Texto, FechaPattern, RegexOptions.IgnoreCase)) ||
            (Texto.Length == 8 && Regex.IsMatch(Texto, FechaPattern8, RegexOptions.IgnoreCase)));
    }

    public static string EnteroLargo(Int32 Valor, Int32 Largo)
    {
      string Resp = Valor.ToString();
      if (Resp.Length < Largo)
      {
        return new string('0', Largo - Resp.Length) + Resp;
      }
      else
      {
        return Resp;
      }
    }

    public static string FormatearFecha(DateTime Fecha)
    {
      if (Fecha.Hour == 0 && Fecha.Minute == 0 && Fecha.Second == 0)
      {
        return EnteroLargo(Fecha.Day, 2) + "/" +
            EnteroLargo(Fecha.Month, 2) + "/" +
            EnteroLargo(Fecha.Year % 100, 2);
      }
      else
      {
        return EnteroLargo(Fecha.Day, 2) + "/" +
            EnteroLargo(Fecha.Month, 2) + "/" +
            EnteroLargo(Fecha.Year % 100, 2) + " " +
            EnteroLargo(Fecha.Hour, 2) + ":" +
            EnteroLargo(Fecha.Minute, 2) + ":" +
            EnteroLargo(Fecha.Second, 2);
      }
    }

    public static string FormatearFecha(double FechaReal)
    {
      DateTime Fecha = DateTime.FromOADate(FechaReal);
      return EnteroLargo(Fecha.Day, 2) + "/" +
          EnteroLargo(Fecha.Month, 2) + "/" +
          EnteroLargo(Fecha.Year % 100, 2);
    }

    public static string FormatoSemana(DateTime Fecha)
    {
      DateTime FechaH = Fecha.AddDays(6);
      return EnteroLargo(Fecha.Day, 2) + "/" +
          EnteroLargo(Fecha.Month, 2) + "-" +
          EnteroLargo(FechaH.Day, 2) + "/" +
          EnteroLargo(FechaH.Month, 2);
    }

    public static string MostrarMensajeError(Exception ex)
    {
      string Msg;
      if (ex.InnerException == null)
      {
        Msg = ex.Message;
      }
      else
      {
        Msg = ex.InnerException.Message;
      }
      return (Msg.Length < 1000 ? Msg : Msg.Substring(0, 1000));
    }

    public static double PonerEnRango(double Angulo)
    {
      while (Angulo < 0)
      {
        Angulo += (2 * Math.PI);
      }
      while (Angulo >= (2 * Math.PI))
      {
        Angulo -= (2 * Math.PI);
      }
      return Angulo;
    }

    public static ModoAgruparIndependiente AgrupamientoSegunRangoDias(Int32 Dias)
    {
      if (Dias <= 31)
      {
        return ModoAgruparIndependiente.Dia;
      }
      else
      {
        if (Dias <= 60)
        {
          return ModoAgruparIndependiente.Semana;
        }
        else
        {
          if (Dias < 732)
          {
            return ModoAgruparIndependiente.Mes;
          }
          else
          {
            return ModoAgruparIndependiente.Anio;
          }
        }
      }
    }

    public static string FloatVStr(double R)
    {
      string Aa = R.ToString();
      return Aa.Replace(
          System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator,
          ".");
    }

    public static double StrVFloat(string Texto)
    {
      try
      {
        if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
        {
          Texto = Texto.Replace(",", ".");
        }
        else
        {
          Texto = Texto.Replace(".", ",");
        }
        return double.Parse(Texto);
      }
      catch (Exception)
      {
        return double.NaN;
      }
    }

    public static Point PuntoIncorrecto()
    {
      return new Point(-1000, -1000);
    }

    private static Point PuntoDesdeCoordenadas(string Valor)
    {
      if (Valor.Contains(" "))
      {
        try
        {
          string[] Valores = Valor.Trim().Split(' ');
          return new Point(CRutinas.StrVFloat(Valores[0]), CRutinas.StrVFloat(Valores[1]));
        }
        catch (Exception)
        {
          //
        }
      }
      return null;

    }

    public static CAreaWFSCN AreaCentroPunto(CCapaWFSCN Capa, Point Punto)
    {
      double DistMin = double.MaxValue;
      CAreaWFSCN Respuesta = null;
      foreach (CAreaWFSCN Area in Capa.Areas)
      {
        double DistArea = (Area.Centro.X - Punto.X) * (Area.Centro.X - Punto.X) +
            (Area.Centro.Y - Punto.Y) * (Area.Centro.Y - Punto.Y);
        if (DistArea < DistMin)
        {
          DistMin = DistArea;
          Respuesta = Area;
        }
      }
      return (DistMin < 0.000001 ? Respuesta : null);
    }

    public static string DeterminarAreaContenedora(Point Punto, CCapaWFSCN CapaWFS, string ValorResto,
        bool PorCodigo = false, bool UsaCentro = false)
    {
      CAreaWFSCN Area = (UsaCentro ? AreaCentroPunto(CapaWFS, Punto) :
          AreaContenedoraPunto(CapaWFS, Punto));
      if (Area == null && UsaCentro)
      {
        Area = AreaContenedoraPunto(CapaWFS, Punto);
      }
      return (Area == null ? ValorResto : (PorCodigo ? Area.Codigo : Area.Nombre));
    }

    public static CPuntoWFSCN PuntoMasCercano(CCapaWFSCN Capa, Point Punto, double Rango)
    {
      double RangoRefe = Rango * 180 / (6378137.0 * Math.PI);
      RangoRefe = RangoRefe * RangoRefe;
      double DistMinima = 1000000000;
      CPuntoWFSCN PuntoMasCercano = null;
      foreach (CPuntoWFSCN PuntoWFS in Capa.Puntos)
      {
        double DistPunto = DistanciaCuadradaEntrePuntos(PuntoWFS.Punto, Punto);
        if (DistPunto < DistMinima)
        {
          DistMinima = DistPunto;
          PuntoMasCercano = PuntoWFS;
        }
      }
      if (DistMinima <= RangoRefe)
      {
        return PuntoMasCercano;
      }
      else
      {
        return null;
      }
    }

    public static string DeterminarPuntoMasCercano(Point Punto, CCapaWFSCN CapaWFS,
        double Rango, string ValorResto, bool PorCodigo = false)
    {
      CPuntoWFSCN PuntoWFS = PuntoMasCercano(CapaWFS, Punto, Rango);
      return (PuntoWFS == null ? ValorResto : (PorCodigo ? PuntoWFS.Codigo : PuntoWFS.Nombre));
    }

    public static string TextoPunto(Point Punto, CCapaWFSCN CapaWFS, double Rango,
        string ValorResto, bool PorCodigo = false, bool UsaCentro = false)
    {
      switch (CapaWFS.Elemento)
      {
        case ElementoWFS.Superficie:
          return DeterminarAreaContenedora(Punto, CapaWFS, ValorResto, PorCodigo, UsaCentro);
        case ElementoWFS.Punto:
          return DeterminarPuntoMasCercano(Punto, CapaWFS, Rango, ValorResto, PorCodigo);
        default:
          return ValorResto;
      }
    }

    public static Point PosicionValor(string Valor, CCapaWFSCN Capa)
    {
      Point PuntoCoordenadas = PuntoDesdeCoordenadas(Valor);
      if (PuntoCoordenadas != null && !double.IsNaN(PuntoCoordenadas.X) && !double.IsNaN(PuntoCoordenadas.Y))
      {
        return PuntoCoordenadas;
      }
      else
      {
        foreach (CAreaWFSCN Area in Capa.Areas)
        {
          if (Area.Codigo.Equals(Valor, StringComparison.OrdinalIgnoreCase))
          {
            return new Point(Area.Centro.X, Area.Centro.Y);
          }
        }

        foreach (CPuntoWFSCN Punto in Capa.Puntos)
        {
          if (Punto.Codigo.Equals(Valor, StringComparison.OrdinalIgnoreCase))
          {
            return new Plantillas.Point(Punto.Punto.X, Punto.Punto.Y);
          }
        }
      }

      return PuntoIncorrecto();

    }

    private static bool TramoIntersectaPorIzq(CPosicionWFSCN P1, CPosicionWFSCN P2, Point Punto)
    {
      if (P1.X > Punto.X && P2.X >= Punto.X)
      {
        return false;
      }
      else
      {
        if ((P1.Y > Punto.Y && P2.Y >= Punto.Y) || (P1.Y < Punto.Y && P2.Y <= Punto.Y))
        {
          return false;
        }
      }
      if (P1.Y == P2.Y)
      {
        return (Punto.Y == P1.Y && P1.X <= Punto.X);
      }
      else
      {
        double AbscRefe = P1.X + (P2.X - P1.X) * (Punto.Y - P1.Y) / (P2.Y - P1.Y);
        return (AbscRefe <= Punto.X);
      }
    }

    public static bool AreaContienePunto(CAreaWFSCN Area, Point Punto)
    {
      // el criterio es hacer una linea horizontal desde el infinito y verificar cuantas veces corta
      // al contorno.
      Int32 Cantidad = 0;
      for (Int32 i = 1; i < Area.Contorno.Count; i++)
      {
        if (TramoIntersectaPorIzq(Area.Contorno[i - 1], Area.Contorno[i], Punto))
        {
          Cantidad++;
        }
      }
      return ((Cantidad % 2) != 0);
    }

    public static CAreaWFSCN AreaContenedoraPunto(CCapaWFSCN Capa, Point Punto)
    {
      foreach (CAreaWFSCN Area in Capa.Areas)
      {
        if (AreaContienePunto(Area, Punto))
        {
          return Area;
        }
      }
      return null;
    }

    public static bool PoligonoContienePunto(List<CPosicionWFSCN> Contorno, Point Punto)
    {
      // el criterio es hacer una linea horizontal desde el infinito y verificar cuantas veces corta
      // al contorno.
      Int32 Cantidad = 0;
      for (Int32 i = 1; i < Contorno.Count; i++)
      {
        if (TramoIntersectaPorIzq(Contorno[i - 1], Contorno[i], Punto))
        {
          Cantidad++;
        }
      }
      return ((Cantidad % 2) != 0);
    }

    public static double DistanciaCuadradaEntrePuntos(CPosicionWFSCN P1, Point P2)
    {
      return (P1.X - P2.X) * (P1.X - P2.X) + (P1.Y - P2.Y) * (P1.Y - P2.Y);
    }

    public static bool PuntoEnCirculos(Point Punto,
        List<CPosicionWFSCN> Contorno, double Distancia2)
    {
      foreach (CPosicionWFSCN Posicion in Contorno)
      {
        if (DistanciaCuadradaEntrePuntos(Posicion, Punto) <= Distancia2)
        {
          return true;
        }
      }
      return false;
    }

    public static List<string> ExtraerListaElementosWFS(CCapaWFSCN CapaWFS, bool PorCodigo = false)
    {
      List<string> Respuesta = new List<string>();
      foreach (CAreaWFSCN Area in CapaWFS.Areas)
      {
        Respuesta.Add(PorCodigo ? Area.Codigo.ToUpper() : Area.Nombre.ToUpper());
      }
      foreach (CPuntoWFSCN Punto in CapaWFS.Puntos)
      {
        Respuesta.Add(PorCodigo ? Punto.Codigo.ToUpper() : Punto.Nombre.ToUpper());
      }
      return Respuesta;
    }

    public static double StrVFloatNaN(string Texto)
    {
      try
      {
        if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
        {
          Texto = Texto.Replace(",", ".");
        }
        else
        {
          Texto = Texto.Replace(".", ",");
        }
        return double.Parse(Texto);
      }
      catch (Exception)
      {
        return double.NaN;
      }
    }

    /// <summary>
    /// Formato: yyyymmddhhnnss
    /// </summary>
    /// <param name="FechaHora"></param>
    /// <returns></returns>
    public static string CodificarFechaHora(DateTime FechaHora)
    {
      if (FechaHora.Year <= 1900)
      {
        return "";
      }
      else
      {
        return EnteroLargo(FechaHora.Year, 4) +
            EnteroLargo(FechaHora.Month, 2) +
            EnteroLargo(FechaHora.Day, 2) +
            EnteroLargo(FechaHora.Hour, 2) +
            EnteroLargo(FechaHora.Minute, 2) +
            EnteroLargo(FechaHora.Second, 2);
      }
    }

    public static DateTime DecodificarFecha(string Valor, ref bool bHayDatos)
    {
      if (Valor.Length > 0)
      {
        try
        {
          bHayDatos = true;
          if (Valor.Length == 8)
          {
            Valor += "000000";
          }
          return new DateTime(
              Int32.Parse(Valor.Substring(0, 4)),
              Int32.Parse(Valor.Substring(4, 2)),
              Int32.Parse(Valor.Substring(6, 2)),
              Int32.Parse(Valor.Substring(8, 2)),
              Int32.Parse(Valor.Substring(10, 2)),
              Int32.Parse(Valor.Substring(12, 2)));
        }
        catch (Exception)
        {
        }
      }
      bHayDatos = false;
      return CRutinas.NoFecha();
    }

    public static DateTime DecodificarFechaHora(string Texto)
    {
      try
      {
        return (Texto.Length == 0 ? NoFecha() :
            new DateTime(Int32.Parse(Texto.Substring(0, 4)),
              Int32.Parse(Texto.Substring(4, 2)),
              Int32.Parse(Texto.Substring(6, 2)),
              Int32.Parse(Texto.Substring(8, 2)),
              Int32.Parse(Texto.Substring(10, 2)),
              Int32.Parse(Texto.Substring(12, 2))));
      }
      catch (Exception)
      {
        DateTime Salida;
        if (DateTime.TryParse(Texto, out Salida))
        {
          return Salida;
        }
        else
        {
          return NoFecha();
        }
      }
    }

    public static DateTime FechaDesdeTexto(string Texto)
    {
      try
      {
        return DateTime.Parse(Texto);
      }
      catch (Exception)
      {
        return new DateTime(1800, 1, 1);
      }
    }

    public static DateTime FechaMaxima()
    {
      return new DateTime(3000, 1, 1);
    }

    public static bool EsFechaMaxima(DateTime Fecha)
    {
      return (Fecha.Year >= 2200);
    }

    public const Int32 ANIO_MINIMO = 1800;
    public static DateTime FechaMinima()
    {
      return new DateTime(ANIO_MINIMO, 1, 1);
    }

    public static DateTime NoFecha()
    {
      return new DateTime(1900, 1, 1);
    }

    //public static bool EstaComprendido(CDetallePreguntaIcono Detalle,
    //      List<CPreguntaPreguntaWISCN> Preguntas)
    //{
    //  if (Detalle == null)
    //  {
    //    return true; // si no hay un elemento seleccionado, usa el menu completo.
    //  }

    //  foreach (CPreguntaPreguntaWISCN Dato in Preguntas)
    //  {
    //    if (Dato.Codigo == Detalle.Detalle.Codigo &&
    //      Dato.Clase == Detalle.Detalle.Clase)
    //    {
    //      return true;
    //    }
    //    else
    //    {
    //      if (Dato.Clase == ClaseDetalle.Pregunta &&
    //        Detalle.Detalle.Clase == ClaseDetalle.Indicador &&
    //        Contenedores.CContenedorDatos.IndicadorEnPregunta(Detalle.Detalle.Codigo, Dato.CodigoPregunta,
    //            Detalle.Detalle.CodigoElementoDimension))
    //      {
    //        return true;
    //      }
    //    }
    //  }
    //  return false;
    //}

    public async static Task<List<CEntradaIndicador>> ListaIndicadoresDimensionAsync(HttpClient Http, Int32 CodigoIndicador, Int32 Dimension,
          Int32 ElementoDimension)
    {
      List<CEntradaIndicador> Respuesta = new List<CEntradaIndicador>();
      List<CEntidadCN> Entidades = await Contenedores.CContenedorDatos.ObtenerElementosDimensionAsync(Http, Dimension,
            ElementoDimension, -1, -1);
      if (Entidades != null)
      {
        foreach (CEntidadCN Entidad in Entidades)
        {
          Respuesta.Add(new CEntradaIndicador(CodigoIndicador, Entidad.Codigo));
        }
      }
      return Respuesta;
    }

    private static bool IndicadorEnLista(List<CEntradaIndicador> Lista,
          Int32 Indicador, Int32 CodElemento)
    {
      foreach (CEntradaIndicador Elemento in Lista)
      {
        if (Elemento.CodigoIndicador == Indicador && Elemento.CodigoElementoDimension == CodElemento)
        {
          return true;
        }
      }
      return false;
    }

    //public static List<CEntradaIndicador> IndicadoresEnContenidos(
    //      List<CPreguntaPreguntaWISCN> Contenidos)
    //{
    //  List<CEntradaIndicador> Respuesta = new List<CEntradaIndicador>();
    //  if (Contenidos != null)
    //  {
    //    foreach (CPreguntaPreguntaWISCN Entrada in Contenidos)
    //    {
    //      if (Entrada.Clase == ClaseDetalle.Indicador)
    //      {
    //        if (Entrada.CodigoDimension >= 0 &&
    //            Entrada.CodigoDimension == Contenedores.CContenedorDatos.DimensionIndicador(Entrada.CodigoElemento))
    //        {
    //          foreach (CEntradaIndicador DatoIndi in ListaIndicadoresDimension(Entrada.CodigoElemento,
    //            Entrada.CodigoDimension, Entrada.CodigoElementoDimension))
    //          {
    //            if (!IndicadorEnLista(Respuesta, DatoIndi.CodigoIndicador, DatoIndi.CodigoElementoDimension))
    //            {
    //              Respuesta.Add(DatoIndi);
    //            }
    //          }
    //        }
    //        else
    //        {
    //          if (!IndicadorEnLista(Respuesta, Entrada.CodigoElemento, Entrada.CodigoElementoDimension))
    //          {
    //            Respuesta.Add(new CEntradaIndicador(Entrada.CodigoElemento, Entrada.CodigoElementoDimension));
    //          }
    //        }
    //      }
    //      else
    //      {
    //        if (Entrada.Clase == ClaseDetalle.Pregunta)
    //        {
    //          List<CEntradaIndicador> ListaLocal =
    //              Contenedores.CContenedorDatos.CodigosIndicadoresPregunta(Entrada.CodigoElemento);
    //          foreach (CEntradaIndicador Elem in ListaLocal)
    //          {
    //            if (Entrada.CodigoDimension < 0 || Elem.CodigoElementoDimension >= 0)
    //            {
    //              if (!IndicadorEnLista(Respuesta, Elem.CodigoIndicador, Elem.CodigoElementoDimension))
    //              {
    //                Respuesta.Add(Elem);
    //              }
    //            }
    //            else
    //            {
    //              foreach (CEntradaIndicador DatoIndi in ListaIndicadoresDimension(Elem.CodigoIndicador,
    //                Entrada.CodigoDimension, Elem.CodigoElementoDimension))
    //              {
    //                if (!IndicadorEnLista(Respuesta, DatoIndi.CodigoIndicador, DatoIndi.CodigoElementoDimension))
    //                {
    //                  Respuesta.Add(DatoIndi);
    //                }
    //              }
    //            }
    //          }
    //        }
    //      }
    //    }
    //  }
    //  return Respuesta;
    //}

    public static ColorBandera DeterminarColorBanderaRelojEstatico(CInformacionAlarmaCN Datos)
    {
      if (double.IsNaN(Datos.Valor)) // || (mbFueraDelPeriodo && ConsideraFueraRango))
      {
        return ColorBandera.SinDatos;
      }
      else
      {
        if (double.IsNaN(Datos.Minimo) || double.IsNaN(Datos.Sobresaliente) || double.IsNaN(Datos.Satisfactorio))
        {
          return ColorBandera.NoCorresponde;
        }
        else
        {
          if (Datos.Minimo==Datos.Sobresaliente)
					{
            return ColorBandera.NoCorresponde;
					}
          if (Datos.Minimo > Datos.Sobresaliente)
          {
            if (Datos.Valor >= Datos.Minimo)
            {
              return ColorBandera.Rojo;
            }
            else
            {
              if (Datos.Valor >= Datos.Satisfactorio)
              {
                return ColorBandera.Amarillo;
              }
              else
              {
                if (Datos.Valor > Datos.Sobresaliente)
                {
                  return ColorBandera.Verde;
                }
                else
                {
                  return ColorBandera.Azul;
                }
              }
            }
          }
          else
          {
            if (Datos.Valor < Datos.Minimo)
            {
              return ColorBandera.Rojo;
            }
            else
            {
              if (Datos.Valor < Datos.Satisfactorio)
              {
                return ColorBandera.Amarillo;
              }
              else
              {
                if (Datos.Valor < Datos.Sobresaliente)
                {
                  return ColorBandera.Verde;
                }
                else
                {
                  return ColorBandera.Azul;
                }
              }
            }
          }
        }
      }
    }

    public async static Task<ColorBandera> ObtenerColorIndicadorAsync(HttpClient Http, Int32 Codigo, Int32 CodigoElementoDimension)
    {
      CInformacionAlarmaCN Datos = await Contenedores.CContenedorDatos.DatosDisponiblesIndicadorFechaAsync(
          Http, Codigo, CodigoElementoDimension);
      if (Datos != null)
      {
        return DeterminarColorBanderaRelojEstatico(Datos);// ColorDesdeTexto(Dato.Color.Trim());
      }
      return ColorBandera.NoCorresponde;
    }

    /// <summary>
    /// Determina el color de una chinche o area caliente, basado en el indicador
    /// seleccionado y las entradas del menu con dimension.
    /// </summary>
    /// <param name="Detalle"></param>
    /// <param name="Contenidos"></param>
    /// <returns></returns>
    //public async static Task<ColorBandera> ObtenerColorBanderaAsync(
    //      CDetallePreguntaIcono Detalle,
    //      List<CPreguntaPreguntaWISCN> Contenidos)
    //{

    //  ColorBandera Respuesta = ColorBandera.SinDatos;

    //  List<CEntradaIndicador> IndicadoresMenu = IndicadoresEnContenidos(Contenidos);
    //  foreach (CEntradaIndicador Elemento in IndicadoresMenu)
    //  {
    //    if (Detalle == null || Elemento.CodigoIndicador == Detalle.Detalle.Codigo)
    //    {
    //      ColorBandera ColorLocal = await ObtenerColorIndicadorAsync(
    //                Elemento.CodigoIndicador, Elemento.CodigoElementoDimension);
    //      if (Respuesta == ColorBandera.SinDatos ||
    //          Respuesta == ColorBandera.Blanco)
    //      {
    //        Respuesta = ColorLocal;
    //      }
    //      switch (ColorLocal)
    //      {
    //        case ColorBandera.Verde:
    //          if (Respuesta == ColorBandera.Azul)
    //          {
    //            Respuesta = ColorBandera.Verde;
    //          }
    //          break;
    //        case ColorBandera.Amarillo:
    //          if (Respuesta == ColorBandera.Azul ||
    //                Respuesta == ColorBandera.Verde)
    //          {
    //            Respuesta = ColorBandera.Amarillo;
    //          }
    //          break;
    //        case ColorBandera.Rojo:
    //          Respuesta = ColorBandera.Rojo;
    //          break;
    //      }
    //    }
    //  }
    //  return Respuesta;
    //}

    public static void AgregarAtributo(System.Xml.Linq.XElement Elemento, string Nombre, string Valor)
    {
      System.Xml.Linq.XAttribute Atributo = new System.Xml.Linq.XAttribute(Nombre, (Valor == null ? "" : Valor));
      Elemento.Add(Atributo);
    }

    public const string CTE_INICIO = "INICIO";
    public const string CTE_ELEMENTO = "ELEMENTO";
    public const string CTE_CLASE_BLOCK = "CLA_ELEM";
    public const string CTE_NIVEL = "NIVEL";
    public const string CTE_NOMBRE = "NOMBRE";
    public const string CTE_CLASE_ELEMENTO = "CLASE_EL";
    public const string CTE_CLASE = "CLASE";
    public const string CTE_CLASE_ORG = "CLASEORG";
    public const string CTE_AGRUPACION_IND = "AGR_IND";
    public const string CTE_AGRUPACION_DEP = "AGR_DEP";
    public const string CTE_COLUMNA_DATOS = "COL_DATOS";
    public const string CTE_COLUMNA_ABSC = "COL_ABSC";
    public const string CTE_COLUMNA_ORD = "COL_ORD";
    public const string CTE_COLUMNA_SEXO = "COL_SEXO";
    public const string CTE_SALTO_HISTOGRAMA = "SALTO";
    public const string CTE_ABSCISA = "ABSC";
    public const string CTE_ORDENADA = "ORD";
    public const string CTE_ALTO = "ALTO";
    public const string CTE_ANCHO = "ANCHO";
    public const string CTE_VISIBLE = "VISIBLE";
    public const string CTE_FILTRO_PROPIO = "F_PROPIO";
    //public const string CTE_INDICADOR = "INDIC";
    public const string CTE_DIMENSION = "DIM";
    public const string CTE_ELEMENTO_DIMENSION = "EL_DIM";
    public const string CTE_FILTRADOR = "FILTRADOR";
    public const string CTE_CONDICION = "CND";
    public const string CTE_CUMPLIR_TODAS = "TODAS";
    public const string CTE_INCLUYE = "INCLY";
    public const string CTE_CANTIDAD = "CNT";
    public const string CTE_FILTRO = "FILTRO";
    public const string CTE_FILTROS = "FILTROS";
    public const string CTE_GRUPO = "GRUPO";
    public const string CTE_CONDICIONES = "CNDS";
    public const string CTE_COLUMNA = "COL";
    public const string CTE_MODO = "MODO";
    public const string CTE_VALOR = "VAL";
    public const string CTE_MAXIMO = "MAX";
    public const string CTE_CODIGO = "CODIGO";
    public const string CTE_CODIGO_2 = "CODIGO_2";
    public const string CTE_CLASE_2 = "CLASE2";
    public const string CTE_SEPARADOR = "$$$$";
    public const string CTE_ES_CAPA_CALOR = "ES_CAPA_CALOR";
    public const string CTE_PARAMETROS = "PRMS";

    public static void AgregarAtributosPosicion(System.Xml.Linq.XElement Elemento, double Abscisa, double Ordenada, double Ancho, double Alto)
    {
      AgregarAtributo(Elemento, CTE_ABSCISA, FloatVStr(Abscisa));
      AgregarAtributo(Elemento, CTE_ORDENADA, FloatVStr(Ordenada));
      AgregarAtributo(Elemento, CTE_ANCHO, FloatVStr(Ancho));
      AgregarAtributo(Elemento, CTE_ALTO, FloatVStr(Alto));
    }

    public static async Task<Logicas.Rectangulo> ObtenerRectanguloElementoAsync(
          Microsoft.JSInterop.IJSRuntime JSRuntime, string Nombre)
    {
      object[] Args = new object[1];
      Args[0] = Nombre;
      string Texto = await JSRuntime.InvokeAsync<string>("FuncionesJS.getRectangulo", Args);
      return new Logicas.Rectangulo(Texto);
    }

    private static string LimpiarParentesis(string Valor)
    {
      return Valor.Substring(1, Valor.Length - 2);
    }

    public static List<CParametroExt> ExtraerPrms(string Contenido)
    {
      List<CParametroExt> Respuesta = new List<CParametroExt>();
      if (Contenido.Length > 0)
      {
        string[] Datos = Contenido.Split(new char[] { '$', '$' }, StringSplitOptions.RemoveEmptyEntries);
        Int32 Cantidad = Int32.Parse(LimpiarParentesis(Datos[0]));
        Int32 Pos = 1;
        for (Int32 i = 0; i < Cantidad; i++)
        {
          CParametroExt Prm = new CParametroExt();
          Prm.CodigoSubconsulta = Int32.Parse(LimpiarParentesis(Datos[Pos++]));
          Prm.Nombre = LimpiarParentesis(Datos[Pos++]);
          Prm.TieneQuery = (LimpiarParentesis(Datos[Pos++]) == "Y");
          Prm.Tipo = LimpiarParentesis(Datos[Pos++]);
          Prm.ValorDateTime = CRutinas.FechaDesdeTexto(LimpiarParentesis(Datos[Pos++]));
          Prm.ValorFloat = CRutinas.StrVFloat(LimpiarParentesis(Datos[Pos++]));
          Prm.ValorInteger = Int32.Parse(LimpiarParentesis(Datos[Pos++]));
          Prm.ValorString = LimpiarParentesis(Datos[Pos++]);
          Respuesta.Add(Prm);
        }
      }
      return Respuesta;
    }

    public static Int32 ExtraerAtributoEntero(System.Xml.Linq.XElement Elemento, string Nombre)
    {
      System.Xml.Linq.XAttribute Atr = Elemento.Attribute(Nombre);
      return (Atr == null ? -1 : Int32.Parse(Atr.Value));
    }

    public static string ExtraerAtributo(System.Xml.Linq.XElement Elemento, string Nombre)
    {
      System.Xml.Linq.XAttribute Atr = Elemento.Attribute(Nombre);
      return (Atr == null ? "" : Atr.Value);
    }

    public static bool ExtraerAtributoBooleano(System.Xml.Linq.XElement Elemento, string Nombre)
    {
      System.Xml.Linq.XAttribute Atr = Elemento.Attribute(Nombre);
      return (Atr == null ? true : Atr.Value != "N");
    }

    public static ModoAgruparIndependiente ModoAgruparPeriodoFechas(DateTime Desde, DateTime Hasta)
    {
      double Delta = Hasta.ToOADate() - Desde.ToOADate();
      if (Delta < 29)
      {
        return ModoAgruparIndependiente.Dia;
      }
      else
      {
        if (Delta < 60)
        {
          return ModoAgruparIndependiente.Semana;
        }
        else
        {
          if (Delta <= 120)
          {
            return ModoAgruparIndependiente.Quincena;
          }
          else
          {
            if (Delta <= 740)
            {
              return ModoAgruparIndependiente.Mes;
            }
            else
            {
              return ModoAgruparIndependiente.Anio;
            }
          }
        }
      }
    }

    public static string MsgMasReciente { get; set; } = "";

    //public static CMapaBingCN CrearMapaBing()
    //{
    //  CMapaBingCN Respuesta = new CMapaBingCN();
    //  Respuesta.AbscisaCentro = -1000;
    //  Respuesta.Capas = new List<CCapaBingCN>();
    //  Respuesta.Codigo = -1;
    //  Respuesta.Descripcion = "";
    //  Respuesta.NivelZoom = 7;
    //  Respuesta.OrdenadaCentro = -1000;
    //  Respuesta.Preguntas = new List<CElementoPreguntasWISCN>();
    //  return Respuesta;
    //}

    public static Int32 ExtraerEntero(string Texto, Int32 PorDefecto)
    {
      try
      {
        return Int32.Parse(Texto.Trim());
      }
      catch (Exception)
      {
        return PorDefecto;
      }
    }

    public static string EnterosALista(List<Int32> Enteros)
    {
      if (Enteros.Count == 0)
      {
        return "-1";
      }
      string Respuesta = Enteros[0].ToString();
      for (Int32 i = 1; i < Enteros.Count; i++)
      {
        Respuesta += ";" + Enteros[i].ToString();
      }
      return Respuesta;
    }

    public static double SaltoMinimo(double Maximo, double Minimo)
    {
      double Referencia = Math.Abs(Math.Max(Maximo, Minimo));
      if (Referencia >= 100)
      {
        return 1;
      }
      else
      {
        if (Referencia >= 10)
        {
          return 0.1;
        }
        else
        {
          return 0.01;
        }
      }
    }

    public static void RedondearExtremos(ref double Minimo, ref double Maximo,
          bool ImponeCero = false)
    {
      if (Maximo <= Minimo)
      {
        Maximo = (Minimo == 0 ? 0.1 : Minimo + 0.005);
      }

      double SaltoAjustado =
        (ImponeCero ?
            BuscarValorEscala((Maximo > 0 ? Maximo : 0) - (Minimo < 0 ? Minimo : 0)) :
            BuscarValorEscala((Maximo - Minimo))) / 5;

      double SaltoMin = SaltoMinimo(Minimo, Maximo);
      if (SaltoMin > SaltoAjustado)
      {
        Minimo -= 2.5 * (SaltoMin - SaltoAjustado);
        SaltoAjustado = SaltoMin;
      }

      double Pos1 = Math.Floor(Minimo / SaltoAjustado);
      double ExtremoMinimo = Pos1 * SaltoAjustado;
      if (Minimo >= 0 && ImponeCero)
      {
        ExtremoMinimo = 0;
      }
      else
      {
        while (ExtremoMinimo >= Minimo)
        {
          ExtremoMinimo -= SaltoAjustado;
        }
      }

      double MaxRefe = ExtremoMinimo + 5 * SaltoAjustado;

      if (ExtremoMinimo < 0 && Minimo >= 0)
      {
        MaxRefe -= ExtremoMinimo;
        ExtremoMinimo = 0;
      }
      else
      {
        if (MaxRefe > 0 && Maximo <= 0)
        {
          ExtremoMinimo -= MaxRefe;
          MaxRefe = 0;
        }
      }

      Minimo = ExtremoMinimo;
      Maximo = MaxRefe;

    }

    //private static CPosicionWFSCN CopiarPuntoCPosicionWFSCN Punto)
    //{
    //  CPosicionWFSCN Respuesta = new CPosicionWFSCN();
    //  Respuesta.X = Punto.X;
    //  Respuesta.Y = Punto.Y;
    //  return Respuesta;
    //}

    //private static List<CPosicionWFSCN> CopiarContorno(List<CPosicionWFSCN> Contorno)
    //{
    //  List<CPosicionWFSCN> Respuesta = new List<CPosicionWFSCN>();
    //  foreach CPosicionWFSCN Punto in Contorno)
    //  {
    //    Respuesta.Add(CopiarPunto(Punto));
    //  }
    //  return Respuesta;
    //}

    //private static CValorDimensionCN CopiarValorDimensionCValorDimensionCN Punto)
    //{
    //  CValorDimensionCN Respuesta = new CValorDimensionCN();
    //  Respuesta.Dimension = Punto.Dimension;
    //  Respuesta.Valor = Punto.Valor;
    //  return Respuesta;
    //}

    //private static List<CValorDimensionCN> CopiarDimensiones(List<CValorDimensionCN> Dimensiones)
    //{
    //  List<CValorDimensionCN> Respuesta = new List<CValorDimensionCN>();
    //  foreach CValorDimensionCN Valor in Dimensiones)
    //  {
    //    Respuesta.Add(CopiarValorDimension(Valor));
    //  }
    //  return Respuesta;
    //}

    //private static CAreaWFSCN CopiarAreaCAreaWFSCN Area)
    //{
    //  CAreaWFSCN Respuesta = new CAreaWFSCN();
    //  Respuesta.Codigo = Area.Codigo;
    //  Respuesta.Area = Area.Area;
    //  Respuesta.Nombre = Area.Nombre;
    //  Respuesta.Centro = CopiarPunto(Area.Centro);
    //  Respuesta.Contorno = CopiarContorno(Area.Contorno);
    //  Respuesta.Dimensiones = CopiarDimensiones(Area.Dimensiones);
    //  return Respuesta;
    //}

    //private static List<CAreaWFSCN> CopiarAreasWFS(List<CAreaWFSCN> Areas)
    //{
    //  List<CAreaWFSCN> Respuesta = new List<CAreaWFSCN>();
    //  if (Areas != null)
    //  {
    //    foreach CAreaWFSCN Area in Areas)
    //    {
    //      Respuesta.Add(CopiarArea(Area));
    //    }
    //  }
    //  return Respuesta;
    //}

    //private static CPuntoWFSCN CopiarPuntoWFSCPuntoWFSCN Punto)
    //{
    //  CPuntoWFSCN Respuesta = new CPuntoWFSCN();
    //  Respuesta.Codigo = Punto.Codigo;
    //  Respuesta.Nombre = Punto.Nombre;
    //  Respuesta.Punto = CopiarPunto(Punto.Punto);
    //  return Respuesta;
    //}

    //private static List<CPuntoWFSCN> CopiarPuntosWFS(List<CPuntoWFSCN> Puntos)
    //{
    //  List<CPuntoWFSCN> Respuesta = new List<CPuntoWFSCN>();
    //  foreach CPuntoWFSCN Punto in Puntos)
    //  {
    //    Respuesta.Add(CopiarPuntoWFS(Punto));
    //  }
    //  return Respuesta;
    //}

    //private static CLineaWFSCN CopiarLineaWFSCLineaWFSCN Linea)
    //{
    //  CLineaWFSCN Respuesta = new CLineaWFSCN();
    //  Respuesta.Codigo = Linea.Codigo;
    //  Respuesta.Nombre = Linea.Nombre;
    //  Respuesta.Centro = CopiarPunto(Linea.Centro);
    //  Respuesta.Contorno = CopiarContorno(Linea.Contorno);
    //  return Respuesta;
    //}

    //private static List<CLineaWFSCN> CopiarLineasWFS(List<CLineaWFSCN> Lineas)
    //{
    //  List<CLineaWFSCN> Respuesta = new List<CLineaWFSCN>();
    //  foreach CLineaWFSCN Linea in Lineas)
    //  {
    //    Respuesta.Add(CopiarLineaWFS(Linea));
    //  }
    //  return Respuesta;
    //}

    //public static CCapaWFSCN CopiarCapaWFSCCapaWFSCN Layer)
    //{
    //  CCapaWFSCN Copiado = new CCapaWFSCN();
    //  Copiado.Codigo = Layer.Codigo;
    //  Copiado.Areas = CopiarAreasWFS(Layer.Areas);
    //  Copiado.Puntos = CopiarPuntosWFS(Layer.Puntos);
    //  Copiado.Descripcion = Layer.Descripcion;
    //  Copiado.Capa = Layer.Capa;
    //  Copiado.CodigoProveedor = Layer.CodigoProveedor;
    //  Copiado.Detalle = Layer.Detalle;
    //  Copiado.DireccionURL = Layer.DireccionURL;
    //  Copiado.Elemento = Layer.Elemento;
    //  Copiado.FechaRefresco = Layer.FechaRefresco;
    //  Copiado.GuardaCompactada = Layer.GuardaCompactada;
    //  Copiado.Lineas = CopiarLineasWFS(Layer.Lineas);
    //  Copiado.NombreCampoCodigo = Layer.NombreCampoCodigo;
    //  Copiado.NombreCampoDatos = Layer.NombreCampoDatos;
    //  Copiado.NombreElemento = Layer.NombreElemento;
    //  Copiado.PuntosMaximosContorno = Layer.PuntosMaximosContorno;
    //  Copiado.Version = Layer.Version;
    //  return Copiado;
    //}

    //public static CCapaWISCN CopiarCapaWISCCapaWISCN Layer)
    //{
    //  CCapaWISCN Copiado = new CCapaWISCN();
    //  Copiado.Codigo = Layer.Codigo;
    //  Copiado.CodigoWFS = Layer.CodigoWFS;
    //  Copiado.Descripcion = Layer.Descripcion;
    //  return Copiado;
    //}

    //public static CElementoPreguntasWISCN CopiarPreguntaCElementoPreguntasWISCN Pregunta)
    //{
    //  CElementoPreguntasWISCN Copiada = new CElementoPreguntasWISCN();
    //  Copiada.Codigo = Pregunta.Codigo;
    //  Copiada.Nombre = Pregunta.Nombre;
    //  Copiada.ClaseWIS = Pregunta.ClaseWIS;
    //  Copiada.CodigoWIS = Pregunta.CodigoWIS;
    //  Copiada.CodigoArea = Pregunta.CodigoArea;
    //  Copiada.Dimension = Pregunta.Dimension;
    //  Copiada.ElementoDimension = Pregunta.ElementoDimension;
    //  Copiada.Abscisa = Pregunta.Abscisa;
    //  Copiada.Ordenada = Pregunta.Ordenada;
    //  Copiada.Contenidos = new List<CPreguntaPreguntaWISCN>();
    //  foreach CPreguntaPreguntaWISCN Elemento in Pregunta.Contenidos)
    //  {
    //    CPreguntaPreguntaWISCN Agregada = new CPreguntaPreguntaWISCN();
    //    Agregada.Codigo = Elemento.Codigo;
    //    Agregada.Clase = Elemento.Clase;
    //    Agregada.CodigoPregunta = Elemento.CodigoPregunta;
    //    Agregada.CodigoDimension = Elemento.CodigoDimension;
    //    Agregada.CodigoElemento = Elemento.CodigoElemento;
    //    Agregada.CodigoElementoDimension = Elemento.CodigoElementoDimension;
    //    Copiada.Contenidos.Add(Agregada);
    //  }
    //  return Copiada;
    //}

    //public static List<CElementoPreguntasWISCN> CopiarPreguntas(List<CElementoPreguntasWISCN> Preguntas)
    //{
    //  List<CElementoPreguntasWISCN> Respuesta = new List<CElementoPreguntasWISCN>();
    //  foreach (CElementoPreguntasWISCN Pregunta in Preguntas)
    //  {
    //    Respuesta.Add(CopiarPregunta(Pregunta));
    //  }
    //  return Respuesta;
    //}

    //public static CCapaWMSCN CopiarCapaWMSCCapaWMSCN Layer)
    //{
    //  CCapaWMSCN Copiado = new CCapaWMSCN();
    //  Copiado.Codigo = Layer.Codigo;
    //  Copiado.Descripcion = Layer.Descripcion;
    //  Copiado.Capa = Layer.Capa;
    //  Copiado.CodigoProveedor = Layer.CodigoProveedor;
    //  Copiado.EPGS = Layer.EPGS;
    //  Copiado.LatMaxima = Layer.LatMaxima;
    //  Copiado.LatMinima = Layer.LatMinima;
    //  Copiado.LongMaxima = Layer.LongMaxima;
    //  Copiado.LongMinima = Layer.LongMinima;
    //  return Copiado;
    //}

  }
}
