Bạn là Antigravity, chịu trách nhiệm thực thi trên project E:\code\TLTD. Hãy sửa các finding đã xác nhận dưới đây, kiểm chứng và bàn giao ZIP thật. Không dừng ở việc giải thích hoặc lập kế hoạch.

Trạng thái hiện tại: phần production R1 đã được Tech Lead công nhận qua source/log; P09-A chưa LOCK do các điểm còn lại ở runner/wrapper. P08 giữ ACCEPTED / LOCKED. Chủ dự án sẽ kiểm tra game trực tiếp; kết quả tự động và kết quả người dùng phải được ghi riêng.

1. PHẠM VI VÀ BẢO TOÀN

- Snapshot đối chiếu: review_package_p09a_r1_r3_fixed.zip, SHA256 E456C9F3E1878BD646C9A3C7B2512A379F6077CF42FE43A98199CA28321429A6. HEAD từng được báo cáo là 5562ef15632a8624dd81bc13c6e5c98d7392b0c5; tự ghi nhận HEAD/branch/status thực tế trước khi làm, không ép workspace về commit này.
- Sửa tập trung: Assets/_Game/Editor/Prototype01PlayTestRunner_P09.cs; Tools/Verification/P09/run_gate1_p09.ps1; run_gate2_playmode_p09.ps1; test_wrapper_failure_paths.ps1. Được thêm helper kiểm thử dùng chung hoặc launcher quan sát thủ công có Save Guard nếu cần. Không nhân bản logic wrapper để kiểm thử bản sao.
- Giữ production R1 đã chốt; không sửa gameplay để làm test PASS. Không chỉnh UI/Dash/VFX, scene hay skill asset sản xuất để phục vụ báo cáo. Giữ nguyên các thay đổi chưa commit có sẵn.
- Không git reset, checkout/restore ghi đè, clean, stash, stage, commit hoặc push. Ghi inventory/hash trước–sau để phân biệt thay đổi của nhiệm vụ với thay đổi đã có.

2. SỬA T21: SCENE UNLOAD THẬT

- T21 hiện gọi HandleSceneUnloaded bằng reflection, fallback gọi Cancel; đây chưa phải integration test scene unload.
- Tạo scene fixture tạm, bố trí ownership rõ ràng và thực sự đóng/unload scene liên quan bằng API Unity. Quan sát cleanup, active projectile và việc không phát sinh damage sau unload.
- Không gọi handler, Cancel hoặc ClearAllProjectiles làm tác nhân thay thế unload. Cleanup dự phòng của finally không được tính là bằng chứng assertion.
- Nếu cần natural frames, đưa phần integration vào Gate 2 và ghi rõ mapping/test count thực tế; không giữ nhãn “21/21 integration verified” nếu chưa chạy hành vi tương ứng.

3. SỬA T02/T09: COOLDOWN VÀ OBSERVER

- Chỉ dùng TestTimeProvider sẵn có trong test Edit Mode. Tiến clock giữa release và impact; kiểm tra deadline không thay đổi hoặc remaining giảm đúng khoảng thời gian đã tiến. Không chỉ dùng remainingAfter <= remainingAtRelease khi clock đứng yên. Khôi phục provider cũ trong finally, không sửa CooldownManager production.
- Giữ assertion zero damage trước impact, đúng một damage event, đúng target/attacker, Rage không trừ lần hai và không có damage lặp.
- Rage trừ tại cast start; cooldown bắt đầu tại successful release. Với cast-time skill, phải kiểm chứng hai thời điểm này riêng biệt.
- damagedSkillId = skill.SkillId rồi so lại cùng expected value không chứng minh identity. Quan sát identity thực nếu API hiện có cung cấp; nếu không, bỏ claim này và mô tả đúng những gì event chứng minh được. Không thêm production API chỉ để hợp thức hóa claim.
- Gate 2 và kiểm tra trực tiếp dùng Unity natural frames, Time.timeScale = 1.0 khi chạy combat. Ghi timeScale thực tế và kiểm tra cooldown qua thời gian thực của game. Không synthetic deltaTime, manual Tick/Update/Attack, teleport, chỉnh transform trong lúc quan sát, ép damage/HP/death. Được cấu hình fixture RAM và vị trí ban đầu trước khi bắt đầu scenario.

4. SỬA WRAPPER VÀ KIỂM THỬ NHÁNH LỖI THỰC

- Overall PASS đòi hỏi scenario/suite PASS, Unity OS exit 0, không timeout, Restore thành công và Compare khớp baseline.
- Lỗi persistence phải làm overall FAIL, kể cả scenario PASS. RestoreFail + ComparePass vẫn FAIL; Timeout + CompareFail vẫn là persistence failure. Chỉ trả trạng thái riêng SCENARIO_PASS_EXIT_TIMEOUT khi scenario đã PASS và recovery/compare đều thành công.
- Start-Process phải được bắt lỗi; xác nhận process object/PID hợp lệ trước wait/watchdog/kill. Backup lỗi thì không launch Unity. Chỉ dừng cây tiến trình do chính lần chạy tạo ra, không kill Unity theo tên hoặc động vào Editor đang mở bên ngoài.
- Sau backup hợp lệ, bảo đảm recovery trong finally khi launch lỗi, crash hoặc timeout. Ghi và xử lý riêng kết quả Restore/Compare; lỗi bước trước không được âm thầm bỏ qua bước sau. Không hardcode Diff=0.
- test_wrapper_failure_paths.ps1 phải chạy wrapper thật hoặc đúng implementation điều phối dùng chung mà cả hai wrapper gọi. Dùng sandbox registry/process dùng một lần và failure injection có kiểm soát; không tác động save thật để thử lỗi.
- Kiểm chứng backup fail không launch; launch fail được xử lý; crash/timeout vẫn recovery; compare mismatch bị từ chối PASS; RestoreFail + ComparePass bị từ chối PASS; Timeout + CompareFail không bị che thành timeout đơn thuần. Đối chiếu exit code/status và launch/recovery sentinel thực tế. Không chỉ gọi helper hoặc viết một try/finally khác rồi suy ra wrapper đã đúng.

5. CHẠY VÀ LƯU BẰNG CHỨNG

- Sau thay đổi cuối cùng, chạy compile, P09 Gate 1, Gate 2 và failure-path tests. Không dùng log của source cũ làm bằng chứng cho source mới.
- Ghi timestamp, Unity version, lệnh thực thi, HEAD và hash source kiểm chứng; giữ raw Unity log, stdout/stderr wrapper và Save Guard log. Tách suite/scenario result, OS exit, wrapper exit, timeout, Restore result và Compare result.
- Gate 2 trước đây đã có hai segment gameplay PASS nhưng process timeout. Kiểm tra teardown/exit, ghi mốc shutdown để xác định nguyên nhân. Giữ watchdog hữu hạn; không đổi timeout hoặc hard-kill rồi gắn exit 0 để làm báo cáo đẹp.
- Nếu vẫn timeout, giữ nguyên sự thật và bàn giao bằng chứng cụ thể. Không kết luận headless không có frame loop khi log đã có natural frames.
- Dùng lại bằng chứng P08 50/50 đã có nếu production và pipeline dùng chung không đổi, ghi rõ REUSED REFERENCE. Chỉ chạy lại hồi quy liên quan khi thực sự thay đổi phần dùng chung.

6. CHUẨN BỊ CHO CHỦ DỰ ÁN KIỂM TRA TRỰC TIẾP

- Tạo MANUAL_P09A_CHECKLIST.md với scene/path, skill name/ID, các giá trị Rage/cast time/cooldown theo dữ liệu thực tế, thao tác chọn/cast/đổi target/pause/chuyển encounter hiện có, expected result và nơi lấy video/log. Không bịa nút hoặc tính năng chưa tồn tại.
- Xác minh projectile skill đã có đường thao tác từ game bình thường hay hiện chỉ nằm trong fixture RAM. Nếu chỉ có fixture, ghi rõ giới hạn; không tuyên bố gameplay UI đã được kiểm chứng và không tạo production skill/UI mới để che khoảng trống đó.
- Nếu cần fixture để người dùng quan sát, chuẩn bị session Unity có Game View hiển thị, dùng canonical skill entry point và fixture RAM, được bọc Save Guard từ trước launch đến sau khi đóng. Launcher ghi backup/restore/compare thực tế, chỉ quản lý process của nó và có timeout riêng đủ cho thao tác thủ công. Không dùng menu P09 đang bị chặn, không bỏ chặn menu để chạy với save thật chưa được bảo vệ.
- Đưa hướng dẫn lưu công việc và đóng Editor đang dùng project trước khi launcher mở session; tránh hai session đồng thời ghi cùng save. Launcher không tự đóng/kill Editor của người dùng. Không thay thế scene đang mở có thay đổi chưa lưu.
- Checklist kiểm tra: instant projectile; cast-time projectile; đổi target trong lúc đạn bay nếu game hỗ trợ; pause/resume; target/caster chết tự nhiên nếu tái hiện được; chuyển encounter/scene qua thao tác game hiện có; visual và projectile được dọn sạch. Không ép người dùng chỉnh HP/transform hoặc gọi hàm nội bộ để tạo kết quả.
- Trường hợp chưa thể thao tác phải ghi NOT AVAILABLE hoặc NOT EXECUTED, nêu đúng nguyên nhân và bằng chứng tự động thay thế nếu có. Phân biệt quan sát fixture với gameplay sản xuất. Không đánh dấu USER VERIFIED trước khi chủ dự án gửi kết quả.
- Bàn giao một lệnh mở session hoặc chuỗi thao tác chính xác, và một lệnh/vị trí xác minh recovery sau khi kết thúc. Nếu chưa có cách chạy an toàn, ghi MANUAL_NOT_READY và nguyên nhân thay vì yêu cầu người dùng thử menu bị chặn.

7. ĐÓNG GÓI VÀ PHẢN HỒI CUỐI

- Tạo E:\code\TLTD\review_package_p09a_closure.zip gồm source cần review, helper/launcher mới nếu có, correction diff so với snapshot E456C9F3..., raw logs mới, provenance trước–sau, REVIEW_RESPONSE.md, MANUAL_P09A_CHECKLIST.md và MANIFEST_SHA256.txt. Phân biệt bằng chứng mới với reference được dùng lại.
- Manifest liệt kê mọi file payload, không tự liệt kê chính manifest. Giải nén độc lập, kiểm tra toàn bộ checksum; báo riêng số payload và tổng file bao gồm manifest.
- Trả đường dẫn thật, dung lượng bytes, SHA256 ZIP, số file, kết quả manifest, bảng finding → sửa gì → test/log chứng minh, trạng thái từng gate và danh sách chưa thực thi.
- Đính kèm ZIP nếu giao diện hỗ trợ. Nếu không hỗ trợ, nói rõ ZIP đang ở máy local và chỉ dẫn người dùng mở thư mục rồi kéo file ZIP vào cuộc trò chuyện review; không coi một đường dẫn E:\... là file đã được gửi.
- Ghi trạng thái tự động đúng bằng chứng: PASS, FAIL hoặc SCENARIO_PASS_EXIT_TIMEOUT. Ghi trạng thái thủ công: READY_FOR_USER_TEST, MANUAL_NOT_READY hoặc NOT_EXECUTED. Không tự tuyên bố P09-A ACCEPTED/LOCKED, không ghi “không còn thiếu” khi còn timeout hoặc chưa có kết quả người dùng.

Mục tiêu của lần này là sửa đúng finding, cung cấp bằng chứng tương ứng và chuẩn bị lượt kiểm tra trực tiếp cho chủ dự án. Không mở rộng sang tính năng mới hoặc thêm vòng audit ngoài phạm vi trên.
