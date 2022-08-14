using ChargeIt.Data;
using ChargeIt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargeIt.Controllers
{
    [Authorize]
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CarsController(ApplicationDbContext dbContext)
        {
            _applicationDbContext = dbContext;
        }
        public IActionResult Index()
        {
            var carViewModel = _applicationDbContext.Cars
                .Include(co => co.Owner)
                .Where(co => co.Owner.Id == co.OwnerId)
                .Select(co => new CarViewModel
                {
                    Id = co.Id,
                    OwnerName = co.Owner.Name,
                    PlateNumber = co.PlateNumber
                })
                .ToList();

            return View(carViewModel);
        }

        public IActionResult AddCar()
        {
            var ownersViewModel = _applicationDbContext.CarOwners.Select(o => new DropDownViewModel
            {
                Id = o.Id,
                Value = o.Name
            }).ToList();
            var carViewModel = new CarViewModel
            {
                Owners = ownersViewModel,
            };
            return View(carViewModel);
        }
        [HttpPost]
        public IActionResult AddNewCar(CarViewModel carViewModel)
        {
            var ownersViewModel = _applicationDbContext.CarOwners
               .Select(o => new DropDownViewModel
               {
                   Id = o.Id,
                   Value = o.Name
               }).ToList();
            carViewModel.Owners = ownersViewModel;

            if (!ModelState.IsValid)
            {
                return View("AddCar", carViewModel);
            }

            var existingCar = _applicationDbContext.Cars.FirstOrDefault(c => c.PlateNumber == carViewModel.PlateNumber);

            if (existingCar != null)
            {
                ModelState.AddModelError("PlateNumber", "This plate number already exist!");
                return View("AddCar", carViewModel);
            }

            _applicationDbContext.Cars.Add(new CarDbModel
            {
                PlateNumber = carViewModel.PlateNumber,
                OwnerId = carViewModel.CarOwnerId
            });

            _applicationDbContext.SaveChanges();

            return RedirectToAction("Index", "Cars");
        }
        public IActionResult CarDetails(int id)
        {
            var existingCar = _applicationDbContext.Cars.FirstOrDefault(ec => ec.Id == id);
            if (existingCar == null)
            {
                return RedirectToAction("Index");
            }

            var carViewModel = _applicationDbContext.Cars
               .Include(co => co.Owner)
               .Where(co => co.Owner.Id == co.OwnerId && co.Id == id)
               .Select(co => new CarViewModel
               {
                   Id = existingCar.Id,
                   OwnerName = co.Owner.Name,
                   PlateNumber = co.PlateNumber
               }).FirstOrDefault();

            var availableBookings = _applicationDbContext.Bookings
                .Include(b => b.ChargeMachine)
                .Where(b => b.CarId == existingCar.Id)
                .OrderByDescending(b => b.StartTime)
                .Select(b => new BookingDetailsViewModel
                {
                    Id = b.Id,
                    BookingCode = b.Code,
                    StartTime = b.StartTime,
                    StationCode = b.ChargeMachine.Code,
                    City = b.ChargeMachine.City,
                    Street = b.ChargeMachine.Street,
                    Number = b.ChargeMachine.Number,
                })
                .ToList();

            var carDetailsViewModel = new CarDetailsViewModel
            {
                Car = carViewModel,
                Bookings = availableBookings
            };

            return View(carDetailsViewModel);
        }
        public IActionResult EditCar(int id)
        {
            var exsitingCar = _applicationDbContext.Cars.FirstOrDefault(ec => ec.Id == id);
            if (exsitingCar == null)
            {
                RedirectToAction("Index");
            }

            var ownersViewModel = _applicationDbContext.CarOwners
                .Select(o => new DropDownViewModel
                {
                    Id = o.Id,
                    Value = o.Name
                }).ToList();

            var carViewModel = new CarViewModel
            {
                Id = exsitingCar.Id,
                PlateNumber = exsitingCar.PlateNumber,
                Owners = ownersViewModel,
                OwnerName = ownersViewModel.FirstOrDefault().Value
            };
            return View(carViewModel);
        }
        public IActionResult EditExistingCar(CarViewModel model)
        {
            var ownersViewModel = _applicationDbContext.CarOwners
                  .Select(o => new DropDownViewModel
                  {
                      Id = o.Id,
                      Value = o.Name
                  }).ToList();

            model.Owners = ownersViewModel;
            if (!ModelState.IsValid)
            {
                return View("EditCar", model);
            }


            var checkPlateNumber = _applicationDbContext.Cars.FirstOrDefault(cpn => cpn.PlateNumber == model.PlateNumber);
            if (checkPlateNumber != null)
            {
                if (checkPlateNumber.Id == model.Id && checkPlateNumber.OwnerId != model.CarOwnerId)
                {
                    var existingCarDb = _applicationDbContext.Cars.FirstOrDefault(ec => ec.Id == model.Id);
                    existingCarDb.PlateNumber = model.PlateNumber;
                    existingCarDb.OwnerId = model.CarOwnerId;

                    _applicationDbContext.SaveChanges();
                    return RedirectToAction("Index");
                }
                else if (checkPlateNumber.Id != model.Id)
                {
                    checkPlateNumber = _applicationDbContext.Cars.FirstOrDefault(cpn => cpn.PlateNumber == model.PlateNumber);
                    if (checkPlateNumber != null)
                    {
                        ModelState.AddModelError("PlateNumber", "This plate number already exist!");
                        return View("EditCar", model);
                    }
                    else
                    {
                        var existingCarNumber = _applicationDbContext.Cars.FirstOrDefault(ec => ec.Id == model.Id);
                        existingCarNumber.PlateNumber = model.PlateNumber;
                        existingCarNumber.OwnerId = model.CarOwnerId;

                        _applicationDbContext.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    ModelState.AddModelError("PlateNumber", "This plate number already exist!");
                    return View("EditCar", model);
                }
            }
            var existingCar = _applicationDbContext.Cars.FirstOrDefault(ec => ec.Id == model.Id);
            existingCar.PlateNumber = model.PlateNumber;
            existingCar.OwnerId = model.CarOwnerId;

            _applicationDbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteCar(int id)
        {
            var existingCar = _applicationDbContext.Cars.FirstOrDefault(ec => ec.Id == id);
            if (existingCar != null)
            {
                _applicationDbContext.Cars.Remove(existingCar);
                _applicationDbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
