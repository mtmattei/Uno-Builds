using ReservoomUno.Exceptions;
using ReservoomUno.Models;
using ReservoomUno.Stores;
using ReservoomUno.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservoomUno.Commands
{
    public class MakeReservationCommand : AsyncCommandBase
    {
        private readonly MakeReservationViewModel _makeReservationViewModel;
        private readonly HotelStore _hotelStore;
        private readonly Action _navigateBack;

        public MakeReservationCommand(MakeReservationViewModel makeReservationViewModel,
            HotelStore hotelStore,
            Action navigateBack)
        {
            _makeReservationViewModel = makeReservationViewModel;
            _hotelStore = hotelStore;
            _navigateBack = navigateBack;

            _makeReservationViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            return _makeReservationViewModel.CanCreateReservation && base.CanExecute(parameter);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            _makeReservationViewModel.SubmitErrorMessage = string.Empty;
            _makeReservationViewModel.IsSubmitting = true;

            Reservation reservation = new Reservation(
                new RoomID(_makeReservationViewModel.FloorNumber, _makeReservationViewModel.RoomNumber),
                _makeReservationViewModel.Username,
                _makeReservationViewModel.StartDate,
                _makeReservationViewModel.EndDate);

            try
            {
                await _hotelStore.MakeReservation(reservation);

                _navigateBack();
            }
            catch (ReservationConflictException)
            {
                _makeReservationViewModel.SubmitErrorMessage = "This room is already taken on those dates.";
            }
            catch (InvalidReservationTimeRangeException)
            {
                _makeReservationViewModel.SubmitErrorMessage = "Start date must be before end date.";
            }
            catch (Exception)
            {
                _makeReservationViewModel.SubmitErrorMessage = "Failed to make reservation.";
            }

            _makeReservationViewModel.IsSubmitting = false;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(MakeReservationViewModel.CanCreateReservation))
            {
                OnCanExecutedChanged();
            }
        }
    }
}
