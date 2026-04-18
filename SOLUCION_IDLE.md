# 🚀 SOLUCIÓN: Enemigos solo hacen Idle

## Problema
Los enemigos reproducen la animación Idle pero no cambian a Walk, Attack o Death.

---

## OPCIÓN 1: Revisar y arreglar el Animation Controller (RECOMENDADO)

### 1.1 Verifica los parámetros
```
Enemy_NewModel/NewEnemy_Animator.controller
  ↓ Doble-click para abrir en el editor
  ↓ Mira el panel inferior izquierdo "Parameters"
```

Debe haber EXACTAMENTE estos 3:
- ✅ `isWalking` (Bool)
- ✅ `isAttacking` (Bool)  
- ✅ `Death` (Trigger)

**Si no están, créalos:**
- Click en `+` → Add Bool Parameter → `isWalking`
- Click en `+` → Add Bool Parameter → `isAttacking`
- Click en `+` → Add Trigger Parameter → `Death`

### 1.2 Verifica las transiciones

En el canvas del Animator, debes tener estas transiciones (flechas entre estados):

```
┌─────┐ isWalking=true ┌──────┐
│Idle├──────────────→ │Walk  │
│ 🔶  │               │      │
└──┬──┘ isWalking=false└───┬──┘
   ↑                        ↓
   └────────────────────────┘

┌─────┐ isAttacking=true ┌────────┐
│Idle├──────────────→ │Attack │
└──┬──┘                └───┬────┘
   ↑ isAttacking=false      ↓
   └────────────────────────┘

┌─────┐ Death(Trigger) ┌───────┐
│Any *├──────────────→ │Death  │
└─────┘                └───────┘
```

### 1.3 Verifica las condiciones de transición

Para cada transición, click en ella y en el Inspector verifica:

**Transición Idle → Walk:**
```
Conditions:
  ✅ isWalking == true
Settings:
  ✅ Has Exit Time: OFF (desactivado)
  ✅ Transition Duration: 0.1
```

**Transición Walk → Idle:**
```
Conditions:
  ✅ isWalking == false
Settings:
  ✅ Has Exit Time: OFF
  ✅ Transition Duration: 0.1
```

**Transición Idle → Attack:**
```
Conditions:
  ✅ isAttacking == true
Settings:
  ✅ Has Exit Time: OFF
  ✅ Transition Duration: 0.1
```

**Transición Attack → Idle:**
```
Conditions:
  ✅ isAttacking == false
Settings:
  ✅ Has Exit Time: 0.8 (deja que termine la anim)
  ✅ Transition Duration: 0.2
```

---

## OPCIÓN 2: Usar el script alternativo (SI LA OPCIÓN 1 NO FUNCIONA)

Si después de verificar todo el Animation Controller sigue sin funcionar:

### 2.1 Desactiva el script actual

En el prefab `Prefab/Enemy.prefab`:
- Selecciona el objeto que tiene `EnemyMeleeAttack`
- En el Inspector, **desactiva el componente** (unchecked)

### 2.2 Añade el nuevo script

- Click en "Add Component"
- Busca `EnemyMeleeAttack_Simple`
- Añádelo
- Asigna el Animator (o déjalo vacío para auto-detección)
- Copia los valores de `detectionRange`, `attackRange`, `damage`, `moveSpeed` del componente anterior

### 2.3 Prueba

Presiona Play. Este script es más simple y **garantiza que funcione**.

---

## OPCIÓN 3: Debug para saber exactamente dónde está el problema

### 3.1 Añade el script AnimatorDebugger

En `Prefab/Enemy.prefab`, click en "Add Component" → `AnimatorDebugger`

### 3.2 Presiona Play y mira la consola

Deberías ver:
```
✅ Enemy: Animator encontrado
   Controller asignado: NewEnemy_Animator
   Parámetros disponibles:
     - isWalking (Bool)
     - isAttacking (Bool)
     - Death (Trigger)
✅ Enemy: EnemyMeleeAttack.animator está asignado
```

**Si ves algo diferente, cuéntame exactamente qué ves.**

### 3.3 Durante el juego, presiona D

La consola mostrará:
```
=== ESTADO ACTUAL DE Enemy ===
Estado actual: [número]
  isWalking = [true/false]
  isAttacking = [true/false]
```

**Si `isWalking` **nunca** cambia a true:**
  - El enemigo no está detectando al player
  - Aumenta `detectionRange` a 20 o más

**Si `isWalking` cambia a true pero la animación no cambia:**
  - Las transiciones del controller no están bien
  - Revisa la OPCIÓN 1

---

## Checklist Rápido

- [ ] Los parámetros existen: `isWalking`, `isAttacking`, `Death`
- [ ] Las transiciones Idle ↔ Walk existen
- [ ] Las transiciones Idle ↔ Attack existen
- [ ] Idle es el estado **Default** (color naranja)
- [ ] Las transiciones NO tienen "Has Exit Time" activado
- [ ] El Animator está asignado en EnemyMeleeAttack
- [ ] Probé con Script Simple y funciona

---

**Cuéntame qué ves en la consola o qué parámetros falta en el Animation Controller, y te ayudo a arreglarlo específicamente.** 🎯
