# Cobertura de errores de los temas 1.6 y 1.7

Contraste con [`LyA2_1.6&1.7_Documento.pdf`](LyA2_1.6%261.7_Documento.pdf). Las páginas 1–5 tratan símbolos, ámbitos y direcciones; las páginas 6–7 enumeran errores semánticos y estrategias de tratamiento. El ejemplo del PDF está escrito en C#; los ejemplos de esta guía usan **la gramática del proyecto** (`INI`, `FUNC`, `ENT`, `REGR`, etc.).

## Tema 1.6: símbolos y direcciones

| Requisito del documento | Comprobación del proyecto | Evidencia |
| --- | --- | --- |
| Declaraciones y usos (págs. 1 y 3) | Se registra una declaración por ámbito; cada uso resuelve a un símbolo concreto. Un nombre inexistente o fuera de alcance da error. | B01, B05, B08, B14, B20–B23. |
| Redefinición (pág. 3) | Se rechazan duplicados del mismo ámbito y sombreado de sus antecesores. Los hermanos pueden reutilizar nombres. | B02–B04, B09, B29, B32–B33. |
| Nombre, tipo, alcance y dirección (págs. 1–3) | La tabla guarda clase, ámbito y línea. A las variables y parámetros válidos se les asigna región, tamaño y desplazamiento. | D01–D05, D07–D08; `dgvSimbolos`. |
| Reserva según tipo y direcciones únicas (págs. 1–3) | `CatalogoTipos` asigna tamaños; los intervalos `[offset, offset + tamaño)` no se solapan dentro de una región. Una función tiene marco independiente; se detecta desbordamiento del contador. | D01–D05, D11–D15; `dgvFunciones`. |
| Código para acceder a memoria real (págs. 3 y 5) | La dirección es **simbólica**: región + desplazamiento. No se emiten instrucciones de acceso ni se conoce la dirección real antes de ejecutar. | [Memoria simbólica](memoria-simbolica.md). |

## Tema 1.7: categorías de errores

| Categoría (pág. 6) | Comportamiento validado | Evidencia |
| --- | --- | --- |
| Inicialización | Se rechaza leer una variable sin asignación previa. Se comprueban ramas de `SI`/`SINO` y `ENCASO` por intersección; `MIENT` y el cuerpo de `POR` pueden no ejecutarse, mientras `REPT` ejecuta el cuerpo al menos una vez. Los parámetros comienzan inicializados. | E01–E13, E29, E32–E37; `ERROR DE INICIALIZACIÓN`. |
| Tipos de datos | Se comprueban operadores, asignaciones, condición `BOOL`, rango de literal `ENT`, cantidad y tipos de argumentos, y tipo de cada `REGR` (incluido `VAC`). Se permite `ENT → DEC`, pero no al revés. | M01–M23, N01–N24, E14–E28, E30–E31; `ERROR DE TIPO`. |
| Lógica | Se rechazan comparaciones y operadores con tipos incompatibles y condiciones no booleanas. No se puede inferir si el autor **quería** escribir otro operador o si un bucle terminará. | M07–M17, R02, R09–R18. |
| Alcance | Se detectan usos antes de declarar, fuera del bloque, desde otro caso o desde otra función. | B01–B38; `ERROR DE ÁMBITO` o `ERROR DE TIPO` cuando el nombre no existe. |
| Recursos | Se valida el espacio simbólico por región con suma `checked`; una reserva duplicada o que exceda `Int32.MaxValue` no modifica el marco. Los literales `ENT` fuera de rango se rechazan. | D07–D15, N05–N10; `ERROR DE MEMORIA` o `ERROR DE TIPO`. |

Una fuga de memoria, la terminación efectiva de un ciclo, un fallo de ejecución o un resultado aritmético desbordado **no pueden deducirse en general** de esta representación estática. Tampoco se valida aún el rango de `DEC` ni que **todos los caminos** de una función con retorno lleguen a `REGR`. Son comprobaciones distintas de validar cada `REGR` presente. El análisis de inicialización es conservador: una variable asignada únicamente en un bucle que podría no ejecutarse sigue considerándose no inicializada después; no se calculan constantes para descartar ramas imposibles y no se elimina código inalcanzable después de `REGR`. Si una asignación contiene ya un error de tipo o de nombre, el destino se considera asignado para evitar añadir diagnósticos de inicialización derivados de ese error previo.

## Tratamiento y presentación

Si el análisis léxico detecta errores, el programa no pasa a LL(1). Si LL(1) rechaza la sintaxis, no se construyen ámbitos ni se ejecuta la semántica. Con sintaxis válida se acumulan diagnósticos de ámbito, memoria, tipos e inicialización con la línea del token; un identificador sin resolver no produce además un error de inicialización para el mismo uso. `Form1` los presenta juntos en `dgvErroresSintaxis`, conserva los errores léxicos en `dgvErrores` y reconstruye tablas y errores en cada análisis. Es el enfoque **reportar y continuar** para los errores recuperables descrito en la página 7; asertos, reintentos, patrones de registro y monitorización son alternativas generales del documento, no características de este lenguaje.

Ejemplos para probar en el editor:

```text
INI {
    ENT vA;
    IMP(vA);                // ERROR DE INICIALIZACIÓN en línea 3.
    FUNC ENT fUno(ENT vB) { REGR vB; }
    ENT vC = fUno("texto"); // ERROR DE TIPO en línea 5 (argumento 1).
}
```

Los comentarios `//` son explicativos. Las referencias a archivos fuente y el resultado reproducible de la batería están en [Pruebas de los analizadores](pruebas-analizadores.md). Esta revisión añadió E01–E37 y actualizó R04 para reconocer **dos fallos independientes**: su destino de inicialización no declarado y la lectura posterior de otra variable que nunca recibió valor.
