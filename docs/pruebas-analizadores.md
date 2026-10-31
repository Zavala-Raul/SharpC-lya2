# Pruebas de diagnósticos sintácticos y semánticos

## Cómo ejecutarlas

Requisito: SDK de .NET 10. Desde la raíz del repositorio:

```sh
dotnet run --project tests/Automatas.Pruebas/Automatas.Pruebas.csproj
```

En el entorno WSL donde se ejecutaron, se utilizó el SDK instalado en Windows:

```sh
"/mnt/c/Program Files/dotnet/dotnet.exe" run --project tests/Automatas.Pruebas/Automatas.Pruebas.csproj
```

El ejecutable imprime `OK` o `FALLO` por caso y devuelve código de salida `1` si hay alguna prueba fallida. No requiere paquetes de un framework de pruebas. Los casos están en [`Program.cs`](../tests/Automatas.Pruebas/Program.cs), [`PruebasIntegracionAmbitos.cs`](../tests/Automatas.Pruebas/PruebasIntegracionAmbitos.cs), [`PruebasMemoria.cs`](../tests/Automatas.Pruebas/PruebasMemoria.cs), [`PruebasErroresDocumentos.cs`](../tests/Automatas.Pruebas/PruebasErroresDocumentos.cs) y [`PruebasLectura.cs`](../tests/Automatas.Pruebas/PruebasLectura.cs); se lanza con `dotnet run`, no con `dotnet test`.

## Qué se comprueba

El proyecto compila directamente los archivos actuales de `AnalizadorSintacticoLl1`, `TablaSintacticaSharpC`, `VerificadorTipos`, `AnalizadorInicializacion`, `CatalogoTipos`, `Ambito`, `TablaAmbitos`, `RegionMemoria`, `TokenizadorFuenteSharpC`, `Simbolo` y `Funcion`. No copia ni simula los algoritmos del sintáctico o del verificador.

Cada caso utiliza tablas independientes y exige:

- **Error sintáctico:** que exista el diagnóstico esperado en la línea esperada, con un fragmento específico del mensaje. Se permiten otros mensajes debidos a la recuperación de errores.
- **Error semántico:** que primero pase el análisis sintáctico; después se comprueban el prefijo `ERROR DE TIPO`, `ERROR DE ÁMBITO`, `ERROR DE MEMORIA` o `ERROR DE INICIALIZACIÓN`, el mensaje, la línea y la cantidad de errores. Un rechazo sintáctico no cuenta como detección correcta de un error semántico.
- **Programa válido:** que ninguna de las dos etapas reporte errores.

Los números de línea comienzan en `1`. Un delimitador omitido puede reportarse en la línea del siguiente token, que es donde el sintáctico detecta la omisión. Por ejemplo, la falta de `;` antes de `}` se señala en la línea de esa llave.

### Alcance de la preparación de tokens

Se usa el separador de lexemas real `TokenizadorFuenteSharpC`. Sus lexemas se clasifican mediante un fixture limitado al vocabulario de las pruebas. Esto evita depender de la base SQLite del AFD léxico y permite comprobar los analizadores sintáctico y semántico de forma aislada.

La parte 1 de la entrega 2 añadió 12 casos que ejercitan `Ambito` directamente, sin pasar por el verificador: visibilidad desde la línea de declaración, rechazo del uso anterior, duplicados, prohibición del sombreado, reutilización de nombres entre hermanos, búsqueda a través de dos niveles, aislamiento entre bloques, funciones invocables antes de declararse, parámetros de función, registro de ámbito y clase, orden de declaraciones y errores de entrada. La integración posterior añadió 38 casos de programas completos. Las reglas y el historial están en el [anexo técnico consolidado](presentacion-analizador-sharpc-tecnica.md#anexo-técnico-consolidado).

Las tablas comienzan vacías, salvo los casos que proporcionan explícitamente una firma externa; `Verificar` construye las declaraciones mediante `TablaAmbitos`, igual que en `Form1`. También se comprueban IDs, tipos y valores separados entre hermanos, firmas y límites de funciones multilínea, y reinicio de estado. No se ejecutan el AFD léxico ni los controles de Windows Forms. Los resultados comprueban los diagnósticos devueltos por las clases, no su visualización en la interfaz.

## Resultado de la ejecución

Ejecución realizada con SDK **10.0.401**:

```text
Total: 317; correctas: 317; fallidas: 0.
```

| Grupo | Casos | Resultado |
| --- | --- | --- |
| V01–V12: controles válidos | 12 | Todos correctos. |
| S01–S13: errores sintácticos y recuperación | 13 | Todos correctos. |
| M01–M23: errores semánticos | 23 | Todos correctos. |
| R01–R22: regresiones y consistencia entre etapas | 22 | Todos correctos. |
| C01–C04: condiciones aisladas | 4 | Todos correctos. |
| N01–N24: rango de literales ENT y clasificación numérica | 24 | Todos correctos. |
| A01–A12: modelo de ámbitos | 12 | Todos correctos. |
| B01–B38: integración de ámbitos, declaraciones y referencias | 38 | Todos correctos. |
| D01–D15: disposición de memoria simbólica | 15 | Todos correctos. |
| E01–E37: errores semánticos de 1.6/1.7 | 37 | Todos correctos. |
| L01–L11: instrucción `LEER` y destino de lectura | 11 | Todos correctos. |
| I01–I26: generación de cuádruplos | 26 | Todos correctos. |
| G01–G42: lenguaje completo y presentación textual | 42 | Todos correctos. |
| T01–T20: retorno garantizado en semántica | 20 | Todos correctos. |
| U01–U18: Triplos, Código P y tres notaciones | 18 | Todos correctos. |

También se compiló correctamente el proyecto Windows Forms original, .NET Framework 4.7.2, Debug/x64:

```sh
"/mnt/c/Program Files/dotnet/dotnet.exe" msbuild Automatas/Automatas.csproj /t:Build /p:Configuration=Debug /p:Platform=x64 /v:minimal /nologo
```

La compilación verifica la integración con `Form1`; no constituye una prueba visual de la aplicación.

También se realizaron comprobaciones de integración en Windows: instanciar el formulario y accionar «Analizar y generar» con el AFD SQLite real. Tras ampliar la generación se verificaron 26 cuádruplos con función, llamada, POR, SI/SINO e IMP; dos filas de LEER/IMP; y diagnóstico sin código parcial para ROMPER fuera de un contenedor. Las comprobaciones anteriores cubrieron padres de los controles/pestañas, limpieza al editar y bloqueo por errores de análisis. No se automatizó la inspección visual del diseño.

I01–I26 están en [`PruebasCodigoIntermedio.cs`](../tests/Automatas.Pruebas/PruebasCodigoIntermedio.cs). Comprueban precedencia/asociatividad, grupos hermanos, autoasignación, copias, temporales de tipo estable, promoción, división entera antes de asignar a decimal, identidad de símbolos, posición/línea, signos, exponente, reinicio y no mutación de regiones. I15–I24 ahora exigen traducción de las construcciones antes fuera del alcance. Se conserva el bloqueo por análisis no válido.

G01–G32 están en [`PruebasGeneracionCompleta.cs`](../tests/Automatas.Pruebas/PruebasGeneracionCompleta.cs). Un oráculo de ejecución exclusivo de las pruebas comprueba salidas de selección, repetición, funciones recursivas y anidadas, parámetros, retornos, capturas antes de llamadas y evaluación estricta de ambos operandos lógicos. También se verifican etiquetas y marcos, todas las incorporadas, lectura tipada, homónimos, diagnósticos de ROMPER/casos incompatibles/VAC usado como valor y generación de los controles válidos de la batería anterior. Las pruebas de consola usan el fixture léxico; la comprobación del formulario es independiente.

La primera ejecución tenía 51 casos, con 48 correctos y 3 fallidos. Se corrigieron las causas de esos fallos y se agregaron 23 casos de cobertura relacionados. Las expectativas originales de R01–R03 se conservaron. El comando ahora devuelve `0`.

La entrega de tipos añadió otros 24 casos: mínimo y máximo de `ENT`, signos, ceros iniciales, valores fuera de rango, números muy largos, destino `DEC`, propagación de `ERROR`, línea del literal, impresión, retornos, argumentos, `POR` y condiciones aisladas. También comprueban la deduplicación por literal y línea y la clasificación de números con exponente como `DEC`. Los resultados de operaciones constantes y el rango de `DEC` quedan fuera de esta entrega.

B01–B38 comprueban uso anterior en la misma línea, duplicados, sombreado independiente del orden textual, hermanos, parámetros, llamadas adelantadas, recursión, funciones anidadas, ámbito de `POR`, `REPT`/`HASTA`, casos de `ENCASO`, referencias en impresión/retornos/argumentos, expresiones de argumentos y conservación de símbolos tras errores. Véanse [Ámbitos y declaraciones](presentacion-analizador-sharpc-tecnica.md#reglas-de-ámbitos-y-declaraciones).

D01–D10 construyen programas completos para comprobar offsets y tamaños en `INI` y marcos, parámetros, funciones anidadas, bloques y ciclos, declaraciones rechazadas, reasignaciones y reinicio del análisis. D11–D15 prueban directamente reserva única, desbordamiento sin mutación, rechazo de funciones como variables, `VAC` y tipos desconocidos. Véase [Memoria simbólica](presentacion-analizador-sharpc-tecnica.md#modelo-de-memoria-simbólica).

E01–E37 cubren inicialización definida por caminos (`SI`/`SINO`, `MIENT`, `POR`, `REPT`, `ENCASO`), parámetros, locales, lecturas en condiciones, llamadas y retornos. También verifican cantidad y tipos de argumentos, promociones `ENT → DEC`, retorno `VAC`, función contenedora y ausencia de errores derivados. R04 ahora espera tanto el error del destino no declarado como el uso independiente de `vI` sin inicializar. La [matriz 1.6/1.7](#cobertura-de-los-temas-16-y-17) distingue lo detectable estáticamente de errores de ejecución.

L01–L11 reproducen el problema observado con `LEER(vNombre);`: el léxico reconoce `LEER` como `PR4` pero faltaba una producción para usarlo como instrucción independiente. Prueban variable declarada, iniciada después de la lectura, ámbitos y ramas, nombres ausentes y destinos sintácticamente inválidos; conservan además `vNombre = LEER();`. El fixture clasifica tokens para el parser y la semántica; la captura de la aplicación confirma la categoría `PR4` del AFD configurado. Véanse [Ejemplos de prueba](ejemplos-prueba.md) y [Guía de escritura](reglas-lexicas.md).

### Errores sintácticos corroborados

- Inicio sin `INI`.
- Falta de llave de apertura o cierre.
- Falta de punto y coma, identificador o `=`.
- Asignación sin expresión.
- Dos números sin operador intermedio.
- Operadores consecutivos sin operando.
- Paréntesis sin cerrar.
- `SI` sin condición.
- Token extra antes de un identificador.
- Recuperación tras una omisión y detección de otra omisión en la siguiente instrucción.

### Errores semánticos corroborados

- Asignación y reasignación incompatibles, incluida conversión no permitida de `DEC` a `ENT`.
- Variable de destino, operando o función no declarados en las tablas de prueba.
- Multiplicación de texto y resta de booleano.
- Cambio de agrupación por paréntesis que conduce a una operación incompatible.
- Comparación entre tipos incompatibles.
- Uso de operandos no booleanos en `&`, `|` y `!`.
- Condiciones no booleanas de `SI`, `MIENT`, `HASTA` y `POR`.
- Inicialización de `POR` incompatible cuando incluye declaración.
- Incremento de `POR` incompatible.
- Propagación de un error interno sin duplicar el diagnóstico de asignación.
- Línea correcta para una variable desconocida dentro de una expresión multilínea.
- Dos errores de tipo independientes en instrucciones sucesivas.
- Tipo de retorno de función incompatible con el destino.

Los controles válidos cubren los tipos básicos, promoción `ENT → DEC`, declaración y reasignación, precedencia, agrupación, operadores lógicos, ciclos y tipo de retorno compatible. La aceptación de potencias encadenadas no demuestra por sí sola su asociatividad: distintas agrupaciones numéricas pueden producir el mismo tipo.

Los casos adicionales comprueban variables de destino no declaradas en `POR`, líneas de encabezados multilínea, promociones válidas, negaciones sucesivas, aritmética agrupada a ambos lados de una comparación, condiciones agrupadas dentro de ciclos, errores de tipos en grupos y sintaxis incompleta. También ejercitan `CrearTablaCondicion` directamente hasta `EOF`.

## Problemas encontrados y cómo se corrigieron

### R01. Inicialización incompatible de POR sin declaración

```text
INI {
ENT vI;
POR (vI = "hola"; vI < 3; vI = vI + 1) { }
}
```

**Esperado, línea 3:**

```text
ERROR DE TIPO: No se puede asignar una expresión de tipo 'TXT' a la variable 'vI' de tipo 'ENT'.
```

**Antes:** sin errores sintácticos ni semánticos. **Ahora:** se devuelve el error esperado en la línea 3.

#### Causa

En `VerificadorTipos.VerificarBuclePor`, `idx` empezaba en `0` y solo cambiaba a `2` si había un tipo declarado. En el caso `vI = ...`, el código buscaba `ASIG` en la posición `0`, donde se encuentra `IDV`; por eso no llamaba a `VerificarAsignacion` para la inicialización.

#### Solución paso a paso

Se extrajo la comprobación a `VerificarAsignacionPor`, compartida por la inicialización y el incremento. Primero se identifica la posición de la variable:

| Forma | Posición de la variable | Posición de `=` | Inicio de la expresión |
| --- | --- | --- | --- |
| `ENT vI = 0` | 1 | 2 | 3 |
| `vI = 0` | 0 | 1 | 2 |

En ambos casos se aplica la misma relación:

```csharp
int indiceVariable = esDeclaracion ? 1 : 0;
int indiceAsignacion = indiceVariable + 1;
int inicioExpresion = indiceAsignacion + 1;
```

Después:

1. Si hay declaración, registra el tipo.
2. Consulta el tipo de la variable de destino.
3. Si no existe, registra el error de variable no declarada.
4. Extrae los tokens posteriores a `=`.
5. Llama al mismo `VerificarAsignacion` que aplica las reglas de compatibilidad del resto del programa.

La línea del error se obtiene del token de la variable, no de la palabra `POR`. Así funciona también cuando la inicialización o el incremento están escritos en líneas posteriores. Para la condición se utiliza la línea de su primer token.

**Idea para explicarlo:** «El error estaba en dónde buscábamos el signo igual. Ahora lo localizamos respecto de la variable y usamos la misma comprobación en las dos asignaciones del ciclo».

### R02. Negación coherente con la gramática

```text
INI {
SI (!2 < 3) { }
}
```

La gramática actual admite `COND_NOT → ! COND_NOT`, y ese segundo `COND_NOT` puede reconocer la comparación `2 < 3`. Bajo esa gramática, la condición equivale a `!(2 < 3)` y debe tener tipo `BOOL`.

**Esperado según la gramática:** ninguna incompatibilidad de tipos.

**Antes, línea 2:**

```text
ERROR DE TIPO: El operador lógico '!' requiere un operando 'BOOL', pero recibió 'ENT'.
```

**Ahora:** el sintáctico y el semántico aceptan la condición sin errores.

#### Causa

El sintáctico aceptaba la condición. El verificador daba prioridad máxima a `!` y lo aplicaba al `2` antes de aplicar `<`.

#### Solución paso a paso

Se conservó la interpretación de la gramática y se ajustó la precedencia (actualmente centralizada en `OperadoresExpresion.Precedencia`):

```text
Mayor prioridad
  ^                         6
  * /                       5
  + -                       4
  == <> < > <= >=           3
  !                         2
  &&                        1
  ||                        0
Menor prioridad
```

Los paréntesis siguen siendo barreras explícitas en la pila de operadores. Los nombres `&&` y `||` son las representaciones internas de los tokens de `&` y `|`.

Con `!2 < 3`, la pila funciona así (cima a la derecha):

| Paso | `pilaTipos` | `pilaOps` |
| --- | --- | --- |
| Leer `!` | `[]` | `[!]` |
| Leer `2` | `[ENT]` | `[!]` |
| Leer `<`: tiene mayor prioridad que `!` | `[ENT]` | `[!, <]` |
| Leer `3` | `[ENT, ENT]` | `[!, <]` |
| Aplicar `<` | `[BOOL]` | `[!]` |
| Aplicar `!` | `[BOOL]` | `[]` |

La regla de tipos de `!` no se relajó: sigue exigiendo `BOOL`. Lo que cambió fue el momento de aplicarla. `!2` sigue siendo un error, y `(!2) < 3` también, porque el cierre del paréntesis obliga a aplicar `!` al entero antes de la comparación exterior.

Esta precedencia pertenece al lenguaje del proyecto, no a C#. La potencia sigue siendo asociativa a la derecha; solo se reordenaron los niveles de prioridad respecto de la negación.

**Idea para explicarlo:** «No permitimos negar números; dejamos que primero termine la comparación para que la negación reciba un booleano».

El caso explícitamente agrupado `SI (!(2 < 3)) { }` tiene un control equivalente que sí pasa.

### R03. Operandos agrupados dentro de comparaciones

```text
INI {
SI ((2 + 3) < 6) { }
}
```

**Esperado para admitir operandos aritméticos agrupados:** sintaxis válida y condición de tipo `BOOL`.

**Antes, línea 2:**

```text
SINTAXIS INVÁLIDA: Token inesperado '<'. Verifique la escritura de la instrucción según el lenguaje SharpC.
```

**Ahora:** el programa se acepta y la comparación tiene tipo `BOOL`.

#### Causa

En `TablaSintacticaSharpC.AgregarCondiciones`, cuando `COND_REL` comenzaba por `(` se seleccionaba `COND_REL → ( COND )`. Esa producción no admitía después una continuación relacional, como `< 6`. Por ello se rechazaba la expresión antes de llegar al verificador de tipos.

#### Solución paso a paso

Ahora toda comparación comienza por una expresión, incluso si su primer token es un paréntesis:

```text
COND_REL   → EXP_COND REL_OPC
REL_OPC    → operador_relacional EXP_COND | ε

EXP_COND   → TERM_COND EXP_P_COND
TERM_COND  → POT_COND TERM_P_COND
POT_COND   → VALOR_COND POT_P_COND
VALOR_COND → literal | variable | llamada | ( COND )
```

Las continuaciones `_P_COND` reconocen los mismos operadores de suma, producto y potencia que sus equivalentes aritméticos. La idea es bajar los paréntesis hasta el nivel de **valor**, para que el grupo no termine obligatoriamente toda la comparación.

En `(2 + 3) < 6`:

1. `VALOR_COND` abre el grupo y reconoce `2 + 3` dentro de `COND`. Una condición puede reconocer sintácticamente una expresión sin comparación; su tipo se comprueba después.
2. Al cerrar `)`, el grupo completo forma un valor de `EXP_COND`.
3. Las continuaciones aritméticas terminan al ver `<`.
4. `REL_OPC` consume `<` y reconoce `6` como la expresión derecha.
5. El semántico obtiene `ENT < ENT → BOOL`.

Esto también permite `(2 + 3) * 4 < 30` y `(2 < 3) & VDD`. Si alguien usa un grupo booleano en una operación numérica, como `(2 < 3) + 1`, el sintáctico reconoce la estructura y el semántico reporta `BOOL + ENT` incompatible.

`AgregarNivelesAritmeticos` construye las reglas compartidas una sola vez en el código y las registra con dos juegos de nombres: los habituales para asignaciones y los terminados en `_COND` para condiciones. De ese modo se conserva la jerarquía aritmética y no se duplican manualmente sus reglas. El analizador continúa siendo predictivo LL(1): no se añadió retroceso ni búsqueda de varias alternativas para el mismo token.

También se adaptó `ObtenerAyudaSintaxis` para que los nuevos nombres usen los mismos mensajes aritméticos, y se agregaron salidas `ε` ante `EOF` en las continuaciones de condiciones. Esto permite probar condiciones aisladas sin envolverlas artificialmente en `INI { ... }`.

**Idea para explicarlo:** «Un paréntesis agrupa un valor que puede seguir participando en operaciones. Antes se trataba como si al cerrarlo ya hubiera terminado toda la comparación».

## Cobertura de los temas 1.6 y 1.7

Contraste con [el documento de clase](LyA2_1.6%261.7_Documento.pdf): páginas 1–5, símbolos y direcciones; páginas 6–7, errores y tratamiento. Esta sección conserva la matriz antes publicada como guía independiente.

| Tema o categoría | Comprobación | Evidencia |
| --- | --- | --- |
| Declaraciones y usos | Cada referencia resuelve a una declaración visible concreta. | B01, B05, B08, B14, B20–B23. |
| Redefinición | Rechazo de duplicados y sombreado; nombres repetidos permitidos entre hermanos. | B02–B04, B09, B29, B32–B33. |
| Atributos y dirección | Nombre, tipo, clase, ámbito, línea, región, tamaño y offset. | D01–D05, D07–D08; tablas de símbolos/funciones. |
| Reserva y recursos | Tamaños por tipo, intervalos sin solapamiento, marcos independientes, reserva única y suma comprobada. | D01–D15. |
| Inicialización | Lecturas previas a asignación; intersección de ramas; ciclos conservadores; parámetros inicialmente asignados. | E01–E13, E29, E32–E37; L01–L11. |
| Tipos | Operadores, asignaciones, condiciones booleanas, rango de literal `ENT`, firmas, argumentos y retorno presente. Promoción `ENT → DEC`. | M01–M23, N01–N24, E14–E28, E30–E31. |
| Lógica tipada | Comparaciones y operadores con tipos compatibles; no deduce la intención del autor ni terminación de ciclos. | M07–M17, R02, R09–R18. |
| Alcance | Uso anterior, fuera de bloque, caso o función. | B01–B38. |
| Acceso a memoria física | No se emite; la dirección actual es región + desplazamiento simbólico. | [Modelo de memoria](presentacion-analizador-sharpc-tecnica.md#modelo-de-memoria-simbólica). |

Límites: no se comprueban fugas reales, rango de `DEC` ni desbordamiento de resultados calculados. `AnalizadorRetornos` sí comprueba conservadoramente el retorno por caminos y descarta sentencias inalcanzables para ese análisis. No se evalúan constantes para descartar ramas imposibles ni se elimina código inalcanzable del IR. Una asignación con error previo de tipo/nombre se considera asignada en inicialización para evitar cascadas.

El tratamiento es **reportar y continuar** en los errores recuperables. Un fallo léxico bloquea LL(1); uno sintáctico bloquea semántica. Con sintaxis válida se acumulan errores de ámbito, memoria, tipos e inicialización. `dgvErrores` muestra los léxicos; `dgvErroresSintaxis`, sintácticos y semánticos. Cada análisis reconstruye resultados. Asertos, reintentos y monitorización descritos en el PDF no son características implementadas del lenguaje.

G33–G35 comprueban el código de tres direcciones: orden de operaciones, promociones, funciones, saltos, incorporadas, campos ausentes y desambiguación de homónimos compartida con la tabla. Los controles válidos de generación también pasan por el formateador para detectar instrucciones sin representación textual.

## Alineación con ACT 1 y ACT 3

Se verificaron los documentos de definición y expresiones regulares originales. Los nombres de PR6–PR10 son LIMP, FONCOL, TXTCOL, SLTLN y POS; PR29–PR31 son INTER, TECL e IMPREP. PR21 es FALS y PR22 es NUL. Se eliminó del fixture el falso FLS y los nombres inventados de incorporadas. Los operadores OPR1–OPR6 del fixture y la normalización por categoría ahora siguen ACT 3; se conserva prioridad del lexema reconocido para evitar cambios de significado.

G24 y G34 usan las incorporadas oficiales. G37–G42 verifican intercambio real (ENT, DEC, TXT, BOOL, CAR y variable repetida), rechazo de tipos distintos/diferentes del tipo explícito, lectura de variables inicializadas, colores hexadecimales y cantidades ENT de SLTLN/IMPREP. INTER produce tres copias con temporal; no representa arreglos. Total: **299/299**.

## Nuevas representaciones y equivalencia

[`PruebasRepresentaciones.cs`](../tests/Automatas.Pruebas/PruebasRepresentaciones.cs) añade U01–U18: expresiones fuente completas en infija/prefija/postfija, precedencia y asociatividad, llamadas con aridad, literales con espacios, lectura, condiciones, retornos, incorporadas, homónimos, errores, reinicio y balance de pila. También prueba `vA = 3 - vA` para impedir que un acumulador sobrescriba el operando derecho.

Los casos ejecutables del grupo G ahora comparan sus resultados con los obtenidos después de traducir a Triplos y Código P. Adaptadores exclusivos de las pruebas reconstruyen operaciones desde los campos de esas representaciones; no reutilizan los operandos del cuádruplo de origen para ocultar pérdidas de información. Solo toman de él los metadatos de posición y función. Las pruebas cubren selección, repetición, recursión, ámbitos anidados y captura de valores antes de llamadas con efectos.

Los controles válidos también pasan por `ConstructorRepresentaciones`, de modo que una instrucción del lenguaje sin traducción no puede pasar inadvertida. Una operación desconocida produce diagnóstico con línea y sin vistas parciales. La interfaz publica las cinco vistas después de preparar conversiones y textos; sus diagnósticos se hacen visibles cuando existen.

Comprobación del formulario en Windows con SQLite real: cinco vistas interiores, 3 cuádruplos/7 triplos/2 expresiones para el ejemplo aritmético, las tres columnas de notación correctas, función y ciclo en Código P, controles con espacio útil a 900 × 600, diagnóstico visible para ROMPER inválido y bloqueo de todas las vistas ante error semántico.

## Archivos principales de las correcciones

**G36 — operadores relacionales del AFD real.** La base configurada devuelve `< → OPR2`, `<> → OPR5`, `> → OPR1`, `<= → OPR4`, `>= → OPR3`, `== → OPR6`, una numeración distinta del fixture histórico. `OperadoresExpresion.Normalizar` prioriza ahora el lexema reconocido para preservar el significado en inferencia y generación. Se comprobaron los seis operadores a través del formulario con SQLite real, además de la regresión automatizada.

**T01–T20 — retorno garantizado.** [`PruebasRetornos.cs`](../tests/Automatas.Pruebas/PruebasRetornos.cs) comprueba funciones vacías, SI/SINO, retornos posteriores, ciclos, REPT con ROMPER, ENCASO, funciones anidadas, VAC y líneas de declaración. Exige diagnóstico semántico y bloqueo de generación para caminos incompletos. El generador y formateador ya no emiten validación de falta de retorno. G27 ahora verifica rechazo semántico de la función vacía. El ejemplo de función duplicada de D08 conserva su prueba de memoria, pero añade retornos para aislar ese error de alcance. Total: **293/293**.

- [`VerificadorTipos.cs`](../Automatas/VerificadorTipos.cs): asignaciones de `POR`, líneas de diagnóstico y prioridades de `!`.
- [`TablaSintacticaSharpC.cs`](../Automatas/TablaSintacticaSharpC.cs): niveles aritméticos de condiciones y paréntesis.
- [`AnalizadorSintacticoLl1.cs`](../Automatas/AnalizadorSintacticoLl1.cs): mensajes para los nuevos no terminales.
- [`Program.cs` de pruebas](../tests/Automatas.Pruebas/Program.cs), [`PruebasIntegracionAmbitos.cs`](../tests/Automatas.Pruebas/PruebasIntegracionAmbitos.cs), [`PruebasMemoria.cs`](../tests/Automatas.Pruebas/PruebasMemoria.cs), [`PruebasErroresDocumentos.cs`](../tests/Automatas.Pruebas/PruebasErroresDocumentos.cs), [`PruebasLectura.cs`](../tests/Automatas.Pruebas/PruebasLectura.cs), [`PruebasCodigoIntermedio.cs`](../tests/Automatas.Pruebas/PruebasCodigoIntermedio.cs), [`PruebasGeneracionCompleta.cs`](../tests/Automatas.Pruebas/PruebasGeneracionCompleta.cs), [`PruebasRetornos.cs`](../tests/Automatas.Pruebas/PruebasRetornos.cs) y [`PruebasRepresentaciones.cs`](../tests/Automatas.Pruebas/PruebasRepresentaciones.cs): 317 casos reproducibles.
