import pandas as pd

def Aplicar_media_movil(df, ventana=100):
    """
    Aplica una media móvil de 1 segundo (100 muestras a 100 Hz)
    a las columnas relevantes del DataFrame.
    """
    df_media = df.copy()

    columnas = ["Total", "COP_X", "COP_Y"]

    for col in columnas:
        # Aplicamos la media móvil centrada
        df_media[col] = df[col].rolling(window=ventana, center=False).mean()

    print(f"✅ Media móvil aplicada (ventana={ventana} muestras ≈ 1s). Total: {len(df_media)} muestras.")
    return df_media