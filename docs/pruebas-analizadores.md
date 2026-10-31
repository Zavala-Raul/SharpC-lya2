# Pruebas de generación y representaciones de código intermedio

Esta guía conserva la verificación relacionada con generación de código intermedio: cuádruplos, tres direcciones, triplos, Código P, notaciones y bloqueo de salidas ante errores. El [contrato de implementación](plan-codigo-intermedio.md) describe los formatos y reglas.

## Cómo ejecutarlas

Desde la raíz del repositorio, con el SDK .NET 10:

```sh
dotnet run --project tests/Automatas.Pruebas/Automatas.Pruebas.csproj
```

En el entorno Windows/WSL utilizado para las comprobaciones:

```sh
"/mnt/c/Program Files/dotnet/dotnet.exe" run --project tests/Automatas.Pruebas/Automatas.Pruebas.csproj
```

El ejecutable imprime `OK` o `FALLO` por caso y devuelve código `1` si alguna prueba falla. No usa un framework de pruebas; se ejecuta con `dotnet run`.

## Resultado registrado

```text
Total: 317; correctas: 317; fallidas: 0.
```

Este total corresponde a toda la batería, incluidas las regresiones de análisis que protegen las precondiciones de generación. La limpieza documental no implica una nueva ejecución de las pruebas.

| Grupo relacionado con la síntesis | Casos | Comprobación |
| --- | ---: | --- |
| I01–I26 | 26 | Generación base: expresiones, asignaciones, precedencia, tipos, identidad de símbolos y reinicio. |
| G01–G42 | 42 | Lenguaje completo, control de flujo, funciones, incorporadas y formato textual. |
| U01–U18 | 18 | Equivalencia de Triplos y Código P, tres notaciones y errores de conversión. |
| T01–T20 | 20 | Retorno garantizado en semántica, para bloquear generación de funciones incompletas. |
| L01–L11 | 11 | Formas de LEER, destino e inicialización necesarios para representar lectura correctamente. |

Los otros 200 casos son regresiones del análisis previo; no son 200 pruebas adicionales del generador.

## Generación base: I01–I26

Archivo: [`PruebasCodigoIntermedio.cs`](../tests/Automatas.Pruebas/PruebasCodigoIntermedio.cs).

- Copias, inicializadores y declaraciones sin asignación implícita.
- Prioridad de multiplicación, agrupación y potencia asociativa a la derecha.
- Resta/división asociativas a la izquierda y conservación de operandos.
- Grupos hermanos y autoasignaciones sin sobrescribir un resultado todavía necesario.
- Identidad de símbolos, posición de tokens y número de línea.
- Temporales de tipo estable; promoción ENT a DEC sin convertir una división entera en decimal.
- Reinicio de contadores y ausencia de mutación de la memoria simbólica del usuario.
- Bloqueo de generación si no existe un análisis válido o hay errores semánticos.

I15–I24 inicialmente comprobaban construcciones fuera del alcance. Ahora exigen su traducción: tipos no numéricos, lectura, impresión, selección, ciclos y funciones.

## Lenguaje completo y tres direcciones: G01–G42

Archivo: [`PruebasGeneracionCompleta.cs`](../tests/Automatas.Pruebas/PruebasGeneracionCompleta.cs).

| Área | Ejemplos de lo verificado |
| --- | --- |
| Tipos y expresiones | ENT, DEC, TXT, CAR, BOOL, concatenación y operadores relacionales/lógicos. |
| Selección | SI/SINO, ENCASO/DFCT y ausencia de caída entre casos. |
| Repetición | Reevaluación de condiciones, orden de POR, ejecución inicial de REPT y alcance de ROMPER. |
| Funciones | Llamadas adelantadas y anidadas, recursión, parámetros, retornos y ámbitos léxicos. |
| Efectos de llamadas | Captura del operando izquierdo y de argumentos antes de funciones que modifican variables. |
| Identidad | Variables y funciones homónimas de ámbitos diferentes. |
| Entrada/salida | LEER tipado, IMP y firmas de incorporadas oficiales. |
| Presentación | Texto de tres direcciones, saltos, llamadas, tipos y campos ausentes. |
| Intercambio | INTER se traduce a tres copias con temporal; no representa arreglos. |

G36 protege el significado del operador escrito con las categorías del AFD real: `< → OPR2`, `<> → OPR5`, `> → OPR1`, `<= → OPR4`, `>= → OPR3` y `== → OPR6`.

G37–G42 verifican intercambio entre tipos iguales, rechazo de tipos incompatibles y valores sin inicializar, colores hexadecimales y cantidades ENT de SLTLN/IMPREP.

## Triplos, Código P y notaciones: U01–U18

Archivo: [`PruebasRepresentaciones.cs`](../tests/Automatas.Pruebas/PruebasRepresentaciones.cs).

- Infija/prefija/postfija de expresiones completas, no solo fragmentos con temporales.
- Precedencia, asociatividad, negación y paréntesis.
- Cadenas con espacios conservadas como operandos.
- Llamadas con aridad explícita y lectura sin argumentos.
- Contextos de condiciones, retornos e incorporadas sin duplicar filas por cada subárbol.
- Triplos de acumulador que protegen casos como `vA = 3 - vA`.
- Código P con el orden correcto de resta/división y tipos de promoción.
- Conservación de aridad, identidad, origen y balance de pila.
- Reinicio y publicación conjunta: una conversión fallida deja todas las salidas vacías.

## Cómo se comprueba la equivalencia

Los casos ejecutables del grupo G comparan sus salidas con las obtenidas tras traducir a Triplos y Código P. Adaptadores exclusivos de pruebas reconstruyen operaciones desde los campos de esas representaciones; del cuádruplo de origen solo conservan metadatos de posición y función.

Un oráculo de pruebas recorre las operaciones resultantes y verifica el comportamiento de ciclos, llamadas, activaciones recursivas y efectos. Ese oráculo y sus adaptadores **no forman parte de la aplicación**: el formulario genera y muestra instrucciones, no las ejecuta.

## Errores que deben impedir resultados

- Un error léxico, sintáctico o semántico bloquea las fases siguientes desde el formulario.
- El [análisis de retornos](../Automatas/AnalizadorRetornos.cs), cubierto por [`PruebasRetornos.cs`](../tests/Automatas.Pruebas/PruebasRetornos.cs), rechaza rutas incompletas antes de generar. No se emite FALTA_RETORNO en el IR.
- Un fallo de generación, por ejemplo ROMPER sin contenedor válido, devuelve diagnóstico y cero instrucciones publicadas.
- Una operación sin traducción produce un error de representación con línea y nombre de la conversión.
- Editar la fuente o iniciar otro análisis limpia las cinco vistas. No deben quedar resultados de una fuente anterior.

## Compilación e integración del formulario

```sh
"/mnt/c/Program Files/dotnet/dotnet.exe" msbuild Automatas/Automatas.csproj /t:Build /p:Configuration=Debug /p:Platform=x64 /v:minimal /nologo
```

Compilación registrada: Windows Forms, .NET Framework 4.7.2, Debug/x64, correcta.

Comprobaciones realizadas con el AFD SQLite real:

- Cinco vistas interiores: tres direcciones, cuádruplos, triplos, Código P y notaciones.
- Ejemplo aritmético con 3 cuádruplos, 7 triplos y 2 expresiones con tres notaciones.
- Funciones, parámetros, llamadas y ciclos presentes en las nuevas representaciones.
- Controles con espacio útil a 900 × 600.
- Diagnóstico visible y salidas vacías ante error de generación.
- Bloqueo de todas las vistas ante error semántico.
- Reconocimiento de las incorporadas oficiales y rechazo semántico de INTER incompatible.

Las pruebas de consola usan un fixture léxico separado: no sustituyen la comprobación del AFD ni una inspección visual de la aplicación.

## Material para continuar

- [Contrato y representaciones](plan-codigo-intermedio.md).
- [Guía de flujo en tres perspectivas](flujo-codigo-intermedio-tres-perspectivas.md).
- [Guía visual de clases y métodos](guia-visual-defensa-codigo-intermedio.md).
- [Ejemplos manuales](ejemplos-prueba.md).
