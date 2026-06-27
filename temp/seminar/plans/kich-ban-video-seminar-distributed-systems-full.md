# KỊCH BẢN VIDEO SEMINAR (BẢN FULL) — DISTRIBUTED SYSTEMS & MICROSERVICES

**Chủ đề:** 4 cơ chế giữ cho hệ phân tán đáng tin cậy — Heartbeats · Service Discovery · Consensus · Distributed Locking
**Thời lượng mục tiêu:** ~13–15 phút | **Định dạng:** Kịch bản đầy đủ (lời thoại word-by-word + chỉ dẫn hình ảnh)
**Phong cách:** Motion graphic phẳng, tông xanh dương + cam nhấn; node hình tròn; đường nối animate.

> **Quy ước:** 🎙️ **VO** = lời đọc nguyên văn (thu âm theo đúng câu). 🎬 **HÌNH** = chỉ dẫn dựng hình. 🔤 **ON-SCREEN** = chữ hiện trên màn. ⏱ = mốc thời gian.

---

# PHẦN MỞ ĐẦU ⏱ 0:00–0:40

🎬 **HÌNH:** Cận cảnh điện thoại — app ngân hàng, Netflix, giỏ hàng đang quay vòng loading. Camera kéo lùi xuyên qua màn hình, lộ ra một "thành phố" gồm hàng trăm server nhỏ nối nhau bằng các tia sáng dữ liệu. Một server chợt tắt đèn, chuyển đỏ; lập tức một tia sáng khác vòng qua nó.

🔤 **ON-SCREEN:** `DISTRIBUTED SYSTEMS & MICROSERVICES`
`Một cú chạm = hàng trăm dịch vụ phối hợp`

🎙️ **VO:**
"Mỗi lần bạn xem một bộ phim, chuyển khoản, hay đặt một món hàng — phía sau cú chạm tưởng chừng đơn giản đó là hàng trăm dịch vụ chạy trên nhiều máy, nhiều mạng, nhiều trung tâm dữ liệu khác nhau.

Cách làm này giúp hệ thống mở rộng quy mô, tăng tính sẵn sàng và chia ứng dụng phức tạp thành những phần dễ quản lý. Nhưng nó cũng sinh ra những vấn đề không tồn tại khi chạy trên một máy duy nhất: máy có thể hỏng, tiến trình có thể sập, mạng có thể trễ hoặc mất gói — bất cứ lúc nào, không hề báo trước.

Vậy làm sao để một hệ thống như thế vẫn hoạt động đáng tin cậy? Trong video này, chúng ta đi qua bốn cơ chế nền tảng: Heartbeats để phát hiện lỗi, Service Discovery để các dịch vụ tìm thấy nhau, Consensus để cùng đồng thuận, và Distributed Locking để bảo vệ dữ liệu dùng chung."

---

# CHƯƠNG 1 — HEARTBEATS ⏱ 0:40–4:00

## 1.1 — Heartbeat là gì? ⏱ 0:40–1:20

🎬 **HÌNH:** Hai server đặt cạnh nhau. Từ Server1, một đường sóng ECG (nhịp tim) màu xanh phát ra đều đặn bay sang Server2, kèm bong bóng thoại *"Hey, tôi vẫn còn ở đây và đang chạy!"*. Server2 nhấp nháy đáp *"OK, bro"*. Nhịp lặp lại theo chu kỳ như tiếng tim đập.

🔤 **ON-SCREEN:** `HEARTBEAT = thông điệp nhỏ, gửi định kỳ: "Tôi vẫn sống!"`

🎙️ **VO:**
"Trong hệ phân tán, sự cố là điều không thể tránh. Câu hỏi đặt ra: làm sao biết một dịch vụ cụ thể có còn sống và chạy đúng hay không? Đó là lúc heartbeat — nhịp tim — phát huy vai trò.

Heartbeat là một thông điệp được gửi định kỳ từ thành phần này sang thành phần khác, để theo dõi sức khỏe và trạng thái của nhau. Mục đích của nó rất đơn giản, như đang nói: 'Này, tôi vẫn còn đây và vẫn đang hoạt động!'. Tín hiệu này thường chỉ là một gói dữ liệu nhỏ, gửi đều đặn — từ vài giây đến vài phút, tùy yêu cầu của hệ thống."

## 1.2 — Vì sao cần Heartbeat? ⏱ 1:20–2:00

🎬 **HÌNH:** Bên trái — hệ thống KHÔNG có heartbeat: một node chết âm thầm, lỗi lan ra (đỏ loang), đồng hồ downtime tăng vọt. Bên phải — 4 lợi ích hiện ra dạng thẻ icon: **Giám sát · Phát hiện lỗi · Kích hoạt khôi phục · Cân bằng tải**.

🔤 **ON-SCREEN:** `Không heartbeat → lỗi phát hiện chậm, downtime tăng, độ tin cậy giảm`

🎙️ **VO:**
"Nếu không có heartbeat, việc phát hiện sự cố trở nên rất khó: lỗi được phát hiện và khắc phục chậm, thời gian ngừng hoạt động tăng, và độ tin cậy tổng thể giảm sút.

Ngược lại, heartbeat giúp bốn việc. Một, giám sát — theo dõi tình trạng các bộ phận. Hai, phát hiện sự cố — nếu một node bỏ lỡ vài nhịp dự kiến, đó là dấu hiệu có vấn đề. Ba, kích hoạt khôi phục — chuyển tác vụ sang node khỏe, khởi động lại thành phần lỗi, hoặc báo cho quản trị viên. Và bốn, cân bằng tải — bộ cân bằng dựa vào heartbeat để phân phối việc theo sức khỏe từng node."

## 1.3 — Heartbeat hoạt động thế nào? ⏱ 2:00–2:40

🎬 **HÌNH:** Sơ đồ rõ ràng: **Node (Sender)** → gửi tín hiệu mỗi 30 giây → **Monitor (Receiver)**. Monitor cập nhật bảng trạng thái: node "alive" (xanh). Một node bỏ lỡ 3 nhịp → đường ECG thành đường thẳng → Monitor đánh dấu "DEAD" (đỏ) → mũi tên cam: chuyển việc / khởi động lại.

🔤 **ON-SCREEN:** `Sender gửi đều · Monitor theo dõi · Bỏ lỡ nhiều nhịp → đánh dấu "chết" → khôi phục`

🎙️ **VO:**
"Cơ chế gồm hai vai. Bên gửi — node — phát tín hiệu heartbeat định kỳ. Bên nhận — monitor — tiếp nhận và giám sát các tín hiệu đó.

Quy trình đơn giản: node gửi nhịp đến monitor, ví dụ mỗi 30 giây; monitor nhận được thì cập nhật trạng thái node là 'còn sống'. Nếu node bỏ lỡ một số nhịp liên tiếp theo ngưỡng đã định, monitor coi node đó đã chết và kích hoạt hành động khôi phục."

## 1.4 — Các loại & những điểm tinh tế ⏱ 2:40–3:25

🎬 **HÌNH:** Hai nhánh: **PUSH** — node chủ động đẩy nhịp lên monitor; **PULL** — monitor chủ động hỏi node. Sau đó hiện 4 "núm chỉnh": *Frequency · Timeout · Payload*, mỗi núm có thanh trượt minh họa đánh đổi.

🔤 **ON-SCREEN:** `Push (node đẩy) vs Pull (monitor hỏi)` · `Chỉnh: Tần suất · Timeout · Payload`

🎙️ **VO:**
"Có hai loại heartbeat chính. Push — các node chủ động gửi nhịp lên monitor. Pull — monitor định kỳ truy vấn các node để hỏi trạng thái.

Tuy đơn giản về ý tưởng, việc triển khai có vài điểm tinh tế. Tần suất: gửi quá dày thì tốn tài nguyên mạng, quá thưa thì phát hiện lỗi chậm. Timeout: chờ bao lâu mới coi node là chết — quá ngắn dễ nhầm node còn sống thành chết, quá dài thì khôi phục chậm. Và payload: nhịp thường chỉ chứa dấu thời gian hoặc số thứ tự, nhưng cũng có thể mang thêm mức tải, chỉ số sức khỏe, hay phiên bản."

## 1.5 — Thách thức & ví dụ thực tế ⏱ 3:25–4:00

🎬 **HÌNH:** Lướt nhanh 4 thách thức (icon cảnh báo): *Tắc nghẽn mạng · Báo động giả · Tốn tài nguyên · Split-brain* (mạng đứt đôi, hai phía tuyên bố bên kia chết). Cắt sang 3 logo: **Database Replication · Kubernetes · Elasticsearch**.

🔤 **ON-SCREEN:** `Cẩn thận: nghẽn mạng · báo động giả · split-brain` — `Dùng trong: K8s · DB Replication · Elasticsearch`

🎙️ **VO:**
"Heartbeat cũng có thách thức: luồng tín hiệu liên tục có thể gây nghẽn mạng; cấu hình kém dẫn đến báo động giả — node chỉ chậm nhưng bị coi là hỏng; giám sát liên tục tiêu tốn tài nguyên; và hiếm gặp hơn là split-brain — khi mạng bị chia tách, hai phía đều tuyên bố bên kia đã chết.

Trong thực tế, heartbeat có mặt khắp nơi. Sao chép cơ sở dữ liệu: primary và replica trao đổi nhịp để đồng bộ và kích hoạt failover. Kubernetes: mỗi node gửi nhịp lên control plane để báo còn sống. Elasticsearch: các node trao đổi nhịp để phát hiện nhau và phát hiện lỗi. Heartbeat chính là những nhịp đập vô hình giữ cho hệ phân tán luôn sống và phối hợp nhịp nhàng."

---

# CHƯƠNG 2 — SERVICE DISCOVERY ⏱ 4:00–7:20

## 2.1 — Vấn đề: thế giới IP đổi liên tục ⏱ 4:00–4:45

🎬 **HÌNH:** Trục thời gian. Trái — "Ngày xưa": một server vật lý, IP cố định ghim cứng, mọi thứ gọi nhau êm. Phải — "Nay": hàng trăm container bật/tắt mỗi phút, IP nhảy liên tục. Một service cầm tờ giấy ghi IP cũ → gọi tới một ô đã trống → ❌ lỗi dây chuyền (đỏ loang).

🔤 **ON-SCREEN:** `Monolith: IP cố định` → `Cloud-native: IP đổi từng phút → ghi cứng = sập`

🎙️ **VO:**
"Ngày xưa, ứng dụng chạy trên một máy chủ duy nhất, các thành phần gọi nhau qua địa chỉ IP và cổng cố định, ghi cứng trong cấu hình — đơn giản.

Ứng dụng cloud-native ngày nay phức tạp hơn nhiều: hàng chục, hàng trăm microservice độc lập, triển khai trên các container động, liên tục co giãn lên xuống theo từng phút. Endpoint mạng được cấp phát, hủy bỏ, dời chỗ liên tục. Trong môi trường đó, duy trì file cấu hình tĩnh là bất khả thi — chỉ một node đổi chỗ là cả chuỗi dịch vụ đổ theo."

## 2.2 — Lời giải & Service Registry ⏱ 4:45–5:35

🎬 **HÌNH:** Cuốn "danh bạ" trung tâm — **Service Registry** — sáng lên ở giữa. Service mới khởi động → tự ghi một dòng vào danh bạ. Bung ra cấu trúc một bản ghi: *tên · IP · cổng · trạng thái · metadata · health · load · bảo mật*. Một service khác hỏi *"payment-service ở đâu?"* → danh bạ trả về địa chỉ hiện tại.

🔤 **ON-SCREEN:** `SERVICE REGISTRY = nguồn sự thật duy nhất, thời gian thực`

🎙️ **VO:**
"Đó là lúc Service Discovery vào cuộc — một cơ chế thời gian thực cho phép các dịch vụ tự định vị và giao tiếp với nhau một cách động, không cần con người can thiệp.

Trung tâm của nó là service registry — một cơ sở dữ liệu sẵn sàng cao, ghi nhận theo thời gian thực mọi endpoint đang hoạt động. Đây là nguồn sự thật duy nhất của hệ thống. Mỗi bản ghi lưu thông tin cốt lõi: tên dịch vụ, IP, cổng, trạng thái — kèm metadata phiên bản, thông tin sức khỏe, trọng số cân bằng tải và cấu hình bảo mật. Vì nó chi phối toàn bộ định tuyến, registry phải chịu được hàng ngàn thao tác đọc-ghi đồng thời khi các instance liên tục biến động."

## 2.3 — Cách đăng ký & vì sao quan trọng ⏱ 5:35–6:20

🎬 **HÌNH:** Bốn kiểu đăng ký xếp thành thang tiến hóa: *Thủ công → Tự đăng ký → Bên thứ ba (Sidecar) → Tự động bởi orchestrator*. Cái thủ công có dấu ❌ "không hợp hệ thống động". Bên cạnh: logo Netflix với "hàng trăm microservice, hàng triệu request".

🔤 **ON-SCREEN:** `Đăng ký: thủ công → tự đăng ký → sidecar → tự động` · `Lợi ích: ít cấu hình · mở rộng · chịu lỗi · dễ quản lý`

🎙️ **VO:**
"Để có mặt trong danh bạ, dịch vụ phải đăng ký. Có nhiều cách: đăng ký thủ công — admin tự nhập, đơn giản nhưng không hợp môi trường động; service tự đăng ký; đăng ký qua bên thứ ba theo mô hình sidecar; và tự động bởi các bộ điều phối như Kubernetes.

Hãy nghĩ tới quy mô như Netflix — hàng trăm microservice, hàng triệu request đồng thời. Ghi cứng địa chỉ ở đây là bất khả thi. Service Discovery mang lại bốn lợi ích: giảm cấu hình thủ công, cải thiện khả năng mở rộng, tăng khả năng chịu lỗi nhờ health check tự động định tuyến tránh node lỗi, và đơn giản hóa việc quản lý nhờ một danh mục tập trung."

## 2.4 — Client-side vs Server-side ⏱ 6:20–7:00

🎬 **HÌNH:** Màn chia đôi.
**Trái — Client-side:** (1) service đích tự đăng ký → (2) client hỏi registry lấy danh sách instance → (3) client tự cân bằng tải, gọi thẳng instance. Nhãn *Netflix Eureka*.
**Phải — Server-side:** client chỉ gọi một địa chỉ proxy cố định → Load Balancer/Gateway hỏi registry và định tuyến hộ → backend. Nhãn *AWS ELB*.

🔤 **ON-SCREEN:** `Client-side: client tự tìm (Eureka)` | `Server-side: gateway tìm hộ (AWS ELB)`

🎙️ **VO:**
"Có hai mô hình tìm kiếm. Client-side: trách nhiệm thuộc về client — nó tự hỏi registry để lấy danh sách instance rồi tự cân bằng tải và gọi thẳng. Ưu điểm: đơn giản, giảm tải cho thiết bị trung tâm; nhược: client phải tự cài logic và sửa code khi giao thức registry đổi. Công cụ tiêu biểu: Netflix Eureka.

Server-side: client chỉ gọi tới một cổng cố định — API Gateway hoặc Load Balancer; cổng đó hỏi registry và định tuyến hộ. Client hoàn toàn không cần biết về registry. Ưu điểm: gọn cho client, dễ quản lý tập trung; nhược: thêm một bước nhảy mạng và cổng đó có thể trở thành điểm lỗi duy nhất. Công cụ tiêu biểu: AWS Elastic Load Balancer."

## 2.5 — Thực hành tốt & công cụ ⏱ 7:00–7:20

🎬 **HÌNH:** Checklist trượt nhanh: *Chọn đúng mô hình · Registry sẵn sàng cao (nhân bản) · Tự động đăng ký · Health check · Quy ước đặt tên + version · Caching · Mở rộng*. Cuối cùng: logo **Eureka · AWS ELB · Consul · ZooKeeper**.

🔤 **ON-SCREEN:** `Best practices` · `Eureka · ELB · Consul · ZooKeeper`

🎙️ **VO:**
"Vài thực hành tốt: chọn mô hình hợp kiến trúc; làm cho chính registry sẵn sàng cao bằng nhân bản nhiều node và đồng thuận; tự động hóa đăng ký và hủy đăng ký; dùng health check để loại node lỗi; đặt tên chuẩn kèm phiên bản; caching để vẫn chạy khi registry tạm gián đoạn; và đảm bảo mở rộng theo lượng truy vấn. Công cụ phổ biến: Eureka, AWS ELB, HashiCorp Consul và Apache ZooKeeper."

---

# CHƯƠNG 3 — CONSENSUS (ĐỒNG THUẬN) ⏱ 7:20–10:50

## 3.1 — Đồng thuận là gì & vì sao cần ⏱ 7:20–8:05

🎬 **HÌNH:** 5 node tròn nối nhau bằng các đường truyền tin. Node A phát loa: *"Tôi chọn giá trị V — các bạn đồng ý chứ?"*. Các node truyền thông điệp qua lại. Hai kịch bản: (1) tất cả gật ✅; (2) một node lắc đầu *"Tôi thích W hơn"* → dấu hỏi lớn.

🔤 **ON-SCREEN:** `CONSENSUS = nhiều node cùng chốt MỘT giá trị chung`

🎙️ **VO:**
"Trong hệ phân tán, nhiều máy — gọi là node — kết nối và cộng tác qua việc truyền tin. Để phối hợp, chúng phải thống nhất về một giá trị chung. Hiện tượng đó gọi là đồng thuận phân tán.

Hình dung n node nối nhau. Một node nói: 'Tôi chọn giá trị V, các bạn đồng ý không?'. Nếu tất cả gật đầu, mọi node làm việc với V. Nhưng nếu một node nói 'tôi thích W hơn', việc đạt đồng thuận trở nên phức tạp — và đây mới là phần thú vị."

## 3.2 — Ba điều kiện của một thuật toán đồng thuận ⏱ 8:05–8:45

🎬 **HÌNH:** Ba huy hiệu lần lượt sáng lên: **AGREEMENT** (mọi node tốt chọn cùng giá trị) · **VALIDITY** (giá trị do node tốt đề xuất) · **TERMINATION** (mọi node tốt đều đi đến quyết định, không treo mãi).

🔤 **ON-SCREEN:** `Agreement (Nhất trí) · Validity (Hợp lệ) · Termination (Kết thúc)`

🎙️ **VO:**
"Để đạt đồng thuận, mọi node phải tuân theo cùng một giao thức và thỏa ba điều kiện. Agreement — nhất trí: tất cả node không lỗi cùng chọn một giá trị. Validity — hợp lệ: giá trị được chọn phải do một node không lỗi đề xuất. Termination — kết thúc: mọi node không lỗi đều phải đi đến quyết định, không được treo mãi mãi. Node không lỗi nghĩa là node không sập, không bị tấn công, hoạt động bình thường."

## 3.3 — Hai loại lỗi: Crash vs Byzantine ⏱ 8:45–9:30

🎬 **HÌNH:** Trái — **Crash failure:** một node tắt ngúm, các node khác chỉ việc bỏ qua nó (dễ). Phải — **Byzantine failure:** một node đeo mặt nạ phản bội, gửi thông điệp KHÁC NHAU cho từng node (mâu thuẫn, gây rối — khó).

🔤 **ON-SCREEN:** `Crash: node chết (dễ xử lý)` · `Byzantine: node nói dối, gửi tin mâu thuẫn (khó)`

🎙️ **VO:**
"Hệ phân tán chủ yếu gặp hai loại lỗi. Crash failure: một node ngừng phản hồi do lỗi phần cứng, phần mềm hoặc mạng — xử lý khá đơn giản, chỉ cần bỏ qua node đó.

Byzantine failure: node không sập, nhưng hành xử bất thường — gửi các thông điệp khác nhau cho các node khác nhau, do bị tấn công từ trong hoặc ngoài. Loại này rất khó xử lý. Và một nguyên tắc quan trọng: thuật toán nào chịu được lỗi Byzantine thì xử lý được mọi loại lỗi đồng thuận."

## 3.4 — pBFT & bài toán các tướng Byzantine ⏱ 9:30–10:20

🎬 **HÌNH:** Minh họa cổ điển: nhiều đạo quân bao vây một thành, mỗi đạo do một tướng chỉ huy, liên lạc qua người đưa tin; vài tướng là kẻ phản bội. Thanh "hơn 2/3 trung thực" lấp đầy → các tướng trung thành đồng loạt tấn công 🗡️. Bên cạnh: sơ đồ 3 pha **pre-prepare → prepare → commit**, một node Primary + các Secondary.

🔤 **ON-SCREEN:** `Lamport: >2/3 trung thực → đạt đồng thuận` · `pBFT: pre-prepare → prepare → commit`

🎙️ **VO:**
"Hiểu pBFT qua bài toán các tướng Byzantine. Nhiều đạo quân bao vây một thành, liên lạc chỉ qua người đưa tin, và một số tướng là kẻ phản bội cố phá. Họ cần một thuật toán đảm bảo: tất cả tướng trung thành chọn cùng một kế hoạch, và một nhóm nhỏ phản bội không thể khiến họ chọn kế hoạch tệ.

Leslie Lamport — cha đẻ của hệ thống phân tán — chứng minh: nếu hơn hai phần ba số node là trung thực, vẫn đạt được đồng thuận. pBFT chia thành ba pha — pre-prepare, prepare, commit — với một node Primary và các node Secondary. Client gửi yêu cầu tới Primary; Primary phát cho các Secondary; tất cả thực thi và phản hồi; yêu cầu thành công khi client nhận được phản hồi giống nhau từ ít nhất hai phần ba số node. Nếu Primary im lặng quá lâu, nó bị thay bằng giao thức view change."

## 3.5 — Các thuật toán thực tế & ứng dụng ⏱ 10:20–10:50

🎬 **HÌNH:** Hai nhóm. **Voting-based:** Raft (logo etcd/Kubernetes/CockroachDB), Paxos (Google Spanner/ZooKeeper), HotStuff. **Proof-based:** PoW (Bitcoin — miner giải toán), PoS (Ethereum — validator đặt cược). Cuối: ứng dụng — Blockchain · Google PageRank · Load balancing.

🔤 **ON-SCREEN:** `Voting: Raft · Paxos · HotStuff` | `Proof: PoW (Bitcoin) · PoS (Ethereum)`

🎙️ **VO:**
"Có hai họ thuật toán. Họ bỏ phiếu: Raft — đơn giản hơn Paxos, dùng trong etcd của Kubernetes và CockroachDB; Paxos — nền tảng lý thuyết, dùng trong Google Spanner và ZooKeeper; HotStuff — tối ưu cho blockchain.

Họ dựa trên bằng chứng, dùng cho mạng mở quy mô lớn: Proof of Work — như Bitcoin, miner phải giải bài toán tính toán để được ghi block; Proof of Stake — như Ethereum, validator đặt cược tài sản để có quyền xác nhận. Đồng thuận xuất hiện trong blockchain, trong xếp hạng PageRank của Google, và cả trong cân bằng tải."

---

# CHƯƠNG 4 — DISTRIBUTED LOCKING ⏱ 10:50–14:10

## 4.1 — Khóa là gì & vì sao cần khóa phân tán ⏱ 10:50–11:35

🎬 **HÌNH:** Hai client trên hai máy cùng lao vào sửa một ô dữ liệu — "vé cuối cùng" / "số dư" — kết quả nhấp nháy đỏ, sai lệch. Cắt sang: một ổ khóa gắn lên tài nguyên; chỉ client cầm chìa mới vào được, client kia phải xếp hàng chờ.

🔤 **ON-SCREEN:** `Tài nguyên dùng chung + nhiều máy → cần MUTUAL EXCLUSION`

🎙️ **VO:**
"Khóa đảm bảo loại trừ tương hỗ — mỗi lúc chỉ một bên được thao tác trên một tài nguyên. Trên một máy việc này dễ. Nhưng khi nhiều tiến trình nằm trên nhiều máy cùng giành một tài nguyên dùng chung — chiếc vé cuối cùng, số dư tài khoản — ta cần một khóa hoạt động xuyên suốt nhiều máy: khóa phân tán. Nếu không, hai bên cùng ghi một lúc sẽ làm hỏng dữ liệu."

## 4.2 — Vòng đời khóa & đánh đổi an toàn/sống ⏱ 11:35–12:15

🎬 **HÌNH:** Vòng đời khóa: *Acquire → Hold (làm việc) → Release*. Một đồng hồ "lease" (thời gian thuê) đếm ngược gắn trên khóa. Thanh trượt giữa hai cực: **Safety** (an toàn) ↔ **Liveness** (tính sống). Kéo lease ngắn → liveness tốt nhưng rủi ro hết hạn giữa chừng; kéo dài → an toàn nhưng node sập khóa chết người khác.

🔤 **ON-SCREEN:** `Lease = khóa tự hết hạn` · `Lease ngắn = sống ↑ an toàn ↓ · Lease dài = an toàn ↑ sống ↓`

🎙️ **VO:**
"Một khóa đi qua ba bước: giành khóa, giữ khóa để làm việc, rồi nhả khóa. Vấn đề: nếu bên giữ sập mà không kịp nhả, khóa sẽ kẹt mãi. Giải pháp là lease — thời gian thuê: khóa tự hết hạn sau một khoảng.

Nhưng lease tạo ra đánh đổi giữa an toàn và tính sống. Lease quá ngắn cải thiện tính sống nhưng có nguy cơ hết hạn khi bên giữ vẫn đang làm việc — phá vỡ an toàn. Lease quá dài bảo vệ an toàn nhưng để một node sập chặn những node khác lâu — hại tính sống. Phần lớn nghệ thuật của khóa phân tán nằm ở việc chọn điểm cân bằng hợp lý."

## 4.3 — Redis & thuật toán Redlock ⏱ 12:15–12:55

🎬 **HÌNH:** Đầu tiên một node Redis đơn — gắn nhãn cảnh báo "single point of failure". Rồi bung ra 5 node Redis độc lập. Client xin khóa trên cả 5 trong một "ngân sách thời gian" ngắn; 3 node sáng xanh ✅ → biểu ngữ "GIỮ KHÓA (3/5 majority)". Một node sập nhưng dịch vụ khóa vẫn sống.

🔤 **ON-SCREEN:** `Redlock: giành đa số (3/5 node) = giữ khóa → chịu được mất thiểu số node`

🎙️ **VO:**
"Khóa dựa trên Redis rất nhanh, nhưng một node Redis đơn lại là điểm hỏng duy nhất. Vì vậy tác giả Redis đề xuất Redlock — chạy trên nhiều node Redis độc lập, thường là năm.

Client cố giành cùng một khóa trên tất cả các node trong một ngân sách thời gian ngắn, và chỉ coi là giữ khóa nếu thành công trên đa số — ba trên năm. Vì đa số vẫn tồn tại khi mất một thiểu số node, không một sự cố sập đơn lẻ nào làm sập được dịch vụ khóa."

## 4.4 — Split-brain & Fencing token ⏱ 12:55–13:45

🎬 **HÌNH:** Phần A — mạng đứt làm đôi (split-brain): hai phía đều tưởng mình giữ khóa 😱, cùng ghi vào tài nguyên. Phần B — giải pháp fencing: mỗi lần cấp khóa kèm một số tăng dần (#33, #34). Tài nguyên ghi nhớ token lớn nhất từng thấy; client cũ hồi sinh mang #33 → bị từ chối ❌; client mới #34 → ✅. Minh họa "quorum" chỉ một phía chứa đa số mới thắng.

🔤 **ON-SCREEN:** `Split-brain: 2 bên cùng tưởng giữ khóa` · `Quorum chặn split-brain` · `Fencing token: chỉ số LỚN NHẤT mới ghi được`

🎙️ **VO:**
"Cạm bẫy nguy hiểm nhất là split-brain: khi mạng bị phân vùng, mỗi phía có thể tự kết luận mình giữ khóa, và hai bên cùng thao tác trên một tài nguyên. Đây là lý do khóa đúng đắn đòi hỏi quorum — đa số: chỉ một phía của bất kỳ phân vùng nào chứa được đa số, nên chỉ một phía thắng.

Lá chắn thứ hai là fencing token. Mỗi lần cấp khóa, dịch vụ phát ra một con số tăng nghiêm ngặt. Client phải kèm token này vào mọi yêu cầu. Tài nguyên ghi nhớ token lớn nhất từng thấy và từ chối mọi token nhỏ hơn. Nhờ vậy, một bên giữ khóa cũ vừa hồi sinh sẽ mang token cũ, nhỏ hơn, và bị chặn an toàn. Fencing làm cho tính an toàn không còn phụ thuộc vào thời gian: dù hai client cùng tưởng mình giữ khóa, chỉ bên có token lớn hơn mới ghi được dữ liệu."

## 4.5 — Cuộc tranh luận Redlock & thực hành tốt ⏱ 13:45–14:10

🎬 **HÌNH:** Hai phe tranh luận: "Redlock không đủ an toàn cho tính đúng đắn" vs "Redlock ổn cho hiệu quả". Trọng tài chốt: *"Cần đúng đắn → thêm fencing"*. Cuối: checklist thực hành — *lease đủ dài + gia hạn · định danh chủ bằng token duy nhất · vùng tới hạn ngắn · quorum · fencing khi cần*.

🔤 **ON-SCREEN:** `Hiệu quả → Redlock đủ` · `Đúng đắn → BẮT BUỘC thêm fencing`

🎙️ **VO:**
"Redlock từng là tâm điểm một cuộc tranh luận nổi tiếng. Phía phê phán: vì Redlock dựa trên giả định thời gian, một lần tạm dừng dài hay nhảy đồng hồ có thể khiến hai client cùng giữ khóa. Phía bênh vực: Redlock vẫn ổn cho mục tiêu hiệu quả — tránh làm trùng việc — miễn đừng tin nó cho tính đúng đắn. Kết luận được đồng thuận: nếu tính đúng đắn thật sự quan trọng, hãy thêm fencing token.

Tóm lại vài thói quen tốt: luôn đặt lease nhưng để dài hơn công việc dự kiến và gia hạn nếu cần; định danh chủ sở hữu bằng token duy nhất để không ai nhả nhầm khóa của người khác; giữ vùng tới hạn thật ngắn; dùng quorum; và khi dữ liệu tuyệt đối không được hỏng, bảo vệ chính tài nguyên bằng fencing token."

---

# PHẦN KẾT ⏱ 14:10–14:40

🎬 **HÌNH:** Bốn biểu tượng hội tụ thành một hệ thống chạy mượt, mạng sáng xanh toàn bộ: 🫀 nhịp tim đập đều · 🧭 danh bạ sáng · 🗳️ các node cùng bỏ phiếu · 🔒 khóa an toàn. Logo seminar hiện lên.

🔤 **ON-SCREEN:**
`Heartbeat → phát hiện lỗi`
`Service Discovery → tìm thấy nhau`
`Consensus → cùng đồng thuận`
`Distributed Locking → bảo vệ dữ liệu`

🎙️ **VO:**
"Vậy là bốn cơ chế, cùng một mục tiêu: giữ cho hệ phân tán đáng tin cậy. Heartbeat phát hiện ai còn sống. Service Discovery giúp các dịch vụ tìm thấy nhau khi địa chỉ luôn thay đổi. Consensus để nhiều node cùng đi đến một quyết định, ngay cả khi có lỗi. Và Distributed Locking bảo vệ dữ liệu dùng chung khỏi xung đột.

Hiểu được bốn nền tảng này, bạn đã nắm được bộ khung để xây những hệ thống vừa mở rộng được, vừa bền bỉ trước sự cố. Cảm ơn các bạn đã theo dõi!"

---

## 📝 GHI CHÚ SẢN XUẤT

| Hạng mục | Gợi ý |
|---|---|
| **Tổng thời lượng** | ~14 phút 40 giây. Muốn ngắn hơn: gộp 1.4 vào 1.3, lược 2.5 và 3.5. |
| **Nhạc nền** | Nền nhẹ, nâng tempo ở đầu mỗi chương, hạ xuống ở đoạn narration kỹ thuật. |
| **SFX** | "bíp" nhịp tim (C1) · "ting" tra danh bạ (C2) · gõ búa chốt vote (C3) · "cạch" khóa (C4). |
| **Bảng màu node** | Xanh lá = khỏe · Đỏ = chết/lỗi · Cam = hành động khôi phục · Xám = chưa rõ. |
| **Chữ on-screen** | Sans-serif đậm, tối đa 1–2 dòng/màn để đọc tốt trên mobile. |
| **Nhịp đọc** | ~150 từ/phút; chèn 0.5–1s lặng giữa các tiểu mục để khớp chuyển cảnh. |
| **Bản rút gọn** | Xem `kich-ban-video-seminar-distributed-systems.md` cho phiên bản storyboard ~6 phút. |
