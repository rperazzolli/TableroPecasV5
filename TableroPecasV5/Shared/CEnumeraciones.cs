using System;
using System.Collections.Generic;
using System.Text;

namespace TableroPecasV5.Shared
{
  public enum ClaseDetalle
  {
    Pregunta = 1,
    Indicador = 2,
    Bing = 3,
    Mimico = 4,
    SalaReunion = 5,
    Metas = 6,
    NoDefinido = -1
  }

  public enum ClaseVinculo
  {
    Coordenadas = 1,
    Areas = 2,
    Dimension = 3,
    Marcador = 4,
    Lineas = 5,
    ColumnasGIS = 6,
    NoDefinido = -1
  }

  public enum ClaseFlecha 
  {
    Flecha = 0,
    Mensaje = 1,
    NoDefinida = 2
  }

  public enum ClaseRombo
  {
    EntradaAnd = 0,
    EntradaOr = 1,
    EntradaXOr = 2,
    EntradaCond = 3,
    SalidaAnd = 4,
    SalidaOr = 5,
    SalidaXOr = 6,
    SalidaCond = 7,
    NoDefinida = 8
  }

  public enum ElementoWFS
  {
    Linea = 1,
    Punto = 2,
    Superficie = 3,
    NoDefinido = -1
  }

  public enum ClaseIntervalo
  {
    NoDefinido = -1,
    Indicador = 1,
    Lineal = 2,
    Cuantiles = 3,
    Manual = 4
  }

  public enum ClaseCapa : int
  {
    WMS = 1,
    WFS = 2,
    Drones = 3,
    WIS = 4,
    Bing = 5,
    NoDefinida = -1
  }

  public enum ModoGeoreferenciar
  {
    NoDefinido = -1,
    Vinculo = 1,
    Coordenadas = 2
  }

  public enum ClaseGrafico
  {
    Torta = 0,
    Barras = 1,
    Pareto = 2,
    Histograma = 3,
    Puntos = 4,
    Grilla = 5,
    SobreGIS = 6,
    Mimico = 7,
    NoDefinido = 8,
    Piramide = 9,
    BarrasH = 10
  }

  public enum ModoAgruparIndependiente
  {
    Todos = 0,
    Rangos = 1,
    Dia = 2,
    Semana = 3,
    Quincena = 4,
    Mes = 5,
    Anio = 6,
    NoDefinido = 7
  }

  public enum ModoAgruparDependiente
  {
    Todos = 0,
    Acumulado = 1,
    Cantidad = 2,
    Media = 3,
    Minimo = 4,
    Maximo = 5,
    NoDefinido = 6
  }

  public enum ModoFiltrar
  {
    NoDefinido = 0,
    PorValor = 1,
    PorRango = 2,
    Igual = 3,
    Mayor = 4,
    MayorIgual = 5,
    Menor = 6,
    MenorIgual = 7
  }

  public enum ClaseVariable
  {
    Entero = 0,
    Real = 1,
    Booleano = 2,
    Fecha = 3,
    Texto = 4,
    NoDefinida = -1
  }

  public enum ClaseComentario
  {
    Texto = 1,
    Link = 2,
    Archivo = 3,
    Varios = 4,
    NoDefinido = -1
  }

  public enum ClaseElemento
  {
    Tarea = 1,
    Flecha = 2,
    Variable = 3,
    Proceso = 4,
    Rombo = 5,
    EventoInicio = 6,
    EventoCierre = 7,
    EventoIntermedio = 8,
    EventoPropio = 9,
    Linea = 10,
    Consulta = 11,
    Parametro = 12,
    AreaGIS = 14,
    AreaCaliente = 15,
    Chinche = 16,
    Pregunta = 18,
    Indicador = 19,
    SalaReunion = 20,
    Solapa = 21,
    Mimico = 22,
    Bing = 23,
    FA = 24,
    CapaGis = 25,
    CKam = 26,
    SubConsulta = 27,
    NoDefinida = -1
  }

}



