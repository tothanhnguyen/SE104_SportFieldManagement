#!/usr/bin/env python3
"""Generate clean schematic diagrams for the Distributed Locking document.

Each conceptual figure is rendered twice: an English version (suffix _en) and a
Vietnamese version (suffix _vi), per the assignment requirement that English
figures be accompanied by a Vietnamese translation.
"""
import os
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch

OUT = os.path.join(os.path.dirname(__file__), "images")
os.makedirs(OUT, exist_ok=True)

# Color palette (calm, print-friendly)
C_CLIENT = "#DCE9F7"
C_CLIENT_E = "#2E5F8A"
C_LOCK = "#FCE8C3"
C_LOCK_E = "#B8860B"
C_RES = "#D8F0D8"
C_RES_E = "#3C7D3C"
C_BAD = "#F8D7D7"
C_BAD_E = "#B03030"
C_OK = "#D8F0D8"
C_GRAY = "#EDEDED"
C_GRAY_E = "#777777"


def box(ax, x, y, w, h, text, fc, ec, fs=11, bold=False, tc="#1A1A1A"):
    p = FancyBboxPatch((x, y), w, h, boxstyle="round,pad=0.02,rounding_size=0.06",
                       fc=fc, ec=ec, lw=1.6)
    ax.add_patch(p)
    ax.text(x + w / 2, y + h / 2, text, ha="center", va="center",
            fontsize=fs, wrap=True, color=tc,
            fontweight="bold" if bold else "normal")


def arrow(ax, x1, y1, x2, y2, color="#444444", style="-|>", lw=1.6, ls="-"):
    a = FancyArrowPatch((x1, y1), (x2, y2), arrowstyle=style,
                        mutation_scale=16, color=color, lw=lw, linestyle=ls)
    ax.add_patch(a)


def new_ax(w=8.4, h=4.6):
    fig, ax = plt.subplots(figsize=(w, h))
    ax.set_xlim(0, 10)
    ax.set_ylim(0, 6)
    ax.axis("off")
    return fig, ax


def save(fig, name):
    path = os.path.join(OUT, name + ".png")
    fig.savefig(path, dpi=150, bbox_inches="tight", facecolor="white")
    plt.close(fig)
    print("wrote", path)


# ---------------------------------------------------------------------------
# Figure 1-1: The mutual-exclusion problem (without lock vs. with lock)
# ---------------------------------------------------------------------------
def fig_mutex(lang):
    t = {
        "en": dict(title_l="Without a lock (race condition)",
                   title_r="With a distributed lock",
                   a="Client A", b="Client B", res="Shared resource\n(balance = 100)",
                   bad="Lost update!\n(balance wrong)", ok="Consistent\n(balance correct)",
                   w="write", lock="holds lock", wait="waits"),
        "vi": dict(title_l="Không có khóa (tranh chấp dữ liệu)",
                   title_r="Có khóa phân tán",
                   a="Client A", b="Client B", res="Tài nguyên dùng chung\n(số dư = 100)",
                   bad="Mất cập nhật!\n(số dư sai)", ok="Nhất quán\n(số dư đúng)",
                   w="ghi", lock="giữ khóa", wait="chờ"),
    }[lang]
    fig, ax = new_ax(9.2, 4.8)
    # left panel
    ax.text(2.5, 5.7, t["title_l"], ha="center", fontsize=11, fontweight="bold")
    box(ax, 0.3, 4.0, 1.9, 0.8, t["a"], C_CLIENT, C_CLIENT_E)
    box(ax, 0.3, 2.5, 1.9, 0.8, t["b"], C_CLIENT, C_CLIENT_E)
    box(ax, 3.2, 3.1, 1.7, 1.1, t["res"], C_RES, C_RES_E, fs=9)
    arrow(ax, 2.2, 4.4, 3.2, 3.9, color=C_CLIENT_E)
    arrow(ax, 2.2, 2.9, 3.2, 3.4, color=C_CLIENT_E)
    ax.text(2.7, 4.35, t["w"], fontsize=8, color=C_CLIENT_E)
    ax.text(2.7, 2.75, t["w"], fontsize=8, color=C_CLIENT_E)
    box(ax, 3.0, 1.3, 2.1, 0.9, t["bad"], C_BAD, C_BAD_E, fs=9, bold=True)
    arrow(ax, 4.05, 3.1, 4.05, 2.2, color=C_BAD_E)
    # divider
    ax.plot([5.4, 5.4], [0.8, 5.4], color="#CCCCCC", lw=1, ls="--")
    # right panel
    ax.text(7.9, 5.7, t["title_r"], ha="center", fontsize=11, fontweight="bold")
    box(ax, 5.7, 4.0, 1.9, 0.8, t["a"], C_CLIENT, C_CLIENT_E)
    box(ax, 5.7, 2.5, 1.9, 0.8, t["b"], C_CLIENT, C_CLIENT_E)
    box(ax, 8.4, 3.1, 1.4, 1.1, t["res"], C_RES, C_RES_E, fs=8)
    arrow(ax, 7.6, 4.4, 8.4, 3.95, color=C_LOCK_E, lw=2.0)
    arrow(ax, 7.6, 2.9, 8.4, 3.35, color=C_GRAY_E, ls=":")
    ax.text(7.75, 4.45, t["lock"], fontsize=8, color=C_LOCK_E, fontweight="bold")
    ax.text(7.75, 2.55, t["wait"], fontsize=8, color=C_GRAY_E)
    box(ax, 7.0, 1.3, 2.1, 0.9, t["ok"], C_OK, C_RES_E, fs=9, bold=True)
    arrow(ax, 9.1, 3.1, 8.2, 2.2, color=C_RES_E)
    save(fig, "fig1_1_mutex_" + lang)


# ---------------------------------------------------------------------------
# Figure 1-2: Lock lifecycle with lease (TTL)
# ---------------------------------------------------------------------------
def fig_lifecycle(lang):
    t = {
        "en": dict(s=["Acquire\n(with TTL)", "Critical\nsection", "Release\n(or expire)"],
                   note="If the holder crashes, the lease (TTL) expires\nautomatically so the lock is not held forever.",
                   title="Lifecycle of a distributed lock"),
        "vi": dict(s=["Giành khóa\n(kèm TTL)", "Vùng\ntới hạn", "Trả khóa\n(hoặc hết hạn)"],
                   note="Nếu tiến trình giữ khóa bị sập, thời gian thuê (TTL)\ntự hết hạn nên khóa không bị giữ mãi mãi.",
                   title="Vòng đời của một khóa phân tán"),
    }[lang]
    fig, ax = new_ax(9.0, 3.4)
    ax.text(5, 5.5, t["title"], ha="center", fontsize=11, fontweight="bold")
    xs = [0.6, 4.0, 7.4]
    cols = [C_CLIENT, C_LOCK, C_RES]
    ecs = [C_CLIENT_E, C_LOCK_E, C_RES_E]
    for i, x in enumerate(xs):
        box(ax, x, 3.0, 2.0, 1.2, t["s"][i], cols[i], ecs[i], fs=10, bold=True)
    arrow(ax, 2.6, 3.6, 4.0, 3.6, lw=2)
    arrow(ax, 6.0, 3.6, 7.4, 3.6, lw=2)
    ax.text(5.0, 1.5, t["note"], ha="center", fontsize=9, color="#555555",
            bbox=dict(boxstyle="round,pad=0.4", fc="#FFF9E6", ec=C_LOCK_E))
    save(fig, "fig1_2_lifecycle_" + lang)


# ---------------------------------------------------------------------------
# Figure 2-1: Lock-service architecture
# ---------------------------------------------------------------------------
def fig_architecture(lang):
    t = {
        "en": dict(c="Client", lm="Lock Manager\n(coordination service)",
                   res="Protected resource",
                   req="1. request lock", grant="2. grant / deny",
                   acc="3. access (lock held)", title="Lock-service architecture"),
        "vi": dict(c="Client", lm="Bộ quản lý khóa\n(dịch vụ điều phối)",
                   res="Tài nguyên được bảo vệ",
                   req="1. yêu cầu khóa", grant="2. cấp / từ chối",
                   acc="3. truy cập (đang giữ khóa)", title="Kiến trúc dịch vụ khóa"),
    }[lang]
    fig, ax = new_ax(8.6, 4.4)
    ax.text(5, 5.7, t["title"], ha="center", fontsize=11, fontweight="bold")
    for i, y in enumerate([4.3, 2.9, 1.5]):
        box(ax, 0.4, y, 1.9, 0.9, t["c"] + f" {i+1}", C_CLIENT, C_CLIENT_E, fs=10)
    box(ax, 3.6, 2.6, 2.6, 1.4, t["lm"], C_LOCK, C_LOCK_E, fs=10, bold=True)
    box(ax, 7.2, 2.7, 2.4, 1.2, t["res"], C_RES, C_RES_E, fs=9)
    for y in [4.75, 3.35, 1.95]:
        arrow(ax, 2.3, y, 3.6, 3.3, color=C_CLIENT_E)
    arrow(ax, 6.2, 3.3, 7.2, 3.3, color=C_LOCK_E, lw=2)
    ax.text(1.9, 5.05, t["req"], fontsize=8, color=C_CLIENT_E)
    ax.text(3.7, 4.15, t["grant"], fontsize=8, color=C_LOCK_E)
    ax.text(6.25, 3.45, t["acc"], fontsize=8, color=C_RES_E)
    save(fig, "fig2_1_architecture_" + lang)


# ---------------------------------------------------------------------------
# Figure 2-2: Redlock majority acquisition
# ---------------------------------------------------------------------------
def fig_redlock(lang):
    t = {
        "en": dict(client="Client", title="Redlock: acquire on a majority of N nodes",
                   note="Lock is held only if it is acquired on\n3 of 5 independent Redis nodes (a majority).",
                   ok="OK", fail="timeout"),
        "vi": dict(client="Client", title="Redlock: giành khóa trên đa số N node",
                   note="Khóa chỉ được coi là đã giành nếu lấy được trên\n3 trong 5 node Redis độc lập (đa số).",
                   ok="OK", fail="quá hạn"),
    }[lang]
    fig, ax = new_ax(8.6, 4.6)
    ax.text(5, 5.7, t["title"], ha="center", fontsize=11, fontweight="bold")
    box(ax, 0.4, 2.6, 1.8, 1.0, t["client"], C_CLIENT, C_CLIENT_E, bold=True)
    ys = [4.7, 3.7, 2.7, 1.7, 0.7]
    states = [True, True, True, False, False]
    for i, (y, ok) in enumerate(zip(ys, states)):
        fc = C_OK if ok else C_BAD
        ec = C_RES_E if ok else C_BAD_E
        box(ax, 5.6, y, 2.6, 0.8, f"Redis {i+1}", fc, ec, fs=10)
        arrow(ax, 2.2, 3.1, 5.6, y + 0.4,
              color=C_RES_E if ok else C_BAD_E, ls="-" if ok else ":")
        ax.text(8.35, y + 0.4, t["ok"] if ok else t["fail"], fontsize=8,
                va="center", color=C_RES_E if ok else C_BAD_E, fontweight="bold")
    ax.text(2.9, 0.7, t["note"], ha="center", fontsize=9, color="#555555",
            bbox=dict(boxstyle="round,pad=0.35", fc="#FFF9E6", ec=C_LOCK_E))
    save(fig, "fig2_2_redlock_" + lang)


# ---------------------------------------------------------------------------
# Figure 2-3: ZooKeeper ephemeral sequential znodes (lock queue)
# ---------------------------------------------------------------------------
def fig_zookeeper(lang):
    t = {
        "en": dict(title="ZooKeeper lock: ephemeral sequential znodes",
                   parent="/lock", holder="lock-0001  (holder)",
                   w1="lock-0002  (waits, watches 0001)",
                   w2="lock-0003  (waits, watches 0002)",
                   note="Smallest sequence number owns the lock.\nEach waiter watches only the node just before it."),
        "vi": dict(title="Khóa ZooKeeper: znode tuần tự tạm thời",
                   parent="/lock", holder="lock-0001  (đang giữ)",
                   w1="lock-0002  (chờ, theo dõi 0001)",
                   w2="lock-0003  (chờ, theo dõi 0002)",
                   note="Số thứ tự nhỏ nhất sở hữu khóa.\nMỗi tiến trình chờ chỉ theo dõi node liền trước nó."),
    }[lang]
    fig, ax = new_ax(8.4, 4.6)
    ax.text(5, 5.7, t["title"], ha="center", fontsize=11, fontweight="bold")
    box(ax, 3.7, 4.5, 2.6, 0.8, t["parent"], C_GRAY, C_GRAY_E, bold=True)
    box(ax, 3.0, 3.2, 4.0, 0.8, t["holder"], C_OK, C_RES_E, fs=10, bold=True)
    box(ax, 3.0, 2.1, 4.0, 0.8, t["w1"], C_LOCK, C_LOCK_E, fs=9)
    box(ax, 3.0, 1.0, 4.0, 0.8, t["w2"], C_LOCK, C_LOCK_E, fs=9)
    arrow(ax, 5.0, 4.5, 5.0, 4.0, color=C_GRAY_E)
    arrow(ax, 2.9, 2.5, 2.9, 3.6, color=C_LOCK_E, ls=":")
    arrow(ax, 2.9, 1.4, 2.9, 2.5, color=C_LOCK_E, ls=":")
    ax.text(5.0, 0.4, t["note"], ha="center", fontsize=9, color="#555555")
    save(fig, "fig2_3_zookeeper_" + lang)


# ---------------------------------------------------------------------------
# Figure 3-1: Fencing tokens
# ---------------------------------------------------------------------------
def fig_fencing(lang):
    t = {
        "en": dict(title="Fencing tokens reject a stale lock holder",
                   a="Client A\n(paused, token 33)", b="Client B\n(token 34)",
                   res="Storage / resource",
                   wa="write, token = 33", wb="write, token = 34",
                   rej="REJECTED\n(33 < 34)", acc="ACCEPTED",
                   note="The resource remembers the highest token seen and\nrejects any write carrying a smaller token."),
        "vi": dict(title="Fencing token loại bỏ tiến trình giữ khóa cũ",
                   a="Client A\n(bị treo, token 33)", b="Client B\n(token 34)",
                   res="Kho lưu trữ / tài nguyên",
                   wa="ghi, token = 33", wb="ghi, token = 34",
                   rej="TỪ CHỐI\n(33 < 34)", acc="CHẤP NHẬN",
                   note="Tài nguyên ghi nhớ token lớn nhất đã thấy và từ chối\nmọi lệnh ghi mang token nhỏ hơn."),
    }[lang]
    fig, ax = new_ax(8.8, 4.6)
    ax.text(5, 5.7, t["title"], ha="center", fontsize=11, fontweight="bold")
    box(ax, 0.3, 3.8, 2.3, 1.0, t["a"], C_BAD, C_BAD_E, fs=9)
    box(ax, 0.3, 1.6, 2.3, 1.0, t["b"], C_CLIENT, C_CLIENT_E, fs=9)
    box(ax, 6.4, 2.6, 2.9, 1.2, t["res"], C_RES, C_RES_E, fs=9, bold=True)
    arrow(ax, 2.6, 4.2, 6.4, 3.4, color=C_BAD_E, ls=":")
    arrow(ax, 2.6, 2.1, 6.4, 3.0, color=C_CLIENT_E)
    ax.text(4.0, 4.15, t["wa"], fontsize=8, color=C_BAD_E)
    ax.text(4.0, 2.25, t["wb"], fontsize=8, color=C_CLIENT_E)
    box(ax, 6.6, 4.0, 2.4, 0.8, t["rej"], C_BAD, C_BAD_E, fs=8, bold=True)
    box(ax, 6.6, 1.5, 2.4, 0.7, t["acc"], C_OK, C_RES_E, fs=9, bold=True)
    ax.text(5.0, 0.5, t["note"], ha="center", fontsize=9, color="#555555")
    save(fig, "fig3_1_fencing_" + lang)


# ---------------------------------------------------------------------------
# Figure 3-2: Split-brain after a network partition
# ---------------------------------------------------------------------------
def fig_splitbrain(lang):
    t = {
        "en": dict(title="Split-brain: a partition lets two holders believe they own the lock",
                   a="Client A", b="Client B", res="Shared resource",
                   p="network partition", both="Both think\nthey hold the lock!"),
        "vi": dict(title="Split-brain: phân vùng khiến hai bên cùng tưởng mình giữ khóa",
                   a="Client A", b="Client B", res="Tài nguyên dùng chung",
                   p="phân vùng mạng", both="Cả hai đều tưởng\nmình đang giữ khóa!"),
    }[lang]
    fig, ax = new_ax(8.6, 4.4)
    ax.text(5, 5.7, t["title"], ha="center", fontsize=10.5, fontweight="bold")
    box(ax, 0.5, 3.6, 2.0, 1.0, t["a"], C_BAD, C_BAD_E, bold=True)
    box(ax, 0.5, 1.0, 2.0, 1.0, t["b"], C_BAD, C_BAD_E, bold=True)
    box(ax, 7.0, 2.3, 2.4, 1.2, t["res"], C_RES, C_RES_E, fs=9)
    ax.plot([5.0, 5.0], [0.6, 5.0], color=C_BAD_E, lw=2.4, ls="--")
    ax.text(5.0, 5.15, t["p"], ha="center", fontsize=9, color=C_BAD_E, fontweight="bold")
    arrow(ax, 2.5, 4.1, 7.0, 3.1, color=C_BAD_E)
    arrow(ax, 2.5, 1.5, 7.0, 2.7, color=C_BAD_E)
    box(ax, 3.3, 0.2, 3.4, 0.8, t["both"], C_BAD, C_BAD_E, fs=9, bold=True)
    save(fig, "fig3_2_splitbrain_" + lang)


def main():
    for lang in ("en", "vi"):
        fig_mutex(lang)
        fig_lifecycle(lang)
        fig_architecture(lang)
        fig_redlock(lang)
        fig_zookeeper(lang)
        fig_fencing(lang)
        fig_splitbrain(lang)
    print("All diagrams generated.")


if __name__ == "__main__":
    main()
