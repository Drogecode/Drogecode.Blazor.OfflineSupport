using OfflineSupport.Demo.Enums;

namespace OfflineSupport.Demo.Models;

public class LocalUserSettings(DarkLightMode darkLightMode)
{
  public DarkLightMode DarkLightMode { get; set; } = darkLightMode;
}
