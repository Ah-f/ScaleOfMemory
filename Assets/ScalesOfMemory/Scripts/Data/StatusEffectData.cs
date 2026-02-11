using UnityEngine;

[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "ScalesOfMemory/StatusEffectData")]
public class StatusEffectData : ScriptableObject
{
    public string effectName;
    public StatusEffectType type;
    public int damagePerTurn;
    public bool preventsAction;
    public Color iconColor = Color.white;
}
