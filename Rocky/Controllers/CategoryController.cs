using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky_DataAccess.Data;
using Rocky_DataAccess.Repository;
using Rocky_Models;
using Rocky_Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class CategoryController : Controller
    {

        private readonly ICategoryRepository _catRepo;

        public CategoryController(ICategoryRepository catRepo)
        {
            _catRepo = catRepo;
        }
        public IActionResult Index()
        {
            IEnumerable<Category> objList = _catRepo.GetAll();
            return View(objList);
        }
        //GET - CREATE
        public IActionResult Create()
        {            
            return View();
        }

        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _catRepo.Add(obj);
                _catRepo.Save();
                TempData[WC.Success] = "Категория '" + obj.Name + "' успешно создана";
                return RedirectToAction("Index");
            }
            TempData[WC.Error] = "Не удалось создать категорию '" + obj.Name + "'";
            return View(obj);
        }

        //GET - EDIT
        public IActionResult Edit(int? id)
        {
            if (id==null || id==0)
            {
                return NotFound();
            }

            var obj = _catRepo.Find(id.GetValueOrDefault());

            if (id == null)
            {
                return NotFound();
            }


            return View(obj);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _catRepo.Update(obj);
                _catRepo.Save();

                TempData[WC.Success] = "Категория '" + obj.Name + "' успешно обновлена";
                return RedirectToAction("Index");
            }
            return View(obj);
        }


        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _catRepo.Find(id.GetValueOrDefault());

            if (id == null)
            {
                return NotFound();
            }


            return View(obj);
        }

        //POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? Id)
        {

            var obj = _catRepo.Find(Id.GetValueOrDefault());

            if (obj == null)
                return NotFound();

            _catRepo.Remove(obj);
            _catRepo.Save();
            TempData[WC.Success] = "Категория '" + obj.Name + "' успешно удалена";
            return RedirectToAction("Index");
          
        }

    }
}
