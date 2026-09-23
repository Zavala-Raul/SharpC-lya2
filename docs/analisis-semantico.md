# Análisis semántico: jerarquía de operaciones, tipos y pilas

Esta guía explica la implementación actual del proyecto y sirve como apoyo para exponerla. La idea central es:

> El analizador sintáctico comprueba la estructura de una expresión. El verificador semántico determina su tipo y comprueba que las operaciones y asignaciones sean compatibles.

## 1. Cómo se conectan las etapas

En `Form1.cs`, el flujo de análisis utiliza los tokens del programa y las tablas de símbolos y funciones. Si no hay errores léxicos registrados y existen tokens, ejecuta el análisis sintáctico; si este termina sin errores, ejecuta la verificación de tipos.

```text
Tokens y tablas de símbolos/funciones
                  |
                  v
AnalizadorSintacticoLl1: ¿la estructura es válida?
                  |
          si no hay errores
                  v
VerificadorTipos: ¿los tipos son compatibles?
                  |
                  v
Lista de errores de tipo, con línea y mensaje
```

Por ejemplo:

```text
ENT vEdad = "hola";
```

La declaración tiene una estructura válida: tipo, variable, asignación, expresión y punto y coma. Sin embargo, su significado es incorrecto para las reglas del proyecto: intenta guardar un `TXT` en una variable `ENT`.

**Para explicarlo:** «La sintaxis revisa cómo está escrita la instrucción; la semántica revisa si los tipos permiten hacer lo que la instrucción pide».

## 2. Jerarquía de operaciones del analizador sintáctico

### 2.1. Orden de prioridad

La jerarquía aritmética está definida en `TablaSintacticaSharpC.cs`, dentro de `AgregarExpresiones`:

| Prioridad, de mayor a menor | Elemento | Parte de la gramática |
| --- | --- | --- |
| 1 | Agrupación mediante paréntesis `( … )` | `VALOR → ( EXP )` |
| 2 | Potencia `^` | `POT` y `POT_P` |
| 3 | Multiplicación `*` y división `/` | `TERM` y `TERM_P` |
| 4 | Suma `+` y resta `-` | `EXP` y `EXP_P` |

Los paréntesis no son una operación aritmética: obligan a tratar una expresión completa como una unidad.

### 2.2. Cómo la gramática impone esa prioridad

Estas son las producciones aritméticas esenciales, escritas con los símbolos del lenguaje en lugar de sus códigos de token:

```text
EXP    → TERM EXP_P
EXP_P  → + TERM EXP_P | - TERM EXP_P | ε

TERM   → POT TERM_P
TERM_P → * POT TERM_P | / POT TERM_P | ε

POT    → VALOR POT_P
POT_P  → ^ POT | ε

VALOR  → identificador | literal | llamada a función | ( EXP )
```

`ε` significa «no agregar nada»; en el código se representa con `"e"`. Los nombres terminados en `_P` representan la continuación de ese nivel: permiten reconocer operadores adicionales o terminarlo.

La expresión se construye por niveles:

```text
EXP: suma y resta
 └─ TERM: multiplicación y división
     └─ POT: potencia
         └─ VALOR: dato individual o expresión entre paréntesis
```

Aunque el reconocimiento comienza por `EXP`, sus operandos son términos completos. Por eso una multiplicación queda dentro de un término antes de combinarse con una suma. **Comenzar a reconocer por `EXP` no significa dar mayor prioridad a la suma.**

`AnalizadorSintacticoLl1` consulta esta gramática mediante la tabla `tablaM`. En `ExpandirProduccion`, coloca la producción en su pila en orden inverso para procesar primero el símbolo de la izquierda. No calcula resultados numéricos ni construye un árbol de expresión en este método; comprueba la estructura usando la tabla.

### 2.3. Ejemplos para explicar la agrupación

```text
2 + 3 * 4      → 2 + (3 * 4)
(2 + 3) * 4    → la suma queda agrupada como un VALOR
2 + 3 * 4 ^ 2  → 2 + (3 * (4 ^ 2))
```

Los resultados matemáticos serían `14`, `20` y `50`, respectivamente. Sirven para ilustrar la prioridad; estas etapas del proyecto no ejecutan esas cuentas.

**Para explicarlo:** «Una suma recibe términos completos; un término recibe potencias completas; y una potencia recibe valores. Si el valor contiene paréntesis, se vuelve a reconocer una expresión dentro de ellos».

### 2.4. Precedencia y asociatividad son distintas

- **Precedencia:** decide qué operador agrupa primero cuando hay operadores de distinta prioridad. Ejemplo: `*` frente a `+`.
- **Asociatividad:** decide cómo se agrupan operadores con la misma prioridad. Ejemplo: dos potencias seguidas.

La producción `POT_P → ^ POT` hace que la gramática agrupe `2 ^ 3 ^ 2` como `2 ^ (3 ^ 2)`, hacia la derecha.

Hay una diferencia en la implementación semántica: `EvaluarTipoExpresion` aplica los operadores pendientes cuya precedencia es **mayor o igual** que la del nuevo operador. Por eso procesa las potencias encadenadas hacia la izquierda. En una cadena formada únicamente por números, esto no cambia el tipo inferido (`ENT` o `DEC`), pero sí cambiaría el valor si ese algoritmo se usara para ejecutar las operaciones. Es importante no afirmar que ambas etapas coinciden en la asociatividad de `^`.

## 3. Verificación de tipos

### 3.1. Responsabilidad de la clase

`VerificadorTipos.cs` contiene la clase que infiere tipos de expresiones y registra incompatibilidades. Trabaja con:

| Dato | Para qué sirve |
| --- | --- |
| `tablaSimbolos` | Consultar variables y su propiedad `Tipo`. |
| `tablaFunciones` | Consultar funciones y su `TipoRetorno`. |
| `tiposVariables` | Mantener los tipos conocidos por nombre, incluidos parámetros y declaraciones reconocidas. |
| `Errores` | Guardar pares de línea y mensaje de error. |

La tabla de símbolos es un diccionario de información sobre variables. No es la pila semántica.

### 3.2. Recorrido principal

El método `Verificar` recorre los tokens y reconoce casos concretos:

1. **Declaración con asignación:** registra el tipo declarado y comprueba la expresión inicial.
2. **Declaración sin asignación:** registra el tipo de la variable.
3. **Reasignación:** consulta el tipo de la variable y comprueba el nuevo valor.
4. **Condiciones de `SI`, `MIENT` y `HASTA`:** comprueba que el tipo resultante sea `BOOL`.
5. **Bucle `POR`:** delega en `VerificarBuclePor`, que separa inicialización, condición e incremento e intenta comprobar esas partes.

Para una asignación, la secuencia principal es:

```text
VerificarAsignacion
  → EvaluarTipoExpresion
      → ObtenerTipoOperando
      → AplicarOperador / EvaluarOperacion
  → SonCompatiblesAsignacion
  → registrar error o guardar el texto de la expresión
```

### 3.3. Cómo obtiene el tipo de un operando

`ObtenerTipoOperando` aplica, entre otras, estas reglas:

| Operando | Tipo obtenido |
| --- | --- |
| Número sin punto, como `12` | `ENT` |
| Número con punto, como `12.5` | `DEC` |
| Cadena, como `"hola"` | `TXT` |
| Carácter, como `'A'` | `CAR` |
| Token de literal booleano | `BOOL` |
| Variable | El tipo registrado para su nombre. |
| Función conocida | Su tipo de retorno. |

Una variable o función que no se encuentra produce un error. `ERROR` indica que se detectó un problema; `DESCONOCIDO` indica que no se logró identificar un tipo. No son tipos declarables del lenguaje.

### 3.4. Reglas de las operaciones

En `EvaluarOperacion`, los operadores aritméticos `+`, `-`, `*`, `/` y `^` comparten estas reglas numéricas:

| Tipo izquierdo | Tipo derecho | Tipo resultante |
| --- | --- | --- |
| `ENT` | `ENT` | `ENT` |
| `ENT` | `DEC` | `DEC` |
| `DEC` | `ENT` | `DEC` |
| `DEC` | `DEC` | `DEC` |

Por ejemplo, `2 + 3.5` produce el **tipo** `DEC`. El verificador no necesita calcular `5.5` para saberlo.

Otras reglas actuales:

- **Concatenación:** `+` devuelve `TXT` si al menos uno de los operandos es `TXT`, después de descartar `ERROR` y `DESCONOCIDO`. La implementación permite, por ejemplo, texto más entero.
- **Comparaciones:** `==`, `<>`, `<`, `>`, `<=` y `>=` devuelven `BOOL` si ambos operandos son numéricos o tienen exactamente el mismo tipo. Esta regla también permite comparar tipos iguales no numéricos.
- **Operadores lógicos binarios:** requieren dos `BOOL` y devuelven `BOOL`. Internamente se normalizan a `&&` y `||` a partir de `OPL1` y `OPL2`.
- **Negación `!`:** requiere un `BOOL` y devuelve `BOOL`; se comprueba en `AplicarOperador`.

La división de dos `ENT` se clasifica como `ENT`, igual que el resto de operaciones entre enteros. Esto describe la inferencia implementada, no un cálculo ni una comprobación de divisiones entre cero.

### 3.5. Reglas de asignación y condiciones

`SonCompatiblesAsignacion` permite:

- Asignar una expresión a una variable del mismo tipo.
- Asignar `ENT` a `DEC` como promoción permitida.

No permite la conversión inversa de `DEC` a `ENT`.

```text
DEC vPrecio = 10;           // Compatible: ENT → DEC.
ENT vCantidad = 10.5;       // Incompatible: DEC → ENT.
TXT vMensaje = "Total: " + 3; // Compatible: la suma produce TXT.
ENT vTotal = "hola" * 2;    // Operación incompatible: TXT * ENT.
```

Son fragmentos de instrucciones para colocar dentro de un programa `INI { ... }`.

En condiciones, `VerificarCondicion` exige `BOOL` cuando logra inferir un tipo válido:

```text
SI (vEdad >= 18) { ... }    // La comparación numérica produce BOOL.
SI (vEdad + 1) { ... }      // Si vEdad es ENT, produce ENT: error de tipo.
```

Aquí `...` representa el cuerpo omitido para explicar la condición; no es código del lenguaje.

**Para explicarlo:** «Primero se obtiene el tipo de cada dato, después se deduce el tipo de las operaciones y, al final, se compara ese resultado con lo que necesita la instrucción».

## 4. La pila semántica sí es explícita

### 4.1. Dónde está

En `EvaluarTipoExpresion` aparecen estas dos estructuras:

```csharp
var pilaTipos = new Stack<string>();
var pilaOps = new Stack<string>();
```

Aunque no existe una clase llamada `PilaSemantica`, **`pilaTipos` cumple explícitamente ese papel**: guarda los tipos de los operandos y de los resultados parciales. `pilaOps` guarda los operadores pendientes y los paréntesis de apertura.

Una pila sigue el principio LIFO: el último elemento que entra es el primero que sale. `Push` agrega, `Pop` extrae y `Peek` consulta la cima sin extraerla.

| Estructura | Ubicación | Contenido |
| --- | --- | --- |
| `pila` sintáctica | `AnalizadorSintacticoLl1.Analizar` | Terminales y no terminales que falta reconocer. |
| `pilaTipos` semántica | `VerificadorTipos.EvaluarTipoExpresion` | Tipos como `ENT`, `DEC`, `BOOL` o `ERROR`. |
| `pilaOps` auxiliar | `VerificadorTipos.EvaluarTipoExpresion` | Operadores pendientes y `(`. |

La pila sintáctica responde «¿qué símbolo espero?». La semántica responde «¿qué tipo tienen los operandos y los resultados parciales?».

### 4.2. Cómo se usan las dos pilas

El algoritmo es similar al de dos pilas usado para procesar expresiones infijas, pero opera sobre **tipos**, no sobre valores:

1. Si encuentra un operando, obtiene su tipo y lo apila en `pilaTipos`.
2. Si encuentra `(`, lo apila en `pilaOps` como barrera de agrupación.
3. Si encuentra `)`, aplica los operadores pendientes hasta llegar a `(` y retira ese paréntesis.
4. Si encuentra un operador binario, aplica primero los pendientes de mayor o igual prioridad y luego apila el nuevo.
5. `!` tiene tratamiento especial: se apila y, al aplicarse, consume un solo tipo.
6. Al terminar la expresión, aplica los operadores restantes y devuelve el tipo de la cima.

`AplicarOperador` extrae primero el operando derecho y después el izquierdo. Consulta `EvaluarOperacion` y coloca el tipo resultante de nuevo en la pila:

```text
[ENT, DEC]  + operador *  →  [DEC]
```

Así, dos tipos de operandos se sustituyen por un tipo de resultado parcial.

### 4.3. Traza completa de ejemplo

Para esta declaración:

```text
DEC vResultado = 2 + 3 * 4.5;
```

El verificador analiza la expresión `2 + 3 * 4.5`. En la siguiente tabla, la cima está a la **derecha**:

| Paso | Acción | `pilaTipos` | `pilaOps` |
| --- | --- | --- | --- |
| Inicio | Crear las pilas | `[]` | `[]` |
| Leer `2` | Apilar `ENT` | `[ENT]` | `[]` |
| Leer `+` | Apilar operador | `[ENT]` | `[+]` |
| Leer `3` | Apilar `ENT` | `[ENT, ENT]` | `[+]` |
| Leer `*` | Tiene mayor prioridad que `+`; se apila | `[ENT, ENT]` | `[+, *]` |
| Leer `4.5` | Apilar `DEC` | `[ENT, ENT, DEC]` | `[+, *]` |
| Fin: aplicar `*` | `ENT * DEC → DEC` | `[ENT, DEC]` | `[+]` |
| Aplicar `+` | `ENT + DEC → DEC` | `[DEC]` | `[]` |
| Devolver el tipo | Extraer el `DEC` final | `[]` | `[]` |

Finalmente, `SonCompatiblesAsignacion("DEC", "DEC")` devuelve verdadero. Si la variable existe en `tablaSimbolos`, se guarda en `Valor` el texto `2 + 3 * 4.5`, **no** el resultado numérico `15.5`.

Con `(2 + 3) * 4.5`, al leer `)` se combina primero `ENT + ENT → ENT`. Después se comprueba `ENT * DEC → DEC`. Los paréntesis cambian el orden de combinación, aunque en este ejemplo el tipo final siga siendo `DEC`.

### 4.4. Prioridades internas del verificador

`ObtenerPrecedencia` devuelve estos números; un número mayor indica mayor prioridad:

| Operador interno | Prioridad |
| --- | --- |
| `!` | 6 |
| `^` | 5 |
| `*`, `/` | 4 |
| `+`, `-` | 3 |
| `==`, `<>`, `<`, `>`, `<=`, `>=` | 2 |
| `&&` | 1 |
| `||` | 0 |

Los paréntesis se procesan mediante reglas especiales, no mediante un número en esta tabla. La jerarquía aritmética se representa, por tanto, en dos lugares: en las producciones del sintáctico y en las prioridades del verificador.

## 5. Alcance de la implementación actual

Estos detalles permiten describir con precisión qué verifica esta clase:

- **Infiere tipos; no ejecuta el programa.** No calcula el valor de las expresiones.
- **Las llamadas a funciones aportan su tipo de retorno.** En `EvaluarTipoExpresion` se saltan los tokens de los argumentos, respetando los paréntesis anidados. Ese recorrido no comprueba la cantidad ni los tipos de los argumentos.
- **No hay una comprobación específica de `REGR` en `Verificar`.** No debe atribuirse a esta clase una validación completa del retorno de las funciones.
- **Los tipos se almacenan por nombre.** `tiposVariables` no es una pila de ámbitos; los parámetros se incorporan al mismo diccionario.
- **El tratamiento de `POR` tiene un caso incompleto.** En la inicialización sin declaración, como `vI = 0`, `idx` queda en `0` y el código busca `ASIG` en esa posición, donde está la variable. Por eso esa rama no verifica la asignación inicial como pretende el comentario. La condición sí se envía a `VerificarCondicion`.
- **`ERROR` y `DESCONOCIDO` se propagan.** La comprobación final de asignación o condición se omite si se obtiene uno de esos estados. Que no aparezca una incompatibilidad final no demuestra que todos los tipos hayan sido resueltos.
- **Las condiciones tienen su propia gramática.** La tabla de prioridades del verificador no describe por sí sola todo el reconocimiento sintáctico de condiciones. Por ejemplo, la gramática permite que `!` preceda a `COND_NOT`, mientras el verificador le asigna prioridad máxima. Para explicar la negación de una comparación sin esa diferencia de agrupación, conviene usar `!(vEdad < 18)`.

## 6. Guion breve para exponer

> «El proyecto separa la revisión de estructura de la revisión de tipos. Primero, el analizador LL(1) comprueba que la expresión siga la gramática.
>
> La jerarquía aritmética está dividida en niveles: paréntesis, potencias, multiplicación y división, y finalmente suma y resta. Una expresión contiene términos, los términos contienen potencias y las potencias contienen valores. Eso hace que, en `2 + 3 * 4.5`, la multiplicación se agrupe antes que la suma.
>
> Después entra la clase `VerificadorTipos`. Esta consulta los tipos de las variables y reconoce los tipos de los literales. No calcula el resultado numérico: calcula el tipo del resultado. Por ejemplo, entero por decimal produce decimal, y entero más decimal también produce decimal.
>
> Para conservar los resultados parciales usa dos pilas explícitas: una de tipos y otra de operadores. Al aplicar un operador binario, saca dos tipos, comprueba si son compatibles y apila el tipo resultante.
>
> Al terminar, comprueba que ese tipo se pueda asignar a la variable, o que sea booleano si se trata de una condición. La pila sintáctica guarda símbolos pendientes; la pila semántica guarda tipos».

## 7. Dónde mostrarlo en el código

| Tema | Archivo y método |
| --- | --- |
| Conexión entre etapas | [`Form1.cs`](../Automatas/Form1.cs): `EjecutarAnalisisSintactico`, `EjecutarVerificacionTipos` y sus llamadas. |
| Jerarquía aritmética | [`TablaSintacticaSharpC.cs`](../Automatas/TablaSintacticaSharpC.cs): `AgregarExpresiones`. |
| Pila de reconocimiento LL(1) | [`AnalizadorSintacticoLl1.cs`](../Automatas/AnalizadorSintacticoLl1.cs): `Analizar` y `ExpandirProduccion`. |
| Recorrido semántico | [`VerificadorTipos.cs`](../Automatas/VerificadorTipos.cs): `Verificar`. |
| Pilas semánticas y su recorrido | `VerificadorTipos.cs`: `EvaluarTipoExpresion` y `AplicarOperador`. |
| Reglas de tipos | `VerificadorTipos.cs`: `ObtenerTipoOperando`, `EvaluarOperacion`, `SonCompatiblesAsignacion` y `VerificarCondicion`. |
| Prioridades de operadores | `VerificadorTipos.cs`: `ObtenerPrecedencia`. |
| Información de variables y funciones | [`Simbolo.cs`](../Automatas/Simbolo.cs) y [`Funcion.cs`](../Automatas/Funcion.cs). |
