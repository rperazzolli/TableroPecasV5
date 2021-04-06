using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;


namespace TableroPecasV5.Client.Datos
{
  public class CColumnaBase
  {
    protected Int32 mOrden;
    protected string mszNombre;
    protected bool mbSucio;
    protected ClaseVariable mClase;
    protected List<object> mValores;
    protected List<Int32> mCodigos;
//    protected List<CSinonimoCompletoCN> mPaquetes;
    protected List<string> mszValores;

    public CColumnaBase()
    {
      mOrden = -1;
      mClase = ClaseVariable.NoDefinida;
      mValores = new List<object>();
      mCodigos = new List<int>();
//      mPaquetes = new List<CSinonimoCompletoCN>();
      mszValores = new List<string>();
    }

    public ClaseVariable Clase
    {
      get { return mClase; }
      set { mClase = value; }
    }

    public Int32 Orden
    {
      get { return mOrden; }
      set { mOrden = value; }
    }

    public string Nombre
    {
      get { return mszNombre; }
      set { mszNombre = value; }
    }

    public bool DatosSucios
    {
      set { mbSucio = value; }
    }

    //public List<CSinonimoCompletoCN> Sinonimos
    //{
    //  get { return mPaquetes; }
    //  set { mPaquetes = value; }
    //}

    public List<object> Valores
    {
      get { return mValores; }
      set
      {
        mbSucio = true;
        mValores = value;
      }
    }

    public List<Int32> Codigos
    {
      get { return mCodigos; }
    }

    public List<object> CopiarValores()
    {
      List<object> Respuesta = new List<object>();
      Respuesta.AddRange(mValores);
      return Respuesta;
    }

    public List<Int32> CopiarCodigos()
    {
      List<Int32> Respuesta = new List<Int32>();
      Respuesta.AddRange(mCodigos);
      return Respuesta;
    }

    public void AjustarClase(ClaseVariable ClaseExterna, bool bHayDatos = false)
    {
      if (mValores.Count == 0 && !bHayDatos)
      {
        mClase = ClaseExterna;
      }
      else
      {
        switch (ClaseExterna)
        {
          case ClaseVariable.Texto:
            mClase = ClaseExterna;
            break;
          case ClaseVariable.Entero:
            if (mClase == ClaseVariable.Real || mClase == ClaseVariable.Fecha)
            {
              mClase = ClaseExterna;
            }
            break;
          case ClaseVariable.Real:
            if (mClase == ClaseVariable.Fecha)
            {
              mClase = ClaseExterna;
            }
            break;
        }
      }
    }

    public CColumnaBase CopiarColumna(bool IncluirValores = true)
    {
      CColumnaBase Respuesta;
      switch (mClase)
      {
        case ClaseVariable.Entero:
          Respuesta = new CColumnaEntero();
          break;
        case ClaseVariable.Real:
          Respuesta = new CColumnaReal();
          break;
        default:
          Respuesta = new CColumnaTexto();
          break;
      }
      Respuesta.Clase = mClase;
      Respuesta.Orden = mOrden;
      Respuesta.Nombre = mszNombre;
      if (IncluirValores)
      {
        Respuesta.Valores = CopiarValores();
        Respuesta.mCodigos = CopiarCodigos();
      }
      return Respuesta;
    }

    public List<string> ListaValores
    {
      get
      {
        if (mbSucio)
        {
          mbSucio = false;
          mszValores.Clear();
          switch (mClase)
          {
            case ClaseVariable.Entero:
              foreach (object Valor in mValores)
              {
                mszValores.Add(((Int32)Valor).ToString());
              }
              break;
            case ClaseVariable.Real:
              foreach (object Valor in mValores)
              {
                mszValores.Add(double.IsNaN((double)Valor) ? "" : ((double)Valor).ToString());
              }
              break;
            default:
              foreach (object Valor in mValores)
              {
                mszValores.Add((string)Valor);
              }
              break;
          }
        }
        return mszValores;
      }
    }

    public override string ToString()
    {
      return mszNombre;
    }

    public string TextoIndice(Int32 Indice)
    {
      switch (mClase)
      {
        case ClaseVariable.Entero:
          return ((Int32)mValores[Indice]).ToString();
        case ClaseVariable.Real:
          return ((double)mValores[Indice]).ToString();
        case ClaseVariable.Fecha:
          return string.Format("{0:dd/MM/yy HH:mm}",
            CRutinas.DecodificarFecha(Indice<0?"":(string)mValores[Indice]));
        default:
          return (string)mValores[Indice];
      }
    }

    private DateTime FechaPosicion(Int32 Posicion)
    {
      if (Posicion >= 0 && Posicion < mValores.Count)
      {
        return CRutinas.DecodificarFecha((string)mValores[Posicion]);
      }
      else
      {
        return CRutinas.FechaMinima();
      }
    }

    public DateTime PrimeraFechaValida()
    {
      for (Int32 i = 0; i < mValores.Count; i++)
      {
        DateTime Fecha = FechaPosicion(i);
        if (Fecha.Year > 1900)
        {
          return Fecha;
        }
      }
      return CRutinas.NoFecha();
    }

    private Int32 FncCompara1(Int32 i1, Int32 i2)
    {
      return CompararValores(i1, mValores[i2]);
    }

    private List<Int32> ArmarVectorInicializado()
    {
      List<Int32> Posicion = new List<int>();
      for (Int32 i = 0; i < mValores.Count; i++)
      {
        Posicion.Add(i);
      }
      return Posicion;
    }

    public void ReemplazarValorTexto(string Antes, string Ahora)
    {
      for (Int32 i = 0; i < Valores.Count; i++)
      {
        if (((string)Valores[i]) == Antes)
        {
          Valores[i] = Ahora;
          break;
        }
      }
    }

    /// <summary>
    /// Se usa cuando se modifican los valores al modificar la clase de columna
    /// cuando se importa de Excel.
    /// </summary>
    public List<Int32> ReordenarValores()
    {
      List<Int32> Posicion = ArmarVectorInicializado();
      Posicion.Sort(FncCompara1);

      // ahora busca los codigos de traduccion.
      List<Int32> Respuesta = ArmarVectorInicializado();
      Respuesta.Sort(delegate(Int32 i1, Int32 i2)
      {
        return Posicion[i1].CompareTo(Posicion[i2]);
      });

      // ahora reordena los contenidos de valores.
      mValores.Sort(FncComparar);

      return Respuesta;
    }

    public Int32 RangoDeFechas()
    {
      switch (mClase)
      {
        case ClaseVariable.Fecha:
          return (mValores.Count < 2 ? 1 :
              (Int32)(FechaPosicion(mValores.Count - 1).ToOADate() - PrimeraFechaValida().ToOADate() + 0.01));
        default:
          return 1;
      }
    }

    public ModoAgruparIndependiente ModoAgruparPorDefecto()
    {
      switch (mClase)
      {
        case ClaseVariable.Entero:
        case ClaseVariable.Real:
          return ModoAgruparIndependiente.Rangos;
        case ClaseVariable.Fecha:
          return ModoAgruparSegunFechasExtremas();
        default:
          return ModoAgruparIndependiente.Todos;
      }
    }

    public ModoAgruparIndependiente ModoAgruparSegunFechasExtremas()
    {
      // determinar diferencia de fechas (en dias).
      if (mszNombre == Datos.CProveedorComprimido.PERIODO_DATOS_DATASET)
      {
        return ModoAgruparIndependiente.Todos;
      }
      else
      {
        return CRutinas.AgrupamientoSegunRangoDias(RangoDeFechas());
      }
    }

    public double ValorReal(Int32 Posicion)
    {
      if (Posicion < 0 || Posicion >= mValores.Count)
      {
        return double.NaN;
      }
      else
      {
        switch (mClase)
        {
          case ClaseVariable.Real:
            return (double)mValores[Posicion];
          case ClaseVariable.Entero:
            return (Int32)mValores[Posicion];
          default:
            return double.NaN;
        }
      }
    }

    public DateTime ValorFecha(Int32 Posicion)
    {
      if (Posicion < 0 || Posicion >= mValores.Count)
      {
        return CRutinas.FechaMinima();
      }
      else
      {
        switch (mClase)
        {
          case ClaseVariable.Fecha:
            return CRutinas.DecodificarFecha(Posicion<0?"":(string)mValores[Posicion]);
          default:
            return CRutinas.FechaMinima();
        }
      }
    }

    public List<CParEnteros> CodigosTransformados()
    {
      List<CParEnteros> Respuesta = new List<CParEnteros>();
      for (Int32 i = 0; i < mValores.Count; i++)
      {
        Respuesta.Add(new CParEnteros() { Codigo1 = mCodigos[i], Codigo2 = i });
      }

      Respuesta.Sort(delegate(CParEnteros V1, CParEnteros V2)
      {
        return V1.Codigo1.CompareTo(V2.Codigo1);
      });

      return Respuesta;

    }

    public virtual Int32 CompararValores(Int32 Posicion, object Elemento)
    {
      throw new Exception("Declarar CompararValores");
    }

    public virtual Int32 FncComparar(object O1, object O2)
    {
      throw new Exception("Declarar FncComparar");
    }

    public virtual void CargarValores(BinaryReader Lector, ClaseVariable ClaseDatos)
    {
    }

    public bool ValorRealCoincide(double Valor, Int32 Posicion)
    {
      switch (mClase)
      {
        case ClaseVariable.Real:
          return (Valor == (double)mValores[Posicion]);
        case ClaseVariable.Fecha:
          return (CRutinas.CodificarFechaHora(DateTime.FromOADate(Valor)) == (string)mValores[Posicion]);
        default:
          return false;
      }
    }

    public Int32 PosicionValorIgual(object Elemento)
    {
      Int32 PosBase = PosicionValorMayorIgual(Elemento);
      if (PosBase == 0)
      {
        return (CompararValores(PosBase, Elemento) > 0 ? -1 : 0);
      }
      else
      {
        // por si se trata de reales, donde puede haber valores muy parecidos.
        return (PosBase >= mValores.Count ? PosBase : (CompararValores(PosBase, Elemento) > 0 ? PosBase - 1 : PosBase));
      }
    }

    public virtual object TraducirTexto(string Texto)
    {
      throw new NotImplementedException();
    }

    public Int32 PosicionValorMenorIgual(string Texto,
          bool Coincidente = false)
    {
      Int32 Valor = PosicionValorMayorIgualTexto(Texto, false);

      if (Valor >= mValores.Count)
      {
        Valor = mValores.Count - 1;
      }

      if (Valor >= 0)
      {
        Int32 Comparacion = CompararValores(Valor, TraducirTexto(Texto));
        if (Comparacion > 0)
        {
          Valor--;
        }
        if (Valor >= 0 && Coincidente)
        {
          if (CompararValores(Valor, TraducirTexto(Texto)) != 0)
          {
            Valor = -1;
          }
        }
      }

      return Valor;

    }

    public Int32 PosicionValorIgualTexto(string Texto)
    {
      return PosicionValorIgual(TraducirTexto(Texto));
    }

    public Int32 PosicionValorMayorIgualDoble(double Valor)
    {
      switch (mClase)
      {
        case ClaseVariable.Fecha:
          return PosicionValorMayorIgual(CRutinas.CodificarFechaHora(DateTime.FromOADate(Valor)));
        case ClaseVariable.Real:
          return PosicionValorMayorIgual(Valor);
        case ClaseVariable.Entero:
          return PosicionValorMayorIgual((Int32)Valor);
        default:
          throw new NotImplementedException();
      }
    }

    public Int32 PosicionValorIgualDoble(double Valor)
    {
      switch (mClase)
      {
        case ClaseVariable.Fecha:
          return PosicionValorIgual(CRutinas.CodificarFechaHora(DateTime.FromOADate(Valor)));
        case ClaseVariable.Real:
          return PosicionValorIgual(Valor);
        case ClaseVariable.Entero:
          return PosicionValorIgual((Int32)Valor);
        default:
          throw new NotImplementedException();
      }
    }

    public Int32 PosicionValorMayorIgualTexto(string Texto,bool Coincidente=false)
    {
      return PosicionValorMayorIgual(TraducirTexto(Texto), Coincidente);
    }

    /// <summary>
    /// Determina la posicion de un valor mayor o igual al buscado.
    /// </summary>
    /// <param name="Elemento"></param>
    /// <returns></returns>
    public Int32 PosicionValorMayorIgual(object Elemento, bool Coincidente = false)
    {
      if (mValores.Count == 0)
      {
        return 0;
      }
      Int32 i1 = 0;
      Int32 i2 = mValores.Count - 1;
      Int32 iMedio = -1;
      Int32 Comparacion = -1;

      while (i2 >= i1)
      {
        iMedio = (i2 + i1) / 2;
        Comparacion = CompararValores(iMedio, Elemento);
        if (Comparacion < 0)
        {
          i1 = iMedio + 1;
        }
        else
        {
          if (Comparacion > 0)
          {
            i2 = iMedio - 1;
          }
          else
          {
            return iMedio;
          }
        }
      }

      if (!Coincidente && iMedio >= 0)
      {
        if (Comparacion > 0)
        {
          return iMedio;
        }
        else
        {
          if ((iMedio + 1) < mValores.Count)
          {
            return iMedio + 1;
          }
        }
      }

      return mValores.Count;

    }

    public Int32 AgregarValorTexto(string Texto)
    {
      return AgregarValor(TraducirTexto(Texto));
    }

    public ModoAgruparIndependiente AjustarAgrupamientoIndependientePorDefecto
    {
      get {
        switch (Clase) {
          case ClaseVariable.Fecha:
            return CRutinas.AgrupamientoSegunRangoDias(RangoDeFechas());
          case ClaseVariable.Real:
            return ModoAgruparIndependiente.Rangos;
          default:
            return ModoAgruparIndependiente.Todos;
        }
      }
    }

    public ModoAgruparDependiente AjustarAgrupamientoDependientePorDefecto
    {
      get
      {
        switch (Clase)
        {
          case ClaseVariable.Real:
          case ClaseVariable.Entero:
            return ModoAgruparDependiente.Acumulado;
          default:
            return ModoAgruparDependiente.Cantidad;
        }
      }
    }

    public Int32 AgregarValor(object Elemento)
    {
      Int32 i = PosicionValorMayorIgual(Elemento);
      Int32 Codigo = mValores.Count;
      if (i >= mValores.Count)
      {
        mValores.Add(Elemento);
        mCodigos.Add(Codigo);
        i = Codigo;
      }
      else
      {
        if (CompararValores(i, Elemento) != 0)
        {
          mValores.Insert(i, Elemento);
          mCodigos.Insert(i, Codigo);
        }
      }
      return mCodigos[i];
    }

    //public static List<CColumnaDatasetCN> ArmarListaColumnas(
    //       List<CColumnaBase> Lista)
    //{
    //  List<CColumnaDatasetCN> Respuesta = new List<CColumnaDatasetCN>();
    //  foreach (CColumnaBase Elemento in Lista)
    //  {
    //    CColumnaDatasetCN Columna = new CColumnaDatasetCN();
    //    Columna.Clase = Elemento.Clase;
    //    Columna.Nombre = Elemento.Nombre;
    //    Columna.Orden = Elemento.Orden;
    //    Respuesta.Add(Columna);
    //  }
    //  return Respuesta;
    //}

    public List<Int32> ExtraerCodigosListaValores(List<string> Valores)
    {
      List<Int32> Respuesta = new List<int>();
      foreach (string Valor in Valores)
      {
        switch (Clase)
        {
          case ClaseVariable.Entero:
            Int32 ValInt;
            if (Int32.TryParse(Valor, out ValInt)) { }
            Int32 PosRefe = PosicionValorIgual(ValInt);
            if (PosRefe >= 0 && ((Int32)mValores[PosRefe]) == ValInt)
            {
              Respuesta.Add(PosRefe);
            }
            break;
          case ClaseVariable.Real:
            double ValR;
            if (double.TryParse(Valor, out ValR)) { }
            Int32 PosR = PosicionValorIgual(ValR);
            if (PosR >= 0 && Math.Abs((double)mValores[PosR]-ValR)<0.0001)
            {
              Respuesta.Add(PosR);
            }
            break;
          default:
            Int32 PosT = PosicionValorIgual(Valor);
            if (PosT >= 0 && (string)mValores[PosT] == Valor)
            {
              Respuesta.Add(PosT);
            }
            break;
        }
      }
      return Respuesta;
    }

  }

  public class CColumnaTexto : CColumnaBase
  {

    public CColumnaTexto()
    {
//      mPaquetes = new List<CSinonimoCompletoCN>();
    }

    public override int CompararValores(int Posicion, object Elemento)
    {
      return string.Compare((string)mValores[Posicion], (string)Elemento,
          StringComparison.InvariantCultureIgnoreCase);
    }

    public override Int32 FncComparar(object O1, object O2)
    {
      return ((string)O1).CompareTo((string)O2);
    }

    public void EliminarEspacios()
    {
      for (Int32 i=0;i<mValores.Count;i++)
      {
        if (mValores[i] is string)
        {
          mValores[i] = ((string)mValores[i]).Trim();
        }
      }
    }

    //private void CargarValoresFecha(BinaryReader Lector)
    //{
    //  Int32 CantValores = Lector.ReadInt32();
    //  mValores.Clear();
    //  for (Int32 i = 0; i < CantValores; i++)
    //  {
    //    mValores.Add(CRutinas.CodificarFechaHora(DateTime.FromOADate(Lector.ReadDouble())));
    //  }
    //}

    public override void CargarValores(BinaryReader Lector, ClaseVariable ClaseDatos)
    {
      Int32 CantValores = Lector.ReadInt32();
      mValores.Clear();
      for (Int32 i = 0; i < CantValores; i++)
      {
        mValores.Add(Lector.ReadString().Trim());
      }
    }

    public override object TraducirTexto(string Texto)
    {
      if (mClase == ClaseVariable.Fecha)
      {
        if (!CRutinas.EsFechaCodificada(Texto))
        {
          return CRutinas.CodificarFechaHora(DateTime.Parse(Texto));
        }
      }
      return Texto;
    }

  }

  public class CColumnaReal : CColumnaBase
  {
    public CColumnaReal()
    {
      mClase = ClaseVariable.Real;
    }

    public override int CompararValores(int Posicion, object Elemento)
    {
      if (double.IsNaN((double)mValores[Posicion]))
      {
        if (double.IsNaN((double)Elemento))
        {
          return 0;
        }
        else
        {
          return -1;
        }
      }
      else
      {
        if (double.IsNaN((double)Elemento))
        {
          return 1;
        }
        else
        {
          return ((double)mValores[Posicion]).CompareTo((double)Elemento);
        }
      }
    }

    public void ObtenerExtremos(out double Minimo, out double Maximo)
    {
      Minimo = double.MaxValue;
      Maximo = double.MinValue;
      foreach (double Valor in mValores)
      {
        if (!double.IsNaN(Valor))
        {
          Minimo = Math.Min(Valor, Minimo);
          Maximo = Math.Max(Valor, Maximo);
        }
      }

      if (Minimo > Maximo)
      {
        Minimo = 0;
        Maximo = 0;
      }
      else
      {
        CRutinas.AjustarExtremosEscala(ref Minimo, ref Maximo);
      }
    }

    public override Int32 FncComparar(object O1, object O2)
    {
      return ((double)O1).CompareTo((double)O2);
    }

    public override object TraducirTexto(string Texto)
    {
      return (Texto.Length == 0 ? double.NaN :
        (Texto.Contains(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) ?
            double.Parse(Texto) : CRutinas.StrVFloat(Texto)));
    }

    public override void CargarValores(BinaryReader Lector, ClaseVariable ClaseDatos)
    {
      Int32 CantValores = Lector.ReadInt32();
      mValores.Clear();
      for (Int32 i = 0; i < CantValores; i++)
      {
        mValores.Add(Lector.ReadDouble());
      }
    }

  }

  public class CColumnaEntero : CColumnaBase
  {
    public CColumnaEntero()
    {
      mClase = ClaseVariable.Entero;
    }

    public override int CompararValores(int Posicion, object Elemento)
    {
      return ((Int32)mValores[Posicion]).CompareTo((Int32)Elemento);
    }

    public override Int32 FncComparar(object O1, object O2)
    {
      return ((Int32)O1).CompareTo((Int32)O2);
    }

    public override object TraducirTexto(string Texto)
    {
      return Int32.Parse(Texto);
    }

    public override void CargarValores(BinaryReader Lector, ClaseVariable ClaseDatos)
    {
      Int32 CantValores = Lector.ReadInt32();
      mValores.Clear();
      for (Int32 i = 0; i < CantValores; i++)
      {
        mValores.Add(Lector.ReadInt32());
      }
    }

    public void ObtenerExtremos(out double Minimo, out double Maximo)
    {
      Minimo = double.MaxValue;
      Maximo = double.MinValue;
      foreach (Int32 Valor in mValores)
      {
        Minimo = Math.Min(Valor, Minimo);
        Maximo = Math.Max(Valor, Maximo);
      }

      if (Minimo > Maximo)
      {
        Minimo = 0;
        Maximo = 0;
      }
      else
      {
        CRutinas.AjustarExtremosEscala(ref Minimo, ref Maximo);
      }
    }

  }

  public class CParEnteros
  {
    public Int32 Codigo1;
    public Int32 Codigo2;
  }

  public class CParesEnteros
  {
    public List<Int32> Pares;

    public CParesEnteros()
    {
      Pares = new List<Int32>();
    }

  }

  public class CLineaComprimida
  {
    private bool mbVigente;
    private List<Int32> mReferencias;

    public CLineaComprimida()
    {
      mbVigente = true;
      mReferencias = new List<int>();
    }

    public bool Vigente
    {
      get { return mbVigente; }
      set { mbVigente = value; }
    }

    public List<Int32> Codigos
    {
      get { return mReferencias; }
      set { mReferencias = value; }
    }

    public void PonerCodigosDesdeTextos(List<CColumnaBase> Columnas, List<string> Textos)
    {
      for (Int32 i = 0; i < Textos.Count; i++)
      {
        string Texto = Textos[i];
        mReferencias.Add(Columnas[i].PosicionValorIgualTexto(Texto));
      }
    }

    public void AgregarDatoTexto(string Texto, CColumnaBase Columna)
    {
      mReferencias.Add(Columna.AgregarValorTexto(Texto));
    }

    public CLineaComprimida CopiarLinea()
    {
      CLineaComprimida Respuesta = new CLineaComprimida();
      Respuesta.Vigente = mbVigente;
      Respuesta.Codigos.AddRange(mReferencias);
      return Respuesta;
    }

    public static List<CLineaDatos> ArmarDatos(List<CColumnaBase> Columnas,
          List<CLineaComprimida> Lineas)
    {
      List<CLineaDatos> Respuesta = new List<CLineaDatos>();
      foreach (CLineaComprimida Linea in Lineas)
      {
        CLineaDatos LineaTextos = new CLineaDatos();
        LineaTextos.Contenidos = new List<string>();
        for (Int32 i = 0; i < Linea.Codigos.Count; i++)
        {
          switch (Columnas[i].Clase)
          {
            //case ClaseVariable.Fecha:
            //  LineaTextos.Contenidos.Add(
            //      CRutinas.CodificarFechaHora((DateTime)Columnas[i].Valores[Linea.Codigos[i]]));
            //  break;
            case ClaseVariable.Real:
              LineaTextos.Contenidos.Add(
                  CRutinas.FloatVStr((double)Columnas[i].Valores[Linea.Codigos[i]]));
              break;
            case ClaseVariable.Entero:
              LineaTextos.Contenidos.Add(
                  ((Int32)Columnas[i].Valores[Linea.Codigos[i]]).ToString());
              break;
            default:
              LineaTextos.Contenidos.Add(Columnas[i].ListaValores[Linea.Codigos[i]]);
              break;
          }
        }
        Respuesta.Add(LineaTextos);
      }
      return Respuesta;
    }

  }

  public class CCodigosColumna
  {
    public List<Int32> Codigos;

    public CCodigosColumna()
    {
      Codigos = new List<int>();
    }
  }
  public enum ClaseColumna
  {
    Texto = 1,
    Booleana = 2,
    Entera = 3,
    Real = 4,
    Fecha = 5
  }

  public delegate void FncAjustarDependientes(object sender);

}
