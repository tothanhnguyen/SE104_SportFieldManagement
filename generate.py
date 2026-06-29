import random
import uuid
import datetime

# Fixed seed for reproducibility if needed, but random is fine.
random.seed()

# Configuration
NUM_SAN = 15
HOURS_PER_SAN = 8  # Leads to ~120 timeslots total
NUM_HOIVIEN = 40
NUM_DATSAN = 150

def generate_san_id():
    return f"S{random.randint(10000, 99999)}"

def generate_chitiet_id(san_id):
    hex_str = uuid.uuid4().hex[:4].upper()
    return f"{san_id}-CT{hex_str}"

def generate_hoivien_id():
    return f"HV{random.randint(10000, 99999)}"

datsan_id_counter = 10000
def generate_datsan_id():
    global datsan_id_counter
    datsan_id_counter += 1
    return f"DS{datsan_id_counter}"

loaisan = ['BD', 'CL', 'PB']
tinhtrang = ['HD', 'HD', 'HD', 'HD', 'BT']  # Mostly HD
loaingay = ['NT', 'CT', 'NL']
loaihoivien = ['DO', 'BA', 'VA', 'KC']

san_names = [
    "Chảo Lửa", "K34", "Phú Nhuận", "Tao Đàn", "Thống Nhất", "Kỳ Hòa", "Viettel",
    "Celadon", "D-Court", "Lan Anh", "Hải Đăng", "Bình Thạnh", "Gò Vấp", "Quận 7",
    "Tân Bình", "Cộng Hòa", "Hoàng Hoa Thám", "Lê Đại Hành", "Sư Vạn Hạnh"
]

first_names = ["Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô"]
middle_names = ["Văn", "Thị", "Hoàng", "Minh", "Thu", "Ngọc", "Đình", "Quang", "Hải", "Tuấn", "Thanh", "Mỹ", "Bảo"]
last_names = ["An", "Bình", "Cường", "Dũng", "Em", "Phong", "Giang", "Hải", "Linh", "Oanh", "Trọng", "Vinh", "Đức", "Yến", "Thảo", "Hùng", "Nhung"]

def generate_name():
    return f"{random.choice(first_names)} {random.choice(middle_names)} {random.choice(last_names)}"

sans = []
chitiets = []
hoiviens = []
datsans = []

# Generate San
for i in range(NUM_SAN):
    ls = random.choice(loaisan)
    name = f"Sân {random.choice(san_names)} {i+1}"
    ma_san = generate_san_id()
    sans.append((ma_san, name, ls, random.choice(tinhtrang)))

# Generate ChiTietDatSan (Khung gio)
for san in sans:
    ma_san = san[0]
    # Sân nào cũng phải có ít nhất 2 đến 3 khung giờ có sân cả ngày -> ~8 khung giờ/sân
    start_hour = 5
    for j in range(random.randint(6, 10)):
        if start_hour > 21: break
        end_hour = start_hour + random.randint(1, 2)
        if end_hour > 23: end_hour = 23
        ma_ct = generate_chitiet_id(ma_san)
        ln = random.choice(loaingay)
        chitiets.append((ma_ct, ma_san, f"{start_hour:02d}:00", f"{end_hour:02d}:00", ln))
        start_hour = end_hour

# Generate HoiVien
for i in range(NUM_HOIVIEN):
    ma_hv = generate_hoivien_id()
    gender = random.choice(["Nam", "Nữ"])
    diem = random.randint(0, 400)
    loai = 'DO'
    if diem >= 300: loai = 'KC'
    elif diem >= 200: loai = 'VA'
    elif diem >= 100: loai = 'BA'
    date_reg = f"2025-{random.randint(1,12):02d}-{random.randint(1,28):02d}"
    phone = f"09{random.randint(10000000, 99999999)}"
    email = f"user{i}_{phone}@example.com"
    hoiviens.append((ma_hv, generate_name(), phone, gender, date_reg, diem, loai, email))

# Generate DatSan
booked_slots = set() # Store (MaChiTiet, days_offset) to prevent overlaps

# Mapping dictionaries for calculations
loaingay_price = {'NT': 50000, 'CT': 70000, 'NL': 100000}
loaihoivien_discount = {'DO': 0.0, 'BA': 0.03, 'VA': 0.05, 'KC': 0.10}

for i in range(NUM_DATSAN):
    ma_ds = generate_datsan_id()
    
    hv_tuple = random.choice(hoiviens)
    hv = hv_tuple[0]
    hv_loai = hv_tuple[6]
    
    # Pick a valid slot (Not BT)
    valid_chitiets = [c for c in chitiets if not any(s[0] == c[1] and s[3] == 'BT' for s in sans)]
    if not valid_chitiets:
        break
    
    while True:
        ct_tuple = random.choice(valid_chitiets)
        ct = ct_tuple[0]
        days_offset = random.randint(-40, 5)
        if (ct, days_offset) not in booked_slots:
            booked_slots.add((ct, days_offset))
            break
            
    # Calculate exact duration
    start_h = int(ct_tuple[2].split(':')[0])
    end_h = int(ct_tuple[3].split(':')[0])
    duration = end_h - start_h
    
    # Base price
    base_price = loaingay_price[ct_tuple[4]]
    
    # Discount
    discount = loaihoivien_discount[hv_loai]
    
    # Final price (no 0 VND bookings anymore, future or past doesn't matter)
    tong_tien = int((base_price * duration) * (1.0 - discount))
    
    datsans.append((ma_ds, hv, ct, days_offset, tong_tien))

with open('d:\\SE104\\SE104_SportFieldManagement\\Database\\SampleData.sql', 'w', encoding='utf-8') as f:
    f.write("USE QLSanTheThao;\nGO\n\n")
    f.write("-- ====================================================================\n")
    f.write("-- 1. XÓA DỮ LIỆU TÀI KHOẢN ADMIN CŨ (NẾU CÓ) ĐỂ LÀM SẠCH\n")
    f.write("-- ====================================================================\n")
    f.write("DECLARE @AdminId INT;\nSELECT @AdminId = AccountId FROM ACCOUNT WHERE Username = 'admin';\n")
    f.write("IF @AdminId IS NOT NULL\nBEGIN\n")
    f.write("    EXEC sp_set_session_context @key=N'AccountId', @value=@AdminId;\n")
    f.write("    DELETE FROM DATSAN WHERE AccountId = @AdminId;\n")
    f.write("    DELETE FROM CHITIETDATSAN WHERE MaSan IN (SELECT MaSan FROM SAN WHERE AccountId = @AdminId);\n")
    f.write("    DELETE FROM SAN WHERE AccountId = @AdminId;\n")
    f.write("    DELETE FROM HOIVIEN WHERE AccountId = @AdminId;\n")
    f.write("    DELETE FROM THAMSO WHERE AccountId = @AdminId;\n")
    f.write("    DELETE FROM TICHDIEM WHERE AccountId = @AdminId;\n")
    f.write("    DELETE FROM ACCOUNT WHERE AccountId = @AdminId;\n")
    f.write("END\nGO\n\n")

    f.write("INSERT INTO ACCOUNT (Username, PasswordHash, Email) VALUES ('admin', '12345678', 'admin@hethong.vn');\n")
    f.write("DECLARE @NewAdminId INT; SELECT @NewAdminId = AccountId FROM ACCOUNT WHERE Username = 'admin';\n")
    f.write("EXEC sp_set_session_context @key=N'AccountId', @value=@NewAdminId;\nGO\n\n")

    f.write("IF NOT EXISTS (SELECT 1 FROM LOAIHOIVIEN WHERE MaLoaiHoiVien = 'DO')\n")
    f.write("    INSERT INTO LOAIHOIVIEN (MaLoaiHoiVien, TenLoaiHoiVien, DiemToiThieu, MucGiamGia) VALUES\n")
    f.write("    ('DO', N'Đồng', 0, 0), ('BA', N'Bạc', 100, 0.03), ('VA', N'Vàng', 200, 0.05), ('KC', N'Kim cương', 300, 0.10);\n")
    
    f.write("IF NOT EXISTS (SELECT 1 FROM LOAISAN WHERE MaLoaiSan = 'BD')\n    INSERT INTO LOAISAN (MaLoaiSan, TenLoaiSan) VALUES ('BD', N'Sân bóng đá');\n")
    f.write("IF NOT EXISTS (SELECT 1 FROM LOAISAN WHERE MaLoaiSan = 'CL')\n    INSERT INTO LOAISAN (MaLoaiSan, TenLoaiSan) VALUES ('CL', N'Sân cầu lông');\n")
    f.write("IF NOT EXISTS (SELECT 1 FROM LOAISAN WHERE MaLoaiSan = 'PB')\n    INSERT INTO LOAISAN (MaLoaiSan, TenLoaiSan) VALUES ('PB', N'Sân pickleball');\n")
    
    f.write("IF NOT EXISTS (SELECT 1 FROM TINHTRANG WHERE MaTinhTrang = 'HD')\n    INSERT INTO TINHTRANG (MaTinhTrang, TenTinhTrang) VALUES ('HD', N'Hoạt động'), ('BT', N'Bảo trì');\n")
    
    f.write("IF NOT EXISTS (SELECT 1 FROM LOAINGAY WHERE MaLoaiNgay = 'NT')\n")
    f.write("    INSERT INTO LOAINGAY (MaLoaiNgay, TenLoaiNgay, DonGiaNgay) VALUES ('NT', N'Ngày thường', 50000), ('CT', N'Cuối tuần', 70000), ('NL', N'Ngày lễ', 100000);\nGO\n\n")

    f.write("INSERT INTO THAMSO (MucDiemTichLuyMacDinh, MaLoaiHoiVienMacDinh, TinhTrangKhongDuocDat) VALUES (0, 'DO', 'BT');\n")
    f.write("INSERT INTO TICHDIEM (HeSoTichDiem) VALUES (100000);\nGO\n\n")

    f.write("-- SÂN\n")
    for s in sans:
        f.write(f"INSERT INTO SAN (MaSan, TenSan, DiaChi, GhiChu, MaLoaiSan, MaTinhTrang) VALUES ('{s[0]}', N'{s[1]}', N'TP.HCM', N'', '{s[2]}', '{s[3]}');\n")
    f.write("GO\n\n")

    f.write("-- KHUNG GIỜ CHI TIẾT\n")
    for chunk in [chitiets[i:i+50] for i in range(0, len(chitiets), 50)]:
        for c in chunk:
            f.write(f"INSERT INTO CHITIETDATSAN (MaChiTiet, MaSan, GioBatDau, GioKetThuc, MaLoaiNgay) VALUES ('{c[0]}', '{c[1]}', '{c[2]}', '{c[3]}', '{c[4]}');\n")
        f.write("GO\n")
    f.write("\n")

    f.write("-- HỘI VIÊN\n")
    for h in hoiviens:
        f.write(f"INSERT INTO HOIVIEN (MaHoiVien, HoTen, SDT, Email, GioiTinh, NgayDangKyHoiVien, DiemTichLuy, MaLoaiHoiVien, GhiChu) VALUES ('{h[0]}', N'{h[1]}', '{h[2]}', '{h[7]}', N'{h[3]}', '{h[4]}', {h[5]}, '{h[6]}', N'');\n")
    f.write("GO\n\n")

    f.write("-- ĐẶT SÂN\n")
    for d in datsans:
        date_str = "GETDATE()"
        if d[3] > 0:
            date_str = f"GETDATE() + {d[3]}"
        elif d[3] < 0:
            date_str = f"GETDATE() - {abs(d[3])}"
        f.write(f"INSERT INTO DATSAN (MaDatSan, MaHoiVien, MaChiTiet, NgayDat, TongTien, GhiChu) VALUES ('{d[0]}', '{d[1]}', '{d[2]}', {date_str}, {d[4]}, N'');\n")
    f.write("GO\n\nPRINT N'✅ Đã nạp thành công toàn bộ dữ liệu mẫu khổng lồ!';\n")
