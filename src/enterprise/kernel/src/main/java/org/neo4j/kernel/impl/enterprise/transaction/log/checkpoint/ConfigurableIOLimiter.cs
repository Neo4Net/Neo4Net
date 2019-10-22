/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.enterprise.transaction.log.checkpoint
{

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using Config = Neo4Net.Kernel.configuration.Config;
	using VisibleForTesting = Neo4Net.Utils.VisibleForTesting;

	public class ConfigurableIOLimiter : IOLimiter
	{
		 private static readonly AtomicLongFieldUpdater<ConfigurableIOLimiter> _stateUpdater = AtomicLongFieldUpdater.newUpdater( typeof( ConfigurableIOLimiter ), "state" );

		 private const int NO_LIMIT = 0;
		 private const int QUANTUM_MILLIS = 100;
		 private const int TIME_BITS = 32;
		 private static readonly long _timeMask = ( 1L << TIME_BITS ) - 1;
		 private static readonly int _quantumsPerSecond = ( int )( TimeUnit.SECONDS.toMillis( 1 ) / QUANTUM_MILLIS );

		 private readonly System.Action<object, long> _pauseNanos;

		 /// <summary>
		 /// Upper 32 bits is the "disabled counter", lower 32 bits is the "IOs per quantum" field.
		 /// The "disabled counter" is modified online in 2-increments, leaving the lowest bit for signalling when
		 /// the limiter disabled by configuration.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile long state;
		 private volatile long _state;

		 public ConfigurableIOLimiter( Config config ) : this( config, LockSupport.parkNanos )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting ConfigurableIOLimiter(org.Neo4Net.kernel.configuration.Config config, System.Action<Object, long> pauseNanos)
		 internal ConfigurableIOLimiter( Config config, System.Action<object, long> pauseNanos )
		 {
			  this._pauseNanos = pauseNanos;
			  int? iops = config.Get( GraphDatabaseSettings.check_point_iops_limit );
			  UpdateConfiguration( iops );
			  config.RegisterDynamicUpdateListener( GraphDatabaseSettings.check_point_iops_limit, ( prev, update ) => updateConfiguration( update ) );
		 }

		 private void UpdateConfiguration( int? iops )
		 {
			  long oldState;
			  long newState;
			  if ( iops == null || iops < 1 )
			  {
					do
					{
						 oldState = _stateUpdater.get( this );
						 int disabledCounter = GetDisabledCounter( oldState );
						 disabledCounter |= 1; // Raise the "permanently disabled" bit.
						 newState = ComposeState( disabledCounter, NO_LIMIT );
					} while ( !_stateUpdater.compareAndSet( this, oldState, newState ) );
			  }
			  else
			  {
					do
					{
						 oldState = _stateUpdater.get( this );
						 int disabledCounter = GetDisabledCounter( oldState );
						 disabledCounter &= unchecked( ( int )0xFFFFFFFE ); // Mask off "permanently disabled" bit.
						 int iopq = iops / _quantumsPerSecond;
						 newState = ComposeState( disabledCounter, iopq );
					} while ( !_stateUpdater.compareAndSet( this, oldState, newState ) );
			  }
		 }

		 private long ComposeState( int disabledCounter, int iopq )
		 {
			  return ( ( long ) disabledCounter ) << 32 | iopq;
		 }

		 private int GetIOPQ( long state )
		 {
			  return unchecked( ( int )( state & 0x00000000_FFFFFFFFL ) );
		 }

		 private int GetDisabledCounter( long state )
		 {
			  return ( int )( ( long )( ( ulong )state >> 32 ) );
		 }

		 // The stamp is in two 32-bit parts:
		 // The high bits are the number of IOs performed since the last pause.
		 // The low bits is the 32-bit timestamp in milliseconds (~25 day range) since the last pause.
		 // We keep adding summing up the IOs until either a quantum elapses, or we've exhausted the IOs we're allowed in
		 // this quantum. If we've exhausted our IOs, we pause for the rest of the quantum.
		 // We don't make use of the Flushable at this point, because IOs from fsyncs have a high priority, so they
		 // might jump the IO queue and cause delays for transaction log IOs. Further, fsync on some file systems also
		 // flush the entire IO queue, which can cause delays on IO rate limited cloud machines.
		 // We need the Flushable to be implemented in terms of sync_file_range before we can make use of it.
		 // NOTE: The check-pointer IOPS setting is documented as being a "best effort" hint. We are making use of that
		 // wording here, and not compensating for over-counted IOs. For instance, if we receive 2 quantums worth of IOs
		 // in one quantum, we are not going to sleep for two quantums. The reason is that such compensation algorithms
		 // can easily over-compensate, and end up sleeping a lot more than what makes sense for other rate limiting factors
		 // in the system, thus wasting IO bandwidth. No, "best effort" here means that likely end up doing a little bit
		 // more IO than what we've been configured to allow, but that's okay. If it's a problem, people can just reduce
		 // their IOPS limit setting a bit more.

		 public override long MaybeLimitIO( long previousStamp, int recentlyCompletedIOs, Flushable flushable )
		 {
			  long state = _stateUpdater.get( this );
			  if ( GetDisabledCounter( state ) > 0 )
			  {
					return Neo4Net.Io.pagecache.IOLimiter_Fields.INITIAL_STAMP;
			  }

			  long now = CurrentTimeMillis() & _timeMask;
			  long then = previousStamp & _timeMask;

			  if ( now - then > QUANTUM_MILLIS )
			  {
					return now + ( ( ( long ) recentlyCompletedIOs ) << TIME_BITS );
			  }

			  long ioSum = ( previousStamp >> TIME_BITS ) + recentlyCompletedIOs;
			  if ( ioSum >= GetIOPQ( state ) )
			  {
					long millisLeftInQuantum = QUANTUM_MILLIS - ( now - then );
					_pauseNanos.accept( this, TimeUnit.MILLISECONDS.toNanos( millisLeftInQuantum ) );
					return CurrentTimeMillis() & _timeMask;
			  }

			  return then + ( ioSum << TIME_BITS );
		 }

		 public override void DisableLimit()
		 {
			  long currentState;
			  long newState;
			  do
			  {
					currentState = _stateUpdater.get( this );
					// Increment by two to leave "permanently disabled bit" alone.
					int disabledCounter = GetDisabledCounter( currentState ) + 2;
					newState = ComposeState( disabledCounter, GetIOPQ( currentState ) );
			  } while ( !_stateUpdater.compareAndSet( this, currentState, newState ) );
		 }

		 public override void EnableLimit()
		 {
			  long currentState;
			  long newState;
			  do
			  {
					currentState = _stateUpdater.get( this );
					// Decrement by two to leave "permanently disabled bit" alone.
					int disabledCounter = GetDisabledCounter( currentState ) - 2;
					newState = ComposeState( disabledCounter, GetIOPQ( currentState ) );
			  } while ( !_stateUpdater.compareAndSet( this, currentState, newState ) );
		 }

		 public virtual bool Limited
		 {
			 get
			 {
				  return GetDisabledCounter( _state ) == 0;
			 }
		 }

		 private long CurrentTimeMillis()
		 {
			  return DateTimeHelper.CurrentUnixTimeMillis();
		 }
	}

}