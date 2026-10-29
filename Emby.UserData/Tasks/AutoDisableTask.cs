using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Tasks;
using Emby.UserData.Services;

namespace Emby.UserData.Tasks {
 public sealed class AutoDisableTask : IScheduledTask {
  private readonly IUserManager users;
  public AutoDisableTask(IUserManager users) { this.users=users; }
  public string Name => "Evaluate user deactivation dates";
  public string Key => "EmbyUserDataAutoDisable";
  public string Description => "Disables or re-enables users according to their configured date field.";
  public string Category => "User Data";
  public Task Execute(CancellationToken cancellationToken, IProgress<double> progress) { new UserDataStore().EvaluateAutoDisable(users); progress.Report(100); return Task.CompletedTask; }
  public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() { return new[] { new TaskTriggerInfo { Type=TaskTriggerInfo.TriggerDaily, TimeOfDayTicks=TimeSpan.Zero.Ticks } }; }
 }
 public sealed class AutoDisableEntryPoint : IServerEntryPoint {
  private readonly IUserManager users;
  public AutoDisableEntryPoint(IUserManager users) { this.users=users; }
  public void Run() { Plugin.Instance.ClearRestartNoticeIfUpdated(); new UserDataStore().EvaluateAutoDisable(users); }
  public void Dispose() { }
 }
}
namespace Emby.UserData.Tasks {
 public sealed class AutoUpdateTask : IScheduledTask {
  public string Name => "Check User Data plugin updates";
  public string Key => "EmbyUserDataAutoUpdate";
  public string Description => "Checks GitHub daily and installs a newer User Data plugin version when automatic updates are enabled.";
  public string Category => "User Data";
  public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress) { await new UpdateService().CheckAndUpdateAsync(); progress.Report(100); }
  public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() { return new[] { new TaskTriggerInfo { Type=TaskTriggerInfo.TriggerDaily, TimeOfDayTicks=TimeSpan.Zero.Ticks } }; }
 }
}
