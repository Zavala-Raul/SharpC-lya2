# Contexto: Representaciones de Código Intermedio y Notaciones

## 1. Introducción al Código Intermedio
* **Definición:** Etapa de **síntesis** del compilador donde el código fuente (previamente analizado y libre de errores sintácticos y semánticos) se transforma en operaciones que se pueden analizar y optimizar antes de su traducción final.
* **Función principal:** Actúa como un puente estandarizado entre los lenguajes de alto nivel (lenguaje fuente) y la arquitectura de hardware específica (lenguaje ensamblador o código máquina).
* **Propiedades:**
  * *Fácil de producir:* Generación directa y eficiente a partir del árbol de análisis sintáctico y semántico.
  * *Fácil de traducir:* Conversión sencilla hacia lenguajes de bajo nivel o máquina.
* **Ventajas:**
  * Facilita la fase de optimización (permite optimizadores independientes de la máquina).
  * Aumenta la portabilidad del compilador entre distintas arquitecturas.
  * Permite reutilizar el mismo analizador (front-end) para diferentes generadores de código (back-end).
  * Reduce la complejidad y automatiza la generación de código final.

---

## 2. Notaciones de Expresiones
Son representaciones formales de datos y expresiones sujetas a reglas sintácticas sobre el orden de operadores y operandos.

### 2.1 Tipos de Notación
1. **Notación Infija:**
   * **Estructura:** El operador se ubica *entre* los operandos (ej. $X + 10$ o $X = 10 + 3 * A$).
   * **Características:** Es la forma natural humana. Requiere reglas estrictas de precedencia de operadores y el uso obligatorio de paréntesis para evitar ambigüedades, lo que dificulta su lectura directa por una máquina.
2. **Notación Polaca (Prefija):**
   * **Estructura:** El operador se coloca *antes* de sus operandos (ej. $+ X 10$).
   * **Características:** Representación lineal que no requiere paréntesis. Facilita la traducción a ensamblador.
3. **Notación Polaca Inversa (Postfija):**
   * **Estructura:** El operador se sitúa *después* de sus operandos (ej. $X 10 +$ o $X 10 3 A * + =$).
   * **Características:** Representación lineal sin paréntesis. Es una de las más utilizadas en compiladores y máquinas virtuales porque se evalúa de forma natural mediante una estructura de pila (LIFO).

### 2.2 Comparativa: Prefija vs. Postfija
| Criterio | Notación Polaca (Prefija) | Notación Polaca Inversa (Postfija) |
| :--- | :--- | :--- |
| **Posición del operador** | Antes de los operandos | Después de los operandos |
| **Evaluación con pila** | Se procesa de derecha a izquierda | Se procesa de izquierda a derecha (lectura natural) |
| **Recorrido del árbol sintáctico** | Recorrido en **preorden** | Recorrido en **postorden** |
| **Sincronización de operandos** | Primero se recibe la operación y luego los datos a procesar | Los operandos se cargan primero en la pila y la operación se ejecuta al encontrar el operador |

### 2.3 Ejemplo de Conversión: $(A + B) * (C - D)$
* **Infija:** $(A + B) * (C - D)$ *(Requiere paréntesis para que $B * C$ no se evalúe primero)*.
* **Prefija:** $* + A B - C D$
* **Postfija:** $A B + C D - *$

---

## 3. Representaciones de Código Intermedio
*(Nota: Basado en el enfoque práctico de Cantú Treviño y Mendoza García, 2015).*

### 3.1 Código P (P-Code)
* **Definición:** Lenguaje intermedio diseñado para ejecutarse en una **máquina virtual basada en pila**, en lugar de una CPU física con registros reales.
* **Relación con el intérprete:** Se asocia con lenguajes interpretados. El intérprete analiza, convierte y ejecuta el código línea por línea (interrumpiendo la ejecución si detecta un error).
* **Beneficios:** Portabilidad, sencillez de implementación, ahorro de espacio y facilidad de depuración.
* **Instrucciones base sobre la pila (cima arriba):**
  * `load`: Carga/apila valores o variables en la cima de la pila.
  * `add` / `sub`: Toma los valores superiores de la pila, los suma/resta y deja el resultado en la pila.
  * `store`: Desapila el resultado final y lo guarda en la variable destino.

### 3.2 Triplos
* **Definición:** Representación tabular del código de tres direcciones que organiza cada paso como un registro utilizando **3 campos**.
* **Estructura (según el material de estudio):**
  * `DATO OBJETO`: Destino del resultado (actúa también como operando acumulador mediante variables temporales como $t1$).
  * `DATO FUENTE`: Valor o variable que se asigna o combina con el dato objeto.
  * `OPERADOR`: Operación a realizar (o vacío en asignación inicial al temporal).
  *(Nota teórica general: En otros textos estándar también se suele representar como `Operador | Argumento 1 | Argumento 2`)*.

### 3.3 Cuádruplos
* **Definición:** Es la representación tabular más extendida del código de tres direcciones. Utiliza **4 campos**, separando explícitamente los dos operandos fuente del destino.
* **Estructura (según el material de estudio):**
  * `DATO OBJETO` (Resultado/Destino)
  * `DATO FUENTE 1` (Argumento 1)
  * `DATO FUENTE 2` (Argumento 2)
  * `OPERACIÓN` (Operador)

---

## 4. Ejemplos Prácticos de Traducción

### Caso Base: $w = x + y + 24 - z$

#### A) En Código P
1. `load x` *(Pila: $x$)*
2. `load y` *(Pila: $x, y$)*
3. `add` *(Pila: $x+y$)*
4. `load 24` *(Pila: $x+y, 24$)*
5. `add` *(Pila: $x+y+24$)*
6. `load z` *(Pila: $x+y+24, z$)*
7. `sub` *(Pila: $x+y+24-z$)*
8. `store w` *(Pila vacía; $w = x+y+24-z$)*

#### B) En Triplos (5 registros usando temporal acumulador)
| DATO OBJETO | DATO FUENTE | OPERADOR | Explicación del paso |
| :--- | :--- | :--- | :--- |
| $t1$ | $x$ | *(vacío / asignación)* | $t1 = x$ (Recibe valor inicial) |
| $t1$ | $y$ | $+$ | $t1 = t1 + y$ |
| $t1$ | $24$ | $+$ | $t1 = t1 + 24$ |
| $t1$ | $z$ | $-$ | $t1 = t1 - z$ |
| $w$ | $t1$ | $=$ | $w = t1$ (Copia resultado final) |

#### C) En Cuádruplos (3 registros)
| DATO OBJETO | DATO FUENTE 1 | DATO FUENTE 2 | OPERACIÓN | Explicación del paso |
| :--- | :--- | :--- | :--- | :--- |
| $t1$ | $x$ | $y$ | $+$ | $t1 = x + y$ |
| $t1$ | $t1$ | $24$ | $+$ | $t1 = t1 + 24$ |
| $w$ | $t1$ | $z$ | $-$ | $w = t1 - z$ |

---

## 5. Ejercicios Resueltos (Casos de Estudio)

### Ejercicio 1 (Código P): $r = (a + b) - (c + d)$
```text
load a
load b
add       // Calcula (a + b) y lo deja en la pila
load c
load d
add       // Calcula (c + d) en la cima de la pila
sub       // Resta el segundo grupo al primero: (a + b) - (c + d)
store r   // Guarda en r
```

### Ejercicio 2 (Triplos): $s = a - b + 8 + c$
| DATO OBJETO | DATO FUENTE | OPERADOR |
| :--- | :--- | :--- |
| $t1$ | $a$ | |
| $t1$ | $b$ | $-$ |
| $t1$ | $8$ | $+$ |
| $t1$ | $c$ | $+$ |
| $s$ | $t1$ | $=$ |

### Ejercicio 3 (Cuádruplos): $u = (m + n) - 12 + p$
| DATO OBJETO | DATO FUENTE 1 | DATO FUENTE 2 | OPERACIÓN |
| :--- | :--- | :--- | :--- |
| $t1$ | $m$ | $n$ | $+$ |
| $t1$ | $t1$ | $12$ | $-$ |
| $u$ | $t1$ | $p$ | $+$ |