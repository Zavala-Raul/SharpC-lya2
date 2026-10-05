# Memoria simbólica: regiones y desplazamientos

## Modelo implementado

Después de construir las declaraciones válidas y antes de resolver referencias, `TablaAmbitos.CalcularDisposicion` crea una región `INI` y una región independiente por función válida, incluida cada función anidada. Ordena las declaraciones por posición de token; las variables y los parámetros reciben `Simbolo.Region`, `TamanoBytes` y `DesplazamientoBytes`. Los nombres de función no ocupan un espacio de variable. El contador de bytes de cada región comienza en cero.

| Tipo | Tamaño simbólico |
| --- | ---: |
| `ENT` | 4 bytes |
| `DEC` | 8 bytes |
| `BOOL` | 1 byte |
| `CAR` | 2 bytes |
| `TXT` | 8 bytes (referencia; no incluye caracteres) |
| `VAC` | 0 bytes; únicamente tipo de retorno |

`CatalogoTipos.ObtenerTamanoBytes` es la fuente de estos tamaños. Un tipo desconocido o un tamaño cero no obtiene una posición de variable; el calculador informa `ERROR DE MEMORIA` si encontrase una declaración así. El parser normalmente ya impide declarar variables `VAC`.

`RegionMemoria.Reservar` usa `checked` antes de asignar el siguiente desplazamiento. Si el total superase `Int32.MaxValue`, lanza `OverflowException` y conserva intactos tanto la región como el símbolo rechazado; `TablaAmbitos` la convierte en `ERROR DE MEMORIA` con la línea de la declaración. También impide reservar una declaración dos veces o en dos regiones. Las regiones y posiciones se recalculan desde cero al analizar de nuevo el programa.

## Ejemplo: desplazamientos crecientes, sin alineación

```text
INI {
    ENT vA;
    DEC vB;
    BOOL vC;
    CAR vD;
    TXT vE;
}
```

| Símbolo | Región | Desplazamiento | Tamaño | Bytes del intervalo |
| --- | --- | ---: | ---: | --- |
| `vA` | `INI` | 0 | 4 | [0, 4) |
| `vB` | `INI` | 4 | 8 | [4, 12) |
| `vC` | `INI` | 12 | 1 | [12, 13) |
| `vD` | `INI` | 13 | 2 | [13, 15) |
| `vE` | `INI` | 15 | 8 | [15, 23) |

`INI` tiene un total de 23 bytes simbólicos. Los intervalos contiguos no se superponen. El offset de `CAR` puede no ser múltiplo de 2: el modelo elegido no añade alineación.

## Marcos de función y bloques

```text
INI {
    ENT vGlobal;
    FUNC DEC fSuma(ENT vA, DEC vB) {
        BOOL vC;
        SI (VDD) { TXT vD; }
        REGR vB;
    }
    CAR vFinal;
}
```

`INI` contiene `vGlobal` en offset 0 (4 bytes) y `vFinal` en offset 4 (2 bytes): total 6. La función tiene su propia región, por ejemplo `FUNCION#1:fSuma`: `vA` en 0 (4), `vB` en 4 (8), `vC` en 12 (1) y `vD` en 13 (8): `Funcion.TamanoMarcoBytes` vale 21. El bloque de `SI` crea un ámbito, pero **no** otra región de memoria. Una función anidada sí inicia un marco nuevo desde el offset 0, independiente del marco que la contiene.

Los bloques hermanos comparten contador de región y no reutilizan huecos de otro bloque. La variable declarada en el encabezado de `POR` reserva en la región de la función contenedora o de `INI`. Variables en `REPT` y casos de `ENCASO` se rigen por la misma regla. Reasignar `vA = 2;` cambia únicamente el texto de `Valor`; no modifica offset ni tamaño.

La dirección conceptual de una variable sería `base de la instancia de región + desplazamiento`; el programa **no** reserva RAM, no ejecuta funciones, no gestiona llamadas recursivas ni determina una dirección física. En una futura ejecución, cada invocación de la misma función tendría su propia instancia de marco con los mismos desplazamientos relativos.

Si una declaración se rechaza por duplicado o sombreado, no consume espacio. Tampoco se asigna una región a una función rechazada ni espacio a sus declaraciones interiores, aunque el analizador aún las conserve para los diagnósticos de alcance. Un identificador de función, válido o no, nunca ocupa un hueco de variable.

## Verificación y continuidad

Los casos D01–D15 de `tests/Automatas.Pruebas/PruebasMemoria.cs` comprueban cada tamaño, posiciones y totales de `INI` y funciones, parámetros, funciones anidadas, bloques hermanos, ciclos, casos, reasignación, declaraciones rechazadas, reinicio de análisis, desbordamiento y reserva única. Resultado conjunto actual, incluida la revisión de errores 1.6/1.7: **200/200**. El proyecto principal también compila en .NET Framework 4.7.2 (Debug/x64). `dgvSimbolos` muestra ámbito, clase, línea, región, tamaño y desplazamiento; `dgvFunciones` muestra región y tamaño de marco. Ambos permiten desplazarse horizontalmente.
