using System;
using System.Net;
using System.Windows;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TableroPecasV5.Client.Plantillas;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Datos
{
  /// <summary>
  /// Contiene las funcionalidades de un dataset. Incluye la lectura desde un array de bytes
  /// hasta el filtrado por teoria de conjuntos.
  /// Permite obtenerlo como unificacion de varios datasets o desde datos no comprimidos.
  /// Para poder trabajar con varios datasets se crea el contenedor de proveedores (CContenedorComprimido)
  /// que tiene a varios de ellos y uno seleccionado.
  /// En ese caso los filtros se iran copiando entre proveedores a medida que se cambie la seleccion.
  /// </summary>
  public class CProveedorComprimido
  {
    public const string PERIODO_DATOS_DATASET = "Fecha_Período_Dataset";
    public const Int32 PERIODO_NO_IMPUESTO = -11111111;

    public event FncAjustarDependientes AlAjustarDependientes;

    private CInformacionAlarmaCN mPeriodo; // es la informacion del periodo del dataset. se usa al subdividirlo.
    private ClaseElemento mClaseOrigen;
    private Int32 mCodigoOrigen;
    private List<CColumnaBase> mColumnas; // las columnas del dataset con sus valores unicos.
    private List<CLineaComprimida> mLineas; // todos los datos del dataset.
    private List<CFiltrador> mFiltros; // para poder trabajar con conjuntos.
//    private FncAjustarDependientes mFncAjustarDependientes;
    private List<CLineaDatos> mLineasWCF; // datos para las grillas unicamente.
    private bool mbSucio;
    private bool mbBlockeado;
    private List<CBlockPlantilla> mCondicionesPrevias; // incluyen graficos que no se usan.
    private List<CCondicionFiltradorCN> mCondicionesPropias; // incluye los filtros propios.
    private List<CGraficoCN> mGraficos; // incluye los graficos cuando se trate de datos de un grafico.
    public ModoAgruparDependiente ModoAgrupar = ModoAgruparDependiente.Acumulado;
    public CProveedorComprimido(ClaseElemento ClaseOrg, Int32 CodigoOrg)
    {
      InicializarDatos();
      mClaseOrigen = ClaseOrg;
      mCodigoOrigen = CodigoOrg;
    }

    public CProveedorComprimido(ClaseElemento ClaseOrg, Int32 CodigoOrg,
          List<CColumnaBase> ColIni, List<Datos.CLineaComprimida> DatosIni)
    {
      InicializarDatos();
      mClaseOrigen = ClaseOrg;
      mCodigoOrigen = CodigoOrg;
      ImponerDataset(ColIni, DatosIni);
      mFiltros = new List<CFiltrador>();
    }

    public void RefrescarDependientes()
    {
      if (AlAjustarDependientes != null)
      {
        AlAjustarDependientes(this);
      }
    }

    public List<CParametroExt> ParametrosSC { get; set; }

    //public void ExtraerFiltrosXML(System.Text.StringBuilder Escritor)
    //{
    //  if (Filtros.Count > 0)
    //  {
    //    foreach (CFiltrador Filtro in Filtros)
    //    {
    //      if (Filtro.ValoresSeleccionados.Count > 0 || (Filtro.ValorMinimo.Length > 0 || Filtro.ValorMaximo.Length > 0))
    //      {
    //        Escritor.AppendLine(CRutinas.FILTRO_REP + Filtro.Columna.Nombre);
    //        if (Filtro.ValoresSeleccionados.Count == 0)
    //        {
    //          Escritor.AppendLine(CRutinas.VALOR_MIN_FILTRO_REP + Filtro.ValorMinimo);
    //          Escritor.AppendLine(CRutinas.VALOR_MAX_FILTRO_REP + Filtro.ValorMaximo);
    //        }
    //        else
    //        {
    //          foreach (string Valor in Filtro.ValoresSeleccionados)
    //          {
    //            Escritor.AppendLine(CRutinas.VALOR_FILTRO_REP + Valor);
    //          }
    //        }
    //        Escritor.AppendLine(CRutinas.CIERRE_REP);
    //      }
    //    }

    //    Escritor.AppendLine(CRutinas.PRM_SC__REP + TextoParametrosSC());

    //  }
    //}

    public void AgregarDatosFaltantesColumna(Int32 OrdenColumna, List<string> DatosTotales)
    {
      CColumnaBase Columna = mColumnas[OrdenColumna];
      Dictionary<string, int> Datos = null;
      switch (Columna.Clase)
      {
        case ClaseVariable.Booleano:
        case ClaseVariable.Texto:
        case ClaseVariable.Fecha:
          Datos = new Dictionary<string, int>();
          foreach (object Dato in Columna.Valores)
          {
            Datos.Add((string)Dato, 0);
          }
          foreach (string Texto in DatosTotales)
          {
            if (!Datos.ContainsKey(Texto))
            {
              Datos.Add(Texto, 0);
            }
          }
          break;
        case ClaseVariable.Entero:
          Datos = new Dictionary<string, int>();
          foreach (object Dato in Columna.Valores)
          {
            Datos.Add(((Int32)Dato).ToString(), 0);
          }
          foreach (string Texto in DatosTotales)
          {
            if (!Datos.ContainsKey(Texto))
            {
              Datos.Add(Texto, 0);
            }
          }
          break;
      }

      // Si es alguna de las clases soportadas:
      if (Datos != null)
      {
        List<string> DatosOrdenados = Datos.Keys.ToList();
        DatosOrdenados.Sort(delegate(string A, string B)
        {
          return A.CompareTo(B);
        });

        // Ahora agrega los valores y ajusta el vector de indices.
        List<Int32> ValorConvertido = new List<int>();
        Int32 PosDatos = 0;
        Int32 PosTotal = 0;
        switch (Columna.Clase)
        {
          case ClaseVariable.Booleano:
          case ClaseVariable.Texto:
          case ClaseVariable.Fecha:
            foreach (string Valor in DatosOrdenados)
            {
              if (PosDatos < Columna.Valores.Count && ((string)Columna.Valores[PosDatos]) == Valor)
              {
                PosDatos++;
                ValorConvertido.Add(PosTotal);
              }
              PosTotal++;
            }
            Columna.Valores.Clear();
            Columna.Valores.AddRange(DatosOrdenados);
            Columna.DatosSucios = true;
            break;
          case ClaseVariable.Entero:
            foreach (string Valor in DatosOrdenados)
            {
              Int32 iValor = Int32.Parse(Valor);
              if (PosDatos < Columna.Valores.Count && ((Int32)Columna.Valores[PosDatos]) == iValor)
              {
                PosDatos++;
                ValorConvertido.Add(PosTotal);
              }
              PosTotal++;
            }
            Columna.Valores.Clear();
            foreach (string Texto in DatosOrdenados)
            {
              Columna.Valores.Add(Int32.Parse(Texto));
            }
            Columna.DatosSucios = true;
            break;
        }

        // Ahora corrige las filas.
        foreach (CLineaComprimida Linea in mLineas)
        {
          Linea.Codigos[OrdenColumna] = ValorConvertido[Linea.Codigos[OrdenColumna]];
        }

      }
    }

    public string TextoParametrosSC()
    {
      string Respuesta = "";
      if (ParametrosSC != null)
      {
        foreach (CParametroExt Prm in ParametrosSC)
        {
          Respuesta += Prm.CodigoSubconsulta.ToString() + "@" + Prm.Tipo + "@" +
            Prm.Nombre + "@"+(Prm.TieneQuery?"S":"N")+"@";
          if (Prm.Tipo.IndexOf("Date", StringComparison.CurrentCultureIgnoreCase) >= 0)
          {
            Respuesta += Prm.ValorDateTime.ToString("yyyyMMddHHmmss");
          }
          else
          {
            if (Prm.Tipo.IndexOf("Integer", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
              Respuesta += Prm.ValorInteger.ToString();
            }
            else
            {
              if (Prm.Tipo.IndexOf("Float", StringComparison.InvariantCultureIgnoreCase) >= 0)
              {
                Respuesta += CRutinas.FloatVStr(Prm.ValorFloat);
              }
              else
              {
                Respuesta += Prm.ValorString;
              }
            }
          }
          Respuesta += "$$";
        }
      }
      return Respuesta;
    }

    public static List<CParametroExt> ExtraerParametrosDesdeTexto(string Texto)
    {
      List<CParametroExt> Respuesta = new List<CParametroExt>();
      if (Texto.IndexOf("$$") >= 0)
      {
        foreach (string Prm in Texto.Split(new char[] { '$', '$' }))
        {
          string[] Campos = Prm.Split('@');
          CParametroExt PrmLocal = new CParametroExt();
          PrmLocal.CodigoSubconsulta = Int32.Parse(Campos[0]);
          PrmLocal.Tipo = Campos[1];
          PrmLocal.Nombre = Campos[2];
          PrmLocal.TieneQuery = (Campos[3] == "S");
          if (Campos[1].IndexOf("Date", StringComparison.CurrentCultureIgnoreCase) >= 0)
          {
            PrmLocal.ValorDateTime = new DateTime(Int32.Parse(Campos[4].Substring(0, 4)),
            Int32.Parse(Campos[4].Substring(4, 2)),
            Int32.Parse(Campos[4].Substring(6, 2)),
            Int32.Parse(Campos[4].Substring(8, 2)),
            Int32.Parse(Campos[4].Substring(10, 2)),
            Int32.Parse(Campos[4].Substring(12, 2)));
          }
          else
          {
            if (Campos[1].IndexOf("Integer", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
              PrmLocal.ValorInteger = Int32.Parse(Campos[4]);
            }
            else
            {
              if (Campos[1].IndexOf("Float", StringComparison.InvariantCultureIgnoreCase) >= 0)
              {
                PrmLocal.ValorFloat = CRutinas.StrVFloat(Campos[4]);
              }
              else
              {
                PrmLocal.ValorString = Campos[4];
              }
            }
          }
          Respuesta.Add(PrmLocal);
        }
      }
      return Respuesta;
    }

    public CProveedorComprimido(CProveedorComprimido Anterior)
    {
      InicializarDatos();
      mClaseOrigen = Anterior.ClaseOrigen;
      mCodigoOrigen = Anterior.CodigoOrigen;
      mPeriodo = Anterior.Periodo;
      mColumnas = Anterior.Columnas;
      mLineas = new List<CLineaComprimida>();
      mbSucio = true;
      mFiltros = new List<CFiltrador>();
      mFiltros.AddRange(Anterior.Filtros);
      //      mFncAjustarDependientes = Anterior.AjustarDependientes;
      AlAjustarDependientes = Anterior.AlAjustarDependientes;
    }

    private static Int32 gOrden = 0;

    public Int32 Orden { get; set; }

    private void InicializarDatos()
    {
      Orden = gOrden++;
      mClaseOrigen = ClaseElemento.NoDefinida;
      mCodigoOrigen = -1;
      mPeriodo = new CInformacionAlarmaCN();
      mPeriodo.Periodo = PERIODO_NO_IMPUESTO;
      mColumnas = new List<CColumnaBase>();
      mLineas = new List<CLineaComprimida>();
      mFiltros = new List<CFiltrador>();
      mbSucio = true;
      mbBlockeado = false;
      mCondicionesPrevias = new List<CBlockPlantilla>();
      mCondicionesPropias = new List<CCondicionFiltradorCN>();
      mGraficos = new List<CGraficoCN>();
    }

    private byte ConvertirCaracter(string Chr)
    {
      switch (Chr)
      {
        case "Ñ": return 165;
        case "ñ": return 164;
        case "á": return 160;
        case "é": return 130;
        case "í": return 161;
        case "ó": return 162;
        case "ú": return 163;
        default:
          return (byte)Chr[0];
      }
    }

    private void EscribirTextoUTF8(BinaryWriter Escritor, string Texto)
    {
      byte[] DatosBinarios; // = new byte[Texto.Length];
      //for (Int32 i = 0; i < Texto.Length; i++)
      //{
      DatosBinarios = System.Text.Encoding.UTF8.GetBytes(Texto); // ConvertirCaracter(Texto.Substring(i, 1));
      //}
      //      byte[] DatosBinarios = new System.Text.Encoding(); // new UTF8Encoding().GetBytes(Texto);
      Int32 Largo = DatosBinarios.Length;
      if (Largo == 0)
      {
        byte Aux = (byte)Largo;
        Escritor.Write(Aux);
        return;
      }
      else
      {
        while (Largo > 0)
        {
          byte SubLargo = (byte)(Largo % 128);
          Largo = (Largo - (Int32)SubLargo) / 128;
          if (Largo > 0)
          {
            SubLargo += 128;
          }
          Escritor.Write(SubLargo);
        }
      }

      Escritor.Write(DatosBinarios);

    }
    private void EscribirColumna(BinaryWriter Escritor, CColumnaBase Columna, bool ParaDelphi)
    {
      Escritor.Write((Int32)Columna.Clase);
      Escritor.Write(Columna.Orden);
      if (ParaDelphi)
      {
        EscribirTextoUTF8(Escritor, Columna.Nombre);
      }
      else
      {
        Escritor.Write(Columna.Nombre);
      }
      Escritor.Write((Int32)Columna.Valores.Count);
      switch (Columna.Clase)
      {
        case ClaseVariable.Entero:
          foreach (Int32 Valor in Columna.Valores)
          {
            Escritor.Write(Valor);
          }
          break;
        case ClaseVariable.Real:
          foreach (double Valor in Columna.Valores)
          {
            Escritor.Write(Valor);
          }
          break;
        default:
          foreach (string Valor in Columna.Valores)
          {
            if (ParaDelphi)
            {
              EscribirTextoUTF8(Escritor, Valor);
            }
            else
            {
              Escritor.Write(Valor);
            }
          }
          break;
      }
    }

    public byte[] CrearDatosBinarios(bool ParaDelphi)
    {
      using (MemoryStream Archivo = new MemoryStream())
      {
        using (BinaryWriter Escritor = new BinaryWriter(Archivo))
        {
          Escritor.Write((Int32)mColumnas.Count);
          foreach (CColumnaBase Columna in mColumnas)
          {
            EscribirColumna(Escritor, Columna, ParaDelphi);
          }

          Escritor.Write((Int32)mLineas.Count);
          foreach (CLineaComprimida Linea in mLineas)
          {
            foreach (Int32 Codigo in Linea.Codigos)
            {
              Escritor.Write(Codigo);
            }
          }

          byte[] Respuesta = new byte[Archivo.Length];
          Archivo.Seek(0, SeekOrigin.Begin);
          Archivo.Read(Respuesta, 0, (int)Archivo.Length);

          //          CProveedorComprimido Prueba = new CProveedorComprimido(ClaseElemento.NoDefinida, -1);
          //          Prueba.CargarDatosDesdeBinario(Respuesta);
          return Respuesta;
        }

      }
    }
    public void CopiarLineasVigentes(List<CLineaComprimida> Lineas)
    {
      Datos = new List<Datos.CLineaComprimida>();
      foreach (Datos.CLineaComprimida Lin0 in Lineas)
      {
        if (Lin0.Vigente)
        {
          Datos.Add(Lin0.CopiarLinea());
        }
      }
    }

    public Int32 PosicionColumna(string Nombre)
    {
      Datos.CColumnaBase Columna = ColumnaNombre(Nombre);
      if (Columna == null)
      {
        CRutinas.MsgMasReciente = "No encuentra columna <" + Nombre + ">";
        return -1;
      }
      else
      {
        return Columna.Orden;
      }
    }

    public void EliminarColumna(string Nombre)
    {
      CColumnaBase Columna = ColumnaNombre(Nombre);
      if (Columna != null)
      {
        mColumnas.Remove(Columna);
        for (Int32 i = 0; i < mColumnas.Count; i++)
        {
          mColumnas[i].Orden = i;
        }

        foreach (CLineaComprimida Linea in Datos)
        {
          if (Linea.Codigos.Count > Columna.Orden)
          {
            Linea.Codigos.RemoveAt(Columna.Orden);
          }
        }
      }
    }

    public DateTime PrimeraFechaValida(Int32 OrdenAbscisa)
    {
      return mColumnas[OrdenAbscisa].PrimeraFechaValida();
    }

    public ModoAgruparIndependiente ObtenerAgrupacionAbscisaFechas(Int32 OrdenAbscisa)
    {
      if (Datos.Count > 1)
      {
        return CRutinas.ModoAgruparPeriodoFechas(
            PrimeraFechaValida(OrdenAbscisa),
            ObtenerFechaLinea(Datos[Datos.Count - 1], OrdenAbscisa));
      }
      else
      {
        return ModoAgruparIndependiente.Dia;
      }
    }

    public void CopiarCondicionesPrevias(CProveedorComprimido Anterior)
    {
      mCondicionesPrevias.AddRange(Anterior.mCondicionesPrevias);
      mCondicionesPrevias.Add(Anterior.CrearBlockPlantilla());
    }

    //public double ExtraerValorReal(CLineaComprimida Linea, Int32 Columna)
    //{
    //  if (Columna < 0 || Columna >= mColumnas.Count)
    //  {
    //    return double.NaN;
    //  }

    //  Int32 Posicion = Linea.Codigos[Columna];
    //  if (Posicion < 0 || Posicion >= mColumnas[Columna].Valores.Count)
    //  {
    //    return double.NaN;
    //  }

    //  switch (mColumnas[Columna].Clase)
    //  {
    //    case ClaseVariable.Real:
    //      return (double)mColumnas[Columna].Valores[Posicion];
    //    case ClaseVariable.Entero:
    //      return (Int32)mColumnas[Columna].Valores[Posicion];
    //    default: return double.NaN;
    //  }
    //}

    public CBlockPlantilla CrearBlockPlantilla()
    {
      CBlockPlantilla Respuesta = new CBlockPlantilla();
      Respuesta.Nombre = "qwqw"; // CCcontenedorDatos.ContenedorUnico.NombreReferencia(null, mClaseOrigen, mCodigoOrigen, -1, "");
      Respuesta.FraccionAbscisas = 0;
      Respuesta.FraccionAncho = 0;
      Respuesta.FraccionOrdenadas = 0;
      Respuesta.LatMaxima = 0;
      Respuesta.LatMinima = 0;
      Respuesta.LngMaxima = 0;
      Respuesta.LngMinima = 0;
      Respuesta.Pasos = new List<CPasoGraficoCN>();
      CPasoGraficoCN Paso = new CPasoGraficoCN();
      Paso.Graficos = new List<CGraficoCN>();
      Paso.Graficos.AddRange(mGraficos);
      Paso.CondicionesFiltrador = CondicionesPropias();
      Respuesta.Pasos.Add(Paso);
      return Respuesta;
    }

    public ClaseElemento ClaseOrigen
    {
      get { return mClaseOrigen; }
      set { mClaseOrigen = value; }
    }

    public Int32 CodigoOrigen
    {
      get { return mCodigoOrigen; }
      set { mCodigoOrigen = value; }
    }

    public bool BlockearFiltrado
    {
      get { return mbBlockeado; }
      set { mbBlockeado = value; }
    }

    public CInformacionAlarmaCN Periodo
    {
      get { return mPeriodo; }
      set { mPeriodo = value; }
    }

    public List<CGraficoCN> GraficosOrigenDatos
    {
      get { return mGraficos; }
      set { mGraficos = value; }
    }

    public List<CFiltrador> Filtros
    {
      get { return mFiltros; }
      set { mFiltros = value; }
    }

    public bool DatosSucios
    {
      get { return mbSucio; }
      set { mbSucio = value; }
    }

    //public FncAjustarDependientes AjustarDependientes
    //{
    //  get { return mFncAjustarDependientes; }
    //  set { mFncAjustarDependientes = value; }
    //}

    public List<CLineaDatos> LineasWCF
    {
      get
      {
        if (mbSucio)
        {
          ArmarLineas();
        }
        return mLineasWCF;
      }
      set
      {
        mLineasWCF = value;
      }
    }

    public static List<Datos.CLineaComprimida> CopiarLineas(List<Datos.CLineaComprimida> LinIni)
    {
      List<Datos.CLineaComprimida> Respuesta = new List<CLineaComprimida>();
      foreach (Datos.CLineaComprimida Linea in LinIni)
      {
        CLineaComprimida LinN = new CLineaComprimida();
        LinN.Codigos = new List<int>();
        LinN.Codigos.AddRange(Linea.Codigos);
        Respuesta.Add(LinN);
      }
      return Respuesta;
    }

    public DateTime ObtenerFechaLinea(CLineaComprimida Linea, Int32 Orden)
    {
      if (Orden < 0 || Orden >= mColumnas.Count ||
        mColumnas[Orden].Clase != ClaseVariable.Fecha ||
        Linea.Codigos[Orden] < 0)
      {
        return CRutinas.FechaMinima();
      }
      else
      {
        return CRutinas.DecodificarFecha(Linea.Codigos[Orden]<0?"":(string)mColumnas[Orden].Valores[Linea.Codigos[Orden]]);
      }
    }

    public List<string> DistintosValoresColumnaVigentes(string NombreColumna, bool Mayusculas = false)
    {
      List<string> Respuesta = new List<string>();
      CColumnaBase ColDatos = ColumnaNombre(NombreColumna);
      if (ColDatos != null)
      {
        CFiltrador Filtro = ObtenerFiltroColumna(NombreColumna, false);
        if (Filtro != null)
        {
          foreach (CElementoFilaAsociativa Fila in Filtro.Filas)
          {
            if (Fila.Vigente)
            {
              Respuesta.Add(Fila.Nombre);
            }
          }
        }
        else
        {
          List<CElementoFilaAsociativa> FilasLocales = new List<CElementoFilaAsociativa>();
          foreach (string Valor in ColDatos.ListaValores)
          {
            CElementoFilaAsociativa Elem = new CElementoFilaAsociativa();
            Elem.Cantidad = 0;
            Elem.Nombre = Valor;
            FilasLocales.Add(Elem);
          }
          foreach (CLineaComprimida Linea in DatosVigentes)
          {
            FilasLocales[Linea.Codigos[ColDatos.Orden]].Cantidad = 1;
          }
          foreach (CElementoFilaAsociativa Elem in FilasLocales)
          {
            if (Elem.Cantidad == 1)
            {
              Respuesta.Add((Mayusculas ? Elem.Nombre.ToUpper() : Elem.Nombre));
            }
          }
        }
      }
      return Respuesta;
    }

    public List<string> DistintosValoresColumna(string NombreColumna)
    {
      List<string> Respuesta = new List<string>();
      CColumnaBase ColDatos = ColumnaNombre(NombreColumna);
      if (ColDatos != null)
      {
        CFiltrador Filtro = ObtenerFiltroColumna(NombreColumna, false);
        if (Filtro != null)
        {
          foreach (CElementoFilaAsociativa Fila in Filtro.Filas)
          {
            if (Fila.Vigente)
            {
              Respuesta.Add(Fila.Nombre);
            }
          }
        }
        else
        {
          Respuesta.AddRange(ColDatos.ListaValores);
        }
      }
      return Respuesta;
    }

    public double ObtenerValorRealLinea(CLineaComprimida Linea, Int32 Orden)
    {
      if (Orden < 0 || Orden >= mColumnas.Count)
      {
        return 0;
      }
      else
      {
        if (mColumnas[Orden].Clase != ClaseVariable.Real &&
         mColumnas[Orden].Clase != ClaseVariable.Entero)
        {
          return 1;
        }
        else
        {
          return mColumnas[Orden].ValorReal(Linea.Codigos[Orden]);
        }
      }
    }

    public Int32 ObtenerValorEnteroLinea(CLineaComprimida Linea, Int32 Orden)
    {
      if (Orden < 0 || Orden >= mColumnas.Count || mColumnas[Orden].Clase != ClaseVariable.Fecha)
      {
        return 0;
      }
      else
      {
        return (Int32)mColumnas[Orden].Valores[Linea.Codigos[Orden]];
      }
    }

    public string ObtenerTextoLinea(CLineaComprimida Linea, Int32 Orden)
    {
      if (Orden < 0 || Orden >= mColumnas.Count || mColumnas[Orden].Clase != ClaseVariable.Fecha)
      {
        return "";
      }
      else
      {
        return (string)mColumnas[Orden].ListaValores[Linea.Codigos[Orden]];
      }
    }

    /// <summary>
    /// Crea una lista con los datos de cada fila, despues de filtrarla por las asociaciones.
    /// </summary>
    private void ArmarLineas()
    {
      mbSucio = false;
      // antes de armar los datos, habria que ajustar la vigencia.
      mLineasWCF = new List<CLineaDatos>();
      if (mLineas == null)
      {
        mLineas = new List<CLineaComprimida>();
      }
      foreach (CLineaComprimida LineaLocal in mLineas)
      {
        if (LineaLocal.Vigente)
        {
          CLineaDatos Linea = new CLineaDatos();
          Linea.Contenidos = new List<string>();
          for (Int32 i = 0; i < LineaLocal.Codigos.Count; i++)
          {
            Linea.Contenidos.Add(Columnas[i].ListaValores[LineaLocal.Codigos[i]]);
          }
          mLineasWCF.Add(Linea);
        }
      }
    }

    public void OrdenarCodigos()
    {
      foreach (CColumnaBase Columna in mColumnas)
      {
        if (Columna.Codigos.Count > 0)
        {
          List<CParEnteros> Codigos = Columna.CodigosTransformados();
          Int32 Posicion = Columna.Orden;
          foreach (CLineaComprimida Linea in mLineas)
          {
            Linea.Codigos[Posicion] = Codigos[Linea.Codigos[Posicion]].Codigo2;
          }
        }
      }
    }

    public bool HayFiltros
    {
      get
      {
        foreach (CFiltrador Filtro in mFiltros)
        {
          if (Filtro.HayCondicion)
          {
            return true;
          }
        }
        return false;
      }
    }

    /// <summary>
    /// Copia los filtros de un proveedor a otro y limpia los filtros del proveedor anterior.
    /// No se pueden copiar dentro del mismo proveedor, porque se eliminan.
    /// </summary>
    /// <param name="Anterior"></param>
    public void CopiarFiltros(CProveedorComprimido Anterior)
    {
      if (Anterior != this)
      {
        AlAjustarDependientes = Anterior.AlAjustarDependientes;

        mFiltros.Clear();
        foreach (CFiltrador Filtro in Anterior.Filtros)
        {
          Filtro.Proveedor = this;
          Filtro.Columna = mColumnas[Filtro.Columna.Orden];
          Filtro.AjustarInformacionFilas();
          mFiltros.Add(Filtro);
        }

        foreach (CFiltrador Filtro in mFiltros)
        {
          Filtro.RefrescarAsociacionesAlCambiarProveedor();
          Filtro.AjustarAnchoPantallaContenedor();
        }

        // ajusta los campos asociados a un filtro seleccionado.
        Anterior.Filtros.Clear();

      }

    }

    //public List<CColumnaEntero> ColumnasEnteras()
    //{
    //  List<CColumnaEntero> Respuesta = new List<CColumnaEntero>();
    //  foreach (CColumnaBase Columna in mColumnas)
    //  {
    //    if (Columna is CColumnaEntero)
    //    {
    //      Respuesta.Add((CColumnaEntero)Columna);
    //    }
    //  }
    //  return Respuesta;
    //}

    //public List<CColumnaReal> ColumnasReales()
    //{
    //  List<CColumnaReal> Respuesta = new List<CColumnaReal>();
    //  foreach (CColumnaBase Columna in mColumnas)
    //  {
    //    if (Columna is CColumnaReal)
    //    {
    //      Respuesta.Add((CColumnaReal)Columna);
    //    }
    //  }
    //  return Respuesta;
    //}

    //public List<CColumnaTexto> ColumnasTexto()
    //{
    //  List<CColumnaTexto> Respuesta = new List<CColumnaTexto>();
    //  foreach (CColumnaBase Columna in mColumnas)
    //  {
    //    if (Columna is CColumnaTexto)
    //    {
    //      Respuesta.Add((CColumnaTexto)Columna);
    //    }
    //  }
    //  return Respuesta;
    //}

    public List<CColumnaBase> Columnas
    {
      get { return mColumnas; }
      set { mColumnas = value; }
    }

    public List<CLineaComprimida> Datos
    {
      get { return mLineas; }
      set { mLineas = value; }
    }

    public List<CLineaComprimida> DatosVigentes
    {
      get { return (from CLineaComprimida A in mLineas where A.Vigente select A).ToList(); }
    }

    private void LeerColumna(BinaryReader Lector)
    {
      CColumnaBase Columna;
      ClaseVariable ClaseLocal = (ClaseVariable)Lector.ReadInt32();
      switch (ClaseLocal)
      {
        case ClaseVariable.Entero:
          Columna = new CColumnaEntero();
          break;
        case ClaseVariable.Real:
          Columna = new CColumnaReal();
          break;
        default:
          Columna = new CColumnaTexto();
          Columna.Clase = ClaseLocal;
          break;
      }
      Columna.DatosSucios = true;
      Columna.Orden = Lector.ReadInt32();
      Columna.Nombre = Lector.ReadString();
      Columna.CargarValores(Lector, ClaseLocal);
      //if (Columna.Clase == ClaseVariable.Texto && Columna.Valores.Count > 0)
      //{
      //  Columna.Valores[Columna.Valores.Count - 1] = "ZZZZZZZZ";
      //}
      mColumnas.Add(Columna);
    }

    private CLineaComprimida LeerLinea(BinaryReader Lector, Int32 CantCol)
    {
      CLineaComprimida Respuesta = new CLineaComprimida();
      for (Int32 i = 0; i < CantCol; i++)
      {
        Respuesta.Codigos.Add(Lector.ReadInt32());
      }
      return Respuesta;
    }

    private void CargarDatosDesdeBinario(byte[] Dataset, bool UnicamenteColumnas = false)
    {
      mColumnas = new List<CColumnaBase>();
      mLineas = new List<CLineaComprimida>();
      using (MemoryStream Archivo = new MemoryStream(Dataset))
      {
        using (BinaryReader Lector = new BinaryReader(Archivo))
        {
          Int32 Cantidad = Lector.ReadInt32();
          for (Int32 i = 0; i < Cantidad; i++)
          {
            LeerColumna(Lector);
          }

          Int32 CantidadLineas = Lector.ReadInt32();
          for (Int32 i = 0; i < CantidadLineas; i++)
          {
            mLineas.Add(LeerLinea(Lector, Cantidad));
          }
        }
      }
    }

    private void CargarDatosDesdeBinarioUnicode(byte[] Dataset, bool UnicamenteColumnas = false)
    {
      mColumnas = new List<CColumnaBase>();
      mLineas = new List<CLineaComprimida>();
      using (MemoryStream Archivo = new MemoryStream(Dataset))
      {
        using (BinaryReader Lector = new BinaryReader(Archivo, System.Text.Encoding.Unicode))
        {
          Int32 Cantidad = Lector.ReadInt32();
          for (Int32 i = 0; i < Cantidad; i++)
          {
            LeerColumna(Lector);
          }

          Int32 CantidadLineas = Lector.ReadInt32();
          for (Int32 i = 0; i < CantidadLineas; i++)
          {
            mLineas.Add(LeerLinea(Lector, Cantidad));
          }
        }
      }
    }

    //private void CargarSinonimos(List<CSinonimoCompletoCN> Sinonimos)
    //{
    //  foreach (CSinonimoCompletoCN Sinonimo in Sinonimos)
    //  {
    //    foreach (CColumnaBase Columna in mColumnas)
    //    {
    //      if (Columna is CColumnaTexto && Columna.Nombre == Sinonimo.Columna.ColumnaAsociada)
    //      {
    //        Columna.Sinonimos.Add(Sinonimo);
    //      }
    //    }
    //  }
    //}

    /// <summary>
    /// Lee el binario del dataset y lo carga.
    /// Los datos del dataset vienen en el siguiente formato:
    /// Cantidad columnas (Int32).
    /// Para cada columna:
    ///   Clase:       Int32 (Int32: 1, Int16: 2, Byte: 3, Real: 4, Doble: 5, Fecha: 6, Texto: 7, Bool: 8).
    ///   LargoNombre: Int32.
    ///   Nombre:      char[].
    ///   CantValores: Int32.
    ///   Valores Int32,Int16,Byte,Float,double,double,Int32-Char[],Char (Y/N),
    /// Cantidad Lineas: Int32.
    /// Para cada linea:
    ///   Posicion[Cantidad columnas]: Int32.
    ///   
    /// </summary>
    /// <param name="DatosDataset"></param>
    /// <param name="UnicamenteColumnas"></param>
    public void ProcesarDatasetBinario(byte[] DatosDataset,
          bool UnicamenteColumnas)
    {

      try
      {
        if (DatosDataset != null)
        {
          CargarDatosDesdeBinario(DatosDataset, UnicamenteColumnas);
//          CargarSinonimos(Sinonimos);
        }
      }
      catch (Exception)
      {
        return;
      }
    }

    public void InformarDependientes()
    {
      AlAjustarDependientes?.Invoke(this);
    }

    public void ProcesarDatasetBinarioUnicode(byte[] DatosDataset,
          bool UnicamenteColumnas)
    {

      try
      {
        if (DatosDataset != null)
        {
          CargarDatosDesdeBinarioUnicode(DatosDataset, UnicamenteColumnas);
//          CargarSinonimos(Sinonimos);
        }
      }
      catch (Exception)
      {
        return;
      }
    }

    public void EliminarEspacios()
    {
      foreach (CColumnaBase Columna in mColumnas)
      {
        if (Columna is CColumnaTexto && Columna.Clase == ClaseVariable.Texto)
        {
          ((CColumnaTexto)Columna).EliminarEspacios();
        }
      }
    }

    private Int32 DeterminarPosicionCampo(string Nombre, List<string> Columnas)
    {
      for (Int32 i = 0; i < Columnas.Count; i++)
      {
        Int32 ii = Columnas[i].IndexOf("=");
        if (Columnas[i].Substring(0, ii) == Nombre)
        {
          return i;
        }
      }
      return -1;
    }

    private static CColumnaBase CrearColumnaClase(ClaseVariable ClaseV)
    {
      switch (ClaseV)
      {
        case ClaseVariable.Real: return new CColumnaReal();
        case ClaseVariable.Entero: return new CColumnaEntero();
        default: return new CColumnaTexto();
      }
    }

    //public static List<CColumnaBase> CopiarColumnas(
    //      List<CColumnaBase> ColumnasOriginales)
    //{
    //  List<CColumnaBase> Respuesta = new List<CColumnaBase>();
    //  foreach (CColumnaBase Col0 in ColumnasOriginales)
    //  {
    //    CColumnaBase Columna = CrearColumnaClase(Col0.Datos.Clase);
    //    Columna.Clase = Col0.Clase;
    //    Columna.Orden = Col0.Orden;
    //    Columna.Nombre = Col0.Nombre;
    //    Columna.Valores = new List<object>();
    //    Respuesta.Add(Columna);
    //  }
    //  return Respuesta;
    //}

    private Int32 InsertarEntero(CColumnaBase Columna, string Texto)
    {
      return Columna.AgregarValor(CRutinas.ExtraerEntero(Texto, -1));
    }

    private Int32 InsertarReal(CColumnaBase Columna, string Texto)
    {
      double Valor;
      try
      {
        Valor = double.Parse(Texto);
      }
      catch (Exception)
      {
        Valor = -1;
      }
      return Columna.AgregarValor(Valor);
    }

    private Int32 InsertarTexto(CColumnaBase Columna, string Texto)
    {
      return Columna.AgregarValor(Texto);
    }

    private Int32 InsertarDatosColumna(CColumnaBase Columna, string Texto)
    {
      switch (Columna.Clase)
      {
        case ClaseVariable.Entero:
          return InsertarEntero(Columna, Texto);
        case ClaseVariable.Real:
          return InsertarReal(Columna, Texto);
        default:
          return InsertarTexto(Columna, Texto);
      }
    }

    /// <summary>
    /// Indica cuando la columna de un filtro es la columna valor de algun otro filtro.
    /// </summary>
    /// <param name="FiltroRefe"></param>
    /// <returns></returns>
    public bool EsColumnaValor(CFiltrador FiltroRefe)
    {
      foreach (CFiltrador Filtro in mFiltros)
      {
        if (Filtro.ColumnaValor == FiltroRefe.Columna)
        {
          return true;
        }
      }
      return false;
    }

    public Int32 OrdenColumnaNombre(string Nombre)
    {
      CColumnaBase Col = ColumnaNombre(Nombre);
      return (Col == null ? -1 : Col.Orden);
    }

    public CColumnaBase ColumnaNombre(string Nombre)
    {
      foreach (CColumnaBase Col0 in mColumnas)
      {
        if (Col0.Nombre.Equals(Nombre,StringComparison.OrdinalIgnoreCase))
        {
          return Col0;
        }
      }
      return null;
    }

    public void CorregirCodigosColumna(Int32 Pos)
    {
      List<CParEnteros> CodigosLocales = mColumnas[Pos].CodigosTransformados();
      Int32 Posicion = mColumnas[Pos].Orden;
      if (mLineas != null)
      {
        foreach (CLineaComprimida Linea in mLineas)
        {
          Linea.Codigos[Posicion] = CodigosLocales[Linea.Codigos[Posicion]].Codigo2;
        }
      }
      mColumnas[Pos].DatosSucios = true;
    }

    private void CorregirCodigosOrdenandolos()
    {
      for (Int32 i = 0; i < mColumnas.Count; i++)
      {
        CorregirCodigosColumna(i);
      }
    }

    /// <summary>
    /// Se usa cuando se procesan varios periodos, por ejemplo en comparacion o curvas de puntos en tendencias.
    /// </summary>
    /// <param name="Otro"></param>
    private void CopiarFiltrosDesdeProveedorPeriodo(CProveedorComprimido Otro)
    {

      mFiltros.Clear();

      foreach (CFiltrador Filtro in Otro.Filtros)
      {
        CFiltrador Agregado =
              new CFiltrador(Filtro, this);
        Agregado.Proveedor = this;
        mFiltros.Add(Agregado);
      }

    }

    private void InicializarSeleccionesFiltros()
    {
      foreach (CFiltrador Filtro in mFiltros)
      {
        Filtro.InicializarSelecciones();
      }
    }

    public void FiltrarDesdeFiltrosImpuestos(List<CFiltrador> Filtradores)
    {
      // poner todas vigentes.
      foreach (CLineaComprimida Linea in mLineas)
      {
        Linea.Vigente = true;
      }

      // Ubicar listas de valores.
      foreach (CFiltrador Filtrador in Filtradores)
      {
        Int32 PosCol = Filtrador.Columna.Orden;
        if (Filtrador.EvaluarRangos)
        {
          Int32 Pos1 = Filtrador.Columna.PosicionValorMayorIgual(CRutinas.StrVFloat(Filtrador.ValorMinimo));
          Int32 Pos2 = Filtrador.Columna.PosicionValorMayorIgual(CRutinas.StrVFloat(Filtrador.ValorMaximo));
          if ((double)Filtrador.Columna.Valores[Pos2] > CRutinas.StrVFloat(Filtrador.ValorMaximo))
          {
            Pos2--;
          }
          foreach (CLineaComprimida Linea in mLineas)
          {
            if (Linea.Vigente)
            {
              Linea.Vigente = (Pos1 >= 0 && Pos2 >= Pos1 && Linea.Codigos[PosCol] >= Pos1 && Linea.Codigos[PosCol] <= Pos2);
            }
          }
        }
        else
        {
          if (Filtrador.ValoresSeleccionados.Count > 0)
          {
            List<Int32> ListaValores = Filtrador.Columna.ExtraerCodigosListaValores(Filtrador.ValoresSeleccionados);
            foreach (CLineaComprimida Linea in mLineas)
            {
              if (Linea.Vigente)
              {
                Linea.Vigente = ListaValores.Contains(Linea.Codigos[PosCol]);
              }
            }
          }
        }
      }
    }

    //public CFiltroPorAsociacion CrearFiltroParaColumna(
    //      CColumnaBase Columna, bool InicializarDatos)
    //{
    //  CFiltroPorAsociacion Respuesta =
    //      new CFiltroPorAsociacion();
    //  Respuesta.Filtrador.Columna = Columna;
    //  Respuesta.Proveedor = this;
    //  if (InicializarDatos)
    //  {
    //    Respuesta.CargarDatos();
    //  }
    //  mFiltros.Add(Respuesta.Filtrador);
    //  return Respuesta;
    //}

    public CFiltrador ObtenerFiltroColumna(string NombreColumna,
          bool CrearSiNoHay = true)
    {
      foreach (CFiltrador Elemento in mFiltros)
      {
        if (Elemento.Columna.Nombre == NombreColumna)
        {
          return Elemento;
        }
      }

      if (CrearSiNoHay)
      {
        return CrearFiltroParaColumna(ColumnaNombre(NombreColumna));
      }
      else
      {
        return null;
      }

    }

    public CFiltrador CrearFiltroParaColumna(
          CColumnaBase Columna)
    {
      if (Columna == null)
      {
        throw new Exception("Columna incorrecta al crear filtro columna");
      }
      CFiltrador Respuesta = new CFiltrador();
      Respuesta.Columna = Columna;
      Respuesta.Proveedor = this;
      mFiltros.Add(Respuesta);
      return Respuesta;
    }

    private void AgregarFiltrosAND()
    {
      for (Int32 i = mFiltros.Count - 1; i >= 0; i--)
      {
        // TODO: Ver si funciona.
        CColumnaBase ColPaso = mFiltros[i].Columna; //  ColumnaValor;
        if (ColPaso != null)
        {
          ObtenerFiltroColumna(ColPaso.Nombre);
        }
      }
    }

    private static CColumnaBase CopiarColumna(CColumnaBase Columna0)
    {
      return Columna0.CopiarColumna();
      //CColumnaBase Respuesta;
      //switch (Columna0.Clase)
      //{
      //  case ClaseVariable.Entero:
      //    Respuesta = new CColumnaEntero();
      //    break;
      //  case ClaseVariable.Real:
      //    Respuesta = new CColumnaReal();
      //    break;
      //  default:
      //    Respuesta = new CColumnaTexto();
      //    break;
      //}
      //Respuesta.Clase = Columna0.Clase;
      //Respuesta.Nombre = Columna0.Nombre;
      //Respuesta.Orden = Columna0.Orden;
      //Respuesta.Sinonimos = Columna0.Sinonimos;
      //Respuesta.Valores = new List<object>();
      //Respuesta.CodigosTransformados 
      //foreach (object Valor in Columna0.Valores)
      //{
      //  Respuesta.Valores.Add(Valor);
      //}
      //return Respuesta;
    }

    public static List<CColumnaBase> CopiarColumnas(List<CColumnaBase> ColumnasBase)
    {
      List<CColumnaBase> Respuesta = new List<CColumnaBase>();

      foreach (CColumnaBase Col in ColumnasBase)
      {
        Respuesta.Add(CopiarColumna(Col));
      }

      return Respuesta;

    }

    private void PonerLineasVigentes()
    {
      if (mLineas != null)
      {
        foreach (Datos.CLineaComprimida Linea in mLineas)
        {
          Linea.Vigente = true;
        }
      }
    }

    private void OptimizarOrdenFiltros()
    {
      mFiltros.Sort(delegate(CFiltrador F1,
                CFiltrador F2)
      {
        if (F1.CondicionAND == F2.CondicionAND)
        {
          return F1.ValoresSeleccionados.Count.CompareTo(F2.ValoresSeleccionados.Count);
        }
        else
        {
          return (F1.CondicionAND ? 1 : -1);
        }
      });
    }

    private void AjustarVigenciaLineasDesdeFiltros()
    {
      bool bSucio = true;
      while (bSucio)
      {
        bSucio = false;
        for (Int32 i = 0; i < mFiltros.Count; i++)
        {
          bSucio = bSucio || mFiltros[i].FiltrarDataset();
        }
      }
    }

    /// <summary>
    /// Filtra los datos desde los filtros de otro proveedor.
    /// Para lograrlo crea una serie de filtros y los ajusta a partir de los filtros del otro proveedor.
    /// </summary>
    /// <param name="Otro"></param>
    public void FiltrarDesdeOtrosFiltros(CProveedorComprimido Otro)
    {
      if (mbBlockeado || TableroPecasV5.Client.Contenedores.CContenedorDatos.EventosBloqueados)
      {
        return;
      }
      // Copia los filtros a un vector de filtros en memoria.
      CopiarFiltrosDesdeProveedorPeriodo(Otro);

      // Inicializar selecciones segun las condiciones propias de cada filtro.
      InicializarSeleccionesFiltros();

      // Antes de iniciar el proceso, agrega los filtros que pueden requerirse para condiciones AND.
      AgregarFiltrosAND();

      // primer paso: poner todo vigente.
      PonerLineasVigentes();
      //      PonerFiltrosVigentes();

      // Ordenar para poner primero los OR y despues los AND y dejar las filas con menos valores al principio.
      OptimizarOrdenFiltros();

      // Ahora ajusta la vigencia de las filas desde los filtros.
      AjustarVigenciaLineasDesdeFiltros();

      // ensucia los datos, para volver a determinar los datos vigentes a exportar al proveedor de
      // grillas y graficos.
      mbSucio = true;

    }

    /// <summary>
    /// Refresca los codigos de los elementos seleccionados y los rangos.
    /// </summary>
    private void RefrescarSelecciones()
    {
      foreach (CFiltrador Filtro in mFiltros)
      {
        Filtro.RefrescarSelecciones();
      }
    }

    private void AjustarSumasRelaciones()
    {
      foreach (CFiltrador Filtro in mFiltros)
      {
        if (Filtro.ColumnaValor != null)
        {
          Filtro.AsociarConColumnaValor();
        }
      }
    }

    /// <summary>
    /// Cuando hay una columna asociada, ajusta las cantidades.
    /// </summary>
    private void AjustarValoresFiltros()
    {
      // Cuando hay una columna con una columna de valor y esa columna se elimina porque tambien hay una ventana de filtro
      // sobre la misma columna, hay que continuar el trabajo.
      Int32 CantFiltros = mFiltros.Count;
      Int32 PosFitro = 0;
      while (PosFitro < CantFiltros)
      {
        CFiltrador Filtro = mFiltros[PosFitro];
        Filtro.AjustarValores();
        PosFitro++;
        CantFiltros = mFiltros.Count;
      }
    }

    private string TextoCondicionFiltrador(CCondicionFiltradorCN Cnd)
    {
      string Respuesta = "Campo = "+Cnd.NombreColumna+Environment.NewLine;
      if (Cnd.NombreColumnaAND.Length > 0)
      {
        Respuesta += "Columna AND = " + Cnd.NombreColumnaAND + " " + Cnd.Coincidencias.ToString() +
          " Coincidendias" + Environment.NewLine;
      }
      if (!double.IsNaN(Cnd.RangoMinimo))
      {
        Respuesta += "Mínimo = " + CRutinas.ValorATexto(Cnd.RangoMinimo) + Environment.NewLine;
      }
      if (!double.IsNaN(Cnd.RangoMinimo))
      {
        Respuesta += "Máximo = " + CRutinas.ValorATexto(Cnd.RangoMaximo) + Environment.NewLine;
      }
      if (Cnd.ValoresImpuestos.Count > 0)
      {
        Respuesta += "Valores seleccionados:" + Environment.NewLine;
        foreach (string Valor in Cnd.ValoresImpuestos)
        {
          Respuesta += Valor + Environment.NewLine;
        }
      }
      Respuesta += Environment.NewLine;
      return Respuesta;
    }

    private string TextoCondicionFiltro(CCondicionFiltroCN Cnd,bool Primera)
    {
      string Respuesta = (Primera?"Columna " + Cnd.CampoCondicion + Environment.NewLine:"");
      if (!Primera)
      {
        Respuesta += (Cnd.DebeCumplirTodasEnBlock ? " AND " : " OR ");
      }

      switch (Cnd.ModoDeFiltrar)
      {
        case ModoFiltrar.PorRango:
          Respuesta += "(Entre " + CRutinas.ValorATexto(Cnd.ValorMinimo) + " y " +
                CRutinas.ValorATexto(Cnd.ValorMaximo) + ")";
          break;
        default:
          Respuesta += Cnd.ValorTexto;
          break;
      }

      return Respuesta + Environment.NewLine;

    }

    private string TextoBlock(CBlockPlantilla Block)
    {
      string Respuesta = Block.Nombre + Environment.NewLine;
      foreach (CPasoGraficoCN Paso in Block.Pasos)
      {
        foreach (CCondicionFiltradorCN Cnd in Paso.CondicionesFiltrador)
        {
          Respuesta += TextoCondicionFiltrador(Cnd);
        }
        foreach (CGraficoCN Grafico in Paso.Graficos)
        {
          bool Primera = true;
          foreach (CCondicionFiltroCN Cnd in Grafico.CondicionesFiltro)
          {
            Respuesta += TextoCondicionFiltro(Cnd, Primera);
            Primera = false;
          }
        }
      }
      return Respuesta;
    }

    public string TextoOrigenDatos()
    {
      string Respuesta = "";
      foreach (CBlockPlantilla Block in mCondicionesPrevias)
      {
        Respuesta += TextoBlock(Block) + Environment.NewLine + Environment.NewLine;
      }

      //Respuesta += CCcontenedorDatos.ContenedorUnico.NombreReferencia(null, mClaseOrigen, mCodigoOrigen, -1, "") +
      //    Environment.NewLine;
      foreach (CCondicionFiltradorCN Cnd in CondicionesPropias())
      {
        Respuesta += TextoCondicionFiltrador(Cnd);
      }

      return Respuesta;

    }

    public List<CCondicionFiltradorCN> CondicionesPropias()
    {
      List<CCondicionFiltradorCN> Respuesta = new List<CCondicionFiltradorCN>();
      foreach (CFiltrador Filtro in mFiltros)
      {
        if (Filtro.HayCondicion)
        {
          Respuesta.Add(Filtro.ObtenerCondicion());
        }
      }
      return Respuesta;
    }

    /// <summary>
    /// Obtiene los filtros de acuerdo a la seleccion desde las pantallas.
    /// </summary>
    /// <returns></returns>
    public List<CPasoCondicionesBlock> ObtenerFiltrosBlockDesdeAsociaciones()
    {
      List<CPasoCondicionesBlock> Respuesta = new List<CPasoCondicionesBlock>();
      foreach (CFiltrador Filtro in mFiltros)
      {
        CGrupoCondicionesBlock Grupo = Filtro.ObtenerCondicionBlock();
        if (Grupo.Condiciones.Count > 0)
        {
          CPasoCondicionesBlock Paso = new CPasoCondicionesBlock();
          Paso.Grupos.Add(Grupo);
          Respuesta.Add(Paso);
        }
      }
      return Respuesta;
    }

    private bool VerificarQueHayaLineas()
    {
      if (mLineas.Count == 0)
      {
        return false;
      }
      else
      {
        return true;
      }
    }

    public void FiltrarPorAsociaciones()
    {
      if (mbBlockeado || Contenedores.CContenedorDatos.EventosBloqueados)
      {
        return;
      }

      VerificarQueHayaLineas();

      mbBlockeado = true;

      try
      {
        // Refrescar la seleccion de los filtros.
        RefrescarSelecciones();

        VerificarQueHayaLineas();
        // Inicializar selecciones.
        InicializarSeleccionesFiltros();

        VerificarQueHayaLineas();
        // Antes de iniciar el proceso, agrega los filtros que pueden requerirse para condiciones AND.
        AgregarFiltrosAND();

        VerificarQueHayaLineas();
        // primer paso: poner todo vigente.
        PonerLineasVigentes();
        //      PonerFiltrosVigentes();

        VerificarQueHayaLineas();
        // Ordenar para poner primero los OR y despues los AND y dejar las filas con menos valores al principio.
        OptimizarOrdenFiltros();

        VerificarQueHayaLineas();
        // Ahora ajusta la vigencia de las filas desde los filtros.
        AjustarVigenciaLineasDesdeFiltros();

        // cuando los filtros tengan columnas de valor, refrescan las relaciones.
        VerificarQueHayaLineas();
        AjustarSumasRelaciones();

        // Ajusta los acumulados de los filtros con acumulador.
        VerificarQueHayaLineas();
        AjustarValoresFiltros();

        // Ajusta la representacion de los filtros.
        //foreach (CFiltrador Filtro in mFiltros)
        //{
//          Filtro.RefrescarContenido();
//          {
////            Filtro.Filtro.AjustarCantidades();
//            Filtro.Filtro.RefrescarContenidoLista();
//          }
        //}

        // ensucia los datos, para volver a determinar los datos vigentes a exportar al proveedor de
        // grillas y graficos.
        mbSucio = true;

        VerificarQueHayaLineas();
        // Cuando se ajusta el contenido del dataset, ajustar los elementos dependientes (tortas, etc.).
        ForzarAjustarDependientes();

        VerificarQueHayaLineas();

      }
      finally
      {
        mbBlockeado = false;
      }

    }

    public void ForzarAjustarDependientes()
    {
      if (AlAjustarDependientes != null)
      {
        AlAjustarDependientes(this);
      }
    }

    public void AjustarVentanasAsociadas()
    {
      mbSucio = true;
      ForzarAjustarDependientes();
    }

    public void LimpiarFiltrarAsociaciones(CElementoFilaAsociativa Fila)
    {
      foreach (CFiltrador Filtro in mFiltros)
      {
        Filtro.FiltrarPorFila(Fila);
        //Filtro.Filtro.LimpiarFiltrarAsociaciones(Fila);
        //Filtro.Filtro.LimpiarFiltrosAsociados();
      }
    }

    /// <summary>
    /// Retorna un filtrador que se corresponde con una columna.
    /// </summary>
    /// <param name="Columna"></param>
    /// <returns></returns>
    public CFiltrador FiltroParaColumna(string Columna)
    {
      foreach (CFiltrador Filtro in mFiltros)
      {
        if (Filtro.Columna.Nombre == Columna)
        {
          return Filtro;
        }
      }
      return null;
    }

    /// <summary>
    /// Se usa cuando los datos provienen de una fuente donde ya tenemos los datos
    /// en lineas de texto y los queremos convertir al formato zipeado
    /// para poder filtrar por asociacion.
    /// </summary>
    /// <param name="ColumnasOriginales"></param>
    public void ArmarDatosZip(List<CColumnaBase> ColumnasOriginales,
          List<CLineaDatos> DatosOrg)
    {

      mColumnas = CopiarColumnas(ColumnasOriginales);
      string Msg = "";

      try
      {
        if (DatosOrg != null)
        {
          foreach (CColumnaBase Col0 in mColumnas)
          {
            while (Col0.Codigos.Count < Col0.Valores.Count)
            {
              Col0.Codigos.Add(Col0.Codigos.Count);
            }
          }

          mLineas = new List<CLineaComprimida>();

          Int32 ii = 0;

          foreach (CLineaDatos Dato in DatosOrg)
          {
            CLineaComprimida Linea = new CLineaComprimida();
            Linea.Vigente = true;
            Linea.Codigos = new List<int>();
            Int32 Pos = 0;
            foreach (CColumnaBase Columna in mColumnas)
            {
              Linea.Codigos.Add(Columna.AgregarValor(Columna.TraducirTexto(Dato.Contenidos[Pos])));
              Pos++;
              Msg = ii.ToString() + " " + Pos.ToString();
            }
            mLineas.Add(Linea);
            ii++;
          }
        }
      }
      catch (Exception ex)
      {
        throw new Exception(CRutinas.MostrarMensajeError(ex) + " " + mLineas.Count.ToString() + " " + Msg);
      }

      CorregirCodigosOrdenandolos();

    }

    public double ExtraerDatoReal(CLineaComprimida Linea, Int32 Orden)
    {
      if (Orden >= 0 && Orden < mColumnas.Count)
      {
        return mColumnas[Orden].ValorReal(Linea.Codigos[Orden]);
      }
      else
      {
        return double.NaN;
      }
    }

    public List<CColumnaBase> ColumnasFecha()
    {
      List<CColumnaBase> Respuesta = new List<CColumnaBase>();
      foreach (CColumnaBase Col in mColumnas)
      {
        if (Col.Clase == ClaseVariable.Fecha)
        {
          Respuesta.Add(Col);
        }
      }
      return Respuesta;
    }

    private CParesEnteros ParesTraduccion(Int32 Posicion, CColumnaBase ColumnaDataset)
    {
      try
      {
        CParesEnteros Respuesta = new CParesEnteros();
        Int32 i1 = 0;
        Int32 i2 = 0;
        while (i2 < ColumnaDataset.ListaValores.Count)
        {
          Int32 Comparador = mColumnas[Posicion].CompararValores(i1,
                  ColumnaDataset.Valores[i2]);
          if (Comparador == 0)
          {
            Respuesta.Pares.Add(i1);
            i2++;
          }
          i1++;
        }
        return Respuesta;
      }
      catch (Exception ex)
      {
        throw new Exception("PTrad " + Environment.NewLine + CRutinas.MostrarMensajeError(ex));
      }
    }

    private List<CParesEnteros> ArmarListaCodigosTraduccion(CProveedorComprimido Dataset)
    {
      List<CParesEnteros> Respuesta = new List<CParesEnteros>();
      for (Int32 i = 0; i < Dataset.Columnas.Count; i++)
      {
        Respuesta.Add(ParesTraduccion(i, Dataset.Columnas[i]));
      }
      return Respuesta;
    }

    private void AgregarLineasDataset(CProveedorComprimido Dataset, Int32 PosFecha)
    {
      try
      {
        // armar una lista de vectores con los codigos de conversion.
        List<CParesEnteros> CodigosTraduccion = ArmarListaCodigosTraduccion(Dataset);

        // agregar las lineas traduciendolas.
        foreach (CLineaComprimida Linea in Dataset.Datos)
        {
          CLineaComprimida LinN = new CLineaComprimida();
          LinN.Vigente = true;
          LinN.Codigos = new List<int>();
          for (Int32 i = 0; i < Linea.Codigos.Count; i++)
          {
            LinN.Codigos.Add(
                (Linea.Codigos[i] < 0 || Linea.Codigos[i]>=CodigosTraduccion[i].Pares.Count) ? -1 :
                CodigosTraduccion[i].Pares[Linea.Codigos[i]]);
          }
          if (PosFecha >= 0)
          {
            LinN.Codigos.Add(PosFecha);
          }
          mLineas.Add(LinN);
        }
      }
      catch (Exception ex)
      {
        throw new Exception("AgrLin " + Environment.NewLine + CRutinas.MostrarMensajeError(ex));
      }
    }

    private void AcumularLineasDatasets(List<CProveedorComprimido> Datasets, List<DateTime> ValoresPeriodos)
    {
      try
      {
        mLineas = new List<CLineaComprimida>();
        for (Int32 i = 0; i < Datasets.Count; i++)
        {
          Int32 PosFecha = (ValoresPeriodos == null ? -1 : i);
          AgregarLineasDataset(Datasets[i], i);
        }
      }
      catch (Exception ex)
      {
        throw new Exception("AcLin " + Environment.NewLine + CRutinas.MostrarMensajeError(ex));
      }
    }

    //private void ConsolidarColumnas(CColumnaBase Col1, CColumnaBase Col2)
    //{
    //  Int32 i1 = 0;
    //  Int32 i2 = 0;
    //  // se trata de que el valor apuntado con i1 sea >= al apuntado por i2.
    //  // cuando coincidan, adelantan ambos.
    //  // cuando el valor en i2 es >, inserta.
    //  while (i2 < Col2.Valores.Count)
    //  {
    //    Int32 Comparador = (i1 >= Col1.Valores.Count ? -1 :
    //        Col1.CompararValores(i1, Col2.Valores[i2]));
    //    switch (Comparador)
    //    {
    //      case 0:
    //        i1++;
    //        i2++;
    //        break; // si = entonces continua.
    //      case -1: // si es menor el valor de Col0 o no hay mas.
    //        if (i1 < Col1.Valores.Count) // si quedan valores tiene que comparar con el proximo de Col1.
    //        {
    //          i1++; // si hay mas, adelanta Col0.
    //        }
    //        else
    //        {
    //          // cuando no quedan, agrega el valor de col2 y sigue.
    //          Col1.Valores.Add(Col2.Valores[i2]);
    //          i2++;
    //        }
    //        break;
    //      default: // el valor de col1 es mayor.
    //        Col1.Valores.Insert(i1,Col2.Valores[i2]);
    //        i1++; // el valor de comparacion es el agregado.
    //        i2++; // hay que adelantar Col2.
    //        break;
    //    }
    //  }
    //}

    private Int32 AgregarValor(CColumnaBase ColProcesada, Int32 PosProceso, Int32 IndiceProceso,
          CColumnaBase ColSuma, Int32 PosSuma,
          List<CodigosEquivalentes> Equivalencias)
    {
      if (PosSuma < 0 || ColProcesada.CompararValores(IndiceProceso,
              ColSuma.Valores[PosSuma]) != 0)
      {
        ColSuma.Valores.Add(ColProcesada.Valores[IndiceProceso]);
        PosSuma++;
      }

      Equivalencias[PosProceso].PosicionesNuevas.Add(PosSuma);

      return PosSuma;

    }

    private Int32 TraducirCodigo(Int32 PosLocal, CodigosEquivalentes Equivalentes)
    {
      if (PosLocal < 0 || PosLocal > Equivalentes.PosicionesNuevas.Count)
      {
        return -1;
      }
      else
      {
        return Equivalentes.PosicionesNuevas[PosLocal];
      }
    }

    /// <summary>
    /// Unifica una columna de varios proveedores en una unica, sin hacer sorts ni reacomodar listas.
    /// </summary>
    /// <param name="Col1"></param>
    /// <param name="Proveedores"></param>
    /// <param name="Posicion"></param>
    private void CrearColumnaAgregada(CColumnaBase ColSuma, List<CProveedorComprimido> Proveedores,
          Int32 Posicion)
    {
      // crea una lista con las equivalencias de cada proveedor.
      List<CodigosEquivalentes> Equivalencias = new List<CodigosEquivalentes>();
      List<Int32> PosicionesMinimas = new List<int>();
      for (Int32 i = 0; i < Proveedores.Count; i++)
      {
        PosicionesMinimas.Add(Proveedores[i].Datos.Count > 0 ? 0 : -1);
        Equivalencias.Add(new CodigosEquivalentes());
      }

      Int32 PosProceso;
      Int32 PosSuma = -1;
      CColumnaBase ColProceso;

      // Arma una columna con los datos de todas las columnas y un vector con las equivalencias.
      while (true)
      {
        PosProceso = -1;
        ColProceso = null;
        object ValorProximo = null; // es el valor a agregar.
        // Barre las columnas y busca el valor minimo.
        for (Int32 i = 0; i < Proveedores.Count; i++)
        {
          if (PosicionesMinimas[i] >= 0)
          {
            CColumnaBase ColLocal = Proveedores[i].Columnas[Posicion];
            if (ValorProximo == null ||
                ColLocal.CompararValores(PosicionesMinimas[i], ValorProximo) < 0)
            {
              ColProceso = ColLocal;
              PosProceso = i;
              ValorProximo = ColProceso.Valores[PosicionesMinimas[i]];
            }
          }
        }

        // si no hay mas valores, listo.
        if (ValorProximo == null)
        {
          break;
        }

        // con el minimo, agrega el valor.
        PosSuma = AgregarValor(ColProceso, PosProceso, PosicionesMinimas[PosProceso],
            ColSuma, PosSuma, Equivalencias);
        PosicionesMinimas[PosProceso]++;
        if (PosicionesMinimas[PosProceso] >= ColProceso.Valores.Count)
        {
          PosicionesMinimas[PosProceso] = -1;
        }

      }

      // Ahora agrega los datos.
      bool bCrearLineas = (mLineas.Count == 0);
      Int32 PosLinea = 0;
      for (Int32 i=0;i<Proveedores.Count;i++)
      {
        CProveedorComprimido ProvLocal=Proveedores[i];
        foreach (CLineaComprimida Linea in ProvLocal.mLineas)
        {
          if (bCrearLineas)
          {
            mLineas.Add(new CLineaComprimida());
          }
          mLineas[PosLinea].Codigos.Add(TraducirCodigo(Linea.Codigos[Posicion], Equivalencias[i]));
          PosLinea++;
        }
      }

      //// se trata de que el valor apuntado con i1 sea >= al apuntado por i2.
      //// cuando coincidan, adelantan ambos.
      //// cuando el valor en i2 es >, inserta.
      //while (i2 < Col2.Valores.Count)
      //{
      //  Int32 Comparador = (i1 >= Col1.Valores.Count ? -1 :
      //      Col1.CompararValores(i1, Col2.Valores[i2]));
      //  switch (Comparador)
      //  {
      //    case 0:
      //      i1++;
      //      i2++;
      //      break; // si = entonces continua.
      //    case -1: // si es menor el valor de Col0 o no hay mas.
      //      if (i1 < Col1.Valores.Count) // si quedan valores tiene que comparar con el proximo de Col1.
      //      {
      //        i1++; // si hay mas, adelanta Col0.
      //      }
      //      else
      //      {
      //        // cuando no quedan, agrega el valor de col2 y sigue.
      //        Col1.Valores.Add(Col2.Valores[i2]);
      //        i2++;
      //      }
      //      break;
      //    default: // el valor de col1 es mayor.
      //      Col1.Valores.Insert(i1, Col2.Valores[i2]);
      //      i1++; // el valor de comparacion es el agregado.
      //      i2++; // hay que adelantar Col2.
      //      break;
      //  }
      //}
    }

    private CColumnaBase CrearColumnaUnificada(List<CProveedorComprimido> Proveedores, Int32 Posicion)
    {
      // crea la columna y la inicializa con el primer proveedor.
      CColumnaBase Respuesta = CrearColumnaClase(Proveedores[0].Columnas[Posicion].Clase);
      Respuesta.Clase = Proveedores[0].Columnas[Posicion].Clase;
      Respuesta.Nombre = Proveedores[0].Columnas[Posicion].Nombre;
      Respuesta.Orden = Posicion;
//      Respuesta.Sinonimos = Proveedores[0].Columnas[Posicion].Sinonimos;
      Respuesta.Valores = new List<object>();
//      Respuesta.Valores.AddRange(Proveedores[0].Columnas[Posicion].Valores);

      for (Int32 i = 1; i < Proveedores.Count; i++)
      {
        if (Proveedores[i].Columnas[Posicion].Clase != Proveedores[0].Columnas[Posicion].Clase)
        {
          throw new Exception("Error en clase columna " + Proveedores[0].Columnas[Posicion].Nombre);
        }
//        ConsolidarColumnas(Respuesta, Proveedores[i].Columnas[Posicion]);
      }

      CrearColumnaAgregada(Respuesta, Proveedores, Posicion);

      return Respuesta;

    }

    private CColumnaBase CrearColumnaPeriodos(List<DateTime> Valores, Int32 Posicion,
          List<CProveedorComprimido> Proveedores)
    {
      // crea la columna y la inicializa con el primer proveedor.
      CColumnaBase Respuesta = new CColumnaTexto();
      Respuesta.Clase = ClaseVariable.Fecha;
      Respuesta.Nombre = PERIODO_DATOS_DATASET;
      Respuesta.Orden = Posicion;
//      Respuesta.Sinonimos = new List<IndicadoresV2.CSinonimoCompletoCN>();
      Valores.Sort(delegate(DateTime A, DateTime B)
      {
        return A.CompareTo(B);
      });
      Respuesta.Valores = new List<object>();
      Int32 PosFila = 0;
      for (Int32 i = 0; i < Valores.Count; i++)
      {
        DateTime Fecha = Valores[i];
        Respuesta.Valores.Add(CRutinas.CodificarFechaHora(Fecha));
        for (Int32 ii = 0; ii < Proveedores[i].Datos.Count; ii++)
        {
          mLineas[PosFila].Codigos.Add(i);
          PosFila++;
        }
      }

      return Respuesta;

    }

    private List<CColumnaBase> UnificarColumnasDatasets(List<CProveedorComprimido> Proveedores,
        List<DateTime> ValoresPeriodos)
    {
      try
      {
        List<CColumnaBase> Respuesta = new List<CColumnaBase>();
        if (Proveedores.Count > 0 &&
            (ValoresPeriodos == null || ValoresPeriodos.Count == Proveedores.Count))
        {
          for (Int32 i = 0; i < Proveedores[0].Columnas.Count; i++)
          {
            Respuesta.Add(CrearColumnaUnificada(Proveedores, i));
          }

          if (ValoresPeriodos != null)
          {
            Respuesta.Add(CrearColumnaPeriodos(ValoresPeriodos, Respuesta.Count,Proveedores));
          }
        }
        return Respuesta;
      }
      catch (Exception ex)
      {
        throw new Exception("UnifCol " + Environment.NewLine + CRutinas.MostrarMensajeError(ex));
      }
    }

    public void ImponerDataset(List<CColumnaBase> ColIni, List<CLineaComprimida> LinIni)
    {
      mColumnas = CopiarColumnas(ColIni);
      mLineas = CopiarLineas(LinIni);
      mbSucio = true;
    }

    /// <summary>
    /// Unifica los datos de varios datasets en uno unico y si hay datos en ValoresPeriodos,
    /// agrega una columna con ellos.
    /// </summary>
    /// <param name="Datasets">Lista de datasets a unificar.</param>
    /// <param name="ValoresPeriodos">Fecha de cada periodo (ORDENADA).</param>
    public void UnificarDatasets(List<CProveedorComprimido> Datasets, List<DateTime> ValoresPeriodos)
    {
      try
      {
        if (Datasets.Count > 0)
        {
          mLineas = new List<CLineaComprimida>();
          mColumnas = UnificarColumnasDatasets(Datasets, ValoresPeriodos);
//          AcumularLineasDatasets(Datasets, ValoresPeriodos);
          mbSucio = true;
          foreach (CColumnaBase Col in mColumnas)
          {
            Col.DatosSucios = true;
          }
        }
      }
      catch (Exception ex)
      {
        throw new Exception("UDSet " + Environment.NewLine + CRutinas.MostrarMensajeError(ex));
      }

    }

  }

  public class CodigosEquivalentes
  {
    public List<Int32> PosicionesNuevas = new List<int>();
  }

}
