using System.Collections.Generic;
using UnityEngine;

namespace Skill
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Data/Skills", order = 3)]
    public class Skill : ScriptableObject
    {
        public List<SkillData> skillKeeper = null;
    }
}
