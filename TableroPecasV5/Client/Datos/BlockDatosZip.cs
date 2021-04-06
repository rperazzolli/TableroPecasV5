using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Datos
{
  public class BlockDatosZip
  {
    public Int32 Periodo;
    public DateTime FechaHora;
    public double ValorDeterminado;
    public CProveedorComprimido Proveedor;

    public BlockDatosZip(Int32 CodigoIndicador)
    {
      ValorDeterminado = double.NaN;
      Proveedor = new CProveedorComprimido(ClaseElemento.Indicador, CodigoIndicador);
    }

    public BlockDatosZip(ClaseElemento ClaseIndicador, Int32 CodigoIndicador)
    {
      ValorDeterminado = double.NaN;
      Proveedor = new CProveedorComprimido(
        (ClaseIndicador == ClaseElemento.NoDefinida ? ClaseElemento.Indicador : ClaseIndicador), CodigoIndicador);
    }

    /// <summary>
    /// Extrae un valor de un dataset respetando el filtro y modo de agrupar.
    /// </summary>
    /// <param name="Filtros"></param>
    /// <param name="Columnas"></param>
    /// <param name="PosColDep"></param>
    /// <param name="ModoDeAgrupar"></param>
    /// <returns></returns>
    public double FiltrarValor(List<GrupoCondiciones> Filtros,
        List<CColumnaBase> Columnas,
        Int32 PosColDep, ModoAgruparDependiente ModoDeAgrupar)
    {

      try
      {
        if (PosColDep < 0 || PosColDep >= Columnas.Count)
        {
          return 0;
        }

        CFiltradorPasos Filtrador = new CFiltradorPasos(Proveedor.ClaseOrigen, Proveedor.CodigoOrigen);
        Filtrador.Proveedor.Columnas = Columnas;
        Filtrador.PonerDatosIniciales(Proveedor.Columnas, Proveedor.DatosVigentes);
        foreach (GrupoCondiciones Filtro in Filtros)
        {
          Filtrador.AgregarUnPaso(Filtro.Condiciones, false);
        }

        // si lo que se busca es la cantidad, no hace falta mas.
        if (ModoDeAgrupar == ModoAgruparDependiente.Cantidad)
        {
          return Filtrador.Datos.Count;
        }

        double Respuesta;

        switch (ModoDeAgrupar)
        {
          case ModoAgruparDependiente.Acumulado:
          case ModoAgruparDependiente.Cantidad:
          case ModoAgruparDependiente.Media:
            Respuesta = 0;
            break;
          default:
            Respuesta = double.NaN;
            break;
        }

        foreach (Datos.CLineaComprimida Linea in Filtrador.Datos)
        {
          if (Linea.Vigente)
          {
            double R;
            try
            {
              R = Filtrador.Proveedor.ObtenerValorRealLinea(Linea, PosColDep);
            }
            catch (Exception)
            {
              continue;
            }
            switch (ModoDeAgrupar)
            {
              case ModoAgruparDependiente.Maximo:
                if (double.IsNaN(Respuesta))
                {
                  Respuesta = R;
                }
                else
                {
                  Respuesta = Math.Max(Respuesta, R);
                }
                break;
              case ModoAgruparDependiente.Minimo:
                if (double.IsNaN(Respuesta))
                {
                  Respuesta = R;
                }
                else
                {
                  Respuesta = Math.Min(Respuesta, R);
                }
                break;
              default:
                Respuesta += R;
                break;
            }
          }
        }

        if (ModoDeAgrupar == ModoAgruparDependiente.Media)
        {
          if (Filtrador.Datos.Count == 0)
          {
            Respuesta = 0;
          }
          else
          {
            Respuesta /= (double)Filtrador.Datos.Count;
          }
        }

        return (double.IsNaN(Respuesta) ? 0 : Respuesta);

      }
      catch (Exception ex)
      {
        TableroPecasV5.Client.Rutinas.CRutinas.DesplegarMsg(ex);
        return double.NaN;
      }

    }

    public void DeterminarValor(List<GrupoCondiciones> Filtros,
        List<CColumnaBase> Columnas,
        Int32 PosColDep,
        ModoAgruparDependiente ModoDeAgrupar)
    {
      ValorDeterminado = FiltrarValor(Filtros, Columnas, PosColDep, ModoDeAgrupar);
    }

  }

  public class GrupoCondiciones
  {
    public List<CCondiciones> Condiciones;

    public GrupoCondiciones()
    {
      Condiciones = new List<CCondiciones>();
    }

  }

}
