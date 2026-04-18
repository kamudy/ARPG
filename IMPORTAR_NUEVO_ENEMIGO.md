# Guía: Cambiar Modelo y Animaciones del Enemigo

## ✅ PASO 1: Preparar la carpeta para el nuevo modelo

1. En Unity, en la carpeta **Assets**, crea una **nueva carpeta** llamada `Enemy_NewModel` (o el nombre que quieras)
   - Puedes copiar la estructura de `Enemy_Goblin` como referencia

## ✅ PASO 2: Importar el modelo

1. **Coloca tu archivo modelo** (FBX, GLTF, etc.) en la carpeta `Enemy_NewModel`
2. **Selecciona el archivo** en el Project
3. En el Inspector, ajusta los parámetros de importación:
   - **Model**: Asegúrate que está en modo "Humanoid" o "Generic" (según el rig del modelo)
   - **Materials**: Crea materiales o asigna los existentes
   - **Avatar**: Si es Humanoid, Unity creará el avatar automáticamente
   - **Animations**: Marca la casilla "Import Animation" si las animaciones vienen en el FBX

## ✅ PASO 3: Separar las animaciones (si vienen en un solo FBX)

Si tu modelo tiene todas las animaciones en UN SOLO archivo, necesitas separarlas:

1. **Selecciona el archivo FBX** en el Project
2. En el Inspector, ve a la pestaña **Animations**
3. **Abre la sección de animaciones** (clip list)
4. **Crea clips para cada animación**:
   - **Idle**: Frame inicial → frame final (ej: 0-30)
   - **Walk**: Frame inicial → frame final (ej: 31-60)
   - **Attack**: Frame inicial → frame final (ej: 61-90)
   - **Death**: Frame inicial → frame final (ej: 91-120)

5. **Dale un nombre descriptivo** a cada clip (idle, walk, attack, death)
6. Aplica los cambios

## ✅ PASO 4: Crear el Animation Controller

1. **Botón derecho** en la carpeta `Enemy_NewModel`
2. **Create → Animator Controller**
3. Nombre: `NewEnemy_Controller`
4. **Abre el controller** (doble clic)

### En el Animator Editor:

#### Crear 2 parámetros:
1. **Parámetro 1**: Nombre = `isWalking`, Tipo = Bool (unchecked por defecto)
2. **Parámetro 2**: Nombre = `isAttacking`, Tipo = Bool (unchecked por defecto)
3. **Parámetro 3**: Nombre = `Death`, Tipo = Trigger

#### Crear Estados (drag & drop las animaciones):
1. **Crea un estado "Idle"**: Arrastra el clip `idle` (default state, color naranja)
2. **Crea un estado "Walk"**: Arrastra el clip `walk`
3. **Crea un estado "Attack"**: Arrastra el clip `attack`
4. **Crea un estado "Death"**: Arrastra el clip `death` (marcar como exit time = false)

#### Crear Transiciones:
- **Idle → Walk**: Condición = `isWalking` == true
- **Walk → Idle**: Condición = `isWalking` == false
- **Idle → Attack**: Condición = `isAttacking` == true
- **Attack → Idle**: Condición = `isAttacking` == false (exit time: ~0.8)
- **Cualquier estado → Death**: Condición = `Death` trigger

## ✅ PASO 5: Actualizar el Prefab Enemy

1. **Abre el prefab** `Prefab/Enemy.prefab`
2. **Busca el objeto hijo** que contiene el modelo (normalmente llamado igual que el modelo)
3. **Reemplaza el modelo**:
   - Selecciona el objeto hijo
   - En el Inspector, busca el componente `SkinnedMeshRenderer` o `MeshRenderer`
   - Reemplaza la mesh con la del nuevo modelo
4. **Actualiza el Animator**:
   - Selecciona el objeto que tiene el componente `Animator`
   - En el Inspector, asigna el nuevo `NewEnemy_Controller` al campo "Controller"
5. **Guarda el prefab** (Ctrl+S)

## ✅ PASO 6: Verificar la asignación en EnemyMeleeAttack

1. **Selecciona el Enemy prefab** en `Prefab/Enemy.prefab`
2. **Busca el componente `EnemyMeleeAttack`**
3. En el Inspector, asigna el **Animator** (o déjalo vacío para que se autodetecte)
4. **Guarda** el prefab

## ✅ PASO 7: Probar en juego

1. **Abre la escena** con enemigos
2. **Presiona Play**
3. **Acércate al enemigo** - debe hacer walking
4. **Entra en rango de ataque** - debe atacar
5. **Mata el enemigo** - debe hacer death animation

---

## 🔧 Notas Importantes

- **Nombres de parámetros**: Los nombres deben ser exactamente:
  - `isWalking`
  - `isAttacking`
  - `Death` (trigger)

- **Escala y Posición**: Si el modelo es demasiado grande/pequeño, ajusta la escala en el prefab
  
- **Rotación**: Algunos modelos vienen rotados. Usa el componente `Transform` para ajustar

- **Loop**: Las animaciones de "idle" y "walk" deben estar en **loop** (en la importación del FBX)
  
- **Attack y Death**: Estas NO deben estar en loop

## ❓ Si algo no funciona

1. Verifica que el **Animator está asignado** en EnemyMeleeAttack
2. Comprueba que los **parámetros del controller** se llamen exactamente igual
3. Prueba a hacer manualmente una transición en el Animator para ver si funciona
4. Revisa la consola de Unity por errores de scripts

---

¡Listo! Tu nuevo enemigo debería funcionar con sus animaciones. 🎮
