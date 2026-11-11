using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Combo Chain")]
public class ComboChain : ScriptableObject
{
    public AttackData[] attacks;
    public float comboResetTime = 0.8f;   // pause after a hit before the combo restarts
    public float returnFadeTime = 0.15f;  // blend back to locomotion
    [Tooltip("Extra clicks remembered while an attack is playing. 2 = a quick triple-click plays three hits.")]
    public int maxBufferedPresses = 2;
}