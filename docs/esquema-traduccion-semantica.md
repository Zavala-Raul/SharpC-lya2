# Acciones semánticas asociadas a la gramática (tema 1.5)

El [material 1.5](1.5%20Esquema%20de%20Traduccion.pdf) presenta los esquemas de traducción como reglas gramaticales con atributos y acciones. En este proyecto la correspondencia es **conceptual**: `TablaSintacticaSharpC` define las producciones LL(1), `AnalizadorSintacticoLl1` las reconoce y, solo tras aceptar el programa, `TablaAmbitos` y `VerificadorTipos` recorren los tokens y calculan atributos. La tabla LL(1) no contiene código entre llaves y el analizador no construye un árbol sintáctico ni ejecuta acciones al expandir producciones.

Las llaves de un bloque `INI { ... }` son tokens del lenguaje (`CE3` y `CE4`); las acciones entre `{acción}` que se muestran a continuación son **anotaciones explicativas**, no sintaxis admitida por el programa.

## Correspondencia de reglas con acciones reales

| Producción relevante de `TablaSintacticaSharpC` | Acción conceptual tras validar la sintaxis | Método que la realiza |
| --- | --- | --- |
| `IN01 → PR1 CE3 INS CE4` | Crear el ámbito principal de `INI`. | `TablaAmbitos.Principal`, `TablaAmbitos.Recorrer`. |
| `IN02 → TIPO_VAR IDV ASIG_OPC CE8` | Registrar una variable en su ámbito y asociar tipo, línea, clase y posición de token; comprobar duplicados y sombreado. Si existe `ASIG EXP`, inferir el tipo y comprobar la asignación. | `TablaAmbitos.Declarar` y `RegistrarDeclaraciones`; `VerificadorTipos.VerificarAsignacion`. |
| `IN03 → IDV ASIG EXP_IO CE8` | Resolver el destino visible; inferir la expresión y, si los tipos son compatibles, guardar en `Valor` **su texto**, sin ocupar otro desplazamiento. | `TablaAmbitos.ResolverReferencias`; `VerificadorTipos.VerificarAsignacion`. |
| `IN19 → PR2 TIPO_RET IDF CE1 PARAM CE2 CE3 INS CE4` | Registrar la función en el ámbito contenedor y sus parámetros en el ámbito de función; abrir una región propia y calcular su tamaño de marco. | `TablaAmbitos.Recorrer`, `RegistrarDeclaraciones` y `CalcularDisposicion`. |
| `IN21 → PR3 EXP_OPC CE8` | Inferir el tipo de la expresión de `REGR` y compararlo con el retorno declarado de la función contenedora. | `VerificadorTipos.VerificarRetorno`; `TablaAmbitos.ObtenerFuncionContenedora`. |
| `CALL_FUNC → IDF CE1 ARG CE2` | Resolver el símbolo de la función, inferir argumentos y comparar su cantidad y tipos con la firma. | `VerificadorTipos.EvaluarTipoExpresion`; `TablaAmbitos.ObtenerFuncion`. |
| `IN08 → PR18 CE1 INIT_POR CE8 COND CE8 IDV ASIG EXP CE2 CE3 INS CE4` | Dar un ámbito compartido al encabezado y al cuerpo; verificar sus asignaciones y que `COND` resulte `BOOL`. | `TablaAmbitos.Recorrer`; `VerificadorTipos.VerificarBuclePor`. |
| `IN04`, `IN06`, `IN07` (condiciones y bloques) | Abrir ámbitos de bloque y verificar que cada condición inferida sea `BOOL`. En `REPT`, la condición `HASTA` comparte el ámbito del cuerpo. | `TablaAmbitos.Recorrer`; `VerificadorTipos.VerificarCondicion`. |
| `VALOR → IDV / CNU / CAD / CAR / literal booleano / CALL_FUNC / (EXP)` | Obtener el tipo del operando; resolver referencias por ámbito y posición; rechazar un literal `ENT` fuera de rango. | `TablaAmbitos.ResolverReferencias`; `VerificadorTipos.ObtenerTipoOperando`; `CatalogoTipos`. |
| `EXP → TERM EXP_P`, `TERM → POT TERM_P`, `POT → VALOR POT_P` | Combinar **tipos** según las reglas de cada operador. `EXP_P`, `TERM_P` y `POT_P` reconocen repeticiones; la potencia agrupa hacia la derecha. | `VerificadorTipos.EvaluarTipoExpresion`, `AplicarOperador` y `EvaluarOperacion`. |
| `COND → COND_OR` y niveles relacionados | Combinar comparaciones, negación y lógica para obtener `BOOL` o registrar una incompatibilidad. | `VerificadorTipos.VerificarCondicion` y `EvaluarOperacion`. |

## Atributos y fases

- **Declaración:** `Simbolo.Tipo`, `Clase`, `Ambito`, `LineaDeclaracion` y `PosicionDeclaracion` describen el símbolo; dos nombres iguales de ámbitos hermanos son dos declaraciones y tienen IDs diferentes.
- **Ubicación:** `CatalogoTipos.ObtenerTamanoBytes` determina `Simbolo.TamanoBytes`. `TablaAmbitos.CalcularDisposicion` asigna `Simbolo.Region` y `DesplazamientoBytes`. `Funcion.TamanoMarcoBytes` se obtiene de su región.
- **Expresión:** `pilaTipos` contiene tipos parciales (`ENT`, `DEC`, `ERROR`, etc.), no resultados aritméticos; `pilaOps` contiene operadores pendientes. Un identificador usa el tipo de `SimbolosPorToken` en la posición concreta de ese uso.
- **Diagnóstico:** `TablaAmbitos.Errores` informa alcance y memoria; `VerificadorTipos.Errores` añade literales fuera de rango e incompatibilidades sin cascadas tras `ERROR`.
- **Texto asignado:** `Simbolo.Valor` guarda, por ejemplo, `vA + 3.5`. No es un atributo `expr.valor` numérico evaluado como en algunos ejemplos del PDF.

Orden efectivo: **tokens → LL(1) → construcción de ámbitos y declaraciones → disposición de memoria → resolución de referencias → inferencia y validaciones de tipos → análisis de inicialización definida → tablas de Windows Forms**. Así, la correspondencia con el esquema de traducción no cambia el algoritmo predictivo ni obliga a introducir acciones en la pila sintáctica.

## Ejemplo reproducible

```text
INI {
    ENT vA = 2;
    DEC vB = vA + 3.5;
}
```

1. `IN02` admite ambas declaraciones. `TablaAmbitos` les da ámbito `INI`, tipos `ENT` y `DEC`, y líneas 2 y 3.
2. `CalcularDisposicion` coloca `vA` en `INI + 0` (4 bytes) y `vB` en `INI + 4` (8 bytes); el total de `INI` es 12 bytes simbólicos.
3. El uso de `vA` en la línea 3 se resuelve a la primera declaración. `vA + 3.5` combina `ENT + DEC → DEC`; `SonCompatiblesAsignacion("DEC", "DEC")` permite asignarlo a `vB`.
4. `dgvSimbolos` muestra cada declaración, su ámbito, clase, línea y ubicación. `Valor` conserva los textos `2` y `vA + 3.5`; el programa no calcula `5.5`.

El PDF también ilustra traducción de `2 + 3` a «2 plus 3» y generación de código de tres direcciones; esos ejemplos explican otras acciones posibles, no salidas de este proyecto. La verificación de firmas, `REGR` e inicialización definida se explica en [Cobertura de errores 1.6/1.7](cobertura-errores-1.6-1.7.md). Evaluar expresiones constantes y ejecutar el programa permanecen como ampliaciones posteriores.
