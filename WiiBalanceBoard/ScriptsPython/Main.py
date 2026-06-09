import argparse
import pandas as pd
import numpy as np
import ConsultasBD
import MuestreoEInterpolacion
import FiltroButterworth
import MediaMovil
import Metricas

#Datos del usuario que se quiere reprocesar. Por defecto. En la ejecución del programa se pasan estos valores como argumentos.
usuarioId = 27
numeroPruebas = 1

#Parámetros para el reprocesamiento de datos
fmuestreo = 100
fcorte = 10
ventanaMediaMovil = 100 # la ventana es de 100 muestras que equivale a un segundo
k = 3 # número de desviaciones estándar para el cálculo del desequilibrio

#Nombres de las tablas de bases de datos
DB_LecturasBalanceBoard = 'LecturasBalanceBoard'
DB_LecturasInterpoladas = 'LecturasInterpoladas'
DB_LecturasConFiltro = 'LecturasConFiltro'
DB_LecturasConMediaMovil = 'LecturasConMediaMovil'
DB_Evaluaciones = 'Evaluaciones'

def CalcularPerdidaDeEquilibrioConDistanciaRadial(df):
    #Se define el centro de todas las coordenadas y se calcula la distancia de cada punto a este centro
    xc = df['COP_X'].mean()
    yc = df['COP_Y'].mean()
    df['DistanciaRadial'] = np.sqrt((df['COP_X'] - xc)**2 + (df['COP_Y'] - yc)**2) #DISTANCIA EUCLIDEA
    df['DistanciaRadial_X'] = df['COP_X'] - xc
    df['DistanciaRadial_Y'] = df['COP_Y'] - yc
    
    #Ahora se calcula la variabilidad de los datos, para saber a partir de qué valor consideramos que el
    #usuario se encuentra en desequilibrio. Es decir, calculamos el umbral de equilibrio.
    #Es un umbral estadístico que asume que el comportamiento normal estña centrado alrededor de mu
    #Se coge k = 2, por no ser demasiado estrictos.
    mu = df['DistanciaRadial'].mean()
    sigma = df['DistanciaRadial'].std()
    R = mu + 2 * sigma
    df['Desequilibrio'] = df['DistanciaRadial'] > R

    abs_dx = df['DistanciaRadial_X'].abs()
    Rx = abs_dx.mean() + 2 * abs_dx.std()
    df['Desequilibrio_X'] = df['DistanciaRadial_X'] > Rx

    abs_dy = df['DistanciaRadial_Y'].abs()
    Ry = abs_dy.mean() + 2 * abs_dy.std()
    df['Desequilibrio_Y'] = df['DistanciaRadial_Y'] > Ry
    return df

def CalcularVelocidadYDesplazamientoYDesequilibriosEnVelocidad(df):
    #Calcula la diferencia entre puntos. El primer elemento es NaN, por ello se coloca un cero
    dx = df['COP_X'].diff().fillna(0)
    dy = df['COP_Y'].diff().fillna(0)

    #Calcula ls distancia Euclidea entre puntos en lugar de la media
    df['Desplazamiento'] = np.sqrt(dx**2 + dy**2)
    df['Desplazamiento_X'] = dx
    df['Desplazamiento_Y'] = dy

    #Como sabemos que la frecuencia de muestreo es 100Hz, podemos calcular la velocidad.
    dt = 0.01
    df['Velocidad'] = df['Desplazamiento'] / dt
    df['Velocidad_X'] = dx / dt
    df['Velocidad_Y'] = dy / dt

    mu = df['Velocidad'].mean()
    sigma = df['Velocidad'].std()
    R = mu + k * sigma
    df['DesequilibrioVel'] = df['Velocidad'] > R

    abs_dx = df['Velocidad_X'].abs()
    Rx = abs_dx.mean() + k * abs_dx.std()
    df['DesequilibrioVel_X'] = df['Velocidad_X'] > Rx

    abs_dy = df['Velocidad_Y'].abs()
    Ry = abs_dy.mean() + k * abs_dy.std()
    df['DesequilibrioVel_Y'] = df['Velocidad_Y'] > Ry

    return df

def EliminarPrimerYUltimoSegundo(df, ventana):
    # Eliminar la primeras 'ventana' filas (por NaN). Estas tienen Nan por aplicar la media móvil
    # si la media móvil es de 100 muestras, que equivalen a 1 segundos, entonces las primeras 100 muestras
    # tienen valor Nan
    df = df.dropna().reset_index(drop=True)

    #df = df.iloc[:ventana].reset_index(drop=True) #Puede que con esto se soluciones
    df = df.iloc[:-ventana].reset_index(drop=True)
    print(f"✅ Media móvil aplicada. Se eliminaron el primer y último segundo ({ventana} muestras cada uno).")
    print(f"Total de muestras resultantes: {len(df)}")
    return df

def AnadirIntervaloDeTiempoEnSegundos(df):
    df["TimeStampSegundos"] = (
        (df["TimeStamp"] - df["TimeStamp"].iloc[0]).dt.total_seconds()
    )
    return df

def main(usuario_id: int, configuracion_id: int):
    usuarioId = usuario_id
    #Procesamiento de datos
    dfDatosReales = ConsultasBD.ObtenerLecturasBalanceDeBD(usuarioId)
    dfDatosMuestreados = MuestreoEInterpolacion.Interpolar(dfDatosReales, fs = fmuestreo)
    dfDatosConFiltro = FiltroButterworth.Aplicar_filtro_butterworth(dfDatosMuestreados, fc = fcorte, fs = fmuestreo)
    #dfDatosMediaMovil = MediaMovil.Aplicar_media_movil(dfDatosConFiltro, ventana = ventanaMediaMovil) #Comentado para ahorrar registros en base de datos, ya que no se emplea

    #Se añade una nueva columna con un intervalo de tiempos en segundos
    #dfDatosMuestreados = AnadirIntervaloDeTiempoEnSegundos(dfDatosMuestreados)
    dfDatosConFiltro = AnadirIntervaloDeTiempoEnSegundos(dfDatosConFiltro)
    #dfDatosMediaMovil = AnadirIntervaloDeTiempoEnSegundos(dfDatosMediaMovil)

    #Calcular métricas y eliminar el primer y último segundo
        # Se comenta la siguiente linea para realizar el cálculo empleando 
    #dfDatosMediaMovil = EliminarPrimerYUltimoSegundo(dfDatosMediaMovil, ventana = ventanaMediaMovil). 
    dfDatosConFiltro = EliminarPrimerYUltimoSegundo(dfDatosConFiltro, ventana = ventanaMediaMovil)
    

    #Calcular pérdidas de equilibrio
    dfDatosConFiltro = CalcularPerdidaDeEquilibrioConDistanciaRadial(dfDatosConFiltro)
    dfDatosConFiltro = CalcularVelocidadYDesplazamientoYDesequilibriosEnVelocidad(dfDatosConFiltro)
    #dfDatosMediaMovil = CalcularPerdidaDeEquilibrio(dfDatosMediaMovil)
    metricas = Metricas.Calcular_metricas(dfDatosConFiltro, usuarioId=usuarioId, numeroPruebas=numeroPruebas)

    # Guardar cada etapa en su tabla correspondiente
    #ConsultasBD.Insertar_dataframe(dfDatosMuestreados, DB_LecturasInterpoladas, usuarioId, numeroPruebas)
    ConsultasBD.Insertar_dataframe(dfDatosConFiltro, DB_LecturasConFiltro, usuarioId, numeroPruebas)
    #ConsultasBD.Insertar_dataframe(dfDatosMediaMovil, DB_LecturasConMediaMovil, usuarioId, numeroPruebas)
    ConsultasBD.Actualizar_tiempos_balanceboard(dfDatosReales, DB_LecturasBalanceBoard)

    # Guardar métricas finales
    ConsultasBD.Insertar_metricas(metricas, configuracion_id)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Reprocesar datos de equilibrio para un usuario.")
    parser.add_argument("usuario_id", type=int, help="ID del usuario (número entero).")
    parser.add_argument("configuracion_id", type=int, help="ID de configuración (número entero).")  # nuevo argumento
    args = parser.parse_args()

    main(args.usuario_id, args.configuracion_id)