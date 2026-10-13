# TLTD — Báo cáo kiểm tra code game, chỉ đọc

Ngày: **11/10/2026, UTC+07**. Đối tượng: working tree thực tế tại `E:\code\TLTD`.

**Kết luận:** có các điểm đáng sửa. Ưu tiên cao nhất vẫn là lỗi bàn giao loot sau channel đã biết nhưng chưa được tích hợp vào code game tại E:. Hai điểm mới có đường gọi cụ thể là lựa chọn item cũ trong bảng `[DEV]` vượt qua kiểm tra ownership và nâng Cấp Rơi không tự hoàn tất khi hết giờ. Builder dữ liệu cũng tiếp tục tạo subasset effect dư khi chạy lại. Nền tảng status còn một cửa sổ phụ thuộc thứ tự Update cần xử lý khi có content sử dụng.

Đây là **kết luận từ source và dữ liệu tĩnh**, không phải kết quả tái hiện trên game đang chạy. Tôi chưa sửa code game, chưa chạy Unity/compiler/test, chưa chạy wrapper/SaveGuard/recovery và chưa đọc hoặc sửa Registry/save của người chơi. Báo cáo này không phải prompt triển khai; các hướng sửa bên dưới để Owner phân tích và chọn ưu tiên trước.

## 1. Phạm vi và độ tin cậy

- Đọc sâu các luồng battle/loot, inventory/equipment, progression/resource, combat/status, modal/UI liên quan và một số builder/test source. Inventory thư mục `Assets/_Game` có 149 file C#; **không tuyên bố đã đọc toàn bộ 149 file từng dòng**.
- Đối chiếu những nhận định liên quan với contract và continuity đang có. Chỉ đọc test source để nhận diện phạm vi kiểm tra; không coi chúng là kết quả test vừa chạy.
- `P1`: có thể chặn vòng chơi trong điều kiện nêu rõ. `P2`: sai trạng thái/contract hoặc cần cải tiến có ảnh hưởng cụ thể. `P3`: hardening hoặc tối ưu có điều kiện, ưu tiên thấp hơn. Đây là mức ưu tiên kỹ thuật đề xuất, chưa phải quyết định sản phẩm.
- Không đo FPS, bộ nhớ, thời gian import/build, trải nghiệm điều khiển hoặc tình trạng save thực tế. Không dùng kích thước asset để suy ra các số đo đó.

| Mã | Ưu tiên | Nhận định | Phân loại |
|---|---|---|---|
| R01 | P1 | Loot sau finishing channel bỏ qua cờ mở quyết định | Lỗi đã biết; còn trong production source |
| R02 | P2 | `[DEV]` giữ item đã Tách; Equip bỏ qua kết quả kiểm tra ownership | Điểm mới, có đường gọi cụ thể; giới hạn prototype/debug |
| R03 | P2 | Cấp Rơi hết giờ vẫn chờ nút HOÀN THÀNH | Lệch A9/D22 đang khóa |
| R04 | P2 | Chạy lại data builder thêm effect subasset mới vào asset cũ | Triệu chứng asset phình đã biết; truy được cơ chế tạo dư |
| R05 | P2, có điều kiện | Apply effect hết hạn trước lượt dọn có thể hồi sinh stack cũ | Lỗi nền tảng phụ thuộc content và thứ tự gọi |

## 2. Các điểm cần phân tích trước khi sửa

### R01 — Luồng loot sau finishing channel chưa dùng đường trình bày chuẩn

**Bằng chứng:** đường chuẩn [TryPresentNextQueuedLoot](E:/code/TLTD/Assets/_Game/Core/BattleManager.cs:874) kiểm tra modal, lấy item khỏi queue và đặt `_isLootDecisionOpen = true` ở dòng 887. Trong khi đó [WaitForFinishingExecutionsThenProceed](E:/code/TLTD/Assets/_Game/Core/BattleManager.cs:1236), sau khi chờ cast kết thúc, lấy item trực tiếp ở dòng 1291 rồi phát yêu cầu loot ở dòng 1297, thiếu bước mở cờ và bỏ qua điều kiện modal của đường chuẩn.

[CompleteLootDecisionAndResume](E:/code/TLTD/Assets/_Game/Core/BattleManager.cs:1048) trả `false` trong PlayMode nếu cờ chưa mở. Cả [Equip](E:/code/TLTD/Assets/_Game/UI/LootDecisionUI.cs:500) và [Tách](E:/code/TLTD/Assets/_Game/UI/LootDecisionUI.cs:542) đều gọi API này và giữ modal nếu bị từ chối.

**Điều kiện và ảnh hưởng:** quái cuối chết khi còn channel được giữ để kết thúc, có loot trong queue; sau channel, popup được yêu cầu nhưng quyết định đầu tiên có thể bị từ chối, chặn vòng loot/encounter. Đây là suy luận trực tiếp từ control flow; chưa tái hiện runtime trong lượt audit này.

**Trạng thái lịch sử:** đây là vấn đề của `F-LOOT-CHANNEL-HANDOFF-LAB-01`, không phải phát hiện mới và không phải lý do mở lại toàn bộ P08/P09. `BattleManager.cs` tại E: hiện là **58.100 bytes**, SHA-256 `3A021DE3B78D985A7B51715CD4743F44202A229F9EEBDFEBECF02B0D6DDF6448`; khác bản lab đóng băng **58.723 bytes**, SHA-256 `5B477D0F0DE108A816E1AC1834EA4F04967DEE657E52E86553B19943F7E422A6`. Không coi bản lab đã tự động trở thành bản production. Hồ sơ harness còn giới hạn review/evidence; báo cáo này không xác nhận một phiên chạy mới.

**Hướng xử lý đề xuất:** hoàn tất phần review/evidence còn thiếu của nhánh lab hiện có, rồi mới xem xét chuyển bản sửa hẹp dùng chung đường trình bày/defer chuẩn. Không tích hợp hoặc chạy harness trong lượt này; Phase B vẫn STOPPED.

### R02 — Item đã Tách vẫn còn được chọn trong bảng `[DEV]`

[EquipmentDropDebugUI.SelectItem](E:/code/TLTD/Assets/_Game/UI/EquipmentDropDebugUI.cs:248) giữ tham chiếu `selectedItem`; drop mới tự chọn item ở dòng 373. [RefreshUI](E:/code/TLTD/Assets/_Game/UI/EquipmentDropDebugUI.cs:549) không loại lựa chọn khi item rời inventory, còn nút Equip chỉ xét có lựa chọn và chưa đánh dấu equipped ở [dòng 631](E:/code/TLTD/Assets/_Game/UI/EquipmentDropDebugUI.cs:631).

Luồng Tách hợp lệ gọi [ResourceManager.DismantleEquipment](E:/code/TLTD/Assets/_Game/Progression/ResourceManager.cs:132), bỏ item khỏi inventory rồi cộng Gold. UI loot chỉ xóa lựa chọn riêng của nó. Nếu chưa có drop khác thay lựa chọn debug, [EquipSelected](E:/code/TLTD/Assets/_Game/UI/EquipmentDropDebugUI.cs:266) rơi vào nhánh gọi thẳng `EquipmentManager.Equip(selectedItem)` ở dòng 284.

Tại [EquipmentManager.EquipToSlot](E:/code/TLTD/Assets/_Game/Equipment/EquipmentManager.cs:63), được gọi qua Equip, kết quả `Inventory.RemoveItem(item)` ở dòng 89 bị bỏ qua; hàm vẫn gán item vào slot ở dòng 93 và trả thành công. [Inventory.RemoveItem](E:/code/TLTD/Assets/_Game/Inventory/Inventory.cs:64) có thể trả `false` khi item không còn thuộc inventory.

**Chuỗi kích hoạt cụ thể từ source:** item rơi → debug giữ lựa chọn → Tách qua popup, nhận Gold → trước drop tiếp theo, dùng `[DEV]` EQUIP → object đã Tách có thể được trang bị lại. Cùng tham chiếu cũ còn đi vào [OnUpgradeClicked](E:/code/TLTD/Assets/_Game/UI/EquipmentDropDebugUI.cs:321); [EquipmentUpgradeService](E:/code/TLTD/Assets/_Game/Equipment/EquipmentUpgradeService.cs:127) chưa xác minh ownership trước khi trừ tài nguyên và nâng item.

**Giới hạn:** component/nút debug có wiring trong Prototype01, nhưng chưa bấm thử để xác nhận trạng thái hiển thị và thao tác thực tế. Không gọi đây là exploit của bản phát hành; không kết luận duplicate Tách đã hỏng. Contract Tách chỉ trả Gold/A10 và closeout cũ được giữ nguyên.

**Hướng sửa đề xuất:** xác minh ownership trong transaction Equip **trước khi tháo trang bị cũ**, trả thất bại mà không đổi trạng thái nếu item không hợp lệ; UI xóa lựa chọn không còn sở hữu. Upgrade cần chấp nhận đúng item trong inventory hoặc item đang được trang bị hợp lệ. Không đổi công thức Gold hoặc thêm thưởng Material. Kiểm tra hẹp cần thiết sau khi được chọn sửa: Tách → lựa chọn debug cũ → Equip/Upgrade bị từ chối, trong khi item hợp lệ vẫn hoạt động.

### R03 — Nâng Cấp Rơi hết giờ không tự hoàn tất theo A9/D22

[LootTierProgressionManager.Update](E:/code/TLTD/Assets/_Game/Progression/LootTierProgressionManager.cs:79) có nhánh kiểm tra hết giờ nhưng thân nhánh chỉ có comment. [StartUpgrade](E:/code/TLTD/Assets/_Game/Progression/LootTierProgressionManager.cs:199) trừ chi phí, lưu trạng thái và thời điểm bắt đầu với nâng cấp có duration dương. [LoadState](E:/code/TLTD/Assets/_Game/Progression/LootTierProgressionManager.cs:356) khôi phục các trường nhưng không hoàn tất upgrade đã hết hạn.

Chuyển tier thật nằm ở [CompleteUpgradeInternal](E:/code/TLTD/Assets/_Game/Progression/LootTierProgressionManager.cs:271). Trong các caller runtime được tìm, completion được gọi từ [nút UI](E:/code/TLTD/Assets/_Game/UI/LootTierProgressionUI.cs:582); khi timer bằng 0, UI hiện [“HOÀN THÀNH”](E:/code/TLTD/Assets/_Game/UI/LootTierProgressionUI.cs:408). `LootTier_01.asset` có duration 60 giây; manager được gắn trong scene hiện tại, nên đây không chỉ là API chưa sử dụng.

**Đối chiếu contract:** [A9 đang khóa](E:/code/TLTD/PROJECT_MEMORY/D1_D23_AMENDMENTS_LOCKED.md:174) yêu cầu tự hoàn tất khi tới FinishTime, lên đúng một cấp rồi IDLE; dòng 195 nêu không có Claim button. [D22](E:/code/TLTD/PROJECT_MEMORY/D22_LOCKED.md:27) cũng có yêu cầu thời gian khi mở lại. Chưa tìm thấy quyết định Owner thay thế các yêu cầu này trong tài liệu đã đối chiếu.

**Ảnh hưởng theo source:** upgrade đã trả phí vẫn giữ tier cũ và trạng thái upgrading sau hết giờ/mở lại, đến khi người chơi bấm nút; drop tiếp tục dùng tier hiện tại cũ. Chưa quan sát lỗi này trong Unity, chưa kết luận mất Gold/save. Test source [P05.9-13](E:/code/TLTD/Assets/_Game/Editor/Prototype01PlayTestRunner.cs:12295) gọi `CompleteUpgrade()` tường minh, nên không chứng minh auto-complete.

**Hướng sửa đề xuất:** manager hòa giải thời gian khi load và trong runtime, hoàn tất đúng một lần kể cả panel đóng; UI phản ánh trạng thái đã hoàn tất, không cần Claim. Giữ nguyên chi phí và không tự nối thêm cấp kế tiếp. Kiểm tra hẹp tương lai cần gồm hết giờ khi panel đóng và mở lại sau deadline. Không yêu cầu Owner chơi thử ngay trong lượt báo cáo này.

### R04 — Builder dữ liệu thêm subasset effect khi chạy lại

[GetOrCreateSkillDef](E:/code/TLTD/Assets/_Game/Editor/Prototype01SceneBuilder.cs:2581) nạp skill asset đã có ở dòng 2595, nhưng mỗi lần gọi vẫn tạo `DamageEffectDefinitionSO` mới ở dòng 2605, `AddObjectToAsset` ở dòng 2608 rồi thay danh sách effect ở dòng 2609. [SkillDefinitionSO.SetEffects](E:/code/TLTD/Assets/_Game/Data/SkillDefinitionSO.cs:177) chỉ thay danh sách tham chiếu, không gỡ subasset cũ.

[GetOrCreateMindMethodDatabase](E:/code/TLTD/Assets/_Game/Editor/Prototype01SceneBuilder.cs:2642) gọi các hàm tạo skill kể cả khi database tồn tại. Data builder chạy cho cả Data và Resources; [BuildPrototype01Scene](E:/code/TLTD/Assets/_Game/Editor/Prototype01SceneBuilder.cs:305) cũng gọi data build trước khi dựng scene. Tôi chỉ đọc các hàm này, không chạy builder.

**Đo dữ liệu tĩnh ở bốn mẫu:** đếm block có `m_EditorClassIdentifier: Assembly-CSharp::WuxiaGame.Combat.DamageEffectDefinitionSO`.

| Skill asset, dưới `Assets/_Game/` | Bytes | Block DamageEffect |
|---|---:|---:|
| `Data/Skills/skill_taiji_3_b.asset` | 8.113.773 | 14.555 |
| `Resources/Data/Skills/skill_taiji_3_b.asset` | 8.114.482 | 14.556 |
| `Data/Skills/skill_nyin_3_b.asset` | 8.099.761 | 14.556 |
| `Resources/Data/Skills/skill_nyin_3_b.asset` | 8.099.855 | 14.556 |

Main skill ở mẫu taiji Data hiện chỉ có một tham chiếu effect trong danh sách. Cơ chế builder giải thích vì sao chạy lại có thể tiếp tục thêm subasset không còn được danh sách đó dùng. Tuy nhiên **chưa truy provenance từng lần tạo**, không quy toàn bộ 14 nghìn block cho một người hoặc một lần chạy. Hiện tượng asset phình đã được ghi nhận trước đây; điểm bổ sung là đường tạo dư cụ thể.

**Ảnh hưởng chắc chắn:** payload asset dư và builder không đạt tính chạy lặp mà không sinh thêm dữ liệu. Không suy ra 14 nghìn lần damage, FPS giảm hoặc thời gian import cụ thể.

**Hướng sửa đề xuất:** làm builder tái sử dụng subasset mà nó sở hữu; tách việc dọn asset lịch sử thành migration riêng có đối chiếu GUID/fileID và tham chiếu. Không xóa subasset hàng loạt, không chạy lại scene builder trên working tree hiện tại để thử, không bật thêm skill assets.

### R05 — Effect đã hết hạn có thể giữ lại stack cũ nếu áp lại trước lượt dọn

Ba API [ApplyDebuff](E:/code/TLTD/Assets/_Game/Combat/EntityStatusController.cs:331), [ApplyBuff](E:/code/TLTD/Assets/_Game/Combat/EntityStatusController.cs:531) và [ApplyStatus](E:/code/TLTD/Assets/_Game/Combat/EntityStatusController.cs:609) chọn nhánh effect đang có dựa trên dictionary, không kiểm tra expiry trước khi Refresh/AddStack. Trong khi đó `HasBuff/HasDebuff/HasStatus` đã coi instance hết hạn là không active. Dọn dictionary xảy ra sau qua [Update → Tick](E:/code/TLTD/Assets/_Game/Combat/EntityStatusController.cs:1320).

**Ví dụ điều kiện:** effect timed có ba stack, vừa vượt thời điểm hết hạn; cùng ID được áp lại trước Tick dọn. [AddStack](E:/code/TLTD/Assets/_Game/Combat/RuntimeBuffInstance.cs:94) giữ/increment stack cũ rồi refresh duration. Nếu Tick dọn chạy trước, cùng một lần Apply lại khởi tạo stack một. Do đó trạng thái phụ thuộc thứ tự gọi trong frame; lần expiry cũ cũng có thể không phát notification như luồng dọn bình thường.

**Giới hạn quan trọng:** chưa chứng minh content đang được chọn trong game kích hoạt timed stackable buff/debuff/DoT này, chưa tái hiện Unity. Đây là lỗi nền tảng có điều kiện, không phải khẳng định người chơi hiện tại bị sai buff. Các test source được đọc có kiểm tra cap, refresh khi còn hạn và gọi Tick trước khi xét expiry; chưa thấy chúng bao phủ thứ tự expiry → reapply → cleanup này.

**Hướng sửa đề xuất:** xử lý instance hết hạn trước khi áp stacking policy, phát expiry đúng một lần và tạo instance mới theo cùng quy tắc ở ba họ effect. Giữ nguyên policy của effect còn hạn. Kiểm tra hẹp bằng cùng timestamp cho hai thứ tự Apply/Tick; chưa cần mở thêm content hoặc toàn bộ bộ test đã nghiệm thu.

## 3. Các điểm đã biết hoặc chỉ nên để backlog

**K01 — Restore progression khi khởi động chưa có đường gọi rõ.** [ProgressionManager.Start](E:/code/TLTD/Assets/_Game/Progression/ProgressionManager.cs:100) gọi Initialize với default level 1/EXP 0/title 0. [LoadState](E:/code/TLTD/Assets/_Game/Progression/ProgressionManager.cs:382) tồn tại nhưng chưa tìm thấy caller tự động trong runtime/scene wiring đã tìm. [GameBootstrap.Start](E:/code/TLTD/Assets/_Game/Core/GameBootstrap.cs:27) không tạo restore order trước combat. Inventory/loadout cũng đang có trạng thái RAM chưa thấy cơ chế persistence đầy đủ trong phạm vi đọc.

Đây là khoảng trống continuity đã biết, không báo lại thành lỗi mới, không khẳng định save của Owner đã bị reset/mất. Chỉ đọc source, không mở save thật. Cần chốt phạm vi persistence và thứ tự restore/init/event trước khi coi vòng progression qua nhiều phiên chơi là hoàn chỉnh; không tự thêm migration hoặc Stage Gate feature. Test source mang tên persistence qua encounters dùng cùng fixture chưa đủ chứng minh cold start.

**M01 — Modal cần cứng hơn khi view bị rút hoặc callback ném exception.** [UnregisterModalView](E:/code/TLTD/Assets/_Game/UI/Modal/ModalCoordinator.cs:153) bỏ view nhưng không giải phóng active request. [Complete/Dismiss](E:/code/TLTD/Assets/_Game/UI/Modal/ModalCoordinator.cs:378) gọi callback trước khi đưa modal kế tiếp lên; callback lỗi có thể bỏ qua bước đó. Tuy vậy scene hiện tại gắn coordinator và các view trên cùng Canvas; ẩn panel thông thường không đồng nghĩa disable component view, và chưa tìm thấy runtime caller đang gắn callback completion/dismissal gây lỗi. Đây là hardening có điều kiện, chưa có đường chơi bình thường được chứng minh mắc kẹt vì hai nhánh này.

**M02 — Ownership của effect fallback tạm thời.** [EffectResolver](E:/code/TLTD/Assets/_Game/Combat/EffectResolver.cs:87) tạo SO mới mỗi lần resolve skill legacy thiếu explicit effect; [DoT fallback](E:/code/TLTD/Assets/_Game/Combat/DamageOverTimeEffectDefinitionSO.cs:88) cũng tạo definition khi thiếu StatusDefinition. Chưa thấy ownership/cache/disposal rõ trong phạm vi runtime đã đọc. Chưa xác nhận content hiện tại đi qua fallback hoặc đo memory; chỉ đề xuất P3, không gọi là memory leak đã tái hiện.

**Chưa đủ căn cứ kết luận lỗi:** DoT/timer có cần dừng khi từng loại modal mở hay không. Code status tick và code battle pause khác nhau chưa đủ để quyết định contract thời gian. Không lấy câu hỏi này làm lý do đổi toàn bộ pause policy hoặc mở skill chưa được duyệt.

## 4. Thứ tự xử lý đề xuất — chưa triển khai

1. Giữ R01 trong luồng lab/review đang có; chốt evidence hẹp trước khi xét tích hợp production.
2. Với source game tại E:, xem xét R02 và R03 trước: R02 bảo vệ ownership transaction; R03 đưa behavior Cấp Rơi về contract A9/D22.
3. Chặn builder tiếp tục sinh dữ liệu dư trước; migration dọn asset cũ là việc riêng, cần đánh giá tham chiếu và bản sao bảo toàn.
4. R05 kiểm tra/sửa trong phạm vi foundation sau khi chọn ưu tiên; không gắn việc này với bật hàng loạt skill content.
5. K01 cần một quyết định scope persistence riêng; M01/M02 để backlog nếu chưa có trigger thực tế hoặc nhu cầu liên quan.

Đây là thứ tự kỹ thuật để Owner phân tích, **không phải roadmap gameplay đã phê duyệt**. Không có prompt sửa code gửi Antigravity, không có yêu cầu Owner chơi lại P08/P09 hoặc tạo ZIP nghiệm thu. Chưa có lý do trong audit này để sửa working tree hoặc save.

## 5. Bảo toàn và giới hạn nghiệm thu

Kiểm tra trước–sau bằng Git read-only và SHA-256 trên phạm vi đo:

| Hạng mục | Trước | Sau |
|---|---|---|
| HEAD | `7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425` | Không đổi |
| Tracked modified | 3 file Markdown đã có | Cùng danh sách/trạng thái; byte/hash không đổi |
| Untracked | 530 mục | Cùng danh sách/trạng thái |
| File đo bytes/SHA-256 | 784 | Cùng tập đường dẫn, bytes và hash |

Phạm vi hash: `Assets/_Game`, `Assets/Scenes`, `ProjectSettings`, `Packages` và ba Markdown dirty đã có. Không suy rộng thành bằng chứng byte-for-byte của toàn bộ 530 untracked, `Library/Temp` hoặc save ngoài phạm vi. Snapshot: [trước](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/game_source_audit_before_20261011.json), [sau](C:/Users/conca/Documents/Codex/2026-10-05/b-n-ti-p-qu-n/work/game_source_audit_after_20261011.json). Chỉ viết tài liệu/snapshot audit trong workspace Codex; không commit/push.

P08/P09-A/P09-B giữ ACCEPTED/LOCKED đúng phạm vi và các ngoại lệ/evidence limits cũ. Không chuyển Gate 2 P08 135/137 thành 137/137; không chuyển E2 P09-A timeout thành exit 0. Giữ nguyên lịch sử recovery, phân biệt metadata/snapshot với raw Restore/execution và Compare/preflight; không nâng trạng thái raw evidence từ audit source này. Tách chỉ trả Gold vẫn là contract khóa. Phase B chưa được mở.

**Điểm dừng:** báo cáo đã sẵn để Owner phân tích. Code E:, Unity và save không bị thay đổi bởi lượt audit này; mọi đề xuất sửa còn là đề xuất.
