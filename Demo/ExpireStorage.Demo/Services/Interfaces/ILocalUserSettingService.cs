using ExpireStorage.Demo.Enums;
using ExpireStorage.Demo.Models;

namespace ExpireStorage.Demo.Services.Interfaces
{
  public interface ILocalUserSettingService
  {
    Task<LocalUserSettings> GetLocalUserSettings();
    Task<LocalUserSettings> SetDarkLight(DarkLightMode darkLightMode);
    Task<LocalUserSettings> SetRememberMe(bool rememberMe);
  }
}
