using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Text;
using WCFBPI;

namespace TableroPecasV5.Server.Rutinas
{
  public static class CRutinas
  {

    public static string LimitarTexto(string Texto, Int32 LargoMax)
		{
      return (Texto.Length <= LargoMax ? Texto : Texto.Substring(Texto.Length - LargoMax));
		}

    private static BasicHttpBinding ObtenerBND()
    {
      Int32 LimiteMaximo = 1000 * (Int32)Math.Pow(2, 16);
      TimeSpan TiempoEspera = new TimeSpan(0, 15, 0);

      BasicHttpBinding Bnd = new BasicHttpBinding();
      Bnd.MaxBufferSize = LimiteMaximo;
      Bnd.MaxReceivedMessageSize = LimiteMaximo;
      Bnd.MaxBufferPoolSize = LimiteMaximo;
      Bnd.CloseTimeout = TiempoEspera;
      Bnd.OpenTimeout = TiempoEspera;
      Bnd.ReceiveTimeout = TiempoEspera;
      Bnd.SendTimeout = TiempoEspera;
      Bnd.ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas();
      Bnd.ReaderQuotas.MaxArrayLength = LimiteMaximo;
      Bnd.ReaderQuotas.MaxBytesPerRead = LimiteMaximo;
      Bnd.ReaderQuotas.MaxDepth = LimiteMaximo;
      Bnd.ReaderQuotas.MaxNameTableCharCount = LimiteMaximo;
      Bnd.ReaderQuotas.MaxStringContentLength = LimiteMaximo;
      Bnd.Security.Mode = BasicHttpSecurityMode.None;
      Bnd.UseDefaultWebProxy = true;
      //Bnd.MessageEncoding = WSMessageEncoding.Text;
      Bnd.AllowCookies = false;
      Bnd.TextEncoding = Encoding.UTF8;

      return Bnd;

    }

    public static WCFBPIClient ObtenerClienteWCF(string URL)
    {

      string Direccion = URL; // TableroPecasV5.Server.Properties.Resources.URL_WCFBPI;

      EndpointAddress EndP = new EndpointAddress(Direccion);

      return new WCFBPIClient(ObtenerBND(), EndP);

    }

    public static WCFEstructura.WcfEstructuraClient ObtenerClienteWCFEstructura(string URL)
    {

      string Direccion = URL; // TableroPecasV5.Server.Properties.Resources.URL_WCF_EST;

      EndpointAddress EndP = new EndpointAddress(Direccion);

      return new WCFEstructura.WcfEstructuraClient(ObtenerBND(), EndP);

    }

    public static List<Int32> ExtraerListaEnteros(string Lista)
		{
      string[] Valores = Lista.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      List<Int32> Respuesta = new List<int>();
      foreach (string Texto in Valores)
			{
        Respuesta.Add(Int32.Parse(Texto));
			}
      return Respuesta;
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

    public static string CrearImagen(string Prefijo, Int32 Codigo, byte[] Datos, out Int32 Ancho, out Int32 Alto)
    {
      string NombrePropio = Prefijo + "_" + Codigo.ToString() + ".gif";
      string NombreArchivo = System.IO.Path.Combine(TableroPecasV5.Server.Properties.Resources.DIRECTORIO_IMG,
          NombrePropio);
      System.IO.FileInfo FInfo = new System.IO.FileInfo(NombreArchivo);
      if (!FInfo.Exists || FInfo.Length != Datos.Length)
      {
        using (System.IO.FileStream Archivo = new System.IO.FileStream(NombreArchivo, System.IO.FileMode.Create))
        {
          using (System.IO.BinaryWriter Escritor = new System.IO.BinaryWriter(Archivo))
          {
            Escritor.Write(Datos);
          }
        }
      }

      System.Drawing.Image Imagen = System.Drawing.Bitmap.FromFile(NombreArchivo);
      Ancho = Imagen.Width;
      Alto = Imagen.Height;
      return TableroPecasV5.Server.Properties.Resources.PREFIJO_URL_IMG + NombrePropio;

    }

    public static TableroPecasV5.Shared.CMapaBingCN CrearMapaShared(CMapaBingCN Mapa)
		{
      TableroPecasV5.Shared.CMapaBingCN Respuesta = new Shared.CMapaBingCN();
      Respuesta.Autor = Mapa.Autor;
      Respuesta.Capas = new List<Shared.CCapaBingCN>();
      Respuesta.Codigo = Mapa.Codigo;
      Respuesta.Descripcion = Mapa.Descripcion;
      Respuesta.NivelZoom = Mapa.NivelZoom;
      Respuesta.AbscisaCentro = Mapa.AbscisaCentro;
      Respuesta.OrdenadaCentro = Mapa.OrdenadaCentro;
      Respuesta.Preguntas = new List<Shared.CElementoPreguntasWISCN>();
      Respuesta.Publicador = Mapa.Publicador;

      return Respuesta;
		}

    public static TableroPecasV5.Shared.CCapaWSSCN CrearCapaWSSShared(CCapaWSSCN Capa)
    {
      return new Shared.CCapaWSSCN()
      {
        Agrupacion = (Shared.ModoAgruparDependiente)((Int32)Capa.Agrupacion),
        CapaWFS = Capa.CapaWFS,
        Clase = (Shared.ClaseElemento)((Int32)Capa.Clase),
        Codigo = Capa.Codigo,
        CodigoElemento = Capa.CodigoElemento,
        ColorCompuestoA = Capa.ColorCompuestoA,
        ColorCompuestoB = Capa.ColorCompuestoB,
        ColorCompuestoG = Capa.ColorCompuestoG,
        ColorCompuestoR = Capa.ColorCompuestoR,
        ColumnaGeoreferencia = Capa.ColumnaGeoreferencia,
        ColumnaLatitud = Capa.ColumnaLatitud,
        ColumnaLongitud = Capa.ColumnaLongitud,
        ColumnaValor = Capa.ColumnaValor,
        Formula = Capa.Formula,
        Intervalos = (Shared.ClaseIntervalo)((Int32)Capa.Intervalos),
        Minimo = Capa.Minimo,
        Modo = (Shared.ModoGeoreferenciar)((Int32)Capa.Modo),
        Nombre = Capa.Nombre,
        Rango = Capa.Rango,
        Referencias = CopiarVectorDoubles(Capa.Referencias),
        Satisfactorio = Capa.Satisfactorio,
        Segmentos = Capa.Segmentos,
        Sobresaliente = Capa.Sobresaliente,
        Vinculo = Capa.Vinculo
      };
    }

    public static Shared.CCondicionFiltroCN CrearCondicionesFiltroShared(CCondicionFiltroCN Cnd)
    {
      return new Shared.CCondicionFiltroCN()
      {
        BlockCondiciones = Cnd.BlockCondiciones,
        CampoCondicion = Cnd.CampoCondicion,
        CodigoGrafico = Cnd.CodigoGrafico,
        DebeCumplirTodasEnBlock = Cnd.DebeCumplirTodasEnBlock,
        IncluyeALasQueCumplen = Cnd.IncluyeALasQueCumplen,
        ModoDeFiltrar = (Shared.ModoFiltrar)((Int32)Cnd.ModoDeFiltrar),
        OrdenEvaluacion = Cnd.OrdenEvaluacion,
        Paso = Cnd.Paso,
        ValorMaximo = EvitarNaN(Cnd.ValorMaximo),
        ValorMinimo = EvitarNaN(Cnd.ValorMinimo),
        ValorTexto = Cnd.ValorTexto
      };
    }

    public static double EvitarNaN(double Valor)
		{
      return (double.IsNaN(Valor) ? -999 : Valor);
		}

    public static TableroPecasV5.Shared.CGraficoCN CrearGraficoShared(CGraficoCN Graf)
    {
      return new Shared.CGraficoCN()
      {
        Abscisas = Graf.Abscisas,
        AgrupacionAbscisas = (Shared.ModoAgruparIndependiente)((Int32)Graf.AgrupacionAbscisas),
        AgrupacionOrdenadas = (Shared.ModoAgruparDependiente)((Int32)Graf.AgrupacionOrdenadas),
        Agrupar = Graf.Agrupar,
        Alto = Graf.Alto,
        Ancho = Graf.Ancho,
        CampoAbscisas = Graf.CampoAbscisas,
        CampoOrdenadas = Graf.CampoOrdenadas,
        CampoSexo = Graf.CampoSexo,
        CapaWFS = Graf.CapaWFS,
        CapaWFSAgrupadora = Graf.CapaWFSAgrupadora,
        ClaseDeGrafico = (Shared.ClaseGrafico)((Int32)Graf.ClaseDeGrafico),
        CodigoAgrupador = Graf.CodigoAgrupador,
        CodigoElemento = Graf.CodigoElemento,
        //CondicionesFiltro = (from C in Graf.CondicionesFiltro
        //                     select CrearCondicionesFiltroShared(C)).ToList(),
        CondicionesFiltro = new List<Shared.CCondicionFiltroCN>(),
        Dimension = Graf.Dimension,
        MapaColor = Graf.MapaColor,
        Minimo = EvitarNaN(Graf.Minimo),
        Ordenadas = Graf.Ordenadas,
        PasoEdad = Graf.PasoEdad,
        Rango = Graf.Rango,
        Satisfactorio = EvitarNaN(Graf.Satisfactorio),
        Sobresaliente = EvitarNaN(Graf.Sobresaliente),
        ValorSexo1 = Graf.ValorSexo1,
        ValorSexo2 = Graf.ValorSexo2
      };
    }

    public static TableroPecasV5.Shared.CGraficoCompletoCN CrearGraficoCompletoShared(CGraficoCompletoCN Graf)
    {
      return new Shared.CGraficoCompletoCN()
      {
        Adicional = Graf.Adicional,
        CodigoSC = Graf.CodigoSC,
        Descripcion = (Graf.Descripcion.Trim().Length == 0 ? "S/Nombre" : Graf.Descripcion),
        Graficos = (from G in Graf.Graficos
                    select CrearGraficoShared(G)).ToList(),
        Indicador = Graf.Indicador,
        OrdenGraficacion = Graf.OrdenGraficacion,
        ParamSC = Graf.ParamSC,
        Posicionador = (Shared.PosicionadorGIS)((Int32)Graf.Posicionador),
        Vinculo1 = Graf.Vinculo1,
        Vinculo2 = Graf.Vinculo2
      };
    }

    private static string LimpiarTextoRespuesta(string Texto)
		{
      Texto = Texto.Replace("'", "");
      Texto = Texto.Replace('\\', ' ');
      if (Texto.Length > 80)
			{
        return Texto.Substring(0, 80);
			}
      else
			{
        return Texto;
			}
		}

    public static string TextoMsg(Exception ex)
    {
      return LimpiarTextoRespuesta(ex.InnerException == null ? ex.Message : ex.InnerException.ToString());
    }

    public static void CargarDatosDataset(CRespuestaDatasetBin Respuesta,
          TableroPecasV5.Shared.RespuestaDatasetBin RespuestaShared)
		{
      RespuestaShared.Datos = Respuesta.Datos;
      RespuestaShared.ClaseOrigen = (TableroPecasV5.Shared.ClaseElemento)((Int32)Respuesta.ClaseOrigen);
      RespuestaShared.CodigoOrigen = Respuesta.CodigoOrigen;
      RespuestaShared.Periodo = Respuesta.Periodo;
		}

    public static List<Int32> CopiarVectorEnteros(List<Int32> Vector)
		{
      List<Int32> Respuesta = new List<int>();
      if (Vector != null)
      {
        Respuesta.AddRange(Vector);
      }
      return Respuesta;
		}

    public static List<double> CopiarVectorDoubles(List<double> Vector)
    {
      List<double> Respuesta = new List<double>();
      if (Vector != null)
      {
        Respuesta.AddRange(Vector);
      }
      return Respuesta;
    }

    public static List<string> CopiarVectorTextos(List<string> Vector)
    {
      List<string> Respuesta = new List<string>();
      if (Vector != null)
      {
        Respuesta.AddRange(Vector);
      }
      return Respuesta;
    }

    public static TableroPecasV5.Shared.CInformacionAlarmaCN ConvertirInformacionAlarma(CInformacionAlarmaCN Datos)
		{
      TableroPecasV5.Shared.CInformacionAlarmaCN Respuesta = new Shared.CInformacionAlarmaCN()
      {
        CodigoIndicador = Datos.CodigoIndicador,
        Color = Datos.Color,
        DatosParaFecha = Datos.DatosParaFecha,
        Dimension = Datos.Dimension,
        ElementoDimension = Datos.ElementoDimension,
        FechaDesde = Datos.FechaDesde,
        FechaFinal = Datos.FechaFinal,
        FechaHasta = Datos.FechaHasta,
        FechaInicial = Datos.FechaInicial,
        Minimo = Datos.Minimo,
        Periodo = Datos.Periodo,
        Satisfactorio = Datos.Satisfactorio,
        Sentido = Datos.Sentido,
        Sobresaliente = Datos.Sobresaliente,
        Tendencia = Datos.Tendencia,
        Valor = Datos.Valor,
        ValorAnterior = Datos.ValorAnterior,
        Instancias = CopiarVectorEnteros(Datos.Instancias)
      };

      Respuesta.Comentarios = new List<Shared.CComentarioCN>();
      foreach (CComentarioCN Com in Datos.Comentarios)
			{
        Respuesta.Comentarios.Add(new TableroPecasV5.Shared.CComentarioCN()
        {
          Archivos = CopiarVectorTextos(Com.Archivos),
          Clase = (TableroPecasV5.Shared.ClaseComentario)((Int32)Com.Clase),
          ClaseOrigen = (TableroPecasV5.Shared.ClaseElemento)((Int32)Com.ClaseOrigen),
          CodigoOrigen = Com.CodigoOrigen,
          Contenido = Com.Contenido,
          Fecha = Com.Fecha,
          Links = CopiarVectorTextos(Com.Links),
          Orden = Com.Orden,
          Periodo = Com.Periodo,
          Propietario = Com.Propietario,
          SubCodigoOrigen = Com.SubCodigoOrigen,
          Vinculo = Com.Vinculo
        });
			}

      return Respuesta;

    }

    public static DateTime FechaDesdeTexto(string StrFecha)
		{
      Int32 Anio = Int32.Parse(StrFecha.Substring(0, 4));
      Int32 Mes = Int32.Parse(StrFecha.Substring(4, 2));
      Int32 Dia = Int32.Parse(StrFecha.Substring(6, 2));
      Int32 Hora = Int32.Parse(StrFecha.Substring(8, 2));
      Int32 Minutos = Int32.Parse(StrFecha.Substring(10, 2));
      Int32 Seg = Int32.Parse(StrFecha.Substring(12, 2));
      return new DateTime(Anio, Mes, Dia, Hora, Minutos, Seg);
    }

  }
}
