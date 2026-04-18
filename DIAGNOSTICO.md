# 🔍 DIAGNÓSTICO: Enemigos solo hacen Idle

## PASO 1: Añadir script de diagnóstico

1. **Abre un Enemy en el prefab** (`Prefab/Enemy.prefab`)
2. **En el Inspector**, Click en "Add Component"
3. **Busca y añade** `AnimatorDebugger`
4. **Presiona Play**

## PASO 2: Revisar la consola

Deberías ver mensajes como:

```
✅ Enemy: Animator encontrado
   Controller asignado: NewEnemy_Animator
   Parámetros disponibles:
     - isWalking (Bool)
     - isAttacking (Bool)
     - Death (Trigger)
```

**Si ves esto**, los parámetros están bien. Si NO ves esto, el problema es el Animation Controller.

---

## PASO 3: Checklist de diagnóstico

Durante el juego, presiona **D** para imprimir el estado del animator:

```
=== ESTADO ACTUAL DE Enemy ===
Estado actual: 123456789
  isWalking = false
  isAttacking = false
```

### Problema 1: `isWalking` nunca cambia a `true`

**Causa probable**: 
- El `detectionRange` es demasiado pequeño
- El enemigo no puede detectar al player

**Solución**:
1. En el Inspector del Enemy, abre EnemyMeleeAttack
2. Aumenta `detectionRange` a 15 o 20
3. Vuelve a probar

---

### Problema 2: `isWalking` cambia pero la animación no cambia

**Causa probable**:
- Las **transiciones en el Animation Controller no están configuradas**
- Los nombres de parámetros **no coinciden**
- El estado **Idle no está como Default** (debe ser naranja)

**Solución**:
1. Abre el Animation Controller (`Enemy_NewModel/NewEnemy_Animator`)
2. Doble-click para abrir el editor
3. Verifica que existan estos EXACTAMENTE:
   - ✅ Parámetro: `isWalking` (Bool)
   - ✅ Parámetro: `isAttacking` (Bool)
   - ✅ Parámetro: `Death` (Trigger)
4. Verifica que existan estas TRANSICIONES:
   - ✅ Idle → Walk (cuando `isWalking == true`)
   - ✅ Walk → Idle (cuando `isWalking == false`)
   - ✅ Idle → Attack (cuando `isAttacking == true`)
   - ✅ Attack → Idle (cuando `isAttacking == false`)

---

### Problema 3: Animator dice `isWalking = true` pero sigue en Idle

**Causa probable**:
- La **transición de Idle → Walk NO EXISTE** o está MAL configurada
- El `Exit Time` está muy alto

**Solución**:
1. En el Animation Controller, haz click en la **transición de Idle → Walk**
2. En el Inspector (panel derecho), verifica:
   ```
   Conditions:
   - isWalking == true ✅
   
   Settings:
   - Exit Time: 0 (o muy bajo)
   - Has Exit Time: DESACTIVADO
   - Transition Duration: 0.1
   ```

---

## PASO 4: Solución rápida si las transiciones no funcionan

Si las transiciones están pero no funcionan, prueba esto:

**Option A: Usar triggers en lugar de bools**

Cambia el script a usar triggers en lugar de bools. Abre `EnemyMeleeAttack.cs` y prueba esto:

```csharp
void SetAnimationState(string paramName, bool value)
{
    if (animator != null)
    {
        // Intenta como trigger primero
        if (paramName == "Death")
        {
            animator.SetTrigger(paramName);
        }
        else
        {
            animator.SetBool(paramName, value);
        }
        Debug.Log($"🎬 {gameObject.name}: {paramName} = {value}");
    }
}
```

---

## PASO 5: Si nada funciona - Crear controller desde cero

1. **Elimina** `NewEnemy_Animator.controller`
2. Click derecho en `Enemy_NewModel` → Create → Animator Controller
3. Nombra: `EnemyController_Fresh`
4. **Abre la consola mientras juegas y presiona D**
5. Anota los nombres exactos de parámetros que muestra
6. Crea el controller manualmente con ESOS nombres exactos

---

## Debug Commands

Mientras el juego está en Play:
- **Presiona D**: Imprime estado del Animator
- **Mira la consola**: Deberías ver logs como:
  - `📍 Enemy: Acercándose`
  - `⚔️ Enemy: En rango de ataque`
  - `🎬 Enemy: Parámetro 'isWalking' = true`

Si NO ves estos logs, el enemigo nunca entra en los rangos de distancia.

---

## Preguntas que debes responder:

1. ¿Ves los logs de "Animator encontrado" al iniciar?
2. ¿Se cambian los parámetros (ves logs de "Parámetro 'isWalking' = true")?
3. ¿Qué distancia muestra cuando presionas D? (¿es mayor que `detectionRange`?)
4. ¿El Animation Controller tiene los parámetros correctos?

---

**Ejecuta este diagnóstico y cuéntame qué ves en la consola.** Así podré identificar exactamente dónde está el problema. 🎯
