/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Threading;


namespace Neo4Net.Test
{
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static System.currentTimeMillis;

   /// <summary>
   /// Executes <seealso cref="WorkerCommand"/>s in another thread. Very useful for writing
   /// tests which handles two simultaneous transactions and interleave them,
   /// f.ex for testing locking and data visibility.
   /// </summary>
   /// @param <T> type of state
   /// @author Mattias Persson </param>
   public class OtherThreadExecutor<T> : ThreadFactory, System.IDisposable
   {
      private bool InstanceFieldsInitialized = false;

      private void InitializeInstanceFields()
      {
         _commandExecutor = newSingleThreadExecutor(this);
      }

      private ExecutorService _commandExecutor;

      
      protected internal readonly T _stateConflict;

      private volatile Thread _thread;
      private volatile ExecutionState _executionState;
      private readonly string _name;
      private readonly long _timeout;

      private sealed class AnyThreadState : System.Predicate<Thread>
      {
         internal readonly ISet<Thread.State> PossibleStates;
         internal readonly ISet<Thread.State> SeenStates = new HashSet<Thread.State>();

         internal AnyThreadState(params Thread.State[] possibleStates)
         {
            this.PossibleStates = new HashSet<Thread.State>(asList(possibleStates));
         }

         public override bool Test(Thread thread)
         {
            Thread.State threadState = thread.State;
            SeenStates.Add(threadState);
            return PossibleStates.Contains(threadState);
         }

         public override string ToString()
         {
            return "Any of thread states " + PossibleStates + ", but saw " + SeenStates;
         }
      }

      public static System.Predicate<Thread> AnyThreadState(params Thread.State[] possibleStates)
      {
         return new AnyThreadState(possibleStates);
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      //ORIGINAL LINE: public System.Predicate<Thread> orExecutionCompleted(final System.Predicate<Thread> actual)
      public virtual System.Predicate<Thread> OrExecutionCompleted(System.Predicate<Thread> actual)
      {
         return new PredicateAnonymousInnerClass(this, actual);
      }

      private class PredicateAnonymousInnerClass : System.Predicate<Thread>
      {
         private readonly OtherThreadExecutor<T> _outerInstance;

         private System.Predicate<Thread> _actual;

         public PredicateAnonymousInnerClass(OtherThreadExecutor<T> outerInstance, System.Predicate<Thread> actual)
         {
            this.outerInstance = outerInstance;
            this._actual = actual;
         }

         public override bool test(Thread thread)
         {
            return _actual(thread) || _outerInstance.executionState == ExecutionState.Executed;
         }

         public override string ToString()
         {
            return "(" + _actual.ToString() + ") or execution completed.";
         }
      }

      private enum ExecutionState
      {
         RequestedExecution,
         Executing,
         Executed
      }

      public OtherThreadExecutor(string name, T initialState) : this(name, 10, SECONDS, initialState)
      {
         if (!InstanceFieldsInitialized)
         {
            InitializeInstanceFields();
            InstanceFieldsInitialized = true;
         }
      }

      public OtherThreadExecutor(string name, long timeout, TimeUnit unit, T initialState)
      {
         if (!InstanceFieldsInitialized)
         {
            InitializeInstanceFields();
            InstanceFieldsInitialized = true;
         }
         this._name = name;
         this._stateConflict = initialState;
         this._timeout = MILLISECONDS.convert(timeout, unit);
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      //ORIGINAL LINE: public <R> java.util.concurrent.Future<R> executeDontWait(final WorkerCommand<T, R> cmd)
      public virtual Future<R> ExecuteDontWait<R>(WorkerCommand<T, R> cmd)
      {
         _executionState = ExecutionState.RequestedExecution;
         return _commandExecutor.submit(() =>
         {
            _executionState = ExecutionState.Executing;
            try
            {
               return cmd.DoWork(_stateConflict);
            }
            finally
            {
               _executionState = ExecutionState.Executed;
            }
         });
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public <R> R execute(WorkerCommand<T, R> cmd) throws Exception
      public virtual R Execute<R>(WorkerCommand<T, R> cmd)
      {
         return ExecuteDontWait(cmd).get();
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public <R> R execute(WorkerCommand<T, R> cmd, long timeout, java.util.concurrent.TimeUnit unit) throws Exception
      public virtual R Execute<R>(WorkerCommand<T, R> cmd, long timeout, TimeUnit unit)
      {
         Future<R> future = ExecuteDontWait(cmd);
         bool success = false;
         try
         {
            AwaitStartExecuting();
            R result = future.get(timeout, unit);
            success = true;
            return result;
         }
         finally
         {
            if (!success)
            {
               future.cancel(true);
            }
         }
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public void awaitStartExecuting() throws InterruptedException
      public virtual void AwaitStartExecuting()
      {
         while (_executionState == ExecutionState.RequestedExecution)
         {
            Thread.Sleep(10);
         }
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public <R> R awaitFuture(java.util.concurrent.Future<R> future) throws InterruptedException, java.util.concurrent.ExecutionException, java.util.concurrent.TimeoutException
      public virtual R AwaitFuture<R>(Future<R> future)
      {
         return future.get(_timeout, MILLISECONDS);
      }

      public interface WorkerCommand<T, R>
      {
         //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
         //ORIGINAL LINE: R doWork(T state) throws Exception;
         R DoWork(T state);
      }

      public static WorkerCommand<T, R> Command<T, R>(Race.IThrowingRunnable runnable)
      {
         return StateConflict =>
         {
            try
            {
               runnable.Run();
               return null;
            }
            catch (Exception e)
            {
               throw e;
            }
            catch (Exception throwable)
            {
               throw new Exception(throwable);
            }
         };
      }

      public override Thread NewThread(ThreadStart r)
      {
         //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
         Thread thread = new Thread(r, this.GetType().FullName + ":" + name() =>

           {
             try
         {
            base.run();
         }
         finally
         {
            OtherThreadExecutor.this._thread = null;
         }
      });

			  this._thread = thread;
			  return thread;
		 }

   public override string ToString()
   {
      Thread thread = this._thread;
      return format("%s[%s,state=%s]", this.GetType().Name, _name, thread == null ? "dead" : thread.State);
   }


   public virtual WaitDetails WaitUntilWaiting()
   {
      return WaitUntilWaiting(details => true);
   }

   //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
   //ORIGINAL LINE: public WaitDetails waitUntilBlocked() throws java.util.concurrent.TimeoutException
   public virtual WaitDetails WaitUntilBlocked()
   {
      return WaitUntilBlocked(details => true);
   }

   //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
   //ORIGINAL LINE: public WaitDetails waitUntilWaiting(System.Predicate<WaitDetails> correctWait) throws java.util.concurrent.TimeoutException
   public virtual WaitDetails WaitUntilWaiting(System.Predicate<WaitDetails> correctWait)
   {
      return WaitUntilThreadState(correctWait, Thread.State.WAITING, Thread.State.TIMED_WAITING);
   }

   //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
   //ORIGINAL LINE: public WaitDetails waitUntilBlocked(System.Predicate<WaitDetails> correctWait) throws java.util.concurrent.TimeoutException
   public virtual WaitDetails WaitUntilBlocked(System.Predicate<WaitDetails> correctWait)
   {
      return WaitUntilThreadState(correctWait, Thread.State.BLOCKED);
   }

   //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
   //ORIGINAL LINE: public WaitDetails waitUntilThreadState(final Thread.State... possibleStates) throws java.util.concurrent.TimeoutException
   //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
   public virtual WaitDetails WaitUntilThreadState(params Thread.State[] possibleStates)
   {
      return WaitUntilThreadState(details => true, possibleStates);
   }

   //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
   //ORIGINAL LINE: public WaitDetails waitUntilThreadState(System.Predicate<WaitDetails> correctWait, final Thread.State... possibleStates) throws java.util.concurrent.TimeoutException
   //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
   public virtual WaitDetails WaitUntilThreadState(System.Predicate<WaitDetails> correctWait, params Thread.State[] possibleStates)
   {
      long end = currentTimeMillis() + _timeout;
      WaitDetails details;
      while (!correctWait(details = WaitUntil(new AnyThreadState(possibleStates))))
      {
         LockSupport.parkNanos(MILLISECONDS.toNanos(20));
         if (currentTimeMillis() > end)
         {
            throw new TimeoutException("Wanted to wait for any of " + Arrays.ToString(possibleStates) + " over at " + correctWait + ", but didn't managed to get there in " + _timeout + "ms. " + "instead ended up waiting in " + details);
         }
      }
      return details;
   }

   //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
   //ORIGINAL LINE: public WaitDetails waitUntil(System.Predicate<Thread> condition) throws java.util.concurrent.TimeoutException
   public virtual WaitDetails WaitUntil(System.Predicate<Thread> condition)
   {
      long end = DateTimeHelper.CurrentUnixTimeMillis() + _timeout;
      Thread thread = Thread;
      while (!condition(thread) || _executionState == ExecutionState.RequestedExecution)
      {
         try
         {
            Thread.Sleep(1);
         }
         catch (InterruptedException)
         {
            // whatever
         }

         if (DateTimeHelper.CurrentUnixTimeMillis() > end)
         {
            throw new TimeoutException("The executor didn't meet condition '" + condition + "' inside an executing command for " + _timeout + " ms");
         }
      }

      if (_executionState == ExecutionState.Executed)
      {
         throw new System.InvalidOperationException("Would have wanted " + thread + " to wait for " + condition + " but that never happened within the duration of executed task");
      }

      return new WaitDetails(thread.StackTrace);
   }

   public class WaitDetails
   {
      internal readonly StackTraceElement[] StackTrace;

      public WaitDetails(StackTraceElement[] stackTrace)
      {
         this.StackTrace = stackTrace;
      }

      public override string ToString()
      {
         StringBuilder builder = new StringBuilder();
         foreach (StackTraceElement element in StackTrace)
         {
            builder.Append(format(element.ToString() + "%n"));
         }
         return builder.ToString();
      }

      public virtual bool IsAt(Type clz, string method)
      {
         foreach (StackTraceElement element in StackTrace)
         {
            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
            if (element.ClassName.Equals(clz.FullName) && element.MethodName.Equals(method))
            {
               return true;
            }
         }
         return false;
      }
   }

   public virtual Thread.State State()
   {
      return _thread.State;
   }

   private Thread Thread
   {
      get
      {
         Thread thread = null;
         while (thread == null)
         {
            thread = this._thread;
         }
         return thread;
      }
   }

   public override void Close()
   {
      _commandExecutor.shutdown();
      try
      {
         _commandExecutor.awaitTermination(10, TimeUnit.SECONDS);
      }
      catch (InterruptedException)
      {
         Thread.CurrentThread.Interrupt();
         // shutdownNow() will interrupt running tasks if necessary
      }
      if (!_commandExecutor.Terminated)
      {
         _commandExecutor.shutdownNow();
      }
   }

   public virtual void Interrupt()
   {
      if (_thread != null)
      {
         _thread.Interrupt();
      }
   }

   public virtual void PrintStackTrace(PrintStream @out)
   {
      Thread thread = Thread;
      @out.println(thread);
      foreach (StackTraceElement trace in thread.StackTrace)
      {
         @out.println("\tat " + trace);
      }
   }
}
}