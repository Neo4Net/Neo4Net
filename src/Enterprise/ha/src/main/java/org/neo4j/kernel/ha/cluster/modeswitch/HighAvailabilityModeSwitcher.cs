using System;
using System.Diagnostics;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.ha.cluster.modeswitch
{

	using BindingListener = Neo4Net.cluster.BindingListener;
	using InstanceId = Neo4Net.cluster.InstanceId;
	using ClusterClient = Neo4Net.cluster.client.ClusterClient;
	using ClusterMemberAvailability = Neo4Net.cluster.member.ClusterMemberAvailability;
	using Election = Neo4Net.cluster.protocol.election.Election;
	using CancellationRequest = Neo4Net.Helpers.CancellationRequest;
	using HighAvailabilityStoreFailureException = Neo4Net.Kernel.ha.store.HighAvailabilityStoreFailureException;
	using UnableToCopyStoreFromOldMasterException = Neo4Net.Kernel.ha.store.UnableToCopyStoreFromOldMasterException;
	using MismatchingStoreIdException = Neo4Net.Kernel.impl.store.MismatchingStoreIdException;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.ClusterSettings.INSTANCE_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.NamedThreadFactory.named;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Uris.parameter;

	/// <summary>
	/// Performs the internal switches in various services from pending to slave/master, by listening for
	/// <seealso cref="HighAvailabilityMemberChangeEvent"/>s. When finished it will invoke
	/// <seealso cref="ClusterMemberAvailability.memberIsAvailable(string, URI, StoreId)"/> to announce it's new status to the
	/// cluster.
	/// </summary>
	public class HighAvailabilityModeSwitcher : HighAvailabilityMemberListener, BindingListener, Lifecycle
	{
		 public const string MASTER = "master";
		 public const string SLAVE = "slave";
		 public const string UNKNOWN = "UNKNOWN";

		 public const string INADDR_ANY = "0.0.0.0";

		 private readonly ComponentSwitcher _componentSwitcher;

		 private volatile URI _masterHaURI;
		 private volatile URI _slaveHaURI;
		 private CancellationHandle _cancellationHandle; // guarded by synchronized in startModeSwitching()

		 public static InstanceId GetServerId( URI haUri )
		 {
			  // Get serverId parameter, default to -1 if it is missing, and parse to integer
			  string serverIdParam = parameter( "serverId" ).apply( haUri );
			  return INSTANCE_ID.apply( !string.ReferenceEquals( serverIdParam, null ) ? serverIdParam : "-1" );
		 }

		 private URI _availableMasterId;

		 private SwitchToSlave _switchToSlave;
		 private SwitchToMaster _switchToMaster;
		 private readonly Election _election;
		 private readonly ClusterMemberAvailability _clusterMemberAvailability;
		 private readonly ClusterClient _clusterClient;
		 private readonly System.Func<StoreId> _storeIdSupplier;
		 private readonly InstanceId _instanceId;

		 private readonly Log _msgLog;
		 private readonly Log _userLog;

		 private LifeSupport _haCommunicationLife;

		 private ScheduledExecutorService _modeSwitcherExecutor;
		 private volatile URI _me;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private volatile java.util.concurrent.Future<?> modeSwitcherFuture;
		 private volatile Future<object> _modeSwitcherFuture;

		 /*
		  * Valid values for this is TO_MASTER, TO_SLAVE or PENDING. It is updated before the switcher is
		  * called to the corresponding new state or set to PENDING if a switcher fails and doesn't retry.
		  */
		 private volatile HighAvailabilityMemberState _currentTargetState;
		 private readonly AtomicBoolean _canAskForElections = new AtomicBoolean( true );
		 private readonly DataSourceManager _neoStoreDataSourceSupplier;

		 public HighAvailabilityModeSwitcher( SwitchToSlave switchToSlave, SwitchToMaster switchToMaster, Election election, ClusterMemberAvailability clusterMemberAvailability, ClusterClient clusterClient, System.Func<StoreId> storeIdSupplier, InstanceId instanceId, ComponentSwitcher componentSwitcher, DataSourceManager neoStoreDataSourceSupplier, LogService logService )
		 {
			  this._switchToSlave = switchToSlave;
			  this._switchToMaster = switchToMaster;
			  this._election = election;
			  this._clusterMemberAvailability = clusterMemberAvailability;
			  this._clusterClient = clusterClient;
			  this._storeIdSupplier = storeIdSupplier;
			  this._instanceId = instanceId;
			  this._componentSwitcher = componentSwitcher;
			  this._msgLog = logService.GetInternalLog( this.GetType() );
			  this._userLog = logService.GetUserLog( this.GetType() );
			  this._neoStoreDataSourceSupplier = neoStoreDataSourceSupplier;
			  this._haCommunicationLife = new LifeSupport();
		 }

		 public override void ListeningAt( URI myUri )
		 {
			  _me = myUri;
		 }

		 public override void Init()
		 {
			 lock ( this )
			 {
				  _modeSwitcherExecutor = CreateExecutor();
      
				  _haCommunicationLife.init();
			 }
		 }

		 public override void Start()
		 {
			 lock ( this )
			 {
				  _haCommunicationLife.start();
			 }
		 }

		 public override void Stop()
		 {
			 lock ( this )
			 {
				  _haCommunicationLife.stop();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			 lock ( this )
			 {
				  _modeSwitcherExecutor.shutdown();
      
				  _modeSwitcherExecutor.awaitTermination( 60, TimeUnit.SECONDS );
      
				  _haCommunicationLife.shutdown();
      
				  _switchToMaster.close();
				  _switchToMaster = null;
				  _switchToSlave = null;
			 }
		 }

		 public override void MasterIsElected( HighAvailabilityMemberChangeEvent @event )
		 {
			  if ( @event.NewState == @event.OldState && @event.OldState == HighAvailabilityMemberState.MASTER )
			  {
					_clusterMemberAvailability.memberIsAvailable( MASTER, _masterHaURI, _storeIdSupplier.get() );
			  }
			  else
			  {
					StateChanged( @event );
			  }
		 }

		 public override void MasterIsAvailable( HighAvailabilityMemberChangeEvent @event )
		 {
			  if ( @event.NewState == @event.OldState && @event.OldState == HighAvailabilityMemberState.SLAVE )
			  {
					_clusterMemberAvailability.memberIsAvailable( SLAVE, _slaveHaURI, _storeIdSupplier.get() );
			  }
			  else
			  {
					StateChanged( @event );
			  }
		 }

		 public override void SlaveIsAvailable( HighAvailabilityMemberChangeEvent @event )
		 {
			  // ignored, we don't do any mode switching in slave available events
		 }

		 public override void InstanceStops( HighAvailabilityMemberChangeEvent @event )
		 {
			  StateChanged( @event );
		 }

		 public override void InstanceDetached( HighAvailabilityMemberChangeEvent @event )
		 {
			  SwitchToDetached();
		 }

		 public virtual void ForceElections()
		 {
			  if ( _canAskForElections.compareAndSet( true, false ) )
			  {
					_clusterMemberAvailability.memberIsUnavailable( HighAvailabilityModeSwitcher.SLAVE );
					_election.performRoleElections();
			  }
		 }

		 private void StateChanged( HighAvailabilityMemberChangeEvent @event )
		 {
			  /*
			   * First of all, check if the state change is internal or external. In this context, internal means
			   * that the old and new state are different, so we definitely need to do something.
			   * Both cases may require a switcher to be activated, but external needs to check if the same as previously
			   * should be the one used (because the last attempt failed, for example) or maybe we simply need to update
			   * a field. Internal will probably require a new switcher to be used.
			   */
			  if ( @event.NewState == @event.OldState )
			  {
					/*
					 * This is the external change case. We need to check our internals and perhaps retry a transition
					 */
					if ( @event.NewState != HighAvailabilityMemberState.TO_MASTER )
					{
						 /*
						  * We get here if for example a new master becomes available while we are already switching. In that
						  * case we don't change state but we must update with the new availableMasterId,
						  * but only if it is not null.
						  */
						 if ( @event.ServerHaUri != null )
						 {
							  _availableMasterId = @event.ServerHaUri;
						 }
						 return;
					}
					/*
					 * The other case is that the new state is TO_MASTER
					 */
					else if ( _currentTargetState == HighAvailabilityMemberState.TO_MASTER )
					{
						 /*
						  * We are still switching from before. If a failure had happened, then currentTargetState would
						  * be PENDING.
						  */
						 return;
					}
			  }

			  _availableMasterId = @event.ServerHaUri;

			  _currentTargetState = @event.NewState;
			  switch ( @event.NewState )
			  {
					case TO_MASTER:

						 if ( @event.OldState.Equals( HighAvailabilityMemberState.SLAVE ) )
						 {
							  _clusterMemberAvailability.memberIsUnavailable( SLAVE );
						 }

						 SwitchToMaster();
						 break;
					case TO_SLAVE:
						 SwitchToSlave();
						 break;
					case PENDING:

						 SwitchToPending( @event.OldState );
						 break;
					default:
						 // do nothing
				break;
			  }
		 }

		 private void SwitchToMaster()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CancellationHandle cancellationHandle = new CancellationHandle();
			  CancellationHandle cancellationHandle = new CancellationHandle();
			  StartModeSwitching(() =>
			  {
				if ( _currentTargetState != HighAvailabilityMemberState.ToMaster )
				{
					 return;
				}

				// We just got scheduled. Maybe we are already obsolete - test
				if ( cancellationHandle.CancellationRequested() )
				{
					 _msgLog.info( "Switch to master cancelled on start." );
					 return;
				}

				_componentSwitcher.switchToMaster();

				if ( cancellationHandle.CancellationRequested() )
				{
					 _msgLog.info( "Switch to master cancelled before ha communication started." );
					 return;
				}

				_haCommunicationLife.shutdown();
				_haCommunicationLife = new LifeSupport();

				try
				{
					 _masterHaURI = _switchToMaster.switchToMaster( _haCommunicationLife, _me );
					 _canAskForElections.set( true );
				}
				catch ( Exception e )
				{
					 _msgLog.error( "Failed to switch to master", e );
					 /*
					  * If the attempt to switch to master fails, then we must not try again. We'll trigger an election
					  * and if we are elected again, we'll try again. We differentiate between this case and the case where
					  * we simply receive another election result while switching hasn't completed yet by setting the
					  * currentTargetState as follows:
					  */
					 _currentTargetState = HighAvailabilityMemberState.Pending;
					 // Since this master switch failed, elect someone else
					 _election.demote( _instanceId );
				}
			  }, cancellationHandle);
		 }

		 private void SwitchToSlave()
		 {
			  // Do this with a scheduler, so that if it fails, it can retry later with an exponential backoff with max
			  // wait time.
			  /*
			   * This is purely defensive and should never trigger. There was a race where the switch to slave task would
			   * start after this instance was elected master and the task would constantly try to change as slave
			   * for itself, never cancelling. This now should not be possible, since we cancel the task and wait for it
			   * to complete, all in a single thread executor. However, this is a check worth doing because if this
			   * condition slips through via some other code path it can cause trouble.
			   */
			  if ( GetServerId( _availableMasterId ).Equals( _instanceId ) )
			  {
					_msgLog.error( "I (" + _me + ") tried to switch to slave for myself as master (" + _availableMasterId + ")" );
					return;
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicLong wait = new java.util.concurrent.atomic.AtomicLong();
			  AtomicLong wait = new AtomicLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CancellationHandle cancellationHandle = new CancellationHandle();
			  CancellationHandle cancellationHandle = new CancellationHandle();
			  StartModeSwitching(() =>
			  {
				 if ( _currentTargetState != HighAvailabilityMemberState.ToSlave )
				 {
					  return; // Already switched - this can happen if a second master becomes available while waiting
				 }

				 if ( cancellationHandle.CancellationRequested() )
				 {
					  _msgLog.info( "Switch to slave cancelled on start." );
					  return;
				 }

				 _componentSwitcher.switchToSlave();

				 try
				 {
					  if ( cancellationHandle.CancellationRequested() )
					  {
							_msgLog.info( "Switch to slave cancelled before ha communication started." );
							return;
					  }

					  _haCommunicationLife.shutdown();
					  _haCommunicationLife = new LifeSupport();

					  // it is important for availableMasterId to be re-read on every attempt so that
					  // slave switching would not result in an infinite loop with wrong/stale availableMasterId
					  URI resultingSlaveHaURI = _switchToSlave.switchToSlave( _haCommunicationLife, _me, _availableMasterId, cancellationHandle );
					  if ( resultingSlaveHaURI == null )
					  {
							/*
							 * null slave uri means the task was cancelled. The task then must simply terminate and
							 * have no side effects.
							 */
							_msgLog.info( "Switch to slave is effectively cancelled" );
					  }
					  else
					  {
							_slaveHaURI = resultingSlaveHaURI;
							_canAskForElections.set( true );
					  }
				 }
				 catch ( HighAvailabilityStoreFailureException e )
				 {
					  _userLog.error( "UNABLE TO START UP AS SLAVE: %s", e.Message );
					  _msgLog.error( "Unable to start up as slave", e );

					  _clusterMemberAvailability.memberIsUnavailable( SLAVE );
					  ClusterClient clusterClient = HighAvailabilityModeSwitcher.this._clusterClient;
					  try
					  {
							// TODO I doubt this actually works
							clusterClient.leave();
							clusterClient.stop();
							_haCommunicationLife.shutdown();
					  }
					  catch ( Exception t )
					  {
							_msgLog.error( "Unable to stop cluster client", t );
					  }

					  _modeSwitcherExecutor.schedule( this, 5, TimeUnit.SECONDS );
				 }
				 catch ( MismatchingStoreIdException )
				 {
					  // Try again immediately, the place that threw it have already treated the db
					  // as branched and so a new attempt will have this slave copy a new store from master.
					  run();
				 }
				 catch ( Exception t )
				 {
					  _msgLog.error( "Error while trying to switch to slave", t );

					  // Try again later
					  wait.set( 1 + wait.get() * 2 ); // Exponential backoff
					  wait.set( Math.Min( wait.get(), 5 * 60 ) ); // Wait maximum 5 minutes

					  _modeSwitcherFuture = _modeSwitcherExecutor.schedule( this, wait.get(), TimeUnit.SECONDS );

					  _msgLog.info( "Attempting to switch to slave in %ds", wait.get() );
				 }
			  }, cancellationHandle);
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void switchToPending(final org.neo4j.kernel.ha.cluster.HighAvailabilityMemberState oldState)
		 private void SwitchToPending( HighAvailabilityMemberState oldState )
		 {
			  _msgLog.info( "I am %s, moving to pending", _instanceId );

			  StartModeSwitching(() =>
			  {
				if ( _cancellationHandle.cancellationRequested() )
				{
					 _msgLog.info( "Switch to pending cancelled on start." );
					 return;
				}

				_componentSwitcher.switchToPending();
				_neoStoreDataSourceSupplier.DataSource.beforeModeSwitch();

				if ( _cancellationHandle.cancellationRequested() )
				{
					 _msgLog.info( "Switch to pending cancelled before ha communication shutdown." );
					 return;
				}

				_haCommunicationLife.shutdown();
				_haCommunicationLife = new LifeSupport();
			  }, new CancellationHandle());

			  try
			  {
					_modeSwitcherFuture.get( 10, TimeUnit.SECONDS );
			  }
			  catch ( Exception )
			  {
			  }
		 }

		 private void SwitchToDetached()
		 {
			  _msgLog.info( "I am %s, moving to detached", _instanceId );

			  StartModeSwitching(() =>
			  {
				if ( _cancellationHandle.cancellationRequested() )
				{
					 _msgLog.info( "Switch to pending cancelled on start." );
					 return;
				}

				_componentSwitcher.switchToSlave();
				_neoStoreDataSourceSupplier.DataSource.beforeModeSwitch();

				if ( _cancellationHandle.cancellationRequested() )
				{
					 _msgLog.info( "Switch to pending cancelled before ha communication shutdown." );
					 return;
				}

				_haCommunicationLife.shutdown();
				_haCommunicationLife = new LifeSupport();
			  }, new CancellationHandle());

			  try
			  {
					_modeSwitcherFuture.get( 10, TimeUnit.SECONDS );
			  }
			  catch ( Exception e )
			  {
					_msgLog.warn( "Exception received while waiting for switching to detached", e );
			  }
		 }

		 private void StartModeSwitching( ThreadStart switcher, CancellationHandle cancellationHandle )
		 {
			 lock ( this )
			 {
				  if ( _modeSwitcherFuture != null )
				  {
						// Cancel any delayed previous switching
						this._cancellationHandle.cancel();
						// Wait for it to actually stop what it was doing
						try
						{
							 _modeSwitcherFuture.get();
						}
						catch ( UnableToCopyStoreFromOldMasterException e )
						{
							 throw e;
						}
						catch ( Exception e )
						{
							 _msgLog.warn( "Got exception from cancelled task", e );
						}
				  }
      
				  this._cancellationHandle = cancellationHandle;
				  _modeSwitcherFuture = _modeSwitcherExecutor.submit( switcher );
			 }
		 }

		 internal virtual ScheduledExecutorService CreateExecutor()
		 {
			  return Executors.newSingleThreadScheduledExecutor( named( "HA Mode switcher" ) );
		 }

		 private class CancellationHandle : CancellationRequest
		 {
			  internal volatile bool Cancelled;

			  public override bool CancellationRequested()
			  {
					return Cancelled;
			  }

			  public virtual void Cancel()
			  {
					Debug.Assert( !Cancelled, "Should not cancel on the same request twice" );
					Cancelled = true;
			  }
		 }
	}

}