# Ámbitos y declaraciones: integración actual

## Flujo

```text
AFD / tokens → LL(1) sin errores → VerificadorTipos.Verificar
                                      ├─ TablaAmbitos
                                      │    ├─ árbol de ámbitos
                                      │    ├─ declaraciones y firmas
                                      │    └─ símbolo resuelto por posición de token
                                      └─ tipos de expresiones y asignaciones
```

`Form1` conserva un catálogo de identificadores léxicos para numerar la salida de tokens. Ese número identifica un **lexema**; `Simbolo.Numero` identifica una **declaración**. Son numeraciones independientes. Ver un identificador durante el léxico no declara una variable ni una función. Si falla el léxico o la sintaxis, no se construyen las tablas semánticas.

## Construcción desde tokens

`TablaAmbitos` se usa únicamente después de aceptar la sintaxis:

1. Empareja `CE1/CE2` y `CE3/CE4` mediante una pila. El texto de una cadena no se interpreta como delimitador.
2. Recorre rangos de tokens, crea ámbitos y recoge declaraciones. El formato de las líneas no determina los bloques.
3. Registra las declaraciones de los padres antes que las de sus hijos. Así, duplicados y sombreado se detectan también cuando un nombre del padre aparece más adelante.
4. Ordena los símbolos válidos por posición y les asigna IDs consecutivos. Dos declaraciones homónimas de hermanos conservan símbolos distintos.
5. Resuelve cada uso `IDV`/`IDF` en su ámbito y sus padres. `SimbolosPorToken` guarda la instancia exacta o `null` si hay error. Las declaraciones rechazadas no reemplazan la original.

La búsqueda usa `Simbolo.PosicionDeclaracion`, no solamente `LineaDeclaracion`. Por ejemplo:

```text
INI { vA = 1; ENT vA; }
```

Se rechaza el primer `vA`, aunque ambos estén en la línea 1. La línea se conserva para presentar el diagnóstico. La API original de `Ambito` basada en líneas sigue disponible para sus clientes directos; el análisis completo usa posiciones.

## Reglas de alcance

| Construcción | Ámbito |
| --- | --- |
| `INI { ... }` | Principal. |
| Cuerpo de `SI`, `SINO`, `MIENT` | Hijo del ámbito contenedor. Su condición usa el ámbito exterior. |
| `POR (inicialización; condición; incremento) { ... }` | Un mismo ámbito para encabezado y cuerpo. Una variable exterior reasignada conserva su ámbito original. |
| `REPT { ... } HASTA (...);` | Un mismo ámbito para cuerpo y condición; termina después del `;`. |
| `ENCASO (selector) { ... }` | El selector se resuelve fuera; cada caso, incluido `DFCT`, tiene un ámbito hijo propio. `ROMPER;` termina el caso según la gramática. |
| `FUNC tipo fNombre(parámetros) { ... }` | La función se declara en el ámbito contenedor; parámetros y cuerpo comparten un ámbito hijo. |
| Función anidada | Conserva el padre léxico, sin trasladarse al principal. Puede consultar declaraciones de sus antecesores respetando el orden de las variables. |

Reglas generales:

- Sin duplicados en un mismo ámbito ni sombreado de ningún antecesor, aunque el nombre del antecesor aparezca después.
- Los hermanos pueden reutilizar nombres y tipos diferentes.
- Las funciones pueden llamarse antes de aparecer, pero solo desde ámbitos donde son visibles. La recursión se acepta.
- Las variables y parámetros deben aparecer antes del uso en el texto. El cuerpo de una función no puede usar una variable principal declarada después, aunque una llamada futura pudiera ejecutarse después de su inicialización.
- La declaración se considera visible desde el token del nombre. El análisis posterior de inicialización definida **sí** rechaza `ENT vA = vA;` porque la expresión lee antes de asignar.

## Ejemplos

Declaraciones independientes y válidas:

```text
INI {
    SI (VDD) { ENT vDato = 1; vDato = 2; }
    SINO { TXT vDato = "hola"; vDato = "adiós"; }
}
```

Hay dos símbolos `vDato`, con distinto ID, ámbito, tipo y texto de valor. Las reasignaciones actualizan la instancia resuelta y no la de otro bloque.

Variable de ciclo fuera de alcance:

```text
INI {
    POR (ENT vI = 0; vI < 3; vI = vI + 1) { IMP(vI); }
    IMP(vI);
}
```

Solo el último uso produce `ERROR DE TIPO: La variable 'vI' no ha sido declarada.`

## Integración y límites

`VerificadorTipos` conserva el índice original de los fragmentos al extraer expresiones y partes de `POR`. No compara tokens por nombre y línea: dos apariciones iguales pueden resolver a símbolos diferentes. Guarda el destino de una asignación antes de analizar su expresión, para actualizar el símbolo correcto.

Todos los identificadores se comprueban en una pasada de resolución, incluidos argumentos, retornos e impresión. La inferencia posterior utiliza `ERROR` para referencias fallidas, sin emitir otra vez el mismo diagnóstico. Dos usos inválidos distintos sí producen dos diagnósticos, incluso en una misma línea.

Se comprueban las operaciones dentro de `IMP`, `REGR` y argumentos anidados. Las llamadas comprueban cantidad y tipos de argumentos; cada `REGR` presente se contrasta con el retorno de su función. Después, `AnalizadorInicializacion` rechaza lecturas sin valor definido. `Valor` conserva texto, no un valor ejecutado. La interfaz muestra las declaraciones reales y sus columnas de ámbito y memoria, con desplazamiento horizontal.

Las declaraciones válidas también reciben un [desplazamiento relativo en una región simbólica](memoria-simbolica.md). Los ámbitos de bloques controlan visibilidad, pero no crean una región independiente: comparten la de `INI` o la de la función contenedora.

## Verificación

- B01–B38 en `tests/Automatas.Pruebas/PruebasIntegracionAmbitos.cs`: análisis sintáctico real, construcción de tablas y verificación de tipos.
- Además de mensajes y líneas, se comprueban IDs, tipos, valores independientes, firmas multilínea y reinicio del análisis.
- Resultado conjunto actual, incluida la revisión de errores 1.6/1.7: **200 correctas, 0 fallidas**.
- Compilación Windows Forms, .NET Framework 4.7.2, Debug/x64: correcta. No se automatiza la interacción con la interfaz ni el AFD SQLite.
