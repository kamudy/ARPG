// REFERENCIA: Parámetros del Animation Controller que necesitas crear manualmente en Unity

/*
=== PARÁMETROS ===
1. isWalking (Bool) - Default: OFF
2. isAttacking (Bool) - Default: OFF
3. Death (Trigger)

=== ESTADOS ===
1. Idle (Default - naranja)
   - Animation clip: "idle"
   - Loop: ON

2. Walk
   - Animation clip: "walk"
   - Loop: ON

3. Attack
   - Animation clip: "attack"
   - Loop: OFF
   - Speed: 1.0

4. Death
   - Animation clip: "death"
   - Loop: OFF
   - Speed: 1.0

=== TRANSICIONES ===

Idle → Walk:
  - Condición: isWalking == true
  - Exit time: 0
  - Transition duration: 0.1

Walk → Idle:
  - Condición: isWalking == false
  - Exit time: 0
  - Transition duration: 0.1

Idle → Attack:
  - Condición: isAttacking == true
  - Exit time: 0
  - Transition duration: 0.1

Attack → Idle:
  - Condición: isAttacking == false
  - Exit time: 0.8 (deja que la animación termine)
  - Transition duration: 0.2

Any State → Death:
  - Condición: Death (Trigger)
  - Exit time: 0
  - Transition duration: 0.2

*/

// EL SCRIPT YA ESTÁ ACTUALIZADO PARA USAR:
// - animator.SetBool("isWalking", true/false)
// - animator.SetBool("isAttacking", true/false)
// - animator.SetTrigger("Death")
