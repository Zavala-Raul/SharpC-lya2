# Código intermedio: implementación y contrato

Para comprender el recorrido a distintos niveles, consulta [El flujo explicado de tres formas](flujo-codigo-intermedio-tres-perspectivas.md): explicación sencilla, defensa de métodos ante el profesor y guía para continuar el desarrollo.

## Estado actual

La generación cubre las instrucciones de la gramática actual de SharpC. La pestaña «Código intermedio» ofrece **Código de tres direcciones, Cuádruplos, Triplos, Código P y Notaciones**. No ejecuta el programa, no solicita entrada ni abre ventanas gráficas del lenguaje analizado.

- Tipos `ENT`, `DEC`, `TXT`, `CAR`, `BOOL`; `VAC` para retornos.
- Declaraciones, inicializadores, asignaciones y concatenación.
- Expresiones aritméticas, relacionales y lógicas en los contextos permitidos por la gramática.
- `SI`/`SINO`, `ENCASO`/`DFCT`, `MIENT`, `REPT`/`HASTA`, `POR` y `ROMPER`.
- Funciones, parámetros, llamadas adelantadas/anidadas/recursivas, retornos y referencias a ámbitos exteriores.
- `LEER(variable);`, `variable = LEER();`, `IMP`, `LIMP`, `FONCOL`, `TXTCOL`, `SLTLN`, `POS`, `INTER`, `TECL` e `IMPREP`.

La matriz SQLite y la gramática conservan sus reglas: añadir un generador no habilita nuevas formas de escribir el lenguaje. Una expresión lógica válida como condición no pasa automáticamente a ser una expresión válida en cualquier asignación. Las palabras nativas oficiales son `VDD`, `FALS` y `NUL`. ACT 1 describe NUL como ausencia de valor; su política de compatibilidad con los tipos no se especifica completamente y el verificador aún lo agrupa con BOOL. Esa discrepancia semántica sigue pendiente: reconocer el nombre correcto no equivale a implementar nulabilidad.

Las referencias del lenguaje son [ACT 1](ACT%201.%20definicion%20y%20caracteristicas%20%20ene-jun%202026-7.docx) (significado/sintaxis) y [ACT 3](ACT%203.%20DEFINICIONES%20REGULARES,%20COMPLEMENTO%20DE%20LA%20ACT%202%20ene-jun%202026.docx) (categorías y lexemas). Se corrigieron los nombres que el código atribuía erróneamente a PR6–PR10 y PR29–PR31. No existe una instrucción de arreglos en estos documentos: PR29 es intercambio.

## Relación con el material de clase

Los cuádruplos son una representación completa de código intermedio, no una etapa previa al «verdadero» código intermedio. Son la base común para obtener traducciones equivalentes a Triplos y Código P según el [material de clase](s_ntesis_de_c_digo_intermedio_y_representaciones.md). Las notaciones se obtienen de los árboles originales, no de las operaciones ya fragmentadas en temporales.

Se conservan las columnas de clase y la posibilidad de usar un temporal acumulador. Las operaciones de salto, llamadas y servicios incorporados que se especifican aquí son la extensión del proyecto para representar las construcciones que no aparecen en los ejemplos aritméticos de clase.

## Datos y metadatos

### Vista como código de tres direcciones

Dentro de «Código intermedio», **Código de tres direcciones** (vista inicial) y **Cuádruplos** muestran directamente la misma lista. Se añaden las vistas Triplos, Código P y Notaciones. Los cuadros de texto son de solo lectura, permiten seleccionar/copiar y tienen desplazamiento horizontal y vertical. Editar la fuente limpia todas las vistas.

Por ejemplo, `ENT vA = 2; ENT vR = vA + 3 * 4;` se muestra como:

```text
vA = 2
t1 = 3 * 4
vR = vA + t1
```

Una etiqueta se muestra como `L1:`, un salto como `goto L1`, una condición como `ifFalse t1 goto L2`, una llamada como `t2 = call fSumar, 2` y un retorno como `return t2`. `arg índice, valor` prepara argumentos; `param índice: tipo nombre` identifica parámetros formales. Se usan `&` y `|` en texto para conservar visible la evaluación sin cortocircuito del lenguaje. Las promociones al destino se anotan como comentario.

Esto **sí es código intermedio**, en notación textual de tres direcciones con extensiones de control de flujo y servicios. No es C#, ensamblador ni código SharpC para volver a pegar en el editor fuente. Es una vista legible, no un formato de serialización completo de tipos y símbolos para un backend. `FormateadorCodigoIntermedio` se encarga únicamente de esta presentación y comparte con la tabla los nombres desambiguados.

Los cuatro campos son `Destino`, `Fuente1`, `Fuente2` y `Operacion`. Un campo ausente se almacena como `null` y se muestra como `—`. No se inventan variables para rellenar campos.

`OperandoIntermedio` distingue literales, símbolos, temporales y etiquetas. Un símbolo conserva su objeto `Simbolo`, con ID, tipo, ámbito y región. Dos nombres iguales de ámbitos diferentes no se confunden. Si aparecen homónimos en la salida, la interfaz muestra `nombre#Numero`.

Cada `Cuadruplo` conserva además:

- `TipoResultado`, anterior a convertir al destino.
- Posición de token y línea fuente.
- `FuncionContenedora`, o `null` para el principal. Identifica el marco en que se interpretan sus temporales y parámetros.

Estos metadatos no agregan operandos al formato de cuatro campos. El cursor sobre una celda muestra línea, tipos y función contenedora.

## Conjunto de instrucciones

| Operación | Destino | Fuente 1 | Fuente 2 | Significado |
| --- | --- | --- | --- | --- |
| `=` | Variable/temporal | Valor | — | Copiar/promover al destino. |
| `+`, `-`, `*`, `/`, `^` | Variable/temporal | Izquierdo | Derecho | Operación aritmética tipada; `+` también concatena TXT. |
| `==`, `<>`, `<`, `>`, `<=`, `>=` | Temporal BOOL | Izquierdo | Derecho | Comparación. |
| `!` | Temporal BOOL | Operando | — | Negación. |
| `&&`, `\|\|` | Temporal BOOL | Izquierdo | Derecho | Operadores fuente `&`, `\|`; evaluación de ambos operandos. |
| `ETIQUETA` | Etiqueta | — | — | Punto de destino de saltos. |
| `SALTO` | Etiqueta | — | — | Transferencia incondicional. |
| `SALTO_FALSO` | Etiqueta | Condición BOOL | — | Saltar si la condición es falsa. |
| `FUNCION` | Símbolo de función | Función padre o — | — | Entrada de cuerpo y vínculo léxico. |
| `PARAMETRO` | Símbolo de parámetro | Índice desde 0 | — | Vincular argumento recibido al parámetro formal. |
| `ARGUMENTO` | Índice desde 0 | Valor capturado | — | Preparar argumento para la próxima llamada de esta activación. |
| `LLAMAR` | Resultado o — si VAC | Símbolo de función | Cantidad de argumentos | Invocar la función y obtener su retorno. |
| `RETORNO` | — | Valor o — si VAC | — | Terminar la activación y devolver al llamador. |
| `FIN_FUNCION` | Símbolo de función | — | — | Delimitador del cuerpo. |
| `LEER` | Variable | — | — | Entrada del tipo del destino. Ambas formas de LEER producen esta operación. |
| `IMP` | — | Valor | — | Salida de valor. |
| `LIMP` | — | — | — | Limpiar consola y reposicionar el cursor al origen. |
| `FONCOL`, `TXTCOL` | — | Literal TXT hexadecimal | — | Color del fondo o texto, respectivamente. |
| `SLTLN` | — | Cantidad ENT | — | Emitir saltos de línea. |
| `POS` | — | Coordenada X | Coordenada Y | Posicionar cursor. |
| `TECL` | — | — | — | Esperar una tecla. |
| `IMPREP` | — | Literal TXT | Cantidad ENT | Imprimir el texto la cantidad de veces indicada. |

Las incorporadas son instrucciones abstractas de servicios: se conserva la operación y su firma gramatical, pero todavía no hay un backend que implemente sus efectos. La semántica comprueba colores hexadecimales y cantidades ENT en SLTLN/IMPREP; los documentos no fijan una longitud hexadecimal ni una política para cantidades negativas, por lo que no se inventan esas restricciones.

`INTER(TIPO, variable1, variable2)` se baja a tres copias, no a un servicio de arreglos:

```text
t1 = variable1
variable1 = variable2
variable2 = t1
```

La semántica exige que ambas variables estén declaradas, inicializadas y coincidan exactamente con el tipo explícito. No se permite promoción ENT/DEC en un intercambio. Usar la misma variable en ambos argumentos conserva su valor.

## Reglas de evaluación

1. Operandos y argumentos se evalúan de izquierda a derecha. `&` y `|` son **estrictos**, sin cortocircuito, coherentes con la comprobación actual de inicialización de ambos operandos. Se emiten como `&&` y `||` por la normalización ya utilizada en el verificador, pero esos nombres no implican la semántica de C#.
2. La potencia asocia a la derecha; suma, resta, producto y división a la izquierda. La negación tiene menos prioridad que comparación: `!2 < 3` equivale a `!(2 < 3)`.
3. Los temporales empiezan en `t1` por generación. Se reutiliza el izquierdo consumido solo si su tipo no cambia. No se escriben variables del usuario como espacio de trabajo.
4. Antes de una llamada que pueda modificar variables, se captura en un temporal el operando izquierdo que debe conservar su valor previo. En `vA + fCambiar()`, se lee `vA` antes de llamar.
5. Los argumentos se evalúan y capturan **antes de emitir el grupo ARGUMENTO** de la llamada. Así, `fCombinar(vA, fCambiar())` conserva el primer valor y las llamadas anidadas no consumen argumentos de la exterior.
6. Los tipos controlan promoción, concatenación y división. `DEC vR = 5 / 2;` produce una división de resultado `ENT` y destino `DEC`: un backend deberá dividir como ENT y después promover. Lo mismo se aplica a parámetros y retornos: el tipo formal procede de la firma del símbolo. No se emiten conversiones explícitas ni se pliegan constantes.
7. Las declaraciones sin inicializador no producen asignación ni inicialización implícita. Su información de almacenamiento está en la tabla semántica.

## Traducción del control de flujo

- **SI/SINO:** salto condicional hacia SINO o final, y salto incondicional tras la rama verdadera para no ejecutar la falsa.
- **MIENT:** etiqueta anterior a la evaluación de la condición; salto al final si es falsa; cuerpo y salto al inicio. Las llamadas dentro de la condición se repiten en cada iteración.
- **REPT/HASTA:** etiqueta antes del cuerpo; al final, saltar atrás mientras la condición sea falsa.
- **POR:** inicialización una vez; etiqueta, condición, cuerpo, incremento y salto al inicio, en ese orden.
- **ENCASO:** capturar el selector una vez, comparar cada caso y saltar al siguiente si no coincide. Cada rama termina saltando al final; DFCT no lleva comparación. No hay caída a la rama siguiente.
- **ROMPER:** salto al final del ciclo/caso más cercano dentro de la misma función. Fuera de un contenedor válido se informa un diagnóstico y se descartan todos los cuádruplos.

Las etiquetas `L1`, `L2`, … son únicas por programa y se reinician al regenerar. Ningún salto ordinario cruza de una activación de función a otra.

## Funciones, ámbitos y recursión

El cuerpo de una función se emite en su posición fuente, protegido por un `SALTO` que lo omite al ejecutar secuencialmente su declaración. `LLAMAR` referencia el símbolo, no la posición textual ni solamente el nombre; por eso admite llamadas adelantadas y funciones homónimas.

Contrato para un futuro ejecutor/backend:

1. Indexar todas las entradas `FUNCION` por identidad de símbolo antes de ejecutar el principal.
2. Crear una activación por llamada, con parámetros, locales, temporales y dirección de retorno propios. Aunque `t1` tenga nombre estático único, cada llamada recursiva necesita sus propios valores.
3. El enlace léxico de una función apunta a la activación vigente de su función padre indicada por `FUNCION.Fuente1`; `—` indica el principal. Esto no es necesariamente el llamador inmediato.
4. Resolver una referencia no local mediante su símbolo/ámbito/región y ese enlace léxico. Nunca buscar únicamente por el texto del identificador.
5. `RETORNO` termina la activación inmediatamente. `VAC` tiene retorno implícito al final. Para otros tipos, el análisis semántico exige retorno garantizado antes de generar; el IR no contiene una instrucción de validación `FALTA_RETORNO` ni inventa un valor cero.

`VerificadorTipos.Verificar` ejecuta `AnalizadorRetornos` y produce `ERROR DE RETORNO` en la declaración de una función no VAC si un camino puede alcanzar su final. SI/SINO combina ambas ramas; ENCASO incluye todos los casos y DFCT; MIENT/POR no garantizan ejecutar el cuerpo; REPT sí ejecuta una vez, pero ROMPER puede permitir salir sin retornar. Se respeta el alcance de cada función y no se cuentan retornos inalcanzables tras REGR/ROMPER. No se evalúan constantes ni se intenta demostrar terminación de ciclos. Las capturas no escapan como objetos de función: la gramática tiene llamadas por nombre, no funciones de primera clase.

## Flujo en las clases

```text
Form1.btnEjecutar_Click
  → léxico y LL(1)
  → Form1.EjecutarVerificacionTipos
      → VerificadorTipos.Verificar
      → GeneradorCodigoIntermedio.Generar
          → validar precondiciones y LL(1)
          → crear Sesion (tokens, cierres, contadores, función actual)
          → Bloque
              → Asignar / Seleccion / Funcion / Incorporada
              → ExpresionRango → Parsear
              → Emitir / Capturar / Agregar
           → ResultadoCodigoIntermedio
       → Form1.MostrarCodigoIntermedio
           → ConstructorRepresentaciones.Construir
               → GeneradorTriplos.Generar
               → GeneradorCodigoP.Generar
               → FormateadorNotaciones.Generar
           → preparar textos y llenar las cinco vistas
           → dgvErroresGeneracion si hay diagnósticos
```

`Parsear` usa precedencia compartida de `OperadoresExpresion` y nodos de literal, referencia, operador unario/binario y llamada. Los tipos se obtienen mediante el verificador. `Bloque` recorre rangos de tokens con delimitadores emparejados, no líneas de texto.

Cada sesión es independiente. Si una comprobación de generación falla (por ejemplo, ROMPER sin contenedor, un caso incompatible con su selector o una llamada VAC utilizada donde hace falta un valor), el resultado contiene diagnósticos y **cero instrucciones**, aunque ya hubiera instrucciones preparadas internamente. Las comprobaciones del lenguaje existentes siguen bloqueando generación antes de esta fase.

## Triplos: traducción equivalente de acumulador

Se usa la variante de clase: **Dato objeto, Dato fuente, Operador**. Una celda de operador vacía, presentada como `—`, carga inicialmente el acumulador. Para `ENT vR = 2 + 3 * 4;`:

| Dato objeto | Dato fuente | Operador |
| --- | --- | --- |
| r1 | 3 | — |
| r1 | 4 | * |
| t1 | r1 | = |
| r2 | 2 | — |
| r2 | t1 | + |
| vR | r2 | = |

Es equivalente, aunque tenga más registros que una traducción compacta directa del árbol. `t1` pertenece al IR común; `r1`, `r2`, … son acumuladores exclusivos de esta conversión. Se reinician en cada generación y no ocupan posiciones en la memoria simbólica del usuario.

Cada operación binaria/unaria usa un acumulador independiente y copia al destino al terminar: esto protege `vA = 3 - vA`, donde inicializar directamente el destino destruiría el valor derecho. El acumulador puede cambiar de tipo efectivo entre registros (p.ej. carga ENT y después comparación BOOL); `Triplo.TipoResultado` conserva el tipo tras cada paso. Una copia final al símbolo realiza la promoción permitida. El tipo interno `ACUMULADOR` no es un tipo declarable del lenguaje.

- `!` carga el operando y aplica un registro sin fuente; el acumulador recibe su negación.
- Etiquetas, saltos, parámetros, argumentos, retornos y servicios de un argumento conservan su operación y los dos campos necesarios.
- `POS` e `IMPREP` usan objeto y fuente como primer y segundo argumento, no como una asignación.
- Una llamada requiere tres datos (resultado, función, aridad). Se descompone en `(función, aridad, PREPARAR_LLAMADA)` seguido de `(resultado o —, función, LLAMAR)`. No se pierde la aridad ni se ocultan argumentos dentro de un texto.
- Los registros conservan línea, tipo y función contenedora mediante sus metadatos de origen. Las activaciones de funciones mantienen acumuladores propios, igual que los temporales de los cuádruplos.

## Código P: traducción equivalente basada en pila

`GeneradorCodigoP` traduce los cuádruplos en orden, sin compactación ni ejecución. El mismo ejemplo produce (se omiten aquí los comentarios de tipo que muestra la interfaz):

```text
load 3
load 4
mul
store t1
load 2
load t1
add
store vR
```

En operaciones binarias se extrae primero el derecho y después el izquierdo; `sub` significa `izquierdo - derecho`, y `div`, `izquierdo / derecho`. `TipoResultado` distingue división ENT de DEC y concatenación TXT de suma numérica. `store` convierte al tipo del destino cuando corresponde. No se reasocian operaciones ni se altera el orden de efectos.

| Instrucciones P | Efecto sobre la pila de evaluación |
| --- | --- |
| `load valor` | Apilar un valor (+1). |
| `store destino` | Extraer el resultado y asignar (-1). |
| `add sub mul div pow eq ne lt gt le ge and or` | Extraer dos operandos y apilar un resultado (-1 neto). |
| `not` | Extraer uno y apilar su negación (0 neto). |
| `L1:` / `jmp L1` | Etiqueta / salto incondicional (0). |
| `jfalse L1` | Extraer condición y saltar si es falsa (-1). |
| `func función[, padre]`, `endfunc función` | Entradas y límites de funciones (0); padre ausente significa INI. |
| `param variable, índice` | Vincular un argumento formal (0). |
| `arg índice` | Extraer un valor y guardarlo en los argumentos pendientes de la activación (-1). |
| `call función, cantidad` | Consumir ese grupo de argumentos pendientes y apilar retorno si no es VAC (+1 o 0). |
| `retv` / `ret` | Extraer retorno (-1) / retornar sin valor (0). |
| `read` / `print` | Leer y apilar (+1) / extraer e imprimir (-1). |
| `limp`, `tecl` | Servicios sin operandos de pila (0). |
| `foncol`, `txtcol`, `sltln` | Extraer un argumento (-1). |
| `pos`, `imprep` | Extraer primero el segundo argumento y luego el primero (-2). |

Los argumentos pendientes se mantienen separados de la pila de evaluación. Los cuádruplos ya evalúan/capturan argumentos anidados antes de preparar la llamada exterior. Cada activación necesita su pila, parámetros, temporales y enlace léxico; no se confunde la recursión con un almacenamiento global de temporales.

La conversión comprueba altura no negativa y **pila vacía al terminar cada cuádruplo**. Así las entradas de etiquetas/funciones tienen altura cero y las ramas no dependen de residuos de otra expresión. Las operaciones P conservan los tipos y el contexto fuente; los comentarios de tipo visibles son parte de la explicación, no instrucciones adicionales.

## Notaciones: tres columnas juntas

La tabla «Notaciones» muestra línea/contexto y **Infija, Prefija y Postfija** de cada expresión fuente completa. No muestra solamente las expresiones fragmentadas en temporales.

| Infija | Prefija | Postfija |
| --- | --- | --- |
| `vR = (2 + (3 * 4))` | `= vR + 2 * 3 4` | `vR 2 3 4 * + =` |
| `vR = (2 ^ (3 ^ 2))` | `= vR ^ 2 ^ 3 2` | `vR 2 3 2 ^ ^ =` |

`ExpresionIntermedia` conserva el árbol creado por `Parsear`; `ExpresionFuente` añade destino, contexto, línea, posición y función. Las expresiones se registran una vez en sus contextos principales: asignaciones, condiciones, retornos, llamadas, salidas, casos e incorporadas. No se duplican filas por cada paréntesis o argumento interno.

`FormateadorNotaciones` recorre ese árbol:

- Infija completamente parentizada para mostrar exactamente la agrupación, sin depender de las reglas de otro lenguaje.
- Prefija en preorden; postfija en postorden.
- Llamadas con aridad explícita: `call[fSuma/2]`. Por ejemplo `fSuma(1, fUno())` se muestra como `call[fSuma/2] 1 call[fUno/0]` y `1 call[fUno/0] call[fSuma/2]`.
- Cadenas entre comillas conservadas como operandos únicos y referencias homónimas con su ID visible compartido con las demás vistas.
- `LEER(vA)` y `vA = LEER()` normalizados como asignación de `LEER()` al mismo destino; INTER se conserva como llamada en esta vista fuente aunque se baje a tres copias en las representaciones de instrucciones.

Cambiar de notación no cambia el orden efectivo de las operaciones ni vuelve a analizar prefija/postfija como otro lenguaje de entrada. Las instrucciones de control de flujo son representaciones de programa; no se transforman en supuestas «etiquetas postfijas».

## Errores y publicación conjunta

`ConstructorRepresentaciones.Construir` prepara Triplos, Código P y Notaciones antes de llenar controles. Los errores del análisis siguen bloqueando generación. Los errores de generación o conversión producen diagnóstico con línea/representación y **ninguna salida parcial**. Editar o analizar de nuevo limpia las cinco vistas.

`dgvErroresGeneracion`, anteriormente oculto permanentemente, ahora se muestra cuando hay diagnósticos y se oculta al limpiar. Los errores léxicos/sintácticos/semánticos se siguen consultando en «Análisis». Un error de conversión no se presenta como un error de tipos del usuario.

## Ejemplo para probar en el formulario

```text
INI {
    ENT vTotal = 0;
    TXT vTitulo = "Total: ";
    CAR vMarca = 'A';
    BOOL vMostrar = VDD;
    FUNC DEC fDoble(ENT vDato) {
        REGR vDato * 2;
    }
    POR (ENT vI = 0; vI < 3; vI = vI + 1) {
        vTotal = vTotal + vI;
    }
    DEC vResultado = fDoble(vTotal);
    SI (vMostrar & vResultado >= 0) {
        IMP(vTitulo + vResultado);
        IMP(vMarca);
    }
    SINO { IMP("Sin resultado"); }
}
```

Pulsa «Ejecutar» en «Análisis» o «Analizar y generar» en la segunda pestaña. Deben aparecer operaciones de asignación, función, parámetros, salto, comparación, llamada y salida. `IMP` queda representada como instrucción: no imprime el resultado al analizar.

## Verificación y continuidad

**317/317 casos correctos**, incluidos I01–I26, G01–G42, T01–T20 y U01–U18. Los casos ejecutables de G comparan también el comportamiento de Triplos y Código P mediante adaptadores de prueba que usan sus propios campos, conservando solo metadatos de origen. Se comprueban ciclos, recursión, funciones anidadas, captura de efectos y equivalencia de resultados. El oráculo y sus adaptadores no forman parte de la aplicación. U01–U18 verifican las tres notaciones, aridad, alias de operandos, promociones, servicios, balance de pila, reinicio, errores y desambiguación de símbolos.

Compilación Windows Forms .NET Framework 4.7.2, Debug/x64 correcta. Integración actual con AFD SQLite real: cinco pestañas interiores; un ejemplo aritmético produce 3 cuádruplos, 7 triplos y 2 filas de notaciones; función y ciclo aparecen también en Código P. Se comprobó espacio útil de cada vista a 900 × 600, limpieza conjunta al editar, diagnóstico visible de ROMPER sin contenedor y bloqueo de todas las vistas por error semántico. Las pruebas de consola usan un fixture léxico separado, no prueban todas las grafías del AFD.

Quedan como fases distintas: optimización, exportación/serialización completa de los metadatos, ejecución real y backend para una plataforma concreta. El rango de DEC, overflow aritmético y errores de ejecución mantienen las limitaciones documentadas del analizador. La generación cubre el lenguaje actual sin prometer ejecutar sus servicios de entrada/salida y gráficos.
