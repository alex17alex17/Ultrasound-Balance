# Ultrasound Balance

Sistema experimental desarrollado para el estudio del efecto del toque ligero mediante estimulación háptica ultrasónica sobre el control postural.

El proyecto permite adquirir datos de una Wii Balance Board, calcular métricas de equilibrio basadas en el Centro de Presión (COP), almacenar la información en una base de datos SQL y procesar los resultados mediante scripts de análisis desarrollados en Python.

---

## Características principales

- Comunicación Bluetooth con Wii Balance Board.
- Adquisición de datos a 100 Hz.
- Cálculo en tiempo real del Centro de Presión (COP).
- Almacenamiento estructurado en base de datos SQL.
- Procesamiento de señales mediante interpolación y filtrado digital.
- Cálculo de métricas posturales:
  - RMS
  - Longitud de trayectoria COP
  - Área del rectángulo
  - Velocidad del COP
  - Episodios de desequilibrio
- Visualización y análisis mediante Power BI.
---

## Flujo de procesamiento

1. Adquisición de datos desde Wii Balance Board.
2. Cálculo de fuerzas registradas por los cuatro sensores.
3. Obtención de coordenadas COPX y COPY.
4. Almacenamiento en base de datos SQL.
5. Procesamiento mediante scripts Python:
   - Interpolación temporal.
   - Filtrado Butterworth.
   - Cálculo de métricas posturales.
6. Visualización y análisis mediante Power BI.

---

## Tecnologías utilizadas

### Aplicación principal

- C#
- .NET Framework
- WiimoteLib
- Bluetooth

### Procesamiento de datos

- Python
- NumPy
- Pandas
- SciPy
- Matplotlib

### Persistencia

- SQL Server

### Visualización

- Microsoft Power BI

---

## Hardware empleado

### Plataforma de equilibrio

Nintendo Wii Balance Board

- 4 sensores de fuerza
- Frecuencia de muestreo aproximada de 100 Hz

### Sistema háptico

Dispositivo ultrasónico basado en:

- Arduino Nano
- Driver L298N
- Transductores ultrasónicos de 40 kHz
- Configuración bowl semiesférica

---

## Proyecto académico

Este repositorio fue desarrollado como parte de un Trabajo Fin de Máster centrado en el estudio de la influencia de la estimulación háptica ultrasónica sobre el equilibrio postural humano.

El objetivo principal fue analizar cómo diferentes configuraciones de interacción (número de manos, orientación de las palmas y ubicación espacial) afectan al control postural utilizando tecnologías de háptica sin contacto.

---
