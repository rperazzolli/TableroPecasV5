using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.Model;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaMapaGradiente : ComponentBase, IDisposable
  {

    private CCapaWFSCN mCapaVinculo; // cuando el vinculo es por una capa, leer la capa.

    public CLogicaMapaGradiente()
		{
      mPosicionMapBing = -1;
		}

    public async void Dispose()
    {
      object[] Args = new object[1];
      Args[0] = mPosicionMapBing;
      try
      {
        await JSRuntime.InvokeAsync<string>("LiberarMap", Args);
      }
      catch (Exception)
      {
        // Ignora lo que puede ocurrir porque al cerrar puede dar una excepcion.
      }
    }

    [Parameter]
    public CDatoIndicador Indicador { get; set; }

    [Parameter]
    public Int32 CodigoElementoDimension { get; set; }

    [Parameter]
    public Int32 CodigoCapa { get; set; }

    [Parameter]
    public Plantillas.CLinkMapa Mapa { get; set; }

    [Parameter]
    public string Direccion { get; set; } = "MapaGradiente";

    [Parameter]
    public double Abscisa { get; set; } = -999;

    [Parameter]
    public double Ordenada { get; set; } = -999;

    [Parameter]
    public double Ancho { get; set; } = -999;

    [Parameter]
    public double Alto { get; set; } = -999;

    [Parameter]
    public Int32 NivelFlotante { get; set; }

    public bool DatosCompletos { get; set; } = false;

    public string EstiloMapaComponente
    {
      get
      {
        return "width: " + Math.Floor(Ancho).ToString() + "px; left: " + Math.Floor(Abscisa).ToString() + "px; top: " +
            Math.Floor(Ordenada).ToString() + "px; height: " +
            Math.Floor(Alto - 25).ToString() +
            "px; margin-top: 28px; overflow: hidden; background-color: white; position: absolute;";
      }
    }

    private List<Clases.CDatosSC> mDatosSC;
    private CDatosSC SubconsultaCodigo(Int32 Codigo)
    {
      return (from SC in mDatosSC
              where SC.Codigo == Codigo
              select SC).FirstOrDefault();
    }

    private CCapaWSSCN mCapa = null;
    private CCapaWFSCN mCapaWFS = null;
    private CVinculoIndicadorCompletoCN mVinculo = null;

    private List<CParametroExt> ArmarParametrosSC(string PrmFecha)
    {
      List<CParametroExt> Respuesta = new List<CParametroExt>();
      if (PrmFecha.Length > 0)
      {
        CParametroExt Prm = new CParametroExt();
        Prm.Nombre = PrmFecha;
        Prm.Tipo = "ftDateTime";
        Prm.TieneQuery = false;
        Prm.ValorDateTime = DateTime.Now;
        Respuesta.Add(Prm);
      }
      return Respuesta;
    }

    private bool DesdeCoordenadas()
    {
      return (mCapa != null && mCapa.Modo == ModoGeoreferenciar.Coordenadas);
    }

    [Inject]
    public HttpClient Http { get; set; }

    private async Task CargarSCAsync()
    {
      if (mCapa.Formula != null)
      {
        mDatosSC = new List<CDatosSC>();
        string[] Frm = mCapa.Formula.Split('@', StringSplitOptions.RemoveEmptyEntries);
        for (Int32 i = 1; i < Frm.Length; i += 4)
        {
          if (Frm[i].Length > 0)
          {
            string Texto = "[" + Frm[i] + "]";
            Int32 Codigo0;
            if (Int32.TryParse(Frm[i + 1], out Codigo0))
            {
              if (Frm[0].IndexOf(Texto) >= 0 && SubconsultaCodigo(Codigo0) == null)
              {
                RespuestaDatasetBin Respuesta = await Contenedores.CContenedorDatos.LeerDetalleSubconsultaAsync(Http,
                    Codigo0, ArmarParametrosSC(Frm[i + 3].Trim()));
                if (Respuesta != null && Respuesta.RespuestaOK)
                {
                  CDatosSC Elemento = new CDatosSC(Frm[i], Codigo0, Respuesta.Datos);
                  if (Frm[i + 2] == "Y")
                  {
                    Elemento.CorregirReferencias(mCapaWFS);
                  }
                  mDatosSC.Add(Elemento);
                }
              }
            }
          }
        }
      }

      mListaPrm.Clear();
      await LeerParametrosWFSSiCorrespondeAsync();

    }

    private string UbicarProximoParametro(string Frm, ref Int32 PosIni)
    {
      Int32 Pos = Frm.IndexOf("[Prm-", PosIni);
      if (Pos > 0)
      {
        Pos += 5;
        Int32 Pos2 = Frm.IndexOf("]", Pos);
        if (Pos2 > 0)
        {
          PosIni = Pos2 + 1;
          return Frm.Substring(Pos, Pos2 - Pos);
        }
      }
      PosIni = Frm.Length + 1;
      return "";
    }

    private bool ParametroYaLeido(string Nombre)
    {
      return (from P in mListaPrm
              where P.Parametro == Nombre
              select P).FirstOrDefault() != null;
    }

    private async Task LeerParametroWFSAsync(string NombrePrm)
    {
      try
      {
        RespuestaTextos RespPrm = await Http.GetFromJsonAsync<RespuestaTextos>(
            "api/ParametrosWSF/LeerParametroWFS?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
            "&CodigoCapa=" + mCapaWFS.Codigo.ToString() +
            "&Parametro=" + NombrePrm);
        if (!RespPrm.RespuestaOK)
        {
          throw new Exception("Al leer prm " + RespPrm.MsgErr);
        }
        CDatosPrmWFS Datos = new CDatosPrmWFS(NombrePrm);
        Datos.ParesValores = RespPrm.Contenidos;
        mListaPrm.Add(Datos);

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    private List<CDatosPrmWFS> mListaPrm = new List<CDatosPrmWFS>();

    private async Task LeerParametrosWFSSiCorrespondeAsync()
    {
      if (mCapa.Formula != null)
      {
        Int32 Pos = 0;
        string PrmEncontrado;
        while ((PrmEncontrado = UbicarProximoParametro(mCapa.Formula, ref Pos)) != "")
        {
          if (!ParametroYaLeido(PrmEncontrado))
          {
            await LeerParametroWFSAsync(PrmEncontrado);
            return;
          }
        }
      }

    }

    public bool SinDatos { get; set; } = false;

    private async Task LeerDatosNecesariosAsync()
    {
      try
      {
        if (Proveedor==null && Indicador == null)
        {
          SinDatos = true;
          return;
        }

        if (Mapa != null && Proveedor == null && Mapa.Filtros != null && Mapa.Filtros.Filtrador != null)
        {
          Proveedor = Mapa.Filtros.Filtrador.Proveedor;
        }

        // leer datos indicador.
        if (Proveedor == null && Indicador != null)
        {
          RespuestaDatasetBin RespuestaDatos = await Http.GetFromJsonAsync<RespuestaDatasetBin>(
            "api/Dataset/GetProveedor?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
            "&Indicador=" + Indicador.Codigo.ToString() +
            "&Dimension=" + Indicador.Dimension.ToString()+
            "&Elemento="+CodigoElementoDimension.ToString());
          if (!RespuestaDatos.RespuestaOK)
					{
            throw new Exception("Al leer detalle indicador " + RespuestaDatos.MsgErr);
					}
          byte[] Datos = RespuestaDatos.Datos;
          if (Datos.Length == 0)
          {
            SinDatos = true;
            return;
          }
          else
          {
            Proveedor = new CProveedorComprimido(ClaseElemento.NoDefinida, -1);
            Proveedor.ProcesarDatasetBinario(Datos, false);
          }
        }

        // leer capas.
        RespuestaCapasWSS RespCapas = await Http.GetFromJsonAsync<RespuestaCapasWSS>(
          "api/Capas/ListarCapasWSS?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
            "&ClaseElemento=" + ((Int32)(Indicador.Codigo > 0 ? ClaseElemento.Indicador : Proveedor.ClaseOrigen)).ToString() +
            "&CodigoElemento=" + (Indicador.Codigo > 0 ? Indicador.Codigo : Proveedor.CodigoOrigen).ToString());
        if (!RespCapas.RespuestaOK)
        {
          throw new Exception(RespCapas.MsgErr);
        }
        mCapa = (from C in RespCapas.Capas
                 where C.Codigo == CodigoCapa
                 select C).FirstOrDefault();
        if (mCapa == null)
        {
          throw new Exception("No encuentra capa WSS");
        }

        // lee capa WFS.
        RespuestaCapaWFS RespWFS = await Http.GetFromJsonAsync<RespuestaCapaWFS>(
          "api/Capas/LeerCapaWFS?URL=" + Contenedores.CContenedorDatos.UrlBPI +
          "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
          "&Codigo=" + mCapa.CapaWFS.ToString() +
          "&ForzarWeb=N");
        if (!RespWFS.RespuestaOK)
        {
          throw new Exception(RespWFS.MsgErr);
        }
        mCapaWFS = RespWFS.Capa;
        if (mCapaWFS == null)
        {
          throw new Exception("No encuentra capa WFS");
        }

        if (mCapa.Modo == ModoGeoreferenciar.Vinculo)
        {
          RespuestaDetalleVinculo RespVinculo = await Http.GetFromJsonAsync<RespuestaDetalleVinculo>(
            "api/Indicadores/LeerVinculoDeUnIndicadorCodigo?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
            "&Vinculo=" + mCapa.Vinculo.ToString());
          if (!RespVinculo.RespuestaOK)
          {
            throw new Exception("Al leer vínculo " + RespVinculo.MsgErr);
          }
          mVinculo = RespVinculo.Vinculo;
          switch (mVinculo.Vinculo.ClaseVinculada)
          {
            case ClaseVinculo.Areas:
            case ClaseVinculo.Lineas:
            case ClaseVinculo.Marcador:
              RespuestaCapaWFS RespWFSV = await Http.GetFromJsonAsync<RespuestaCapaWFS>(
                "api/Capas/LeerCapaWFS?URL=" + Contenedores.CContenedorDatos.UrlBPI +
                "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
                "&Codigo=" + mVinculo.Vinculo.CodigoVinculado.ToString() +
                "&ForzarWeb=N");
              if (!RespWFSV.RespuestaOK)
              {
                throw new Exception(RespWFSV.MsgErr);
              }
              mCapaVinculo = RespWFSV.Capa;
              break;
            default:
              await CargarSCAsync();
              //          PonerEnPantalla();
              break;
          }
        }
        else
        {
          await CargarSCAsync();
        }

        // leer detalle indicador.
        // leer capa.
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    private CFiltrador mFiltrador = null;

    [Parameter]
    public CFiltrador Filtrador
    {
      get { return mFiltrador; }
      set
      {
        if (mFiltrador != value)
        {
          mFiltrador = value;
          if (mFiltrador != null && mFiltrador.Proveedor != null)
          {
            Proveedor = mFiltrador.Proveedor;
          }
        }
      }
    }

    private CProveedorComprimido mProveedor = null;
    public CProveedorComprimido Proveedor
    {
      get { return mProveedor; }
      set
      {
        if (mProveedor != value)
        {
          if (mProveedor != null)
          {
            mProveedor.AlAjustarDependientes -= FncAjustarVentana;
          }
          mProveedor = value;
          if (mProveedor != null)
          {
            mProveedor.AlAjustarDependientes += FncAjustarVentana;
          }
        }
      }
    }

    private void FncAjustarVentana(object sender)
    {
      StateHasChanged();
    }

    private void AcumularValor(ref List<CParValores> Lista, Int32 Pos, Int32 PosColDatos, Int32 Posicion)
    {
      switch (Proveedor.Columnas[PosColDatos].Clase)
      {
        case ClaseVariable.Entero:
          Lista[Pos].ValorElemento += (Int32)Proveedor.Columnas[PosColDatos].Valores[Posicion];
          break;
        case ClaseVariable.Real:
          Lista[Pos].ValorElemento += (double)Proveedor.Columnas[PosColDatos].Valores[Posicion];
          break;
      }
      Lista[Pos].Cantidad++;
    }

    private const string VALOR_RESTO = "NO_REFERENCIADOS";

    private List<CParValores> CrearParesValores()
    {
      // Si hay formula, tratar de ajustar los valores.
      List<CParValores> Respuesta = new List<CParValores>();
      //      CVinculadorCN Vinculo = CCcontenedorDatos.ContenedorUnico.VinculoDesdeCodigo(mCapa.Vinculo);
      CPosicionadorDatos Posicionador = new CPosicionadorDatos();
      Posicionador.PorCodigo = true;
      if (mCapa.Modo == ModoGeoreferenciar.Vinculo)
      {
        if (mVinculo == null)
        {
          throw new Exception("No encuentra vinculo");
        }
        Posicionador.ProcesarAgregadoDeColumna("CAg" + mCapa.Vinculo.ToString(),
              mCapa.ColumnaGeoreferencia, mVinculo, mVinculo.Vinculo.ColumnaLat,
              mVinculo.Vinculo.ColumnaLng, (mCapaVinculo == null ? mCapaWFS : mCapaVinculo),
              Proveedor, true, VALOR_RESTO, mCapa.Rango, mCapaWFS, true);
      }
      else
      {
        Posicionador.ProcesarAgregadoDeColumnaDesdeCoordenadas("CAg" + mCapa.Codigo.ToString(),
          Proveedor.OrdenColumnaNombre(mCapa.ColumnaLatitud),
          Proveedor.OrdenColumnaNombre(mCapa.ColumnaLongitud), mCapaWFS, Proveedor,
          "S/Referencia", mCapa.Rango);
      }
      Int32 PosColAg = Proveedor.Columnas.Count - 1;
      Int32 PosColDatos = Proveedor.ColumnaNombre(mCapa.ColumnaValor).Orden;

      // Crea las entradas a partir de los valores de la columna de partida.
      foreach (string Valor in Proveedor.Columnas[PosColAg].ListaValores)
      {
        Respuesta.Add(new CParValores(Valor));
      }

      // Acumula los valores del dataset.
      foreach (CLineaComprimida Linea in Proveedor.DatosVigentes)
      {
        AcumularValor(ref Respuesta, Linea.Codigos[PosColAg], PosColDatos, Linea.Codigos[PosColDatos]);
      }

      foreach (CParValores Par in Respuesta)
      {
        double Valor = double.NaN;
        switch (mCapa.Agrupacion)
        {
          case ModoAgruparDependiente.Acumulado:
            Valor = Par.ValorElemento;
            break;
          case ModoAgruparDependiente.Cantidad:
            Valor = Par.Cantidad;
            break;
          case ModoAgruparDependiente.Media:
            Valor = (Par.Cantidad > 0 ? Par.ValorElemento / Par.Cantidad : double.NaN);
            break;
        }

        Par.ValorElemento = Valor;

      }
      return Respuesta;
    }

    private void CrearReferenciasLineales(List<CParValores> Pares)
    {
      if (Pares.Count > 0)
      {
        double Minimo = (from P in Pares
                         where !double.IsNaN(P.ValorElemento) && P.CodigoElemento != VALOR_RESTO
                         select P.ValorElemento).Min();
        double Maximo = (from P in Pares
                         where !double.IsNaN(P.ValorElemento) && P.CodigoElemento != VALOR_RESTO
                         select P.ValorElemento).Max();
        CRutinas.AjustarExtremosEscala(ref Minimo, ref Maximo);
        mCapa.Referencias = new List<double>();
        for (Int32 i = 0; i < mCapa.Segmentos; i++)
        {
          mCapa.Referencias.Add(Minimo + i * (Maximo - Minimo) / mCapa.Segmentos);
        }
      }
    }

    private void CrearReferenciasCuantiles(List<CParValores> Pares)
    {
      if (Pares.Count > 0)
      {
        List<double> Valores = (from P in Pares
                                where !double.IsNaN(P.ValorElemento) && P.CodigoElemento != VALOR_RESTO
                                orderby P.ValorElemento
                                select P.ValorElemento).ToList();
        mCapa.Referencias = new List<double>();
        Int32 PosAnterior = 0;
        for (Int32 i = 1; i < mCapa.Segmentos; i++)
        {
          while (PosAnterior < (Valores.Count * i / mCapa.Segmentos))
          {
            PosAnterior++;
          }
          if (mCapa.Referencias.Count == 0 ||
              Valores[PosAnterior] != mCapa.Referencias[mCapa.Referencias.Count - 1])
          {
            mCapa.Referencias.Add(Valores[PosAnterior]);
          }
        }
      }
    }

    private byte ModificarComponenteColor(byte Componente, double Fraccion)
    {
      return (byte)(Componente + Math.Floor((double)(255 - Componente) * (1 - Fraccion)));
    }

    private string ColorEscalonGradiente(Int32 Escalon)
    {
      double Fraccion = Math.Min(1, (double)Escalon / (double)(mCapa.Referencias.Count + 1));
      return "rgba(" +
          ModificarComponenteColor(mCapa.ColorCompuestoR, Fraccion).ToString() + ", " +
          ModificarComponenteColor(mCapa.ColorCompuestoG, Fraccion).ToString() + ", " +
          ModificarComponenteColor(mCapa.ColorCompuestoB, Fraccion).ToString() + ", " +
          CRutinas.FloatVStr(mCapa.ColorCompuestoA).ToString() + ")";
    }

    /// <summary>
    /// En pares ya viene un valor por cada elemento. Aca se le asigna el color.
    /// </summary>
    /// <param name="Pares"></param>
    private void AjustarColoresPares(List<CParValores> Pares)
    {
      if (mCapa.Intervalos == ClaseIntervalo.Indicador)
      {
        foreach (CParValores Par in Pares)
        {

          if (double.IsNaN(Par.ValorElemento))
          {
            Par.ColorElemento = ColorBandera.SinDatos;
          }
          else
          {
            if (mCapa.Sobresaliente > mCapa.Minimo)
            {
              if (Par.ValorElemento >= mCapa.Sobresaliente)
              {
                Par.ColorElemento = ColorBandera.Azul;
              }
              else
              {
                if (Par.ValorElemento >= mCapa.Satisfactorio)
                {
                  Par.ColorElemento = ColorBandera.Verde;
                }
                else
                {
                  if (Par.ValorElemento >= mCapa.Minimo)
                  {
                    Par.ColorElemento = ColorBandera.Amarillo;
                  }
                  else
                  {
                    Par.ColorElemento = ColorBandera.Rojo;
                  }
                }
              }
            }
            else
            {
              if (Par.ValorElemento <= mCapa.Sobresaliente)
              {
                Par.ColorElemento = ColorBandera.Azul;
              }
              else
              {
                if (Par.ValorElemento <= mCapa.Satisfactorio)
                {
                  Par.ColorElemento = ColorBandera.Verde;
                }
                else
                {
                  if (Par.ValorElemento <= mCapa.Minimo)
                  {
                    Par.ColorElemento = ColorBandera.Amarillo;
                  }
                  else
                  {
                    Par.ColorElemento = ColorBandera.Rojo;
                  }
                }
              }
            }
          }
        }
      }
      else
      {
        foreach (CParValores Par in Pares)
        {
          Int32 Pos = 0;
          foreach (double Valor in mCapa.Referencias)
          {
            if (Par.ValorElemento > Valor)
            {
              Pos++;
            }
            else
            {
              break;
            }
          }
          Par.ColorImpuesto = ColorEscalonGradiente(Pos);
        }
      }
    }

    private void CrearViewerLayer()
    {
      CCapaComodin CapaComodin = new CCapaComodin();
      CapaComodin.Opacidad = (mCapa.Intervalos == ClaseIntervalo.Indicador ? 0.5 : 1);
      CapaComodin.CapaWFS = mCapaWFS;
      CapaComodin.Clase = ClaseCapa.WFS;
      // Crea un valor por cada area. Hasta aca no considera la formula.
      CapaComodin.Pares = CrearParesValores();

      CapaComodin.AgregarFormulaWSS(mCapa, mDatosSC, mListaPrm);

      // Si se trata de intervalos lineales o cuantiles, determina los gradientes.
      switch (mCapa.Intervalos)
      {
        case ClaseIntervalo.Lineal:
          CrearReferenciasLineales(CapaComodin.Pares);
          break;
        case ClaseIntervalo.Cuantiles:
          CrearReferenciasCuantiles(CapaComodin.Pares);
          break;
      }

      AjustarColoresPares(CapaComodin.Pares);

      mProyectoBing.CapasCompletas.Add(CapaComodin);

    }

    private CProyectoBing mProyectoBing;

    private void CrearProyectoFicticio()
    {
      mProyectoBing = new CProyectoBing();
      //CCapaComodin Capa = new CCapaComodin();
      //Capa.CapaWFS = mCapaWFS;
      //Capa.CapaWIS = new CCapaWISCN();
      //Capa.CapaWIS.CodigoWFS = mCapaWFS.Codigo;
      //mProyectoBing.CapasCompletas.Add(Capa);
//      mProyectoBing.OrdenarCapasCompletas();
      CrearViewerLayer();
    }

    private bool mbReubicarCentro = false;
    public bool ReubicarCentro
    {
      get { return mbReubicarCentro; }
      set
      {
        if (mbReubicarCentro != value)
        {
          mbReubicarCentro = value;
          if (mbReubicarCentro)
          {
            StateHasChanged();
          }
        }
      }
    }

    private Logicas.CLogicaMapaGradiente mComponenteGradiente = null;
    public Logicas.CLogicaMapaGradiente ComponenteGradiente
    {
      get { return mComponenteGradiente; }
      set
      {
        if (value != mComponenteGradiente)
        {
          mComponenteGradiente = value;
        }
      }
    }

    [Inject]
    IJSRuntime JSRuntime { get; set; }

    private bool mbGraficando = false;
    private object OBJ_LOCK = new object();

    private void ObtenerReferenciasIndicador()
    {
      if (mCapa.Minimo > mCapa.Sobresaliente)
      {
        mReferencias.Add("> " + CRutinas.ValorATexto(mCapa.Minimo));
        mReferencias.Add(CRutinas.ValorATexto(mCapa.Minimo) + " A " + CRutinas.ValorATexto(mCapa.Satisfactorio));
        mReferencias.Add(CRutinas.ValorATexto(mCapa.Satisfactorio) + " A " + CRutinas.ValorATexto(mCapa.Sobresaliente));
        mReferencias.Add("< " + CRutinas.ValorATexto(mCapa.Sobresaliente));
      }
      else
      {
        mReferencias.Add("< " + CRutinas.ValorATexto(mCapa.Minimo));
        mReferencias.Add(CRutinas.ValorATexto(mCapa.Minimo) + " A " + CRutinas.ValorATexto(mCapa.Satisfactorio));
        mReferencias.Add(CRutinas.ValorATexto(mCapa.Satisfactorio) + " A " + CRutinas.ValorATexto(mCapa.Sobresaliente));
        mReferencias.Add("> " + CRutinas.ValorATexto(mCapa.Sobresaliente));
      }
      mColores.Add(CRutinas.ColorBanderaATexto(ColorBandera.Rojo, false));
      mColores.Add(CRutinas.ColorBanderaATexto(ColorBandera.Amarillo, false));
      mColores.Add(CRutinas.ColorBanderaATexto(ColorBandera.Verde, false));
      mColores.Add(CRutinas.ColorBanderaATexto(ColorBandera.Azul, false));
    }

    private void ObtenerReferenciasSaltos()
    {
      Int32 Escalon = 0;
      foreach (double Valor in mCapa.Referencias)
      {
        mReferencias.Add(CRutinas.ValorATexto(Valor));
        mColores.Add(ColorEscalonGradiente(Escalon++));
      }
    }

    private List<string> mReferencias = new List<string>();
    private List<string> mColores = new List<string>();

    private async Task AjustarReferenciasAsync()
    {
      mReferencias.Clear();
      mColores.Clear();
      if (mCapa.Intervalos == ClaseIntervalo.Indicador)
      {
        ObtenerReferenciasIndicador();
      }
      else
      {
        ObtenerReferenciasSaltos();
      }
      Int32 Largo = (from Refe in mReferencias
                     select Refe.Length).Max();
      object[] Args = new object[3];
      Args[0] = 12;
      Args[1] = "serif";
      Args[2] = Largo;
      double R = await JSRuntime.InvokeAsync<double>("FuncionesJS.ObtenerDimensiones", Args);
      mAnchoEscala = R + 52;
      mAltoEscala = mReferencias.Count * 30 + 22; // 10 entre elementos.
    }

    public BECanvas CanvasReferencia;
    private Canvas2DContext mContexto;

    private double mDimensionCaracter = -1;

    private async Task DibujarReferenciasAsync()
    {
      if (CanvasReferencia == null)
      {
        return;
      }

      mContexto = await Blazor.Extensions.CanvasContextExtensions.CreateCanvas2DAsync(CanvasReferencia);

      await mContexto.BeginBatchAsync();

      try
      {

        await mContexto.ClearRectAsync(0, 0, mAnchoEscala, mAltoEscala);

        await mContexto.SetFontAsync("12px serif");

        if (mDimensionCaracter < 0) {
          TextMetrics Medida = await mContexto.MeasureTextAsync("H");
          mDimensionCaracter = Medida.Width;
        }
        await mContexto.SetLineWidthAsync(1);
        await mContexto.SetStrokeStyleAsync("black");
        await mContexto.SetFillStyleAsync("#000000");
        await mContexto.StrokeRectAsync(0, 0, mAnchoEscala - 1, mAltoEscala - 1);

        for (Int32 i=0;i<mReferencias.Count;i++)
        {
          double Ordenada = 11 + 30 * i;
          await mContexto.SetFillStyleAsync(mColores[i]);
          await mContexto.FillRectAsync(11, Ordenada, 20, 20);
          Ordenada += (20 + mDimensionCaracter) / 2;
          await mContexto.SetFillStyleAsync("black");
          await mContexto.FillTextAsync(mReferencias[i], 41, Ordenada, mAnchoEscala - 42);
        }

      }
      finally
      {
        await mContexto.EndBatchAsync();
      }

    }

    public string NombreReferencias
    {
      get { return "RefGrad_" + CodigoCapa.ToString(); }
    }

    private double mAnchoEscala = 0;
    private double mAltoEscala = 0;

    public long AnchoReferencia
    {
      get { return (long)mAnchoEscala; }
    }

    public long AltoReferencia
    {
      get { return (long)mAltoEscala; }
    }

    public string EstiloReferencias
    {
      get
      {
        return "width: " + Math.Min(mAnchoEscala, Ancho - 5).ToString() + "px; height: " +
          Math.Min(mAltoEscala, Alto - 5).ToString() + "px; " +
          "position: absolute; margin-left: 5px; margin-top: 5px; background-color: transparent; z-index: 25; ";
      }
    }

    private Int32 mPosicionMapBing = -1;

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {

      bool bRedibujar = false;

      lock (OBJ_LOCK)
      {
        if (mbGraficando)
        {
          return;
        }
        else
        {
          mbGraficando = true;
        }
      }
      try
      {
        if (!DatosCompletos)
        {
          // Leer los datos correspondientes.
          await LeerDatosNecesariosAsync();
          // determinar valores escala.
          if (mProyectoBing == null)
          {
            CrearProyectoFicticio();
          }
          await AjustarReferenciasAsync();

          //await mProyectoBing.CargarDetallesCapasAsync();
          mProyectoBing.UbicarCentro(Abscisa < -998 ? Contenedores.CContenedorDatos.AnchoPantalla : Ancho,
              Abscisa < -998 ? (Contenedores.CContenedorDatos.AltoPantalla - 45) : (Alto - 45));
          DatosCompletos = true;
          bRedibujar = true;
          return;
        }
        else
        {
          if (ReubicarCentro)
          {
            mProyectoBing.UbicarCentro(Abscisa < -998 ? Contenedores.CContenedorDatos.AnchoPantalla : Ancho,
                Abscisa < -998 ? (Contenedores.CContenedorDatos.AltoPantalla - 45) : (Alto - 45));
            ReubicarCentro = false;
          }
          object[] Args = new object[5];
          Args[0] = mPosicionMapBing;
          Args[1] = '#' + Direccion;
          Args[2] = mProyectoBing.LatCentro;
          Args[3] = mProyectoBing.LngCentro;
          Args[4] = mProyectoBing.NivelZoom;
          try
          {
            if (mPosicionMapBing < 0)
            {
              string CodigoMapa = await JSRuntime.InvokeAsync<string>("loadMapRetPos", Args);
              mPosicionMapBing = Int32.Parse(CodigoMapa);
            }
            await mProyectoBing.DibujarGradientesAsync(JSRuntime, mPosicionMapBing);
            if (Ancho > 800 && Alto > 500)
            {
              // dibujar las referencias.
              await DibujarReferenciasAsync();
            }
          }
          catch (Exception ex)
          {
            CRutinas.DesplegarMsg(ex);
          }
        }
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
      finally
      {
        mbGraficando = false;
        if (bRedibujar)
				{
          StateHasChanged();
				}
      }
    }

  }

}
