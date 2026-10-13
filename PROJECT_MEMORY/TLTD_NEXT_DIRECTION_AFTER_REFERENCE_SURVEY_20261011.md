# TLTD — hướng sửa lỗi và phát triển sau khảo sát game tham chiếu

Ngày 11/10/2026, UTC+07. Trạng thái: **ĐỀ XUẤT ĐỂ OWNER PHÂN TÍCH — CHƯA TRIỂN KHAI**.

Có đủ căn cứ để xếp ưu tiên sửa và chọn hướng gameplay tiếp theo. Chưa đủ căn cứ để chốt roadmap implementation toàn bộ game, cân bằng hoặc tutorial hoàn chỉnh. Tài liệu này là phân tích; không phải prompt sửa code, không cấp quyền chạy Unity hay mở Stage Gate Phase B.

## 1. Căn cứ và giới hạn

- Báo cáo source read-only ngày 11/10: `TLTD_GAME_SOURCE_READONLY_REVIEW_20261011.md`, trên working tree `E:\code\TLTD` ở mốc kiểm tra đã ghi trong báo cáo. Có đường source cụ thể, không phải các lỗi vừa tái hiện trong Unity.
- Hai phiên tham chiếu tách biệt: `TLTD_GIANG_HO_TRONG_TAY_GAMEPLAY_OBSERVATIONS_20261011.md` và `TLTD_GIANG_HO_TRONG_TAY_NEW_PLAYER_SURVEY_20261011.md`. Phiên mới dừng vì nhân vật bị khóa; chưa hoàn thành boss đầu, chuyển ải, loot đầu hoặc lên cấp.
- Gameplay gap audit đã CLOSED WITH REVIEW ADDENDUM ngày 05/10. Dùng kết luận đã sửa trong `TLTD_GAMEPLAY_GAP_AUDIT_REVIEW_20261005.md`, không lấy nguyên các claim quá mức của executor làm authority.
- Stage Gate R1 CLOSED AS ANALYSIS WITH ADDENDUM; lab riêng từng được cho phép để kiểm chứng kỹ thuật, production Stage/Boss vẫn chưa được duyệt. Review STATIC03-R3 mới nhất trong hồ sơ đã đọc yêu cầu **R4 / SOURCE PREPARATION ONLY / NO RUN AUTHORIZED**.
- Không resolve remote main, đo lại toàn source hoặc kiểm tra save trong lượt tổng hợp này. Các commit/hash là bằng chứng có ngày đo trong báo cáo nguồn, không được nâng thành khẳng định remote hiện tại.

## 2. Ưu tiên sửa lỗi

| Thứ tự | Vấn đề và căn cứ | Hướng sửa hẹp đề xuất | Kết quả cần chứng minh khi có task thực thi |
|---|---|---|---|
| 1 | **R01: loot sau finishing channel**. Source E còn đường trình bày riêng thiếu cờ mở quyết định; Phase A trước đây đã xác minh một fixture bị từ chối giao dịch. Bản sửa runtime lab được chấp nhận với giới hạn evidence, closeout còn phụ thuộc harness. | Tiếp tục R4 sửa hai file harness theo prompt đã có, review static trước. Chỉ sau source readiness và một chỉ đạo chạy riêng mới có phiên kiểm chứng; tích hợp E là bước riêng. | Handoff dùng presentation/defer authority hiện có; first loot và chuỗi loot qua UI thật; mỗi quyết định có effect đúng một lần; modal drain rồi mới advance; hủy khi Hero chết; cleanup và evidence đủ trong phạm vi đã giao. |
| 2 | **R02: ownership của Equip/Upgrade**. Lựa chọn item cũ trong bảng DEV có thể tồn tại sau Tách; transaction chưa chặn item không còn sở hữu. Giới hạn: đường source prototype/debug, chưa tái hiện exploit bản phát hành. | Validate ownership trước mọi thay đổi hoặc chi tài nguyên; xóa lựa chọn UI đã hết hiệu lực. Item đang mặc hợp lệ vẫn được xử lý theo contract. | Item đã Tách bị từ chối Equip/Upgrade; không tháo đồ hiện tại, không trừ tiền, không đổi stats. Item hợp lệ vẫn hoạt động. |
| 3 | **R03: Cấp Rơi tới hạn vẫn chờ HOÀN THÀNH**, lệch A9/D22. | Manager đối chiếu timestamp khi load và runtime; hoàn tất đúng một cấp, đúng một lần, cả khi panel đóng. UI bỏ yêu cầu Claim. | Panel đóng vẫn complete; mở lại sau deadline complete đúng một lần; không trả phí lần hai, không queue/auto-chain/cancel; drop dùng cấp đã hoàn tất. |
| 4 | **R04: builder tiếp tục thêm effect subasset dư**. Cơ chế có đường tạo cụ thể và asset mẫu đã phình; chưa đo tác động FPS/import. | Ngăn builder tạo thêm dữ liệu dư trước vòng authoring tiếp. Dọn asset lịch sử thành migration riêng sau đánh giá GUID/fileID và tham chiếu. | Chạy lặp builder trong bản sao kiểm chứng không tiếp tục tăng owned subassets và không làm mất tham chiếu. Không thử trên working tree/save thật. |

R02 và R03 là hai bản sửa hẹp có thể tách thành task riêng sau khi Owner phân tích báo cáo. Không gộp chúng với rewrite battle, save framework hoặc UI polish.

**R05 — expiry/reapply của status** để backlog foundation có điều kiện: chưa xác định content đang chọn thực sự kích hoạt timed stackable effect này. Khi sửa cần kiểm hai thứ tự Apply/Tick tại cùng timestamp; chưa có lý do bật thêm skill content để ép test.

**Gold-only recycle vẫn CLOSED / FIX VERIFIED trong evidence limits cũ.** R02 không mở lại reward contract, không thêm Material/EXP và không yêu cầu làm lại ZIP hoặc nghiệm thu Tách đã xong.

## 3. Hướng gameplay tiếp theo

Lát cắt ưu tiên đề xuất vẫn là **Stage progress + Boss Gate foundation**, vì source đã có chiến đấu/loot nhưng chưa nối thành một mục tiêu vượt ải. Khảo sát tham chiếu củng cố giá trị của việc người chơi thấy đích tiến triển kế tiếp; nó không thay thế D18 hoặc phê duyệt Stage/Boss production.

Phạm vi foundation cần giữ:

1. Tiến độ từ cái chết hợp lệ của quái thường, dedupe theo identity; ngưỡng hiện hành theo thiết kế là **50 quái**, không chuyển thành 5 wave hoặc copy số nhóm quái vừa thấy ở game tham chiếu.
2. Đạt ngưỡng giữa wave thì latch chờ; giữ roster 4–5 quái và growth theo completed wave của P08. Không xóa quái sống để giả hoàn thành.
3. Chờ wave, natural channel, loot và modal kết thúc hợp lệ rồi mới vào gate; không spawn wave thường kế tiếp.
4. HUD chức năng chỉ rõ đang tiến tới gate hay đã sẵn sàng; chưa thêm Boss combat/reward/advance giả.

**Trạng thái hiện tại:** Stage Gate Phase B vẫn STOPPED bởi luồng corrective/harness. Không triển khai các mục trên vào E trong lượt này. Hồ sơ analysis/lab đã có, không cần audit lại tám nhóm từ đầu. Bước sau R4 là review nguồn và evidence đúng boundary, rồi mới xét tiếp continuation lab; không tự mở Phase B khi thấy một báo cáo PASS.

Trước production phải chốt riêng Stage persistence/checkpoint và các nhánh reopen/background/result/reward theo thiết kế. Không chứng minh persistence chỉ bằng việc có vài PlayerPrefs keys; cũng không tự mở một save framework toàn cục. Không khẳng định thêm Stage sẽ tự gỡ một yêu cầu Danh Hiệu đang active — audit addendum đã loại claim này.

Một lát cắt nhỏ khác có thể chọn sau đó là **chọn/trang bị Công Pháp đang có qua UI**: nối API chọn slot hiện hữu với dữ liệu đã chọn, thay vì chỉ in danh sách. Đây là phương án phát triển đề xuất, không task đã được duyệt; không đồng nghĩa bật projectile/dash cho toàn bộ asset hoặc làm lại hệ skill.

## 4. Điều học được từ trải nghiệm mới

Chuỗi đã quan sát có ích: **một hành động rõ → nhiệm vụ ngắn → thưởng đủ cho thao tác kế → nâng Cấp Rơi → chỉ tới boss**. TLTD nên được đánh giá theo khả năng người mới hiểu việc tiếp theo và tác dụng của phần thưởng.

Các tiêu chí UX đề xuất để rà soát khi có vòng gameplay ổn định:

- Phân biệt bằng trạng thái/nhãn chức năng giữa đánh thường, mở cổng, khiêu chiến và tự động; người chơi hiểu thao tác nào tiêu tài nguyên.
- Loot cho thấy so sánh, lựa chọn và kết quả đủ rõ; quyết định chưa xong không làm người chơi mất dấu tiến trình.
- Cấp Rơi cho thấy cấp hiện tại, giá nâng, thời gian và thay đổi chất lượng; hết giờ không thêm một bước Claim trái contract.
- Chỉ dẫn tập trung vào một mục tiêu trước mắt, không mở đồng loạt hệ thống.

Đây là hướng đánh giá, chưa phê duyệt tutorial/quest mới. Không copy nhãn nhiệm vụ lệch 5 với counter 3/6; không lấy cấp mở Công Pháp 4, giá 500 Xu, thời gian 5 phút, xác suất rarity hoặc thưởng boss của reference làm balance TLTD. Phiên mới chỉ chứng minh completion do tăng tốc, chưa chứng minh timer tự hết/offline. Không dùng các lần thua/chuyển trạng thái khi điều khiển chậm để chốt cân bằng combat.

Các phần còn thiếu không chặn việc chuẩn bị bản sửa source đã nêu. Chưa cần một tài khoản tham chiếu mới hoặc một phiên tự động hóa khác. Mọi khảo sát game tiếp theo để Owner chơi, Codex quan sát/phân tích.

## 5. Bước tiếp theo cụ thể và boundary

**Việc kỹ thuật đang dang dở:** `F-LOOT-LAB-HARNESS-STATIC-03-R4`. Prompt thực thi đã tách riêng tại `PROMPT_ANTIGRAVITY_LOOT_LAB_STATIC03_R4_FIX_20261008.md`. Chỉ hai file mutable trong lab; runtime/meta/settings và E giữ frozen; không Unity/compiler qua Editor, wrapper/Guard/preflight action hoặc live Registry/save. Chưa có bằng chứng dispatch/thực hiện R4 trong hồ sơ đã đọc. Tài liệu này không gửi prompt cho Antigravity và không thay quyền hạn bằng một lời đề xuất.

Sau Owner phân tích các ưu tiên R02/R03, nếu chọn sửa thì lập prompt riêng cho từng task, ghi allowlist, before/after, kiểm chứng bị ảnh hưởng và điểm dừng review. Không lấy câu hỏi về hướng đi thành lệnh sửa code.

P08/P09-A/P09-B giữ ACCEPTED / LOCKED. Không retest/ZIP các phần đã xong khi không có delta hoặc rủi ro mới cụ thể. Giữ nguyên giới hạn raw evidence và recovery history; không chạy recovery để làm đẹp hồ sơ. Không sửa code/asset/save, chạy Unity hoặc thao tác Git trong lượt tổng hợp này.
