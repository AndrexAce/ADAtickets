/*
 * ADAtickets is a simple, lightweight, open source ticketing system
 * interacting with your enterprise repositories on Azure DevOps
 * with a two-way synchronization.
 * Copyright (C) 2025  Andrea Lucchese
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
namespace ADAtickets.Web.Components.Utilities;

/// <summary>
///     Service used to manage ticket refresh requests across different components.
/// </summary>
internal sealed class TicketRefreshService : IDisposable
{
    private readonly PeriodicTimer _timer;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task? _timerTask;

    /// <summary>
    ///     Incapsulates the method to be called when a refresh is requested.
    /// </summary>
    public event Action? RefreshRequested;

    /// <summary>
    ///     Gets or sets the auto-refresh interval in seconds. Set to 0 to disable auto-refresh.
    /// </summary>
    public int AutoRefreshIntervalSeconds { get; set; } = 5;

    /// <summary>
    ///     Gets whether auto-refresh is currently enabled.
    /// </summary>
    public bool IsAutoRefreshEnabled => AutoRefreshIntervalSeconds > 0;

    public TicketRefreshService()
    {
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(AutoRefreshIntervalSeconds));
    }

    /// <summary>
    ///     Request the list of tickets to be refreshed with all the filters and sortings applied.
    /// </summary>
    public void RequestRefresh()
    {
        RefreshRequested?.Invoke();
    }

    /// <summary>
    ///     Starts the auto-refresh timer.
    /// </summary>
    public void StartAutoRefresh()
    {
        if (IsAutoRefreshEnabled && _timerTask is null)
        {
            _timer.Period = TimeSpan.FromSeconds(AutoRefreshIntervalSeconds);
            _timerTask = DoWorkAsync();
        }
    }

    /// <summary>
    ///     Stops the auto-refresh timer.
    /// </summary>
    public void StopAutoRefresh()
    {
        _cancellationTokenSource.Cancel();
        _timerTask = null;
    }

    /// <summary>
    ///     Updates the auto-refresh interval and restarts the timer if running.
    /// </summary>
    /// <param name="intervalSeconds">New interval in seconds. Set to 0 to disable.</param>
    public void UpdateRefreshInterval(int intervalSeconds)
    {
        var wasRunning = _timerTask is not null;

        if (wasRunning)
        {
            StopAutoRefresh();
        }

        AutoRefreshIntervalSeconds = intervalSeconds;

        if (wasRunning && IsAutoRefreshEnabled)
        {
            StartAutoRefresh();
        }
    }

    private async Task DoWorkAsync()
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(_cancellationTokenSource.Token))
            {
                RefreshRequested?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
    }

    public void Dispose()
    {
        StopAutoRefresh();
        _timer.Dispose();
        _cancellationTokenSource.Dispose();
    }
}