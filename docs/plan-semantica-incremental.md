# Plan incremental: tipos, ámbitos y memoria simbólica

Este archivo conserva las decisiones y el avance para continuar en entregas pequeñas. Cada entrega termina con pruebas y una explicación de los cambios. Los elementos pendientes no describen capacidades actuales.

## Decisiones del lenguaje

- Ámbito léxico por bloques, con ámbito principal de `INI` y ámbitos propios para funciones.
- Sin sombreado: una declaración no puede repetir un nombre de su ámbito o sus padres, incluso si la declaración del padre aparece después. Los ámbitos hermanos pueden tener nombres iguales.
- Las variables deben declararse antes de usarse; los parámetros pertenecen a su función.
- `POR` tiene un ámbito que incluye encabezado y cuerpo. Su variable declarada no es visible después del ciclo.
- `REPT` comparte ámbito con la condición `HASTA`; cada caso de `ENCASO` crea un ámbito hijo separado.
- Las funciones anidadas pertenecen al ámbito donde se declaran; no se trasladan a `INI`. Pueden llamarse antes de su declaración dentro de ese ámbito. Las variables y parámetros siguen el orden textual de los tokens.
- Memoria simbólica: una región principal y otra por función; desplazamientos crecientes, sin alineación ni reutilización de huecos.
- Modelo de tamaños: `ENT` 4 bytes, `DEC` 8, `BOOL` 1, `CAR` 2, `TXT` 8 como referencia, `VAC` 0 (solo retorno). No representa el consumo de objetos del CLR.
- `ENT` es entero con signo de 32 bits: de -2147483648 a 2147483647. Un literal fuera de rango es error, incluso si se asigna a `DEC`.
- Los números con punto o exponente (`e`/`E`) se clasifican como `DEC`. La notación científica ya se reconoce en el separador de lexemas; su aceptación léxica depende del AFD SQLite.
- Para una futura ejecución, la representación propuesta de `DEC` es de 64 bits (`double`), no `decimal` de C#. La validación de su rango y de resultados calculados se abordará por separado.
- El esquema de traducción de 1.5 se documenta mediante acciones asociadas a las reglas y su correspondencia con métodos reales. No se presupone generación de código intermedio.

## Entregas y archivos previstos

### 1. Base de tipos y límites de ENT — completada

- [x] Crear `Automatas/CatalogoTipos.cs`: tamaños del modelo y validación de literales enteros.
- [x] Integrar la validación en `VerificadorTipos.cs` con mensajes de línea y propagación de `ERROR`.
- [x] Incluir el archivo en `Automatas.csproj` y en el proyecto de pruebas.
- [x] Probar límites, signos, números muy largos, uso en varios contextos y ausencia de errores en cascada.
- [x] Actualizar documentación con el resultado verificado.

Resultado: **98 pruebas correctas, 0 fallidas** (74 anteriores y 24 nuevas). Se ejecutaron las clases reales mediante el proyecto de consola; el AFD SQLite y la interfaz no forman parte de esa prueba.

`ObtenerTamanoBytes` define los tamaños utilizados por la entrega 4. El rango de los literales `ENT` se comprueba por separado de la disposición de memoria.

### 2. Ámbitos y declaraciones — completada

- [x] Crear `Ambito.cs`: cadena principal/hijos, sin sombreado y visibilidad por línea.
- [x] Extender `Simbolo.cs` con `Clase`, `LineaDeclaracion`, `Ambito` e `IgnorarLineaDeclaracion`.
- [x] Registrar las declaraciones reales en `TablaAmbitos.cs`, con ID único por declaración.
- [x] Identificar bloques a partir de tokens y contexto gramatical, no de líneas que contengan únicamente una llave.
- [x] Resolver `POR`, parámetros, duplicados y uso antes de declarar dentro del programa.
- [x] Reemplazar los diccionarios de tipo de `VerificadorTipos` por la consulta del ámbito (entrega 3).
- [x] Separar en `Form1` el catálogo de lexemas de las declaraciones semánticas; las llamadas no registran funciones.

Resultado de la parte 1: **110 pruebas correctas, 0 fallidas** (98 anteriores y 12 del modelo de ámbitos).

Resultado de la integración de las entregas 2 y 3: **148 pruebas correctas, 0 fallidas**, incluidas 38 nuevas de programas completos. Compilación del proyecto Windows Forms para .NET Framework 4.7.2, Debug/x64, correcta. Las pruebas no ejecutan el AFD SQLite ni interactúan con los controles de la interfaz.

Reglas ya implementadas en `Ambito`:

| Regla | Comportamiento |
| --- | --- |
| Visibilidad | El programa utiliza `PosicionDeclaracion` (índice de token), incluso si declaración y uso están en la misma línea. La API por línea del modelo se conserva. |
| Uso anterior | La declaración más cercana gana aunque aún no sea visible; el error no se resuelve con el ámbito padre. |
| Duplicado | Dos declaraciones con el mismo nombre en un ámbito se rechazan y no se almacenan. |
| Sombreado | Un hijo no puede repetir un nombre de ningún antecesor, independientemente del orden textual. |
| Hermanos | Dos bloques hermanos pueden usar el mismo nombre si ningún antecesor lo declara. |
| Funciones | `IgnorarLineaDeclaracion` permite invocarlas antes de que aparezcan en el archivo. |
| Parámetros | Se declaran en el ámbito de la función y el cuerpo los ve; el principal no. |

`TablaAmbitos` agrega el prefijo `ERROR DE ÁMBITO` a duplicados, sombreado y usos anteriores a la declaración. Los identificadores inexistentes conservan `ERROR DE TIPO` por compatibilidad. El verificador propaga `ERROR` sin duplicar el diagnóstico de cada referencia.

### 3. Integración del verificador — completada

- [x] Consultar el símbolo asociado a la posición de cada referencia, conservando la posición original al extraer expresiones y partes de `POR`.
- [x] Revisar usos en asignaciones, condiciones, impresión, retornos, argumentos y selectores de `ENCASO`.
- [x] Evitar que un identificador desconocido genere diagnósticos de tipo derivados del mismo problema.
- [x] Preservar la jerarquía y las reglas de tipos ya probadas.

También se infieren las expresiones de `IMP`, `REGR` y argumentos; la revisión posterior de 1.6/1.7 añadió la comparación con parámetros y retorno declarado. Los diccionarios opcionales del constructor solo sirven para clientes de expresiones aisladas y firmas externas explícitas; `Form1` analiza programas completos desde los tokens.

### 4. Regiones, tamaños y desplazamientos — completada

- [x] Crear `RegionMemoria.cs` y calcular la disposición desde `TablaAmbitos` con `CatalogoTipos`.
- [x] Añadir región, tamaño y desplazamiento a símbolos válidos de variables y parámetros; `Funcion.TamanoMarcoBytes` consulta el tamaño de su región.
- [x] Reservar una posición por declaración válida; las reasignaciones no consumen espacio.
- [x] Comprobar tamaños conocidos, offsets consecutivos sin superposición, reserva única y desbordamiento mediante `checked`.
- [x] Mantener direcciones relativas, sin memoria física, alineación, huecos reutilizados ni contenido de cadenas.

**Resultado:** 163 pruebas correctas, 0 fallidas (148 anteriores y 15 nuevas); compilación Debug/x64 de .NET Framework 4.7.2 correcta. Las pruebas de overflow usan una región aislada con una reserva grande, ya que ningún tipo del lenguaje individual ocupa tantos bytes. Véase [Memoria simbólica](memoria-simbolica.md) para ejemplos y limitaciones.

### 5. Interfaz y documentación 1.5–1.7 — completada

- [x] Mostrar ámbito, clase, línea de declaración, región, tamaño y desplazamiento en `dgvSimbolos`; región y tamaño de marco en `dgvFunciones` (`Form1.cs` / `Form1.Designer.cs`).
- [x] Documentar la correspondencia de acciones semánticas con la gramática en [Esquema de traducción semántica](esquema-traduccion-semantica.md), y ejemplos de búsqueda y disposición en [Ámbitos y declaraciones](ambitos-y-declaraciones.md) y [Memoria simbólica](memoria-simbolica.md).
- [x] Añadir pruebas de integración de construcción de tablas: el ejecutable de pruebas y Windows Forms ya llaman al mismo constructor semántico.

**Interfaz:** los dos paneles tienen desplazamiento horizontal y anchos fijos para conservar legibles las columnas nuevas. Las celdas sin región u offset muestran «—». Compilación Windows Forms (Debug/x64) y 163 pruebas de análisis correctas; la interacción visual de Windows Forms no se automatiza desde las pruebas de consola.

## Ampliaciones posteriores

- [x] Comprobar cantidad y tipos de argumentos y validar cada `REGR` frente al retorno declarado, incluido `VAC`.
- [x] Comprobar inicialización definida por intersección de ramas y tratamiento conservador de ciclos; parámetros inicializados al entrar.
- Evaluación de expresiones constantes, rango de `DEC` y desbordamiento de resultados numéricos.
- Intérprete y comprobaciones durante la ejecución.

Revisión de los errores de 1.6 y 1.7: [Cobertura de errores](cobertura-errores-1.6-1.7.md). **200/200 pruebas correctas** (37 pruebas nuevas E01–E37, además de una expectativa ampliada en R04). Se comprueban diagnósticos estáticos de inicialización, tipos, lógica tipada, alcance y memoria simbólica; fugas, direcciones físicas y fallos que dependen de ejecutar el programa no forman parte del analizador estático.

## Punto de continuación

Las entregas 1–5 y la revisión estática de errores 1.6/1.7 están completadas. A continuación: decidir si se requiere comprobar que **todas las rutas** de una función devuelvan valor y validar `DEC`/expresiones constantes; después, si se implementa un intérprete, añadir comprobaciones de ejecución. La matriz está en [Cobertura de errores](cobertura-errores-1.6-1.7.md).

## Cómo funciona la entrega 1

1. `Verificar` limpia los errores y revisa todos los tokens `CNU`. Esto cubre literales en `IMP`, `REGR` y argumentos, aunque la comprobación completa de esas instrucciones siga pendiente.
2. `ObtenerTipoOperando` consulta `CatalogoTipos`: un número con punto o exponente produce `DEC`; un literal entero debe poder representarse mediante `Int32`.
3. `int.TryParse` usa signo opcional y cultura invariante. Un número demasiado grande produce `false`, sin lanzar una excepción ni depender del idioma de Windows.
4. Un entero fuera de rango genera un mensaje con la línea del literal y devuelve `ERROR`. Las operaciones posteriores propagan ese estado sin añadir incompatibilidades derivadas.
5. La pasada previa y la inferencia pueden encontrar el mismo literal. Se conserva un solo diagnóstico por combinación de literal y línea; el mismo literal en dos líneas diferentes sí produce dos mensajes.

Ejemplo para probar tras recompilar la aplicación:

```text
INI {
    ENT vMaximo = 2147483647;
    ENT vMinimo = -2147483648;
    ENT vFuera = 2147483648;
}
```

Las dos primeras declaraciones son válidas. En la línea 4 se espera:

```text
ERROR DE TIPO: El literal '2147483648' está fuera del rango permitido para ENT (-2147483648 a 2147483647; 4 bytes).
```

Esta entrega comprueba los literales, no el resultado de operaciones. Por ejemplo, `2147483647 + 1` requiere evaluación de constantes para detectar su desbordamiento; esa ampliación sigue pendiente. Tampoco se comprueba todavía el rango de `DEC`.
