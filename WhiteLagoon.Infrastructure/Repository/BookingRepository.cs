using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly ApplicationDbContext _db;

        public BookingRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Booking entity)
        {
            _db.Bookings.Update(entity);
        }

        public void UpdateStatus(int bookingId, string bookingStatus)
        {
            var booking = _db.Bookings.FirstOrDefault(m => m.Id == bookingId);
            if (booking != null)
            {
                booking.Status = bookingStatus;
                
                if (bookingStatus == StaticDetail.StatusCheckedIn)
                {
                    // booking.VillaNumber = villaNumber;
                    booking.ActualCheckInDate = DateTime.Now;
                }
                if (bookingStatus == StaticDetail.StatusCompleted)
                {
                    booking.ActualCheckOutDate = DateTime.Now;
                }
            }
        }

        public void UpdateStripePaymentId(int bookingId, string sessionId, string paymentIntentId)
        {
            var booking = _db.Bookings.FirstOrDefault(m => m.Id == bookingId);
            if (booking != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    booking.StripeSessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    booking.StripePaymentIntentId = paymentIntentId;
                    booking.PaymentDate = DateTime.Now;
                    booking.IsPaymentSuccessful = true;
                }
            }
        }
    }
}
