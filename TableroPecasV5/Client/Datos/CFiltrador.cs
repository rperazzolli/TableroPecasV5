using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using TableroPecasV5.Client.Plantillas;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Datos
{
  /// <summary>
  /// Cada FiltroTextos contiene un filtrador que lleva el detalle de las condiciones.
  /// </summary>
  public class CFiltrador
  {

    public delegate void FncEventoFiltrador(CFiltrador F);
    public delegate void FncEventoEnteros(CFiltrador F, List<Int32> Codigos);
    public delegate void FncEventoFiltrarPorFila(CFiltrador F, CElementoFilaAsociativa Fila);

//    public event FncEventoFiltrador evFiltrador;
    public event FncEventoEnteros AlImponerFiltrosAsociados;
    public event FncEventoFiltrador evAjustarVisibilidadAND;
    public event FncEventoFiltrador evAjustarVisibilidadPorc;
    public event FncEventoFiltrador evAjustarVisibilidadAlfabetico;
    public event FncEventoFiltrador evAjustarExtremosDesdePantalla;
    public event FncEventoFiltrador evAjustarAnchoPantalla;
    public event FncEventoFiltrador evAjustarValores;
    public event FncEventoFiltrador evOrdenarElementos;
    public event FncEventoFiltrarPorFila evFiltrarFilas;
    //public event FncEventoFiltrador evRefrescarContenido;

    private Int32 mCantidadNecesaria;
    private List<CElementoFilaAsociativa> mFilas; // se mantienen ordenadas por orden.
    private CColumnaBase mColumna;
    //    private CColumnaBase mColumnaPaso;
    private CColumnaBase mColumnaValor;
    private List<string> mValoresSeleccionados; // valores de las columnas seleccionadas.
    private List<Int32> mCodigosSeleccionados; // se recalculan al cambiar de dataset.
                                               //    private List<Int32> mPosSinonimosSeleccionados; // los sinonimos de los que se selecciono algun elemento.
    private List<CRangoFechas> mRangosSeleccionados; // cuando se trata de rangos de fechas.
    private string mszValorMinimo; // se mantiene al cambiar de dataset.
    private string mszValorMaximo;
    private Int32 mCodigoMinimo; // unicamente cuando se trabaje con rangos (reales o fechas).
    private Int32 mCodigoMaximo;
    private CProveedorComprimido mProveedor;

    public bool Habilitado { get; set; }
    //public List<CFiltradorStep> FiltrosBlocks { get; set; }

    public CFiltrador()
    {
      mFilas = new List<CElementoFilaAsociativa>();
      mValoresSeleccionados = new List<string>();
      mCodigosSeleccionados = new List<int>();
      //      mPosSinonimosSeleccionados = new List<int>();
      mRangosSeleccionados = new List<CRangoFechas>();
      mszValorMinimo = "";
      mszValorMaximo = "";
      mCodigoMinimo = -1;
      mCodigoMaximo = -1;
      mCantidadNecesaria = -1;
      Habilitado = true;
    }

    /// <summary>
    /// Usarlo unicamente cuando se hagan filtros sobre conjuntos de proveedores.
    /// </summary>
    /// <param name="Otro"></param>
    public CFiltrador(CFiltrador Otro, CProveedorComprimido ProvPropio)
    {
      mColumna = ProvPropio.ColumnaNombre(Otro.Columna.Nombre);
      //      mColumnaPaso = (Otro.ColumnaPaso == null ? null : ProvPropio.ColumnaNombre(Otro.ColumnaPaso.Nombre));
      mColumnaValor = (Otro.ColumnaValor == null ? null : ProvPropio.ColumnaNombre(Otro.ColumnaValor.Nombre));
      mValoresSeleccionados = Otro.ValoresSeleccionados;
      mCodigosSeleccionados = new List<int>();
      //      mPosSinonimosSeleccionados = new List<int>();
      mRangosSeleccionados = Otro.RangosSeleccionados;
      mszValorMaximo = Otro.ValorMaximo;
      mszValorMinimo = Otro.ValorMinimo;
      mCantidadNecesaria = Otro.CantidadNecesaria;
      mProveedor = ProvPropio; // no se copia desde el otro.
      mFilas = new List<CElementoFilaAsociativa>();
      Habilitado = true;
      CompatibilizarCodigos(Otro);
    }

    public void AjustarFiltrosAsociados(List<Int32> CodigosAsociados)
    {
      if (AlImponerFiltrosAsociados != null)
      {
        AlImponerFiltrosAsociados(this, CodigosAsociados);
      }
    }

    public void AjustarAnchoPantallaContenedor()
    {
      if (evAjustarAnchoPantalla != null)
      {
        evAjustarAnchoPantalla(this);
      }
    }

    public void AjustarValores()
    {
      if (evAjustarValores != null)
      {
        evAjustarValores(this);
      }
      if (ColumnaValor != null)
      {
        if (evAjustarValores != null)
        {
          evAjustarValores(this);
        }
      }
      else
      {
        if (evOrdenarElementos != null)
        {
          evOrdenarElementos(this);
        }
      }
    }

    public void FiltrarPorFila(CElementoFilaAsociativa Fila)
    {
      if (evFiltrarFilas != null)
      {
        evFiltrarFilas(this, Fila);
      }
    }

    //public void RefrescarContenido()
    //{
    //  if (evRefrescarContenido != null)
    //  {
    //    evRefrescarContenido(this);
    //  }
    //}

    public CColumnaBase Columna
    {
      get { return mColumna; }
      set { mColumna = value; }
    }

    public CProveedorComprimido Proveedor
    {
      get { return mProveedor; }
      set { mProveedor = value; }
    }

    public CCondiciones ExtraerCondiciones()
    {
      //if (mColumnaValor != null)
      //{
      //  throw new Exception("No soporta condiciones AND");
      //}

      if (mColumna.Clase == ClaseVariable.Fecha)
      {
        throw new Exception("No imponer condiciones en fechas");
      }

      CCondiciones Respuesta = new CCondiciones();
      Respuesta.TodasLasCondiciones = false;
      Respuesta.IncluyeCondiciones = true;
      switch (mColumna.Clase)
      {
        case ClaseVariable.Real:
          CCondicion CndValor = new CCondicion();
          CndValor.Clase = mColumna.Clase;
          CndValor.ColumnaCondicion = mColumna.Orden;
          CndValor.Modo = ModoFiltrar.PorRango;
          CndValor.ValorMaximo = CRutinas.StrVFloat(mszValorMaximo);
          CndValor.ValorMinimo = CRutinas.StrVFloat(mszValorMinimo);
          CndValor.ValorIgual = "";
          Respuesta.Condiciones.Add(CndValor);
          break;
        case ClaseVariable.Entero:
        case ClaseVariable.Booleano:
        case ClaseVariable.Texto:
          foreach (string Valor in mValoresSeleccionados)
          {
            CCondicion CndIgual = new CCondicion();
            CndIgual.Clase = mColumna.Clase;
            CndIgual.ColumnaCondicion = mColumna.Orden;
            CndIgual.Modo = ModoFiltrar.Igual;
            CndIgual.ValorIgual = Valor;
            Respuesta.Condiciones.Add(CndIgual);
          }
          break;
      }

      return Respuesta;

    }

    //public IndicadoresV2.CColumnaBase ColumnaPaso
    //{
    //  get { return mColumnaPaso; }
    //  set { mColumnaPaso = value; }
    //}

    public ModoAgruparDependiente ModoAgrupar { get; set; } = ModoAgruparDependiente.NoDefinido;

    public CColumnaBase ColumnaValor
    {
      get { return mColumnaValor; }
      set
      {
        EliminarAsociacionValor();
        if (mColumnaValor != null)
        {
          CFiltrador FiltroValor = mProveedor.FiltroParaColumna(mColumnaValor.Nombre);
          if (FiltroValor != null)
          {
            if (AlImponerFiltrosAsociados != null)
            {
              AlImponerFiltrosAsociados(this, new List<Int32>());
            }
//            FiltroValor.Filtro.ImponerFiltrosAsociados(new List<int>());
          }
        }
        mColumnaValor = value;
        if (evAjustarVisibilidadAND != null)
        {
          evAjustarVisibilidadAND(this);
        }
        AjustarVisibilidadPorcAlfabetico();
      }
    }

    public CGrupoCondicionesBlock ObtenerCondicionBlock()
    {

      CGrupoCondicionesBlock Respuesta = new CGrupoCondicionesBlock();

      Respuesta.CantidadMinima = mCantidadNecesaria;
      Respuesta.CumplirTodas = false;
      Respuesta.Incluye = true;

      switch (mColumna.Clase)
      {
        case ClaseVariable.Real:
          CCondicionBlock Cnd = new CCondicionBlock();
          Cnd.Columna = mColumna.Nombre;
          Cnd.Modo = ModoFiltrar.PorRango;
          Cnd.Valor = (mszValorMinimo.Length == 0 ? CRutinas.FloatVStr(double.MinValue) : mszValorMinimo);
          Cnd.ValorMaximo = (mszValorMaximo.Length == 0 ? CRutinas.FloatVStr(double.MaxValue) : mszValorMaximo);
          Respuesta.Condiciones.Add(Cnd);
          return Respuesta;
        case ClaseVariable.Fecha:
          AjustarCodigosRangos();
          if (mszValorMinimo.Length > 0 && mszValorMaximo.Length > 0)
          {
            CCondicionBlock Cnd2G = new CCondicionBlock();
            Cnd2G.Columna = mColumna.Nombre;
            Cnd2G.Modo = ModoFiltrar.PorRango;
            Cnd2G.Valor = mszValorMinimo;
            Cnd2G.ValorMaximo = mszValorMaximo;
            Respuesta.Condiciones.Add(Cnd2G);
          }
          foreach (CRangoFechas Rango in mRangosSeleccionados)
          {
            CCondicionBlock Cnd2 = new CCondicionBlock();
            Cnd2.Columna = mColumna.Nombre;
            Cnd2.Modo = ModoFiltrar.PorRango;
            Cnd2.Valor = CRutinas.CodificarFechaHora(Rango.Desde);
            Cnd2.ValorMaximo = CRutinas.CodificarFechaHora(Rango.Hasta);
            Respuesta.Condiciones.Add(Cnd2);
          }
          return Respuesta;
      }

      Respuesta.CantidadMinima = mCantidadNecesaria;

      foreach (string Valor in ValoresSeleccionados)
      {
        CCondicionBlock CndLocal = new CCondicionBlock();
        CndLocal.Columna = mColumna.Nombre;
        CndLocal.Modo = ModoFiltrar.Igual;
        CndLocal.Valor = Valor;
        Respuesta.Condiciones.Add(CndLocal);
      }

      return Respuesta;

    }

    public CCondicionFiltradorCN ObtenerCondicion()
    {

      CCondicionFiltradorCN Respuesta = new CCondicionFiltradorCN();

      Respuesta.NombreColumna = mColumna.Nombre;
      Respuesta.Coincidencias = mCantidadNecesaria;
      Respuesta.NombreColumnaAND = "";

      switch (mColumna.Clase)
      {
        case ClaseVariable.Real:
          Respuesta.RangoMinimo = (mszValorMinimo.Length == 0 ? double.NaN :
                CRutinas.StrVFloat(mszValorMinimo));
          Respuesta.RangoMaximo = (mszValorMaximo.Length == 0 ? double.NaN :
                CRutinas.StrVFloat(mszValorMaximo));
          break;
        case ClaseVariable.Fecha:
          AjustarCodigosRangos();
          Respuesta.RangoMinimo = (mszValorMinimo.Length == 0 ? double.NaN :
              CRutinas.DecodificarFechaHora(mszValorMinimo).ToOADate());
          Respuesta.RangoMaximo = (mszValorMaximo.Length == 0 ? double.NaN :
              CRutinas.DecodificarFechaHora(mszValorMaximo).ToOADate());
          break;
        default:
          Respuesta.RangoMinimo = double.NaN;
          Respuesta.RangoMaximo = double.NaN;
          break;
      }

      Respuesta.ValoresImpuestos.AddRange(mColumna.ListaValores);

      return Respuesta;

    }

    private void AjustarVisibilidadPorcAlfabetico()
    {
      if (evAjustarVisibilidadPorc!=null)
      {
        evAjustarVisibilidadPorc(this);
      }
      if (evAjustarVisibilidadAlfabetico != null)
      {
        evAjustarVisibilidadAlfabetico(this);
      }
    }

    public List<CElementoFilaAsociativa> Filas
    {
      get { return mFilas; }
      set { mFilas = value; }
    }

    public Int32 CantidadNecesaria
    {
      get { return mCantidadNecesaria; }
      set { mCantidadNecesaria = value; }
    }

    public List<Int32> CodigosSeleccionados
    {
      get { return mCodigosSeleccionados; }
      set { mCodigosSeleccionados = value; }
    }

    public List<string> ValoresSeleccionados
    {
      get { return mValoresSeleccionados; }
      set { mValoresSeleccionados = value; }
    }

    public List<CRangoFechas> RangosSeleccionados
    {
      get { return mRangosSeleccionados; }
      set { mRangosSeleccionados = value; }
    }

    public string ValorMinimo
    {
      get { return mszValorMinimo; }
      set { mszValorMinimo = value; }
    }

    public string ValorMaximo
    {
      get { return mszValorMaximo; }
      set { mszValorMaximo = value; }
    }

    public Int32 CodigoMinimo
    {
      get { return mCodigoMinimo; }
      set { mCodigoMinimo = value; }
    }

    public Int32 CodigoMaximo
    {
      get { return mCodigoMaximo; }
      set { mCodigoMaximo = value; }
    }

    public bool HayCondicion
    {
      get
      {
        switch (mColumna.Clase)
        {
//          case ClaseVariable.Fecha:
          case ClaseVariable.Real:
            return (mCodigoMinimo >= 0 || mCodigoMaximo >= 0);
          default:
            //          case ClaseVariable.Entero:
            return (mValoresSeleccionados.Count > 0);
        }
      }
    }

    private bool mbMostrarPorcentaje = true;
    public bool MostrarPorcentaje
    {
      get { return mbMostrarPorcentaje; }
      set { mbMostrarPorcentaje = value; }
    }

    public void SimularSeleccion(string Texto)
    {
      if (!mValoresSeleccionados.Contains(Texto))
      {
        mValoresSeleccionados.Add(Texto);
      }
      CElementoFilaAsociativa Fila = (from F in Filas
                                      where F.Nombre == Texto
                                      select F).FirstOrDefault();
      if (Fila != null)
      {
        Fila.Seleccionado = true;
      }
    }

    public void AjustarPorcentajes()
    {

      double Total = (from F in mFilas
                      where !double.IsNaN(F.Valor) && F.Valor > 0
                      select F.Valor).Sum();
      foreach (CElementoFilaAsociativa Fila in mFilas)
      {
        Fila.Porcentaje = (mbMostrarPorcentaje ?
          ((!double.IsNaN(Fila.Valor) && Fila.Valor > 0) ? (100 * Fila.Valor / Total) : 0) : 0);
      }
    }

    public void LimpiarCondiciones()
    {
      mValoresSeleccionados.Clear();
      mCodigosSeleccionados.Clear();
      //      mPosSinonimosSeleccionados.Clear();
      mRangosSeleccionados.Clear();
      mszValorMinimo = "";
      mszValorMaximo = "";
      mCodigoMinimo = -1;
      mCodigoMaximo = -1;
      mCantidadNecesaria = -1;
    }

    public double SumaValores()
    {
      double Respuesta = 0;
      foreach (CElementoFilaAsociativa Fila in mFilas)
      {
        Respuesta += Fila.Valor;
      }
      return Respuesta;
    }

    public void AjustarSeleccionPorcentaje(double Porc, List<CElementoFilaAsociativa> FilasOrdenadas)
    {
      List<CElementoFilaAsociativa> Locales = new List<CElementoFilaAsociativa>();
      Locales.AddRange(FilasOrdenadas);
      Locales.Sort(delegate (CElementoFilaAsociativa F1, CElementoFilaAsociativa F2)
      {
        return F2.Valor.CompareTo(F1.Valor);
      });

      Porc *= SumaValores() / 100;
      double Suma = 0;
      foreach (CElementoFilaAsociativa Fila in Locales)
      {
        Fila.Seleccionado = (Porc > 0 && Suma < Porc);
        Suma += Fila.Valor;
      }
      mProveedor.FiltrarPorAsociaciones();
    }

    public string TextoCondicion()
    {
      string Respuesta = "";
      if (mValoresSeleccionados.Count != 0)
      {
        foreach (string Valor in mValoresSeleccionados)
        {
          Respuesta += (Respuesta.Length > 0 ? " o " : "") + Valor;
        }
        Respuesta = mColumna.Nombre + " = " + Respuesta;
        if (mColumnaValor != null)
        {
          Respuesta += " Vinculados por " + mColumnaValor.Nombre + " (" + mCantidadNecesaria.ToString() + ")";
        }
      }
      return Respuesta;
    }

    public void RefrescarSelecciones()
    {
      AjustarCodigosSeleccionados();
      //      AjustarCodigosSinonimos();
      if (evAjustarExtremosDesdePantalla != null)
      {
        evAjustarExtremosDesdePantalla(this);
      }
      AjustarCodigosRangos();
    }

    public void AjustarCodigosValoresSeleccionados()
    {
      if (DatoEsFecha())
      {
        CrearFilasFecha();
      }
      else
      {
        mCodigosSeleccionados.Clear();
        //        mPosSinonimosSeleccionados.Clear();
        foreach (string Valor in mValoresSeleccionados)
        {
          Int32 ValorI = CodigoValor(Valor);
          if (ValorI >= 0 && ValorI < mColumna.ListaValores.Count &&
              mColumna.ListaValores[ValorI] == Valor)
          {
            mCodigosSeleccionados.Add(ValorI);
          }
          //          AgregarSinonimoValor(Valor);
        }
      }
    }

    public void CrearInformacionFilas()
    {
      mFilas.Clear();
      Int32 Pos = 0;
      foreach (string Texto in mColumna.ListaValores)
      {
        AgregarFila(Texto, Pos++);
      }
      AjustarCodigosSeleccionados();
    }

    private void CompatibilizarCodigos(CFiltrador Otro)
    {
      // ajustar las filas en base a los valores de la columna.
      CrearInformacionFilas();

      // ajusta los codigos seleccionados.
      AjustarCodigosValoresSeleccionados();

      // ajustar los codigos de los sinonimos.
      //      AjustarCodigosSinonimos();
    }

    public bool DatoEsFecha()
    {
      return (mColumna == null ? false : mColumna.Clase == ClaseVariable.Fecha);
    }

    public void AjustarCodigosRangos()
    {
      if (DatoEsFecha())
      {
        if (mszValorMinimo.Length > 0)
        {
          mszValorMinimo = CRutinas.CodificarFechaHora(
                CRutinas.FechaDesdeTexto(mszValorMinimo));
        }
        if (mszValorMaximo.Length > 0)
        {
          mszValorMaximo = CRutinas.CodificarFechaHora(
                CRutinas.FechaDesdeTexto(mszValorMaximo));
        }
      }
      mCodigoMinimo = (mszValorMinimo.Length == 0 ? -1 : CodigoValor(mszValorMinimo));
      mCodigoMaximo = (mszValorMaximo.Length == 0 ? mColumna.ListaValores.Count :
            CodigoValor(mszValorMaximo, true));
    }

    public Int32 CodigoValor(string Valor, bool MenorIgual = false)
    {
      if (Valor.Length == 0)
      {
        return (mColumna.ListaValores[0].Length == 0 ? 0 : -1);
      }
      else
      {
        return (mColumna.ListaValores.Count == 0 ? -1 :
          (MenorIgual ? mColumna.PosicionValorMenorIgual(Valor, false) :
          mColumna.PosicionValorMayorIgualTexto(Valor, false)));
      }
    }

    public void AjustarRangosSeleccionados()
    {
      mRangosSeleccionados.Clear();
      foreach (string Valor in mValoresSeleccionados)
      {
        for (Int32 i = 0; i < mFilas.Count; i++)
        {
          CElementoFilaAsociativaFecha Elemento = (CElementoFilaAsociativaFecha)mFilas[i];
          if (Elemento.Nombre == Valor)
          {
            CRangoFechas Rango = new CRangoFechas();
            Rango.Posicion = i;
            Rango.PosMinima = Elemento.IndiceMinimo;
            Rango.PosMaxima = Elemento.IndiceMaximo;
            Rango.Desde = Elemento.FechaMinima;
            Rango.Hasta = Elemento.FechaMaxima;
            mRangosSeleccionados.Add(Rango);
          }
        }
      }
    }

    /// <summary>
    /// Ajusta los codigos seleccionados y se asegura de que las listas de
    /// valores seleccionados sean coherentes.
    /// </summary>
    public void AjustarCodigosSeleccionados()
    {
      if (DatoEsFecha())
      {
        AjustarRangosSeleccionados();
      }
      else
      {
        mCodigosSeleccionados.Clear();
        mValoresSeleccionados.Clear();
        //        mPosSinonimosSeleccionados.Clear();
        foreach (CElementoFilaAsociativa Fila in mFilas)
        {
          if (Fila.Seleccionado)
          {
            mCodigosSeleccionados.Add(CodigoValor(Fila.Nombre));
            if (!mValoresSeleccionados.Contains(Fila.Nombre))
            {
              mValoresSeleccionados.Add(Fila.Nombre);
            }
            //            AgregarSinonimoValor(Fila.Nombre);
          }
          else
          {
            if (mValoresSeleccionados.Contains(Fila.Nombre))
            {
              mValoresSeleccionados.Remove(Fila.Nombre);
            }
          }
        }
      }
    }

    private CElementoFilaAsociativaFecha SumarElementoFecha(
        string Nombre, DateTime Desde, DateTime Hasta, Int32 Nivel,
        CElementoFilaAsociativaFecha Superior)
    {
      CElementoFilaAsociativaFecha Elemento = new CElementoFilaAsociativaFecha(Nivel, Superior);
      Elemento.Nombre = Nombre;
      Elemento.IndiceMinimo = ObtenerIndiceFecha(Desde, false);
      Elemento.IndiceMaximo = ObtenerIndiceFecha(Hasta, true);
      Elemento.FechaMinima = Desde;
      Elemento.FechaMaxima = Hasta;
      mFilas.Add(Elemento);
      return Elemento;
    }

    /// <summary>
    /// Adapta la informacion de las filas al contenido de valores de la columna.
    /// </summary>
    public void AjustarInformacionFilas()
    {
      // primero elimina las filas que no estan en los valores de la columna.
      for (Int32 i = mFilas.Count - 1; i >= 0; i--)
      {
        if (!mColumna.ListaValores.Contains(mFilas[i].Nombre))
        {
          mFilas.RemoveAt(i);
        }
      }

      // ahora agrega filas para los valores de columna que faltan.
      for (Int32 i = 0; i < mColumna.ListaValores.Count; i++)
      {
        string Valor = mColumna.ListaValores[i];
        if (i >= Filas.Count || Filas[i].Nombre != Valor)
        {
          AgregarFila(Valor, i);
        }
      }
    }

    public bool PosibleCondicionAND()
    {
      if (mColumnaValor != null)
      {
        switch (mColumna.Clase)
        {
          case ClaseVariable.Texto:
          case ClaseVariable.Fecha:
          case ClaseVariable.Entero:
            switch (mColumnaValor.Clase)
            {
              case ClaseVariable.Entero:
              case ClaseVariable.Fecha:
              case ClaseVariable.Texto:
                return true;
            }
            break;
        }
      }
      return false;
    }

    private Int32 ObtenerIndiceFecha(DateTime Fecha, bool MenorIgual)
    {
      return CodigoValor(CRutinas.CodificarFechaHora(Fecha), MenorIgual);
    }

    private void SumarTrimestre(Int32 MesI, Int32 MesH, Int32 Anio,
          Int32 AnioMin, Int32 MesMin, Int32 AnioMax, Int32 MesMax,
          CElementoFilaAsociativaFecha Superior)
    {
      if ((Anio > AnioMin || MesMin <= MesH) && (Anio < AnioMax || MesI <= MesMax))
      {
        for (Int32 Mes = MesI; Mes <= MesH; Mes++)
        {
          if ((Anio > AnioMin || MesMin <= Mes) && (Anio < AnioMax || Mes <= MesMax))
          {
            CElementoFilaAsociativaFecha MesSuperior = SumarElementoFecha("Mes " + Mes.ToString() + "/" + Anio.ToString(),
                  new DateTime(Anio, Mes, 1, 0, 0, 0),
                  new DateTime(Anio, Mes, 1, 0, 0, 0).AddMonths(1).AddSeconds(-1), 1, Superior);
            DateTime Fecha = new DateTime(Anio, Mes, 1);
            for (Int32 Dia = 1; Dia <= new DateTime(Anio, Mes, 1, 0, 0, 0).AddMonths(1).AddSeconds(-1).Day; Dia++)
            {
              SumarElementoFecha("Dia " + Dia.ToString() + "/" + Mes.ToString() + "/" + Anio.ToString(),
                    new DateTime(Anio, Mes, Dia, 0, 0, 0),
                    new DateTime(Anio, Mes, Dia, 0, 0, 0).AddDays(1).AddSeconds(-1), 2, MesSuperior);
            }
          }
        }
      }
    }

    private void SumarItemsAnio(Int32 Anio, Int32 AnioMin, Int32 MesMin, Int32 AnioMax, Int32 MesMax,
        CElementoFilaAsociativaFecha Contenedor)
    {
      CElementoFilaAsociativaFecha Elemento =
        SumarElementoFecha(Anio.ToString(), new DateTime(Anio, 1, 1, 0, 0, 0),
           new DateTime(Anio, 12, 31, 23, 59, 59), 0, null);
      SumarTrimestre(1, 3, Anio, AnioMin, MesMin, AnioMax, MesMax, Elemento);
      SumarTrimestre(4, 6, Anio, AnioMin, MesMin, AnioMax, MesMax, Elemento);
      SumarTrimestre(7, 9, Anio, AnioMin, MesMin, AnioMax, MesMax, Elemento);
      SumarTrimestre(10, 12, Anio, AnioMin, MesMin, AnioMax, MesMax, Elemento);
    }

    private void AjustarElementosSeleccionados(string Nombre, Int32 Posicion, bool Selecciona)
    {
      if (Selecciona)
      {
        mValoresSeleccionados.Add(Nombre);
      }
      else
      {
        mValoresSeleccionados.Remove(Nombre);
      }
    }

    public void AgregarFila(string Nombre, Int32 CodigoOrden, Int32 Posicion = -1)
    {
      CElementoFilaAsociativa Elemento = new CElementoFilaAsociativa();
      Elemento.CodigoOrden = CodigoOrden;
      Elemento.Nombre = Nombre;
      Elemento.Seleccionado = mValoresSeleccionados.Contains(Nombre);
      Elemento.Vigente = true;
      if (Posicion >= 0)
      {
        mFilas.Insert(Posicion, Elemento);
      }
      else
      {
        mFilas.Add(Elemento);
      }
    }

    private DateTime FechaMinimaAceptable()
    {
      return mColumna.PrimeraFechaValida();
    }

    private void PonerVisiblesMellizos(CElementoFilaAsociativaFecha Fila)
    {
      foreach (CElementoFilaAsociativaFecha FilaL in Filas)
      {
        if (FilaL.Dependientes.Contains(Fila))
        {
          foreach (CElementoFilaAsociativaFecha Mellizo in FilaL.Dependientes)
          {
            Mellizo.Visible = true;
          }
        }
      }
    }

    private void PonerSuperioresVisibles(CElementoFilaAsociativaFecha Fila)
    {
      foreach (CElementoFilaAsociativaFecha FilaL in Filas)
      {
        if (FilaL.Dependientes.Contains(Fila))
        {
          FilaL.Visible = true;
          PonerSuperioresVisibles(FilaL);
          break;
        }
      }
    }

    public void CrearFilasFecha()
    {
      Filas.Clear();
      if (mColumna.ListaValores.Count > 0)
      {
        DateTime FMin = FechaMinimaAceptable();
        if (FMin.Year < 1901)
        {
          return;
        }
        DateTime FMax = CRutinas.DecodificarFecha(
              mColumna.ListaValores[mColumna.ListaValores.Count - 1]);
        Int32 AnioMin = FMin.Year;
        Int32 MesMin = FMin.Month;
        Int32 AnioMax = FMax.Year;
        Int32 MesMax = FMax.Month;
        // Armar lista de elementos anuales.
        for (Int32 Anio = AnioMin; Anio <= AnioMax; Anio++)
        {
          SumarItemsAnio(Anio, AnioMin, MesMin, AnioMax, MesMax, null);
        }

        foreach (CElementoFilaAsociativaFecha Fila in Filas)
        {
          Fila.Seleccionado = mValoresSeleccionados.Contains(Fila.Nombre);
          if (Fila.Seleccionado)
          {
            Fila.Visible = true;
            PonerVisiblesMellizos(Fila);
            PonerSuperioresVisibles(Fila);
          }
        }

      }
    }

    private void AjustarVigenciaLinea(CFiltrador Filtro,
          CLineaComprimida Linea, bool CndAND,
          CFiltrador ColumnaPaso,
          List<Int32> CodigosSeleccionados)
    {
      Linea.Vigente = Filtro.HabilitarLinea(Linea, CndAND, ColumnaPaso, CodigosSeleccionados);
    }

    private List<Int32> DeterminarCodigosSeleccionadosSinSinonimos()
    {
      // Obtener lista de elementos.
      List<Int32> Respuesta = new List<int>();
      Respuesta.AddRange(mCodigosSeleccionados);

      //// Obtener lista de sinonimos.
      //for (Int32 i = Respuesta.Count - 1; i >= 0; i--)
      //{
      //  Int32 CodigoSinonimo = PosicionSinonimoParaElemento(Respuesta[i]);
      //  if (CodigoSinonimo >= 0)
      //  {
      //    Respuesta.RemoveAt(i);
      //  }
      //}

      return Respuesta;

    }

    /// <summary>
    /// Ajusta la vigencia de las lineas del dataset.
    /// </summary>
    /// <param name="Filtro"></param>
    /// <returns></returns>
    private bool AjustarVigenciaLineasPorFiltro(CFiltrador Filtro)
    {
      bool bRetorno = false;
      bool CndAND = Filtro.CondicionAND;
      CColumnaBase ColComprimida = Filtro.ColumnaValor; // Filtro.ColumnaPaso;
      CFiltrador FiltroAsoPaso =
        (ColComprimida == null ? null :
        mProveedor.ObtenerFiltroColumna(ColComprimida.Nombre));

      List<Int32> CodigosSeleccionados = (DatoEsFecha() ? null :
          DeterminarCodigosSeleccionadosSinSinonimos());

      if (DatosBase != null)
      {
        foreach (CLineaComprimida Linea in DatosBase)
        {
          if (Linea.Vigente)
          {
            AjustarVigenciaLinea(Filtro, Linea, CndAND, FiltroAsoPaso, CodigosSeleccionados);
            bRetorno = bRetorno || (!Linea.Vigente);
          }
        }
      }
      return bRetorno;
    }

    private bool FiltrarDatasetANDFechas()
    {
      // Obtener lista de elementos.
      List<Int32> CodigosLineas = new List<int>();
      CodigosLineas.AddRange(mCodigosSeleccionados);

      // Crear elementos de verificacion.
      CFiltrador FiltroPaso = mProveedor.ObtenerFiltroColumna(mColumnaValor.Nombre);
      List<CElementoFilaAND> ListaPaso = ListaElementosAND(FiltroPaso,
        mRangosSeleccionados.Count + (HayRango() ? 1 : 0));

      // Ajusta los elementos de la columna de paso que estan OK.
      AjustarFiltrosANDFechas(FiltroPaso, ListaPaso);

      // Aca se llego a saber que lineas de la columna de paso estan asociadas a que columnas o sinonimos
      // dedinidos.
      // Ahora hay que determinar los que cumplen la condicion de cantidad y despues
      // agregar una lista a la funcion FiltrarDatasetOR de FiltroPaso.
      FiltroPaso.AjustarVigentePorAND(mCantidadNecesaria, ListaPaso);

      // Ahora lo que hay que hacer es ajustar la vigencia de las filas del dataset.
      bool bRetorno = AjustarVigenciaLineasPorFiltro(this);

      // Ahora se ajusta la vigencia de los elementos de la columna propia.
      AjustarVigenciaPropiaPorANDFecha();

      return bRetorno;

    }

    private void AjustarFiltrosANDFechas(CFiltrador FiltroPaso, List<CElementoFilaAND> ListaPaso)
    {
      Int32 OrdenPaso = FiltroPaso.Columna.Orden;
      Int32 OrdenLocal = mColumna.Orden;
      bool HayRangoLocal = HayRango();

      foreach (CLineaComprimida Linea in DatosBase)
      {
        if (Linea.Vigente)
        {
          Int32 PosOtra = Linea.Codigos[OrdenPaso];
          List<Int32> Posiciones = RangosFechaContenedora(Linea.Codigos[OrdenLocal]);
          foreach (Int32 i in Posiciones)
          {
            ListaPaso[PosOtra].Incluidos[i] = true;
          }
          if (HayRangoLocal)
          {
            if (CumpleCondicionFiltroRango(Linea))
            {
              ListaPaso[PosOtra].Incluidos[mRangosSeleccionados.Count] = true;
            }
          }
        }
      }
    }

    /// <summary>
    /// Primero ajusta la vigencia de las filas de la columna asociada y despues
    /// ajusta la vigencia del dataset.
    /// </summary>
    /// <param name="ColumnaPaso"></param>
    private bool FiltrarDatasetAND()
    {
      if (DatoEsFecha())
      {
        return FiltrarDatasetANDFechas();
      }
      // Obtener lista de elementos.
      List<Int32> CodigosLineas = DeterminarCodigosSeleccionadosSinSinonimos();

      // Crear elementos de verificacion.
      CFiltrador FiltroPaso = mProveedor.ObtenerFiltroColumna(mColumnaValor.Nombre);
      List<CElementoFilaAND> ListaPaso = ListaElementosAND(FiltroPaso,
            CodigosLineas.Count);

      // Ajusta los elementos de la columna de paso que estan OK.
      AjustarFiltrosAND(FiltroPaso, ListaPaso, CodigosLineas);

      // Aca se llego a saber que lineas de la columna de paso estan asociadas a que columnas o sinonimos
      // dedinidos.
      // Ahora hay que determinar los que cumplen la condicion de cantidad y despues
      // agregar una lista a la funcion FiltrarDatasetOR de FiltroPaso.
      FiltroPaso.AjustarVigentePorAND(mCantidadNecesaria, ListaPaso);

      // Ahora lo que hay que hacer es ajustar la vigencia de las filas del dataset.
      bool bRetorno = AjustarVigenciaLineasPorFiltro(this);

      // Ahora se ajusta la vigencia de los elementos de la columna propia.
      AjustarVigenciaPropiaPorAND();

      return bRetorno;

    }

    private bool CumpleCondicionFiltroRango(CLineaComprimida Linea)
    {
      Int32 Codigo = Linea.Codigos[mColumna.Orden];
      if (mCodigoMinimo >= 0 && mCodigoMinimo > Codigo)
      {
        return false;
      }
      if (mCodigoMaximo >= 0 && mCodigoMaximo < Codigo)
      {
        return false;
      }
      return true;
    }

    private List<Int32> RangosFechaContenedora(Int32 Posicion)
    {
      List<Int32> Respuesta = new List<int>();
      // lo primero que encuentra es el anio.
      for (Int32 i = 0; i < mRangosSeleccionados.Count; i++)
      {
        if (mRangosSeleccionados[i].PosMinima <= Posicion &&
            mRangosSeleccionados[i].PosMaxima >= Posicion) // incluye la fecha.
        {
          Respuesta.Add(i);
        }
      }
      return Respuesta;
    }

    private bool HabilitarLinea(CLineaComprimida Linea, bool CndAND,
          CFiltrador ColumnaPaso, List<Int32> CodigosSeleccionados)
    {
      if (CndAND)
      {
        if (DatoEsFecha())
        {
          return (CumpleCondicionFiltroFechasOR(Linea) &&
                ColumnaPaso.CumpleCondicionPorVigentes(Linea));
        }
        else
        {
          return (CumpleCondicionFiltroOR(Linea, CodigosSeleccionados) &&
                ColumnaPaso.CumpleCondicionPorVigentes(Linea));
        }
      }
      else
      {
        if (EvaluarRangos)
        {
          return CumpleCondicionFiltroRango(Linea);
        }
        else
        {
          return CumpleCondicionFiltroOR(Linea, CodigosSeleccionados);
        }
      }
    }

    /// <summary>
    /// Filtra los elementos que esten en alguna de las posiciones seleccionadas.
    /// Cuando se agregan CodigosAND, tambien tiene que estan en algunos de ellos.
    /// </summary>
    /// <param name="CodigosAND"></param>
    private bool FiltrarDatasetRangos(List<Int32> CodigosAND = null)
    {
      bool bRetorno = false;
      foreach (CLineaComprimida Linea in DatosBase)
      {
        if (Linea.Vigente)
        {
          Linea.Vigente = (CumpleCondicionFiltroRango(Linea) &&
            (CodigosAND == null ? true : CodigosAND.Contains(Linea.Codigos[mColumna.Orden])));
          bRetorno = bRetorno || (!Linea.Vigente);
        }
      }
      return bRetorno;
    }

    public void AjustarAnchoBandasAzules(double Ancho0)
    {
      if (!DatoEsFecha() && !double.IsNaN(Ancho0))
      {
        //double AnchoMax;
        //try
        //{
        //  AnchoMax = (mFilas.Count == 0 ? 0 :
        //      (from Fila in mFilas where Fila.Vigente select Fila.Valor).Max());
        //}
        //catch (Exception)
        //{
        //  AnchoMax = 0;
        //}
        //AnchoMax = (AnchoMax > 0 ?
        //  (Ancho0 - 25) / AnchoMax : 0);
        double ValorMaximo = (mFilas.Count == 0 ? 0 :
              ((from F in mFilas where F.Vigente select F).Count() > 0 ?
              (from Fila in mFilas where Fila.Vigente select Fila.Valor).Max() : 0));
        if (ValorMaximo > 0)
        {
          double AnchoMax = Ancho0 / ValorMaximo;
          foreach (CElementoFilaAsociativa Fila in mFilas)
          {
            if (mColumnaValor == null || !Fila.Vigente || AnchoMax <= 0)
            {
              Fila.AnchoAzul = 0;
            }
            else
            {
              Fila.AnchoAzul = Math.Max(0, AnchoMax * Fila.Valor);
            }
          }
        }
      }
    }

    // Los filtrosblocks se eliminaron (01/marzo/2021).
    private bool HayFiltrosBlocks
    {
      get { return false; } // return FiltrosBlocks != null && FiltrosBlocks.Count > 0; }
    }

    private List<CLineaComprimida> DatosLocales { get; set; }

    public List<CLineaComprimida> DatosFiltrados
		{
      get
      {
        return (from L in DatosLocales
                where L.Vigente
                select L).ToList();
      }
		}

    private List<CLineaComprimida> DatosBase
    {
      get
      {
        return (HayFiltrosBlocks ? DatosLocales : mProveedor.Datos);
      }
    }

    private void AjustarFiltrosBlocks()
		{
      if (HayFiltrosBlocks)
      {
        //foreach (CFiltradorStep Paso in FiltrosBlocks)
        //{
        //  if (!Paso.FiltrarDatos(DatosBase, mProveedor.Columnas))
        //  {
        //    throw new Exception("No pudo filtrar");
        //  }
        //  DatosLocales = Paso.Datos;
        //}
      }
    }

    public void PonerDatosIniciales()
		{
      if (HayFiltrosBlocks)
      {
        DatosLocales = new List<CLineaComprimida>();
        DatosLocales.AddRange(mProveedor.Datos);
      }
    }

    public bool FiltrarDataset()
    {

      try
      {

        //if (HayFiltrosBlocks)
        //{
        //  DatosLocales = new List<CLineaComprimida>();
        //  DatosLocales.AddRange(mProveedor.Datos);
        //}

        if (HayCondicion || HayFiltrosBlocks)
        {
          if (!CondicionAND)
          {
            bool bRetorno = (EvaluarRangos ? FiltrarDatasetRangos() : FiltrarDatasetOR());
            //AjustarFiltrosBlocks();
            AjustarVigenciaFilas();
            return bRetorno;
          }
          else
          {
            return FiltrarDatasetAND();
          }
        }
        else
        {
          //bool Sucias = (from F in DatosBase
          //               where !F.Vigente
          //               select F).FirstOrDefault() != null;
          //if (Sucias)
          //{
          //  foreach (CLineaComprimida Elemento in DatosBase)
          //  {
          //    Elemento.Vigente = true;
          //  }
          //  //AjustarFiltrosBlocks();
          //}
          //return Sucias;
          return false;
        }
      }
      finally
      {
        //if (Proveedor!=null)
        //{
        //  Proveedor.RefrescarDependientes();
        //}
      }
    }

    /// <summary>
    /// Filtra los elementos que esten en alguna de las posiciones seleccionadas.
    /// Cuando se agregan CodigosAND, tambien tiene que estan en algunos de ellos.
    /// </summary>
    /// <param name="CodigosAND"></param>
    private bool FiltrarDatasetOR(List<Int32> CodigosAND = null)
    {
      switch (mColumna.Clase)
      {
        case ClaseVariable.Fecha:
          return FiltrarDatasetFechasOR(CodigosAND);
        default:
          return FiltrarDatasetTextosOR(CodigosAND);
      }
    }

    private bool FiltrarDatasetFechasOR(List<Int32> CodigosAND)
    {
      bool bRetorno = false;
      foreach (CLineaComprimida Linea in DatosBase)
      {
        if (Linea.Vigente)
        {
          Linea.Vigente = (CumpleCondicionFiltroFechasOR(Linea) &&
            (CodigosAND == null ? true : CodigosAND.Contains(Linea.Codigos[mColumna.Orden])));
          bRetorno = bRetorno || (!Linea.Vigente);
        }
      }
      return bRetorno;
    }

    public bool CondicionAND
    {
      get
      {
        return false;
        //return (ColumnaValor == null || mFiltro == null || mFiltro.cbAND.IsChecked != true ? false :
        //  ((mValoresSeleccionados.Count > 1 || (HayRango() && mValoresSeleccionados.Count > 0))));
      }
    }

    public bool CumpleCondicionFiltroFechasOR(CLineaComprimida Linea)
    {
      Int32 Codigo = Linea.Codigos[mColumna.Orden];

      // si cumple la condicion del rango, ya esta.
      if (mCodigoMinimo >= 0 || mCodigoMaximo >= 0 && mRangosSeleccionados.Count == 0)
      {
        if (Codigo >= mCodigoMinimo && Codigo <= mCodigoMaximo)
        {
          return true;
        }
      }

      foreach (CRangoFechas Rango in mRangosSeleccionados)
      {
        if (Codigo >= Rango.PosMinima && Codigo <= Rango.PosMaxima)
        {
          return true;
        }
      }

      return false;

    }

    /// <summary>
    /// Asocia los elementos de la fila con los de la columna valor.
    /// Cuando haya sinonimos, hay que hacer un proceso posterior para incluir todos los
    /// elementos de los sinonimos incluidos en los elementos seleccionados.
    /// </summary>
    public void AsociarConColumnaValor()
    {
      EliminarAsociacionValor();
      if (mColumnaValor != null)
      {
        Int32 PosColValor = mColumnaValor.Orden;
        if (mProveedor.Columnas[mColumna.Orden].Clase == ClaseVariable.Fecha)
        {
          foreach (CLineaComprimida Linea in DatosBase)
          {
            //if (Linea.Vigente)
            //{
            foreach (CElementoFilaAsociativaFecha Fila in mFilas)
            {
              if (Fila.IndiceMinimo <= Linea.Codigos[mColumna.Orden] && Fila.IndiceMaximo >= Linea.Codigos[mColumna.Orden])
              {
                Fila.SumarPosicionAsociada(Linea.Codigos[PosColValor],
                      Linea.Vigente);
              }
            }
            //}
          }
        }
        else
        {
          foreach (CLineaComprimida Linea in DatosBase)
          {
            //if (Linea.Vigente)
            //{
            mFilas[Linea.Codigos[mColumna.Orden]].SumarPosicionAsociada(Linea.Codigos[PosColValor],
                  Linea.Vigente);
            //}
          }
        }
      }
    }

    public void EliminarAsociacionValor()
    {
      foreach (CElementoFilaAsociativa Fila in mFilas)
      {
        Fila.PosicionesAsociadas.Clear();
        Fila.PosicionesAsociadasVigentes.Clear();
      }
    }

    public void EliminarFiltroAsociado()
    {
      foreach (CElementoFilaAsociativa Fila in mFilas)
      {
        Fila.EstaAsociadoAFiltro = false;
      }
    }

    public void RefrescarAsociacionesAlCambiarProveedor()
    {
      if (mColumnaValor != null)
      {
        CFiltrador FiltroValor = mProveedor.FiltroParaColumna(mColumnaValor.Nombre);
        if (FiltroValor == null)
        {
          return;
        }

        FiltroValor.EliminarFiltroAsociado();

        AsociarConColumnaValor();

        foreach (CElementoFilaAsociativa Fila in mFilas)
        {
          if (FiltroValor.AlImponerFiltrosAsociados!=null)
          {
            AlImponerFiltrosAsociados(this, CodigosFiltroAsociados());
//            FiltroValor.Filtro.ImponerFiltrosAsociados(CodigosFiltroAsociados());
          }
        }

      }

    }

    public void ImponerVigencia(bool B)
    {
      foreach (CElementoFilaAsociativa Elemento in mFilas)
      {
        Elemento.Vigente = B;
      }
    }

    private void AjustarVigenciaPropiaPorAND()
    {

      ImponerVigencia(false);

      foreach (CLineaComprimida Linea in DatosBase)
      {
        if (Linea.Vigente)
        {
          mFilas[Linea.Codigos[mColumna.Orden]].Vigente = true;
        }
      }
    }

    private Int32 PosicionCondicion(Int32 PosLocal, List<Int32> CodigosLineas)
    {
      Int32 PosCodigo = CodigosLineas.IndexOf(PosLocal);
      if (PosCodigo >= 0)
      {
        return PosCodigo;
      }

      return -1;

    }

    private void AjustarVigenciaPropiaPorANDFecha()
    {

      ImponerVigencia(false);

      foreach (CRangoFechas Rango in mRangosSeleccionados)
      {
        Rango.Vigente = false;
      }

      foreach (CLineaComprimida Linea in DatosBase)
      {
        if (Linea.Vigente)
        {
          Int32 Codigo = Linea.Codigos[mColumna.Orden];
          foreach (CRangoFechas Rango in mRangosSeleccionados)
          {
            Rango.Vigente |= (Codigo >= Rango.PosMinima && Codigo <= Rango.PosMaxima);
          }
        }
      }

      foreach (CElementoFilaAsociativaFecha Fila in mFilas)
      {
        foreach (CRangoFechas Rango in mRangosSeleccionados)
        {
          if (Rango.Vigente && Rango.PosMinima >= Fila.IndiceMinimo && Rango.PosMaxima <= Fila.IndiceMaximo)
          {
            Fila.Vigente = true;
            break;
          }
        }
      }
    }

    public void AjustarVigenciaFilas()
    {
      // cuando se trata de enteros o reales, no corresponde.
      switch (mColumna.Clase)
      {
        //        case ClaseVariable.Entero:
        case ClaseVariable.Real:
          return;
      }

      foreach (CElementoFilaAsociativa Elemento in mFilas)
      {
        Elemento.Vigente = false;
      }

      if (mColumna.Clase == ClaseVariable.Fecha)
      {
        AjustarVigenciaFilasFecha();
      }
      else
      {
        foreach (CLineaComprimida Linea in DatosBase)
        {
          if (Linea.Vigente)
          {
            Int32 Pos = Linea.Codigos[mColumna.Orden];
            if (Pos >= 0 && Pos < mFilas.Count)
            {
              mFilas[Linea.Codigos[mColumna.Orden]].Vigente = true;
            }
          }
        }
      }

      AjustarVisibilidadPorcAlfabetico();

    }

    public void AjustarVigenciaFilasFecha()
    {
      foreach (CLineaComprimida Linea in DatosBase)
      {
        if (Linea.Vigente)
        {
          Int32 Codigo = Linea.Codigos[mColumna.Orden];
          foreach (CElementoFilaAsociativaFecha Fila in mFilas)
          {
            if (Fila.IndiceMinimo <= Codigo && Fila.IndiceMaximo >= Codigo)
            {
              Fila.Vigente = true;
            }
          }
        }
      }
    }

    /// <summary>
    /// Retorna una lista con la fila indicada, mas todas las vinculadas por algun sinonimo.
    /// </summary>
    /// <param name="FilaInicial"></param>
    /// <returns></returns>
    private List<CElementoFilaAsociativa> FilasConSinonimos(CElementoFilaAsociativa FilaInicial)
    {
      List<CElementoFilaAsociativa> Respuesta = new List<CElementoFilaAsociativa>();
      Respuesta.Add(FilaInicial);
      return Respuesta;
    }

    /// <summary>
    /// Determina los codigos de los elementos de la columna ColumnaValor que estan asociados a los filtros.
    /// Incluye los que no estan vigentes.
    /// </summary>
    /// <returns></returns>
    public List<Int32> CodigosFiltroAsociados()
    {
      List<Int32> Respuesta = new List<int>();
      foreach (CElementoFilaAsociativa Fila in mFilas)
      {
        if (Fila.UsadoParaFiltrarAsociaciones)
        {
          List<CElementoFilaAsociativa> FilasGrupo = FilasConSinonimos(Fila);
          foreach (CElementoFilaAsociativa FilaGrupo in FilasGrupo)
          {
            foreach (Int32 Codigo in FilaGrupo.PosicionesAsociadasVigentes)
            {
              Respuesta.Add(Codigo);
            }
          }
        }
      }
      return Respuesta;
    }

    public void SumarValoresPorFecha(CProveedorComprimido Proveedor,
          ModoAgruparDependiente Modo)
    {
      foreach (CLineaComprimida Linea in Proveedor.Datos)
      {
        if (Linea.Vigente)
        {
          List<Int32> PosFilas = FilasFechaContenedora(Linea.Codigos[mColumna.Orden]);
          foreach (Int32 PosFila in PosFilas)
          {
            switch (Modo)
            {
              case ModoAgruparDependiente.Acumulado:
              case ModoAgruparDependiente.Media:
                mFilas[PosFila].Valor += ValorLinea(Linea);
                mFilas[PosFila].Cantidad++;
                break;
              case ModoAgruparDependiente.Maximo:
                double R = ValorLinea(Linea);
                if (double.IsNaN(mFilas[PosFila].Valor) || mFilas[PosFila].Valor < R)
                {
                  mFilas[PosFila].Valor = R;
                }
                break;
              case ModoAgruparDependiente.Minimo:
                double R2 = ValorLinea(Linea);
                if (double.IsNaN(mFilas[PosFila].Valor) || mFilas[PosFila].Valor > R2)
                {
                  mFilas[PosFila].Valor = R2;
                }
                break;
              case ModoAgruparDependiente.Cantidad:
                mFilas[PosFila].Valor++;
                break;
            }
          }
        }
      }
    }

    public void SumarValoresPorLinea(CProveedorComprimido Proveedor,
        ModoAgruparDependiente Modo)
    {
      mColumna = Proveedor.Columnas[mColumna.Orden];
      mColumnaValor = Proveedor.Columnas[mColumnaValor.Orden];
      foreach (CLineaComprimida Linea in Proveedor.Datos)
      {
        if (Linea.Vigente)
        {
          Int32 PosFila = Linea.Codigos[mColumna.Orden];
          switch (Modo)
          {
            case ModoAgruparDependiente.Acumulado:
            case ModoAgruparDependiente.Media:
              mFilas[PosFila].Valor += ValorLinea(Linea);
              mFilas[PosFila].Cantidad++;
              break;
            case ModoAgruparDependiente.Maximo:
              double R = ValorLinea(Linea);
              if (double.IsNaN(mFilas[PosFila].Valor) || mFilas[PosFila].Valor < R)
              {
                mFilas[PosFila].Valor = R;
              }
              break;
            case ModoAgruparDependiente.Minimo:
              double R2 = ValorLinea(Linea);
              if (double.IsNaN(mFilas[PosFila].Valor) || mFilas[PosFila].Valor > R2)
              {
                mFilas[PosFila].Valor = R2;
              }
              break;
            case ModoAgruparDependiente.Cantidad:
              mFilas[PosFila].Valor++;
              break;
          }
        }
      }
    }

    private double ValorLinea(CLineaComprimida Linea)
    {
      return CRutinas.StrVFloat(mColumnaValor.ListaValores[Linea.Codigos[mColumnaValor.Orden]]);
    }

    private List<Int32> FilasFechaContenedora(Int32 Posicion)
    {
      List<Int32> Respuesta = new List<int>();
      // lo primero que encuentra es el anio.
      for (Int32 i = 0; i < mFilas.Count; i++)
      {
        CElementoFilaAsociativaFecha Fila = (CElementoFilaAsociativaFecha)mFilas[i];
        if (Fila.IndiceMinimo <= Posicion && Fila.IndiceMaximo >= Posicion) // incluye la fecha.
        {
          Respuesta.Add(i);
        }
      }
      return Respuesta;
    }

    public void AjustarMedias (ModoAgruparDependiente Modo)
    {
      foreach (CElementoFilaAsociativa Elemento in mFilas)
      {
        if (Modo == ModoAgruparDependiente.Media)
        {
          if (Elemento.Cantidad > 0)
          {
            Elemento.Valor = Elemento.Valor / Elemento.Cantidad;
            Elemento.Cantidad = 0;
          }
          else
          {
            Elemento.Valor = double.NaN;
          }
        }
        else
        {
          Elemento.Cantidad = 0;
        }

      }

    }

    /// <summary>
    /// Inicializa las vigencias de las filas por las selecciones dentro del propio filtro.
    /// </summary>
    public void InicializarSelecciones()
    {

      if (mValoresSeleccionados.Count == 0)
      {
        ImponerVigencia(true);
      }
      else
      {
        foreach (CElementoFilaAsociativa Fila in mFilas)
        {
          Fila.Vigente = Fila.Seleccionado;
        }
      }
    }

    /// <summary>
    /// Despues de imponer una condicion AND, unicamente quedaran vigentes las lineas
    /// donde se cumpla la condicion AND.
    /// Lo que hace la rutina es recorrer los valores de la columna
    /// y mantener vigentes los que cumplan la condicion.
    /// Despues vuelve sobre el dataset y ajusta la vigencia.
    /// </summary>
    public void AjustarVigentePorAND(Int32 Cantidad, List<CElementoFilaAND> ListaPaso)
    {
      for (Int32 i = 0; i < mFilas.Count; i++)
      {
        mFilas[i].AjustarVigentePorAND(Cantidad, ListaPaso[i]);
      }
    }

    private bool FiltrarDatasetTextosOR(List<Int32> CodigosAND)
    {
      // Obtener lista de elementos.
      List<Int32> CodigosLineas = DeterminarCodigosSeleccionadosSinSinonimos();

      bool bRetorno = false;
      foreach (CLineaComprimida Linea in DatosBase)
      {
        if (Linea.Vigente)
        {
          Linea.Vigente = (CumpleCondicionFiltroOR(Linea, CodigosLineas) &&
            (CodigosAND == null ? true : CodigosAND.Contains(Linea.Codigos[mColumna.Orden])));
          bRetorno = bRetorno || (!Linea.Vigente);
        }
      }
      return bRetorno;
    }

    /// <summary>
    /// Se usa cuando se verifica una condicion AND.
    /// Retorna la vigencia del elemento correspondiente a una linea.
    /// </summary>
    /// <param name="Linea"></param>
    /// <returns></returns>
    public bool CumpleCondicionPorVigentes(CLineaComprimida Linea)
    {
      return mFilas[Linea.Codigos[mColumna.Orden]].Vigente;
    }

    public bool CumpleCondicionFiltroOR(CLineaComprimida Linea,
          List<Int32> CodigosElementos)
    {
      // si no hay condiciones, esta ok.
      if (!HayCondicion) // (CodigosElementos.Count==0)
      {
        return true;
      }

      Int32 Codigo = Linea.Codigos[mColumna.Orden];
      foreach (Int32 CodLocal in CodigosElementos)
      {
        if (CodLocal == Codigo)
        {
          return true;
        }
      }
      //foreach (Int32 Pos in mPosSinonimosSeleccionados)
      //{
      //  CSinonimoCompletoCN Sinonimo = mColumna.Sinonimos[Pos];
      //  foreach CElementosSinonimosCN Elemento in Sinonimo.ElementosAsociados)
      //  {
      //    if (Elemento.Codigo == Codigo)
      //    {
      //      return true;
      //    }
      //  }
      //}
      return false;
    }

    private bool HayRango()
    {
      return (mszValorMinimo.Length > 0 || mszValorMaximo.Length > 0);
    }

    /// <summary>
    /// Arma una lista de CElementoFilaAND para cada elemento de la columna de paso que aun esta vigente.
    /// </summary>
    /// <param name="FiltroPaso"></param>
    /// <param name="Cantidad"></param>
    /// <returns></returns>
    private List<CElementoFilaAND> ListaElementosAND(CFiltrador FiltroPaso, Int32 Cantidad)
    {
      //      mCantidadNecesaria = Cantidad;
      List<CElementoFilaAND> ListaPaso = new List<CElementoFilaAND>();
      for (Int32 i = 0; i < FiltroPaso.Filas.Count; i++)
      {
        CElementoFilaAsociativa Elemento = FiltroPaso.Filas[i];
        if (Elemento.Vigente)
        {
          ListaPaso.Add(new CElementoFilaAND(i, Cantidad));
        }
        else
        {
          ListaPaso.Add(new CElementoFilaAND(i, 0));
        }
      }
      return ListaPaso;
    }

    void AjustarFiltrosAND(CFiltrador FiltroPaso, List<CElementoFilaAND> ListaPaso,
              List<Int32> CodigosLineas)
    {
      Int32 OrdenPaso = FiltroPaso.Columna.Orden;
      Int32 OrdenLocal = mColumna.Orden;
      foreach (CLineaComprimida Linea in DatosBase)
      {
        if (Linea.Vigente)
        {
          Int32 PosOtra = Linea.Codigos[OrdenPaso];
          Int32 PosCnd = PosicionCondicion(Linea.Codigos[OrdenLocal], CodigosLineas);
          // cuando haya and enlazados, puede darse que se apunte a una fila que no esta vigente
          // porque aun la fila no se filtro, pero si se ajusto la vigencia de los elementos de la columna.
          if (PosCnd >= 0 && PosCnd < ListaPaso[PosOtra].Incluidos.Count)
          {
            ListaPaso[PosOtra].Incluidos[PosCnd] = true;
          }
        }
      }
    }

    public bool EvaluarRangos
    {
      get
      {
        switch (mColumna.Clase)
        {
          //          case ClaseVariable.Entero:
          case ClaseVariable.Real:
            return true;
          case ClaseVariable.Fecha:
            return (mValoresSeleccionados.Count == 0); // verifica exclusivamente rangos cuando no hay periodos.
          default:
            return false;
        }
      }
    }

    private CElementoFilaAsociativa FilaDesdeNombre(string Nombre)
    {
      Int32 Codigo = CodigoValor(Nombre);
      return (Codigo >= 0 ? mFilas[Codigo] : (CElementoFilaAsociativa)null);
    }

  }

  public class CElementoFilaAND
  {
    public Int32 Orden;
    public List<bool> Incluidos;

    public CElementoFilaAND(Int32 OrdenImpuesto, Int32 Cantidad)
    {
      Orden = OrdenImpuesto;
      Incluidos = new List<bool>();
      for (Int32 i = 0; i < Cantidad; i++)
      {
        Incluidos.Add(false);
      }
    }

    public Int32 CantidadCheckeado()
    {
      Int32 Respuesta = 0;
      foreach (bool Check in Incluidos)
      {
        Respuesta += (Check ? 1 : 0);
      }
      return Respuesta;
    }

  }

  public class CElementoFilaAsociativa
  {
    private string mszNombre;
    private double mValor;
    private double mCantidad;
    private bool mbSeleccionado;
    private bool mbVigente;
    private bool mbFiltroAsociaciones; // cuando se selecciona para mostrar las asociaciones.
    private bool mbEstaAsociado; // cuando desde otra ventana se selecciono algun elemento asociado.
    protected double mAnchoFila0 = 0;
    private double mAnchoFila1 = 0;
    private double mAnchoAzul;
    private List<Int32> mPosDatoAsociado;
    private List<Int32> mPosDatoAsociadoVigente;

    public CElementoFilaAsociativa()
    {
      CodigoOrden = -1;
      mszNombre = "";
      mValor = double.NaN;
      mCantidad = 0;
      mbSeleccionado = false;
      mbVigente = true;
      mAnchoFila0 = 185;
      mAnchoAzul = 0;
      mbFiltroAsociaciones = false;
      mbEstaAsociado = false;
      mPosDatoAsociado = new List<int>();
      mPosDatoAsociadoVigente = new List<int>();
    }

    public Int32 CodigoOrden { get; set; }

    public bool Seleccionado
    {
      get { return mbSeleccionado; }
      set
      {
        mbSeleccionado = value;
        AjustarVisibilidadAlSeleccionar();
      }
    }

    protected virtual void AjustarVisibilidadAlSeleccionar()
    {
      //
    }

    public Int32 AnchoTextoValor()
		{
      Int32 Caracteres = TextoValor.Length;
      double AnchoUnico = CRutinas.TamanioLetraMedia("Microsoft Sans Serif", 11);
      return (Int32)Math.Floor(AnchoUnico * Caracteres + 5.5);
		}

    public string FuenteNombre
    {
      get { return "font-weight: "+(mbVigente ? "bold;" : "normal;"); }
    }

    public bool UsadoParaFiltrarAsociaciones
    {
      get { return mbFiltroAsociaciones; }
      set { mbFiltroAsociaciones = value; }
    }

    public bool EstaAsociadoAFiltro
    {
      get { return mbEstaAsociado; }
      set { mbEstaAsociado = value; }
    }

    public List<Int32> PosicionesAsociadas
    {
      get { return mPosDatoAsociado; }
      set { mPosDatoAsociado = value; }
    }

    public List<Int32> PosicionesAsociadasVigentes
    {
      get { return mPosDatoAsociadoVigente; }
      set { mPosDatoAsociadoVigente = value; }
    }

    public double AnchoAzul
    {
      get { return mAnchoAzul; }
      set { mAnchoAzul = Math.Floor(value); }
    }

    public bool RectVisible
    {
      get { return (AnchoAzul > 0 ? true : false); }
    }

    public bool Vigente
    {
      get { return mbVigente; }
      set { mbVigente = value; }
    }

    public virtual double AnchoCol0
    {
      get { return mAnchoFila0; }
      set { mAnchoFila0 = value; }
    }

    public double AnchoCol1
    {
      get { return mAnchoFila1; }
      set { mAnchoFila1 = value; }
    }

    public double Porcentaje { get; set; }

    public object Referencia
    {
      get { return this; }
    }

    public string ColorFuente
    {
      get
      {
        return "color: "+ (mbFiltroAsociaciones || mbEstaAsociado ? "red;" : "black;");
      }
    }

    public string ColorBase
    {
      get
      {
        return "background-color: "+(mbVigente ? "white;" : "lightgray;");
      }
    }

    //public SolidColorBrush ColorBase
    //{
    //  get
    //  {
    //    return new SolidColorBrush(mbVigente ?
    //    (mbFiltroAsociaciones ? Colors.Orange : (mbEstaAsociado ? Colors.Yellow : Colors.White)) :
    //    (mbEstaAsociado ? Color.FromArgb(255, 180, 180, 0) : Colors.LightGray));
    //  }
    //}

    public string Nombre
    {
      get { return mszNombre; }
      set { mszNombre = value; }
    }

    public double Valor
    {
      get { return mValor; }
      set { mValor = value; }
    }

    public double Cantidad
    {
      get { return mCantidad; }
      set { mCantidad = value; }
    }

    public bool HayValor
		{
      get
			{
        return (mbVigente || mCantidad > 0) && !double.IsNaN(mValor);
			}
		}

    public string TextoValor
    {
      get
      {
        return ((mbVigente || mCantidad > 0) && !double.IsNaN(mValor) ?
          (mCantidad > 0 ? (((Int32)mValor).ToString() + " de " + mCantidad.ToString() +
          (Porcentaje > 0 ? (" (" + Porcentaje.ToString("##0.0") + "%)") : "")) :
          (CRutinas.ValorATexto(mValor) + (Porcentaje > 0 ? (" (" + Porcentaje.ToString("##0.0") + "%") + ")" : ""))) : "");
      }
    }

    public bool CumpleCondicionTexto(string Texto)
    {
      return mszNombre.IndexOf(Texto, StringComparison.InvariantCultureIgnoreCase) >= 0;
    }

    public void AjustarVigentePorAND(Int32 Cantidad, CElementoFilaAND ListaPaso)
    {
      if (mbVigente)
      {
        mbVigente = (ListaPaso.CantidadCheckeado() >= Cantidad);
      }
    }

    public void SumarPosicionAsociada(Int32 Posicion, bool Vigente)
    {
      if (!mPosDatoAsociado.Contains(Posicion))
      {
        mPosDatoAsociado.Add(Posicion);
      }
      if (Vigente && !mPosDatoAsociadoVigente.Contains(Posicion))
      {
        mPosDatoAsociadoVigente.Add(Posicion);
      }
    }

  }

  public class CElementoFilaAsociativaFecha : CElementoFilaAsociativa
  {
    private const double SALTO_NIVEL = 15;
    public Int32 mIndiceMinimo;
    public Int32 mIndiceMaximo;
    public Int32 Saltos;
    private bool mbAbierto; // si muestra los dependientes.
    public List<CElementoFilaAsociativaFecha> Dependientes;
    public DateTime FechaMinima { get; set; }
    public DateTime FechaMaxima { get; set; }
    private bool mbVisible;

    public CElementoFilaAsociativaFecha(Int32 Nivel, CElementoFilaAsociativaFecha Contenedor)
    {
      Saltos = Nivel;
      mbVisible = (Contenedor == null);
      Dependientes = new List<CElementoFilaAsociativaFecha>();
      if (Contenedor != null)
      {
        Contenedor.Dependientes.Add(this);
      }
      mbAbierto = false;
    }

    public string TextoMas
    {
      get
      {
        return (Dependientes.Count == 0 || Seleccionado ? " " : (mbAbierto ? "-" : "+"));
      }
    }

    protected override void AjustarVisibilidadAlSeleccionar()
    {
      if (Seleccionado)
      {
        CambiarVisibilidadDependientes(!Seleccionado);
      }
    }

    public bool DependientesAbiertos
    {
      get { return mbAbierto; }
      set
      {
        mbAbierto = value;
        AnguloGiro = (mbAbierto ? 180 : 0);
      }
    }

    public bool Visible
    {
      get { return mbVisible; }
      set
      {
        if (mbVisible != value)
        {
          mbVisible = value;
        }
      }
    }

    public void CambiarVisibilidadDependientes(
           bool QuedaVisible)
    {
      foreach (CElementoFilaAsociativaFecha Elemento in Dependientes)
      {
        Elemento.Visible = QuedaVisible;
        Elemento.DependientesAbiertos = false;
        Elemento.CambiarVisibilidadDependientes(false);
      }
    }

    private double mdAngulo;
    public double AnguloGiro
    {
      get { return mdAngulo; }
      set
      {
        if (mdAngulo != value)
        {
          mdAngulo = value;
        }
      }
    }

    public double DefasajeInicial
    {
      get { return SALTO_NIVEL * Saltos + (Saltos == 2 ? 15 : 0); }
    }

    public string VisiBoton
    {
      get { return "visibility: "+(Saltos < 2 ? "visible;" : "hidden;"); }
    }

    private bool mVisiBotonDinamico = false;
    public bool VisiBotonDinamico
    {
      get { return mVisiBotonDinamico; }
      set
      {
        if (mVisiBotonDinamico != value)
        {
          mVisiBotonDinamico = value;
        }
      }
    }

    public override double AnchoCol0
    {
      get { return mAnchoFila0 - Saltos * SALTO_NIVEL; }
      set { mAnchoFila0 = value; }
    }

    public Int32 IndiceMinimo
    {
      get { return mIndiceMinimo; }
      set { mIndiceMinimo = value; }
    }

    public Int32 IndiceMaximo
    {
      get { return mIndiceMaximo; }
      set { mIndiceMaximo = value; }
    }

  }

  public class CRangoFechas
  {
    public Int32 Posicion;
    public Int32 PosMinima;
    public Int32 PosMaxima;
    public bool Vigente;
    public DateTime Desde;
    public DateTime Hasta;
  }



}
