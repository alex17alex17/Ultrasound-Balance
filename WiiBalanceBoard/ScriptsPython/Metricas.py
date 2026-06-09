import numpy as np
import pandas as pd
from datetime import datetime


def CalcularEventosDeDesequilibrio(df, min_muestras=5):

    def analizar_columna(columna):
        col = columna.astype(int)
        grupos = col.groupby((col != col.shift()).cumsum())
        
        duraciones = [len(grupo) for _, grupo in grupos if grupo.iloc[0] == 1 and len(grupo) >= min_muestras]
        count = len(duraciones)

        if count == 0:
            return count, 0.0, 0.0, 0.0, 0.0
        
        return (
            count,
            float(min(duraciones))*10,   # DuracionMinima
            float(sum(duraciones) / count)*10,  # DuracionMedia
            float(max(duraciones))*10,   # DuracionMaxima
            float(sum(duraciones))*10    # DuracionTotal
        )

    vel,   vel_min,   vel_media,   vel_max,   vel_total   = analizar_columna(df['DesequilibrioVel'])
    vel_x, vel_x_min, vel_x_media, vel_x_max, vel_x_total = analizar_columna(df['DesequilibrioVel_X'])
    vel_y, vel_y_min, vel_y_media, vel_y_max, vel_y_total = analizar_columna(df['DesequilibrioVel_Y'])

    return {
        "PerdidasDesequilibrioVel":   vel,
        "PerdidasDesequilibrioVel_X": vel_x,
        "PerdidasDesequilibrioVel_Y": vel_y,

        "DuracionMinimaDesequilibrioVel":   vel_min,
        "DuracionMediaDesequilibrioVel":    vel_media,
        "DuracionMaximaDesequilibrioVel":   vel_max,
        "DuracionTotalDesequilibrioVel":    vel_total,

        "DuracionMinimaDesequilibrioVel_X":  vel_x_min,
        "DuracionMediaDesequilibrioVel_X":   vel_x_media,
        "DuracionMaximaDesequilibrioVel_X":  vel_x_max,
        "DuracionTotalDesequilibrioVel_X":   vel_x_total,

        "DuracionMinimaDesequilibrioVel_Y":  vel_y_min,
        "DuracionMediaDesequilibrioVel_Y":   vel_y_media,
        "DuracionMaximaDesequilibrioVel_Y":  vel_y_max,
        "DuracionTotalDesequilibrioVel_Y":   vel_y_total,
    }




def Calcular_metricas(df, usuarioId, numeroPruebas):
    """
    Calcula métricas de equilibrio a partir de los datos del COP suavizados.
    Devuelve un diccionario con los valores listos para insertar en la tabla Evaluaciones.
    """

    # La media cuandoe existen número negativos se calcula de la misma manera de siempre.
    # Se suman todos y se divide entre el número total de elementos.
    mean_copx = df["COP_X"].mean()
    mean_copy = df["COP_Y"].mean()

    # Calcula la diferencia de cada elemento del COP con su siguiente.
    # Si son número negativos también los tiene en cuenta. Es decir, de -2 a 1 se obtiene 3 y de -1 a -3 se obtiene -2, ya que a
    # -1 se le debe restar -2 para obtener -3. Además, el signo no importa ua que luego se hace el cuadrado
    # al menos en la longitud de la trayectoria.
    dx = np.diff(df["COP_X"])
    dy = np.diff(df["COP_Y"])
    longitud_trayectoria = np.sum(np.sqrt(dx**2 + dy**2))

    # Si se coge el menor de todos y es negativo, entonces se hace positivo y la diferencia es mayor. 4 - (-3) entonces la diferencia es 7
    # En el caso de que sean los dos positivos (7 - 3), la diferencia es 4.
    # El problema está en que si los dos números son negativos (-3 - (-7)) esto es 4, lo cual sería correcto también.
    area_rectangulo = (df["COP_X"].max() - df["COP_X"].min()) * (df["COP_Y"].max() - df["COP_Y"].min())

    # Al calcular la diferencia de -3 con la media 10, entonces la distancia del COP_X con su media es -13
    # Como despues se realiza el cuadrado, el negativo siempre se va
    # Si se tiene 3 - 10, entonces da 7 la diferencia.
    # por otro lado, 
    rms = np.sqrt(np.mean((df["COP_X"] - mean_copx)**2 + (df["COP_Y"] - mean_copy)**2))

    dic = CalcularEventosDeDesequilibrio(df)

    metricas = {
        "UsuarioId": usuarioId,
        "NumeroPruebas": numeroPruebas,
        "Mean_COPX": mean_copx,
        "Mean_COPY": mean_copy,
        "LongitudTrayectoria": longitud_trayectoria,
        "AreaRectangulo": area_rectangulo,
        "RMS": rms,
        "TimeStamp": datetime.now(),
        "PerdidasDesequilibrioVel": dic['PerdidasDesequilibrioVel'],
        "PerdidasDesequilibrioVel_X": dic['PerdidasDesequilibrioVel_X'],
        "PerdidasDesequilibrioVel_Y": dic['PerdidasDesequilibrioVel_Y'],
        "DuracionMinimaEnDesequilibrio": dic['DuracionMinimaDesequilibrioVel'],
        "DuracionMediaDesequilibrio": dic['DuracionMediaDesequilibrioVel'],
        "DuracionMaximaDesequilibrio": dic['DuracionMaximaDesequilibrioVel'],
        "DuracionTotalDesequilibrio": dic['DuracionTotalDesequilibrioVel'],
        "DuracionMinimaEnDesequilibrio_X": dic['DuracionMinimaDesequilibrioVel_X'],
        "DuracionMediaDesequilibrio_X": dic['DuracionMediaDesequilibrioVel_X'],
        "DuracionMaximaDesequilibrio_X": dic['DuracionMaximaDesequilibrioVel_X'],
        "DuracionTotalDesequilibrio_X": dic['DuracionTotalDesequilibrioVel_X'],
        "DuracionMinimaEnDesequilibrio_Y": dic['DuracionMinimaDesequilibrioVel_Y'],
        "DuracionMediaDesequilibrio_Y": dic['DuracionMediaDesequilibrioVel_Y'],
        "DuracionMaximaDesequilibrio_Y": dic['DuracionMaximaDesequilibrioVel_Y'],
        "DuracionTotalDesequilibrio_Y": dic['DuracionTotalDesequilibrioVel_Y'],
    }

    print("✅ Métricas calculadas:")
    for k, v in metricas.items():
        print(f"   {k}: {v}")

    return metricas