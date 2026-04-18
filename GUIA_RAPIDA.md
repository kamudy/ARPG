# 🚀 GUÍA RÁPIDA - CAMBIAR MODELO Y ANIMACIONES

## EN UNITY (Editor)

### PASO 1: Preparar carpeta
```
Assets → Click Derecho → New Folder → "Enemy_NewModel"
```

### PASO 2: Importar modelo y clips
- Arrastra tu **modelo.fbx** a la carpeta `Enemy_NewModel`
- Unity importará automáticamente los clips de animación
- Si tienes clips separados, también cópialos a esta carpeta

### PASO 3: Crear Animation Controller
```
Enemy_NewModel folder → Click Derecho → Create → Animator Controller → "NewEnemy_Animator"
```

### PASO 4: Configurar el Controller (IMPORTANTE)
Doble-click en `NewEnemy_Animator` para abrirlo:

#### 4a. Crear Parámetros (ventana inferior izquierda)
- Click en **+** → Add Bool Parameter → nombre: `isWalking`
- Click en **+** → Add Bool Parameter → nombre: `isAttacking`
- Click en **+** → Add Trigger Parameter → nombre: `Death`

#### 4b. Crear Estados
En el canvas del Animator:
1. **Click derecho** en el canvas → Create State → From New Blend Tree → nombre: `Idle`
   - Arrastra el clip `Idle` a este estado
   - Este debe ser el **Default** (color naranja)

2. **Click derecho** → Create State → From New Blend Tree → nombre: `Walk`
   - Arrastra el clip `Walk` a este estado

3. **Click derecho** → Create State → From New Blend Tree → nombre: `Attack`
   - Arrastra el clip `Attack` a este estado

4. **Click derecho** → Create State → From New Blend Tree → nombre: `Death`
   - Arrastra el clip `Death` a este estado

#### 4c. Crear Transiciones
```
Idle → Walk:
  • Click en Idle → Haz una flecha hacia Walk
  • En Inspector: Condición = "isWalking" == true

Walk → Idle:
  • Click en Walk → Flecha hacia Idle
  • Condición = "isWalking" == false

Idle → Attack:
  • Click en Idle → Flecha hacia Attack
  • Condición = "isAttacking" == true

Attack → Idle:
  • Click en Attack → Flecha hacia Idle
  • Condición = "isAttacking" == false
  • Exit time: 0.8 (deja que termine la animación)

Any State → Death:
  • Click derecho en "Any State" → Make Transition
  • Flecha hacia Death
  • Condición = "Death" (trigger)
```

### PASO 5: Actualizar Prefab Enemy
```
Prefab/Enemy.prefab → Click derecho → Open Prefab
```

1. **En la jerarquía del prefab**, busca el objeto con el modelo goblin
2. **Elimina o reemplaza**:
   - El mesh renderer del goblin antiguo
   - O simplemente arrastra tu nuevo modelo como hijo
3. **Busca el componente Animator** (en el padre o donde esté)
4. En el Inspector, asigna `NewEnemy_Animator` al campo **Controller**
5. **Guarda** (Ctrl+S)

### PASO 6: Verificar EnemyMeleeAttack
En el prefab Enemy:
1. Selecciona el objeto que tiene **EnemyMeleeAttack**
2. En el Inspector, verifica que el campo **Animator** esté asignado
3. Si está vacío, asígna manualmente el Animator del enemigo
4. (O déjalo vacío: el script lo detectará automáticamente)

### PASO 7: TEST
```
Play → Acércate al enemigo → Debería hacer idle, luego walk, luego attack, luego muerte
```

---

## 📋 CHECKLIST

- [ ] Modelo importado en `Enemy_NewModel`
- [ ] Animation Controller creado (`NewEnemy_Animator`)
- [ ] Parámetros creados: `isWalking`, `isAttacking`, `Death`
- [ ] Estados creados: Idle, Walk, Attack, Death
- [ ] Transiciones configuradas correctamente
- [ ] Prefab Enemy actualizado con el nuevo modelo
- [ ] Animator Controller asignado en el prefab
- [ ] Probado en juego

---

## ❌ Si no funciona

### Animaciones no reproducen:
- [ ] Verifica que los parámetros se llamen **exactamente igual**
- [ ] Revisa que el Animator está asignado en EnemyMeleeAttack
- [ ] Abre la consola (Ctrl+Shift+C) y busca errores

### Enemigo no se mueve:
- [ ] El script de movimiento sigue igual, solo se añadieron animaciones
- [ ] Si el enemigo no se acerca, revisa `detectionRange` en el Inspector

### Modelo no aparece:
- [ ] Asegúrate de que importaste el modelo con materials
- [ ] Verifica la escala (puede estar muy grande o muy pequeño)
- [ ] Comprueba que el modelo tiene rig/avatar asignado

---

¡Éxito! 🎮
