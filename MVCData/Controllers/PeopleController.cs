﻿using Microsoft.AspNetCore.Mvc;
using MVCData.ViewModels;
using MVCData.Models;
using MVCData.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;

namespace MVCData.Controllers
{
    [Authorize]
    public class PeopleController : Controller
    {

        public PeopleController(ApplicationDBContext database)
    {
        Database = database;
    }


        private readonly ApplicationDBContext Database;


    [HttpGet]
    public IActionResult Index()
    {
            ViewModelsContainer viewModels = new()
            {
                People = new PeopleViewModel()
                {
                    List = Database.People.Include(p => p.City).ToList(),
                },
                CreatePerson = new CreatePersonViewModel()
                {
                    SelectCity = new SelectList(Database.Cities, "ID", "Name")
                }
            };

            return View(viewModels);
        }


    [HttpPost]
    public IActionResult Search(string search)
    {
        ViewModelsContainer viewModels = new()
        {
            People = new PeopleViewModel()
            {
                Search = search,
                List = PeopleModel.Search(Database.People.ToList(), search)
            }
        };

        return View("Index", viewModels);
    }


    [HttpPost]
   
    public IActionResult CreatePerson(CreatePersonViewModel person)
        {
            int cityID = int.Parse(person.City);
            bool cityExists = Database.Cities.ToList().Exists(c => c.ID == cityID);

            CreatePersonViewModel createPerson = new()
            {
                SelectCity = new SelectList(Database.Cities, "ID", "Name")
            };

            if (ModelState.IsValid && cityExists)
            {
                City city = Database.Cities.Where(c => c.ID == cityID).ToList().First();
                Person newPerson = new(person.Name, person.Phone, city);
                Database.People.Add(newPerson);
                Database.SaveChanges();
            }
            else
            {
                createPerson.Name = person.Name;
                createPerson.Phone = person.Phone;
                createPerson.City = person.City;
            }

            ViewModelsContainer viewModels = new()
            {
                People = new PeopleViewModel()
                {
                    List = Database.People.Include(p => p.City).ToList(),
                },
                CreatePerson = createPerson
            };

            return View("Index", viewModels);
        }

        public IActionResult UpdatePerson(int id = 0)
        {
            UpdatePersonViewModel viewModel = new();
            bool personExists = Database.People.Select(p => p.ID).ToList().Contains(id);

            if (personExists)
            {
                Person person = Database.People
                .Include(p => p.City)
                
                .Where(p => p.ID == id)
                .ToList()
                .FirstOrDefault();

                viewModel.ID = person!.ID;
                viewModel.Name = person.Name;
                viewModel.Phone = person.Phone;
                viewModel.City = person.City.ID;
                viewModel.SelectCity = new SelectList(Database.Cities, "ID", "Name");
               
            }

            return View(viewModel);
        }



        [HttpPost]
        public IActionResult UpdatePerson(UpdatePersonViewModel model)
        {
            if (ModelState.IsValid)
            {
                Person person = Database.People.Include(p => p.City).Where(p => p.ID == model.ID).ToList().First();

                person.Name = model.Name;
                person.Phone = model.Phone;
                person.City = Database.Cities.Where(c => c.ID == model.City).ToList().First();

                

                Database.Update(person);
                Database.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                model.SelectCity = new SelectList(Database.Cities, "ID", "Name");
             
                return View(model);
            }
        }

        [HttpGet]
    public IActionResult DeletePerson(string name)
    {
            var personToDelete = Database.People.FirstOrDefault(p => p.Name == name);
            if (personToDelete != null)
        {
                Database.People.Remove(personToDelete);
                Database.SaveChanges();
            }



            return RedirectToAction("Index");
        }
}
}