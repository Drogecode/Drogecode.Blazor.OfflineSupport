using ExpireStorage.Demo.Enums;

namespace ExpireStorage.Demo.Models;

public class LocalUserSettings(DarkLightMode darkLightMode)
{
  public DarkLightMode DarkLightMode { get; set; } = darkLightMode;
}
