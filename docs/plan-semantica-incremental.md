# Plan incremental: tipos, ámbitos y memoria simbólica

Este archivo conserva las decisiones y el avance para continuar en entregas pequeñas. Cada entrega termina con pruebas y una explicación de los cambios. Los elementos pendientes no describen capacidades actuales.

## Decisiones del lenguaje

- Ámbito léxico por bloques, con ámbito principal de `INI` y ámbitos propios para funciones.
- Sin sombreado: una declaración no puede repetir un nombre visible en su ámbito o sus padres. Los ámbitos hermanos pueden tener nombres iguales.
- Las variables deben declararse antes de usarse; los parámetros pertenecen a su función.
- `POR` tiene un ámbito que incluye encabezado y cuerpo. Su variable declarada no es visible después del ciclo.
- Memoria simbólica: una región principal y otra por función; desplazamientos crecientes, sin alineación ni reutilización de huecos.
- Modelo de tamaños: `ENT` 4 bytes, `DEC` 8, `BOOL` 1, `CAR` 2, `TXT` 8 como referencia, `VAC` 0 (solo retorno). No representa el consumo de objetos del CLR.
- `ENT` es entero con signo de 32 bits: de -2147483648 a 2147483647. Un literal fuera de rango es error, incluso si se asigna a `DEC`.
- Los números con punto o exponente (`e`/`E`) se clasifican como `DEC`. La notación científica ya se reconoce en el separador de lexemas; su aceptación léxica depende del AFD SQLite.
- Para una futura ejecución, la representación propuesta de `DEC` es de 64 bits (`double`), no `decimal` de C#. La validación de su rango y de resultados calculados se abordará por separado.
- El esquema de traducción de 1.5 se documentará mediante acciones asociadas a las reglas y su correspondencia con métodos reales. No se presupone generación de código intermedio.

## Entregas y archivos previstos

### 1. Base de tipos y límites de ENT — completada

- [x] Crear `Automatas/CatalogoTipos.cs`: tamaños del modelo y validación de literales enteros.
- [x] Integrar la validación en `VerificadorTipos.cs` con mensajes de línea y propagación de `ERROR`.
- [x] Incluir el archivo en `Automatas.csproj` y en el proyecto de pruebas.
- [x] Probar límites, signos, números muy largos, uso en varios contextos y ausencia de errores en cascada.
- [x] Actualizar documentación con el resultado verificado.

Resultado: **98 pruebas correctas, 0 fallidas** (74 anteriores y 24 nuevas). Se ejecutaron las clases reales mediante el proyecto de consola; el AFD SQLite y la interfaz no forman parte de esa prueba.

`ObtenerTamanoBytes` deja definidos los tamaños que utilizará la entrega 4; todavía no asigna desplazamientos ni crea regiones.

### 2. Ámbitos y declaraciones — en curso (parte 1 completada)

- [x] Crear `Ambito.cs`: cadena principal/hijos, sin sombreado y visibilidad por línea.
- [x] Extender `Simbolo.cs` con `Clase`, `LineaDeclaracion`, `Ambito` e `IgnorarLineaDeclaracion`.
- [ ] Registrar las declaraciones reales: dejar de usar el diccionario por nombre como tabla de símbolos.
- [ ] Identificar bloques a partir de tokens y contexto gramatical, no de líneas que contengan únicamente una llave.
- [ ] Resolver `POR`, parámetros, duplicados y uso antes de declarar dentro del programa.
- [ ] Reemplazar los diccionarios de tipo de `VerificadorTipos` por la consulta del ámbito (entrega 3).

Resultado de la parte 1: **110 pruebas correctas, 0 fallidas** (98 anteriores y 12 del modelo de ámbitos).

Reglas ya implementadas en `Ambito`:

| Regla | Comportamiento |
| --- | --- |
| Visibilidad | Una declaración se encuentra desde su línea de declaración hasta el final de su ámbito. |
| Uso anterior | La declaración más cercana gana aunque aún no sea visible; el error no se resuelve con el ámbito padre. |
| Duplicado | Dos declaraciones con el mismo nombre en un ámbito se rechazan y no se almacenan. |
| Sombreado | Un hijo no puede repetir un nombre de su padre. |
| Hermanos | Dos bloques hermanos pueden usar el mismo nombre si el principal no lo declara. |
| Funciones | `IgnorarLineaDeclaracion` permite invocarlas antes de que aparezcan en el archivo. |
| Parámetros | Se declaran en el ámbito de la función y el cuerpo los ve; el principal no. |

Los mensajes de `Ambito` no llevan prefijo porque todavía ninguna etapa los emite; `VerificadorTipos` añadirá el prefijo al integrarlos.

### 3. Integración del verificador — pendiente

- Reemplazar los diccionarios globales de tipos por la consulta del símbolo visible en cada referencia.
- Revisar usos en asignaciones, condiciones, impresión, retornos y argumentos; no solo los casos que actualmente recorre `Verificar`.
- Evitar que un identificador desconocido genere diagnósticos de tipo derivados del mismo problema.
- Preservar la jerarquía y las reglas de tipos ya probadas.

### 4. Regiones, tamaños y desplazamientos — pendiente

- Crear un calculador de disposición por región usando `CatalogoTipos`.
- Añadir región, tamaño y desplazamiento a los símbolos de variables y parámetros; registrar tamaño de marco por función.
- Reservar posición una sola vez por declaración válida; una reasignación no consume otra posición.
- Comprobar tamaños conocidos, rangos sin superposición y desbordamiento del contador mediante aritmética comprobada.
- No asignar memoria física ni calcular el contenido variable de cadenas. Los offsets de funciones son relativos a cada futura instancia del marco.

### 5. Interfaz y documentación 1.5–1.7 — pendiente

- Mostrar ámbito, clase, línea de declaración, región, tamaño y desplazamiento en `Form1.cs` / `Form1.Designer.cs`.
- Documentar acciones semánticas y su relación con la gramática, así como ejemplos de búsqueda y disposición de memoria.
- Añadir pruebas de integración de construcción de tablas para reducir diferencias entre el ejecutable de pruebas y Windows Forms.

## Ampliaciones posteriores

- Comprobar firmas completas de llamadas y retornos.
- Comprobar inicialización definida mediante análisis de flujo (un simple booleano no basta para ramas y ciclos).
- Evaluación de expresiones constantes, rango de `DEC` y desbordamiento de resultados numéricos.
- Intérprete y comprobaciones durante la ejecución.

## Punto de continuación

Completar la parte 2 de la entrega 2: construir la tabla de símbolos por ámbito a partir de los tokens, empezando por separar en `Form1.cs` el registro de lexemas del registro de declaraciones. No introducir offsets en el diccionario global por nombre.

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
