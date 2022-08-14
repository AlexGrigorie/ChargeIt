using ChargeIt.Configuration;
using ChargeIt.Data;
using ChargeIt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Net;
using System.Net.Mail;

namespace ChargeIt.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly EmailConfigurations _emailConfigurations;
        private readonly List<int> _totalAvailableHours;
        private const int TotalAvailableHoursInADay = 24;
        public BookingsController(ApplicationDbContext dbContext, EmailConfigurations emailConfigurations)
        {
            _applicationDbContext = dbContext;
            _emailConfigurations = emailConfigurations;
            _totalAvailableHours = new List<int>();
            for (var hour = 0; hour < TotalAvailableHoursInADay; hour++)
            {
                _totalAvailableHours.Add(hour);
            }
        }
        public IActionResult Index()
        {
            var chargeMachineViewModels = _applicationDbContext.ChargeMachines.Select(cm => new DropDownViewModel
            {
                Id = cm.Id,
                Value = $"{cm.Code}, {cm.City}, {cm.Street}, {cm.Number}",
                Latitude = cm.Latitude,
                Longitude = cm.Longitude,
            }).ToList();

            var carViewModels = _applicationDbContext.Cars.Select(c => new DropDownViewModel
            {
                Id = c.Id,
                Value = c.PlateNumber,
            }).ToList();
            var bookingsViewModel = new BookingsViewModel
            {
                ChargeMachines = chargeMachineViewModels,
                Cars = carViewModels,
            };
            return View(bookingsViewModel);
        }
        public IActionResult AddNewBooking(BookingsViewModel bookingViewModel)
        {
            if (ModelState.IsValid)
            {
                var startTime = bookingViewModel.Date.Value.AddHours(bookingViewModel.IntervalHour.Value);
                var endTime = startTime.AddMinutes(59).AddSeconds(59);

                if (_applicationDbContext.Bookings.FirstOrDefault(b => b.ChargeMachineId ==
                 bookingViewModel.ChargeMachineId && b.StartTime == startTime) != null)
                {
                    ModelState.AddModelError(nameof(BookingsViewModel.IntervalHour), "There is an already allocated " +
                        "interval for the selected machine for the selected interval");
                }

                if (_applicationDbContext.Bookings.FirstOrDefault(b => b.CarId ==
                 bookingViewModel.CarId && b.StartTime == startTime) != null)
                {
                    ModelState.AddModelError(nameof(BookingsViewModel.IntervalHour), "There is car has been already allocated " +
                        "interval to the selected interval on different charge machine");
                }
                if (ModelState.IsValid)
                {
                    var booking = new BookingDbModel
                    {
                        CarId = bookingViewModel.CarId.Value,
                        ChargeMachineId = bookingViewModel.ChargeMachineId.Value,
                        StartTime = startTime,
                        EndTime = endTime,
                        Code = Guid.NewGuid()
                    };

                    _applicationDbContext.Bookings.Add(booking);
                    _applicationDbContext.SaveChanges();
                    SendEmailToTheUser(booking.Id);
                }
            }
            if (!ModelState.IsValid)
            {
                var chargeMachineViewModels = _applicationDbContext.ChargeMachines.Select(cm => new DropDownViewModel
                {
                    Id = cm.Id,
                    Value = $"{cm.Code}, {cm.City}, {cm.Street}, {cm.Number}"
                }).ToList();

                var carViewModels = _applicationDbContext.Cars.Select(c => new DropDownViewModel
                {
                    Id = c.Id,
                    Value = c.PlateNumber,
                }).ToList();

                bookingViewModel.ChargeMachines = chargeMachineViewModels;
                bookingViewModel.Cars = carViewModels;
                bookingViewModel.IntervalHour = null;
                return View("Index", bookingViewModel);
            }

            return RedirectToAction("Index");
        }
        private void SendEmailToTheUser(int bookingId)
        {
            var booking = _applicationDbContext.Bookings
                .Include(b => b.ChargeMachine)
                .Include(b => b.Car)
                .ThenInclude(c => c.Owner)
                .FirstOrDefault(b => b.Id == bookingId);

            var qrCode = GetBookingQRCode(booking.Code);

            var emailBody = @$"<h3>A new order has been created for your car : {booking.Car.PlateNumber}</h3>
                <p><b>Order number:</b>{booking.Code}</p>
                <p><b>Interval:</b>{booking.StartTime.ToString("yyyy-MM-dd HH:mm")} - {booking.EndTime.ToString("yyyy-MM-dd HH:mm")}</p>
                <p><b>Charge machine code:</b>{booking.ChargeMachine.Code}</p>
                <p><b>City:</b>{booking.ChargeMachine.City}</p>
                <p><b>Street:</b>{booking.ChargeMachine.Street}</p>
                <p><b>Number:</b>{booking.ChargeMachine.Number}</p>
            ";

            var message = new MailMessage();
            message.From = new MailAddress(_emailConfigurations.EmailAddress);
            message.To.Add(booking.Car.Owner.Email);
            message.Subject = "New booking was created for you";
            message.IsBodyHtml = true;
            message.Body = emailBody;

            using (MemoryStream ms = new MemoryStream(qrCode))
            {
                message.Attachments.Add(new Attachment(ms, $"{booking.Code}.png"));

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_emailConfigurations.EmailAddress, _emailConfigurations.AppPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);
            }
        }
        private byte[] GetBookingQRCode(Guid code)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(code.ToString(), QRCodeGenerator.ECCLevel.Q);
            var bitmapByteQRCode = new BitmapByteQRCode(qrCodeData);
            var encodedQrCode = bitmapByteQRCode.GetGraphic(5);
            return encodedQrCode;
        }

        [HttpGet("Bookings/GetAvailableIntervals")]
        public ActionResult<List<int>> GetAvailableIntervals(int chargeMachineId, DateTime date)
        {
            var notAvailableHours = _applicationDbContext.Bookings.Where(b => b.ChargeMachineId == chargeMachineId && b.StartTime >= date
             && b.StartTime <= date.AddHours(23).AddMinutes(59).AddSeconds(59)).Select(b => b.StartTime.Hour).ToList();

            var currentDate = DateTime.Now;
            var totalAvailableHours = _totalAvailableHours;
            if (date.Date == DateTime.Now.Date)
            {
                var currentHour = currentDate.Hour;
                totalAvailableHours = totalAvailableHours.Where(tav => tav > currentHour).ToList();
            }
            var availableHours = totalAvailableHours.Except(notAvailableHours).ToList();
            return availableHours;
        }
        [HttpGet("Bookings/DeleteBooking/{id}/{entityId}/{sourceAction}")]
        public IActionResult DeleteActiveBooking(int id, int entityId, string sourceAction)
        {
            var existingBooking = _applicationDbContext.Bookings.FirstOrDefault(ec => ec.Id == id);
            var getCurrentDate = DateTime.Now;
            if (existingBooking != null && existingBooking.StartTime >= getCurrentDate)
            {
                _applicationDbContext.Bookings.Remove(existingBooking);
                _applicationDbContext.SaveChanges();
                if (sourceAction == "CarDetails")
                {
                    return RedirectToAction("CarDetails", "Cars", new { id = entityId });
                }
                else
                {
                    return RedirectToAction("StationDetails", "ChargeMachines", new { id = entityId });
                }
            }
            else
            {
                return View("UnAvailableBooking");
            }


        }
    }

}
