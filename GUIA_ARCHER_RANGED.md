# Sistema de Ataque a Rango para Archer Enemy

## Descripción
Este sistema permite crear enemigos arqueros (archers) que disparan flechas al jugador manteniéndose a distancia.

## Scripts Creados

### 1. **EnemyRangedAttack.cs**
Script principal que controla el comportamiento del archer:
- Detección del jugador en rango
- Movimiento inteligente (mantiene distancia)
- Disparo de proyectiles con cooldown
- Gestión de estados (Idle, Walking, Attacking, Dead)
- Integración con animador del archer

**Parámetros configurables:**
- `detectionRange`: Rango de detección del jugador (default: 8)
- `attackRange`: Rango máximo de disparo (default: 6)
- `stopDistance`: Distancia mínima a mantener del jugador (default: 3)
- `damage`: Daño por flecha (default: 8)
- `attackCooldown`: Tiempo entre disparos (default: 2s)
- `moveSpeed`: Velocidad de movimiento (default: 2)
- `projectilePrefab`: Prefab de la flecha a disparar (REQUERIDO)
- `projectileSpawnPoint`: Transform donde salen las flechas (opcional)

### 2. **Arrow.cs**
Script que controla el comportamiento del proyectil (flecha):
- Movimiento en línea recta hacia el objetivo
- Detección de colisión con el jugador
- Cálculo de daño con defensa
- Generación de VFX de impacto
- Auto-destrucción después de cierto tiempo

## Pasos para Configurar en Unity

### Paso 1: Crear el Prefab de la Flecha

1. **Crea un GameObject vacío** en tu escena llamado "Arrow"

2. **Añade un modelo visual** (elige uno):
   - Opción A: Crear un cilindro simple
     - Crea un Cube como hijo
     - Escala: X=0.1, Y=0.1, Z=1 (forma de flecha)
     - Material: Color rojo o naranja
   - Opción B: Usar un modelo 3D si tienes

3. **Añade un Collider**:
   - Click derecho en Arrow → Add Component → Capsule Collider
   - Configura:
     - **Height**: 1.2
     - **Radius**: 0.1
     - **Is Trigger**: ACTIVO ✓ (importante para detectar jugador)
   
4. **Añade un Rigidbody**:
   - Add Component → Rigidbody
   - Configura:
     - **Mass**: 0.1
     - **Drag**: 0
     - **Angular Drag**: 0
     - **Gravity**: DESACTIVO ✗
     - **Is Kinematic**: DESACTIVO ✗ (el script lo controlará)
     - **Collision Detection**: Continuous (para velocidades altas)

5. **Añade el Script**:
   - Add Component → Arrow (el script que creamos)
   - Propiedades:
     - `speed`: 12
     - `lifetime`: 8
     - `impactVFXPrefab`: (opcional) Arrastra ImpactVFX.prefab si lo tienes

6. **Guarda como Prefab**:
   - En Project panel, crea carpeta Assets/Prefab si no existe
   - Arrastra tu Arrow desde Hierarchy a Assets/Prefab/
   - Nombre: **Arrow.prefab**
   - **Borra la copia de la escena** (la que está en Hierarchy)

### Paso 2: Crear el Prefab del Archer

1. Si ya existe un prefab de Enemy, duplicalo o crea uno nuevo llam "ArcherEnemy"
2. Asigna el modelo del archer (goblin_archer)
3. Reemplaza el componente **EnemyMeleeAttack** con **EnemyRangedAttack**
4. Configura EnemyRangedAttack:
   - `detectionRange`: 8
   - `attackRange`: 6
   - `stopDistance`: 3
   - `damage`: 8
   - `attackCooldown`: 2
   - `moveSpeed`: 2
   - `animator`: Asigna el Animator del archer
   - `projectilePrefab`: Arrastra el Arrow.prefab
   - `projectileSpawnPoint`: (Opcional) Crea un Transform hijo para especificar dónde salen las flechas

5. Asegúrate de que el resto de componentes estén presentes:
   - Enemy.cs
   - EnemyHealthBar
   - Animator con parámetros: isWalking, isAttackin, Dead

### Paso 3: Animador del Archer

Verifica que el Animator del archer tenga estos parámetros:
- **isWalking** (bool): Animar caminata
- **isAttackin** (bool): Animar disparo
- **Dead** (trigger): Animar muerte

Si no los tiene, agrégalos en el Animator Controller: archer_AnimatorController.controller

### Paso 4: Spawning en el Juego

Para instanciar el archer en tu escena:
- Arrastra el ArcherEnemy.prefab a la escena donde desees
- O usa EnemySpawner si tienes un sistema de spawn

## Características del Sistema

✅ **Detección inteligente**: El archer detecta al jugador dentro de su rango  
✅ **Movimiento dinámico**: Mantiene distancia óptima y se posiciona                
✅ **Combate a rango**: Dispara flechas con cooldown configurable  
✅ **Sistema de daño**: Considera defensa del jugador en cálculo de daño  
✅ **Integración con animador**: Sincroniza con animaciones del archer  
✅ **Visual feedback**: Impactos y VFX al golpear  
✅ **Escalable**: Fácil de ajustar dificultad (daño, cooldown, rango)  

## Ejemplo de Uso en Código

Si quieres crear el prefab del archer desde código:

```csharp
GameObject archeryEnemy = Instantiate(archerEnemyPrefab, spawnPosition, Quaternion.identity);
EnemyRangedAttack rangedAttack = archeryEnemy.GetComponent<EnemyRangedAttack>();
rangedAttack.damage = 12;
rangedAttack.attackCooldown = 1.8f;
```

## Troubleshooting

| Problema | Solución |
|----------|----------|
| **No se ve la flecha cuando dispara** | 1. Verifica que el Arrow prefab tenga un modelo visual (Mesh Renderer) <br> 2. El Arrow.cs tiene logging - abre Console para ver si se instancia <br> 3. Si ves "[Arrow] Instanciado..." pero no la ves, es problema del modelo <br> 4. Asegúrate que la flecha NO está dentro del archer (fuera de su collider) |
| **La flecha no daña al jugador** | 1. Verifica que Arrow.cs tenga componente PlayerHealth <br> 2. El Collider del Arrow debe estar en TRIGGER ✓ o tener Rigidbody <br> 3. Mira los logs: "[Arrow] Colisión trigger con:" <br> 4. Verifica que el jugador tenga el tag "Player" o componente PlayerHealth |
| **El archer no dispara** | 1. Abre Console y mira el error "[ArqueroName] NO HAY PROJECTILE PREFAB ASIGNADO!" <br> 2. Asigna el Arrow.prefab en EnemyRangedAttack → projectilePrefab <br> 3. Verifica que el archer esté dentro de attackRange del jugador |
| **La flecha se ve pero cae lentamente** | El isKinematic está activado. Desactívalo en Rigidbody del Arrow prefab |
| **La flecha desaparece inmediatamente** | El Arrow.cs está destruyendo al instante. Mira Console para ver el error real |
| **El archer no gira hacia el jugador** | Verifica que el Animator esté asignado en EnemyRangedAttack |
| **Múltiples flechas volando a la vez** | Aumenta `attackCooldown` a un valor más alto (ej: 3) |

## Notas

- El sistema usa detección de colisión tanto Trigger como OnCollisionEnter para máxima compatibilidad
- Las flechas se destruyen automáticamente después de 8 segundos
- El daño se calcula considerando la defensa del jugador: `finalDamage = max(1, damage - defense)`
- El archer mantiene su mirada en el jugador mientras ataca (FacePlayer)

