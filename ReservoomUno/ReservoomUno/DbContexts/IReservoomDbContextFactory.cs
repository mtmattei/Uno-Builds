namespace ReservoomUno.DbContexts
{
    public interface IReservoomDbContextFactory
    {
        ReservoomDbContext CreateDbContext();
    }
}
