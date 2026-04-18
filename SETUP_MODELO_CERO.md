# 🚀 IMPORTAR MODELO Y ANIMACIONES - DESDE CERO

## PASO 1: Preparar la carpeta

1. En Unity, en la carpeta `Assets`, crea una **nueva carpeta**: `Enemy_Model`
2. Esta carpeta contendrá todo: modelo, animaciones, animator controller

---

## PASO 2: Importar tu modelo

1. **Descarga tu modelo** (FBX con animaciones)
2. **Colócalo** en la carpeta `Assets/Enemy_Model`
3. **En Unity**, selecciona el archivo FBX
4. **En el Inspector**, ajusta:
   - **Animation Type**: Humanoid (o Generic si no es humanoid)
   - **Avatar**: Auto-generate o crea uno
   - **Import Animation**: ON ✅

---

## PASO 3: Separar las animaciones (MUY IMPORTANTE)

Si tu FBX tiene **TODAS las animaciones en 1 archivo**, necesitas separarlas en clips:

1. **Selecciona el FBX** en el Project
2. **En el Inspector**, ve a **"Animation"** tab (a la derecha)
3. **Abre la lista** de animaciones (Animation Clips)
4. **Crea un clip para cada animación**:
   - Nombre: `Idle`, Frames: 0-100 (ej, ajusta a tu modelo)
   - Nombre: `Walk`, Frames: 101-200
   - Nombre: `Attack`, Frames: 201-300
   - Nombre: `Death`, Frames: 301-400

5. **Guarda** (los clips se crearán automáticamente en la carpeta)

---

## PASO 4: Crear el Animation Controller

1. Click derecho en `Enemy_Model` → **Create → Animator Controller**
2. Nombre: `EnemyAnimator`
3. **Doble-click** para abrirlo

### 4a. Crear parámetros
En el panel izquierdo, click en **+**:
- Add Bool Parameter → `isWalking`
- Add Bool Parameter → `isAttacking`
- Add Trigger Parameter → `Death`

### 4b. Crear estados
En el canvas central, arrastra cada clip de animación:
1. **Idle** (default, naranja) - clic derecho, "Set as Layer Default State"
2. **Walk**
3. **Attack**
4. **Death**

### 4c. Crear transiciones
Haz flechas entre estados:

**Idle → Walk**
- Click en Idle → Arrastra a Walk
- En el Inspector, Conditions: `isWalking == true`
- Has Exit Time: OFF

**Walk → Idle**
- Click en Walk → Arrastra a Idle
- Conditions: `isWalking == false`
- Has Exit Time: OFF

**Idle → Attack**
- Click en Idle → Arrastra a Attack
- Conditions: `isAttacking == true`
- Has Exit Time: OFF

**Attack → Idle**
- Click en Attack → Arrastra a Idle
- Conditions: `isAttacking == false`
- Has Exit Time: OFF
- Exit Time: 0.8

**AnyState → Death**
- Click derecho en "AnyState" → Make Transition
- Selecciona Death
- Conditions: `Death` (trigger)
- Has Exit Time: OFF

---

## PASO 5: Actualizar el prefab Enemy

1. **Abre**: `Assets/Prefab/Enemy.prefab`
2. **Busca** el objeto del modelo actual
3. **Reemplaza** o actualiza:
   - La **malla** (Mesh) con la del nuevo modelo
   - El **Animator Controller** con `EnemyAnimator`

---

## PASO 6: Verificar y Probar

En el prefab Enemy:
1. Busca `EnemyMeleeAttack` (script)
2. En el campo **Animator**, asigna el Animator del enemy
3. **Guarda** (Ctrl+S)
4. **Play** y prueba

---

## Checklist

- [ ] Carpeta `Enemy_Model` creada
- [ ] Modelo FBX importado
- [ ] Animaciones separadas en clips (Idle, Walk, Attack, Death)
- [ ] Animation Controller creado con parámetros
- [ ] Estados creados y asignados clips
- [ ] Transiciones configuradas
- [ ] Prefab actualizado
- [ ] Animator asignado en EnemyMeleeAttack
- [ ] ¡Probado y funciona!

---

## 🎯 El Script ya está listo

El `EnemyMeleeAttack.cs` **ya está actualizado** y:
- ✅ Detecta el Animator automáticamente
- ✅ Control: `isWalking` (bool) y `isAttacking` (bool)
- ✅ Trigger: `Death`
- ✅ Estados: Idle → Walk → Attack → Death

**No necesitas cambiar nada del script.** Solo importar el modelo y crear el controller.

---

¿Necesitas ayuda en algún paso específico? Cuéntame dónde estás. 🚀
