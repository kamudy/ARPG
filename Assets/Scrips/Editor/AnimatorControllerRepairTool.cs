#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AnimatorControllerRepairTool
{
    [MenuItem("Tools/ARPG/Animator/Repair Selected Controller")]
    public static void RepairSelectedController()
    {
        Object active = Selection.activeObject;
        AnimatorController controller = active as AnimatorController;
        if (controller == null)
        {
            Debug.LogWarning("[AnimatorRepair] Selecciona un AnimatorController en el Project.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(controller);
        int removed = RepairController(controller);
        SaveController(path, removed);
    }

    [MenuItem("Tools/ARPG/Animator/Repair All Controllers")]
    public static void RepairAllControllers()
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimatorController");
        int fixedControllers = 0;
        int removedTotal = 0;

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
                continue;

            int removed = RepairController(controller);
            if (removed > 0)
            {
                fixedControllers++;
                removedTotal += removed;
                EditorUtility.SetDirty(controller);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[AnimatorRepair] Controllers reparados: {fixedControllers}, enlaces eliminados: {removedTotal}.");
    }

    private static void SaveController(string path, int removed)
    {
        if (removed > 0)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AnimatorRepair] Reparado: {path}. Enlaces eliminados: {removed}.");
        }
        else
        {
            Debug.Log($"[AnimatorRepair] Sin cambios: {path}.");
        }
    }

    private static int RepairController(AnimatorController controller)
    {
        int removed = 0;
        if (controller == null)
            return removed;

        AnimatorControllerLayer[] layers = controller.layers;
        for (int i = 0; i < layers.Length; i++)
        {
            AnimatorStateMachine sm = layers[i].stateMachine;
            if (sm == null)
                continue;

            removed += RepairStateMachineRecursive(sm);
        }

        if (removed > 0)
            EditorUtility.SetDirty(controller);

        return removed;
    }

    private static int RepairStateMachineRecursive(AnimatorStateMachine sm)
    {
        int removed = 0;
        if (sm == null)
            return removed;

        Undo.RecordObject(sm, "Repair Animator StateMachine");

        // Limpia states huérfanos.
        List<ChildAnimatorState> validStates = new List<ChildAnimatorState>();
        ChildAnimatorState[] states = sm.states;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].state != null)
                validStates.Add(states[i]);
            else
                removed++;
        }
        if (validStates.Count != states.Length)
            sm.states = validStates.ToArray();

        // Limpia sub-state machines huérfanas.
        List<ChildAnimatorStateMachine> validSubMachines = new List<ChildAnimatorStateMachine>();
        ChildAnimatorStateMachine[] subMachines = sm.stateMachines;
        for (int i = 0; i < subMachines.Length; i++)
        {
            if (subMachines[i].stateMachine != null)
                validSubMachines.Add(subMachines[i]);
            else
                removed++;
        }
        if (validSubMachines.Count != subMachines.Length)
            sm.stateMachines = validSubMachines.ToArray();

        // Limpia AnyState transitions inválidas.
        List<AnimatorStateTransition> validAny = new List<AnimatorStateTransition>();
        AnimatorStateTransition[] anyTransitions = sm.anyStateTransitions;
        for (int i = 0; i < anyTransitions.Length; i++)
        {
            AnimatorStateTransition t = anyTransitions[i];
            if (t == null)
            {
                removed++;
                continue;
            }

            bool hasDestination = t.isExit || t.destinationState != null || t.destinationStateMachine != null;
            if (hasDestination)
                validAny.Add(t);
            else
                removed++;
        }
        if (validAny.Count != anyTransitions.Length)
            sm.anyStateTransitions = validAny.ToArray();

        // Limpia Entry transitions inválidas.
        List<AnimatorTransition> validEntry = new List<AnimatorTransition>();
        AnimatorTransition[] entryTransitions = sm.entryTransitions;
        for (int i = 0; i < entryTransitions.Length; i++)
        {
            AnimatorTransition t = entryTransitions[i];
            if (t == null)
            {
                removed++;
                continue;
            }

            bool hasDestination = t.destinationState != null || t.destinationStateMachine != null;
            if (hasDestination)
                validEntry.Add(t);
            else
                removed++;
        }
        if (validEntry.Count != entryTransitions.Length)
            sm.entryTransitions = validEntry.ToArray();

        // Limpia transiciones por estado inválidas.
        ChildAnimatorState[] refreshedStates = sm.states;
        for (int i = 0; i < refreshedStates.Length; i++)
        {
            AnimatorState state = refreshedStates[i].state;
            if (state == null)
                continue;

            Undo.RecordObject(state, "Repair Animator State");
            AnimatorStateTransition[] transitions = state.transitions;
            List<AnimatorStateTransition> validTransitions = new List<AnimatorStateTransition>();

            for (int j = 0; j < transitions.Length; j++)
            {
                AnimatorStateTransition t = transitions[j];
                if (t == null)
                {
                    removed++;
                    continue;
                }

                bool hasDestination = t.isExit || t.destinationState != null || t.destinationStateMachine != null;
                if (hasDestination)
                    validTransitions.Add(t);
                else
                    removed++;
            }

            if (validTransitions.Count != transitions.Length)
            {
                state.transitions = validTransitions.ToArray();
                EditorUtility.SetDirty(state);
            }
        }

        // Recorre recursivamente sub-state machines.
        ChildAnimatorStateMachine[] refreshedSubMachines = sm.stateMachines;
        for (int i = 0; i < refreshedSubMachines.Length; i++)
            removed += RepairStateMachineRecursive(refreshedSubMachines[i].stateMachine);

        if (removed > 0)
            EditorUtility.SetDirty(sm);

        return removed;
    }
}
#endif
