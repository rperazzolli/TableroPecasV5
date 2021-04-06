using System;
using System.Net;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Datos
{

  /// <summary>
  /// Es una condicion independiente.
  /// </summary>
  public class CCondicion
  {

    public Int32 ColumnaCondicion;
    public ClaseVariable Clase;
    public ModoFiltrar Modo;
    public string ValorIgual;
    public double ValorMinimo;
    public double ValorMaximo;
    private Int32 mPosIgual;
    private Int32 mPosMinimo;
    private Int32 mPosMaximo;

    public CCondicion()
    {
      ValorIgual = null;
      ValorMinimo = double.NaN;
      ValorMaximo = double.NaN;
    }

    private Int32 BuscarRangoMinimo(CColumnaBase Columna, double Valor)
    {
      if (double.IsNaN(Valor))
      {
        return -1;
      }
      else
      {
        Int32 Pos0 = Columna.PosicionValorMayorIgualDoble(Valor);
        // si no coinciden (no esta dentro del listado)
        if (Modo == ModoFiltrar.Mayor && !Columna.ValorRealCoincide(Valor, Pos0))
        {
          switch (Modo)
          {
            case ModoFiltrar.MenorIgual:
            case ModoFiltrar.Mayor:
              Pos0--;
              break;
          }
        }
        return Pos0;
      }
    }

    public static CCondicionFiltroCN ExtraerCondicion(bool IncluyeCondiciones, Int32 Pos, Int32 SubPos,
          CCondicion Cnd, string NombreColumna)
    {
      CCondicionFiltroCN Respuesta = new CCondicionFiltroCN();
      Respuesta.BlockCondiciones = Pos;
      Respuesta.OrdenEvaluacion = SubPos;
      Respuesta.DebeCumplirTodasEnBlock = true;
      Respuesta.IncluyeALasQueCumplen = IncluyeCondiciones;
      Respuesta.ModoDeFiltrar = Cnd.Modo;
      Respuesta.CampoCondicion = NombreColumna;
      Respuesta.ValorTexto = Cnd.ValorIgual ?? "";
      Respuesta.ValorMinimo = Cnd.ValorMinimo;
      Respuesta.ValorMaximo = Cnd.ValorMaximo;
      return Respuesta;
    }

    private Int32 BuscarRangoMaximo(CColumnaBase Columna, double Valor)
    {
      if (double.IsNaN(Valor))
      {
        return -1;
      }
      else
      {
        Int32 Pos0 = Columna.PosicionValorMayorIgualDoble(Valor);
        // si no coinciden (no esta dentro del listado)
        if (Pos0 >= Columna.Valores.Count || !Columna.ValorRealCoincide(Valor, Pos0))
        {
          Pos0--;
        }
        return Pos0;
      }
    }

    public void AjustarIndices(List<CColumnaBase> Columnas)
    {
      mPosIgual = (ValorIgual == null ? -1 : Columnas[ColumnaCondicion].PosicionValorIgualTexto(ValorIgual));
      // como los rangos pueden estar impuestos, hay que considerar que puede tratarse de valores que
      // no esten dentro del listado de posibles valores de la columna.
      // segun el modo del filtro, puede haber que corregir.
      mPosMinimo = BuscarRangoMinimo(Columnas[ColumnaCondicion], ValorMinimo);
      mPosMaximo = BuscarRangoMaximo(Columnas[ColumnaCondicion], ValorMaximo);
    }

    private double ExtraerValorVariable(string Valor)
    {
      switch (Clase)
      {
        case ClaseVariable.Fecha:
          bool bHayDatos = false;
          DateTime Fecha = CRutinas.DecodificarFecha(Valor, ref bHayDatos);
          if (bHayDatos)
          {
            return Fecha.ToOADate();
          }
          else
          {
            return CRutinas.NoFecha().ToOADate();
          }
        case ClaseVariable.Real:
        case ClaseVariable.Entero:
          return CRutinas.StrVFloat(Valor);
        default:
          throw new Exception("Clase de variable incorrecta");
      }
    }

    public bool CumpleCondicionTexto(Int32 Posicion, ModoFiltrar Modo)
    {
      switch (Modo)
      {
        case ModoFiltrar.Mayor:
          return (Posicion.CompareTo(mPosIgual) > 0);
        case ModoFiltrar.MayorIgual:
          return (Posicion.CompareTo(mPosIgual) >= 0);
        case ModoFiltrar.Menor:
          return (Posicion.CompareTo(mPosIgual) < 0);
        case ModoFiltrar.MenorIgual:
          return (Posicion.CompareTo(mPosIgual) <= 0);
        case ModoFiltrar.Igual:
          return (Posicion.CompareTo(mPosIgual) == 0);
        default:
          throw new Exception("Error en cumple condición");
      }
    }

    public bool CumpleCondicionRango(Int32 Posicion, ModoFiltrar Modo)
    {
      switch (Modo)
      {
        case ModoFiltrar.PorRango:
          return (Posicion >= mPosMinimo && Posicion <= mPosMaximo);
        case ModoFiltrar.Mayor:
          return (Posicion > mPosMinimo);
        case ModoFiltrar.MayorIgual:
          return (Posicion >= mPosMinimo);
        case ModoFiltrar.Menor:
          return (Posicion < mPosMinimo);
        case ModoFiltrar.MenorIgual:
          return (Posicion <= mPosMinimo);
        case ModoFiltrar.Igual:
          return (Posicion == mPosMinimo);
        default:
          throw new Exception("Error en cumple condición");
      }
    }

    public bool CumpleCondicion(CLineaComprimida Linea)
    {
      switch (Modo)
      {
        case ModoFiltrar.PorValor:
          return (mPosIgual == Linea.Codigos[ColumnaCondicion]);
        case ModoFiltrar.PorRango:
        case ModoFiltrar.Mayor:
        case ModoFiltrar.MayorIgual:
        case ModoFiltrar.Menor:
        case ModoFiltrar.MenorIgual:
        case ModoFiltrar.Igual:
          switch (Clase)
          {
            case ClaseVariable.Booleano:
            case ClaseVariable.Texto:
              return CumpleCondicionTexto(Linea.Codigos[ColumnaCondicion],
                  Modo);
            default:
              return CumpleCondicionRango(
                  Linea.Codigos[ColumnaCondicion],
                  Modo);
          }
        default:
          throw new Exception("Modo incorrecto");
      }
    }

    public override string ToString()
    {
      switch (Clase)
      {
        case ClaseVariable.Entero:
        case ClaseVariable.Real:
          if (Modo == ModoFiltrar.PorRango)
          {
            return ValorMinimo + "-" + ValorMaximo;
          }
          else
          {
            return ValorIgual;
          }
        case ClaseVariable.Fecha:
          if (Modo == ModoFiltrar.PorRango)
          {
            return DateTime.FromOADate(ValorMinimo).ToString() +
              "-" + DateTime.FromOADate(ValorMaximo).ToString();
          }
          else
          {
            bool bHayDatos = false;
            return CRutinas.DecodificarFecha(ValorIgual, ref bHayDatos).ToString();
          }
        default:
          return ValorIgual;
      }
    }

  }

  
  /// <summary>
  /// Es un paquete de condiciones. Se cumple si alguna de las condiciones
  /// que incluye se cumple.
  /// </summary>
  public class CCondiciones
  {

    // true: se cumple si alguna OK. false: se cumple si ninguna OK.
    public bool IncluyeCondiciones;
    public bool TodasLasCondiciones;

    private List<CCondicion> mCondiciones;

    public CCondiciones()
    {
      mCondiciones = new List<CCondicion>();
      TodasLasCondiciones = false;
    }

    public List<CCondicion> Condiciones
    {
      get
      {
        return mCondiciones;
      }
      set
      {
        mCondiciones = value;
      }
    }

    public void AgregarCondicionTorta(Int32 Columna,
        ModoFiltrar Modo,
        bool IncluyeValores,
        ClaseVariable Clase,
        List<CDatosTorta> CondicionesTorta)
    {

      IncluyeCondiciones = IncluyeValores;

      foreach (CDatosTorta Dato in CondicionesTorta)
      {
        CCondicion Condicion = new CCondicion();
        Condicion.ColumnaCondicion = Columna;
        Condicion.Modo = Modo;
        Condicion.Clase = Clase;
        if (Modo == ModoFiltrar.PorValor)
        {
          Condicion.ValorIgual = Dato.NombreOriginal;
          Condicion.ValorMinimo = 0;
          Condicion.ValorMaximo = 0;
        }
        else
        {
          Condicion.ValorIgual = "";
          Condicion.ValorMinimo = Dato.MinimoRango;
          Condicion.ValorMaximo = Dato.MaximoRango;
        }
        Condiciones.Add(Condicion);
      }
    }

    public void AgregarCondicionManual(Int32 Columna,
        ModoFiltrar Modo,
        bool IncluyeValores,
        ClaseVariable Clase,
        string Valor, double Minimo, double Maximo)
    {

      IncluyeCondiciones = IncluyeValores;

      CCondicion Condicion = new CCondicion();
      Condicion.ColumnaCondicion = Columna;
      Condicion.Modo = Modo;
      Condicion.Clase = Clase;
      if (Modo == ModoFiltrar.PorValor)
      {
        Condicion.ValorIgual = Valor;
        Condicion.ValorMinimo = 0;
        Condicion.ValorMaximo = 0;
      }
      else
      {
        Condicion.ValorIgual = "";
        Condicion.ValorMinimo = Minimo;
        Condicion.ValorMaximo = Maximo;
      }
      Condiciones.Add(Condicion);
    }

    public void AgregarCondiciones(List<CCondicion> CondicionesExternas,
        bool IncluyeValores)
    {
      mCondiciones = CondicionesExternas;
      IncluyeCondiciones = IncluyeValores;
    }

    public void AgregarCondicion(CCondicion CondicionExterna)
    {
      mCondiciones.Add(CondicionExterna);
    }

    public bool CumpleCondicion(CLineaComprimida Datos)
    {
      foreach (CCondicion Condicion in Condiciones)
      {
        if (TodasLasCondiciones)
        {
          if (!Condicion.CumpleCondicion(Datos))
          {
            return !IncluyeCondiciones;
          }
        }
        else
        {
          if (Condicion.CumpleCondicion(Datos))
          {
            return IncluyeCondiciones; // si incluye OK, else rechaza.
          }
        }
      }

      if (TodasLasCondiciones)
      {
        return IncluyeCondiciones;
      }
      else
      {
        return !IncluyeCondiciones; // si incluye no cumple, else si.
      }

    }

  }

  /// <summary>
  /// Es un escalon en la secuencia de filtrado de datos.
  /// El inicial no contiene condiciones y se van agregando a medida
  /// que se filtran los datos.
  /// Cuando hay multiples condiciones, si vienen de una torta se usa
  /// un OR y si se trata de condiciones manuales un AND.
  /// Constituye los pasos del CFiltradorPasos.
  /// </summary>
  public class CFiltradorStep
  {
    private bool mbCumplirTodas;
    private bool mbImpuestoExternamente;
    private List<CLineaComprimida> mDatos;
    private List<CCondiciones> mCondiciones; // si hay varias, se tiene que cumplir todas.

    public CFiltradorStep()
    {
      mbCumplirTodas = true;
      mbImpuestoExternamente = false;
      mDatos = new List<CLineaComprimida>();
      mCondiciones = new List<CCondiciones>();
    }

    public List<CCondiciones> Condiciones
    {
      get
      {
        return mCondiciones;
      }
    }

    public bool ImpuestoExternamente
    {
      get
      {
        return mbImpuestoExternamente;
      }
      set
      {
        mbImpuestoExternamente = value;
      }
    }

    public bool CumplirTodas
    {
      get
      {
        return mbCumplirTodas;
      }
      set
      {
        mbCumplirTodas = value;
      }
    }

    public List<CLineaComprimida> Datos
    {
      get
      {
        return mDatos;
      }
    }

    public void EliminarCondiciones()
    {
      mCondiciones.Clear();
    }

    public void AgregarCondicionTorta(Int32 Columna,
        ModoFiltrar Modo,
        bool IncluyeValores,
        ClaseVariable Clase,
        List<CDatosTorta> CondicionesTorta)
    {
      mbCumplirTodas = false;
      CCondiciones Condicion = new CCondiciones();
      Condicion.AgregarCondicionTorta(Columna,
          Modo, IncluyeValores, Clase, CondicionesTorta);
    }

    public void AgregarCondicionManual(Int32 Columna,
        ModoFiltrar Modo,
        bool IncluyeValores,
        ClaseVariable Clase,
        string Valor,
        double Minimo, double Maximo)
    {
      CCondiciones Condicion = new CCondiciones();
      Condicion.AgregarCondicionManual(Columna,
          Modo, IncluyeValores, Clase, Valor,
          Minimo, Maximo);

    }

    public void AgregarCondiciones(List<CCondiciones> CondicionesExternas)
    {
      mCondiciones = CondicionesExternas;
    }

    public void AgregarCondicion(CCondiciones CondicionExterna)
    {
      mCondiciones.Add(CondicionExterna);
    }

    private bool CumpleCondicion(CLineaComprimida Linea)
    {
      foreach (CCondiciones Condicion in mCondiciones)
      {
        if (Condicion.CumpleCondicion(Linea))
        {
          if (!mbCumplirTodas)
          {
            return true;
          }
        }
        else
        {
          if (mbCumplirTodas)
          {
            return false;
          }
        }
      }
      return (mCondiciones.Count == 0);
    }

    private void AjustarIndicesCondiciones(List<CColumnaBase> Columnas)
    {
      foreach (CCondiciones Condicion in mCondiciones)
      {
        foreach (CCondicion CondicionUnica in Condicion.Condiciones)
        {
          CondicionUnica.AjustarIndices(Columnas);
        }
      }
    }

    private bool HayCondiciones
    {
      get
      {
        if (mCondiciones != null)
        {
          foreach (CCondiciones C in mCondiciones)
          {
            if (C.Condiciones.Count > 0)
            {
              return true;
            }
          }
        }
        return false;
      }
    }

    /// <summary>
    /// Filtra los datos con las condiciones impuestas. Retorna true si pudo hacerlo.
    /// </summary>
    /// <param name="DatosSinFiltrar"></param>
    /// <param name="Columnas"></param>
    /// <returns></returns>
    public bool FiltrarDatos(List<CLineaComprimida> DatosSinFiltrar,
          List<CColumnaBase> Columnas)
    {
      if (mDatos != null && mCondiciones != null)
      {
        try
        {
          mDatos.Clear();
          if (HayCondiciones)
          {
            AjustarIndicesCondiciones(Columnas);
            foreach (CLineaComprimida Linea in DatosSinFiltrar)
            {
              if (Linea.Vigente && CumpleCondicion(Linea))
              {
                mDatos.Add(Linea);
              }
            }
          }
          else
					{
            mDatos.AddRange((from L in DatosSinFiltrar
                             where L.Vigente
                             select L).ToList());
					}
          return true;
        }
        catch (Exception ex)
        {
          CRutinas.MsgMasReciente = "Error al agregar un paso al filtro" +
              Environment.NewLine +
              CRutinas.MostrarMensajeError(ex);
          return false;
        }
      }
      else
      {
        return true;
      }
    }

  }

}
