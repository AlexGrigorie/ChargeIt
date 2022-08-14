using ChargeIt.Data;
using ChargeIt.Data.DbModels;
using ChargeIt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargeIt.Controllers
{
    [Authorize]
    public class CarOwnersController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public CarOwnersController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public IActionResult Index()
        {
            var carOwnersViewModels = _applicationDbContext.CarOwners
                .Select(co => new CarOwnerViewModel
                {
                    Id = co.Id,
                    Name = co.Name,
                    Email = co.Email
                }).ToList();
            return View(carOwnersViewModels);
        }

        public IActionResult AddCarOwner()
        {
            var carOwnerViewModel = new CarOwnerViewModel();
            return View(carOwnerViewModel);
        }
        [HttpPost]
        public IActionResult AddNewCarOwner(CarOwnerViewModel carOwnerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("AddCarOwner", carOwnerViewModel);
            }
            _applicationDbContext.CarOwners.Add(new CarOwnerDbModel
            {
                Name = carOwnerViewModel.Name,
                Email = carOwnerViewModel.Email
            });
            _applicationDbContext.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult CarOwnerDetails(int id)
        {
            var existingCarOwner = _applicationDbContext.CarOwners.FirstOrDefault(co => co.Id == id);
            if (existingCarOwner == null)
            {
                return RedirectToAction("Index");
            }

            var carViewModel = _applicationDbContext.Cars
                 .Include(co => co.Owner)
                 .Where(co => existingCarOwner.Id == co.OwnerId)
                 .Select(co => new CarViewModel
                 {
                     Id = co.Id,
                     OwnerName = co.Owner.Name,
                     PlateNumber = co.PlateNumber
                 })
                 .ToList();

            return View(carViewModel);
        }
        public IActionResult EditCarOwner(int id)
        {
            var existingCarOwner = _applicationDbContext.CarOwners.FirstOrDefault(co => co.Id == id);
            if (existingCarOwner == null)
            {
                return RedirectToAction("Index");
            }

            var carOwnerViewModel = new CarOwnerViewModel
            {
                Id = existingCarOwner.Id,
                Name = existingCarOwner.Name,
                Email = existingCarOwner.Email
            };

            return View(carOwnerViewModel);
        }
        [HttpPost]
        public IActionResult EditExistingCarOwner(CarOwnerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditCarOwner", model);
            }
            var existingCarOwner = _applicationDbContext.CarOwners.FirstOrDefault(co => co.Id == model.Id);
            existingCarOwner.Name = model.Name;
            existingCarOwner.Email = model.Email;
            _applicationDbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteCarOwner(int id)
        {
            var existingCarOwner = _applicationDbContext.CarOwners.FirstOrDefault(co => co.Id == id);
            if (existingCarOwner != null)
            {
                var existingCarOwnerId = _applicationDbContext.Cars.FirstOrDefault(co => co.OwnerId == existingCarOwner.Id);
                if (existingCarOwnerId != null)
                {
                    var updateCarOwnerId = from x in _applicationDbContext.Cars
                                           where x.OwnerId == existingCarOwnerId.Id
                                           select x;
                    foreach (var car in updateCarOwnerId)
                    {
                        car.OwnerId = null;
                    }

                    _applicationDbContext.SaveChanges();
                }

                _applicationDbContext.CarOwners.Remove(existingCarOwner);
                _applicationDbContext.SaveChanges();
            }
            return RedirectToAction("Index");

        }
    }
}
