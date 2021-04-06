using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Clases
{

  public delegate void FncRefrescarCapas(CProyectoBing Proyecto);
  public class CProyectoBing
  {
    public event FncRefrescarCapas AlRefrescarCapas;

    private CCapaComodin mCapaEnProceso;

    public CProyectoBing()
    {
      Proyecto = CRutinas.CrearMapaBing();
      CapasCompletas = new List<CCapaComodin>();
    }

    public CMapaBingCN Proyecto { get; set; }

    public List<CCapaComodin> CapasCompletas { get; set; }

    private CElementoPreguntasWISCN PreguntaCodigoEnLista(string Codigo,
        List<CElementoPreguntasWISCN> Lista)
    {
      foreach (CElementoPreguntasWISCN Pregunta in Lista)
      {
        if (Pregunta.CodigoArea == Codigo)
        {
          return Pregunta;
        }
      }
      return null;
    }

    public CCapaComodin BuscarOCrearCapaChinches(CCapaComodin CapaChinches)
    {
      foreach (CCapaComodin Capa in CapasCompletas)
      {
        if (Capa.Clase == ClaseCapa.Bing)
        {
          return Capa;
        }
      }
      CCapaComodin Respuesta = new CCapaComodin(this);
      CapasCompletas.Add(Respuesta);
      return Respuesta;
    }

    public CElementoPreguntasWISCN PreguntaMarcadorWFS(CCapaComodin Capa, string Codigo)
    {
      return (Capa == null ? null : PreguntaCodigoEnLista(Codigo, Capa.Preguntas));
    }

    //public bool EsLayerBase(CCapaComodin Capa)
    //{
    //  if (Capa.Clase == ClaseCapa.WFS)
    //  {
    //    foreach (CCapaComodin CapaLocal in CapasCompletas)
    //    {
    //      if (CapaLocal.Clase == ClaseCapa.WIS && CapaLocal.CapaWIS.CodigoWFS == Capa.CodigoCapa)
    //      {
    //        return true;
    //      }
    //    }
    //  }
    //  return false;
    //}

    public void ExtraerDatosCapas()
    {
      Proyecto.Capas = new List<CCapaBingCN>();
      Int32 Orden = 0;
      foreach (CCapaComodin Capa in CapasCompletas)
      {
        if (Capa.Clase != ClaseCapa.Bing)
        {
          CCapaBingCN Datos = new CCapaBingCN();
          Datos.Codigo = Proyecto.Codigo;
          Datos.Clase = Capa.Clase;
          Datos.CodigoCapa = Capa.CodigoCapa;
          Datos.Opacidad = Capa.Opacidad;
          Datos.Orden = Orden++;
          if (Capa.Clase == ClaseCapa.WFS)
          {
            Datos.Rojo = Capa.ColorWFS.R;
            Datos.Verde = Capa.ColorWFS.G;
            Datos.Azul = Capa.ColorWFS.B;
          }
          else
          {
            Datos.Rojo = 255;
            Datos.Verde = 255;
            Datos.Azul = 255;
          }
          Proyecto.Capas.Add(Datos);
        }
      }
    }

    public CCapaComodin CapaWFSParaWIS(CCapaComodin CapaWIS)
    {
      if (CapaWIS.Clase == ClaseCapa.WIS)
      {
        foreach (CCapaComodin Capa in CapasCompletas)
        {
          if (Capa.Clase == ClaseCapa.WFS && Capa.CodigoCapa == CapaWIS.CapaWIS.CodigoWFS)
          {
            return Capa;
          }
        }
      }
      return null;
    }

    public CElementoPreguntasWISCN PreguntaAreaWFS(Int32 CodigoLayer, string Codigo)
    {
      foreach (CCapaComodin Capa in CapasCompletas)
      {
        if (Capa.Clase == ClaseCapa.WIS &&
            Capa.CapaWIS.Codigo == CodigoLayer)
        {
          return (Capa == null ? null : PreguntaCodigoEnLista(Codigo, Capa.Preguntas));
        }
      }
      return null;
    }

    private bool CapaWFSYaCargada(ClaseCapa Clase, Int32 Codigo)
    {
      return (from Capa in CapasCompletas
              where Capa.Clase == Clase && Capa.CodigoCapa == Codigo &&
                  (Capa.CapaWFS != null && (Capa.CapaWFS.Areas.Count > 0 ||
                      Capa.CapaWFS.Puntos.Count > 0 || Capa.CapaWFS.Lineas.Count > 0))
              select Capa).Any();
    }

    private bool CapaYaCargada(ClaseCapa Clase, Int32 Codigo)
    {
      return (from Capa in CapasCompletas
              where Capa.Clase == Clase && Capa.CodigoCapa == Codigo
              select Capa).Any();
    }

    private bool FaltaCargarDatosWFSParaWIS(Int32 CodigoWIS)
    {
      return (from Capa in CapasCompletas
              where Capa.Clase == ClaseCapa.WIS && Capa.CodigoCapa == CodigoWIS
                  && Capa.CapaWIS != null && Capa.CapaWFS == null
              select Capa).Any();
    }

    private async Task CargarColoresCapaAsync(Int32 CodigoCapa)
    {
      foreach (CCapaComodin Capa in CapasCompletas)
      {
        if (Capa.CodigoCapa == CodigoCapa &&
            ((Capa.Clase == ClaseCapa.WIS && Capa.Colores == null) ||
            Capa.Clase == ClaseCapa.Bing))
        {
          await Capa.AjustarColoresElementosCapaAsync();
          break;
        }
      }
    }

    public double LatCentro { get; set; } = 0;
    public double LngCentro { get; set; } = 0;
    public double NivelZoom { get; set; } = 1;

    public void UbicarCentro(double AnchoRefe, double AltoRefe)
    {
      if (CapasCompletas.Count > 0)
      {
        double LatMin = double.MaxValue;
        double LatMax = double.MinValue;
        double LngMin = double.MaxValue;
        double LngMax = double.MinValue;
        foreach (CCapaComodin Capa in CapasCompletas)
        {
          Capa.BuscarExtremos(ref LatMin, ref LngMin, ref LatMax, ref LngMax);
        }

        foreach (CElementoPreguntasWISCN Preg in Proyecto.Preguntas)
				{
          LatMin = Math.Min(LatMin, Preg.Ordenada);
          LatMax = Math.Max(LatMax, Preg.Ordenada);
          LngMin = Math.Min(LngMin, Preg.Abscisa);
          LngMax = Math.Max(LngMax, Preg.Abscisa);
        }

        LatCentro = (LatMin + LatMax) / 2;
        LngCentro = (LngMin + LngMax) / 2;

        double Relacion1 = (LatMax - LatMin) * 650 / AltoRefe;
        double Relacion2 = (LngMax - LngMin) * 1280 / AnchoRefe;
        double Salto = Math.Max(Relacion1, Relacion2);
        if (Salto == 0)
        {
          NivelZoom = 10;
        }
        else
        {
          Salto *= Math.Pow(2, 7);
          for (Int32 i = 15; i > 1; i--)
          {
            if (Salto < 1.5)
            {
              NivelZoom = i;
              return;
            }
            Salto /= 2;
          }
          NivelZoom = 1;
        }

      }
    }

    private async Task DibujarPuntoAsync(IJSRuntime JSRuntime, CElementoPreguntasWISCN Pregunta, Int32 Posicion)
    {
      object[] Args = new object[7];
      Args[0] = Posicion;
      Args[1] = Pregunta.Abscisa;
      Args[2] = Pregunta.Ordenada;
      Args[3] = CRutinas.ColorEquivalente(Pregunta.Color);
      Args[4] = Pregunta.Nombre;
      Args[5] = "";
      Args[6] = Proyecto.Codigo.ToString() + ";P;" + Pregunta.Codigo.ToString();
      await JSRuntime.InvokeAsync<Task>("AgregarPushpin", Args);
    }

    private bool PuedeDibujarWFS()
    {
      CCapaComodin CapaWIS = (from C in CapasCompletas
                              where C.Clase == ClaseCapa.WIS
                              select C).FirstOrDefault();
      CCapaComodin CapaWFS = (from C in CapasCompletas
                              where C.Clase == ClaseCapa.WFS
                              select C).FirstOrDefault();
      if (CapaWFS != null && CapaWFS.CapaWFS.Areas != null && CapaWFS.CapaWFS.Areas.Count > 0 && CapaWIS != null)
      {
        return (CapaWIS.CapaWFS.Areas == null || CapaWIS.CapaWFS.Areas.Count == 0);
      }
      else
      {
        return true;
      }
    }

    public async Task DibujarAsync(IJSRuntime JSRuntime, Int32 Posicion)
    {
      bool BloquearWFS = !PuedeDibujarWFS();

      foreach (CCapaComodin Capa in CapasCompletas)
      {
        Capa.ProyectoBing = this;
        await Capa.DibujarAsync(JSRuntime, Posicion, BloquearWFS);
      }

      foreach (CElementoPreguntasWISCN Pregunta in Proyecto.Preguntas)
			{
        await DibujarPuntoAsync(JSRuntime, Pregunta, Posicion);
			}
    }

    public async Task DibujarGradientesAsync(IJSRuntime JSRuntime, Int32 Posicion)
    {
      foreach (CCapaComodin Capa in CapasCompletas)
      {
        await Capa.DibujarAreasGradienteAsync(JSRuntime, Posicion);
      }
    }

    private CCapaBingCN mCapaEnLectura;

    private void AjustarColorOpacidad(CCapaComodin Capa)
    {
      if (mCapaEnLectura == null)
      {
        Capa.Opacidad = 128;
        Capa.ColorWFS = System.Drawing.Color.Gray;
      }
      else
      {
        Capa.Opacidad = mCapaEnLectura.Opacidad;
        Capa.ColorWFS = System.Drawing.Color.FromArgb(255,
            mCapaEnLectura.Rojo, mCapaEnLectura.Verde, mCapaEnLectura.Azul);
      }
    }

    private CCapaComodin UbicarCapaComodin(List<CCapaComodin> Capas, CCapaBingCN CapaBing)
    {
      foreach (CCapaComodin Capa in Capas)
      {
        if (Capa.Clase == CapaBing.Clase && Capa.CodigoCapa == CapaBing.CodigoCapa)
        {
          return Capa;
        }
      }
      return null;
    }

  }

}
