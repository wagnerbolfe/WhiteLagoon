using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    [Authorize(Roles = StaticDetail.RoleAdmin)]
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;

        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _env = env;
        }

        public IActionResult Index()
        {
            var villas = _unitOfWork.Villa.GetAll();
            return View(villas);
        }

        public IActionResult Create()
        {
            return View(); 
        }

        [HttpPost]
        public IActionResult Create(Villa obj)
        {
            if (obj.Name == obj.Description)
            {
                ModelState.AddModelError("Name", "The description cannot exactly match the Name");
            }
            if (!ModelState.IsValid) return View();

            if (obj.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                string imagePath = Path.Combine(_env.WebRootPath, @"images\VillaImage");

                using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                obj.Image.CopyTo(fileStream);

                obj.ImageUrl = @"\images\VillaImage\" + fileName;
            }
            else
            {
                obj.ImageUrl = "https://placehold.co/600x400";
            }

            _unitOfWork.Villa.Add(obj);
            _unitOfWork.Save();
            TempData["success"] = "The villa has been created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Update(int villaId)
        {
            var obj = _unitOfWork.Villa.Get(u => u.Id == villaId);
            if (obj is null) return RedirectToAction("Error", "Home");

            return View(obj);
        }

        [HttpPost]
        public IActionResult Update(Villa obj)
        {
            if (!ModelState.IsValid && obj.Id > 0) return View();

            if (obj.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                string imagePath = Path.Combine(_env.WebRootPath, @"images\VillaImage");

                if (!string.IsNullOrEmpty(obj.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, obj.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                obj.Image.CopyTo(fileStream);

                obj.ImageUrl = @"\images\VillaImage\" + fileName;
            }

            _unitOfWork.Villa.Update(obj);
            _unitOfWork.Save();
            TempData["success"] = "The villa has been updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int villaId)
        {
            var obj = _unitOfWork.Villa.Get(u => u.Id == villaId);
            if (obj is null) return RedirectToAction("Error", "Home");

            return View(obj);
        }

        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
            var villa = _unitOfWork.Villa.Get(u => u.Id == obj.Id);

            if (villa is not null)
            {
                if (!string.IsNullOrEmpty(villa.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, villa.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.Villa.Remove(villa);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa could not be deleted.";
            return View();

        }
    }
}
