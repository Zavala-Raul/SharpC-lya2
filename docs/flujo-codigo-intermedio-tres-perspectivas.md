# El nuevo flujo del programa explicado de tres formas

Esta guía explica la implementación actual de SharpC: cómo pasa del texto fuente a **código de tres direcciones, cuádruplos, triplos, Código P y notaciones**. Está escrita para comprenderla, defenderla en una exposición y continuar desarrollándola.

## Cómo leer esta guía

1. [Como si tuvieras 13 años](#1-como-si-tuvieras-13-años): qué sucede, con un ejemplo y pocas palabras técnicas.
2. [Para responder al profesor](#2-para-responder-al-profesor): para qué sirve cada clase o método que podría abrir.
3. [Para tomar las riendas del proyecto](#3-para-tomar-las-riendas-del-proyecto): arquitectura, contratos, decisiones, pruebas y puntos de modificación.

Referencia detallada de las instrucciones: [Contrato de código intermedio](plan-codigo-intermedio.md). Esta guía explica el recorrido; el contrato especifica el significado de cada operación.

## Ejemplo común

Usaremos este programa en las tres explicaciones:

```text
INI {
    ENT vA = 2;
    ENT vR = vA + 3 * 4;

    SI (vR > 10) {
        IMP(vR);
    }
    SINO {
        IMP(vA);
    }
}
```

Si se ejecutara, `vR` valdría 14 y se imprimiría 14. **Nuestra aplicación no ejecuta ese cálculo ni esa impresión**: valida el programa y construye instrucciones que representan lo que tendría que ocurrir.

La traza de este ejemplo se comprobó en el formulario con el AFD SQLite real: **10 cuádruplos, 16 triplos, 23 instrucciones P y 5 expresiones fuente**, sin cambiar el código de la aplicación para preparar esta guía.

---

# 1. Como si tuvieras 13 años

## 1.1. La idea: traducir una receta a instrucciones pequeñas

Imagina que escribes una receta y se la entregas a un equipo de ayudantes. Cada ayudante tiene una tarea:

1. Uno separa las palabras y los signos.
2. Otro reconoce qué significa cada pieza.
3. Otro revisa que la receta esté bien escrita.
4. Otro comprueba que las acciones tengan sentido.
5. Otro divide las acciones grandes en instrucciones pequeñas.
6. Los últimos muestran esas instrucciones de distintas maneras.

En el programa ocurre algo parecido:

```text
Texto escrito
     │
     ▼
Separar piezas → reconocerlas → revisar estructura → revisar significado
                                                           │
                                                           ▼
                                              Crear instrucciones pequeñas
                                                           │
                                                           ▼
                                               Mostrar las cinco vistas
```

No son cinco programas diferentes. Son cinco maneras de estudiar el mismo programa y sus expresiones.

## 1.2. Primero cortamos la línea en piezas

La línea:

```text
ENT vR = vA + 3 * 4;
```

se separa así:

```text
ENT | vR | = | vA | + | 3 | * | 4 | ;
```

La clase `TokenizadorFuenteSharpC` hace esta separación. Cada pieza se llama **lexema**.

Después `AnalizadorLexico` reconoce las piezas mediante la matriz de transición de SQLite:

| Pieza | Qué reconoce |
| --- | --- |
| `ENT` | Palabra reservada de tipo entero. |
| `vR`, `vA` | Identificadores de variables. |
| `3`, `4` | Números. |
| `+`, `*` | Operadores. |
| `=` | Asignación. |
| `;` | Fin de instrucción. |

El programa guarda tanto la categoría como el texto de cada pieza. Saber solamente «esto es una variable» no permite distinguir `vA` de `vR`.

## 1.3. Después revisamos que todo esté bien

`AnalizadorSintacticoLl1` revisa la **estructura**: llaves, paréntesis, orden de las piezas y puntos y comas.

`VerificadorTipos` y sus auxiliares revisan el **significado**:

- Que `vA` exista antes de usarla.
- Que tenga un valor antes de leerla.
- Que se puedan sumar y multiplicar esos datos.
- Que el resultado se pueda guardar en `vR`.
- Que la condición de `SI` produzca verdadero o falso.
- Si hubiera funciones, que sus argumentos y retornos fueran correctos.

Si alguna de esas revisiones encuentra errores, no se publican las representaciones de código intermedio.

## 1.4. Ahora hacemos un dibujo de la cuenta

Una vez validado el programa, el generador organiza esta expresión:

```text
vA + 3 * 4
```

como un árbol:

```text
        +
       / \
     vA   *
         / \
        3   4
```

El dibujo dice que el producto forma el lado derecho de la suma. Se construye con objetos `ExpresionIntermedia`.

Todavía no hemos calculado `3 * 4`. Hemos decidido **cómo está organizada la cuenta**.

## 1.5. Dividimos la cuenta en pasos pequeños

La traducción de esa asignación es:

```text
t1 = 3 * 4
vR = vA + t1
```

`t1` es un **temporal**: un nombre interno para guardar un resultado intermedio cuando el programa llegue a ejecutarse.

Piensa en `t1` como una cajita auxiliar. El generador escribe que debe usarse esa cajita; no mete ahora el número 12 dentro de ella.

## 1.6. ¿Y qué pasa con SI y SINO?

La selección necesita instrucciones que indiquen por dónde seguir:

```text
t2 = vR > 10
ifFalse t2 goto L1
IMP(vR)
goto L2
L1:
IMP(vA)
L2:
```

- `t2` representa el resultado de la comparación.
- `L1:` es un marcador: aquí empieza la rama falsa.
- `L2:` es otro marcador: aquí termina la selección.
- `ifFalse` significa «si la condición es falsa, salta».
- `goto` significa «continúa en este marcador».

`L1` no significa «línea 1», ni guarda un dato. Es el nombre de una **etiqueta**.

## 1.7. Las cinco vistas

### A. Código de tres direcciones

Muestra las instrucciones como texto:

```text
vA = 2
t1 = 3 * 4
vR = vA + t1
t2 = vR > 10
ifFalse t2 goto L1
IMP(vR)
goto L2
L1:
IMP(vA)
L2:
```

### B. Cuádruplos

Guarda las mismas instrucciones en cuatro campos. Para la multiplicación:

| Destino | Fuente 1 | Fuente 2 | Operación |
| --- | --- | --- | --- |
| t1 | 3 | 4 | * |

Esta tabla y el texto anterior salen de la misma lista de instrucciones.

### C. Triplos

Usan el formato de clase: objeto, fuente y operador. Para la misma multiplicación:

| Objeto | Fuente | Operador |
| --- | --- | --- |
| r1 | 3 | — |
| r1 | 4 | * |
| t1 | r1 | = |

Se lee: «copia 3 a `r1`; multiplica `r1` por 4; copia el resultado a `t1`». `r1` es una cajita de trabajo exclusiva de esta representación.

El guion `—` indica que ese campo no tiene operador escrito: en la primera fila se carga inicialmente el acumulador.

### D. Código P

Describe instrucciones para una máquina que usa una pila, como una torre de platos:

```text
load 3
load 4
mul
store t1
```

Se lee: «apila 3; apila 4; toma los dos para multiplicarlos; guarda el resultado en `t1`».

La aplicación muestra esas instrucciones. No ejecuta una máquina de pila real en el formulario.

### E. Notaciones

Muestran la **expresión original completa**, no solamente los pasos con temporales:

| Infija | Prefija | Postfija |
| --- | --- | --- |
| `vR = (vA + (3 * 4))` | `= vR + vA * 3 4` | `vR vA 3 4 * + =` |

- Infija: el operador va entre sus datos.
- Prefija: el operador va antes.
- Postfija: el operador va después.

Las tres dicen lo mismo. Los paréntesis adicionales de la infija permiten ver exactamente qué se agrupa primero.

## 1.8. Si solo recuerdas una idea

> El programa separa y revisa lo que escribimos. Después crea instrucciones pequeñas y las presenta de varias formas. Las cuentas no se ejecutan por cambiar de pestaña.

---

# 2. Para responder al profesor

Versión con mapas, diagramas de llamadas y fichas visuales: [Guía visual para defender el código intermedio](guia-visual-defensa-codigo-intermedio.md).

## 2.1. Cómo responder aunque abra un método de sorpresa

No hace falta recitar todo el programa. Usa cuatro preguntas:

1. **¿Qué recibe?**
2. **¿Qué transforma o comprueba?**
3. **¿Qué devuelve o qué lista modifica?**
4. **¿Quién lo llamó y quién usa su resultado?**

Una respuesta breve puede empezar así:

> «Este método recibe X, realiza Y y entrega Z. Se llama desde A y su resultado lo usa B».

Los fragmentos siguientes son selecciones o simplificaciones del código; los archivos enlazados son la referencia completa.

## 2.2. Si abre Form1

Archivo: [`Automatas/Form1.cs`](../Automatas/Form1.cs).

### «¿Qué hace btnEjecutar_Click?»

> «Es el coordinador. Limpia resultados anteriores, prepara tokens y habilita las etapas siguientes solo cuando las anteriores no tienen errores. La generación ocurre después de un análisis semántico válido».

No contiene las reglas de traducción de expresiones: esas están en el generador.

### «¿Qué hace EjecutarVerificacionTipos?»

```csharp
var verificador = new VerificadorTipos(null);
verificador.Verificar(tokensSintactico);
// Recoger tablas y diagnósticos...
if (erroresTipos.Count == 0)
    MostrarCodigoIntermedio(new GeneradorCodigoIntermedio().Generar(verificador));
```

> «Construye el análisis semántico y, si no hay errores, entrega ese verificador al generador. Así se reutilizan sus tokens y símbolos resueltos».

### «¿Qué hace MostrarCodigoIntermedio?»

> «Prepara las representaciones antes de llenar controles. Si hay un error de generación o conversión, muestra el diagnóstico y deja las salidas vacías. Si todo está bien, llena las cinco vistas».

El punto decisivo es:

```csharp
var vistas = ConstructorRepresentaciones.Construir(resultado);
if (vistas.Errores.Count != 0)
{
    // Mostrar diagnósticos y regresar.
    return;
}
```

### «¿Para qué sirve LimpiarCodigoIntermedio?»

> «Invalida las cinco salidas al editar o analizar otra vez. Evita que el usuario vea una traducción de un código fuente anterior».

El parámetro `estado` existe, pero actualmente no se muestra en una etiqueta ni se utiliza para decidir la generación.

### «¿El botón de la segunda pestaña usa otro generador?»

> «No. btnGenerarCuadruplos_Click llama al mismo btnEjecutar_Click y selecciona la pestaña de código intermedio».

## 2.3. Si abre GeneradorCodigoIntermedio

Archivo: [`Automatas/GeneradorCodigoIntermedio.cs`](../Automatas/GeneradorCodigoIntermedio.cs).

### «¿Qué recibe y devuelve Generar?»

```csharp
public ResultadoCodigoIntermedio Generar(VerificadorTipos analisis)
```

> «Recibe tokens y semántica ya construidos. Comprueba sus precondiciones y la sintaxis, crea una sesión, recorre el principal y devuelve cuádruplos, expresiones fuente y diagnósticos».

No obtiene el programa desde el `dgv` ni analiza el texto guardado en `Simbolo.Valor`.

### «¿Por qué hay una Sesion?»

> «Separa el estado de una generación: tokens, cierres de delimitadores, contadores, función actual y listas de resultados. Cada llamada empieza con una sesión nueva, por lo que no hereda temporales o etiquetas anteriores».

### «¿Qué guarda cierres?»

> «Relaciona la posición de cada paréntesis o llave de apertura con su cierre. Permite recorrer rangos de bloques y condiciones sin depender de las líneas del editor».

### «¿Qué hace Bloque?»

> «Recorre instrucciones entre inicio incluido y fin excluido. Identifica declaraciones, asignaciones, selección, ciclos, funciones y servicios. Despacha su traducción y avanza al token posterior».

`salida` representa la etiqueta para `ROMPER` del contenedor actual. Una función nueva recibe `null` para impedir saltos hacia un ciclo de la función exterior.

### «¿Qué hace Asignar?»

> «Distingue declaración de reasignación, obtiene el símbolo destino y traduce el lado derecho. Si no hay inicializador, no emite nada. Si hay un dato simple, emite copia; si hay una operación o llamada, usa Emitir con el destino final».

### «¿Cuál es la diferencia entre ExpresionRango y Parsear?»

> «ExpresionRango delimita el fragmento y comprueba que se consuma completo. Parsear construye recursivamente el árbol según precedencia y asociatividad».

`Parsear(ref i, fin, minimo)` modifica `i` para comunicar hasta dónde consumió tokens. `minimo` indica qué prioridad debe tener un operador para pertenecer a esa llamada recursiva.

### «¿Parsear calcula el resultado?»

> «No. Construye nodos y consulta el tipo de cada operación. Para vA + 3 * 4 produce un nodo de suma cuyo hijo derecho es una multiplicación, no el número 14».

### «¿Qué hace Emitir?»

> «Recorre el árbol, agrega las instrucciones necesarias y devuelve el operando donde estará disponible el resultado. Un literal o referencia se devuelve directamente; una operación puede necesitar un temporal».

```csharp
if (expresion.Operando != null)
    return expresion.Operando;
```

No siempre crea un temporal: la última operación puede escribir directamente en el destino de la asignación.

### «¿Para qué existe Capturar?»

> «Copia el valor de una variable a un temporal antes de una llamada que podría modificarla. Preserva el orden de evaluación y los valores de argumentos ya evaluados».

Ejemplo: en `vA + fCambiar()`, la llamada podría modificar `vA`; la suma debe usar el valor que se leyó primero. Los argumentos de una llamada se evalúan y capturan antes de emitir su grupo `ARGUMENTO`.

### «¿Qué hacen Registrar, Fuente y RegistrarLlamada?»

> «Conservan expresiones fuente y su contexto para las notaciones. Fuente combina construcción y registro; RegistrarLlamada registra una forma de llamada incorporada, como IMP o LEER, sin confundirla con una función declarada».

Es importante conservar este árbol: reconstruir desde cuádruplos mostraría fragmentos con temporales en lugar de la expresión completa.

### «¿Qué hace Funcion?»

> «Emite un salto para omitir la declaración durante el flujo normal, marca la entrada de función y sus parámetros, traduce el cuerpo bajo su contexto, termina la función y restaura el contexto exterior».

Una llamada posterior emite `LLAMAR`; no vuelve a generar el cuerpo.

### «¿Qué hace Seleccion?»

> «Traduce ENCASO. Captura el selector una vez, compara cada caso y usa saltos para elegir una rama. El ROMPER obligatorio de cada caso salta al final».

La selección `SI/SINO`, en cambio, se implementa directamente dentro de `Bloque`.

### «¿Qué hace Incorporada?»

> «Traduce las palabras de ACT 1/ACT 3 como LIMP, FONCOL, POS e IMPREP. INTER es especial: se baja a tres copias para intercambiar sin perder el primer valor».

No hay una instrucción de arreglos: PR29 corresponde a `INTER`.

### Auxiliares que también puede abrir

| Método | Respuesta breve |
| --- | --- |
| `Es` | Comprueba la categoría de un token en una posición. |
| `EsTipo` | Identifica tipos permitidos para variables; no incluye VAC. |
| `PuntoComa` | Localiza el delimitador de una instrucción ya validada. |
| `Variable` | Recupera el símbolo resuelto y lo envuelve como operando. También se usa para símbolos de función, a pesar del nombre del auxiliar. |
| `Temporal` | Crea un operando auxiliar t1, t2… con tipo. |
| `Etiqueta` | Crea un destino de salto L1, L2…; no es una variable. |
| `Numero` | Crea un literal ENT para índices y aridades. |
| `Argumentos` | Divide argumentos respetando paréntesis anidados y construye sus expresiones. |
| `Agregar` | Añade un Cuadruplo con operandos, tipo, línea, posición y función actual. |
| `Marcar` | Agrega la instrucción ETIQUETA. |
| `Saltar` | Agrega un SALTO incondicional. |
| `Error` | Construye un diagnóstico de generación con línea fuente. |

## 2.4. Si abre los modelos de datos

Archivos: [`Cuadruplo.cs`](../Automatas/Cuadruplo.cs), [`ExpresionIntermedia.cs`](../Automatas/ExpresionIntermedia.cs), [`RepresentacionesIntermedias.cs`](../Automatas/RepresentacionesIntermedias.cs).

| Clase | Qué decir si pregunta para qué es |
| --- | --- |
| `OperandoIntermedio` | «Representa literal, símbolo, temporal o etiqueta. Una variable conserva la referencia a su declaración real». |
| `Cuadruplo` | «Es una instrucción con destino, dos fuentes y operación, más metadatos de tipo y origen». |
| `ExpresionIntermedia` | «Es el árbol de una expresión, con datos, operaciones, llamadas, tipos y posición». |
| `ExpresionFuente` | «Añade al árbol destino, contexto, línea y función para mostrar notaciones completas». |
| `ResultadoCodigoIntermedio` | «Devuelve instrucciones, árboles fuente y errores desde el generador». |
| `Triplo` | «Es un registro de acumulador con objeto, fuente, operador y metadatos». |
| `InstruccionP` | «Es una instrucción de pila, con operandos de control si requiere, tipo, cambio de altura y origen». |
| `NotacionExpresion` | «Reúne las tres cadenas de presentación y su contexto fuente». |
| `ResultadoRepresentaciones` | «Agrupa Triplos, Código P, Notaciones y errores de conversión». |
| `ErrorRepresentacion` | «Identifica una conversión que no puede completarse, con representación y línea». |

## 2.5. Si abre las conversiones y formateadores

### ConstructorRepresentaciones.Construir

> «Coordina las tres salidas nuevas. Si alguna conversión produce ErrorRepresentacion, devuelve listas vacías y diagnóstico; de lo contrario devuelve todas las listas preparadas».

```csharp
var triplos = new GeneradorTriplos().Generar(resultado.Instrucciones);
var codigoP = new GeneradorCodigoP().Generar(resultado.Instrucciones);
var notaciones = new FormateadorNotaciones().Generar(resultado);
```

No hace falta ejecutar una representación para obtener la siguiente. Triplos y P parten independientemente de los cuádruplos; Notaciones parte de los árboles fuente.

### GeneradorTriplos.Generar

Archivo: [`GeneradorTriplos.cs`](../Automatas/GeneradorTriplos.cs).

> «Convierte cada cuádruplo a la variante de triplos enseñada en clase: objeto, fuente y operador. Usa acumuladores r1, r2… independientes para proteger operandos que coincidan con el destino».

Para `vA = 3 - vA`, usar primero `vA = 3` destruiría el valor que debe restarse. Por eso el cálculo se hace en un acumulador aparte.

Una llamada se divide en `PREPARAR_LLAMADA` y `LLAMAR` para conservar función, cantidad de argumentos y resultado dentro de registros de tres campos.

### GeneradorCodigoP.Generar

Archivo: [`GeneradorCodigoP.cs`](../Automatas/GeneradorCodigoP.cs).

> «Convierte cada instrucción al modelo de pila sin optimizarla. Para una operación binaria carga izquierdo y derecho, emite la operación y guarda el resultado. Comprueba que la altura no sea negativa y termine en cero por cuádruplo».

No contiene una pila con valores ejecutados: `altura` es un contador de validación de la traducción.

| Auxiliar local | Propósito |
| --- | --- |
| `Emitir` | Agrega una InstruccionP y actualiza la altura prevista. |
| `Cargar` | Emite load y aumenta la altura. |
| `Guardar` | Emite store y reduce la altura. |
| `OperacionP` | Traduce + a add, * a mul, >= a ge, etc. |

Este `Emitir` local no es `Sesion.Emitir`: pertenecen a fases y clases diferentes.

### FormateadorNotaciones.Generar y Formatear

Archivo: [`FormateadorNotaciones.cs`](../Automatas/FormateadorNotaciones.cs).

> «Generar ordena las expresiones fuente y obtiene sus tres cadenas. Formatear recorre cada árbol según el modo: infija parentizada, prefija en preorden y postfija en postorden».

Las llamadas usan `call[fNombre/n]` para expresar su aridad. Los literales de texto conservan sus comillas; sus espacios no se interpretan como separación de operandos.

### FormateadorCodigoIntermedio.ComoTexto, Operando e Instruccion

Archivo: [`FormateadorCodigoIntermedio.cs`](../Automatas/FormateadorCodigoIntermedio.cs).

> «Convierte la lista de cuádruplos a código de tres direcciones. Operando presenta los nombres, incluyendo ID si hay homónimos; Instruccion decide la forma textual de cada operación».

No calcula ni genera nuevos cuádruplos.

### ConstructorRepresentaciones.TextoP y Binario

> «TextoP presenta las instrucciones de pila y sus tipos. Binario identifica las operaciones comunes de dos operandos que Triplos y Código P deben traducir».

## 2.6. Preguntas que conviene saber contestar

**¿Por qué existen temporales y etiquetas?** Los temporales representan resultados parciales; las etiquetas identifican destinos de salto. Cumplen funciones diferentes.

**¿Estamos usando infija o postfija?** La fuente y las operaciones textuales normales son infijas. La tabla Notaciones muestra las tres. Código P usa instrucciones de pila; no es un selector de «la misma tabla en postfija».

**¿El código P es más correcto que los cuádruplos?** No. Son representaciones equivalentes. La traducción actual conserva temporales y puede tener más instrucciones que una versión compacta.

**¿Dónde se comprueba que una función retorne?** En `AnalizadorRetornos`, llamado por `VerificadorTipos.Verificar`, antes de generar. No existe una instrucción `FALTA_RETORNO` en el generador actual.

**¿Por qué la tabla de símbolos no muestra el resultado calculado?** Porque conserva declaraciones y texto de asignaciones; no es el estado de una ejecución.

**¿Por qué algunos campos dicen —?** Es la presentación de un campo ausente; no es un operando real. En el operador inicial de un triplo significa la carga inicial del acumulador.

---

# 3. Para tomar las riendas del proyecto

## 3.1. Mapa de archivos y responsabilidades

Las clases están en el espacio de nombres `Automatas`. Un archivo puede contener varias clases; no asumas que el nombre del archivo enumera todas las clases que define.

| Archivo | Responsabilidad |
| --- | --- |
| [`Form1.cs`](../Automatas/Form1.cs) | Orquestación del análisis, publicación de resultados y limpieza. |
| [`Form1.Designer.cs`](../Automatas/Form1.Designer.cs) | Controles de las dos pestañas principales y cinco vistas interiores. |
| [`TokenizadorFuenteSharpC.cs`](../Automatas/TokenizadorFuenteSharpC.cs) | Separación de lexemas y posiciones. |
| [`AnalizadorLexico.cs`](../Automatas/AnalizadorLexico.cs) | Clasificación mediante AFD SQLite. |
| [`TablaSintacticaSharpC.cs`](../Automatas/TablaSintacticaSharpC.cs) | Producciones y tabla predictiva. |
| [`AnalizadorSintacticoLl1.cs`](../Automatas/AnalizadorSintacticoLl1.cs) | Reconocimiento LL(1), diagnósticos y recuperación. |
| [`VerificadorTipos.cs`](../Automatas/VerificadorTipos.cs) | Inferencia, compatibilidad, firmas y coordinación semántica. |
| [`TablaAmbitos.cs`](../Automatas/TablaAmbitos.cs), [`Ambito.cs`](../Automatas/Ambito.cs) | Declaraciones, visibilidad y referencias por posición. |
| [`CatalogoTipos.cs`](../Automatas/CatalogoTipos.cs), [`RegionMemoria.cs`](../Automatas/RegionMemoria.cs) | Tipos, límites de ENT y disposición simbólica. |
| [`AnalizadorInicializacion.cs`](../Automatas/AnalizadorInicializacion.cs) | Usos antes de inicializar y combinación de ramas. |
| [`AnalizadorRetornos.cs`](../Automatas/AnalizadorRetornos.cs) | Retorno garantizado conservador por caminos. |
| [`OperadoresExpresion.cs`](../Automatas/OperadoresExpresion.cs) | Normalización, precedencia y asociatividad compartidas. |
| [`GeneradorCodigoIntermedio.cs`](../Automatas/GeneradorCodigoIntermedio.cs) | Descenso de instrucciones y expresiones al IR común. |
| [`Cuadruplo.cs`](../Automatas/Cuadruplo.cs) | Operandos, cuádruplos y resultado común. |
| [`ExpresionIntermedia.cs`](../Automatas/ExpresionIntermedia.cs) | Árboles de expresión y registros de contexto fuente. |
| [`GeneradorTriplos.cs`](../Automatas/GeneradorTriplos.cs) | Conversión al acumulador de tres campos. |
| [`GeneradorCodigoP.cs`](../Automatas/GeneradorCodigoP.cs) | Conversión tipada al modelo de pila. |
| [`FormateadorCodigoIntermedio.cs`](../Automatas/FormateadorCodigoIntermedio.cs) | Tres direcciones y nombres visibles compartidos. |
| [`FormateadorNotaciones.cs`](../Automatas/FormateadorNotaciones.cs) | Infija, prefija y postfija del árbol fuente. |
| [`RepresentacionesIntermedias.cs`](../Automatas/RepresentacionesIntermedias.cs) | Modelos de las nuevas vistas, coordinación de conversiones y texto P. |

## 3.2. El diagrama de datos que debes tener en mente

```text
scintilla1.Text
      │
      ▼
Form1.btnEjecutar_Click
      │
      ├── tokenización + AFD → tokensSintactico
      ├── LL(1) → diagnósticos sintácticos
      └── VerificadorTipos.Verificar
               ├── TablaAmbitos / SimbolosPorToken
               ├── tipos / inicialización / retornos
               └── si no hay errores:
                      GeneradorCodigoIntermedio.Generar
                           │
                           ▼
                      ResultadoCodigoIntermedio
                      ├── Instrucciones: Cuadruplo[]
                      ├── Expresiones: ExpresionFuente[]
                      └── Errores
                           │
                           ▼
                      Form1.MostrarCodigoIntermedio
                           │
                           ├── ConstructorRepresentaciones.Construir
                           │     ├── Cuadruplos → Triplos
                           │     ├── Cuadruplos → Código P
                           │     └── Árboles fuente → Notaciones
                           │
                           ├── FormateadorCodigoIntermedio → texto de tres direcciones
                           ├── ConstructorRepresentaciones.TextoP → texto de Código P
                           └── publicar controles si no hubo errores
```

`Cuadruplo[]` y `ExpresionFuente[]` en el esquema significan colecciones, no que sus propiedades sean arreglos de C#: se exponen como `IReadOnlyList`.

## 3.3. Contratos importantes de entrada y salida

### Antes del generador

El verificador debe pertenecer al mismo análisis del que se tomaron los tokens. `TokensVerificados` y `SimbolosPorToken` usan las mismas posiciones. No ordenes, elimines o reemplaces tokens después de construir la tabla semántica.

La sintaxis tiene que ser válida antes de construir las pasadas semánticas que suponen delimitadores emparejados. El formulario respeta este orden; el chequeo LL(1) adicional del generador no arregla una llamada incorrecta a semántica realizada antes.

### Salida común

- `Instrucciones` conserva el orden del IR, no solo fórmulas aritméticas.
- `Expresiones` conserva árboles originales y contexto para las notaciones.
- Un error de generación devuelve instrucciones y expresiones vacías.
- Las colecciones se exponen con envolturas de solo lectura. Eso no hace inmutables los objetos de todos los nodos: `ExpresionIntermedia` tiene campos modificables. Los consumidores deben tratarlos como datos de lectura.

### Salida de representaciones

`ConstructorRepresentaciones.Construir` acepta el resultado común y produce Triplos, P, Notaciones y errores. Si una conversión produce `ErrorRepresentacion`, descarta las listas preparadas y devuelve un diagnóstico. No convierte automáticamente cualquier excepción de programación en un error semántico del usuario.

`MostrarCodigoIntermedio` limpia primero, prepara las conversiones y textos y después llena los controles. Es publicación conjunta de resultados esperados, no una transacción de base de datos ni una ejecución paralela.

## 3.4. Traza técnica del ejemplo común

Supón que `verificador` ya pasó semántica.

### Paso A: arranque

```csharp
var sesion = new Sesion(analisis);
sesion.Bloque(2, tokens.Count - 1, null);
```

Estado inicial: contadores cero, función actual nula, listas vacías. El constructor de la sesión llena `cierres`.

### Paso B: declaración vA

`Bloque → Asignar → Variable → ExpresionRango → Parsear` produce un literal ENT y el destino vA. `Registrar` guarda su expresión fuente; `Agregar` crea la copia.

```text
Instrucciones: [vA = 2]
Expresiones:   [Asignación de vA ← 2]
```

### Paso C: declaración vR

`Parsear` produce el árbol `+(vA, *(3,4))`. `Registrar` conserva ese árbol con destino vR. `Emitir` agrega primero la multiplicación y luego la suma.

```text
Nuevas instrucciones:
    t1 = 3 * 4
    vR = vA + t1

Nueva expresión fuente:
    Asignación de vR ← +(vA, *(3,4))
```

El árbol no se reemplaza por `vA + t1`: la vista de notaciones necesita seguir mostrando la expresión original.

### Paso D: condición y ramas

`Fuente` construye y registra `>(vR,10)`. `Emitir` crea t2. `Bloque` reserva L1 como rama falsa, traduce la verdadera, reserva L2 como final y traduce SINO. Las impresiones se registran como llamadas incorporadas en las expresiones fuente.

Resultado común:

| Destino | Fuente 1 | Fuente 2 | Operación |
| --- | --- | --- | --- |
| vA | 2 | — | = |
| t1 | 3 | 4 | * |
| vR | vA | t1 | + |
| t2 | vR | 10 | > |
| L1 | t2 | — | SALTO_FALSO |
| — | vR | — | IMP |
| L2 | — | — | SALTO |
| L1 | — | — | ETIQUETA |
| — | vA | — | IMP |
| L2 | — | — | ETIQUETA |

Hay **10 cuádruplos y 5 expresiones fuente**: dos asignaciones, la condición y las dos impresiones. Los subárboles y paréntesis internos no generan filas fuente extra.

### Paso E: conversión a Triplos

Cada copia simple conserva un registro. Cada una de las tres operaciones binarias produce tres registros: cargar acumulador, operar y copiar resultado. Las otras seis instrucciones conservan un registro cada una.

```text
1 copia + 3 operaciones × 3 registros + 6 controles/salidas = 16 triplos
```

El registro inicial de la comparación carga vR en r3; el siguiente cambia el tipo efectivo del acumulador a BOOL al comparar. El tipo tras cada registro está en `Triplo.TipoResultado`, no en el texto `r3`.

### Paso F: conversión a Código P

Se generan **23 instrucciones P** para este ejemplo. La copia inicial usa dos, cada operación binaria cuatro, el salto condicional dos, cada impresión dos, y cada etiqueta/salto incondicional una.

Ejemplo de la comparación:

```text
load vR
load 10
gt
store t2
load t2
jfalse L1
```

La pila queda vacía al terminar la comparación y vuelve a quedar vacía después de consumir la condición del salto.

### Paso G: notaciones y publicación

`FormateadorNotaciones` ordena las 5 expresiones por posición de token y produce tres cadenas para cada una. Por ejemplo, la condición queda:

```text
Infija:   (vR > 10)
Prefija:  > vR 10
Postfija: vR 10 >
```

La impresión verdadera queda:

```text
Infija:   IMP(vR)
Prefija:  call[IMP/1] vR
Postfija: vR call[IMP/1]
```

`Form1` publica 10 filas de cuádruplos, 16 de triplos, texto con 23 instrucciones P, 5 filas de notaciones y el texto de tres direcciones. Los comentarios de tipo no son instrucciones P adicionales.

## 3.5. Invariantes que no debes romper

### Identidad y posiciones

- Un nombre no identifica de manera única una declaración. Conserva los objetos `Simbolo` y sus IDs.
- Usa posiciones de token para resolver referencias, no solamente línea o texto.
- `Simbolo.Valor` contiene texto de asignaciones; no es memoria de ejecución ni una fuente adecuada para reconstruir el programa.
- Las líneas de `ExpresionFuente` se toman de la posición del nodo registrado; en expresiones multilínea pueden señalar el operador principal, no necesariamente la línea del tipo declarado.

### Tipos y operadores

- `OperadoresExpresion` comparte precedencia y normalización con semántica. El lexema reconocido tiene prioridad para evitar cambiar `<` a `<>` por una numeración incorrecta.
- Potencia asocia a la derecha. La negación tiene menos prioridad que comparación, según SharpC: `!2 < 3` significa `!(2 < 3)`.
- `&` y `|` evalúan ambos operandos de izquierda a derecha; las representaciones no añaden cortocircuito.
- `TipoResultado` puede diferir del tipo destino: `DEC vR = 5 / 2;` conserva división ENT y promoción posterior. No conviertas prematuramente esa operación en división decimal.

### Temporales, funciones y llamadas

- t1… son temporales del IR común; r1… son acumuladores de Triplos; L1… son etiquetas. No pertenecen a la tabla de declaraciones del usuario.
- Los temporales regulares se reutilizan solo sin alterar su tipo. Los acumuladores de Triplos tienen tipo efectivo por registro.
- Las llamadas pueden modificar variables visibles. Preserva `Capturar` y el orden de evaluación.
- Los argumentos se evalúan/capturan antes de emitir su grupo de preparación. Una llamada anidada no debe consumir los de una llamada exterior.
- La función se identifica por símbolo, no por texto ni posición de declaración. Los metadatos conservan función contenedora y padre léxico.
- Cada activación recursiva necesita sus propios temporales, acumuladores y argumentos. Una etiqueta de salto ordinario no cruza marcos.

### Representaciones

- La operación inicial vacía de un triplo es una carga al acumulador, no una instrucción desconocida.
- `PREPARAR_LLAMADA` conserva la aridad que no cabe junto con función y resultado en una sola fila de tres campos.
- En POS e IMPREP, los campos de Triplo contienen los dos argumentos; no todos los campos llamados objeto representan una escritura.
- El Código P actual es equivalente, no compacto. Conserva los temporales del IR y valida altura cero por cuádruplo.
- `arg` consume un valor de la pila de evaluación y lo guarda en el grupo de argumentos de la activación. Ese grupo es distinto de la pila de evaluación.
- Las notaciones describen expresiones fuente. No conviertas las etiquetas ni el programa completo en una cadena supuestamente prefija.

## 3.6. Errores: dónde pertenecen y dónde aparecen

| Etapa | Ejemplo | Resultado |
| --- | --- | --- |
| Léxico | Lexema rechazado por el AFD. | No se ejecutan las etapas siguientes; diagnóstico en Análisis. |
| Sintáctico | Falta un operando o delimitador. | No se construye semántica del programa; diagnóstico en Análisis. |
| Semántico | Tipo incompatible, uso sin inicializar o función sin retorno garantizado. | No se invoca generación desde el formulario; diagnóstico en Análisis. |
| Generación | ROMPER sin contenedor válido. | Se descarta la sesión publicada y se informa en Código intermedio. |
| Conversión | Operación sin traducción a Triplos/P. | Se descartan las representaciones preparadas y se informa representación y línea. |

No todas las comprobaciones de contexto se han trasladado a semántica: ROMPER sin contenedor y algunas comprobaciones defensivas continúan en generación. La comprobación de retorno garantizado sí está en semántica. Un nuevo error de significado debe añadirse allí, no ocultarse detrás de una instrucción de fallo en el IR.

Las capturas existentes son específicas de `ErrorGeneracion` y `ErrorRepresentacion`. Una excepción inesperada por un defecto de programación no queda automáticamente clasificada como error de usuario. Al ampliar una fase, conserva diagnósticos de dominio y no silencies excepciones perdiendo información.

## 3.7. Dónde modificar según lo que te pidan

| Cambio solicitado | Lugares principales |
| --- | --- |
| Cambiar solo cómo se ve una instrucción textual. | `FormateadorCodigoIntermedio.Instruccion`. |
| Cambiar nombres, columnas o ubicación de controles. | `Form1.Designer.cs` y publicación en `Form1.cs`. |
| Añadir o cambiar notaciones. | `FormateadorNotaciones`; si falta contexto, registro en el generador. |
| Cambiar cómo se crean cuádruplos de una instrucción. | Su rama en `Sesion.Bloque` o el método especializado. |
| Añadir una operación al IR. | Generador, formateador textual, Triplos, Código P, reglas compartidas si es binaria, pruebas y contrato. |
| Añadir una palabra o forma sintáctica al lenguaje. | ACT 1/3 como referencia, AFD si procede, tabla LL(1), semántica y todas las traducciones. |
| Añadir una regla de tipos o una prohibición semántica. | `VerificadorTipos` o su auxiliar semántico correspondiente. |
| Añadir ejecución real. | Nuevo intérprete/backend con marcos, memoria, pila y servicios; no lógica dentro del dgv. |

Una nueva instrucción debe mantener las representaciones equivalentes. Si faltan traducciones, los conversores actuales producen error; no la omitas silenciosamente.

## 3.8. Ruta práctica para depurar

Coloca puntos de interrupción en este orden:

1. `Form1.EjecutarVerificacionTipos`: comprobar errores y símbolos antes de generar.
2. `GeneradorCodigoIntermedio.Generar`: inspeccionar el verificador recibido.
3. Constructor `Sesion`: revisar `tokens` y `cierres`.
4. `Sesion.Bloque`: identificar la rama y el avance de `i`.
5. `Sesion.Asignar`, `Funcion`, `Seleccion` o `Incorporada`, según el caso.
6. `Sesion.Parsear`: revisar nodo izquierdo, operador, `minimo` y nodo derecho.
7. `Sesion.Registrar`: comprobar árbol, destino y contexto de la fila de notaciones.
8. `Sesion.Emitir` y `Agregar`: revisar temporales y la secuencia agregada.
9. `ConstructorRepresentaciones.Construir`: comparar qué recibe cada conversor.
10. `GeneradorTriplos.Generar` o `GeneradorCodigoP.Generar`: revisar expansión de una instrucción y tipos.
11. `FormateadorNotaciones.Formatear`: revisar el recorrido si una columna difiere.
12. `Form1.MostrarCodigoIntermedio`: comprobar listas preparadas y controles finales.

Si la tabla de cuádruplos ya está mal, cambiar el formateador no corrige la causa. Si los cuádruplos están bien pero una vista no, depura la conversión o presentación de esa vista.

## 3.9. Compilación y pruebas

Aplicación: Windows Forms, .NET Framework 4.7.2. Configuración comprobada: Debug/x64, con C# 7.3. Pruebas: ejecutable de consola .NET 10 que enlaza las clases reales de producción.

Desde la raíz, con el SDK de Windows instalado en este entorno:

```sh
"/mnt/c/Program Files/dotnet/dotnet.exe" run --project tests/Automatas.Pruebas/Automatas.Pruebas.csproj
```

```sh
"/mnt/c/Program Files/dotnet/dotnet.exe" msbuild Automatas/Automatas.csproj /t:Build /p:Configuration=Debug /p:Platform=x64 /v:minimal /nologo
```

La ejecución registrada antes de esta guía contiene **317 casos correctos**. Esta edición documental no representa una nueva ejecución de toda la batería.

Pruebas especialmente relevantes:

- [`PruebasCodigoIntermedio.cs`](../tests/Automatas.Pruebas/PruebasCodigoIntermedio.cs): asignaciones, precedencia, temporales y reinicio.
- [`PruebasGeneracionCompleta.cs`](../tests/Automatas.Pruebas/PruebasGeneracionCompleta.cs): control de flujo, funciones, recursión, incorporadas y orden de efectos.
- [`PruebasRetornos.cs`](../tests/Automatas.Pruebas/PruebasRetornos.cs): retorno garantizado conservador y bloqueo previo a generación.
- [`PruebasRepresentaciones.cs`](../tests/Automatas.Pruebas/PruebasRepresentaciones.cs): tres notaciones, Triplos, Código P, alias de operandos, pila y errores.

Los casos ejecutables comparan el resultado de distintas representaciones mediante un oráculo y adaptadores exclusivos de pruebas. Eso no añade ejecución al producto. Las pruebas de consola clasifican lexemas con un fixture: no prueban el AFD SQLite real ni la distribución visual del formulario. Esas integraciones se comprueban por separado.

Si creas un archivo de producción, agrégalo a [`Automatas.csproj`](../Automatas/Automatas.csproj), que tiene inclusiones explícitas. Si debe probarse desde consola, enlázalo también en [`Automatas.Pruebas.csproj`](../tests/Automatas.Pruebas/Automatas.Pruebas.csproj). Evita características que solo compile .NET 10 y no el proyecto de la aplicación.

## 3.10. Estado real y pendientes

Las cinco vistas están implementadas para la gramática actual. Siguen siendo fases diferentes la optimización, exportación completa, ejecución y generación para una plataforma concreta.

Detalles que el siguiente responsable debe conocer:

- `AnalizadorLexico` usa supuestos concretos sobre la matriz, el alfabeto y los estados; no se puede conectar cualquier lenguaje cambiando solo una base de datos.
- La ruta SQLite actual se configura en el código del formulario. Verifica el entorno antes de diagnosticar un problema como error del generador.
- Las incorporadas oficiales son LIMP, FONCOL, TXTCOL, SLTLN, POS, INTER, TECL e IMPREP, además de LEER e IMP. No vuelvas a introducir los nombres incorrectos que antes se asignaban a esos PR.
- INTER representa intercambio, no arreglos; se baja a tres copias y exige tipos iguales e inicialización.
- NUL significa ausencia de valor en ACT 1, pero su compatibilidad de tipos no está resuelta y el verificador aún lo agrupa con BOOL. Esa discrepancia no queda solucionada por tener cinco representaciones.
- No se valida todavía todo el rango de DEC ni el desbordamiento de resultados calculados. Las regiones son simbólicas, no reservas físicas.
- El árbol de expresión no es un AST completo del programa. Las instrucciones de control se recorren directamente desde rangos de tokens; el LL(1) continúa siendo reconocedor.
- El nombre `Expresion` que se ve en el generador es un alias de C#: `using Expresion = Automatas.ExpresionIntermedia;`. No es otra clase ni otro árbol distinto.

## 3.11. Referencias para continuar

- [Contrato de código intermedio](plan-codigo-intermedio.md): campos, operaciones, tipos, funciones y semántica de las representaciones.
- [Pruebas y cobertura](pruebas-analizadores.md): resultados y límites de la verificación.
- [Programas manuales](ejemplos-prueba.md): entradas para el formulario.
- [Reglas de escritura](reglas-lexicas.md): palabras reservadas y distinción de errores.
- [Presentación técnica](presentacion-analizador-sharpc-tecnica.md): detalles del análisis anterior a la síntesis.
- [Material de representaciones de clase](s_ntesis_de_c_digo_intermedio_y_representaciones.md): variante de triplos, Código P y notaciones.
- [ACT 1](ACT%201.%20definicion%20y%20caracteristicas%20%20ene-jun%202026-7.docx) y [ACT 3](ACT%203.%20DEFINICIONES%20REGULARES,%20COMPLEMENTO%20DE%20LA%20ACT%202%20ene-jun%202026.docx): referencias de significado, sintaxis y categorías del lenguaje.

> Resumen de mantenimiento: valida una vez, conserva símbolos y expresiones originales, baja a un IR común, traduce sin cambiar los efectos y publica las vistas únicamente cuando estén preparadas.
