﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tks.G1Track.Mobile.Shared.Common
{
  public interface IJobDirector
  {
    Task StartAsync();
    Task StopAsync();
  }

  public abstract class JobDirectorBase : IJobDirector
  {
    #region =====[ ctor ]==========================================================================================

    public JobDirectorBase(ILogger logger)
    {
      Logger = logger;
    }

    #endregion

    #region =====[ Private Properties ]============================================================================

    private CancellationTokenSource CancellationTokenSource { get; set; }
    private Task Task { get; set; }

    #endregion

    #region =====[ Protected Properties ]============================================================================

    protected ILogger Logger { get; set; }

    #endregion

    #region =====[ IJobDirector Methods ]============================================================================

    public Task StartAsync()
    {
      CancellationTokenSource = new CancellationTokenSource();
      Task = InternalStartAsync(CancellationTokenSource.Token);
      return Task;
    }

    public async Task StopAsync()
    {
      Logger.Verbose("Before CancellationTokenSource.Cancel()");
      Cancel();
      Logger.Verbose("After CancellationTokenSource.Cancel()");

      Logger.Verbose("Before awaiting Task");
      await Task;
      Logger.Verbose("After awaiting Task");
    }

    #endregion

    #region =====[ Protected Methods ]===============================================================================

    protected void Cancel()
    {
      CancellationTokenSource.Cancel();
    }

    protected async Task DoOperationAsync(IJobExceptionState jobExceptionState, Func<Task> operation)
    {
      await DoOperationAsync(jobExceptionState, async () =>
      {
        await operation().ConfigureAwait(false);
        return 0;
      }).ConfigureAwait(false);
    }

    protected async Task<TReturn> DoOperationAsync<TReturn>(
      IJobExceptionState jobExceptionState,
      Func<Task<TReturn>> operation)
    {
      try
      {
        return await operation().ConfigureAwait(false);
      }
      catch (OperationCanceledException operationCanceledException)
      {
        Logger.Verbose("Caught OperationCanceledException");
        if (!CancellationTokenSource.IsCancellationRequested)
        {
          Logger.Verbose("Before log OperationCanceledException");
          HandleException(jobExceptionState, operationCanceledException, Logger.Warning);
          Logger.Verbose("After log OperationCanceledException");
        }
      }
      catch (AggregateException aggregateException)
      {
        Logger.Verbose("Caught AggregateException");
        aggregateException.Handle((e) =>
        {
          if (e is OperationCanceledException)
          {
            if (!CancellationTokenSource.IsCancellationRequested)
            {
              Logger.Verbose("Before log aggregate's OperationCanceledException");
              HandleException(jobExceptionState, e, Logger.Warning);
              Logger.Verbose("After log aggregate's OperationCanceledException");
            }
            return true;
          }

          Logger.Verbose("Before log aggregate's other inner exception");
          HandleException(jobExceptionState, e, Logger.Error);
          Logger.Verbose("After log aggregate's other inner exception");
          return false;
        });
      }
      catch (Exception exception)
      {
        Logger.Verbose("Before log Exception");
        HandleException(jobExceptionState, exception, Logger.Error);
        Logger.Verbose("After log Exception");
      }

      return default(TReturn);
    }

    protected abstract Task InternalStartAsync(CancellationToken cancellationToken);

    protected async Task<JobResult> RunJobAsync(
      IDirectableJob job,
      IJobExceptionState jobExceptionState,
      CancellationToken cancellationToken)
    {
      var jobResult = await DoOperationAsync(jobExceptionState, async () =>
      {
        Logger.Verbose("Before IDirectableJob.Run()");
        var result = await job.Run(jobExceptionState, Logger, cancellationToken).ConfigureAwait(false);
        jobExceptionState.Clear();
        Logger.Verbose("After IDirectableJob.Run()");
        return result;
      }).ConfigureAwait(false);
      if (ShouldStop(cancellationToken, jobResult)) return JobResult.FalseResult;

      // Now let it handle any exception that occurred
      if (jobExceptionState.LastException != null)
      {
        if (!job.HandleException(jobExceptionState, Logger)) return JobResult.FalseResult;
      }

      return jobResult ?? JobResult.TrueResult;
    }

    protected bool ShouldStop(CancellationToken cancellationToken, JobResult jobResult = null)
    {
      if (cancellationToken.IsCancellationRequested) return true;
      if (jobResult != null && !jobResult.Continue) return true;
      return false;
    }

    #endregion

    #region =====[ Private Methods ]=================================================================================

    private void HandleException(IJobExceptionState jobExceptionState, Exception exception, Action<string> logAction)
    {
      // If no jobExceptionState, ignore this operation's exception
      if (jobExceptionState == JobExceptionState.None) return;

      jobExceptionState.LastException = exception;
      jobExceptionState.ExceptionCount++;

      string message = exception.ExpandMessage();
      if (message != jobExceptionState.LastExceptionMessage)
      {
        jobExceptionState.LastExceptionCount = 1;
        jobExceptionState.LastExceptionMessage = message;
        logAction(message);
      }
      else
      {
        jobExceptionState.LastExceptionCount++;
      }
    }

    #endregion
  }
}
