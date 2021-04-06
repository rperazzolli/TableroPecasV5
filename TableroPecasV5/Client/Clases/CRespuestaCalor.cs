using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableroPecasV5.Client.Clases
{
  public class CRespuestaCalor
  {
    public List<float> Valores;
    public float Minimo;
    public float Maximo;
    public float DistanciaEscala;
    public List<float> ValoresEscala;
    public Int32 SegmentosHCfg;
    public Int32 SegmentosVCfg;
    public Int32 SegmentosH;
    public Int32 SegmentosV;
    public double AbscisaMinima;
    public double AbscisaMaxima;
    public double OrdenadaMinima;
    public double OrdenadaMaxima;
    public double PixelsAncho;
    public double PixelsAlto;
    public List<double> Abscisas { get; set; }
    public List<double> Ordenadas { get; set; }

    public bool Aplanado;
    public bool Empuntado;
    public bool Acumulado;
    public double FactorDistancia;
    public double Discretizacion;
    public double FactorOpacidad;
    public byte Opacidad;
    public bool Monocromatico;
    public bool Cuantiles;

    public CRespuestaCalor()
    {
      Valores = new List<float>();
      Minimo = 0;
      Maximo = 0;
      SegmentosH = 0;
      SegmentosV = 0;
      SegmentosHCfg = -1;
      SegmentosVCfg = -1;
      Aplanado = false;
      Empuntado = false;
      Acumulado = false;
      PixelsAncho = 0;
      PixelsAlto = 0;
      FactorDistancia = 1;
      Discretizacion = 5;
      FactorOpacidad = 1;
      Opacidad = 255;
      Monocromatico = true;
      Cuantiles = false;
      ValoresEscala = new List<float>();
    }
  }
}
