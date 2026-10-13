using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellType
{
    Empty  = 0,  // Ô trống
    Player = 1,  // Vị trí xuất phát của cầu thủ
    NPC    = 2,  // Vị trí NPC
    Ball   = 3,  // Vị trí xuất phát của bóng
    Fence  = 4,  // Hàng rào
    Cone   = 5,  // Cọc tập
    Goal   = 6   // Khung thành
}
