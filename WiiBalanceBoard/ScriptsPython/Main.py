import pandas as pd
import numpy as np
import ConsultasBD
import MuestreoEInterpolacion
import FiltroButterworth
import MediaMovil
import Metricas

#Datos del usuario que se quiere reprocesar
usuarioId = 9
numeroPruebas = 1
fmuestreo = 100
fcorte = 10
ventanaMediaMovil = 100 # la ventana es de 100 muestras que equivale a un segundo

#Nombres de las tablas de bases de datos
DB_LecturasBalanceBoard = 'LecturasBalanceBoard'
DB_LecturasInterpoladas = 'LecturasInterpoladas'
DB_LecturasConFiltro = 'LecturasConFiltro'
DB_LecturasConMediaMovil = 'LecturasConMediaMovil'
DB_Evaluaciones = 'Evaluaciones'

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

if __name__ == "__main__":
    #Procesamiento de datos
    dfDatosReales = ConsultasBD.ObtenerLecturasBalanceDeBD(usuarioId, numeroPruebas)
    dfDatosMuestreados = MuestreoEInterpolacion.Interpolar(dfDatosReales, fs = fmuestreo)
    dfDatosConFiltro = FiltroButterworth.Aplicar_filtro_butterworth(dfDatosMuestreados, fc = fcorte, fs = fmuestreo)
    dfDatosMediaMovil = MediaMovil.Aplicar_media_movil(dfDatosConFiltro, ventana = ventanaMediaMovil)

    #Se añade una nueva columna con un intervalo de tiempos en segundos
    dfDatosMuestreados = AnadirIntervaloDeTiempoEnSegundos(dfDatosMuestreados)
    dfDatosConFiltro = AnadirIntervaloDeTiempoEnSegundos(dfDatosConFiltro)
    dfDatosMediaMovil = AnadirIntervaloDeTiempoEnSegundos(dfDatosMediaMovil)

    #Calcular métricas y eliminar el primer y último segundo
    dfDatosMediaMovil = EliminarPrimerYUltimoSegundo(dfDatosMediaMovil, ventana = ventanaMediaMovil)
    metricas = Metricas.Calcular_metricas(dfDatosMediaMovil, usuarioId=usuarioId, numeroPruebas=numeroPruebas)

    # Guardar cada etapa en su tabla correspondiente
    ConsultasBD.Insertar_dataframe(dfDatosMuestreados, DB_LecturasInterpoladas, usuarioId, numeroPruebas)
    ConsultasBD.Insertar_dataframe(dfDatosConFiltro, DB_LecturasConFiltro, usuarioId, numeroPruebas)
    ConsultasBD.Insertar_dataframe(dfDatosMediaMovil, DB_LecturasConMediaMovil, usuarioId, numeroPruebas)
    ConsultasBD.Actualizar_tiempos_balanceboard(dfDatosReales, DB_LecturasBalanceBoard)

    # Guardar métricas finales
    ConsultasBD.Insertar_metricas(metricas)