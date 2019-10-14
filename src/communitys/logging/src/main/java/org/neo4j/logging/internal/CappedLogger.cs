using System;

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
namespace Neo4Net.Logging.@internal
{

	/// <summary>
	/// A CappedLogger will accept log messages, unless they occur "too much", in which case the messages will be ignored
	/// until some time passes, or the logger is reset.
	/// 
	/// It is also desirable to be aware that log capping is taking place, so we don't mistakenly lose log output due to
	/// output capping.
	/// 
	/// By default, the CappedLogger does not filter out any messages. Filtering can be configured at any time using the
	/// "set" and "unset" methods.
	/// </summary>
	public class CappedLogger
	{
		 private readonly Log @delegate;
		 // We use the filter indirection so we can atomically update the configuration without locking
		 private volatile Filter _filter;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public CappedLogger(@Nonnull Log delegate)
		 public CappedLogger( Log @delegate )
		 {
			  if ( @delegate == null )
			  {
					throw new System.ArgumentException( "The delegate StringLogger cannot be null" );
			  }
			  _filter = new Filter();
			  this.@delegate = @delegate;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void debug(@Nonnull String msg)
		 public virtual void Debug( string msg )
		 {
			  if ( _filter.accept( msg, null ) )
			  {
					@delegate.Debug( msg );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void debug(@Nonnull String msg, @Nonnull Throwable cause)
		 public virtual void Debug( string msg, Exception cause )
		 {
			  if ( _filter.accept( msg, cause ) )
			  {
					@delegate.Debug( msg, cause );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void info(@Nonnull String msg)
		 public virtual void Info( string msg )
		 {
			  if ( _filter.accept( msg, null ) )
			  {
					@delegate.Info( msg );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void info(@Nonnull String msg, @Nonnull Throwable cause)
		 public virtual void Info( string msg, Exception cause )
		 {
			  if ( _filter.accept( msg, cause ) )
			  {
					@delegate.Info( msg, cause );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void warn(@Nonnull String msg)
		 public virtual void Warn( string msg )
		 {
			  if ( _filter.accept( msg, null ) )
			  {
					@delegate.Warn( msg );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void warn(@Nonnull String msg, @Nonnull Throwable cause)
		 public virtual void Warn( string msg, Exception cause )
		 {
			  if ( _filter.accept( msg, cause ) )
			  {
					@delegate.Warn( msg, cause );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void error(@Nonnull String msg)
		 public virtual void Error( string msg )
		 {
			  if ( _filter.accept( msg, null ) )
			  {
					@delegate.Error( msg );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void error(@Nonnull String msg, @Nonnull Throwable cause)
		 public virtual void Error( string msg, Exception cause )
		 {
			  if ( _filter.accept( msg, cause ) )
			  {
					@delegate.Error( msg, cause );
			  }
		 }

		 /// <summary>
		 /// Reset the filtering state of this CappedLogger. This usually means that something good happened, and that all
		 /// filtering states that grows towards a state where the log messages are filtered out, should calm down and reset.
		 /// 
		 /// Specifically, this means that the counter in the count limit should return to zero, and that the time limit and
		 /// duplicate filter should forget about past messages.
		 /// </summary>
		 public virtual void Reset()
		 {
			  _filter = _filter.reset();
		 }

		 /// <summary>
		 /// Set a limit to the amount of logging that this logger will accept between resets. </summary>
		 /// <param name="limit"> The number of log messages that the CappedLogger will let through in between resets. </param>
		 public virtual CappedLogger setCountLimit( int limit )
		 {
			  if ( limit < 1 )
			  {
					throw new System.ArgumentException( "The count limit must be positive" );
			  }
			  _filter = _filter.setCountLimit( limit );
			  return this;
		 }

		 /// <summary>
		 /// Unset the count limit, and allow any number of messages through, provided other limits don't apply.
		 /// </summary>
		 public virtual CappedLogger UnsetCountLimit()
		 {
			  _filter = _filter.unsetCountLimit();
			  return this;
		 }

		 /// <summary>
		 /// Set a time based limit to the amount of logging that this logger will accept between resets. With a time limit
		 /// of 1 second, for instance, then the logger will log at most one message per second. </summary>
		 /// <param name="time"> The time amount, must be positive. </param>
		 /// <param name="unit"> The time unit. </param>
		 /// <param name="clock"> The clock to use for reading the current time when checking this limit. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public CappedLogger setTimeLimit(long time, @Nonnull TimeUnit unit, @Nonnull Clock clock)
		 public virtual CappedLogger SetTimeLimit( long time, TimeUnit unit, Clock clock )
		 {
			  if ( time < 1 )
			  {
					throw new System.ArgumentException( "The time limit must be positive" );
			  }
			  if ( unit == null )
			  {
					throw new System.ArgumentException( "The time unit cannot be null" );
			  }
			  if ( clock == null )
			  {
					throw new System.ArgumentException( "The clock used for time limiting cannot be null" );
			  }
			  _filter = _filter.setTimeLimit( time, unit, clock );
			  return this;
		 }

		 /// <summary>
		 /// Unset the time limit, and allow any number of messages through, as often as possible, provided other limits
		 /// don't apply.
		 /// </summary>
		 public virtual CappedLogger UnsetTimeLimit()
		 {
			  _filter = _filter.unsetTimeLimit();
			  return this;
		 }

		 /// <summary>
		 /// Enable or disable filtering of duplicate messages. This filtering only looks at the previous message, so a
		 /// sequence of identical messages will only have that message logged once, but a sequence of two alternating
		 /// messages will get logged in full. </summary>
		 /// <param name="enabled"> {@code true} if duplicates should be filtered, {@code false} if they should not. </param>
		 public virtual CappedLogger setDuplicateFilterEnabled( bool enabled )
		 {
			  _filter = _filter.setDuplicateFilterEnabled( enabled );
			  return this;
		 }

		 private class Filter
		 {
			  internal static readonly AtomicIntegerFieldUpdater<Filter> CurrentCount = AtomicIntegerFieldUpdater.newUpdater( typeof( Filter ), "currentCount" );
			  internal static readonly AtomicLongFieldUpdater<Filter> LastCheck = AtomicLongFieldUpdater.newUpdater( typeof( Filter ), "lastCheck" );

			  // The thread-safety of these normal fields are guarded by the volatile reads and writes to the
			  // CappedLogger.filter field.
			  internal bool HasCountLimit;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int CountLimitConflict;
			  internal long TimeLimitMillis;
			  internal readonly Clock Clock;
			  internal bool FilterDuplicates;

			  // Atomically updated
			  internal volatile int CurrentCount;
			  internal volatile long LastCheck;

			  // Read and updated together; guarded by synchronized(this) in checkDuplicate()
			  internal string LastMessage;
			  internal Exception LastException;

			  internal Filter() : this(false, 0, 0, 0, 0, null, false)
			  {
			  }

			  internal Filter( bool hasCountLimit, int countLimit, int currentCount, long timeLimitMillis, long lastCheck, Clock clock, bool filterDuplicates )
			  {
					this.HasCountLimit = hasCountLimit;
					this.CountLimitConflict = countLimit;
					this.CurrentCount = currentCount;
					this.TimeLimitMillis = timeLimitMillis;
					this.LastCheck = lastCheck;
					this.Clock = clock;
					this.FilterDuplicates = filterDuplicates;
			  }

			  public virtual Filter setCountLimit( int limit )
			  {
					return new Filter( true, limit, CurrentCount, TimeLimitMillis, LastCheck, Clock, FilterDuplicates );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public boolean accept(@Nonnull String msg, @Nullable Throwable cause)
			  public virtual bool Accept( string msg, Exception cause )
			  {
					return ( !HasCountLimit || ( AndIncrementCurrentCount < CountLimitConflict ) ) && ( Clock == null || !CheckExpiredAndSetLastCheckTime() ) && (!FilterDuplicates || CheckDuplicate(msg, cause));
			  }

			  public virtual int AndIncrementCurrentCount
			  {
				  get
				  {
						return CurrentCount.getAndIncrement( this );
				  }
			  }

			  internal virtual bool CheckExpiredAndSetLastCheckTime()
			  {
					long now = Clock.millis();
					long check = this.LastCheck;
					if ( check > now - TimeLimitMillis )
					{
						 return true;
					}
					while ( !LastCheck.compareAndSet( this, check, now ) )
					{
						 check = LastCheck;
						 if ( check > now )
						 {
							  break;
						 }
					}
					return false;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private synchronized boolean checkDuplicate(@Nonnull String msg, @Nullable Throwable cause)
			  internal virtual bool CheckDuplicate( string msg, Exception cause )
			  {
				  lock ( this )
				  {
						string last = LastMessage;
						Exception exc = LastException;
						if ( StringEqual( last, msg ) && SameClass( cause, exc ) && SameMsg( cause, exc ) )
						{
							 // Duplicate! Filter it out.
							 return false;
						}
						else
						{
							 // Distinct! Update and let it through.
							 LastMessage = msg;
							 LastException = cause;
							 return true;
						}
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private boolean sameMsg(@Nullable Throwable cause, @Nullable Throwable exc)
			  internal virtual bool SameMsg( Exception cause, Exception exc )
			  {
					return ( cause == null && exc == null ) || ( cause != null && exc != null && StringEqual( exc.Message, cause.Message ) );
			  }

			  internal virtual bool StringEqual( string a, string b )
			  {
					return string.ReferenceEquals( a, null ) ? string.ReferenceEquals( b, null ) : a.Equals( b );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private boolean sameClass(@Nullable Throwable cause, @Nullable Throwable exc)
			  internal virtual bool SameClass( Exception cause, Exception exc )
			  {
					return ( cause == null && exc == null ) || ( cause != null && exc != null && exc.GetType().Equals(cause.GetType()) );
			  }

			  public virtual Filter Reset()
			  {
					return new Filter( HasCountLimit, CountLimitConflict, 0, TimeLimitMillis, 0, Clock, FilterDuplicates );
			  }

			  public virtual Filter UnsetCountLimit()
			  {
					return new Filter( false, 0, CurrentCount, TimeLimitMillis, LastCheck, Clock, FilterDuplicates );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public Filter setTimeLimit(long time, @Nonnull TimeUnit unit, @Nonnull Clock clock)
			  public virtual Filter SetTimeLimit( long time, TimeUnit unit, Clock clock )
			  {
					return new Filter( HasCountLimit, CountLimitConflict, CurrentCount, unit.toMillis( time ), LastCheck, clock, FilterDuplicates );
			  }

			  public virtual Filter UnsetTimeLimit()
			  {
					return new Filter( HasCountLimit, CountLimitConflict, CurrentCount, 0, LastCheck, null, FilterDuplicates );
			  }

			  public virtual Filter setDuplicateFilterEnabled( bool enabled )
			  {
					return new Filter( HasCountLimit, CountLimitConflict, CurrentCount, TimeLimitMillis, LastCheck, Clock, enabled );
			  }
		 }
	}

}