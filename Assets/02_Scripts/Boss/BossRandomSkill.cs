using System;
using Unity.Properties;


namespace Unity.Behavior
{
    /// <summary>
    /// Executes a random branch.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "BossRandomSkill", story: "Random Skill not Cooldown [Skill]", category: "Flow", id: "a1dc04903f4059f351f52fd492bef815")]
    internal partial class BossRandomSkill : Composite
    {
        int m_RandomIndex = 0;

        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            m_RandomIndex = UnityEngine.Random.Range(0, Children.Count);
            if (m_RandomIndex < Children.Count)
            {
                var status = StartNode(Children[m_RandomIndex]);
                if (status == Status.Success || status == Status.Failure)
                    return status;

                return Status.Waiting;
            }

            return Status.Success;
        }

        private void CheckCoolDown()
        {
            foreach(Node child in Children)
            {
                if (child.GameObject.GetComponent<BlackboardVariable<string>>().Value == "a")
                {

                }
            }
        }
    }
}