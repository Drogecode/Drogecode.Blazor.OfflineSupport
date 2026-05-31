using OfflineSupport.Demo.Enums;
using OfflineSupport.Demo.Models;

namespace OfflineSupport.Demo.Services.Interfaces
{
  public interface ILocalUserSettingService
  {
    Task<LocalUserSettings> GetLocalUserSettings();
    Task<LocalUserSettings> SetDarkLight(DarkLightMode darkLightMode);
    Task<LocalUserSettings> SetRememberMe(bool rememberMe);
  }
}
