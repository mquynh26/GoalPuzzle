using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellType
{
    Empty  = 0,  // Ô trống: cầu thủ và bóng đi/lăn qua được
    Player = 1,  // Vị trí xuất phát của cầu thủ
    NPC    = 2,  // Vị trí NPC (hướng ban đầu mặc định Up)
    Ball   = 3,  // Vị trí xuất phát của bóng
    Fence  = 4,  // Hàng rào: vật cản cố định
    Cone   = 5,  // Cọc tập: đẩy được, bị bóng đập thì biến mất
    Goal   = 6   // Khung thành: bóng vào là thắng
}
