# Crear Prefab Arrow - Guía Rápida

## Checklist Visual - Sigue estos pasos en orden

### ✅ Step 1: GameObject Base
```
- Crea un GameObject vacío: Right Click en Scene → 3D Object → Empty
- Nombra: "Arrow"
- Position: (0, 0, 0)
```

### ✅ Step 2: Modelo Visual (Opción más simple)
```
1. En "Arrow", crea un hijo: Right Click Arrow → 3D Object → Sphere
2. Escala la esfera: (0.2, 0.2, 1) para forma de flecha
3. Material: (opcional) Color diferente para verla bien
```

### ✅ Step 3: Rigidbody
```
1. Select "Arrow"
2. Add Component → Rigidbody
3. Propiedades:
   - Mass: 0.1
   - Drag: 0
   - Angular Drag: 0
   - Gravity: ☐ (DESACTIVADO)
   - Is Kinematic: ☐ (DESACTIVADO)
   - Collision Detection: Continuous (recomendado)
```

### ✅ Step 4: Collider (Trigger)
```
1. Select "Arrow"
2. Add Component → Capsule Collider
3. Propiedades:
   - Height: 1.2
   - Radius: 0.1
   - Is Trigger: ☑ (ACTIVADO) ← MUY IMPORTANTE
```

### ✅ Step 5: Script Arrow
```
1. Select "Arrow"
2. Add Component → Arrow (busca el script)
3. Propiedades en Inspector:
   - Speed: 12
   - Lifetime: 8
   - Impact VFXPrefab: (nada si no tienes)
```

### ✅ Step 6: Crear Prefab
```
1. En Project panel, navega a Assets/Prefab/
2. Arrastra "Arrow" desde Hierarchy a la carpeta Prefab
3. Aparecerá un nuevo archivo "Arrow.prefab"
4. Borra el "Arrow" de la Hierarchy (solo quedan copias en esta escena)
```

### ✅ Step 7: Asignar al Archer
```
1. Select el archer enemy en la escena
2. En Inspector, busca "EnemyRangedAttack"
3. Field "Projectile Prefab": Arrastra Assets/Prefab/Arrow.prefab
4. ¡Listo! Ya debería disparar
```

---

## Verificación Rápida

Abre Console (Window → General → Console) y:

1. **Play** (ejecuta la escena)
2. **Muévete cerca del archer**
3. **Mira la Console para estos mensajes:**
   ```
   [EnemyRangedAttack] ¡DISPARANDO!
   [Arrow] Instanciado.
   [Arrow] Lanzado.
   [Arrow] Colisión trigger con: Player
   [Arrow] ¡Golpeado jugador! Daño: X
   ```

Si ves todos estos mensajes: ✅ ESTÁ FUNCIONANDO

Si no ves "[Arrow] Instanciado": El prefab no se está creando → Revisar projectilePrefab en archer

Si ves "[Arrow] Instanciado" pero NO "[Arrow] Colisión": La flecha no colisiona → Revisar Is Trigger ☑

---

## Problema Común: "No veo la flecha"

✅ **Solución 1: Aumentar tamaño**
- Select Arrow en Hierarchy
- Scale: (1, 1, 2) en vez de (0.2, 0.2, 1)

✅ **Solución 2: Cambiar color**
- Arrow → Material → Color → Material rojo/naranja

✅ **Solución 3: Verificar posición spawn**
- En EnemyRangedAttack, verifica:
  - projectileSpawnPoint está en buena posición (frente al archer)
  - O está null (usa transform.position del archer)

✅ **Solución 4: Debug en tiempo real**
```
En EnemyRangedAttack.cs, pon esto en TryAttack():
Debug.Log($"Arrow position: {projectile.transform.position}");
```

---

## Video mental de lo que debería pasar:

1. El archer ve al jugador
2. El archer corre hacia el jugador hasta cierta distancia
3. El archer gira hacia el jugador
4. El archer dispara ← Aquí debería verse **una flecha saliendo**
5. La flecha vuela en línea recta
6. La flecha golpea al jugador (recibe daño)
7. La flecha desaparece

Si no ves paso 4: Revisar prefab Arrow
Si no ves daño en paso 6: Revisar Is Trigger y PlayerHealth component
