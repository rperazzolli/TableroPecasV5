using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using Blazor.Extensions.Canvas.Canvas2D;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Datos
{
  /// <summary>
  /// Clase que se usa para manejar el listado de registros
  /// seleccionados para graficarlos y sus filtros, ya sean por
  /// seleccion desde un grafico o por condiciones.
  /// </summary>
  public class CFiltradorPasos
  {
    private ClaseElemento mClaseOrigen;
    private Int32 mCodigoOrigen;
    private CProveedorComprimido mProveedor;
    private List<CFiltradorStep> mPasos;

    public CFiltradorPasos (ClaseElemento ClaseOrg,Int32 CodigoOrg)
    {
      mPasos = new List<CFiltradorStep>();
      mClaseOrigen = ClaseOrg;
      mCodigoOrigen = CodigoOrg;
    }

    public  ClaseElemento ClaseOrigen
    {
      get { return mClaseOrigen; }
      set { mClaseOrigen = value; }
    }

    public Int32 CodigoOrigen
    {
      get { return mCodigoOrigen; }
      set { mCodigoOrigen = value; }
    }

    public CProveedorComprimido Proveedor
    {
      get
      {
        if (mProveedor == null)
        {
          mProveedor = new CProveedorComprimido(mClaseOrigen, mCodigoOrigen);
        }
        return mProveedor;
      }
      set
      {
        if (value != mProveedor)
        {
          mProveedor = value;
          AjustarDatosIniciales();
        }
      }
    }

    /// <summary>
    /// Indica cuando hay un filtro del grafico anterior.
    /// </summary>
    public bool TieneFiltrosExternos
    {
      get
      {
        if (mPasos.Count <= 1)
        {
          return false;
        }
        else
        {
          return mPasos[1].ImpuestoExternamente;
        }
      }
    }

    public CColumnaBase ColumnaDesdeNombre(string Nombre)
    {
      foreach (CColumnaBase Columna in mProveedor.Columnas)
      {
        if (Columna.Nombre == Nombre)
        {
          return Columna;
        }
      }
      return null;
    }

    public List<CLineaComprimida> FiltrarPorCondiciones(List<CCondicion> Condiciones)
    {
      CCondiciones Condicionador = new CCondiciones();
      Condicionador.Condiciones = Condiciones;
      Condicionador.IncluyeCondiciones = true;
      List<CLineaComprimida> Respuesta = new List<CLineaComprimida>();
      foreach (CLineaComprimida Linea in Datos)
      {
        if (Condicionador.CumpleCondicion(Linea))
        {
          Respuesta.Add(Linea);
        }
      }
      return Respuesta;
    }

    /// <summary>
    /// Pone los datos iniciales y reinicia, eliminando los filtros.
    /// </summary>
    /// <param name="Datos"></param>
    public void PonerDatosIniciales(List<CColumnaBase> ColumnasIni, List<CLineaComprimida> Datos)
    {
      mPasos.Clear();
      CFiltradorStep Paso = new CFiltradorStep();
      if (mProveedor == null)
      {
        mProveedor = new CProveedorComprimido(mClaseOrigen, mCodigoOrigen);
      }
      if (ColumnasIni != null)
      {
        mProveedor.Columnas = ColumnasIni;
      }
      Paso.FiltrarDatos(Datos, mProveedor.Columnas);
      mPasos.Add(Paso);
    }

    private CFiltradorStep ObtenerPrimerPaso()
    {
      if (mPasos.Count > 0)
      {
        return mPasos[0];
      }
      else
      {
        CFiltradorStep Paso = new CFiltradorStep();
        mPasos.Add(Paso);
        return Paso;
      }
    }

    public void EliminarPasosExceptoPrimero()
    {
      while (mPasos.Count > 1)
      {
        mPasos.RemoveAt(mPasos.Count - 1);
      }
    }

    public bool AjustarDatosIniciales(List<Datos.CColumnaBase> ColIni, List<Datos.CLineaComprimida> Datos)
    {
      if (ColIni != null)
      {
        mProveedor.Columnas = ColIni;
      }
      mProveedor.Datos = Datos;
      return AjustarDatosIniciales();
    }

    /// <summary>
    /// Pone los datos iniciales pero mantiene los filtros.
    /// Se usa en los graficos secundarios.
    /// </summary>
    /// <param name="Datos"></param>
    /// <returns></returns>
    public bool AjustarDatosIniciales()
    {

      try
      {
        CFiltradorStep Paso = ObtenerPrimerPaso();

        Paso.EliminarCondiciones();

        Paso.FiltrarDatos(mProveedor.Datos, mProveedor.Columnas);

        for (Int32 i = 1; i < mPasos.Count; i++)
        {
          mPasos[i].FiltrarDatos(mPasos[i - 1].Datos, mProveedor.Columnas);
        }

        return true;

      }
      catch (Exception ex)
      {
        CRutinas.InformarUsuario("Error al filtrar datos secundarios", CRutinas.MostrarMensajeError(ex));
        return false;
      }

    }

    public Int32 CantidadPasos
    {
      get
      {
        return mPasos.Count;
      }
    }

    public List<CFiltradorStep> Pasos
    {
      get
      {
        return mPasos;
      }
    }

    public List<CLineaComprimida> DatosIniciales
    {
      get
      {
        if (mPasos.Count == 0)
        {
          return null;
        }
        else
        {
          return mPasos[0].Datos;
        }
      }
    }

    public List<CLineaComprimida> Datos
    {
      get
      {
        if (mPasos.Count == 0)
        {
          return null;
        }
        else
        {
          return mPasos[mPasos.Count - 1].Datos;
        }
      }
    }

    public List<CLineaComprimida> DatosPasoPrevio
    {
      get
      {
        if (mPasos.Count < 2)
        {
          return null;
        }
        else
        {
          return mPasos[mPasos.Count - 2].Datos;
        }
      }
    }

    public List<CDatosTorta> ArmarSerieSinFiltro(
        Int32 OrdenOrdenada)
    {
      try
      {

        List<CDatosTorta> Respuesta = new List<CDatosTorta>();

        CColumnaBase Columna = mProveedor.Columnas[OrdenOrdenada];

        foreach (CLineaComprimida Linea in DatosIniciales)
        {
          CDatosTorta Dato = new CDatosTorta();
          Dato.Nombre = Columna.ListaValores[Linea.Codigos[OrdenOrdenada]];
          Dato.NombreOriginal = Dato.Nombre;
          Dato.Cantidad = 1;
          Dato.Valor = mProveedor.ObtenerValorRealLinea(Linea, OrdenOrdenada);
          Respuesta.Add(Dato);
        }

        return Respuesta;

      }
      catch (Exception ex)
      {
        CRutinas.InformarUsuario("Error armando serie de datos",
          CRutinas.MostrarMensajeError(ex));
        return new List<CDatosTorta>();
      }

    }

    public void ActualizarDatosGrafico()
    {
      AjustarDatosIniciales();
    }

    /// <summary>
    /// Determina los diferentes valores de abscisa para una grilla en base
    /// a la columna de la variable independiente.
    /// </summary>
    /// <param name="OrdenSerie"></param>
    /// <returns></returns>
    public List<string> ValoresDiferentes(Int32 OrdenSerie)
    {
      return mProveedor.Columnas[OrdenSerie].ListaValores;
    }

    private string TransformarFechaAPantalla(string FechaSerie)
    {
      bool bHayDatos = false;
      DateTime Fecha = CRutinas.DecodificarFecha(FechaSerie, ref bHayDatos);
      if (!bHayDatos || Fecha.Year<=1900)
      {
        return "";
      }
      else
      {
        return CRutinas.FormatearFecha(Fecha);
      }
    }

    private string TransformarRealAPantalla(string ValorReal)
    {
      return ValorReal.Replace(".",
          System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
    }

    /// <summary>
    /// Determina los valores de las abscisas que despues se van a usar en el grafico.
    /// Al regresar hay una lista de valores de abscisas con ordenadas en 0 o indeterminado
    /// segun lo que se desea graficar (indeterminado para minimo y maximo).
    /// </summary>
    /// <param name="OrdenSerie"></param>
    /// <param name="ModoOrdenadas"></param>
    /// <returns></returns>
    public List<CDatosTorta> DiferentesValoresAbscisas(Int32 OrdenSerie,
        ModoAgruparIndependiente ModoAbscisas,
        ModoAgruparDependiente ModoOrdenadas,
        Int32 MaxGajos)
    {

      List<CDatosTorta> Respuesta = new List<CDatosTorta>();

      if (ModoAbscisas == ModoAgruparIndependiente.Rangos)
      {
        double Minimo;
        double Maximo;
        switch (mProveedor.Columnas[OrdenSerie].Clase)
        {
          case ClaseVariable.Real:
            ((CColumnaReal)mProveedor.Columnas[OrdenSerie]).ObtenerExtremos(out Minimo, out Maximo);
            break;
          case ClaseVariable.Entero:
            ((CColumnaEntero)mProveedor.Columnas[OrdenSerie]).ObtenerExtremos(out Minimo, out Maximo);
            break;
          default:
            throw new Exception("No puede agruparse por rango cuando no es real o entero");
        }
        if (Maximo == Minimo)
        {
          Respuesta.Add(new CDatosTorta()
          {
            Nombre = "0 a 0",
            MinimoRango = 0,
            MaximoRango = 0,
            Cantidad = 0,
            Valor = ValorInicial(ModoOrdenadas)
          });
        }
        double Salto = (Maximo - Minimo) / MaxGajos;
        for (Int32 i = 0; i < MaxGajos; i++, Minimo += Salto)
        {
          Respuesta.Add(new CDatosTorta()
          {
            Nombre = CRutinas.ValorATexto(Minimo) + " a " + CRutinas.ValorATexto(Minimo + Salto),
            MinimoRango = Minimo,
            MaximoRango = Minimo + Salto,
            Cantidad = 0,
            Valor = ValorInicial(ModoOrdenadas)
          });
        }
      }
      else
      {

        List<string> ValoresCrudos = ValoresDiferentes(OrdenSerie);

        foreach (string Referencia in ValoresCrudos)
        {
          CDatosTorta Elemento = new CDatosTorta();
          Elemento.NombreOriginal = Referencia;
          switch (mProveedor.Columnas[OrdenSerie].Clase)
          {
            case ClaseVariable.Fecha:
              Elemento.Nombre = TransformarFechaAPantalla(Referencia);
              break;
            case ClaseVariable.Real:
              Elemento.Nombre = TransformarRealAPantalla(Referencia);
              break;
            case ClaseVariable.Booleano:
              Elemento.Nombre =
                  (Elemento.Nombre == "TRUE" ? "Verdadero" : "Falso");
              break;
            default:
              Elemento.Nombre = Referencia;
              break;
          }
          Elemento.Valor = ValorInicial(ModoOrdenadas);
          Elemento.Cantidad = 0;
          Elemento.MinimoRango = double.NaN;
          Elemento.MaximoRango = double.NaN;
          Respuesta.Add(Elemento);
        }

      }

      return Respuesta;

    }

    private void AgregarDatosConTodosPila(ref List<DatosBarraApilada> Respuesta,
        Int32 OrdenAbscisa, Int32 OrdenPila,
        Int32 OrdenOrdenada, ModoAgruparDependiente Agrupamiento,
        List<ElementoPila> ElementosPila,
        List<CCondicionFiltradorCN> CondicionesAdicionales = null)
    {

      if (CondicionesAdicionales != null)
      {
        if (CondicionesAdicionales.Count > 0)
        {
          InicializarProcesoCondicionesAdicionales(CondicionesAdicionales);
        }
        else
        {
          CondicionesAdicionales = null;
        }
      }

      foreach (CLineaComprimida Linea in Datos)
      {
        if (Linea.Vigente)
        {
          if (CondicionesAdicionales == null || VerificaCondicionesFiltrador(Linea, CondicionesAdicionales))
          {
            double Valor = 0;
            switch (Agrupamiento)
            {
              case ModoAgruparDependiente.Cantidad:
                Valor = 1;
                break;
              case ModoAgruparDependiente.Acumulado:
                Valor = mProveedor.ObtenerValorRealLinea(Linea, OrdenOrdenada);
                break;
            }
            Int32 PosPila = Linea.Codigos[OrdenPila];
            Respuesta[Linea.Codigos[OrdenAbscisa]].AgregarValor(Valor, PosPila, mProveedor.Columnas[OrdenPila].ListaValores[PosPila]);
            ElementosPila[PosPila].HayDatos = true;
          }
        }
      }
    }

    private static bool VerificaCondicionFiltrador(CLineaComprimida Linea, CColumnaBase Columna,
          CCondicionFiltradorCN Condicion)
    {
      switch (Columna.Clase)
      {
        case ClaseVariable.Real:
          double Valor = ((CColumnaReal)Columna).ValorReal(Linea.Codigos[Columna.Orden]);
          return (Valor >= Condicion.RangoMinimo && Valor <= Condicion.RangoMaximo);

        case ClaseVariable.Entero:
          if (!double.IsNaN(Condicion.RangoMaximo) && Condicion.RangoMaximo != Condicion.RangoMinimo)
          {
            Int32 ValorI = (Int32)((CColumnaEntero)Columna).Valores[Linea.Codigos[Columna.Orden]];
            return (ValorI >= Condicion.RangoMinimo && ValorI <= Condicion.RangoMaximo);
          }
          else
					{
            return (Condicion.IndicesValoresImpuestos.Contains(Linea.Codigos[Columna.Orden]));
          }
        case ClaseVariable.Texto:
        case ClaseVariable.Booleano:
        case ClaseVariable.Fecha:
          return (Condicion.IndicesValoresImpuestos.Contains(Linea.Codigos[Columna.Orden]));

        default:
          throw new Exception("Columna " + Columna.Nombre + " clase no soportada");

      }

    }

    public bool VerificaCondicionesFiltrador(CLineaComprimida Linea,
        List<CCondicionFiltradorCN> Condiciones)
    {
      for (Int32 i = 0; i < Condiciones.Count; i++)
      {
        if (!VerificaCondicionFiltrador(Linea, mColumnasFiltroAdicionales[i], Condiciones[i]))
        {
          return false;
        }
      }
      return true;
    }

    private static void AjustarIndicesFiltrador(CCondicionFiltradorCN Filtrador, CColumnaBase Columna)
    {
      Filtrador.IndicesValoresImpuestos = new List<int>();
      switch (Columna.Clase)
      {
        case ClaseVariable.Entero:
        case ClaseVariable.Fecha:
        case ClaseVariable.Texto:
        case ClaseVariable.Booleano:
          Filtrador.ValoresImpuestos = (from V in Filtrador.ValoresImpuestos
                                        orderby V
                                        select V).ToList();
          if (Columna != null)
          {
            if (Filtrador.ValoresImpuestos != null && Filtrador.ValoresImpuestos.Count > 0)
            {
              foreach (string V in Filtrador.ValoresImpuestos)
              {
                Filtrador.IndicesValoresImpuestos.Add(Columna.PosicionValorIgual(V));
              }
            }
          }
          break;
      }
    }

    private List<CColumnaBase> mColumnasFiltroAdicionales; // se usan cuando hay condiciones adicionales.

    public void InicializarProcesoCondicionesAdicionales(
          List<CCondicionFiltradorCN> CondicionesFiltrador)
    {
      mColumnasFiltroAdicionales = new List<CColumnaBase>();
      foreach (CCondicionFiltradorCN Cnd in CondicionesFiltrador)
      {
        mColumnasFiltroAdicionales.Add((from C in Proveedor.Columnas
                       where C.Nombre == Cnd.NombreColumna
                       select C).FirstOrDefault());
        AjustarIndicesFiltrador(Cnd, mColumnasFiltroAdicionales.Last());
      }

    }

    private void AgregarDatosConTodos(ref List<CDatosTorta> Respuesta,
        Int32 OrdenAbscisa,
        Int32 OrdenOrdenada, ModoAgruparDependiente Agrupamiento,
        List<CCondicionFiltradorCN> CondicionesAdicionales)
    {

      if (CondicionesAdicionales != null)
      {
        if (CondicionesAdicionales.Count > 0)
        {
          InicializarProcesoCondicionesAdicionales(CondicionesAdicionales);
        }
        else
        {
          CondicionesAdicionales = null;
        }
      }

      foreach (CLineaComprimida Linea in Datos)
      {
        if (Linea.Vigente)
        {
          if (CondicionesAdicionales == null || VerificaCondicionesFiltrador(Linea, CondicionesAdicionales))
          {
            Int32 Posicion = Linea.Codigos[OrdenAbscisa];
            Respuesta[Posicion].TieneDatos = true;
            switch (Agrupamiento)
            {
              case ModoAgruparDependiente.Cantidad:
                Respuesta[Posicion].Valor++;
                break;
              case ModoAgruparDependiente.Acumulado:
              case ModoAgruparDependiente.Media:
                Respuesta[Posicion].Valor += mProveedor.ObtenerValorRealLinea(Linea, OrdenOrdenada);
                Respuesta[Posicion].Cantidad++;
                break;
              case ModoAgruparDependiente.Minimo:
              case ModoAgruparDependiente.Maximo:
                double R = mProveedor.ObtenerValorRealLinea(Linea, OrdenOrdenada);
                if (double.IsNaN(Respuesta[Posicion].Valor))
                {
                  Respuesta[Posicion].Valor = R;
                }
                else
                {
                  if (Agrupamiento == ModoAgruparDependiente.Minimo)
                  {
                    Respuesta[Posicion].Valor = Math.Min(R, Respuesta[Posicion].Valor);
                  }
                  else
                  {
                    Respuesta[Posicion].Valor = Math.Max(R, Respuesta[Posicion].Valor);
                  }
                }
                break;
            }
          }
        }
      }
    }

    private void EliminarPuntosSinDatos(ref List<CDatosTorta> Respuesta)
    {
      for (int i = Respuesta.Count - 1; i >= 0; i--)
      {
        if (!Respuesta[i].TieneDatos)
        {
          Respuesta.RemoveAt(i);
        }
      }
    }

    private void EliminarPuntosSinDatosPila(ref List<DatosBarraApilada> Respuesta)
    {
      for (int i = Respuesta.Count - 1; i >= 0; i--)
      {
        if (Respuesta[i].Elementos.Count == 0)
        {
          Respuesta.RemoveAt(i);
        }
      }
    }

    private void AjustarMedias(ref List<CDatosTorta> Respuesta)
    {
      for (Int32 i = Respuesta.Count - 1; i >= 0; i--)
      {
        CDatosTorta Dato = Respuesta[i];
        if (Dato.Cantidad < 1)
        {
          Respuesta.RemoveAt(i);
        }
        else
        {
          Dato.Valor /= Dato.Cantidad;
        }
      }
    }

    private void OrdenarDescendentePila(ref List<DatosBarraApilada> Respuesta)
    {
      Respuesta.Sort(
        delegate(DatosBarraApilada P1, DatosBarraApilada P2)
        {
          if (P2.ValorTotal == P1.ValorTotal)
          {
            return string.Compare(P1.Nombre, P2.Nombre, StringComparison.InvariantCultureIgnoreCase);
          }
          else
          {
            return P2.ValorTotal.CompareTo(P1.ValorTotal);
          }
        });
    }

    private void OrdenarDescendente(ref List<CDatosTorta> Respuesta)
    {
      Respuesta.Sort(
        delegate(CDatosTorta P1, CDatosTorta P2)
        {
          if (P2.Valor == P1.Valor)
          {
            return string.Compare(P1.Nombre, P2.Nombre, StringComparison.InvariantCultureIgnoreCase);
          }
          else
          {
            return P2.Valor.CompareTo(P1.Valor);
          }
        });
    }

    private double ValorInicial (ModoAgruparDependiente Modo)
    {
      switch (Modo)
      {
        case ModoAgruparDependiente.Acumulado:
        case ModoAgruparDependiente.Cantidad:
        case ModoAgruparDependiente.Media:
          return 0;
        default:
          return double.NaN;
      }
    }

    private void AdicionarDatoTorta(ref double Cantidad,
        ref double Suma, ModoAgruparDependiente Modo,
        CDatosTorta Dato)
    {
      switch (Modo)
      {
        case ModoAgruparDependiente.Cantidad:
        case ModoAgruparDependiente.Acumulado:
          Suma += Dato.Valor;
          break;
        case ModoAgruparDependiente.Media:
          Suma += Dato.Valor * Dato.Cantidad;
          Cantidad += Dato.Cantidad;
          break;
        case ModoAgruparDependiente.Minimo:
          if (double.IsNaN(Suma))
          {
            Suma = Dato.Valor;
          }
          else
          {
            Suma = Math.Min(Suma, Dato.Valor);
          }
          break;
        case ModoAgruparDependiente.Maximo:
          if (double.IsNaN(Suma))
          {
            Suma = Dato.Valor;
          }
          else
          {
            Suma = Math.Max(Suma, Dato.Valor);
          }
          break;
      }
    }

    private double ObtenerValorResto(List<CDatosTorta> Respuesta,
        ModoAgruparDependiente Modo, Int32 PasosMaximos)
    {
      double Cantidad = 0;
      double Suma = ValorInicial(Modo);

      if (PasosMaximos < 1)
      {
        PasosMaximos = CRutinas.PasosMaximos;
      }

      for (int i = PasosMaximos - 1; i < Respuesta.Count; i++)
      {
        AdicionarDatoTorta(ref Cantidad, ref Suma, Modo, Respuesta[i]);
      }

      if (Modo == ModoAgruparDependiente.Media)
      {
        if (Cantidad > 0)
        {
          return Suma / Cantidad;
        }
        else
        {
          return 0;
        }
      }
      else
      {
        return (double.IsNaN(Suma) ? 0 : Suma);
      }

    }

    private void LimitarCantidadElementos(ref List<CDatosTorta> Respuesta,
        ModoAgruparDependiente Modo, Int32 PasosMaximos)
    {

      PasosMaximos = (PasosMaximos < 1 ? CRutinas.PasosMaximos : PasosMaximos);

      if (Respuesta.Count <= PasosMaximos)
      {
        return;
      }

      double ValorResto = ObtenerValorResto(Respuesta, Modo, PasosMaximos);

      for (int i = Respuesta.Count - 1; i >= (PasosMaximos - 1); i--)
      {
        Respuesta.RemoveAt(i);
      }

      // acumula en un adicional.
      CDatosTorta Adicional = new CDatosTorta();
      Adicional.RestoDeDatos = true;
      Adicional.Nombre = "Otros";
      Adicional.Valor = ValorResto;

      Respuesta.Add(Adicional);

    }

    public List<DatosBarraApilada> ArmarSerieApilada(
        Int32 OrdenAbscisa, Int32 OrdenPila, Int32 OrdenOrdenada,
        ModoAgruparDependiente AgrupacionDatos, List<ElementoPila> Pilas,
        List<CCondicionFiltradorCN> CondicionesAdicionales = null)
    {
      try
      {

        // determina las entradas que habra en el listado de respuestas.
        // aun no determina los valores.
        List<CDatosTorta> Barras = DiferentesValoresAbscisas(OrdenAbscisa,
            ModoAgruparIndependiente.Todos,
            AgrupacionDatos, -1);

        List<CDatosTorta> ListaPilas = DiferentesValoresAbscisas(OrdenPila,
            ModoAgruparIndependiente.Todos,
            AgrupacionDatos, -1);

        Pilas.Clear();
        foreach (CDatosTorta ElemPila in ListaPilas)
        {
          Pilas.Add(new ElementoPila()
          {
            Nombre = ElemPila.Nombre
          });
        }

        List<DatosBarraApilada> Respuesta = new List<DatosBarraApilada>();
        foreach (CDatosTorta Barra in Barras)
        {
          DatosBarraApilada Elemento = new DatosBarraApilada()
          {
            Nombre = Barra.Nombre,
            ValorTotal = Barra.Valor
          };
          Respuesta.Add(Elemento);
        }

        // aca agrega los valores de la variable dependiente.
        AgregarDatosConTodosPila(ref Respuesta, OrdenAbscisa, OrdenPila,
            OrdenOrdenada, AgrupacionDatos, Pilas, CondicionesAdicionales);

        List<string> Colores = CRutinas.ArmarListaColores((from P in Pilas
                                                            where P.HayDatos
                                                            select P).Count());
        Int32 i = 0;
        foreach (ElementoPila Elem in Pilas)
        {
          if (Elem.HayDatos)
          {
            Elem.Pincel = Colores[i++];
          }
        }

        EliminarPuntosSinDatosPila(ref Respuesta);
        foreach (DatosBarraApilada Elem in Respuesta)
        {
          Elem.Elementos.Sort(delegate(DatosElementoPila E1, DatosElementoPila E2)
          {
            return E1.Orden.CompareTo(E2.Orden);
          });
        }

        OrdenarDescendentePila(ref Respuesta);

        return Respuesta;
      }
      catch (Exception ex)
      {
        CRutinas.InformarUsuario("Error armando serie de datos",
          CRutinas.MostrarMensajeError(ex));
        return new List<DatosBarraApilada>();
      }

    }

    public List<CDatosTorta> ArmarSerieTodasFechas(
        Int32 OrdenAbscisa, Int32 OrdenOrdenada,
        ModoAgruparIndependiente AgrupacionAbscisas,
        ModoAgruparDependiente AgrupacionOrdenadas,
        Int32 CantidadPasos,
        List<CCondicionFiltradorCN> CondicionesAdicionales = null)
    {
      try
      {
        // determina las entradas que habra en el listado de respuestas.
        // aun no determina los valores.
        List<CDatosTorta> Respuesta = DiferentesValoresAbscisas(OrdenAbscisa,
            AgrupacionAbscisas,
            AgrupacionOrdenadas,
            CantidadPasos);

        // aca agrega los valores de la variable dependiente.
        AgregarDatosConTodos(ref Respuesta, OrdenAbscisa,
            OrdenOrdenada, AgrupacionOrdenadas, CondicionesAdicionales);

        EliminarPuntosSinDatos(ref Respuesta);

        if (AgrupacionOrdenadas == ModoAgruparDependiente.Media)
        {
          AjustarMedias(ref Respuesta);
        }

        if (mProveedor.Columnas[OrdenAbscisa].Clase == ClaseVariable.Fecha)
        {
          while (Respuesta.Count > 0 && Respuesta[0].Nombre.Length == 0)
          {
            Respuesta.RemoveAt(0);
          }
        }

        OrdenarDescendente(ref Respuesta);

        LimitarCantidadElementos(ref Respuesta, AgrupacionOrdenadas, CantidadPasos);

        return Respuesta;
      }
      catch (Exception ex)
      {
        CRutinas.InformarUsuario("Error armando serie de datos", CRutinas.MostrarMensajeError(ex));
        return new List<CDatosTorta>();
      }

    }

    public List<CDatosTorta> ArmarSerieTodosLosDatos(
        Int32 OrdenOrdenada, List<CCondicionFiltradorCN> CondicionesAdicionales = null)
    {
      try
      {

        if (CondicionesAdicionales != null)
        {
          if (CondicionesAdicionales.Count > 0)
          {
            InicializarProcesoCondicionesAdicionales(CondicionesAdicionales);
          }
          else
          {
            CondicionesAdicionales = null;
          }
        }

        List<CDatosTorta> Respuesta = new List<CDatosTorta>();

        foreach (CLineaComprimida Linea in Datos)
        {
          if (Linea.Vigente &&
              (CondicionesAdicionales == null || VerificaCondicionesFiltrador(Linea, CondicionesAdicionales)))
          {
            CDatosTorta Dato = new CDatosTorta();
            Dato.Nombre = mProveedor.Columnas[OrdenOrdenada].ListaValores[Linea.Codigos[OrdenOrdenada]];
            Dato.NombreOriginal = Dato.Nombre;
            Dato.Cantidad = 1;
            Dato.Valor = mProveedor.ObtenerValorRealLinea(Linea, OrdenOrdenada);
            Respuesta.Add(Dato);
          }
        }

        return Respuesta;

      }
      catch (Exception ex)
      {
        CRutinas.InformarUsuario("Error armando serie de datos",
          CRutinas.MostrarMensajeError(ex));
        return new List<CDatosTorta>();
      }

    }

    //public List<CDatosTorta> ArmarSerieSinFiltro(
    //    Int32 OrdenOrdenada)
    //{
    //  try
    //  {

    //    List<CDatosTorta> Respuesta = new List<CDatosTorta>();

    //    foreach (CLineaComprimida Linea in DatosIniciales)
    //    {
    //      CDatosTorta Dato = new CDatosTorta();
    //      Dato.Nombre = mProveedor.ObtenerTextoLinea(Linea,OrdenOrdenada);
    //      Dato.NombreOriginal = Dato.Nombre;
    //      Dato.Cantidad = 1;
    //      Dato.Valor = mProveedor.ObtenerValorRealLinea(Linea,OrdenOrdenada);
    //      Respuesta.Add(Dato);
    //    }

    //    return Respuesta;

    //  }
    //  catch (Exception ex)
    //  {
    //    MessageBox.Show("Error armando serie de datos" +
    //      Environment.NewLine +
    //      ex.Message);
    //    return new List<CDatosTorta>();
    //  }

    //}

    public static Int32 CantidadBarrasHistograma(Int32 CantidadElementos)
    {
      if (CantidadElementos >= 1000)
      {
        return 10 + (Int32)Math.Floor(Math.Log(CantidadElementos / 1000));
      }
      else
      {
        if (CantidadElementos >= 500)
        {
          return 10;
        }
        else
        {
          if (CantidadElementos >= 200)
          {
            return 9;
          }
          else
          {
            if (CantidadElementos >= 100)
            {
              return 8;
            }
            else
            {
              if (CantidadElementos >= 50)
              {
                return 7;
              }
              else
              {
                return Math.Min(6, CantidadElementos);
              }
            }
          }
        }
      }
    }

    public void SumarFiltros(
          List<CCondicionFiltroCN> Condiciones)
    {
      Int32 PasoAnt = -1;
      Int32 CndAnt = -1;
      CCondiciones Cnds = new CCondiciones();
      bool bTodas = false;
      bool bIncluye = false;
      List<CCondiciones> lCnd = new List<CCondiciones>();

      for (Int32 i = mPasos.Count - 1; i > 0; i--)
      {
        mPasos.RemoveAt(i);
      }

      foreach (CCondicionFiltroCN Condicion in Condiciones)
      {
        // cuando cambia el nro de paso, suma a filtrador.
        if (Condicion.Paso != PasoAnt)
        {
          PasoAnt = Condicion.Paso;
          AgregarUnPaso(lCnd, bTodas);
          lCnd.Clear();
          Cnds = new CCondiciones();
          CndAnt = -1;
        }
        // cuando cambia el block de condiciones suma a paso.
        if (Condicion.BlockCondiciones != CndAnt)
        {
          if (Cnds.Condiciones.Count > 0)
          {
            lCnd.Add(Cnds);
            Cnds = new CCondiciones();
          }
          CndAnt = Condicion.BlockCondiciones;
        }
        CCondicion CndLocal = new CCondicion();
        CndLocal.Clase =
            mProveedor.ColumnaNombre(Condicion.CampoCondicion).Clase;
        CndLocal.ColumnaCondicion =
            mProveedor.ColumnaNombre(Condicion.CampoCondicion).Orden;
        CndLocal.Modo = Condicion.ModoDeFiltrar;
        CndLocal.ValorIgual = Condicion.ValorTexto;
        CndLocal.ValorMinimo = Condicion.ValorMinimo;
        CndLocal.ValorMaximo = Condicion.ValorMaximo;
        bTodas = Condicion.DebeCumplirTodasEnBlock;
        bIncluye = Condicion.IncluyeALasQueCumplen;
        Cnds.IncluyeCondiciones = Condicion.IncluyeALasQueCumplen;
        Cnds.Condiciones.Add(CndLocal);
      }
    }

    public static List<DatosTortaColor> ArmarGajosHistogramaSinDatos(
          double Minimo, double Maximo, double Salto)
    {
      List<DatosTortaColor> Respuesta = new List<DatosTortaColor>();
      double Valor = Minimo;
      while (Valor < (Maximo - Salto / 2) ||
            Respuesta.Count == 0) // datos.count==0 es la condicion para que entre algun valor.
      {
        DatosTortaColor Dato = new DatosTortaColor();
        Dato.Datos = new Logicas.CPorcionTorta();
        Dato.Datos.Datos = new CDatosTorta();
        Dato.Datos.Datos.NombreOriginal = CRutinas.ValorATexto(Valor) + "-" +
            CRutinas.ValorATexto(Valor + Salto);
        Dato.Datos.Datos.Nombre = Dato.Datos.Datos.NombreOriginal;
        Dato.Datos.Datos.MinimoRango = Valor;
        Dato.Datos.Datos.MaximoRango = Valor + Salto;
        Dato.Datos.Datos.Valor = 0;
        Dato.Datos.Datos.Cantidad = 0;
        Dato.ColorColumna = "blue";
        Respuesta.Add(Dato);
        Valor += Salto;
      }

      return Respuesta;

    }

    public bool DeterminarRangosHistograma(Int32 OrdenVariable,
          ref double Minimo, ref double Maximo, ref double Salto, double Impuesto, ref Int32 Divisiones,
          bool Aumenta = false)
    {
      Minimo = double.NaN;
      Maximo = double.NaN;
      try
      {
        foreach (CDatosTorta Linea in ArmarSerieSinFiltro(OrdenVariable))
        {
          if (double.IsNaN(Maximo))
          {
            Maximo = Minimo = Linea.Valor;
          }
          else
          {
            Maximo = Math.Max(Maximo, Linea.Valor);
            Minimo = Math.Min(Minimo, Linea.Valor);
          }
        }
        if (Aumenta)
        {
          CRutinas.AjustarExtremosEscala(ref Minimo, ref Maximo);
        }
        Divisiones = CantidadBarrasHistograma(
              DatosIniciales.Count);

        if (double.IsNaN(Minimo))
        {
          return false;
        }

        if (double.IsNaN(Impuesto))
        {

          double SaltoOriginal = (Maximo - Minimo) / (double)Divisiones;

          // divide por 1.25 para compensar multiplicacion en la rutina.
          Salto = CRutinas.BuscarValorEscala(SaltoOriginal / 1.25);
        }
        else
        {
          Salto = Impuesto;
        }

        double MinimoEscala = Math.Floor(Minimo / Salto) * Salto;
        while (MinimoEscala > Minimo)
        {
          MinimoEscala -= Salto;
        }

        double MaximoLocal = Maximo;
        Maximo = Minimo = MinimoEscala;

        while (Maximo <= MaximoLocal)
        {
          Maximo += Salto;
        }

        return true;

      }
      catch (Exception ex)
      {
        CRutinas.InformarUsuario("Error determinando extremos para histograma",
          CRutinas.MostrarMensajeError(ex));
        return false;
      }
    }

    public bool RetrocederUnPaso()
    {
      try
      {
        if (mPasos.Count > 1)
        {
          mPasos.RemoveAt(mPasos.Count - 1);
          return true;
        }
        else
        {
          return false;
        }
      }
      catch (Exception ex)
      {
        CRutinas.InformarUsuario("Error al eliminar paso en filtrado",
          CRutinas.MostrarMensajeError(ex));
        return false;
      }
    }

    public CFiltradorStep AgregarUnPaso(List<CCondiciones> CondicionesPaso,
        bool CumplirTodas)
    {
      CFiltradorStep Paso = new CFiltradorStep();
      Paso.AgregarCondiciones(CondicionesPaso);
      Paso.CumplirTodas = CumplirTodas;
      Paso.FiltrarDatos(Datos, mProveedor.Columnas);
      mPasos.Add(Paso);
      return Paso;
    }

    private bool ExtremosAscisasFechas(ref DateTime Desde, ref DateTime Hasta,
        Int32 OrdenAbscisa)
    {
      try
      {
        if (OrdenAbscisa < 0 || OrdenAbscisa >= mProveedor.Columnas.Count ||
              mProveedor.Columnas[OrdenAbscisa].Valores.Count == 0)
        {
          Desde = CRutinas.FechaMaxima();
          Hasta = CRutinas.FechaMinima();
        }
        else
        {
          Desde = mProveedor.Columnas[OrdenAbscisa].PrimeraFechaValida();
          Hasta = mProveedor.Columnas[OrdenAbscisa].ValorFecha(mProveedor.Columnas[OrdenAbscisa].Valores.Count - 1);
        }
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    private DateTime DeterminarFechaInicialPeriodo(DateTime Desde,
        ModoAgruparIndependiente Agrupacion)
    {
      switch (Agrupacion)
      {
        case ModoAgruparIndependiente.Dia:
          return CRutinas.FechaInicioDia(Desde, false);
        case ModoAgruparIndependiente.Semana:
          return CRutinas.FechaInicialSemana(Desde);
        case ModoAgruparIndependiente.Mes:
          return CRutinas.FechaInicioMes(Desde);
        case ModoAgruparIndependiente.Anio:
          return CRutinas.FechaInicioAnio(Desde);
        default:
          return Desde; // para el compilador.
      }
    }

    private DateTime DeterminarFechaCierrePeriodo(DateTime Desde,
        ModoAgruparIndependiente Agrupacion)
    {
      switch (Agrupacion)
      {
        case ModoAgruparIndependiente.Dia:
          return Desde.AddDays(1);
        case ModoAgruparIndependiente.Semana:
          return Desde.AddDays(7);
        case ModoAgruparIndependiente.Mes:
          if (Desde.Month < 12)
          {
            return new DateTime(Desde.Year, Desde.Month + 1, 1, 0, 0, 0);
          }
          else
          {
            return new DateTime(Desde.Year + 1, 1, 1, 0, 0, 0);
          }
        case ModoAgruparIndependiente.Anio:
          return new DateTime(Desde.Year + 1, 1, 1, 0, 0, 0);
        default:
          throw new Exception("Período incorrecto");
      }
    }

    private string NombreRangoTiempo(DateTime Desde,
        ModoAgruparIndependiente Agrupamiento)
    {
      switch (Agrupamiento)
      {
        case ModoAgruparIndependiente.Dia:
          return CRutinas.FormatearFecha(Desde);
        case ModoAgruparIndependiente.Semana:
          return CRutinas.FormatoSemana(Desde);
        case ModoAgruparIndependiente.Mes:
          return CRutinas.FormatoMes(Desde);
        case ModoAgruparIndependiente.Anio:
          return CRutinas.EnteroLargo(Desde.Year, 4);
        default:
          return "??";
      }
    }

    private void AgregarMedicion(ref CDatosTorta Dato, double Contenido,
        ModoAgruparDependiente Agrupacion)
    {

      Dato.TieneDatos = true;

      switch (Agrupacion)
      {
        case ModoAgruparDependiente.Cantidad:
          Dato.Valor++;
          break;
        case ModoAgruparDependiente.Acumulado:
          Dato.Valor += Contenido;
          break;
        case ModoAgruparDependiente.Media:
          Dato.Cantidad++;
          Dato.Valor += Contenido;
          break;
        case ModoAgruparDependiente.Minimo:
          if (double.IsNaN(Dato.Valor))
          {
            Dato.Valor = Contenido;
          }
          else
          {
            Dato.Valor = Math.Min(Dato.Valor, Contenido);
          }
          break;
        case ModoAgruparDependiente.Maximo:
          if (double.IsNaN(Dato.Valor))
          {
            Dato.Valor = Contenido;
          }
          else
          {
            Dato.Valor = Math.Max(Dato.Valor, Contenido);
          }
          break;
      }
    }

    public List<CDatosTorta> DeterminarGajosFechas(Int32 OrdenAbscisa,
          ModoAgruparIndependiente Agrupacion,
          ModoAgruparDependiente AgrupacionDatos)
    {
      List<CDatosTorta> Respuesta = new List<CDatosTorta>();

      DateTime Desde = DateTime.Now; // para el compilador.
      DateTime Hasta = Desde;
      if (!ExtremosAscisasFechas(ref Desde, ref Hasta, OrdenAbscisa))
      {
        return Respuesta;
      }

      Desde = DeterminarFechaInicialPeriodo(Desde, Agrupacion);

      while (Desde < Hasta || (Desde == Hasta && Respuesta.Count == 0))
      {
        DateTime DesdeH = DeterminarFechaCierrePeriodo(Desde, Agrupacion);
        CDatosTorta Dato = new CDatosTorta();
        Dato.Cantidad = 0;
        Dato.MaximoRango = DesdeH.AddSeconds(-1).ToOADate();
        Dato.MinimoRango = Desde.ToOADate();
        Dato.Valor = ValorInicial(AgrupacionDatos);
        Dato.NombreOriginal = CRutinas.CodificarFechaHora(Desde);
        Dato.Nombre = NombreRangoTiempo(Desde, Agrupacion);
        Respuesta.Add(Dato);
        Desde = DesdeH;
      }

      return Respuesta;

    }

    public void ExtremosColumna(ref double Minimo, ref double Maximo, Int32 Orden)
    {
      if (Orden<0 || Orden>mProveedor.Columnas.Count || Datos==null ||
            mProveedor.Columnas[Orden].Valores.Count==0) {
      Minimo = 2000000000;
      Maximo = -Minimo;
      }
      else {
        Minimo=mProveedor.Columnas[Orden].ValorReal(0);
        Maximo=mProveedor.Columnas[Orden].ValorReal(mProveedor.Columnas[Orden].Valores.Count-1);
      }

    }

    public List<CDatosTorta> DeterminarGajosReales(Int32 OrdenAbscisa,
          ModoAgruparIndependiente Agrupacion,
          ModoAgruparDependiente AgrupacionDatos)
    {
      List<CDatosTorta> Respuesta = new List<CDatosTorta>();

      double Minimo = 0;
      double Maximo = 0;
      ExtremosColumna(ref Minimo, ref Maximo, OrdenAbscisa);
      if (Minimo >= Maximo)
      {
        return Respuesta;
      }

      CRutinas.AjustarExtremosEscala(ref Minimo, ref Maximo);
      double Salto = (Maximo - Minimo) / 10;

      while (Minimo < (Maximo - Salto / 2))
      {
        CDatosTorta Dato = new CDatosTorta();
        Dato.Cantidad = 0;
        Dato.MaximoRango = Minimo + Salto;
        Dato.MinimoRango = Minimo;
        Dato.Valor = ValorInicial(AgrupacionDatos);
        Dato.NombreOriginal = CRutinas.ValorATexto(Minimo) + "-" +
          CRutinas.ValorATexto(Minimo + Salto);
        Dato.Nombre = Dato.NombreOriginal;
        Respuesta.Add(Dato);
        Minimo += Salto;
      }

      return Respuesta;

    }

    private List<CDatosTorta> ArmarSerieDatosPorPeriodoFechas(
        Int32 OrdenAbscisa, Int32 OrdenOrdenada,
        ModoAgruparIndependiente Agrupacion,
        ModoAgruparDependiente AgrupacionDatos,
        Int32 PasosMaximos,
        List<CCondicionFiltradorCN> CondicionesAdicionales = null)
    {

      if (CondicionesAdicionales != null)
      {
        if (CondicionesAdicionales.Count > 0)
        {
          InicializarProcesoCondicionesAdicionales(CondicionesAdicionales);
        }
        else
        {
          CondicionesAdicionales = null;
        }
      }

      if (Agrupacion == ModoAgruparIndependiente.NoDefinido)
      {
        Agrupacion = mProveedor.ObtenerAgrupacionAbscisaFechas(OrdenAbscisa);
      }

      List<CDatosTorta> Respuesta = DeterminarGajosFechas(OrdenAbscisa,
            Agrupacion, AgrupacionDatos);

      foreach (CLineaComprimida Linea in Datos)
      {
        if (Linea.Vigente &&
            (CondicionesAdicionales == null || VerificaCondicionesFiltrador(Linea, CondicionesAdicionales)))
        {
          DateTime Fecha = mProveedor.ObtenerFechaLinea(Linea, OrdenAbscisa);
          double FechaR = Fecha.ToOADate();
          for (Int32 ii = 0; ii < Respuesta.Count; ii++)
          {
            CDatosTorta Dato = Respuesta[ii];
            if (FechaR >= Dato.MinimoRango &&
                FechaR < Dato.MaximoRango)
            {
              AgregarMedicion(ref Dato, mProveedor.ObtenerValorRealLinea(Linea, OrdenOrdenada),
                    AgrupacionDatos);
              break;
            }
          }
        }
      }

      EliminarPuntosSinDatos(ref Respuesta);

      if (AgrupacionDatos == ModoAgruparDependiente.Media)
      {
        AjustarMedias(ref Respuesta);
      }

      OrdenarDescendente(ref Respuesta);

      LimitarCantidadElementos(ref Respuesta, AgrupacionDatos, PasosMaximos);

      return Respuesta;

    }

    private List<CDatosTorta> ArmarSerieDatosPorPeriodoReales(
        Int32 OrdenAbscisa, Int32 OrdenOrdenada,
        ModoAgruparIndependiente Agrupacion,
        ModoAgruparDependiente AgrupacionDatos,
        Int32 PasosMaximos,
        List<CCondicionFiltradorCN> CondicionesAdicionales = null)
    {

      if (CondicionesAdicionales != null)
      {
        if (CondicionesAdicionales.Count > 0)
        {
          InicializarProcesoCondicionesAdicionales(CondicionesAdicionales);
        }
        else
        {
          CondicionesAdicionales = null;
        }
      }

      List<CDatosTorta> Respuesta = DeterminarGajosReales(OrdenAbscisa,
            Agrupacion, AgrupacionDatos);

      CColumnaBase ColumnaAbsc = mProveedor.Columnas[OrdenAbscisa];
      CColumnaBase ColumnaOrd = mProveedor.Columnas[OrdenOrdenada];

      foreach (CLineaComprimida Linea in Datos)
      {
        if (Linea.Vigente &&
            (CondicionesAdicionales == null || VerificaCondicionesFiltrador(Linea, CondicionesAdicionales)))
        {
          try
          {
            double ValorPunto = ColumnaAbsc.ValorReal(Linea.Codigos[OrdenAbscisa]);
            for (Int32 ii = 0; ii < Respuesta.Count; ii++)
            {
              CDatosTorta Dato = Respuesta[ii];
              if (ValorPunto >= Dato.MinimoRango &&
                  ValorPunto < Dato.MaximoRango)
              {
                AgregarMedicion(ref Dato, ColumnaOrd.ValorReal(Linea.Codigos[OrdenOrdenada]),
                    AgrupacionDatos);
                break;
              }
            }
          }
          catch (Exception)
          {
          }
        }
      }

      EliminarPuntosSinDatos(ref Respuesta);

      if (AgrupacionDatos == ModoAgruparDependiente.Media)
      {
        AjustarMedias(ref Respuesta);
      }

      OrdenarDescendente(ref Respuesta);

      LimitarCantidadElementos(ref Respuesta, AgrupacionDatos, PasosMaximos);

      return Respuesta;

    }

    public List<CDatosTorta> ArmarSerieDatosPorPeriodo(
        Int32 OrdenAbscisa, Int32 OrdenOrdenada,
        ModoAgruparIndependiente Agrupacion,
        ModoAgruparDependiente AgrupacionDatos,
        Int32 CantidadPasos,
        List<CCondicionFiltradorCN> CondicionesAdicionales = null)
    {

      try
      {

        switch (mProveedor.Columnas[OrdenAbscisa].Clase)
        {
          case ClaseVariable.Fecha:
            return ArmarSerieDatosPorPeriodoFechas(OrdenAbscisa, OrdenOrdenada,
                Agrupacion, AgrupacionDatos, CantidadPasos, CondicionesAdicionales);
          case ClaseVariable.Entero:
          case ClaseVariable.Real:
            return ArmarSerieDatosPorPeriodoReales(OrdenAbscisa, OrdenOrdenada,
                Agrupacion, AgrupacionDatos,CantidadPasos, CondicionesAdicionales);
          default:
            throw new Exception("No puede agrupar");
        }
      }

      catch (Exception ex)
      {
        CRutinas.InformarUsuario("Error armando serie de datos",
          CRutinas.MostrarMensajeError(ex));
        return new List<CDatosTorta>();
      }

    }

    public List<CDatosPunto> ArmarSeriePuntosFecha(
        Int32 OrdenAbscisa,
        Int32 OrdenOrdenada)
    {
      List<CDatosPunto> Respuesta = new List<CDatosPunto>();

      foreach (CLineaComprimida Linea in Datos)
      {
        CDatosPunto Elemento = new CDatosPunto();
        try
        {
          Elemento.FechaHora = mProveedor.ObtenerFechaLinea(Linea, OrdenAbscisa);
          Elemento.Valor = mProveedor.ObtenerValorRealLinea(Linea,OrdenOrdenada);
          Respuesta.Add(Elemento);
        }
        catch (Exception)
        {
        }
      }

      OrdenarDatosFH(ref Respuesta);

      return Respuesta;
    }

    private void OrdenarDatosFH(ref List<CDatosPunto> Respuesta)
    {
      Respuesta.Sort(
        delegate(CDatosPunto P1, CDatosPunto P2)
        {
          return P1.FechaHora.CompareTo(P2.FechaHora);
        });
    }

    private void OrdenarDatosReales(ref List<CDatosPuntoReal> Respuesta)
    {
      Respuesta.Sort(
        delegate(CDatosPuntoReal P1, CDatosPuntoReal P2)
        {
          return P1.Abscisa.CompareTo(P2.Abscisa);
        });
    }

    public List<CDatosPuntoReal> ArmarSeriePuntosReal(
        Int32 OrdenAbscisa,
        Int32 OrdenOrdenada)
    {
      List<CDatosPuntoReal> Respuesta = new List<CDatosPuntoReal>();

      foreach (CLineaComprimida Linea in Datos)
      {
        CDatosPuntoReal Elemento = new CDatosPuntoReal();
        try
        {
          Elemento.Abscisa = mProveedor.ObtenerValorRealLinea(Linea, OrdenAbscisa);
          Elemento.Valor = mProveedor.ObtenerValorRealLinea(Linea, OrdenOrdenada);
          Respuesta.Add(Elemento);
        }
        catch (Exception)
        {
        }
      }

      OrdenarDatosReales(ref Respuesta);

      return Respuesta;
    }

  }

  public class CValoresDatos
  {
    public bool ConDatos;
    public string Texto;
    public CValoresDatos(string TextoExterno)
    {
      ConDatos = false;
      Texto = TextoExterno;
    }
  }

  public class DatosBarraApilada
  {
    //public pgBarrasH Superior { get; set; }
    public string Nombre { get; set; }
    public double ValorTotal { get; set; }
    public List<DatosElementoPila> Elementos { get; set; }

    public DatosBarraApilada()
    {
      Elementos = new List<DatosElementoPila>();
    }

    public void AgregarValor(double Valor, Int32 PosPila, string Nombre)
    {
      ValorTotal += Valor;
      // Si encuentra el valor, lo agrega.
      for (Int32 Pos = 0; Pos < Elementos.Count; Pos++)
      {
        if (Elementos[Pos].Orden == PosPila)
        {
          Elementos[Pos].Valor += Valor;
          return;
        }
        if (Elementos[Pos].Orden > PosPila)
        {
          Elementos.Insert(Pos, new DatosElementoPila(PosPila, Valor, Nombre));
          return;
        }
      }
      Elementos.Add(new DatosElementoPila(PosPila, Valor, Nombre));
    }

    public double DeterminarAltura(bool Apilada, double AltoTexto)
    {
      if (Apilada)
      {
        return Math.Max(AltoTexto, CRutinas.ANCHO_COLUMNA_H);
      }
      else
      {
        return Elementos.Count * Math.Max(AltoTexto, CRutinas.ANCHO_SUB_COLUMNA_H);
      }
    }

    public static string TextoHint(List<ElementoPila> Pilas, DatosElementoPila Elemento)
    {
      return Pilas[Elemento.Orden].Nombre + " " + CRutinas.ValorATexto(Elemento.Valor);
    }

    //private void DibujarBarrasApiladas(Canvas CV, List<ElementoPila> Pilas, double Defasaje, double AltoTexto,
    //      double Maximo, double AnchoValor)
    //{
    //  double Ordenada = Defasaje + (DeterminarAltura(true, AltoTexto) - pgBarrasH.ANCHO_COLUMNA) / 2;
    //  double Abscisa = 0;
    //  foreach (DatosElementoPila Elemento in Elementos)
    //  {
    //    double Ancho = Elemento.Valor * (CV.ActualWidth - AnchoValor) / Maximo;
    //    if (Ancho > 0)
    //    {
    //      Rectangle RectElemento = new Rectangle()
    //      {
    //        Width = Ancho,
    //        Height = pgBarrasH.ANCHO_COLUMNA,
    //        Fill = Pilas[Elemento.Orden].Pincel
    //      };
    //      if (Elemento.Seleccionado)
    //      {
    //        RectElemento.Stroke = new SolidColorBrush(Colors.Red);
    //        RectElemento.StrokeThickness = 1;
    //      }
    //      RectElemento.HorizontalAlignment = HorizontalAlignment.Left;
    //      RectElemento.VerticalAlignment = VerticalAlignment.Top;
    //      RectElemento.Margin = new Thickness(Abscisa, Ordenada, 0, 0);
    //      RectElemento.Tag = Elemento;
    //      RectElemento.MouseLeftButtonUp += RectElemento_MouseLeftButtonUp;
    //      ToolTipService.SetToolTip(RectElemento, TextoHint(Pilas, Elemento));
    //      CV.Children.Add(RectElemento);
    //      Abscisa += Ancho;
    //    }
    //  }

    //  TextBlock TB = new TextBlock();
    //  TB.Text = ValorTotal.ToString("###,###,###,###,###,##0");
    //  //if (TB.Text.EndsWith("00"))
    //  //{
    //  //  TB.Text = TB.Text.Substring(0, TB.Text.Length - 3);
    //  //}
    //  TB.HorizontalAlignment = HorizontalAlignment.Left;
    //  TB.VerticalAlignment = VerticalAlignment.Top;
    //  TB.Margin = new Thickness(Abscisa + 5, Ordenada + (pgBarrasH.ANCHO_COLUMNA - AltoTexto) / 2, 0, 0);
    //  CV.Children.Add(TB);

    //}

    //void RectElemento_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    //{
    //  if (sender != null && sender is Rectangle)
    //  {
    //    Rectangle Rect = (Rectangle)sender;
    //    Superior.AjustarSeleccion(this, (DatosElementoPila)Rect.Tag);
    //  }
    //}

    //private void DibujarBarrasParalelas(Canvas2DContext CV, List<ElementoPila> Pilas, double Defasaje, double AltoTexto,
    //      double Maximo, double AnchoValor)
    //{
    //  double Ordenada = Defasaje + (DeterminarAltura(false, AltoTexto) - Elementos.Count * pgBarrasH.ANCHO_SUB_COLUMNA) / 2;
    //  foreach (DatosElementoPila Elemento in Elementos)
    //  {
    //    double Ancho = Elemento.Valor * (CV.ActualWidth - AnchoValor) / Maximo;
    //    if (Ancho > 0)
    //    {
    //      Rectangle RectElemento = new Rectangle()
    //      {
    //        Width = Ancho,
    //        Height = pgBarrasH.ANCHO_SUB_COLUMNA,
    //        Fill = Pilas[Elemento.Orden].Pincel
    //      };
    //      if (Elemento.Seleccionado)
    //      {
    //        RectElemento.Stroke = new SolidColorBrush(Colors.Red);
    //        RectElemento.StrokeThickness = 1;
    //      }
    //      RectElemento.HorizontalAlignment = HorizontalAlignment.Left;
    //      RectElemento.VerticalAlignment = VerticalAlignment.Top;
    //      RectElemento.Margin = new Thickness(0, Ordenada, 0, 0);
    //      RectElemento.Tag = Elemento;
    //      ToolTipService.SetToolTip(RectElemento, TextoHint(Pilas, Elemento));
    //      RectElemento.MouseLeftButtonUp += RectElemento_MouseLeftButtonUp;
    //      CV.Children.Add(RectElemento);
    //    }

    //    TextBlock TB = new TextBlock();
    //    TB.Text = Elemento.Valor.ToString("###,###,###,###,##0");
    //    //if (TB.Text.EndsWith(".00"))
    //    //{
    //    //  TB.Text = TB.Text.Substring(0, TB.Text.Length - 3);
    //    //}
    //    TB.HorizontalAlignment = HorizontalAlignment.Left;
    //    TB.VerticalAlignment = VerticalAlignment.Top;
    //    TB.Margin = new Thickness(Ancho + 5, Ordenada + Math.Min(0, (pgBarrasH.ANCHO_SUB_COLUMNA - AltoTexto) / 2), 0, 0);
    //    CV.Children.Add(TB);

    //    Ordenada += pgBarrasH.ANCHO_SUB_COLUMNA;

    //  }

    //}

    //public void DibujarBarras(Canvas2DContext CV, bool Apiladas, List<ElementoPila> Pilas, double Defasaje, double AltoTexto,
    //      double Maximo, double AnchoValor)
    //{
    //  if (Apiladas)
    //  {
    //    DibujarBarrasApiladas(CV, Pilas, Defasaje, AltoTexto, Maximo, AnchoValor);
    //  }
    //  else
    //  {
    //    DibujarBarrasParalelas(CV, Pilas, Defasaje, AltoTexto, Maximo, AnchoValor);
    //  }
    //}

  }

  public class ElementoPila
  {
    public string Nombre { get; set; }
    public string Pincel { get; set; }
    public bool HayDatos { get; set; }
    public double Ancho { get; set; }
  }

  public class DatosElementoPila
  {
    public bool Seleccionado { get; set; }
    public Int32 Orden { get; set; }
    public string Nombre { get; set; }
    public double Valor { get; set; }

    public DatosElementoPila(Int32 Orden0, double Valor0, string Nombre0)
    {
      Orden = Orden0;
      Valor = Valor0;
      Nombre = Nombre0;
      Seleccionado = false;
    }

  }

  public class CDatosPuntoReal
  {
    public double Abscisa { get; set; }
    public double Valor { get; set; }
  }

  public class CDatosPunto
  {
    public DateTime FechaHora { get; set; }
    public double Valor { get; set; }
  }



}
