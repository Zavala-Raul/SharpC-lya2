# Guía breve de escritura para probar SharpC

## Antes de probar

Un programa completo tiene la forma `INI { ... }`. **Cada declaración o instrucción termina en `;`**. La clasificación de un lexema, la estructura de la instrucción y su significado son revisiones distintas:

| Etapa | Qué comprueba | Ejemplo |
| --- | --- | --- |
| Léxica | Reconoce palabras, identificadores, números, comillas y símbolos. | `LEER` aparece como token `PR4`. |
| Sintáctica | Comprueba si los tokens forman una instrucción permitida. | `LEER(vNombre);` sí, `LEER(5);` no. |
| Semántica | Comprueba si los nombres existen y los tipos/valores tienen sentido. | `LEER(vNombre);` requiere declarar `vNombre` antes. |

Por eso un error con el mensaje **«token inesperado»** puede ser *sintáctico* aunque el léxico haya reconocido correctamente la palabra.

## Formas recomendadas

- **Variables:** usa nombres como `vEdad`, `vNombre` o `vTotal2`: letra `v` seguida de letras y/o dígitos. **Funciones:** `fSumar`, `fDos`. No uses espacios, guion medio ni guion bajo dentro del nombre. Usa las palabras reservadas como se muestran aquí (con mayúsculas); no renombres una variable como una palabra reservada.
- **Tipos:** `ENT`, `DEC`, `TXT`, `BOOL` y `CAR` se usan en declaraciones. `VAC` se usa como tipo de retorno de función, no como tipo de variable.
- **Valores:** enteros como `2` o `-3`; decimales como `3.50`; texto entre comillas dobles (`"hola"`); un carácter entre comillas simples (`'A'`); booleanos `VDD` o `FALS`. Para decimales escribe **punto**, no coma. Los literales `ENT` deben estar entre `-2147483648` y `2147483647`.
- **Operadores:** `+`, `-`, `*`, `/`, `^`; comparaciones `==`, `<>`, `<`, `>`, `<=`, `>=`; lógica `&`, `|`, `!`. Usa `=` para asignar y `==` para comparar.
- **Bloques y separadores:** `(` y `)`, `{` y `}`, `;` para terminar instrucciones y `,` entre argumentos. Los paréntesis cambian el orden de agrupación de una expresión.
- **Comentarios:** `//` inicia un comentario hasta el final de la línea. Para la primera prueba, conviene copiar los ejemplos sin añadir comentarios.

La escritura recomendada coincide con los casos comprobados por la [batería de pruebas](pruebas-analizadores.md). La interfaz usa además un AFD cargado desde SQLite: su aceptación de variantes no comprobadas (por ejemplo, otras grafías de números o letras especiales) depende de la base configurada. Esta guía no atribuye un mensaje léxico exacto a cada carácter que aún no se ha probado contra ese AFD.

## LEER e IMP

### Palabras verificadas contra ACT 1, ACT 3 y el AFD real

| Categoría | Escritura | Función |
| --- | --- | --- |
| PR6 | `LIMP();` | Limpiar consola. |
| PR7 | `FONCOL("FFFFFF");` | Color de fondo hexadecimal. |
| PR8 | `TXTCOL("000000");` | Color del texto hexadecimal. |
| PR9 | `SLTLN(2);` | Cantidad entera de saltos de línea. |
| PR10 | `POS(40,5);` | Posición del cursor. |
| PR29 | `INTER(ENT,vUno,vDos);` | Intercambiar dos variables inicializadas del tipo indicado. |
| PR30 | `TECL();` | Esperar una tecla. |
| PR31 | `IMPREP("=",10);` | Repetir la impresión de una cadena. |

PR20, PR21 y PR22 se escriben `VDD`, `FALS` y `NUL`. El AFD rechaza `FLS`. PR29 no define arreglos. Las referencias y discrepancias restantes con los documentos están en el [contrato de código intermedio](plan-codigo-intermedio.md).

El token de la captura es `LEER` (`PR4`). Para leer hacia una variable:

```text
INI {
    TXT vNombre;
    LEER(vNombre);
    IMP(vNombre);
}
```

`LEER(vNombre);` es una **instrucción**: el nombre debe ser una variable declarada. El análisis la trata como una escritura, por lo que un uso posterior no da error de inicialización. También existe la forma de asignación `vNombre = LEER();`, que mantiene su sintaxis previa. Ninguna de las dos lee datos reales en esta aplicación: aquí se analizan instrucciones, no se ejecutan.

`IMP(expresión);` recibe una expresión; si contiene una variable, esta debe existir y estar inicializada antes de usarla. Por ejemplo, `IMP(vEdad);` necesita una declaración y asignación previa de `vEdad`.

## Si aparece un error

1. **Panel superior de errores:** revisa si se rechazó algún lexema. Si el token aparece clasificado en el «Archivo de Tokens» (por ejemplo, `PR4`), el fallo posterior puede ser sintáctico o semántico.
2. **Tabla inferior de errores:** busca la línea y el mensaje. «Token inesperado» o «falta» indican un problema de estructura; «no ha sido declarada» señala un nombre que no existe o no es visible; «sin inicializar» significa que existe, pero aún no recibió valor.
3. Verifica `INI { ... }`, llaves, paréntesis y `;`, y después comprueba que todas las variables usadas estén declaradas y que sus tipos sean compatibles.

Para pegar programas completos y conocer el resultado esperado, utiliza [Ejemplos de prueba](ejemplos-prueba.md). La [Cobertura de errores 1.6 y 1.7](pruebas-analizadores.md#cobertura-de-los-temas-16-y-17) explica qué comprueba el análisis semántico.
