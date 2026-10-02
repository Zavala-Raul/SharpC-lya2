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

El ejecutable imprime `OK` o `FALLO` por caso y devuelve código de salida `1` si hay alguna prueba fallida. No requiere paquetes de un framework de pruebas. Los casos están en [`Program.cs`](../tests/Automatas.Pruebas/Program.cs); es un ejecutable de pruebas que se lanza con `dotnet run`, no con `dotnet test`.

## Qué se comprueba

El proyecto compila directamente los archivos actuales de `AnalizadorSintacticoLl1`, `TablaSintacticaSharpC`, `VerificadorTipos`, `CatalogoTipos`, `TokenizadorFuenteSharpC`, `Simbolo` y `Funcion`. No copia ni simula los algoritmos del sintáctico o del verificador.

Cada caso utiliza tablas independientes y exige:

- **Error sintáctico:** que exista el diagnóstico esperado en la línea esperada, con un fragmento específico del mensaje. Se permiten otros mensajes debidos a la recuperación de errores.
- **Error semántico:** que primero pase el análisis sintáctico; después se comprueban el prefijo `ERROR DE TIPO`, el mensaje, la línea y la cantidad de errores. Un rechazo sintáctico no cuenta como detección correcta de un error semántico.
- **Programa válido:** que ninguna de las dos etapas reporte errores.

Los números de línea comienzan en `1`. Un delimitador omitido puede reportarse en la línea del siguiente token, que es donde el sintáctico detecta la omisión. Por ejemplo, la falta de `;` antes de `}` se señala en la línea de esa llave.

### Alcance de la preparación de tokens

Se usa el separador de lexemas real `TokenizadorFuenteSharpC`. Sus lexemas se clasifican mediante un fixture limitado al vocabulario de las pruebas. Esto evita depender de la base SQLite del AFD léxico y permite comprobar los analizadores sintáctico y semántico de forma aislada.

La parte 1 de la entrega 2 añadió 12 casos que ejercitan `Ambito` directamente, sin pasar por el verificador: visibilidad desde la línea de declaración, rechazo del uso anterior, duplicados, prohibición del sombreado, reutilización de nombres entre hermanos, búsqueda a través de dos niveles, aislamiento entre bloques, funciones invocables antes de declararse, parámetros de función, registro de ámbito y clase, orden de declaraciones y errores de entrada. La documentación está en el [plan incremental](plan-semantica-incremental.md). La parte 2, que registra las declaraciones reales del programa, sigue pendiente.

Las tablas comienzan vacías, salvo los casos que proporcionan explícitamente una función conocida; `Verificar` registra las declaraciones presentes en cada fragmento. No se ejecuta la construcción de tablas de `Form1`, el AFD léxico ni la presentación de errores en los controles de Windows Forms. Por ello, los resultados comprueban los diagnósticos devueltos por las clases, no su visualización en la interfaz.

## Resultado de la ejecución

Ejecución realizada con SDK **10.0.401**:

```text
Total: 110; correctas: 110; fallidas: 0.
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

La primera ejecución tenía 51 casos, con 48 correctos y 3 fallidos. Se corrigieron las causas de esos fallos y se agregaron 23 casos de cobertura relacionados. Las expectativas originales de R01–R03 se conservaron. El comando ahora devuelve `0`.

La entrega 1 del [plan incremental](plan-semantica-incremental.md) añadió otros 24 casos: mínimo y máximo de `ENT`, signos, ceros iniciales, valores fuera de rango, números muy largos, destino `DEC`, propagación de `ERROR`, línea del literal, impresión, retornos, argumentos, `POR` y condiciones aisladas. También comprueban la deduplicación por literal y línea y la clasificación de números con exponente como `DEC`. Los resultados de operaciones constantes y el rango de `DEC` quedan fuera de esta entrega.

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

Se conservó la interpretación de la gramática y se ajustó `ObtenerPrecedencia`:

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

## Archivos principales de las correcciones

- [`VerificadorTipos.cs`](../Automatas/VerificadorTipos.cs): asignaciones de `POR`, líneas de diagnóstico y prioridades de `!`.
- [`TablaSintacticaSharpC.cs`](../Automatas/TablaSintacticaSharpC.cs): niveles aritméticos de condiciones y paréntesis.
- [`AnalizadorSintacticoLl1.cs`](../Automatas/AnalizadorSintacticoLl1.cs): mensajes para los nuevos no terminales.
- [`Program.cs` de pruebas](../tests/Automatas.Pruebas/Program.cs): 98 casos reproducibles.
