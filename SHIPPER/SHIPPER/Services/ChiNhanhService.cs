using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SHIPPER.Data;
using SHIPPER.Data.Entities;
using SHIPPER.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SHIPPER.Services
{
    public class ChiNhanhService:IChiNhanhService
    {
        private readonly Shipper10DBContext _context;
        private string _connectionString;
        public ChiNhanhService(
             Shipper10DBContext context,
             IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> InsertChiNhanh(ChiNhanhViewModel chinhanh)
        {
             await using (SqlConnection customer = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("Insert_Chinhanh", customer);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tenchinhanh", chinhanh.TenChiNhanh);
                cmd.Parameters.AddWithValue("@masothue", chinhanh.MaSoThue);
                cmd.Parameters.AddWithValue("@diachi", chinhanh.DiaChi);
                cmd.Parameters.AddWithValue("@maNVQuanLy", chinhanh.MaNhanVienQuanLy);
                cmd.Parameters.AddWithValue("@maChiNhanhCha", chinhanh.MaChiNhanhCha);
                try
                {
                    customer.Open();
                    cmd.ExecuteNonQuery();
                    customer.Close();
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
            return true;
        }
        public async Task<bool> UpdateChiNhanh(ChiNhanhViewModel chinhanh)
        {
            var objmaquanly = await (from f in _context.QuanLi
                               where f.MaNhanVien == chinhanh.MaNhanVienQuanLy
                               select f
                               ).FirstOrDefaultAsync();
            var objNhanvienChiNhanh = await (from f in _context.NhanVienChiNhanh
                                             where f.MaNhanVien == chinhanh.MaNhanVienQuanLy
                                             select f
                                             ).FirstOrDefaultAsync();
            var nVCN = new NhanVienChiNhanh();
            nVCN.MaNhanVien = chinhanh.MaNhanVienQuanLy;
            nVCN.MaDonVi = chinhanh.MaChiNhanh;
            var qLi = new QuanLi();
            qLi.MaNhanVien = chinhanh.MaNhanVienQuanLy;

            if (objNhanvienChiNhanh == null && chinhanh.MaNhanVienQuanLy.ToString() != "00000000-0000-0000-0000-000000000000")
                _context.NhanVienChiNhanh.Add(nVCN);
            if (objmaquanly == null && chinhanh.MaNhanVienQuanLy.ToString() != "00000000-0000-0000-0000-000000000000")
                _context.QuanLi.Add(qLi);
            var objchiNhanh = (from f in _context.ChiNhanh
                            where f.MaDonVi == chinhanh.MaChiNhanh
                            select f
                            ).FirstOrDefault();
            objchiNhanh.MaDonVi = chinhanh.MaChiNhanh;
            if (chinhanh.MaNhanVienQuanLy.ToString() != "00000000-0000-0000-0000-000000000000")
                objchiNhanh.MaNvquanLy = chinhanh.MaNhanVienQuanLy;
            objchiNhanh.MaSoThue = chinhanh.MaSoThue;
            objchiNhanh.TenChiNhanh = chinhanh.TenChiNhanh;
            objchiNhanh.MaChiNhanhCha = chinhanh.MaChiNhanhCha;
            objchiNhanh.IsACtive = chinhanh.TrangThai;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteChiNhanh(int maChiNhanh)
        {
            var objchiNhanh = (from f in _context.ChiNhanh
                               where f.MaDonVi == maChiNhanh
                               select f
                           ).FirstOrDefault();
            _context.Remove(objchiNhanh);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<QuanLyChiNhanhViewModel> GetListPageChiNhanh()
        {
            QuanLyChiNhanhViewModel pageChiNhanh = new QuanLyChiNhanhViewModel();
            pageChiNhanh.ListChiNhanh = new List<ChiNhanhViewModel>();
            pageChiNhanh.ListQuanLy = new List<QuanLyViewModel>();
            var query = from f in _context.NhanVien
                        where f.LoaiNhanVien =="Quan ly" && f.IsActive == true
                        select f;
            var queryMqlQL = from f in _context.QuanLi
                        select f;
            var listMqlinTableQL = queryMqlQL.Select(x => x.MaNhanVien).ToList();
            var listmaql = query.Select(x => x.MaNhanVien).ToList();
            var listQlnowork = listmaql.Except(listMqlinTableQL).ToList();
            if (listQlnowork.Count() > 0)
            {
                query = from f in query
                        where listQlnowork.Contains(f.MaNhanVien)
                        select f;

                foreach (var row in query)
                {
                    pageChiNhanh.ListQuanLy.Add(new QuanLyViewModel
                    {
                        MaNhanVienQuanLy = row.MaNhanVien,
                        HoVaTen = row.Ho + ' ' + row.TenLot + ' ' + row.Ten,
                        Luong = (int)row.Luong,
                        NgaySinh = (System.DateTime)row.NgaySinh,
                        LoaiNhanVien = row.LoaiNhanVien
                    });
                }
            }
            pageChiNhanh.ListChiNhanh = await (
                                            from f in _context.ChiNhanh
                                            select new ChiNhanhViewModel
                                            {
                                                MaChiNhanh = f.MaDonVi,
                                                TenChiNhanh = f.TenChiNhanh,
                                                MaSoThue = (int)f.MaSoThue,
                                                DiaChi = f.DiaChi,
                                                MaNhanVienQuanLy = (System.Guid)f.MaNvquanLy,
                                                MaChiNhanhCha = (int)f.MaChiNhanhCha,
                                                SoLuongNhanVien= (int)f.SoLuongNhanVien,
                                                TrangThai = (bool)f.IsACtive,
                                            }
                                            ).ToListAsync();
            return pageChiNhanh;
        }
        public async Task<QuanLyChiNhanhViewModel> GetListPageUpdateChiNhanh(int id)
        {
            QuanLyChiNhanhViewModel pageUpdateChiNhanh = new QuanLyChiNhanhViewModel();
            pageUpdateChiNhanh.ListChiNhanh = new List<ChiNhanhViewModel>();
            pageUpdateChiNhanh.ListQuanLy = new List<QuanLyViewModel>();
            pageUpdateChiNhanh.ChiNhanh = new ChiNhanhViewModel();
            var query = from f in _context.NhanVien
                        where f.LoaiNhanVien == "Quan ly" && f.IsActive == true
                        select f;
            var queryMqlQL = from f in _context.QuanLi
                             select f;
            var listMqlinTableQL = queryMqlQL.Select(x => x.MaNhanVien).ToList();
            var listmaql = query.Select(x => x.MaNhanVien).ToList();
            var listQlnowork = listmaql.Except(listMqlinTableQL).ToList();
            if (listQlnowork.Count() > 0)
            {
                query = (from f in query
                        where listQlnowork.Contains(f.MaNhanVien)
                        select f).OrderByDescending(c=>c.Luong);

                foreach (var row in query)
                {
                    pageUpdateChiNhanh.ListQuanLy.Add(new QuanLyViewModel
                    {
                        MaNhanVienQuanLy = row.MaNhanVien,
                        HoVaTen = row.Ho + ' ' + row.TenLot + ' ' + row.Ten,
                        Luong = (int)row.Luong,
                        NgaySinh = (System.DateTime)row.NgaySinh,
                        LoaiNhanVien = row.LoaiNhanVien
                    });
                }
            }
           
            pageUpdateChiNhanh.ChiNhanh = await (from f in _context.ChiNhanh
                                                 where f.MaDonVi == id
                                                 select new ChiNhanhViewModel()
                                                 {
                                                     MaChiNhanh=f.MaDonVi,
                                                     TenChiNhanh = f.TenChiNhanh,
                                                     MaSoThue = (int)f.MaSoThue,
                                                     DiaChi = f.DiaChi,
                                                     MaNhanVienQuanLy = (System.Guid)f.MaNvquanLy,
                                                     MaChiNhanhCha = (int)f.MaChiNhanhCha,
                                                     TrangThai = (bool)f.IsACtive
                                                 }
                                                 ).FirstOrDefaultAsync();

            return pageUpdateChiNhanh;
        }

        public async Task<List<NhanVienChiNhanhViewModel>> GetDetailChiNhanh(int id)
        {
            var list = new List<NhanVienChiNhanhViewModel>();
            await using (SqlConnection cus = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("DanhsachNhanVienChiNhanhX", cus);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@maDonVi", id);
                cus.Open();
                SqlDataReader customer = cmd.ExecuteReader();
                while (customer.Read())
                {
                    NhanVienChiNhanhViewModel khachHang = new NhanVienChiNhanhViewModel
                    {
                        MaNhanVien = (Guid)customer["maNhanVien"],
                        Hovaten = customer["HovaTen"].ToString(),
                        Luong = Convert.ToInt32(customer["luong"]),
                        Ngayvaolam = DateTime.Parse(customer["ngayVaoLam"].ToString()),
                        Loainhanvien = customer["loaiNhanVien"].ToString(),
                        TrangThai = (bool)customer["isActive"],
                    };
                    list.Add(khachHang);
                }
                cus.Close();
            }
            return list;
        }
        public async Task<List<ShipperChiNhanhViewModel>> GetListShipperCN(int id)
        {
            var list = new List<ShipperChiNhanhViewModel>();
            await using (SqlConnection cus = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("DanhsachShipperChiNhanhX", cus);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@maDonVi", id);
                cus.Open();
                SqlDataReader customer = cmd.ExecuteReader();
                while (customer.Read())
                {
                    ShipperChiNhanhViewModel khachHang = new ShipperChiNhanhViewModel
                    {
                        HovaTen = customer["HovaTen"].ToString(),
                        LoaiNhanVien = customer["loaiNhanVien"].ToString(),
                        Luong = Convert.ToInt32(customer["luong"]),
                        NgayVaoLam = DateTime.Parse(customer["ngayVaoLam"].ToString()),
                    };
                    list.Add(khachHang);
                }
                cus.Close();
            }
            return list;
        }
        public async Task<List<ChiNhanhQLXViewModel>> GetListChiNhanhQLX(float id)
        {
            var list = new List<ChiNhanhQLXViewModel>();
            await using (SqlConnection cus = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("DSChiNhanhQLUytinX", cus);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@csuytin", id);
                cus.Open();
                SqlDataReader customer = cmd.ExecuteReader();
                while (customer.Read())
                {
                    ChiNhanhQLXViewModel khachHang = new ChiNhanhQLXViewModel
                    {
                        MaDonVi= Convert.ToInt32(customer["maDonVi"]),
                        TenChiNhanh = customer["tenChiNhanh"].ToString(),
                        MaSoThue = (int)customer["maSoThue"],
                        DiaChi = customer["diaChi"].ToString(),
                        SoLuongNhanVien = Convert.ToInt32(customer["soLuongNhanVien"]),
                    };
                    list.Add(khachHang);
                }
                cus.Close();
            }
            return list;
        }
        public async Task<List<ShipperMaxLuongViewModel>> GetListShipperMaxLuong(int id)
        {
            var list = new List<ShipperMaxLuongViewModel>();
            await using (SqlConnection cus = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("ListShipperHaveMaxLuongPerChiNhanh", cus);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@mimimumluong", id);
                cus.Open();
                SqlDataReader customer = cmd.ExecuteReader();
                while (customer.Read())
                {
                    ShipperMaxLuongViewModel khachHang = new ShipperMaxLuongViewModel
                    {
                        MaNhanVien= (Guid)customer["maNhanVien"],
                        HovaTen = customer["Hovaten"].ToString(),
                        Luong = Convert.ToInt32(customer["luong"]),
                        Ngaysinh = (DateTime)customer["ngaySinh"],
                        NgayVaoLam = (DateTime)customer["ngayVaoLam"],
                        Madonvi = Convert.ToInt32(customer["maDonVi"]),
                    };
                    list.Add(khachHang);
                }
                cus.Close();
            }
            return list;
        }
        public async Task<List<ThongKeShipperViewModel>> GetListThongKe(int id)
        {
            var list = new List<ThongKeShipperViewModel>();
            await using (SqlConnection cus = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("NumofShipperPerChiNhanh", cus);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@numShipper", id);
                cus.Open();
                SqlDataReader customer = cmd.ExecuteReader();
                while (customer.Read())
                {
                    ThongKeShipperViewModel khachHang = new ThongKeShipperViewModel
                    {
                        MaDonVi = Convert.ToInt32(customer["maDonVi"]),
                        SoluongShipper = Convert.ToInt32(customer["SoluongShipper"]),
                    };
                    list.Add(khachHang);
                }
                cus.Close();
            }
            return list;
        }
        // work for insert nhan vien chi nhanh
        public async Task<EmPloyeeChiNhanhViewModel> GetListPageNhanVienChiNhanh()
        {
            EmPloyeeChiNhanhViewModel pageChiNhanh = new EmPloyeeChiNhanhViewModel();
            pageChiNhanh.ListEmployeeNowork = new List<EmployeeNowork>();
            pageChiNhanh.ListDonVi = new List<DonVi>();
            //get list nhan vien ơ trạng thái làm việc
            var query = from f in _context.NhanVien
                        where f.IsActive == true
                        select f;
            // get list nhan vien đã làm ở các chi nhánh
            var queryMqlQL = from f in _context.NhanVienChiNhanh
                             select f;
            // ma nhan vien đã làm việc ở chi nhánh
            var listMqlinTableQL = queryMqlQL.Select(x => x.MaNhanVien).ToList();
            // ma nhan vien toàn bộ công ty
            var listmaql = query.Select(x => x.MaNhanVien).ToList();
            // mã nhân viên chưa dc phân công vào chi nhánh
            var listQlnowork = listmaql.Except(listMqlinTableQL).ToList();
            if (listQlnowork.Count() > 0)
            {
                query = from f in query
                        where listQlnowork.Contains(f.MaNhanVien)
                        select f;

                foreach (var row in query)
                {
                    pageChiNhanh.ListEmployeeNowork.Add(new EmployeeNowork
                    {
                        MaNhanVien = row.MaNhanVien,
                        Hovaten = row.Ho + ' ' + row.TenLot + ' ' + row.Ten,
                        Luong = (int)row.Luong,
                        Loainhanvien = row.LoaiNhanVien, 
                        TrangThai = (bool)row.IsActive,
                    });
                }
            }
            pageChiNhanh.ListDonVi = await (
                                            from f in _context.ChiNhanh
                                            select new DonVi
                                            {
                                                MaDonVi = f.MaDonVi,
                                                TenChiNhanh = f.TenChiNhanh,
                                                MaSoThue = (int)f.MaSoThue,
                                                DiaChi = f.DiaChi,
                                                SoLuongNhanVien = (int)f.SoLuongNhanVien,
                                            }
                                            ).ToListAsync();
            return pageChiNhanh;
        }
        public async Task<bool> InsertNhanVienChiNhanh(InsertNhanVienChiNhanh nvCn)
        {
            var newNVCN = new NhanVienChiNhanh()
            {
                MaDonVi = nvCn.MaDonVi,
                MaNhanVien = nvCn.MaNhanVien
            };
            _context.NhanVienChiNhanh.Add(newNVCN);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteNVChiNhanh(Guid maNV)
        {
            var objNvchiNhanh = (from f in _context.NhanVienChiNhanh
                               where f.MaNhanVien==maNV
                               select f
                           ).FirstOrDefault();
            _context.Remove(objNvchiNhanh);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<UpdateEmployeeViewModel> GetNhanVienChiNhanh(Guid id)
        {
            UpdateEmployeeViewModel employee = new UpdateEmployeeViewModel();
            employee.ListDonVi = new List<DonVi>();
            employee.InsertNVCN = new InsertNhanVienChiNhanh();
            employee.InsertNVCN = await (from f in _context.NhanVienChiNhanh
                                         where f.MaNhanVien == id
                                         select new InsertNhanVienChiNhanh
                                         {
                                             MaNhanVien =f.MaNhanVien,
                                             MaDonVi =f.MaDonVi,
                                         }).FirstOrDefaultAsync();
            employee.ListDonVi = await (
                                            from f in _context.ChiNhanh
                                            select new DonVi
                                            {
                                                MaDonVi = f.MaDonVi,
                                                TenChiNhanh = f.TenChiNhanh,
                                                MaSoThue = (int)f.MaSoThue,
                                                DiaChi = f.DiaChi,
                                                SoLuongNhanVien = (int)f.SoLuongNhanVien,
                                            }
                                            ).ToListAsync();
            return employee;
        }
        public async Task<bool> UpdateNV(UpdateEmployeeViewModel nv)
        {
            var obj = await (
                            from f in _context.NhanVienChiNhanh
                            where f.MaNhanVien == nv.InsertNVCN.MaNhanVien
                            select f
                            ).FirstOrDefaultAsync();
            obj.MaDonVi = nv.InsertNVCN.MaDonVi;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
