import numpy as np
import pandas as pd
from datetime import datetime

def Calcular_metricas(df, usuarioId, numeroPruebas):
    """
    Calcula métricas de equilibrio a partir de los datos del COP suavizados.
    Devuelve un diccionario con los valores listos para insertar en la tabla Evaluaciones.
    """
    mean_copx = df["COP_X"].mean()
    mean_copy = df["COP_Y"].mean()

    # Calcula la diferencia de cada elemento del COP con su siguiente.
    dx = np.diff(df["COP_X"])
    dy = np.diff(df["COP_Y"])
    longitud_trayectoria = np.sum(np.sqrt(dx**2 + dy**2))

    area_rectangulo = (df["COP_X"].max() - df["COP_X"].min()) * (df["COP_Y"].max() - df["COP_Y"].min())

    rms = np.sqrt(np.mean((df["COP_X"] - mean_copx)**2 + (df["COP_Y"] - mean_copy)**2))

    metricas = {
        "UsuarioId": usuarioId,
        "NumeroPruebas": numeroPruebas,
        "Mean_COPX": mean_copx,
        "Mean_COPY": mean_copy,
        "LongitudTrayectoria": longitud_trayectoria,
        "AreaRectangulo": area_rectangulo,
        "RMS": rms,
        "TimeStamp": datetime.now()
    }

    print("✅ Métricas calculadas:")
    for k, v in metricas.items():
        print(f"   {k}: {v}")

    return metricas