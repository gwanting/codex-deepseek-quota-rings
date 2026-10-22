using System.Drawing;
namespace DeepSeekBalance {
static class BalanceAlert {
 public static Color ColorAt(decimal balance){return balance<5m?Color.FromArgb(244,77,86):balance<10m?Color.FromArgb(255,203,65):balance<=20m?Color.FromArgb(66,170,255):Color.FromArgb(45,215,139);}
 public static string Description(decimal balance){return balance<5m?"低于5元：红色":balance<10m?"5元至不足10元：黄色":balance<=20m?"10至20元：蓝色":"超过20元：绿色";}
}
}
