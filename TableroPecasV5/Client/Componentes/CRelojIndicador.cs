using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components;
using BlazorSvgHelper;
using BlazorSvgHelper.Classes.SubClasses;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Componentes
{
  public class CRelojIndicador : CBaseGrafico
  {

    public CRelojIndicador()
    {
      Abscisa = 0;
      Ordenada = 0;
      Ancho = 110;
      Alto = 60;
      Indicador = null;
    }

    [Parameter]
    public CDatoIndicador Indicador { get; set; }

    public List<CInformacionAlarmaCN> Alarmas { get; set; }

    private void AgregarSector(svg Contenedor, string Color, double[] Abscisas, double[] Ordenadas, string SubOrden)
    {
      StringBuilder sb = new StringBuilder();

      sb.Append("M");
      sb.Append(Abscisas[0].ToString());
      sb.Append(" ");
      sb.Append(Ordenadas[0].ToString());
      sb.Append(" ");

      for (int i = 1; i < Abscisas.Length; i++)
      {
        sb.Append("L");
        sb.Append(Abscisas[i].ToString());
        sb.Append(" ");
        sb.Append(Ordenadas[i].ToString());
        sb.Append(" ");
      }

      sb.Append("Z");


      path p = new path()
      {
        fill = Color,
        stroke = Color,
        d = sb.ToString().Replace(",","."),
        stroke_width = 1
      };

      Contenedor.Children.Add(p);

    }

    private double AnguloDesdeAlarmas()
    {
      if (Alarmas==null || Alarmas.Count == 0)
      {
        return 0;
      }
      switch (Alarmas.Last().Color.ToUpper())
      {
        case Rutinas.CRutinas.COLOR_AZUL:
          return -67.5;
        case Rutinas.CRutinas.COLOR_VERDE:
          return -22.5;
        case Rutinas.CRutinas.COLOR_AMARILLO:
          return 22.5;
        case Rutinas.CRutinas.COLOR_ROJO:
          return 67.5;
        default:
          return 0;
      }
    }

    private const double ANCHO_FLECHA = 3;

    private void AgregarFlecha(svg Contenedor)
    {

      double Angulo = AnguloDesdeAlarmas();

      StringBuilder sb = new StringBuilder();

      sb.Append("M0 ");
      sb.Append((-55 + ANCHO_FLECHA / 2).ToString());
      sb.Append(" ");

      sb.Append("L");
      sb.Append((ANCHO_FLECHA / 2).ToString());
      sb.Append(" ");
      sb.Append((-55 + ANCHO_FLECHA).ToString());
      sb.Append(" ");

      sb.Append("L");
      sb.Append((ANCHO_FLECHA / 2).ToString());
      sb.Append(" ");
      sb.Append((ANCHO_FLECHA / 2).ToString());
      sb.Append(" ");

      sb.Append("L");
      sb.Append((-ANCHO_FLECHA / 2).ToString());
      sb.Append(" ");
      sb.Append((ANCHO_FLECHA / 2).ToString());
      sb.Append(" ");

      sb.Append("L");
      sb.Append((-ANCHO_FLECHA / 2).ToString());
      sb.Append(" ");
      sb.Append((-55 + ANCHO_FLECHA).ToString());
      sb.Append(" ");

      sb.Append("Z");

      path p = new path()
      {
        fill = "#000000",
        stroke = "#000000",
        d = sb.ToString().Replace(",", "."),
        stroke_width = 1,
        transform = "translate(55,55) rotate(" + Angulo.ToString().Replace(",", ".") + ")"
      };

      Contenedor.Children.Add(p);

    }

    public async Task<svg> ComposeSVG()
    {

      if (Runtime != null)
      {
        await Contenedores.CContenedorDatos.InicializarDimensionesAsync(Runtime);
      }

      bool HayAlarmas = (Alarmas != null && Alarmas.Count > 0 && Alarmas.Last().Sobresaliente != Alarmas.Last().Minimo);

      svg _svg = new svg
      {
        id = Nombre_SVG,
        width = Ancho,
        height = Alto,
        xmlns = "http://www.w3.org/2000/svg",
      };

      // Agrega los 8 sectores.
      AgregarSector(_svg, (HayAlarmas ? "#e3eaf6" : "#ebf0f9"),
        new double[] { 0.124619071668629, 3.4562471794718, 7.41621565904, 4.12382788786642, 0.170570534942717 },
        new double[] { 51.2996529932281, 35.8103792331299, 37.4087026481646, 51.5905296432393, 50.6717596489673 },
        "1");

      AgregarSector(_svg, (HayAlarmas ? "#D9E2F3" : "#ebf0f9"),
        new double[] { 4.87434559384446, 13.5327204052723, 17.1682443625825, 8.80714959134239, 4.87434559384446 },
        new double[] { 32.3633312884899, 18.8687846452312, 22.3746676570173, 34.2933978066743, 32.3633312884899 },
        "2");

      AgregarSector(_svg, (HayAlarmas ? "#D4E8C6" : "#ebf0f9"),
        new double[] { 16.1199820880667, 29.2912339049685, 31.8384251249064, 19.6240830508008, 19.6240830508008 },
        new double[] { 16.0982750103836, 6.37840658852327, 12.0834059097628, 20.0064029342944, 20.0064029342944 },
        "3");

      AgregarSector(_svg, (HayAlarmas ? "#BEDCAA" : "#ebf0f9"),
        new double[] { 32.7863826274726, 49.014330424218, 49.2941408719179, 35.0021611399866, 32.7863826274726 },
        new double[] { 4.68543746362345, 0.326681464085631, 8.03096717373095, 10.8064580305103, 4.68543746362345 },
        "4");

      AgregarSector(_svg, (HayAlarmas ? "#FFFF82" : "#ebf0f9"),
        new double[] { 52.9962081194302, 70.1404832781533, 66.9978388600133, 52.7058591280821, 52.9962081194302 },
        new double[] { 0.0365137741485438, 2.12499866568361, 10.8064580305103, 8.03096717373095, 0.0365137741485438 },
        "5");

      AgregarSector(_svg, (HayAlarmas ? "#FFFF8C" : "#ebf0f9"),
        new double[] { 74.0384752908443, 89.4452444013966, 82.3759169491991, 70.1615748750936, 74.0384752908443 },
        new double[] { 3.40022811484619, 12.1219737146396, 20.0064029342944, 12.0834059097628, 3.40022811484619 },
        "6");

      AgregarSector(_svg, (HayAlarmas ? "#FF7272" : "#ebf0f9"),
        new double[] { 92.61245909499, 103.572867174313, 93.1928504086576, 84.8317556374174, 92.61245909499 },
        new double[] { 14.8714201493785, 29.1992912022442, 34.2933978066742, 22.3746676570173, 14.8714201493785 },
        "7");

      AgregarSector(_svg, (HayAlarmas ? "#FF7D7D" : "#ebf0f9"),
        new double[] { 105.423051172649, 109.83328228419, 97.8761721121336, 94.58378434096, 105.423051172649 },
        new double[] { 33.0337552039399, 50.7208465857867, 51.5905296432392, 37.4087026481646, 33.0337552039399 },
        "8");

      if (Alarmas != null)
      {
        AgregarFlecha(_svg);
      }

      //_svg.Children.Add(new circle
      //{
      //  cx = 0,
      //  cy = 0,
      //  r = 12,
      //  fill = "pink",
      //  transform = "translate(21,21)"
      //});

      //foreach (Clases.CDatoCirculo Circulo in Circulos)
      //{
      //  _svg.Children.Add(new circle
      //  {
      //    cx = 0,
      //    cy = 0,
      //    r = Circulo.Tamanio,
      //    fill = "red",
      //    transform = "translate(" + Circulo.Abscisa.ToString() + "," + Circulo.Ordenada.ToString() + ")",
      //    onclick= "FncClickElemento(12);"
      //  });
      //}

      return _svg;
    }

    protected override async void BuildRenderTree(RenderTreeBuilder builder)
    {
      if (Alarmas != null)
      {
        svg Dibujador = await ComposeSVG();
        new SvgHelper().Cmd_Render(Dibujador, 0, builder);
      }
    }
  }
}
