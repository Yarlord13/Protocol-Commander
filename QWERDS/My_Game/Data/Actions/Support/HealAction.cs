namespace QWERDS
{
    /// <summary>Лечение самого себя.</summary>
    public class HealAction : ActionBase
    {
        public int Amount { get; set; } = 15;

        public HealAction()
        {
            Name = "Ремонт";
            Description = $"Восстанавливает {Amount} здоровья.";
        }

        public override void Execute(BattleContext context)
        {
            context.User.Heal(Amount);
            context.Battle.LogMessage($"{context.User.Name} восстанавливает {Amount} HP.");
        }
    }
}