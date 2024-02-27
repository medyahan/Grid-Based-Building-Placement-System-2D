namespace Interface
{
    public interface IDamageable
    {
        public bool IsAlive();
        public void TakeDamage(int damage);
    }
}
