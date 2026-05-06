namespace QWERDS
{
    /// <summary>Атака по случайному живому врагу.</summary>
    public class AttackAction : ActionBase
    {
        public int Power { get; set; } = 10;

        public AttackAction()
        {
            Name = "Атака";
            Description = $"Наносит {Power} урона случайному врагу.";
        }

        public override void Execute(BattleContext context)
        {
            var enemy = context.Battle.GetRandomAliveEnemy();
            if (enemy != null)
            {
                enemy.TakeDamage(Power);
                context.Battle.LogMessage($"{context.User.Name} атакует {enemy.Name} на {Power} урона.");
            }
        }
    }
}