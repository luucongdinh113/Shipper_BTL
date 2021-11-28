﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SHIPPER.Models;
using SHIPPER.Services;

namespace SHIPPER.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICustomerService _customerService;
        public HomeController(ILogger<HomeController> logger, ICustomerService customerService)
        {
            _logger = logger;
            _customerService = customerService;
        }
        public async Task<IActionResult> Menu()
        {
            var food = await _customerService.GetFoodAsync();
            return View(food);
        }
        public async Task<IActionResult> InsertMonAn(DonVanChuyenViewModel donVanChuyen)
        {
            await _customerService.InsertFoodAsync(donVanChuyen);
            return RedirectToAction("Menu", "Home");
        }
        public IActionResult Index()
        {
            return View();
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

        [HttpGet("/QuanLiMonAn")]
        public IActionResult QuanLiMonAn()
        {
            return View(new QuanLiMonAnViewModel());
        }
        [HttpPost("/QuanLiMonAn")]
        public IActionResult QuanLiMonAn(QuanLiMonAnViewModel NhaHang)
        {
            if (NhaHang.Type == "search")
            {
                QuanLiMonAnViewModel data = _customerService.QuanLiMonAn(NhaHang.Add);
                data.Add=NhaHang.Add;
                return View(data);
            }
            else if(NhaHang.Type =="delete")
            {
                _customerService.DeleteMonAn(NhaHang);
            }
            else if (NhaHang.Type =="active")
            {
                _customerService.ActiveMonAn(NhaHang);
            }
            QuanLiMonAnViewModel data1 = _customerService.QuanLiMonAn(NhaHang.Add);
            return View(data1);
        }
    }
}
