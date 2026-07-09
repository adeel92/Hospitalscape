using NaughtyAttributes;
using UnityEngine;

public class AnimatorStatePlayer : MonoBehaviour
{
    [SerializeField, ReadOnly] string currentState;
    [SerializeField] Animator animator;

    public string CurrentState => currentState;

    public void PlayTrigger(string triggerName)
    {
        if (!HasParameter(triggerName, AnimatorControllerParameterType.Trigger))
        {
            Debug.LogWarning($"Animator '{animator.name}' does not contain Trigger parameter '{triggerName}'.", this);
            return;
        }

        animator.SetTrigger(triggerName);
        currentState = triggerName;
    }

    public void PlayBool(string parameterName)
    {
        if (!HasParameter(parameterName, AnimatorControllerParameterType.Bool))
        {
            Debug.LogWarning($"Animator '{animator.name}' does not contain Bool parameter '{parameterName}'.", this);
            return;
        }

        animator.SetBool(parameterName, true);
        currentState = parameterName;
    }
    public void StopBool(string parameterName)
    {
        if (!HasParameter(parameterName, AnimatorControllerParameterType.Bool))
        {
            Debug.LogWarning($"Animator '{animator.name}' does not contain Bool parameter '{parameterName}'.", this);
            return;
        }

        animator.SetBool(parameterName, false);
    }

    /*public void PlayInt(string parameterName, int value)
    {
        if (!HasParameter(parameterName, AnimatorControllerParameterType.Int))
        {
            Debug.LogWarning($"Animator '{animator.name}' does not contain Int parameter '{parameterName}'.", this);
            return;
        }

        animator.SetInteger(parameterName, value);
        currentState = $"{parameterName} = {value}";
    }*/

    /*public void PlayFloat(string parameterName, float value)
    {
        if (!HasParameter(parameterName, AnimatorControllerParameterType.Float))
        {
            Debug.LogWarning($"Animator '{animator.name}' does not contain Float parameter '{parameterName}'.", this);
            return;
        }

        animator.SetFloat(parameterName, value);
        currentState = $"{parameterName} = {value}";
    }*/

    public void PlayState(string stateName)
    {
        if (!TryGetState(stateName, out int stateHash, out int layer))
        {
            Debug.LogWarning($"Animator '{animator.name}' does not contain state '{stateName}'.", this);
            return;
        }

        animator.Play(stateHash, layer);
        currentState = stateName;
    }

    /*public void CrossFadeToState(string stateName, float transitionDuration = 0.15f)
    {
        if (!TryGetState(stateName, out int stateHash, out int layer))
        {
            Debug.LogWarning($"Animator '{animator.name}' does not contain state '{stateName}'.", this);
            return;
        }

        animator.CrossFade(stateHash, transitionDuration, layer);
        currentState = stateName;
    }*/

    bool HasParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName &&
                parameter.type == parameterType)
            {
                return true;
            }
        }

        return false;
    }

    bool TryGetState(string stateName, out int stateHash, out int layer)
    {
        stateHash = Animator.StringToHash(stateName);

        for (layer = 0; layer < animator.layerCount; layer++)
        {
            if (animator.HasState(layer, stateHash))
                return true;
        }

        layer = -1;
        return false;
    }
}