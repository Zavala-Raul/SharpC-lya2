# Diagrama de flujo del analizador sintactico LL(1)

Este diagrama representa el metodo principal del analizador sintactico descendente predictivo LL(1).

Nota: si lo pegas en Mermaid Live Editor, pega solo el contenido de adentro del bloque, sin las lineas ```mermaid y ```.

```mermaid
graph TD
    A([Inicio]) --> B["Recibir lista de tokens"]
    B --> C["Agregar EOF al final"]
    C --> D["Crear pila"]
    D --> E["Meter EOF y simbolo inicial S"]
    E --> F{"La pila esta vacia?"}

    F -- Si --> G([Fin])
    F -- No --> H["Tomar cima de pila y token actual"]

    H --> I{"Cima = EOF y token = EOF?"}
    I -- Si --> J["Cadena aceptada"]
    J --> G

    I -- No --> K{"La cima es terminal?"}

    K -- Si --> L{"Cima = token actual?"}
    L -- Si --> M["Sacar cima de la pila"]
    M --> N["Avanzar al siguiente token"]
    N --> F

    L -- No --> O["Registrar error: falta terminal esperado"]
    O --> P["Sacar cima para continuar"]
    P --> F

    K -- No --> Q{"Existe regla en tabla M?"}
    Q -- Si --> R["Sacar no terminal de la pila"]
    R --> S["Meter produccion en orden inverso"]
    S --> F

    Q -- No --> T["Registrar error: token inesperado"]
    T --> U["Saltar hasta CE8, CE4 o EOF"]
    U --> V["Limpiar pila hasta INS, S o EOF"]
    V --> F
```

## Explicacion corta

1. El analizador recibe una cadena de tokens.
2. Agrega `EOF` para saber donde termina la entrada.
3. Inicia la pila con `EOF` y el simbolo inicial `S`.
4. Mientras la pila no este vacia, compara la cima de la pila con el token actual.
5. Si la cima es terminal y coincide, hace `match`.
6. Si la cima es no terminal, busca una produccion en la tabla M.
7. Si encuentra regla, expande la produccion en la pila.
8. Si no encuentra regla, registra error y se recupera saltando a un punto seguro.

## Ejemplo de condicion

Condicion:

```gatosabe
vEdad >= 18 & vActivo == VDD
```

Tokens:

```txt
IDV OPR3 CNU OPL1 IDV OPR6 PR20 EOF
```

La cadena se acepta si al final la pila queda en `EOF` y el token actual tambien es `EOF`.
