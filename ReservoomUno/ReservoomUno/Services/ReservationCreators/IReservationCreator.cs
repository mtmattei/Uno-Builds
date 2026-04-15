using ReservoomUno.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservoomUno.Services.ReservationCreators
{
    public interface IReservationCreator
    {
        Task CreateReservation(Reservation reservation);
    }
}
