using Microsoft.AspNetCore.Mvc;
using SHIPPER.Models;
using SHIPPER.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SHIPPER.Controllers
{
    public class ChiNhanhController : Controller
    {
        private readonly IChiNhanhService _chinhanhService;
        public ChiNhanhController(IChiNhanhService chinhanhService)
        {
            _chinhanhService = chinhanhService;
        }
        [HttpGet]
        public async Task<IActionResult> AddChiNhanh()
        {
            QuanLyChiNhanhViewModel result= await _chinhanhService.GetListPageChiNhanh();
            return View(result);
        }
        [HttpPost]
        public async Task<IActionResult> AddChiNhanh(QuanLyChiNhanhViewModel qlchinhanhVM)
        {
            if (!ModelState.IsValid)
            {
                QuanLyChiNhanhViewModel result = await _chinhanhService.GetListPageChiNhanh();
                qlchinhanhVM.ListChiNhanh = result.ListChiNhanh;
                qlchinhanhVM.ListQuanLy = result.ListQuanLy;
                return View(qlchinhanhVM);
            }
             await _chinhanhService.InsertChiNhanh(qlchinhanhVM.ChiNhanh);
            return RedirectToAction("AddChiNhanh", "ChiNhanh");
        }


        public IActionResult SubmitChiNhanh(ChiNhanhViewModel chinhanhVM )
        {
            if (!ModelState.IsValid)
            {
                return View(chinhanhVM);
            }
            _chinhanhService.InsertChiNhanh(chinhanhVM);
            return RedirectToAction("AddChiNhanh","ChiNhanh");
        }
        [HttpGet]
        public IActionResult ChitietChiNhanh()
        {
            var Cn = new ChiTietChiNhanhViewModel();
            Cn.ListShipperCN = new List<ShipperChiNhanhViewModel>();
            Cn.ListChiNhanhQLX = new List<ChiNhanhQLXViewModel>();
            Cn.ListShipperMaxLuong = new List<ShipperMaxLuongViewModel>();
            Cn.ListThongKe = new List<ThongKeShipperViewModel>();
            return View(Cn);
        }
        [HttpPost]
        public async Task<IActionResult> ChitietChiNhanh(int machinhanh,float chiso,int minimumluong,int minimumshipper)
        {
            var Cn = new ChiTietChiNhanhViewModel();
            Cn.ListShipperCN = new List<ShipperChiNhanhViewModel>();
            Cn.ListChiNhanhQLX = new List<ChiNhanhQLXViewModel>();
            Cn.ListShipperMaxLuong = new List<ShipperMaxLuongViewModel>();
            Cn.ListThongKe = new List<ThongKeShipperViewModel>();
            if (machinhanh != 0)
            {
                Cn.ListShipperCN = await _chinhanhService.GetListShipperCN(machinhanh);
                return View(Cn);
            }
            if (chiso != 0)
            {
                Cn.ListChiNhanhQLX = await _chinhanhService.GetListChiNhanhQLX(chiso);
                return View(Cn);
            }
            if (minimumluong != 0)
            {
                Cn.ListShipperMaxLuong = await _chinhanhService.GetListShipperMaxLuong(minimumluong);
                return View(Cn);
            }
            if (minimumshipper != 0)
                Cn.ListThongKe = await _chinhanhService.GetListThongKe(minimumshipper);
            return View(Cn);
        }
        public async Task<IActionResult> ChiNhanhMagagement(int id)
        {
            var list = await _chinhanhService.GetDetailChiNhanh(id);
            return View(list);
        }
        [HttpGet]
        public async Task<IActionResult> UpdateChiNhanh(int id)
        {
            QuanLyChiNhanhViewModel result = await _chinhanhService.GetListPageUpdateChiNhanh(id);
            return View(result);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateChiNhanh(QuanLyChiNhanhViewModel qlchinhanhVM)
        {
            if (!ModelState.IsValid)
            {
                return View(qlchinhanhVM);
            }
           await _chinhanhService.UpdateChiNhanh(qlchinhanhVM.ChiNhanh);
            return RedirectToAction("AddChiNhanh", "ChiNhanh");
        }
        public async Task<IActionResult> DeleteChiNhanh(int idChiNhanh)
        {
            await _chinhanhService.DeleteChiNhanh(idChiNhanh);
            return RedirectToAction("AddChiNhanh", "ChiNhanh");
        }
        [HttpGet]
        public async Task<IActionResult> InsertNhanVienChiNhanh()
        {
           EmPloyeeChiNhanhViewModel  nvCn = await _chinhanhService.GetListPageNhanVienChiNhanh();
            return View(nvCn);
        }
        [HttpPost]
        public async Task<IActionResult> InsertNhanVienChiNhanh(EmPloyeeChiNhanhViewModel emloyeeCn)
        {
            if (!ModelState.IsValid)
            {
                EmPloyeeChiNhanhViewModel result = await _chinhanhService.GetListPageNhanVienChiNhanh();
                emloyeeCn.ListDonVi = result.ListDonVi;
                emloyeeCn.ListEmployeeNowork = result.ListEmployeeNowork;
                return View(emloyeeCn);
            }
            await _chinhanhService.InsertNhanVienChiNhanh(emloyeeCn.InsertNVCN);
            return RedirectToAction("InsertNhanVienChiNhanh", "ChiNhanh");
        }
        public async Task<IActionResult> ChiNhanhMagagementEmployee(int id)
        {
            var list = await _chinhanhService.GetDetailChiNhanh(id);
            return View(list);
        }
        public async Task<IActionResult> DeleteNhanVienChiNhanh(Guid idNVChiNhanh)
        {
            await _chinhanhService.DeleteNVChiNhanh(idNVChiNhanh);
            return RedirectToAction("InsertNhanVienChiNhanh", "ChiNhanh");
        }
        [HttpGet]
        public async Task<IActionResult> UpdateNhanVienChiNhanh(Guid id)
        {
            var list = await _chinhanhService.GetNhanVienChiNhanh(id);
            return View(list);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateNhanVienChiNhanh(UpdateEmployeeViewModel UpdateEmployee)
        {
            await _chinhanhService.UpdateNV(UpdateEmployee);
            return RedirectToAction("InsertNhanVienChiNhanh", "ChiNhanh");
        }
    }
}
