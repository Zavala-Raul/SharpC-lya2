# Analizador sintactico LL(1) para COND

Este documento resume la logica que se puede pasar a PDF para la evidencia del analizador sintactico de condiciones de GatoSabe.

## Tipo de analizador

Analizador sintactico descendente predictivo LL(1), tambien conocido como top-down LL(1).

Entrada:
- Gramatica de condicion.
- Cadena de tokens, por ejemplo: `IDV OPR3 CNU OPL1 IDV OPR6 PR20 EOF`.

Salida:
- Acepta la cadena si la pila y la entrada llegan juntas a `EOF`.
- Reporta error sintactico si no existe produccion en la tabla M o si el terminal esperado no coincide.

## Gramatica usada para condiciones

```txt
COND       -> COND_OR
COND_OR    -> COND_AND COND_OR_P
COND_OR_P  -> OPL2 COND_AND COND_OR_P | e
COND_AND   -> COND_NOT COND_AND_P
COND_AND_P -> OPL1 COND_NOT COND_AND_P | e
COND_NOT   -> OPL3 COND_NOT | COND_REL
COND_REL   -> EXP REL_OPC | CE1 COND CE2
REL_OPC    -> OPR1 EXP | OPR2 EXP | OPR3 EXP | OPR4 EXP | OPR5 EXP | OPR6 EXP | e
```

La condicion usa `EXP`, por eso el modulo de condicion tambien necesita la gramatica de expresiones.

```txt
EXP    -> TERM EXP_P
EXP_P  -> OPA+ TERM EXP_P | OPA- TERM EXP_P | e
TERM   -> POT TERM_P
TERM_P -> OPA* POT TERM_P | OPA/ POT TERM_P | e
POT    -> VALOR POT_P
POT_P  -> OPA^ POT | e
VALOR  -> IDV | CNU | CAD | CAR | PR20 | PR21 | PR22 | CALL_FUNC | CE1 EXP CE2
```

## Diagrama de flujo

```mermaid
flowchart TD
    A["Inicio"] --> B["Recibir cadena de tokens"]
    B --> C["Agregar EOF a la entrada"]
    C --> D["Inicializar pila: EOF, S"]
    D --> E{"Pila vacia?"}
    E -- "Si" --> F["Fin"]
    E -- "No" --> G["Leer cima de pila y token actual"]
    G --> H{"cima == EOF y token == EOF?"}
    H -- "Si" --> I["Cadena aceptada"]
    H -- "No" --> J{"La cima es terminal?"}
    J -- "Si" --> K{"Terminal == token actual?"}
    K -- "Si" --> L["Pop de pila y avanzar token"]
    L --> E
    K -- "No" --> M["Reportar omision del terminal esperado"]
    M --> N["Pop de pila para recuperacion"]
    N --> E
    J -- "No" --> O{"Existe M[cima, token]?"}
    O -- "Si" --> P["Pop del no terminal"]
    P --> Q["Insertar produccion en pila en orden inverso"]
    Q --> E
    O -- "No" --> R["Reportar token inesperado"]
    R --> S["Avanzar hasta CE8, CE4 o EOF"]
    S --> T["Limpiar pila hasta INS, INS_CASO, S o EOF"]
    T --> E
```

## Ejemplo de recorrido

Condicion:

```gatosabe
vEdad >= 18 & vActivo == VDD
```

Tokens:

```txt
IDV OPR3 CNU OPL1 IDV OPR6 PR20 EOF
```

Resumen:
1. `S` deriva a `COND`.
2. `COND` deriva a `COND_OR`.
3. La primera comparacion `IDV OPR3 CNU` se reconoce como `EXP REL_OPC`.
4. `OPL1` activa `COND_AND_P`.
5. La segunda comparacion `IDV OPR6 PR20` se reconoce igual.
6. Al llegar a `EOF`, la pila tambien queda en `EOF`, por lo tanto se acepta.
