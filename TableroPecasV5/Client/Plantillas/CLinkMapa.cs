using System;
using TableroPecasV5.Client.Logicas;

namespace TableroPecasV5.Client.Plantillas
{
  public class CLinkMapa : CLinkBase
  {
    public CDatosMapaLista Datos { get; set; }

    private CLinkContenedorFiltros mFiltros = null;
    public CLinkContenedorFiltros Filtros
    {
      get { return mFiltros; }
      set
      {
        if (mFiltros != value)
        {
          mFiltros = value;
          if (!mFiltros.ComponentesAsociados.Contains(this))
          {
            mFiltros.ComponentesAsociados.Add(this);
          }
        }
      }
    }


    public bool Minimizado { get; set; } = true;

    private CLogicaBingMaps mComponenteMapa = null;

    public string Direccion
    {
      get { return Datos.ClaseElemento.ToString() + Datos.Codigo.ToString(); }
    }

    public CLogicaBingMaps ComponenteMapa
    {
      get { return mComponenteMapa; }
      set
      {
        if (value != mComponenteMapa)
        {
          mComponenteMapa = value;
        }
      }
    }

    private CLogicaMapaGradiente mComponenteGradiente = null;
    public CLogicaMapaGradiente ComponenteGradiente
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

    private Logicas.CLogicaMapaCalor mComponenteCalor = null;
    public Logicas.CLogicaMapaCalor ComponenteCalor
    {
      get { return mComponenteCalor; }
      set
      {
        if (value != mComponenteCalor)
        {
          mComponenteCalor = value;
        }
      }
    }

    public void ImponerProveedor(Datos.CProveedorComprimido Proveedor, bool Refrescar = true)
    {
      if (ComponenteCalor != null)
      {
        ComponenteCalor.Proveedor = Proveedor;
        ComponenteCalor.DatosSinFiltroPropio = Proveedor.Datos;
        ComponenteCalor.DatosCompletos = false;
        if (Refrescar)
        {
          ComponenteCalor.Refrescar();
        }
      }
    }

    private double mAbscisaPropia;
    private double mOrdenadaPropia;
    private double mAnchoPropio;
    private double mAltoPropio;

    public void Maximizar(Rectangulo RectContenedor)
    {
      if (Minimizado)
      {
        NivelFlotantePropio = 900;
        mAbscisaPropia = Datos.Posicion.X;
        mOrdenadaPropia = Datos.Posicion.Y;
        mAnchoPropio = Datos.Ancho;
        mAltoPropio = Datos.Alto;
        Datos.Posicion.X = 2;
        Datos.Posicion.Y = 2;
        Datos.Ancho = RectContenedor.width - 4; // Contenedores.CContenedorDatos.AnchoPantallaAmpliada - 4;
        Datos.Alto = RectContenedor.height - 29; // Contenedores.CContenedorDatos.AltoPantalla - 100;
        Minimizado = false;
        InformarComponente();
      }
    }

    public Int32 NivelFlotantePropio { get; set; } = 1;

    private void InformarComponente()
    {
      if (ComponenteMapa != null)
      {
        //ComponenteMapa.ImponerAncho(Ancho);
        //ComponenteMapa.ImponerAlto(Alto);
        ComponenteMapa.ReubicarCentro = true;
      }
      else
      {
        if (ComponenteGradiente != null)
        {
          ComponenteGradiente.ReubicarCentro = true;
        }
      }
    }

    public void Reducir()
    {
      if (!Minimizado)
      {
        NivelFlotantePropio = 1;
        Datos.Posicion.X = mAbscisaPropia;
        Datos.Posicion.Y = mOrdenadaPropia;
        Datos.Ancho = mAnchoPropio;
        Datos.Alto = mAltoPropio;
        Minimizado = true;
        InformarComponente();
      }
    }

    public CLinkMapa()
    {
    }
  }

}
