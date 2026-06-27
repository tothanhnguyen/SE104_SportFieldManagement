# KỊCH BẢN VIDEO SEMINAR — DISTRIBUTED SYSTEMS & MICROSERVICES

**Chủ đề:** 4 cơ chế giữ cho hệ phân tán "sống" — Heartbeats · Service Discovery · Consensus · Distributed Locking
**Thời lượng mục tiêu:** ~6 phút | **Định dạng:** Storyboard (phân cảnh)
**Phong cách hình ảnh:** Motion graphic phẳng, màu chủ đạo xanh dương + cam nhấn, icon node hình tròn, đường nối animate.

> **Cách đọc bảng:** mỗi cảnh gồm **HÌNH ẢNH** (cái khán giả thấy) · **ON-SCREEN** (chữ hiện trên màn) · **VOICE-OVER** (lời đọc). Con số ⏱ là thời lượng gợi ý.

---

## 🎬 CẢNH 0 — MỞ ĐẦU (Hook) ⏱ 0:00–0:20

| Yếu tố | Nội dung |
|---|---|
| **HÌNH ẢNH** | Màn hình điện thoại: app ngân hàng, Netflix, đơn hàng đang tải. Camera zoom-out → lộ ra hàng chục server nhỏ nối nhau bằng đường sáng. Một server bỗng tắt đèn (đỏ). |
| **ON-SCREEN** | `Một cú chạm = hàng trăm service nói chuyện với nhau` |
| **VOICE-OVER** | "Mỗi lần bạn xem phim, chuyển khoản hay đặt hàng — phía sau là hàng trăm dịch vụ chạy trên nhiều máy khác nhau. Vấn đề: máy có thể hỏng, mạng có thể rớt, bất cứ lúc nào. Vậy hệ thống làm sao để vẫn sống? Hôm nay: 4 cơ chế cốt lõi." |

---

## 🫀 PHẦN 1 — HEARTBEATS ⏱ 0:20–1:40

### CẢNH 1 — Heartbeat là gì ⏱ 0:20–0:50
| Yếu tố | Nội dung |
|---|---|
| **HÌNH ẢNH** | 2 server. Server1 phát ra "sóng nhịp tim" (đường ECG xanh) bay sang Server2 kèm bong bóng thoại *"Hey, tôi còn sống!"*. Server2 nhấp nháy đáp *"OK, bro"*. Lặp lại đều đặn như nhịp tim. |
| **ON-SCREEN** | `HEARTBEAT = tín hiệu định kỳ "Tôi vẫn ở đây!"` |
| **VOICE-OVER** | "Heartbeat — nhịp tim — là thông điệp nhỏ gửi đều đặn giữa các thành phần, vài giây một lần, chỉ để nói: tôi vẫn còn sống và đang chạy bình thường." |

### CẢNH 2 — Khi nhịp tim ngừng ⏱ 0:50–1:15
| Yếu tố | Nội dung |
|---|---|
| **HÌNH ẢNH** | Monitor (mắt) theo dõi 3 node. Node giữa bỏ lỡ 3 nhịp → đường ECG thành đường thẳng → node chuyển đỏ "DEAD". Monitor kích hoạt: chuyển việc sang node xanh khác (mũi tên cam). |
| **ON-SCREEN** | `Bỏ lỡ nhiều nhịp → coi như "chết" → tự khôi phục` |
| **VOICE-OVER** | "Một bên gửi nhịp, một bên giám sát. Nếu bỏ lỡ vài nhịp liên tiếp, hệ thống coi node đó đã chết, rồi tự xử lý: chuyển tải sang node khỏe, khởi động lại, hoặc báo admin." |

### CẢNH 3 — Đánh đổi & thực tế ⏱ 1:15–1:40
| Yếu tố | Nội dung |
|---|---|
| **HÌNH ẢNH** | Thanh trượt "Tần suất nhịp": kéo về DÀY → biểu tượng mạng nghẽn; kéo về THƯA → đồng hồ phát hiện lỗi chậm. Cuối cảnh: 3 logo **Kubernetes · Database Replication · Elasticsearch** hiện lên. |
| **ON-SCREEN** | `Quá dày = tốn mạng · Quá thưa = phát hiện chậm · Cảnh giác báo động giả` |
| **VOICE-OVER** | "Mấu chốt là cân bằng: gửi quá dày thì nghẽn mạng, quá thưa thì phát hiện lỗi chậm — và coi chừng báo động giả khi node chỉ chậm chứ chưa chết. Heartbeat có mặt khắp nơi: Kubernetes, sao chép cơ sở dữ liệu, Elasticsearch." |

---

## 🧭 PHẦN 2 — SERVICE DISCOVERY ⏱ 1:40–3:00

### CẢNH 4 — Vấn đề ⏱ 1:40–2:05
| Yếu tố | Nội dung |
|---|---|
| **HÌNH ẢNH** | Bên trái: 1 server "ngày xưa" với địa chỉ IP cố định ghim cứng. Bên phải: hàng trăm container bật/tắt liên tục, IP đổi xoành xoạch. Một service cầm tờ giấy IP cũ → gọi tới ô trống → ❌. |
| **ON-SCREEN** | `IP đổi liên tục → ghi cứng địa chỉ = sập` |
| **VOICE-OVER** | "Ngày xưa app chạy một máy, IP cố định, gọi nhau dễ. Nay hàng trăm microservice co giãn từng phút, IP đổi liên tục. Ghi cứng địa chỉ trong file cấu hình? Một node đổi chỗ là cả dây chuyền đổ." |

### CẢNH 5 — Lời giải: Service Registry ⏱ 2:05–2:30
| Yếu tố | Nội dung |
|---|---|
| **HÌNH ẢNH** | Cuốn "danh bạ" trung tâm (Service Registry) sáng lên. Service mới khởi động → tự ghi tên + IP + trạng thái vào danh bạ. Service khác hỏi "payment-service ở đâu?" → danh bạ trả về địa chỉ hiện tại. |
| **ON-SCREEN** | `SERVICE REGISTRY = danh bạ thời gian thực (tên · IP · sức khỏe)` |
| **VOICE-OVER** | "Service Discovery giải quyết việc đó: một danh bạ trung tâm — service registry — luôn cập nhật ai đang sống, ở đâu. Service tự đăng ký khi bật, tự xóa khi tắt. Không cần con người, không cần ghi cứng." |

### CẢNH 6 — Client-side vs Server-side ⏱ 2:30–3:00
| Yếu tố | Nội dung |
|---|---|
| **HÌNH ẢNH** | Màn chia đôi. **Trái (Client-side):** client tự hỏi danh bạ rồi tự chọn server → nhãn *Netflix Eureka*. **Phải (Server-side):** client gọi 1 Load Balancer/Gateway, nó hỏi danh bạ và định tuyến hộ → nhãn *AWS ELB*. |
| **ON-SCREEN** | `Client-side: client tự tìm (Eureka)` / `Server-side: gateway tìm hộ (AWS ELB)` |
| **VOICE-OVER** | "Hai kiểu: Client-side — client tự tra danh bạ và chọn đích, ví dụ Netflix Eureka. Server-side — client chỉ gọi một cổng trung tâm, cổng đó tìm và định tuyến hộ, ví dụ AWS Load Balancer. Đơn giản cho client, nhưng cổng đó phải thật khỏe." |

---

## 🗳️ PHẦN 3 — CONSENSUS (ĐỒNG THUẬN) ⏱ 3:00–4:30

### CẢNH 7 — Bài toán đồng thuận ⏱ 3:00–3:25
| Yếu tố | Nội dung |
|---|---|
| **HÌNH ẢNH** | 5 node tròn nối nhau. Node A phát loa: *"Tôi chọn giá trị V — đồng ý không?"*. 4 node gật ✅ nhưng 1 node lắc đầu *"Tôi thích W"*. Dấu chấm hỏi to. |
| **ON-SCREEN** | `Nhiều node — phải cùng chốt MỘT giá trị` |
| **VOICE-OVER** | "Nhiều máy cùng xử lý, nhưng phải thống nhất MỘT giá trị chung để phối hợp — đó là đồng thuận. Dễ khi tất cả gật đầu; rắc rối bắt đầu khi có node phản đối." |

### CẢNH 8 — 3 điều kiện & 2 loại lỗi ⏱ 3:25–3:55
| Yếu tố | Nội dung |
|---|---|
| **HÌNH ẢNH** | 3 huy hiệu hiện ra: **Agreement · Validity · Termination**. Chuyển cảnh: 1 node tắt ngúm (*Crash*) vs 1 node "hai mặt" gửi tin khác nhau cho từng bên (*Byzantine*, mặt nạ phản bội). |
| **ON-SCREEN** | `3 điều kiện: Nhất trí · Hợp lệ · Kết thúc` — `Lỗi: Crash (dễ) vs Byzantine (khó)` |
| **VOICE-OVER** | "Thuật toán đồng thuận phải đạt 3 điều: tất cả node tốt cùng chọn một giá trị, giá trị đó do node tốt đề xuất, và mọi node đều đi đến quyết định. Hai loại lỗi: Crash — node chết, dễ xử lý; Byzantine — node nói dối, gửi tin mâu thuẫn, khó hơn nhiều." |

### CẢNH 9 — Byzantine Generals & thuật toán thực tế ⏱ 3:55–4:30
| Yếu tố | Nội dung |
|---|---|
| **HÌNH ẢNH** | Minh họa cổ: nhiều tướng bao vây thành, gửi thư qua người đưa tin, vài tướng là kẻ phản bội. Thanh "2/3 trung thực" lấp đầy → quân đồng loạt tấn công 🗡️. Cuối cảnh logo: **Raft (etcd/K8s) · Paxos (Google) · PoW (Bitcoin) · PoS (Ethereum)**. |
| **ON-SCREEN** | `>2/3 trung thực → đạt đồng thuận` · `Raft · Paxos · PoW · PoS` |
| **VOICE-OVER** | "Hình dung bài toán các tướng Byzantine: phải cùng giờ tấn công dù có kẻ phản bội. Lamport chứng minh: nếu hơn hai phần ba node trung thực, vẫn đạt được đồng thuận. Thực tế: Raft và Paxos dùng trong Kubernetes, Google; còn blockchain dùng Proof of Work, Proof of Stake." |

---

## 🔒 PHẦN 4 — DISTRIBUTED LOCKING ⏱ 4:30–5:50

### CẢNH 10 — Vì sao cần khóa ⏱ 4:30–4:55
| Yếu tố | Nội dung |
|---|---|
| **HÌNH ẢNH** | 2 client cùng lao vào sửa 1 ô dữ liệu (vé cuối cùng / số dư) → xung đột, dữ liệu nhấp nháy đỏ. Cắt sang: một ổ khóa gắn lên tài nguyên, chỉ 1 client cầm chìa được vào. |
| **ON-SCREEN** | `1 tài nguyên dùng chung → chỉ 1 người được sửa cùng lúc` |
| **VOICE-OVER** | "Khi nhiều tiến trình trên nhiều máy cùng giành một tài nguyên — vé cuối, số dư tài khoản — ta cần khóa phân tán để mỗi lúc chỉ một bên được thao tác, tránh hỏng dữ liệu." |

### CẢNH 11 — Redlock & lease ⏱ 4:55–5:20
| Yếu tố | Nội dung |
|---|---|
| **HÌNH ẢNH** | 5 node Redis xếp hàng. Client xin khóa trên cả 5; 3 node sáng xanh ✅ → "GIỮ KHÓA" (majority 3/5). Đồng hồ đếm ngược "lease" gắn trên khóa — hết giờ tự nhả. |
| **ON-SCREEN** | `Redlock: giành đa số 3/5 node = giữ khóa` · `Lease = khóa tự hết hạn` |
| **VOICE-OVER** | "Một node Redis đơn là điểm hỏng duy nhất, nên Redlock chạy trên năm node độc lập: giành được đa số — ba trên năm — mới coi là giữ khóa. Kèm theo lease: khóa tự hết hạn để một bên sập không khóa chết cả hệ thống." |

### CẢNH 12 — Cạm bẫy: Split-brain & Fencing token ⏱ 5:20–5:50
| Yếu tố | Nội dung |
|---|---|
| **HÌNH ẢNH** | Mạng đứt làm đôi (split-brain): hai phía đều tưởng mình giữ khóa 😱. Giải pháp: mỗi lần cấp khóa kèm số tăng dần (token #14, #15). Tài nguyên chỉ nhận token lớn nhất → client cũ (#14) bị chặn ❌, client mới (#15) ✅. |
| **ON-SCREEN** | `Split-brain: 2 bên cùng tưởng giữ khóa` · `Fencing token: chỉ số LỚN NHẤT được ghi` |
| **VOICE-OVER** | "Cạm bẫy lớn nhất: mạng đứt đôi, hai bên cùng tưởng mình giữ khóa — split-brain. Hai lá chắn: quorum (đa số) để chỉ một phía thắng, và fencing token — mỗi lần cấp khóa kèm một số tăng dần; tài nguyên chỉ chấp nhận số lớn nhất, nên kẻ giữ khóa cũ quay lại cũng bị chặn." |

---

## 🏁 CẢNH 13 — KẾT ⏱ 5:50–6:10

| Yếu tố | Nội dung |
|---|---|
| **HÌNH ẢNH** | 4 icon gom lại thành một hệ thống chạy mượt: 🫀 nhịp tim đập đều · 🧭 danh bạ sáng · 🗳️ node bỏ phiếu đồng loạt · 🔒 khóa an toàn. Mạng sáng xanh toàn bộ. |
| **ON-SCREEN** | `Heartbeat phát hiện lỗi · Discovery tìm nhau · Consensus đồng thuận · Locking bảo vệ dữ liệu` |
| **VOICE-OVER** | "Bốn cơ chế, một mục tiêu: giữ hệ phân tán đáng tin cậy. Heartbeat phát hiện ai còn sống, Service Discovery giúp chúng tìm nhau, Consensus để cùng đồng thuận, và Distributed Locking bảo vệ dữ liệu dùng chung. Cảm ơn đã theo dõi!" |

---

## 📝 GHI CHÚ SẢN XUẤT

- **Nhạc nền:** nhịp điệu nhẹ, tăng tempo ở mỗi phần mở đầu, lắng xuống ở narration kỹ thuật.
- **Hiệu ứng âm thanh:** tiếng "bíp" nhịp tim (Phần 1), tiếng "ting" khi tra danh bạ (Phần 2), tiếng gõ búa khi chốt vote (Phần 3), tiếng khóa "cạch" (Phần 4).
- **Bảng màu node:** xanh lá = khỏe, đỏ = chết/lỗi, cam = hành động khôi phục, xám = chưa rõ.
- **Tổng thời lượng:** ~6 phút 10 giây (nằm trong mục tiêu 5–7 phút). Cắt bớt CẢNH 3/6/12 nếu cần về dưới 6 phút.
- **Font chữ on-screen:** sans-serif đậm, tối đa 1 dòng/màn để dễ đọc trên mobile.
