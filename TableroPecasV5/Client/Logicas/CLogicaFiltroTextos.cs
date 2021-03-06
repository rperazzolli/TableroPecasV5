using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.JSInterop;
using Blazorise;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Shared;
using System.Threading.Tasks;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaFiltroTextos : CBaseGrafico, IDisposable
  {

    public event CBaseGrafico.FncEventoTextoBool AlCerrarFiltro;
    public event CBaseGrafico.FncEventoRefresco AlCambiarAncho;

//    private ModoAgruparDependiente mModo;
    private CFiltrador mFiltrador;
    private List<CElementoFilaAsociativa> mFilasPantalla = new List<CElementoFilaAsociativa>();

    [CascadingParameter]
    public CLogicaIndicador IndicadorContenedor { get; set; }

    private CLogicaFiltroTextos mDatosAntes = null; // para cuando se va hacia atras.
    public void CopiarDatos(CLogicaFiltroTextos Otro)
    {
      Abscisa = Otro.Abscisa;
      Ordenada = Otro.Ordenada;
      Ancho = Otro.Ancho;
      MinimoRango = Otro.MinimoRango;
      MaximoRango = Otro.MaximoRango;
      ColumnaAgrupadora = Otro.ColumnaAgrupadora;
      mDatosAntes = Otro;
    }

    public void Dispose()
    {
      if (mFiltrador != null)
      {
        if (mFiltrador.Proveedor != null)
        {
          mFiltrador.Proveedor.AlAjustarDependientes -= Proveedor_AlAjustarDependientes;
        }
        mFiltrador = null;
      }
    }

    private void Refrescar(object Referencia)
		{
      mFiltrador.AjustarAnchoBandasAzules(Ancho - 65);
      StateHasChanged();
		}

    private double mAncho0 = 185;

		protected override Task OnAfterRenderAsync(bool firstRender)
		{
      if (AlCambiarAncho == null)
			{
        AlCambiarAncho = Refrescar;
			}
      mFiltrador.AjustarAnchoBandasAzules(Ancho - 65);
      if (Ancho != mAncho0)
			{
        mAncho0 = Ancho;
        StateHasChanged();
			}
			return base.OnAfterRenderAsync(firstRender);
		}

		[CascadingParameter]
    Logicas.CLogicaIndicador Pagina { get; set; }

    public int AbscisaAbajo { get; set; }
    public int OrdenadaAbajo { get; set; }

    public void EventoMouseAbajo(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
      Pagina.PonerElementoEncima(false, false, false, Filtrador.Columna.Orden, -1);
      AbscisaAbajo = (int)e.ScreenX;
      OrdenadaAbajo = (int)e.ScreenY;
      Pagina.Refrescar();
    }

    public double MinimoRango { get; set; } = 0;
    public double MaximoRango { get; set; } = 1000;

    public List<CElementoFilaAsociativa> FilasEnPantalla
    {
      get { return mFilasPantalla; }
    }

    [Parameter]
    public CLinkFiltros Link { get; set; }

    public List<CElementoFilaAsociativaFecha> FilasFechaEnPantallaVisibles
    {
      get
      {
        return (from CElementoFilaAsociativaFecha F in mFilasPantalla
                where F.Visible
                select F).ToList();
      }
    }

		public void RecibirDrop(Microsoft.AspNetCore.Components.Web.DragEventArgs e)
    {
      if (Pagina.LineaDrag != null)
      {
        if (Filtrador != null && Filtrador.Proveedor != null)
        {
          switch (Pagina.LineaDrag.Columna.Clase)
          {
            case ClaseVariable.Real:
            case ClaseVariable.Entero:
              Filtrador.ModoAgrupar = ModoAgruparDependiente.Acumulado;
              break;
            default:
              Filtrador.ModoAgrupar = ModoAgruparDependiente.Cantidad;
              break;
          }
          Filtrador.ColumnaValor = Pagina.LineaDrag.Columna;

          Filtrar();

          if (Ancho < 270)
          {
            Ancho = 270;
            if (AlCambiarAncho != null)
            {
              AlCambiarAncho(this);
            }
          }

        }
        Pagina.LineaDrag = null;
      }
    }

    public bool ColumnaReal
    {
      get { return (mFiltrador != null && mFiltrador.Columna != null && mFiltrador.Columna.Clase == ClaseVariable.Real); }
    }

    public bool ColumnaFecha
    {
      get { return mFiltrador.DatoEsFecha(); }
    }

    //    private FncMostrarCaption mFncCaption;
    public CFiltrador Filtrador
    {
      get { return mFiltrador; }
      set
      {
        if (value != mFiltrador)
        {
          if (mFiltrador!=null && mFiltrador.Proveedor != null)
          {
            mFiltrador.Proveedor.AlAjustarDependientes -= Proveedor_AlAjustarDependientes;
          }
          mFiltrador = value;
          if (mFiltrador!=null && mFiltrador.Proveedor != null)
          {
            mFiltrador.Proveedor.AlAjustarDependientes += Proveedor_AlAjustarDependientes;
          }
          ArmarFilasPantalla();
        }
      }
    }

    public void EventoColAgrup()
		{
      //
		}

    private string mszColumnaAgrupadora = "";
    public string ColumnaAgrupadora
    {
      get { return mszColumnaAgrupadora; }
      set
      {
        if (value == null)
        {
          value = "";
        }
        if (mszColumnaAgrupadora != value)
        {
          mszColumnaAgrupadora = value;
        }
      }
    }

    public void FncAgrupar(Int32 Modo)
    {
      if (mszColumnaAgrupadora.Length > 0) {

        switch (Modo)
        {
          case 1:
            Filtrador.ModoAgrupar = ModoAgruparDependiente.Acumulado;
            break;
          case 2:
            Filtrador.ModoAgrupar = ModoAgruparDependiente.Media;
            break;
          case 3:
            Filtrador.ModoAgrupar = ModoAgruparDependiente.Cantidad;
            break;
          case 4:
            Filtrador.ModoAgrupar = ModoAgruparDependiente.Minimo;
            break;
          case 5:
            Filtrador.ModoAgrupar = ModoAgruparDependiente.Maximo;
            break;
          default:
            return;
        }
        CerrarVentanaAgrupar();
        if (Filtrador != null && Filtrador.Proveedor != null)
        {
          Filtrador.ColumnaValor = Filtrador.Proveedor.ColumnaNombre(mszColumnaAgrupadora);
        }
      }
      else
      {
        Filtrador.ColumnaValor = null;
      }

      Filtrar();

      if (Ancho != 270)
      {
        Ancho = 270;
        if (AlCambiarAncho != null)
        {
          AlCambiarAncho(this);
        }
      }

    }

    private void AjustarCantidadesAsociadas()
    {
      if (mFiltrador.ColumnaValor != null)
      {
        CFiltrador FiltroValor = mFiltrador.Proveedor.ObtenerFiltroColumna(mFiltrador.ColumnaValor.Nombre);
        if (FiltroValor != null)
        {
          foreach (CElementoFilaAsociativa Fila in mFiltrador.Filas)
          {
            Fila.Cantidad = Fila.PosicionesAsociadas.Count;
            Fila.Valor = Fila.PosicionesAsociadasVigentes.Count;
          }
        }
        else
        {
          mFiltrador.AsociarConColumnaValor();
          foreach (CElementoFilaAsociativa Fila in mFiltrador.Filas)
          {
            Fila.Cantidad = Fila.PosicionesAsociadas.Count;
            Fila.Valor = Fila.PosicionesAsociadasVigentes.Count;
          }
        }
      }
    }

    public Int32 CodigoValor(string Valor, bool MenorIgual = false)
    {
      if (Valor.Length == 0)
      {
        return (mFiltrador.Columna.ListaValores[0].Length == 0 ? 0 : -1);
      }
      else
      {
        return (mFiltrador.Columna.ListaValores.Count == 0 ? -1 :
          (MenorIgual ? mFiltrador.Columna.PosicionValorMenorIgual(Valor, false) :
          mFiltrador.Columna.PosicionValorMayorIgualTexto(Valor, false)));
      }
    }

    private Int32 ObtenerIndiceFecha(DateTime Fecha, bool MenorIgual)
    {
      return CodigoValor(Rutinas.CRutinas.CodificarFechaHora(Fecha), MenorIgual);
    }

    private void CopiarSeleccionFilasFecha()
		{
      if (FilasFechaEnPantallaVisibles != null && mDatosAntes.FilasFechaEnPantallaVisibles != null)
      {
        foreach (CElementoFilaAsociativaFecha Otra in (from F in mDatosAntes.FilasFechaEnPantallaVisibles
                                                       where F.Seleccionado
                                                       select F).ToList())
        {
          CElementoFilaAsociativaFecha Fila = (from F in FilasFechaEnPantallaVisibles
                                               where F.Nombre == Otra.Nombre
                                               select F).FirstOrDefault();
          if (Fila != null)
          {
            Fila.Seleccionado = true;
          }
        }
      }
    }

    private void ArmarFilasFecha()
    {
      mFiltrador.CrearFilasFecha();

      mFilasPantalla = Filtrador.Filas;

    }

    public void ArmarFilasPantalla()
    {

      if (ColumnaFecha)
      {
        ArmarFilasFecha();
      }
      else
      {
        List<string> Seleccionadas;
        if (mDatosAntes == null)
        {
          Seleccionadas = (mFilasPantalla == null || mFilasPantalla.Count == 0 ? Filtrador.ValoresSeleccionados :
            (from F in mFilasPantalla
             where F.Seleccionado
             select F.Nombre).ToList());
        }
        else
				{
          Seleccionadas = (from F in mDatosAntes.FilasEnPantalla
                           where F.Seleccionado
                           select F.Nombre).ToList();
				}
        mDatosAntes = null;

        mFilasPantalla.Clear();
        mFiltrador.AjustarPorcentajes();
        mFilasPantalla.AddRange(mFiltrador.Filas);

        OrdenarElementos();

        mFiltrador.AjustarAnchoBandasAzules(Ancho - 65);

        AjustarCantidades();

        foreach (CElementoFilaAsociativa Fila in mFilasPantalla)
        {
          Fila.Seleccionado = (Seleccionadas.Contains(Fila.Nombre));
        }

        OrdenarElementos();

      }

    }

    /// <summary>
    /// Ajusta el textblock con las cantidades de elementos seleccionadas y totales.
    /// </summary>
    public void AjustarCantidades()
    {
      Int32 Suma = 0;
      double SumaPrc = 0;
      double SumaComp = 0;
      double SumaAsociada = 0;
//      bool bAlguno = false;
      bool Acumular = (Filtrador.ModoAgrupar == ModoAgruparDependiente.Acumulado || Filtrador.ModoAgrupar == ModoAgruparDependiente.Cantidad);
      foreach (CElementoFilaAsociativa Fila in mFiltrador.Filas)
      {
        Suma += (Fila.Vigente ? 1 : 0);
        if (!double.IsNaN(Fila.Valor))
        {
          if (Fila.Vigente)
          {
            SumaPrc += Fila.Cantidad;
          }
          SumaComp += Fila.Cantidad;
          if (Acumular && Fila.Vigente)
          {
//            bAlguno = true;
            SumaAsociada += Fila.Valor;
          }
        }
      }
      //tbCantidades.Text = Suma.ToString() + "/" + mFiltrador.Filas.Count.ToString() +
      //  (bAlguno ? (" (" + CRutinas.ValorATexto(SumaAsociada) + ")") : "");
      //tbPorcAct.Text = (SumaComp <= 0 ? "--" : (SumaPrc / SumaComp).ToString("##0.0 %"));
    }

    public void OrdenarElementos()
    {
      if (mFiltrador.ColumnaValor == null)
      {
        mFilasPantalla.Sort(delegate (CElementoFilaAsociativa E1, CElementoFilaAsociativa E2)
        {
          if (E1.Vigente == E2.Vigente)
          {
            if (E1.Seleccionado == E2.Seleccionado)
            {
              if (E1.Seleccionado || E1.EstaAsociadoAFiltro == E2.EstaAsociadoAFiltro)
              {
                switch (mFiltrador.Columna.Clase)
                {
                  case ClaseVariable.Entero:
                    return Int32.Parse(E1.Nombre).CompareTo(Int32.Parse(E2.Nombre));
                  default:
                    return string.Compare(E1.Nombre, E2.Nombre,
                        StringComparison.InvariantCultureIgnoreCase);
                }
              }
              else
              {
                return (E1.EstaAsociadoAFiltro ? -1 : 2);
              }
            }
            else
            {
              return (E1.Seleccionado ? -1 : 1);
            }
          }
          else
          {
            return (E1.Vigente ? -1 : 1);
          }
        });
      }
      else
      {
        mFilasPantalla.Sort(delegate (CElementoFilaAsociativa E1, CElementoFilaAsociativa E2)
        {
          if (E1.Vigente == E2.Vigente)
          {
            if (!E1.Vigente)
            {
              if (E1.Seleccionado == E2.Seleccionado)
              {
                if (E1.Seleccionado || E1.EstaAsociadoAFiltro == E2.EstaAsociadoAFiltro)
                {
                  if (E1.Cantidad != 0 || E2.Cantidad != 0)
                  {
                    return E2.Cantidad.CompareTo(E1.Cantidad);
                  }
                  else
                  {
                    return string.Compare(E1.Nombre, E2.Nombre,
                        StringComparison.InvariantCultureIgnoreCase); // si no estan vigentes, compara los nombres.
                  }
                }
                else
                {
                  return (E1.EstaAsociadoAFiltro ? -1 : 1);
                }
              }
              else
              {
                return (E1.Seleccionado ? -1 : 1);
              }
            }
            else
            {
              if (E2.Valor != E1.Valor)
              {
                return E2.Valor.CompareTo(E1.Valor);
              }
              else
              {
                return E2.Cantidad.CompareTo(E1.Cantidad);
              }
            }
          }
          else
          {
            return (E1.Vigente ? -1 : 1);
          }
        });
      }
    }

    public void AjustarValores()
    {
      if (mFiltrador.ColumnaValor != null && mFiltrador.Proveedor != null)
      {
        if (Filtrador.ModoAgrupar == ModoAgruparDependiente.Cantidad)
        {
          AjustarCantidadesAsociadas();
          ArmarFilasPantalla();
          return;
        }
        foreach (CElementoFilaAsociativa Elemento in mFiltrador.Filas)
        {
          switch (Filtrador.ModoAgrupar)
          {
            case ModoAgruparDependiente.Maximo:
            case ModoAgruparDependiente.Minimo:
              Elemento.Valor = double.NaN;
              break;
            default:
              Elemento.Valor = 0;
              Elemento.Cantidad = 0;
              break;
          }
        }

        if (mFiltrador.DatoEsFecha())
        {
          mFiltrador.SumarValoresPorFecha(mFiltrador.Proveedor, Filtrador.ModoAgrupar);
        }
        else
        {
          mFiltrador.SumarValoresPorLinea(mFiltrador.Proveedor, Filtrador.ModoAgrupar);
        }

        // ajusta las medias.
        mFiltrador.AjustarMedias(Filtrador.ModoAgrupar);

        ArmarFilasPantalla();

      }

    }

    private Modal mModal = null;
    public Modal ModalAgrupar
    {
      get { return mModal; }
      set { mModal = value; }
    }

    public void CerrarVentanaAgrupar()
    {
      if (mModal != null)
      {
        mModal.Hide();
      }
    }

    public void Proveedor_AlAjustarDependientes(object sender)
    {

      ArmarFilasPantalla();

      AjustarValores();

      StateHasChanged();

    }

    public void FncAbrirCerrar(CElementoFilaAsociativaFecha Fila)
    {
      if (Fila.Dependientes.Count > 0)
      {
        Fila.CambiarVisibilidadDependientes(!Fila.Dependientes[0].Visible);
      }
    }

    //public void CargarDatosTexto()
    //{
    //  mFiltrador.Filas.Clear();
    //  Int32 Pos = 0;
    //  foreach (string Nombre in mFiltrador.Columna.ListaValores)
    //  {
    //    mFiltrador.AgregarFila(Nombre, Pos++);
    //  }

    //  mFiltrador.AjustarCodigosSeleccionados();

    //}

    public string EstiloNombre
    {
      get
      {
//        _ = DeterminarDimensionesAsync();
        return "max-width: calc(100% - " + (ColumnaFecha ? 60 : 75).ToString() + "px;)";
      }
    }

    [Inject]
    public IJSRuntime JSRuntime { get; set; }

  //  private async Task DeterminarDimensionesAsync()
		//{
  //    object[] Args = new object[1];
  //    Args[0] = CLogicaIndicador.IdFiltro(Link);
  //    string Dimensiones = await JSRuntime.InvokeAsync<string>("FuncionesJS.getRectangulo", Args);
  //    List<double> Valores = Rutinas.CRutinas.ListaAReales(Dimensiones);
  //    Link.Ancho = (Int32)Valores[2];
  //  }

    public string EstiloFilaTexto(CElementoFilaAsociativa Fila)
    {
      if (Fila.HayValor)
      {
        Int32 AnchoValores = Fila.AnchoTextoValor();
        return "color: black;" +
          (Fila.Seleccionado ? " font-weight: bold;" : "") +
          "width: calc(100% - " + (10 + AnchoValores).ToString() + "px);";
      }
      else
      {
        return "color: black;" +
          (Fila.Seleccionado ? " font-weight: bold;" : "") +
          "width: calc(100% - 10px);";
      }

    }

    public string EstiloFilaTextoFecha(CElementoFilaAsociativaFecha Fila)
    {
      Int32 ii = 35 + 15 * Fila.Saltos;
      return "width: calc(100% - " + (ii + 15).ToString() + "px); color: black;" + (Fila.Seleccionado ? " font-weight: bold;" : "") +
        " margin-left: " + ii.ToString() + "px;";
    }

    public string EstiloMasFecha(CElementoFilaAsociativaFecha Fila)
    {
      return "cursor: pointer; margin-left: " + (15 * Fila.Saltos + 5).ToString() + "px;";
    }

    public string EstiloCheckFecha(CElementoFilaAsociativaFecha Fila)
    {
      return "cursor: pointer; margin-left: " + (15 * Fila.Saltos + 20).ToString() + "px;";
    }

    public string EstiloBandaAbajo(CElementoFilaAsociativa Fila)
    {
      return "background-color: " + (Fila.Vigente ? "white;" : "lightgray;");
    }

    public string EstiloFilaValor(CElementoFilaAsociativa Fila)
    {
      return "width: calc(100% - 10px); color: black; font-family: 'Microsoft Sans Serif';";
    }

    public void AjustarVigenciaFilas()
    {
      // cuando se trata de enteros o reales, no corresponde.
      switch (Filtrador.Columna.Clase)
      {
        //        case ClaseVariable.Entero:
        case ClaseVariable.Real:
          return;
      }

      foreach (CElementoFilaAsociativa Elemento in Filtrador.Filas)
      {
        Elemento.Vigente = false;
      }

      if (mFiltrador.Columna.Clase == ClaseVariable.Fecha)
      {
//        AjustarVigenciaFilasFecha();
      }
      else
      {
        Int32 Orden = Filtrador.Columna.Orden;
        foreach (CLineaComprimida Linea in Filtrador.Proveedor.Datos)
        {
          if (Linea.Vigente)
          {
            Int32 Pos = Linea.Codigos[Orden];
            if (Pos >= 0 && Pos < Filtrador.Filas.Count)
            {
              Filtrador.Filas[Linea.Codigos[Orden]].Vigente = true;
            }
          }
        }
      }

    }

    public string EstiloLista
    {
      get
      {
        return "height: " + (Alto - 35).ToString() + "px; ";
      }
    }

    public List<CFiltradorStep> FiltrosBlocks { get; set; }

    private List<Int32> DeterminarCodigosSeleccionados()
    {
      List<Int32> Respuesta = new List<int>();
      foreach (CElementoFilaAsociativa Linea in mFiltrador.Filas)
      {
        if (Linea.Seleccionado)
        {
          Respuesta.Add(Linea.CodigoOrden);
        }
      }
      return Respuesta;
    }

    public string EstiloBandaAzul(CElementoFilaAsociativa Fila)
    {
      return "width: " + Fila.AnchoAzul.ToString() + "px;";
    }

    public string ValorDerecha(CElementoFilaAsociativa Fila)
    {
      return Fila.TextoValor;
    }

    public string EstiloBoton(Int32 Clase, Int32 Pixels = 50)
    {
      string Base = "width: " + Pixels.ToString() + "px; ";
      CColumnaBase ColRefe = (from C in mFiltrador.Proveedor.Columnas
                              where C.Nombre==mszColumnaAgrupadora
                              select C).FirstOrDefault();
      if (ColRefe == null)
      {
        return Base + "pointer-events: none;";
      }
      switch (ColRefe.Clase)
      {
        case ClaseVariable.Entero:
        case ClaseVariable.Real: return Base;
        default: return Base + (Clase == 3 ? "" : "pointer-events: none;");
      }
    }

    public void Agrupar()
    {
      if (ModalAgrupar!=null)
      {
        ModalAgrupar.Show();
      }
    }

    public void Agrandar()
		{
      Link.Ancho += Link.Ancho / 4;
      IndicadorContenedor.Refrescar();
		}

    public void ImponerFiltrosAsociados(List<Int32> Codigos)
    {
      for (Int32 i = 0; i < mFiltrador.Filas.Count; i++)
      {
        mFiltrador.Filas[i].UsadoParaFiltrarAsociaciones = false;
        mFiltrador.Filas[i].EstaAsociadoAFiltro = Codigos.Contains(i);
      }
    }

    private void AgregarRangosElemento(CElementoFilaAsociativaFecha Fila, ref List<string> Rangos)
    {
      if (Fila.Seleccionado)
      {
        Rangos.Add(Fila.Nombre);
      }
      else
      {
        foreach (CElementoFilaAsociativaFecha FilaL in Fila.Dependientes)
        {
          AgregarRangosElemento(FilaL, ref Rangos);
        }
      }
    }

    private void ObtenerValoresSeleccinadosFecha(ref List<string> Rangos)
    {
      List<CRangoFechas> Respuesta = new List<CRangoFechas>();
      foreach (CElementoFilaAsociativaFecha Fila in (from CElementoFilaAsociativaFecha F in mFilasPantalla
                                                     where F.Saltos==0
                                                     select F).ToList())
      {
        AgregarRangosElemento(Fila, ref Rangos);
      }
    }

    public void Filtrar()
    {
      IndicadorContenedor.Procesando = true;
      IndicadorContenedor.Refrescar();
      try
      {
        if (ColumnaReal)
        {
          try
          {
            Filtrador.ValorMinimo = Rutinas.CRutinas.FloatVStr(MinimoRango);
            Filtrador.ValorMaximo = Rutinas.CRutinas.FloatVStr(MaximoRango);
          }
          catch (Exception ex)
          {
            Rutinas.CRutinas.InformarUsuario("Al extraer rango", ex);
            return;
          }
        }
        else
        {
          if (ColumnaFecha)
          {
            List<string> ValSelecc = new List<string>();
            ObtenerValoresSeleccinadosFecha(ref ValSelecc);
            Filtrador.ValoresSeleccionados = ValSelecc;
          }
          else
          {
            Filtrador.CodigosSeleccionados = DeterminarCodigosSeleccionados();
            //          Filtrador.Proveedor.FiltrarPorAsociaciones();
            //      Filtrador.FiltrarDataset();


            if (mFiltrador.ColumnaValor != null)
            {
              CFiltrador FiltroValor = mFiltrador.Proveedor.FiltroParaColumna(mFiltrador.ColumnaValor.Nombre);
              if (FiltroValor != null)
              {
                List<CElementoFilaAsociativa> Filas = new List<CElementoFilaAsociativa>();
                Filas.AddRange(mFilasPantalla);
                foreach (CElementoFilaAsociativa Fila in Filas)
                {
                  if (Fila.UsadoParaFiltrarAsociaciones)
                  {
                    Fila.UsadoParaFiltrarAsociaciones = false;
                    LimpiarFiltrosAsociados();
                    if (FiltroValor != null)
                    {
                      // marca en el otro los elementos asociados y desmarca los usads para filtrar.
                      FiltroValor.AjustarFiltrosAsociados(mFiltrador.CodigosFiltroAsociados());
                      //                FiltroValor.Filtro.ImponerFiltrosAsociados(mFiltrador.CodigosFiltroAsociados());
                    }
                  }
                }
              }
            }
          }
        }

        mFiltrador.Proveedor.FiltrarPorAsociaciones();
        //      Filtrador.FiltrarDataset();
      }
      finally
			{
        IndicadorContenedor.Procesando = false;
        IndicadorContenedor.Refrescar();
      }

    }

    public void LimpiarFiltrosAsociados()
    {
      for (Int32 i = 0; i < mFiltrador.Filas.Count; i++)
      {
        mFiltrador.Filas[i].EstaAsociadoAFiltro = false;
      }
    }

    public void Cerrar()
    {
      if (mFiltrador != null)
      {
        foreach (CElementoFilaAsociativa Fila in mFiltrador.Filas)
        {
          Fila.Seleccionado = false;
        }
        mFiltrador.ValorMinimo = "";
        mFiltrador.ValorMaximo = "";
        mFiltrador.CodigosSeleccionados.Clear();
      }

      if (AlCerrarFiltro != null && mFiltrador != null && mFiltrador.Columna != null)
      {
        AlCerrarFiltro(mFiltrador.Columna.Nombre, true);
      }
    }

    public void Ocultar()
    {
      if (AlCerrarFiltro != null)
      {
        AlCerrarFiltro(mFiltrador.Columna.Nombre, false);
      }
    }

  }

}
