using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Clases
{
  public class CDatosSC
  {
    public string Nombre { get; set; }
    public Int32 Codigo { get; set; }
    public CProveedorComprimido Proveedor { get; set; }

    public CDatosSC(string Nombre0, Int32 Codigo0, byte[] Datos)
    {
      Nombre = Nombre0;
      Codigo = Codigo0;
      if (Datos != null)
      {
        Proveedor = new CProveedorComprimido(ClaseElemento.SubConsulta, Codigo);
        Proveedor.ProcesarDatasetBinario(Datos, false);
      }
    }

    public double ObtenerValorCorrespondiente(string CodigoBuscado)
    {
      if (Proveedor.Columnas[0].Clase == ClaseVariable.Texto &&
          (Proveedor.Columnas[1].Clase == ClaseVariable.Real ||
          Proveedor.Columnas[1].Clase == ClaseVariable.Entero))
      {
        foreach (CLineaComprimida Linea in Proveedor.Datos)
        {
          if (((string)Proveedor.Columnas[0].TextoIndice(Linea.Codigos[0])) == CodigoBuscado)
          {
            return Proveedor.Columnas[1].ValorReal(Linea.Codigos[1]);
          }
        }
      }
      return double.NaN;
    }

		public void CorregirReferencias(CCapaWFSCN Capa)
		{
			CColumnaBase Columna = Proveedor.Columnas[0];
			foreach (CAreaWFSCN Area in Capa.Areas)
			{
				Columna.ReemplazarValorTexto(Area.Nombre, Area.Codigo);
			}

			foreach (CPuntoWFSCN Punto in Capa.Puntos)
			{
				Columna.ReemplazarValorTexto(Punto.Nombre, Punto.Codigo);
			}

			List<Int32> Posiciones = Proveedor.Columnas[0].ReordenarValores();
			foreach (CLineaComprimida Linea in Proveedor.Datos)
			{
				Linea.Codigos[0] = Posiciones[Linea.Codigos[0]];
			}

		}

	}

}
