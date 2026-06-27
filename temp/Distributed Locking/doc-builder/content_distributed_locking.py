#!/usr/bin/env python3
"""Bilingual (English / Vietnamese) content for the Distributed Locking document.

All prose is original explanatory writing about distributed locking; the only
thing borrowed from the teacher's file is the presentation format.
"""


def build(b):
    chapter_1(b)
    chapter_2(b)
    chapter_3(b)


# ===========================================================================
# CHAPTER 1
# ===========================================================================
def chapter_1(b):
    b.h1("TỔNG QUAN VỀ KHÓA PHÂN TÁN", "AN OVERVIEW OF DISTRIBUTED LOCKING")

    b.p(
        "Modern software rarely runs on a single machine. A web application is "
        "served by dozens of identical processes, a background job is executed by "
        "many workers, and data is replicated across several data centers. This "
        "scale brings a hard question that does not exist on a single computer: "
        "when many independent processes want to touch the same piece of data at "
        "the same time, how do we stop them from corrupting it? Distributed "
        "locking is one of the classic answers to that question.",
        "Phần mềm hiện đại hiếm khi chạy trên một máy duy nhất. Một ứng dụng web "
        "thường được phục vụ bởi hàng chục tiến trình giống hệt nhau, một tác vụ nền "
        "được thực thi bởi nhiều worker, và dữ liệu được sao chép trên nhiều trung "
        "tâm dữ liệu. Quy mô đó làm nảy sinh một câu hỏi hóc búa vốn không tồn tại "
        "trên một máy đơn lẻ: khi nhiều tiến trình độc lập cùng muốn tác động lên "
        "một mẩu dữ liệu tại cùng một thời điểm, làm thế nào để ngăn chúng làm hỏng "
        "dữ liệu đó? Khóa phân tán là một trong những lời giải kinh điển cho câu hỏi này.",
    )
    b.p(
        "On one computer the operating system already provides locks (mutexes, "
        "semaphores) that coordinate threads sharing the same memory. A distributed "
        "lock extends the very same idea across machines that share nothing but a "
        "network. Because the network can be slow, can drop messages, and can fail "
        "entirely, building a correct distributed lock is far harder than it first "
        "appears.",
        "Trên một máy tính, hệ điều hành đã cung cấp sẵn các loại khóa (mutex, "
        "semaphore) để điều phối các luồng cùng chia sẻ một vùng nhớ. Khóa phân tán "
        "mở rộng đúng ý tưởng đó ra cho các máy không chia sẻ gì với nhau ngoài một "
        "đường mạng. Vì mạng có thể chậm, có thể đánh rơi gói tin và thậm chí hỏng "
        "hoàn toàn, nên việc xây dựng một khóa phân tán đúng đắn khó hơn rất nhiều "
        "so với vẻ ngoài ban đầu của nó.",
    )

    # -- 1.1 -------------------------------------------------------------
    b.h2("Khóa là gì?", "What Is a Lock?")
    b.p(
        "A lock is a coordination primitive that grants exclusive access to a shared "
        "resource. At any moment at most one participant may hold the lock; everyone "
        "else must wait. The region of code that runs while the lock is held is "
        "called the critical section, and the property we are buying is that only "
        "one process executes that section at a time.",
        "Khóa (lock) là một cơ chế điều phối nhằm cấp quyền truy cập độc quyền vào "
        "một tài nguyên dùng chung. Tại bất kỳ thời điểm nào, nhiều nhất chỉ một bên "
        "được giữ khóa; tất cả những bên còn lại phải chờ. Đoạn mã chạy trong lúc "
        "khóa đang được giữ gọi là vùng tới hạn (critical section), và điều mà ta đạt "
        "được là tại mỗi thời điểm chỉ có một tiến trình thực thi vùng đó.",
    )
    b.imp(
        "A distributed lock is a mechanism that guarantees that, among many "
        "processes running on different machines, at most one holds the lock for a "
        "given resource at any instant.",
        "Khóa phân tán là cơ chế bảo đảm rằng trong số nhiều tiến trình chạy trên "
        "các máy khác nhau, tại mỗi thời điểm nhiều nhất chỉ một tiến trình giữ khóa "
        "cho một tài nguyên nhất định.",
    )
    b.p(
        "A distributed lock has the same contract but the participants live on "
        "different machines. Instead of asking the local operating system, each "
        "process asks a shared external authority - a database, a key-value store, "
        "or a dedicated coordination service - to decide who currently owns the "
        "lock. That external authority becomes the single source of truth.",
        "Khóa phân tán có cùng một bản giao kèo như vậy, nhưng các bên tham gia lại "
        "nằm trên những máy khác nhau. Thay vì hỏi hệ điều hành cục bộ, mỗi tiến "
        "trình hỏi một thực thể bên ngoài dùng chung - một cơ sở dữ liệu, một kho "
        "khóa-giá trị, hoặc một dịch vụ điều phối chuyên dụng - để quyết định ai "
        "đang sở hữu khóa. Thực thể bên ngoài đó trở thành nguồn chân lý duy nhất.",
    )
    b.q_header("Khóa là gì?", "What Is a Lock?")
    b.qa("Khóa (lock) là gì và nó bảo đảm điều gì?",
         "Khóa là cơ chế điều phối cấp quyền truy cập độc quyền vào tài nguyên dùng "
         "chung; tại mỗi thời điểm nhiều nhất một bên được giữ khóa, các bên khác phải chờ.")
    b.qa("Vùng tới hạn (critical section) là gì?",
         "Là đoạn mã được thực thi trong khi khóa đang được giữ; chỉ một tiến trình "
         "được chạy vùng này tại một thời điểm.")
    b.qa("Điểm khác biệt căn bản giữa khóa cục bộ và khóa phân tán là gì?",
         "Khóa cục bộ do hệ điều hành quản lý cho các luồng chung bộ nhớ; khóa phân "
         "tán dựa vào một thực thể bên ngoài dùng chung để điều phối các tiến trình "
         "trên những máy khác nhau qua mạng.")

    # -- 1.2 -------------------------------------------------------------
    b.h2("Tại sao cần khóa phân tán?", "Why Do We Need Distributed Locks?")
    b.p(
        "Imagine an online bank account with a balance of 100. Two requests arrive "
        "at the same time, each trying to withdraw 100 and each handled by a "
        "different server. Both read the balance (100), both decide the withdrawal "
        "is allowed, and both write back 0. The customer has withdrawn 200 from an "
        "account that held only 100. This is a race condition, and it happens "
        "precisely because two processes entered the critical section together.",
        "Hãy hình dung một tài khoản ngân hàng trực tuyến có số dư 100. Hai yêu cầu "
        "đến cùng lúc, mỗi yêu cầu đều cố rút 100 và mỗi yêu cầu được xử lý bởi một "
        "máy chủ khác nhau. Cả hai cùng đọc số dư (100), cả hai cùng kết luận rằng "
        "được phép rút, và cả hai cùng ghi lại 0. Khách hàng đã rút được 200 từ một "
        "tài khoản chỉ có 100. Đây là một tình huống tranh chấp dữ liệu (race "
        "condition), và nó xảy ra chính vì hai tiến trình cùng bước vào vùng tới hạn.",
    )
    b.fig(1, "fig1_1_mutex",
          "Không có khóa, hai client cùng ghi gây mất cập nhật; có khóa, chỉ một client "
          "được vào vùng tới hạn nên dữ liệu nhất quán.",
          "Without a lock two clients write concurrently and lose an update; with a "
          "lock only one client enters the critical section, so data stays consistent.")
    b.p(
        "A distributed lock prevents this by forcing the two servers to take turns. "
        "Whoever acquires the lock first reads, updates, and writes the balance; the "
        "other waits and then sees the already-updated value, correctly rejecting "
        "the second withdrawal. The same pattern protects countless real tasks: "
        "sending an invoice only once, running a scheduled job on exactly one node, "
        "or assigning a unique sequence number.",
        "Khóa phân tán ngăn chặn điều này bằng cách buộc hai máy chủ phải thay phiên "
        "nhau. Bên nào giành được khóa trước sẽ đọc, cập nhật rồi ghi lại số dư; bên "
        "kia phải chờ và sau đó nhìn thấy giá trị đã được cập nhật, nhờ vậy từ chối "
        "đúng lệnh rút tiền thứ hai. Cùng một khuôn mẫu đó bảo vệ vô số tác vụ thực "
        "tế: chỉ gửi hóa đơn đúng một lần, chạy một tác vụ định kỳ trên đúng một "
        "node, hay cấp một số thứ tự duy nhất.",
    )
    b.q_header("Tại sao cần khóa phân tán?", "Why Do We Need Distributed Locks?")
    b.qa("Race condition (tranh chấp dữ liệu) là gì?",
         "Là lỗi xảy ra khi nhiều tiến trình cùng truy cập và sửa đổi một dữ liệu "
         "dùng chung đồng thời, dẫn đến kết quả sai như mất cập nhật.")
    b.qa("Khóa phân tán giải quyết tình huống rút tiền song song như thế nào?",
         "Buộc các máy chủ thay phiên nhau: bên giành được khóa xử lý trước, bên còn "
         "lại chờ rồi đọc giá trị đã cập nhật nên từ chối đúng lệnh rút thứ hai.")
    b.qa("Nêu vài tác vụ thực tế cần đến khóa phân tán.",
         "Chỉ gửi hóa đơn một lần, chạy tác vụ định kỳ trên đúng một node, cấp số "
         "thứ tự duy nhất, bầu chọn leader.")

    # -- 1.3 -------------------------------------------------------------
    b.h2("Loại trừ tương hỗ và vòng đời của khóa",
         "Mutual Exclusion and the Lock Lifecycle")
    b.p(
        "The guarantee a lock provides is called mutual exclusion: two processes are "
        "mutually excluded from the critical section. A useful distributed lock is "
        "not held forever, however. It follows a lifecycle - a process acquires the "
        "lock, performs its work, and releases it - and a robust lock adds a safety "
        "valve: a lease, or time-to-live (TTL), after which the lock expires on its "
        "own even if the holder forgot, or was unable, to release it.",
        "Sự bảo đảm mà khóa mang lại được gọi là loại trừ tương hỗ (mutual "
        "exclusion): hai tiến trình loại trừ lẫn nhau khỏi vùng tới hạn. Tuy nhiên, "
        "một khóa phân tán hữu dụng không được giữ mãi mãi. Nó tuân theo một vòng "
        "đời - tiến trình giành khóa, thực hiện công việc, rồi trả khóa - và một khóa "
        "vững chắc còn bổ sung một van an toàn: thời gian thuê (lease), hay thời gian "
        "sống (TTL), sau đó khóa tự hết hạn ngay cả khi bên giữ quên trả hoặc không "
        "thể trả nó.",
    )
    b.fig(1, "fig1_2_lifecycle",
          "Vòng đời của một khóa phân tán: giành khóa kèm TTL, làm việc trong vùng tới "
          "hạn, rồi trả khóa hoặc để khóa tự hết hạn.",
          "The lifecycle of a distributed lock: acquire with a TTL, work in the "
          "critical section, then release the lock or let it expire.")
    b.imp(
        "Without a lease, a single crashed process could hold a lock forever and "
        "freeze the whole system. The lease is what turns a fragile lock into a "
        "fault-tolerant one.",
        "Nếu không có thời gian thuê, chỉ cần một tiến trình bị sập là có thể giữ "
        "khóa mãi mãi và làm đóng băng toàn hệ thống. Chính thời gian thuê biến một "
        "khóa mong manh thành một khóa có khả năng chịu lỗi.",
        highlight="yellow",
    )
    b.q_header("Loại trừ tương hỗ và vòng đời của khóa",
               "Mutual Exclusion and the Lock Lifecycle")
    b.qa("Loại trừ tương hỗ (mutual exclusion) nghĩa là gì?",
         "Là bảo đảm rằng tại mỗi thời điểm chỉ một tiến trình được ở trong vùng tới "
         "hạn; các tiến trình loại trừ lẫn nhau khỏi vùng đó.")
    b.qa("Một khóa phân tán đi qua những bước nào trong vòng đời?",
         "Giành khóa (kèm TTL) → làm việc trong vùng tới hạn → trả khóa hoặc để khóa "
         "tự hết hạn.")
    b.qa("Thời gian thuê (lease/TTL) có vai trò gì?",
         "Là van an toàn: nếu bên giữ khóa bị sập hoặc quên trả, khóa vẫn tự hết hạn "
         "sau TTL nên hệ thống không bị đóng băng.")

    # -- 1.4 -------------------------------------------------------------
    b.h2("Các thuộc tính của một khóa phân tán tốt",
         "Properties of a Good Distributed Lock")
    b.p(
        "Three properties together describe a trustworthy distributed lock. Safety "
        "(mutual exclusion) means two clients never hold the same lock at once. "
        "Liveness (deadlock freedom) means the lock is eventually granted to "
        "someone, so the system makes progress and never freezes permanently. Fault "
        "tolerance means the lock service keeps working even when some of its nodes "
        "or some clients crash.",
        "Ba thuộc tính cùng nhau mô tả một khóa phân tán đáng tin cậy. Tính an toàn "
        "(safety - loại trừ tương hỗ) nghĩa là không bao giờ có hai client cùng giữ "
        "một khóa. Tính sống (liveness - không bế tắc) nghĩa là khóa rồi sẽ được cấp "
        "cho ai đó, nhờ vậy hệ thống tiến triển và không bị đóng băng vĩnh viễn. Khả "
        "năng chịu lỗi (fault tolerance) nghĩa là dịch vụ khóa vẫn hoạt động ngay cả "
        "khi một số node của nó hoặc một số client bị sập.",
    )
    b.imp(
        "A good distributed lock must satisfy three properties at once: safety "
        "(mutual exclusion), liveness (no deadlock), and fault tolerance.",
        "Một khóa phân tán tốt phải đồng thời thỏa mãn ba thuộc tính: an toàn (loại "
        "trừ tương hỗ), tính sống (không bế tắc) và khả năng chịu lỗi.",
        highlight="green",
    )
    b.p(
        "These goals pull against each other. A very short lease improves liveness "
        "but risks expiring while the holder is still working, breaking safety. A "
        "very long lease protects safety but lets a crashed holder block others for "
        "a long time, hurting liveness. Much of the craft of distributed locking is "
        "choosing a sensible balance and, as we will see in Chapter 3, adding "
        "fencing so that even a wrong choice cannot corrupt data.",
        "Những mục tiêu này lại kéo ngược nhau. Một thời gian thuê rất ngắn cải thiện "
        "tính sống nhưng có nguy cơ hết hạn khi bên giữ còn đang làm việc, phá vỡ "
        "tính an toàn. Một thời gian thuê rất dài bảo vệ tính an toàn nhưng lại để "
        "một bên giữ bị sập chặn những bên khác trong thời gian dài, làm hại tính "
        "sống. Phần lớn nghệ thuật của khóa phân tán nằm ở việc chọn một điểm cân "
        "bằng hợp lý và, như sẽ thấy ở Chương 3, bổ sung cơ chế fencing để ngay cả "
        "một lựa chọn sai cũng không thể làm hỏng dữ liệu.",
    )
    b.q_header("Các thuộc tính của một khóa phân tán tốt",
               "Properties of a Good Distributed Lock")
    b.qa("Ba thuộc tính của một khóa phân tán tốt là gì?",
         "An toàn (loại trừ tương hỗ), tính sống (không bế tắc), và khả năng chịu lỗi.")
    b.qa("Vì sao việc chọn độ dài thời gian thuê lại là một sự đánh đổi?",
         "Thuê quá ngắn dễ hết hạn giữa chừng, phá vỡ an toàn; thuê quá dài khiến "
         "bên sập chặn người khác lâu, làm hại tính sống.")

    b.key_points([
        "Khóa phân tán mở rộng ý tưởng khóa của hệ điều hành ra nhiều máy chỉ chia "
        "sẻ một đường mạng.",
        "Mục đích cốt lõi là loại trừ tương hỗ: ngăn nhiều tiến trình cùng vào vùng "
        "tới hạn và gây race condition.",
        "Khóa nên có thời gian thuê (TTL) để tự hết hạn khi bên giữ bị sập.",
        "Khóa tốt phải cân bằng đồng thời ba thuộc tính: an toàn, tính sống và khả "
        "năng chịu lỗi.",
    ])
    b.review_section([
        "Giải thích bằng ví dụ tài khoản ngân hàng vì sao thiếu khóa lại gây sai dữ liệu.",
        "Phân biệt khóa cục bộ và khóa phân tán.",
        "Vẽ và mô tả vòng đời của một khóa phân tán có TTL.",
        "Vì sao ba thuộc tính an toàn, tính sống và chịu lỗi thường mâu thuẫn nhau?",
    ])


# ===========================================================================
# CHAPTER 2
# ===========================================================================
def chapter_2(b):
    b.h1("CÁC PHƯƠNG PHÁP TRIỂN KHAI KHÓA PHÂN TÁN",
         "APPROACHES TO IMPLEMENTING DISTRIBUTED LOCKS")
    b.p(
        "There is no single way to build a distributed lock. Every approach relies "
        "on some shared store that all clients can reach and that can make an atomic "
        "decision about ownership. The store may be a relational database you "
        "already run, an in-memory cache such as Redis, or a purpose-built "
        "coordination service such as ZooKeeper or etcd. The right choice depends on "
        "how much correctness you need and how much infrastructure you are willing "
        "to operate.",
        "Không có một cách duy nhất để xây dựng khóa phân tán. Mọi phương pháp đều "
        "dựa vào một kho lưu trữ dùng chung mà mọi client đều truy cập được và có thể "
        "ra một quyết định nguyên tử (atomic) về quyền sở hữu. Kho đó có thể là một "
        "cơ sở dữ liệu quan hệ bạn đã vận hành sẵn, một bộ nhớ đệm trong RAM như "
        "Redis, hay một dịch vụ điều phối chuyên dụng như ZooKeeper hoặc etcd. Lựa "
        "chọn đúng phụ thuộc vào mức độ đúng đắn bạn cần và lượng hạ tầng bạn sẵn "
        "lòng vận hành.",
    )
    b.fig(2, "fig2_1_architecture",
          "Kiến trúc dịch vụ khóa: client yêu cầu khóa, bộ quản lý khóa cấp hoặc từ "
          "chối, bên được cấp mới truy cập tài nguyên.",
          "Lock-service architecture: clients request the lock, the lock manager "
          "grants or denies it, and only the grantee accesses the resource.")

    # -- 2.1 -------------------------------------------------------------
    b.h2("Khóa dựa trên cơ sở dữ liệu", "Database-Based Locks")
    b.p(
        "The simplest lock uses a database you already have. You create a table with "
        "a unique key per resource and try to INSERT a row to acquire the lock; the "
        "unique constraint guarantees that only one client succeeds. Releasing the "
        "lock is just deleting the row. A column storing an expiry timestamp gives "
        "you a crude lease, and a background cleanup removes stale rows.",
        "Loại khóa đơn giản nhất tận dụng cơ sở dữ liệu bạn đã có sẵn. Bạn tạo một "
        "bảng với khóa duy nhất cho mỗi tài nguyên và thử INSERT một dòng để giành "
        "khóa; ràng buộc duy nhất (unique constraint) bảo đảm chỉ một client thành "
        "công. Trả khóa chỉ là xóa dòng đó đi. Một cột lưu mốc thời gian hết hạn cho "
        "bạn một dạng thời gian thuê thô sơ, và một tiến trình dọn dẹp nền sẽ loại bỏ "
        "các dòng quá hạn.",
    )
    b.p(
        "This approach is attractive because it needs no extra infrastructure and is "
        "easy to reason about. Its weaknesses are performance and the database "
        "becoming a single point of failure: every lock operation is a database "
        "round trip, and heavy lock traffic can overwhelm a store that was meant for "
        "business data. It suits low-frequency locks, not hot paths.",
        "Cách này hấp dẫn vì không cần thêm hạ tầng và dễ suy luận. Điểm yếu của nó "
        "là hiệu năng và việc cơ sở dữ liệu trở thành điểm hỏng đơn (single point of "
        "failure): mỗi thao tác khóa là một vòng truy vấn tới cơ sở dữ liệu, và lưu "
        "lượng khóa lớn có thể làm quá tải một kho vốn được thiết kế cho dữ liệu "
        "nghiệp vụ. Nó phù hợp với các khóa tần suất thấp, không phù hợp với các "
        "đường đi nóng (hot path).",
    )
    b.q_header("Khóa dựa trên cơ sở dữ liệu", "Database-Based Locks")
    b.qa("Cách giành khóa bằng cơ sở dữ liệu hoạt động ra sao?",
         "Thử INSERT một dòng có khóa duy nhất cho tài nguyên; ràng buộc unique bảo "
         "đảm chỉ một client INSERT thành công và giữ được khóa.")
    b.qa("Ưu và nhược điểm chính của khóa dựa trên cơ sở dữ liệu là gì?",
         "Ưu: không cần thêm hạ tầng, dễ hiểu. Nhược: chậm do mỗi thao tác là một "
         "vòng truy vấn, và cơ sở dữ liệu dễ thành điểm hỏng đơn khi tải khóa lớn.")

    # -- 2.2 -------------------------------------------------------------
    b.h2("Khóa dựa trên Redis và thuật toán Redlock",
         "Redis-Based Locks and the Redlock Algorithm")
    b.p(
        "Redis is a fast in-memory store, which makes it a popular home for locks. "
        "On a single Redis instance you acquire a lock with one atomic command: SET "
        "key value NX PX 30000, which sets the key only if it does not yet exist (NX) "
        "and attaches a 30-second expiry (PX). The value is a random token unique to "
        "the client, so that only the true owner can release the lock - the release "
        "is done with a small script that deletes the key only if its value still "
        "matches the caller's token.",
        "Redis là một kho lưu trữ trong RAM tốc độ cao, nên nó là nơi đặt khóa rất "
        "phổ biến. Trên một node Redis đơn lẻ, bạn giành khóa bằng một lệnh nguyên tử "
        "duy nhất: SET key value NX PX 30000, lệnh này chỉ đặt khóa nếu nó chưa tồn "
        "tại (NX) và gắn kèm thời hạn 30 giây (PX). Giá trị là một token ngẫu nhiên "
        "riêng của client, để chỉ chủ thật sự mới trả được khóa - việc trả khóa được "
        "thực hiện bằng một đoạn script nhỏ, chỉ xóa khóa nếu giá trị của nó vẫn khớp "
        "với token của bên gọi.",
    )
    b.imp(
        "On a single node, SET key token NX PX <ttl> acquires a lock atomically; the "
        "random token ensures that only the owner can release it.",
        "Trên một node, lệnh SET key token NX PX <ttl> giành khóa một cách nguyên tử; "
        "token ngẫu nhiên bảo đảm chỉ chủ sở hữu mới có thể trả khóa.",
        highlight="yellow",
    )
    b.p(
        "A single Redis node is itself a single point of failure, so Redis's author "
        "proposed Redlock, an algorithm that runs over several independent Redis "
        "nodes - typically five. The client tries to acquire the same lock on all of "
        "them within a short time budget and considers the lock held only if it "
        "succeeds on a majority (three of five). Because a majority can survive the "
        "loss of a minority of nodes, no single crash takes the lock service down.",
        "Bản thân một node Redis lại là một điểm hỏng đơn, nên tác giả của Redis đã "
        "đề xuất Redlock, một thuật toán chạy trên nhiều node Redis độc lập - thường "
        "là năm. Client cố giành cùng một khóa trên tất cả các node trong một ngân "
        "sách thời gian ngắn và chỉ coi là đã giữ khóa nếu thành công trên đa số (ba "
        "trên năm). Vì đa số vẫn tồn tại được khi mất một thiểu số node, nên không "
        "một sự cố sập đơn lẻ nào làm sập được dịch vụ khóa.",
    )
    b.fig(2, "fig2_2_redlock",
          "Redlock: client chỉ coi là giữ được khóa khi giành thành công trên đa số "
          "node Redis độc lập (3/5).",
          "Redlock: the client considers the lock held only when it is acquired on a "
          "majority of independent Redis nodes (3 of 5).")
    b.q_header("Khóa dựa trên Redis và thuật toán Redlock",
               "Redis-Based Locks and the Redlock Algorithm")
    b.qa("Lệnh nào dùng để giành khóa nguyên tử trên một node Redis?",
         "SET key token NX PX <ttl> - chỉ đặt khóa nếu chưa tồn tại (NX) và gắn thời "
         "hạn (PX).")
    b.qa("Vì sao giá trị của khóa lại là một token ngẫu nhiên?",
         "Để chỉ đúng chủ sở hữu mới trả được khóa; khi trả, script kiểm tra token "
         "khớp rồi mới xóa, tránh trả nhầm khóa của người khác.")
    b.qa("Ý tưởng cốt lõi của Redlock là gì?",
         "Chạy trên nhiều node Redis độc lập (thường 5) và chỉ coi là giữ khóa khi "
         "giành được trên đa số node, nhờ vậy chịu được sự cố của một thiểu số node.")

    # -- 2.3 -------------------------------------------------------------
    b.h2("Khóa dựa trên ZooKeeper", "ZooKeeper-Based Locks")
    b.p(
        "ZooKeeper is a coordination service built for exactly this kind of problem. "
        "It stores data in a tree of nodes called znodes and offers two features "
        "that make locking elegant. An ephemeral znode disappears automatically when "
        "the client that created it disconnects, which gives a perfect crash-driven "
        "release. A sequential znode is automatically given a monotonically "
        "increasing number, which gives a natural ordering.",
        "ZooKeeper là một dịch vụ điều phối được xây dựng đúng cho loại bài toán này. "
        "Nó lưu dữ liệu trong một cây các nút gọi là znode và cung cấp hai tính năng "
        "khiến việc khóa trở nên tao nhã. Một znode tạm thời (ephemeral) tự biến mất "
        "khi client tạo ra nó ngắt kết nối, đem lại cơ chế trả khóa hoàn hảo khi bị "
        "sập. Một znode tuần tự (sequential) tự động được gán một số tăng dần đều, "
        "đem lại một thứ tự tự nhiên.",
    )
    b.p(
        "To take a lock, a client creates an ephemeral sequential znode under a lock "
        "folder. The client whose node has the smallest number owns the lock; every "
        "other client simply watches the node immediately before its own and wakes "
        "up when that predecessor disappears. This produces a fair, ordered queue "
        "with no busy polling, and if any holder crashes its ephemeral node vanishes "
        "and the next in line is promoted.",
        "Để lấy khóa, client tạo một znode tạm thời tuần tự bên dưới một thư mục "
        "khóa. Client nào có znode mang số nhỏ nhất sẽ sở hữu khóa; mọi client khác "
        "chỉ việc theo dõi (watch) đúng znode liền trước znode của mình và được đánh "
        "thức khi nút liền trước đó biến mất. Cách này tạo ra một hàng đợi công bằng, "
        "có thứ tự, không cần thăm dò liên tục, và nếu bên giữ khóa bị sập thì znode "
        "tạm thời của nó biến mất và bên kế tiếp được đôn lên.",
    )
    b.fig(2, "fig2_3_zookeeper",
          "Khóa ZooKeeper: các znode tạm thời tuần tự xếp hàng; số nhỏ nhất giữ khóa, "
          "mỗi bên chờ chỉ theo dõi nút liền trước nó.",
          "ZooKeeper lock: ephemeral sequential znodes form a queue; the smallest "
          "number holds the lock and each waiter watches only its predecessor.")
    b.imp(
        "ZooKeeper builds a fair lock from ephemeral sequential znodes: the smallest "
        "sequence number wins, and a crashed holder's znode disappears automatically.",
        "ZooKeeper dựng một khóa công bằng từ các znode tạm thời tuần tự: số thứ tự "
        "nhỏ nhất thắng, và znode của bên giữ bị sập sẽ tự động biến mất.",
        highlight="green",
    )
    b.q_header("Khóa dựa trên ZooKeeper", "ZooKeeper-Based Locks")
    b.qa("Znode tạm thời (ephemeral) và znode tuần tự (sequential) khác nhau ra sao?",
         "Znode tạm thời tự xóa khi client ngắt kết nối; znode tuần tự được gán số "
         "tăng dần đều tạo ra thứ tự.")
    b.qa("Quy tắc xác định ai giữ khóa trong ZooKeeper là gì?",
         "Client có znode mang số thứ tự nhỏ nhất sẽ giữ khóa; các client khác theo "
         "dõi nút liền trước và chờ.")
    b.qa("Vì sao cách này tránh được hiện tượng thăm dò liên tục (busy polling)?",
         "Mỗi client chỉ đặt watch lên đúng nút liền trước nó và được đánh thức khi "
         "nút đó biến mất, thay vì hỏi đi hỏi lại trạng thái khóa.")

    # -- 2.4 -------------------------------------------------------------
    b.h2("Khóa dựa trên etcd và Consul", "etcd- and Consul-Based Locks")
    b.p(
        "etcd and Consul are modern coordination stores that, like ZooKeeper, are "
        "backed by a consensus algorithm (Raft) so that a majority of their nodes "
        "must agree on every change. They expose leases and atomic compare-and-swap "
        "operations from which locks are built directly. A client attaches a key to "
        "a lease, and the key - and therefore the lock - disappears when the lease "
        "is not renewed, giving the same crash safety as ZooKeeper's ephemeral nodes.",
        "etcd và Consul là những kho điều phối hiện đại mà, giống ZooKeeper, được hậu "
        "thuẫn bởi một thuật toán đồng thuận (Raft) để đa số các node phải nhất trí "
        "với mọi thay đổi. Chúng cung cấp cơ chế lease và các thao tác so-sánh-rồi-"
        "đổi (compare-and-swap) nguyên tử, từ đó dựng khóa một cách trực tiếp. Client "
        "gắn một khóa vào một lease, và khóa đó - tức là cái lock - sẽ biến mất khi "
        "lease không được gia hạn, đem lại độ an toàn khi sập tương tự như các znode "
        "tạm thời của ZooKeeper.",
    )
    b.p(
        "Because they are consensus-based, etcd and Consul give strong correctness "
        "guarantees similar to ZooKeeper, and they are the default choice in many "
        "cloud-native systems - etcd, for instance, is the coordination store behind "
        "Kubernetes. The trade-off is that consensus writes are slower than a single "
        "Redis SET, so these stores favour correctness over raw speed.",
        "Vì dựa trên đồng thuận, etcd và Consul đem lại những bảo đảm về tính đúng "
        "đắn mạnh mẽ tương tự ZooKeeper, và chúng là lựa chọn mặc định trong nhiều hệ "
        "thống cloud-native - chẳng hạn etcd chính là kho điều phối đứng sau "
        "Kubernetes. Cái giá phải trả là các lệnh ghi đồng thuận chậm hơn một lệnh "
        "SET đơn của Redis, nên những kho này ưu tiên tính đúng đắn hơn tốc độ thuần túy.",
    )
    b.q_header("Khóa dựa trên etcd và Consul", "etcd- and Consul-Based Locks")
    b.qa("etcd và Consul bảo đảm tính đúng đắn nhờ cơ chế nào?",
         "Nhờ thuật toán đồng thuận Raft: mọi thay đổi phải được đa số node nhất trí.")
    b.qa("etcd đóng vai trò gì trong hệ sinh thái cloud-native?",
         "etcd là kho điều phối đứng sau Kubernetes, lưu trạng thái và hỗ trợ khóa, "
         "bầu chọn leader.")

    b.key_points([
        "Mọi cách triển khai khóa đều cần một kho dùng chung có khả năng ra quyết "
        "định nguyên tử về quyền sở hữu.",
        "Khóa bằng cơ sở dữ liệu đơn giản nhưng chậm và dễ thành điểm hỏng đơn.",
        "Redis cho khóa rất nhanh; Redlock dùng đa số node để chịu lỗi.",
        "ZooKeeper, etcd và Consul dựa trên đồng thuận nên đúng đắn mạnh nhưng ghi chậm hơn.",
        "Hãy chọn theo nhu cầu: ưu tiên tốc độ thì Redis, ưu tiên tính đúng đắn thì "
        "dịch vụ đồng thuận.",
    ])
    b.review_section([
        "So sánh khóa bằng cơ sở dữ liệu và khóa bằng Redis về hiệu năng và độ phức tạp.",
        "Mô tả các bước giành khóa theo thuật toán Redlock và vì sao cần đa số node.",
        "Giải thích cách ZooKeeper dùng znode tạm thời tuần tự để dựng một khóa công bằng.",
        "Vì sao các kho dựa trên đồng thuận như etcd lại đúng đắn hơn nhưng chậm hơn Redis?",
    ])


# ===========================================================================
# CHAPTER 3
# ===========================================================================
def chapter_3(b):
    b.h1("THÁCH THỨC, CẠM BẪY VÀ THỰC HÀNH TỐT",
         "CHALLENGES, PITFALLS, AND BEST PRACTICES")
    b.p(
        "It is easy to make a distributed lock that usually works and surprisingly "
        "hard to make one that always works. The difficulties all trace back to a "
        "single fact: in a distributed system, processes pause, clocks drift, and "
        "the network delays or loses messages, all without warning. This chapter "
        "examines the traps these realities create and the techniques that defend "
        "against them.",
        "Làm một khóa phân tán thường hoạt động được thì dễ, nhưng làm một khóa luôn "
        "luôn hoạt động đúng thì khó đến bất ngờ. Mọi khó khăn đều quy về một sự "
        "thật: trong một hệ phân tán, các tiến trình có thể tạm dừng, đồng hồ chạy "
        "lệch, và mạng làm trễ hoặc đánh mất gói tin - tất cả đều không hề báo trước. "
        "Chương này xem xét những cái bẫy mà các thực tế đó tạo ra và những kỹ thuật "
        "phòng vệ chống lại chúng.",
    )

    # -- 3.1 -------------------------------------------------------------
    b.h2("Đồng hồ, độ trễ và thời gian thuê", "Clocks, Delays, and Leases")
    b.p(
        "A lease assumes that everyone agrees on how long a second is, but they do "
        "not. Clocks on different machines drift apart, and a process can be frozen "
        "for seconds by garbage collection or by the operating system scheduler. "
        "Suppose a client acquires a 30-second lease, then is paused for 40 seconds. "
        "Its lease has expired and another client has taken the lock, yet the first "
        "client wakes up still believing it is the owner. Now two clients act as the "
        "holder at the same time - exactly what the lock was meant to prevent.",
        "Thời gian thuê giả định rằng mọi bên đều nhất trí một giây dài bao nhiêu, "
        "nhưng thực tế không phải vậy. Đồng hồ trên các máy khác nhau trôi lệch dần, "
        "và một tiến trình có thể bị đóng băng vài giây do bộ thu gom rác (garbage "
        "collection) hoặc do bộ định thời của hệ điều hành. Giả sử một client giành "
        "thời gian thuê 30 giây rồi bị tạm dừng 40 giây. Thời gian thuê của nó đã hết "
        "hạn và một client khác đã lấy khóa, vậy mà client đầu tiên tỉnh dậy vẫn tin "
        "rằng mình là chủ. Lúc này hai client cùng hành xử như bên giữ khóa - đúng "
        "điều mà khóa lẽ ra phải ngăn chặn.",
    )
    b.imp(
        "A lock based on time alone is never perfectly safe: a paused process can "
        "outlive its own lease without knowing it.",
        "Một khóa chỉ dựa vào thời gian thì không bao giờ an toàn tuyệt đối: một tiến "
        "trình bị tạm dừng có thể sống lâu hơn cả thời gian thuê của chính nó mà "
        "không hề hay biết.",
        highlight="yellow",
    )

    # -- 3.2 -------------------------------------------------------------
    b.h2("Fencing token", "Fencing Tokens")
    b.p(
        "The cure for the paused-holder problem is a fencing token. Each time the "
        "lock is granted, the lock service also hands out a number that strictly "
        "increases with every grant. The client must include this token on every "
        "request it sends to the protected resource. The resource remembers the "
        "highest token it has ever seen and rejects any request carrying a smaller "
        "one. A revived old holder therefore arrives with a stale, smaller token and "
        "is safely turned away.",
        "Phương thuốc cho bài toán bên-giữ-bị-tạm-dừng là fencing token. Mỗi lần khóa "
        "được cấp, dịch vụ khóa đồng thời phát ra một con số tăng nghiêm ngặt sau mỗi "
        "lần cấp. Client phải kèm token này vào mọi yêu cầu mà nó gửi tới tài nguyên "
        "được bảo vệ. Tài nguyên ghi nhớ token lớn nhất từng thấy và từ chối mọi yêu "
        "cầu mang token nhỏ hơn. Nhờ vậy, một bên giữ cũ vừa hồi sinh sẽ đến với một "
        "token cũ, nhỏ hơn, và bị chặn lại một cách an toàn.",
    )
    b.fig(3, "fig3_1_fencing",
          "Fencing token: tài nguyên chỉ chấp nhận token lớn nhất, nên lệnh ghi từ bên "
          "giữ khóa cũ (token nhỏ hơn) bị từ chối.",
          "Fencing tokens: the resource accepts only the highest token, so a write "
          "from a stale holder carrying a smaller token is rejected.")
    b.imp(
        "Fencing tokens make safety independent of timing: even if two clients "
        "believe they hold the lock, only the one with the higher token can change "
        "the data.",
        "Fencing token làm cho tính an toàn không còn phụ thuộc vào thời gian: dù hai "
        "client cùng tưởng mình giữ khóa, chỉ bên có token lớn hơn mới thay đổi được "
        "dữ liệu.",
        highlight="green",
    )

    # -- 3.3 -------------------------------------------------------------
    b.h2("Phân vùng mạng và split-brain",
         "Network Partitions and Split-Brain")
    b.p(
        "A network partition splits the system into groups that cannot talk to each "
        "other. If the lock service is not careful, each side may independently "
        "conclude that it owns the lock - a condition called split-brain, in which "
        "two holders operate on the same resource at once. This is why correct lock "
        "services require a majority (a quorum) to grant a lock: only one side of "
        "any partition can contain the majority, so only one side can win.",
        "Một sự phân vùng mạng (network partition) chia hệ thống thành các nhóm không "
        "liên lạc được với nhau. Nếu dịch vụ khóa không cẩn thận, mỗi phía có thể độc "
        "lập kết luận rằng mình sở hữu khóa - một tình trạng gọi là split-brain, "
        "trong đó hai bên giữ cùng thao tác trên một tài nguyên cùng lúc. Đây là lý "
        "do các dịch vụ khóa đúng đắn đòi hỏi đa số (quorum) để cấp khóa: chỉ một "
        "phía của bất kỳ phân vùng nào có thể chứa đa số, nên chỉ một phía có thể thắng.",
    )
    b.fig(3, "fig3_2_splitbrain",
          "Split-brain: khi mạng bị phân vùng, hai phía có thể cùng tưởng mình đang giữ "
          "khóa và cùng thao tác trên tài nguyên.",
          "Split-brain: a network partition can leave two sides both believing they "
          "hold the lock and both acting on the resource.")
    b.q_header("Phân vùng mạng và split-brain",
               "Network Partitions and Split-Brain")
    b.qa("Split-brain là gì?",
         "Là tình trạng khi mạng bị phân vùng, hai phía cùng tưởng mình giữ khóa và "
         "cùng thao tác trên một tài nguyên, phá vỡ loại trừ tương hỗ.")
    b.qa("Vì sao yêu cầu đa số (quorum) lại ngăn được split-brain?",
         "Vì chỉ một phía của bất kỳ phân vùng nào mới có thể chứa đa số node, nên "
         "chỉ phía đó cấp được khóa.")

    # -- 3.4 -------------------------------------------------------------
    b.h2("Cuộc tranh luận về Redlock", "The Redlock Debate")
    b.p(
        "Redlock has been the subject of a famous debate. Critics argue that because "
        "Redlock relies on timing assumptions, a long pause or a clock jump can let "
        "two clients hold the lock, so Redlock alone is not safe enough for tasks "
        "where correctness is critical. Defenders reply that Redlock is fine for the "
        "common case of efficiency - avoiding duplicate work - as long as you do not "
        "trust it for correctness. The practical conclusion is widely agreed: if "
        "correctness truly matters, add fencing tokens.",
        "Redlock từng là tâm điểm của một cuộc tranh luận nổi tiếng. Phía phê phán "
        "lập luận rằng vì Redlock dựa vào các giả định về thời gian, một lần tạm dừng "
        "dài hay một cú nhảy đồng hồ có thể khiến hai client cùng giữ khóa, nên chỉ "
        "riêng Redlock thì chưa đủ an toàn cho những tác vụ mà tính đúng đắn là sống "
        "còn. Phía bênh vực đáp lại rằng Redlock vẫn ổn cho trường hợp phổ biến là "
        "tăng hiệu quả - tránh làm trùng việc - miễn là bạn không tin cậy nó cho tính "
        "đúng đắn. Kết luận thực tiễn được đồng thuận rộng rãi: nếu tính đúng đắn "
        "thật sự quan trọng, hãy bổ sung fencing token.",
    )
    b.imp(
        "Use a lock for efficiency and you can tolerate rare mistakes; use it for "
        "correctness and you must add fencing so a mistake cannot corrupt data.",
        "Dùng khóa để tăng hiệu quả thì có thể chấp nhận sai sót hiếm gặp; dùng khóa "
        "để bảo đảm tính đúng đắn thì bắt buộc phải thêm fencing để một sai sót không "
        "thể làm hỏng dữ liệu.",
        highlight="yellow",
    )

    # -- 3.5 -------------------------------------------------------------
    b.h2("Khuyến nghị thực hành", "Best-Practice Recommendations")
    b.p(
        "A few habits keep distributed locks out of trouble. Always set a lease so a "
        "crash cannot freeze the system, but make the lease comfortably longer than "
        "the expected work and renew it if the work runs long. Identify the owner "
        "with a unique token so no client can release another's lock. Keep critical "
        "sections short. And when the data must never be corrupted, do not rely on "
        "the lock alone - protect the resource itself with fencing tokens.",
        "Một vài thói quen giúp khóa phân tán tránh được rắc rối. Luôn đặt thời gian "
        "thuê để một sự cố sập không làm đóng băng hệ thống, nhưng hãy để thời gian "
        "thuê dài hơn công việc dự kiến một cách thoải mái và gia hạn nó nếu công "
        "việc kéo dài. Định danh chủ sở hữu bằng một token duy nhất để không client "
        "nào trả nhầm khóa của người khác. Giữ cho vùng tới hạn thật ngắn. Và khi dữ "
        "liệu tuyệt đối không được phép hỏng, đừng chỉ dựa vào khóa - hãy bảo vệ "
        "chính tài nguyên đó bằng fencing token.",
    )
    b.q_header("Khuyến nghị thực hành", "Best-Practice Recommendations")
    b.qa("Vì sao nên luôn đặt thời gian thuê nhưng để nó dài hơn công việc dự kiến?",
         "Để sự cố sập không khóa hệ thống mãi, đồng thời tránh khóa hết hạn giữa "
         "chừng khi công việc còn đang chạy; nếu công việc dài thì gia hạn lease.")
    b.qa("Biện pháp cuối cùng để dữ liệu không bị hỏng khi tính đúng đắn là sống còn là gì?",
         "Không chỉ dựa vào khóa mà bảo vệ chính tài nguyên bằng fencing token.")

    b.key_points([
        "Tạm dừng tiến trình và lệch đồng hồ khiến khóa chỉ dựa vào thời gian không "
        "bao giờ an toàn tuyệt đối.",
        "Fencing token làm tính an toàn độc lập với thời gian: chỉ token lớn nhất "
        "mới ghi được dữ liệu.",
        "Quorum (đa số) ngăn split-brain khi mạng bị phân vùng.",
        "Redlock phù hợp cho mục tiêu hiệu quả; cho tính đúng đắn cần thêm fencing.",
        "Thực hành tốt: luôn có lease đủ dài, định danh chủ bằng token, vùng tới hạn "
        "ngắn, và fencing khi cần.",
    ])
    b.review_section([
        "Giải thích bằng kịch bản tạm dừng 40 giây vì sao khóa dựa trên thời gian có thể mất an toàn.",
        "Fencing token là gì và nó khôi phục tính an toàn bằng cách nào?",
        "Split-brain xảy ra khi nào và quorum chống lại nó ra sao?",
        "Tóm tắt lập trường của hai phía trong cuộc tranh luận về Redlock.",
        "Liệt kê các khuyến nghị thực hành khi sử dụng khóa phân tán.",
    ])
