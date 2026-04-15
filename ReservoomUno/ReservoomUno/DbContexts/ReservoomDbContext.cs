using Microsoft.EntityFrameworkCore;
using ReservoomUno.DTOs;
using ReservoomUno.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservoomUno.DbContexts
{
    public class ReservoomDbContext : DbContext
    {
        public ReservoomDbContext(DbContextOptions options) : base(options) { }

        public DbSet<ReservationDTO> Reservations { get; set; }
    }
}
