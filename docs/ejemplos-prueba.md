# Programas para probar SharpC manualmente

**Copia un bloque completo por vez** en «Programa Fuente» y pulsa **Ejecutar**. Los ejemplos no dependen entre sí. Las instrucciones `IMP` y `LEER` se **analizan**, pero esta aplicación todavía no ejecuta la impresión ni solicita datos por teclado. Consulta también la [guía de escritura](reglas-lexicas.md).

## Cuádruplos: prueba de la nueva pestaña

Copia este programa, pulsa «Ejecutar» y abre «Código intermedio». También puedes pulsar «Analizar y generar» desde esa pestaña.

```text
INI {
    ENT vA = 2;
    ENT vResultado;
    vResultado = vA + 3 * 4;
}
```

En «Cuádruplos», salida esperada:

| Dato objeto | Dato fuente 1 | Dato fuente 2 | Operación |
| --- | --- | --- | --- |
| vA | 2 | — | = |
| t1 | 3 | 4 | * |
| vResultado | vA | t1 | + |

También puedes abrir «Triplos», «Código P» y «Notaciones». Esta última muestra juntas infija, prefija y postfija; para la asignación final se ve:

| Infija | Prefija | Postfija |
| --- | --- | --- |
| `vResultado = (vA + (3 * 4))` | `= vResultado + vA * 3 4` | `vResultado vA 3 4 * + =` |

No se calcula el resultado. Una declaración sin inicializador no genera fila. Editar el código limpia todas las vistas hasta analizar de nuevo. La tabla inferior de la segunda pestaña se muestra cuando hay diagnósticos de generación, por ejemplo `ROMPER` sin un contenedor válido.

Los ejemplos válidos siguientes también se traducen a cuádruplos, incluidos `IMP`, `LEER`, tipos no numéricos, funciones y control de flujo. Si hay errores no se publica código parcial. Véase el [contrato y ejemplo completo](plan-codigo-intermedio.md).

## Programas sin errores de análisis esperados

### 1. Declaraciones y expresiones

```text
INI {
    ENT vCantidad = 2;
    DEC vPrecio = 3.50;
    DEC vTotal = vCantidad * vPrecio;
    BOOL vActivo = VDD;
    TXT vMensaje = "Total: " + vTotal;
    IMP(vMensaje);
}
```

**Esperado:** sin errores. `vTotal` y `vMensaje` figuran en la tabla de símbolos. Su columna «Valor» conserva el texto de la expresión, no su resultado numérico.

### 2. LEER sobre una variable declarada (caso de la captura)

```text
INI {
    ENT vEdad = 18;
    ENT vTotal = 2;
    TXT vNombre;
    IMP("Hola mundo");
    IMP(vEdad);
    IMP("Total: " + vTotal);
    LEER(vNombre);
    IMP(vNombre);
}
```

**Esperado:** sin errores. En el listado de tokens, `LEER` se reconoce como `PR4`. `LEER(vNombre);` se acepta como instrucción y se considera una asignación para comprobar que el `IMP(vNombre)` posterior no lea una variable sin inicializar. **No se solicita ningún valor real** durante el análisis.

### 3. Condición y ciclo

```text
INI {
    ENT vContador = 0;
    SI (vContador < 3) {
        vContador = vContador + 1;
    } SINO {
        vContador = 2;
    }
    POR (ENT vI = 0; vI < 3; vI = vI + 1) {
        IMP(vI);
    }
}
```

**Esperado:** sin errores. `vI` pertenece al ámbito del `POR`; `vContador` pertenece a `INI`.

### 4. Función y parámetro

```text
INI {
    FUNC DEC fAgregar(DEC vImporte) {
        REGR vImporte + 1;
    }
    DEC vResultado = fAgregar(2);
    IMP(vResultado);
}
```

**Esperado:** sin errores. Se acepta la promoción `ENT → DEC` al pasar `2` al parámetro decimal. El marco simbólico de la función y su parámetro aparecen en las tablas; no se ejecuta la llamada.

## Programas con un error esperado

### 5. Falta de punto y coma: error sintáctico

```text
INI {
    ENT vA = 1
}
```

**Esperado:** error sintáctico por omitir `;`. Como no se acepta la sintaxis, no se calculan tablas semánticas para este programa.

### 6. Tipo incompatible: error semántico

```text
INI {
    ENT vA = "hola";
}
```

**Esperado:** error de tipo: no se puede asignar `TXT` a `ENT`.

### 7. LEER requiere una variable declarada

```text
INI {
    LEER(vNombre);
}
```

**Esperado:** error por variable no declarada en la línea 2. El token `LEER` **sí** es reconocido; el problema es `vNombre`. Para corregirlo, añade `TXT vNombre;` antes de la lectura.

### 8. Leer una variable antes de inicializarla

```text
INI {
    TXT vNombre;
    IMP(vNombre);
    LEER(vNombre);
}
```

**Esperado:** error de inicialización en la línea 3. La lectura de la línea 4 no hace válida una impresión anterior.

### 9. LEER necesita un identificador de variable

```text
INI {
    LEER(5);
}
```

**Esperado:** error sintáctico. La forma admitida es `LEER(vNombre);` con una variable como destino; un número no puede recibir la lectura.

### 10. Uso fuera de ámbito

```text
INI {
    SI (VDD) {
        ENT vTemporal = 1;
    }
    IMP(vTemporal);
}
```

**Esperado:** error por variable no visible fuera del bloque `SI`.

### 11. Literal entero fuera de rango

```text
INI {
    ENT vA = 2147483648;
}
```

**Esperado:** error de tipo/rango de `ENT` (valor máximo permitido: `2147483647`).

## Dónde mirar el resultado

- **Archivo de Tokens:** categorías como `PR4` para `LEER`, `IDV` para un identificador y `CE8` para `;`.
- **Errores (panel superior):** resultados del análisis léxico.
- **Tabla de errores inferior:** diagnósticos sintácticos y semánticos, con línea.
- **Tabla de símbolos y tabla de funciones:** declaraciones y direcciones simbólicas cuando la sintaxis es válida.

Estos fragmentos son casos manuales de comprobación, no ejecuciones del programa. La batería automatizada con clasificación de prueba separada se documenta en [Pruebas de los analizadores](pruebas-analizadores.md).
