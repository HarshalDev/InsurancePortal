using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using InsuranceClient.Models;
using InsuranceClient.Models.ViewModels;
using InsuranceClient.Helpers;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace InsuranceClient.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _config;

        public HomeController(IConfiguration configuration)
        {
            _config = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var tempFile = Path.GetTempFileName();
                StorageHelper storageHelper = new StorageHelper();
                storageHelper.ConnectionString = _config.GetConnectionString("StorageConnection");
                var customerId = Guid.NewGuid();
                using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                {
                    await model.Image.CopyToAsync(fs);
                }
                var fileName = Path.GetFileName(model.Image.FileName);
                var tempPath = Path.GetDirectoryName(tempFile);
                var imagePath = Path.Combine(tempPath, string.Concat(customerId, "-" + fileName));
                System.IO.File.Move(tempFile, imagePath);//rename temp file
                var imageUrl= await storageHelper.UploadCustomerImage("images", imagePath);

                Customer customer = new Customer(customerId.ToString(), model.InsuranceType);
                customer.ImageUrl = imageUrl;
                customer.FullName = model.FullName;
                customer.Email = model.Email;
                customer.Amount = model.Amount;
                customer.EndDate = model.EndDate;
                customer.AppDate = model.AppDate;
                await storageHelper.InsertCustomerAsync("Customers", customer);
                //save cust data to azure table
                //save cust image to azure blob
                //add a confiramation msg to azure queue
                await storageHelper.AddMessageAsync("myqueue", customer);
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
