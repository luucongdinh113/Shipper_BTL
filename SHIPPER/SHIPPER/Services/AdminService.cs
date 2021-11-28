﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SHIPPER.Data;
using SHIPPER.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SHIPPER.Services
{
    public class AdminService:IAdminService
    {
        private readonly Shipper10DBContext _context;
        private string _connectionString;
        public AdminService(
             Shipper10DBContext context,
             IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<List<KhachHangViewModel>> GetKhachHangAsync()
        {
            var account = await (from k in _context.KhachHang
                                 where k.isActive== 1
                                 select new KhachHangViewModel
                                 {
                                     Ho = k.Ho,
                                     TenLot=k.TenLot,
                                     Ten=k.Ten,
                                     Cmnd= (int)k.CccdorVisa,
                                     GioiTinh=k.GioiTinh
                                 }).ToListAsync();
            return account;
        }
        public async Task<List<KhachHangUuDaiViewModel>> TimUuDaiKhachHang(int cmnd)
        {
            var list = new List<KhachHangUuDaiViewModel>();
            using (SqlConnection cus = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("timKhachHangUuDai", cus);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@cmnd", cmnd);
                cus.Open();
                SqlDataReader customer = cmd.ExecuteReader();
                while (customer.Read())
                {
                    KhachHangUuDaiViewModel khachHang = new KhachHangUuDaiViewModel
                    {
                        FullName = customer["Fullname"].ToString(),
                        Discount = customer["discount"].ToString(),
                        Mota= customer["mota"].ToString(),
                        DieuKien =customer["dieuKienApDung"].ToString(),
                        NgayHetHan=Convert.ToDateTime(customer["ngayHetHan"].ToString()),
                    };
                    list.Add(khachHang);
                }
                cus.Close();
            }
            return list;
        }
        public void DeleteKhachHang(int cmnd)
        {
            var khachHang = (from p in _context.KhachHang
                           where p.CccdorVisa==cmnd
                           select p).FirstOrDefault();
            khachHang.isActive = 0;
            _context.SaveChanges();
        }
        public async Task<List<ChiTietDonViewModel>> GetChiTietDonMonAnAsync()
        {
            var chiTiet = await (from k in _context.ChiTietDonMonAn
                                 select new ChiTietDonViewModel
                                 {
                                     MaMonAn=k.MaMonAn,
                                     MaDonMonAn=k.MaDonMonAn,
                                     ApDungUuDai= (bool)k.ApDungUuDai,
                                     DonGiaMon= (int)k.DonGiaMon,
                                     DonGiaUuDai= (int)k.DonGiaUuDai,
                                     SoLuong= (int)k.SoLuong
                                 }).ToListAsync();
            return chiTiet;
        }
        public void DeleteChiTietDonMonAn(int maDon,int maMon)
        {
            var chiTiet = (from p in _context.ChiTietDonMonAn
                             where p.MaDonMonAn==maDon && p.MaMonAn==maMon
                             select p).FirstOrDefault();
            _context.Remove(chiTiet);
            _context.SaveChanges();
        }
        public void UpdateChiTietDonMonAn(int maDon, int maMon)
        {
            var chiTiet = (from p in _context.ChiTietDonMonAn
                           where p.MaDonMonAn == maDon && p.MaMonAn == maMon
                           select p).FirstOrDefault();
            _context.SaveChanges();
        }
    }
}
