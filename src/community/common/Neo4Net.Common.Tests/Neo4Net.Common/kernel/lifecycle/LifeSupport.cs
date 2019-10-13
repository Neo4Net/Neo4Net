using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
namespace Neo4Net.Kernel.Lifecycle
{

	using Exceptions = Neo4Net.Helpers.Exceptions;


	/// <summary>
	/// Support class for handling collections of Lifecycle instances. Manages the transitions from one state to another.
	/// <para>
	/// To use this, first add instances to it that implement the Lifecycle interface. When lifecycle methods on this
	/// class are called it will try to invoke the same methods on the registered instances.
	/// </para>
	/// <para>
	/// Components that internally owns other components that has a lifecycle can use this to control them as well.
	/// </para>
	/// </summary>
	public class LifeSupport : Lifecycle
	{
		 private volatile IList<LifecycleInstance> _instances = new List<LifecycleInstance>();
		 private volatile LifecycleStatus _status = LifecycleStatus.None;
		 private readonly IList<LifecycleListener> _listeners = new List<LifecycleListener>();
		 private LifecycleInstance _last;

		 public LifeSupport()
		 {
		 }

		 /// <summary>
		 /// Initialize all registered instances, transitioning from status NONE to STOPPED.
		 /// <para>
		 /// If transition fails, then it goes to STOPPED and then SHUTDOWN, so it cannot be restarted again.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void init() throws LifecycleException
		 public override void Init()
		 {
			 lock ( this )
			 {
				  if ( _status == LifecycleStatus.None )
				  {
						_status = ChangedStatus( this, _status, LifecycleStatus.Initializing );
						foreach ( LifecycleInstance instance in _instances )
						{
							 try
							 {
								  instance.Init();
							 }
							 catch ( LifecycleException e )
							 {
								  _status = ChangedStatus( this, _status, LifecycleStatus.Stopped );
      
								  try
								  {
										Shutdown();
								  }
								  catch ( LifecycleException shutdownErr )
								  {
										e.addSuppressed( shutdownErr );
								  }
      
								  throw e;
							 }
						}
						_status = ChangedStatus( this, _status, LifecycleStatus.Stopped );
				  }
			 }
		 }

		 /// <summary>
		 /// Start all registered instances, transitioning from STOPPED to STARTED.
		 /// <para>
		 /// If it was previously not initialized, it will be initialized first.
		 /// </para>
		 /// <para>
		 /// If any instance fails to start, the already started instances will be stopped, so
		 /// that the overall status is STOPPED.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="LifecycleException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void start() throws LifecycleException
		 public override void Start()
		 {
			 lock ( this )
			 {
				  Init();
      
				  if ( _status == LifecycleStatus.Stopped )
				  {
						_status = ChangedStatus( this, _status, LifecycleStatus.Starting );
						foreach ( LifecycleInstance instance in _instances )
						{
							 try
							 {
								  instance.Start();
							 }
							 catch ( LifecycleException e )
							 {
								  // TODO perhaps reconsider chaining of exceptions coming from LifeSupports?
								  _status = ChangedStatus( this, _status, LifecycleStatus.Started );
								  try
								  {
										Stop();
      
								  }
								  catch ( LifecycleException stopErr )
								  {
										e.addSuppressed( stopErr );
								  }
      
								  throw e;
							 }
						}
						_status = ChangedStatus( this, _status, LifecycleStatus.Started );
				  }
			 }
		 }

		 /// <summary>
		 /// Stop all registered instances, transitioning from STARTED to STOPPED.
		 /// <para>
		 /// If any instance fails to stop, the rest of the instances will still be stopped,
		 /// so that the overall status is STOPPED.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void stop() throws LifecycleException
		 public override void Stop()
		 {
			 lock ( this )
			 {
				  if ( _status == LifecycleStatus.Started )
				  {
						_status = ChangedStatus( this, _status, LifecycleStatus.Stopping );
						LifecycleException ex = StopInstances( _instances );
						_status = ChangedStatus( this, _status, LifecycleStatus.Stopped );
      
						if ( ex != null )
						{
							 throw ex;
						}
				  }
			 }
		 }

		 /// <summary>
		 /// Shutdown all registered instances, transitioning from either STARTED or STOPPED to SHUTDOWN.
		 /// <para>
		 /// If any instance fails to shutdown, the rest of the instances will still be shut down,
		 /// so that the overall status is SHUTDOWN.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void shutdown() throws LifecycleException
		 public override void Shutdown()
		 {
			 lock ( this )
			 {
				  LifecycleException ex = null;
				  try
				  {
						Stop();
				  }
				  catch ( LifecycleException e )
				  {
						ex = e;
				  }
      
				  if ( _status == LifecycleStatus.Stopped )
				  {
						_status = ChangedStatus( this, _status, LifecycleStatus.ShuttingDown );
						for ( int i = _instances.Count - 1; i >= 0; i-- )
						{
							 LifecycleInstance lifecycleInstance = _instances[i];
							 try
							 {
								  lifecycleInstance.Shutdown();
							 }
							 catch ( LifecycleException e )
							 {
								  ex = Exceptions.chain( ex, e );
							 }
						}
      
						_status = ChangedStatus( this, _status, LifecycleStatus.Shutdown );
      
						if ( ex != null )
						{
							 throw ex;
						}
				  }
			 }
		 }

		 /// <summary>
		 /// Add a new Lifecycle instance. It will immediately be transitioned
		 /// to the state of this LifeSupport.
		 /// </summary>
		 /// <param name="instance"> the Lifecycle instance to add </param>
		 /// @param <T> type of the instance </param>
		 /// <returns> the instance itself </returns>
		 /// <exception cref="LifecycleException"> if the instance could not be transitioned properly </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized <T extends Lifecycle> T add(T instance) throws LifecycleException
		 public virtual T Add<T>( T instance ) where T : Lifecycle
		 {
			 lock ( this )
			 {
				  AddNewComponent( instance );
				  return instance;
			 }
		 }

		 public virtual T setLast<T>( T instance ) where T : Lifecycle
		 {
			 lock ( this )
			 {
				  if ( _last != null )
				  {
						throw new System.InvalidOperationException( format( "Lifecycle supports only one last component. Already defined component: %s, new component: %s", _last, instance ) );
				  }
				  _last = AddNewComponent( instance );
				  return instance;
			 }
		 }

		 private LifecycleInstance AddNewComponent<T>( T instance ) where T : Lifecycle
		 {
			  Objects.requireNonNull( instance );
			  ValidateNotAlreadyPartOfLifecycle( instance );
			  LifecycleInstance newInstance = new LifecycleInstance( this, instance );
			  IList<LifecycleInstance> tmp = new List<LifecycleInstance>( _instances );
			  int position = _last != null ? tmp.Count - 1 : tmp.Count;
			  tmp.Insert( position, newInstance );
			  _instances = tmp;
			  BringToState( newInstance );
			  return newInstance;
		 }

		 private void ValidateNotAlreadyPartOfLifecycle( Lifecycle instance )
		 {
			  foreach ( LifecycleInstance candidate in _instances )
			  {
					if ( candidate.Instance == instance )
					{
						 throw new System.InvalidOperationException( instance + " already added", candidate.AddedWhere );
					}
			  }
		 }

		 private LifecycleException StopInstances( IList<LifecycleInstance> instances )
		 {
			  LifecycleException ex = null;
			  for ( int i = instances.Count - 1; i >= 0; i-- )
			  {
					LifecycleInstance lifecycleInstance = instances[i];
					try
					{
						 lifecycleInstance.Stop();
					}
					catch ( LifecycleException e )
					{
						 ex = Exceptions.chain( ex, e );
					}
			  }
			  return ex;
		 }

		 public virtual bool Remove( Lifecycle instance )
		 {
			 lock ( this )
			 {
				  for ( int i = 0; i < _instances.Count; i++ )
				  {
						if ( _instances[i].isInstance( instance ) )
						{
							 IList<LifecycleInstance> tmp = new List<LifecycleInstance>( _instances );
							 LifecycleInstance lifecycleInstance = tmp.RemoveAt( i );
							 lifecycleInstance.Shutdown();
							 _instances = tmp;
							 return true;
						}
				  }
				  return false;
			 }
		 }

		 public virtual IList<Lifecycle> LifecycleInstances
		 {
			 get
			 {
				  return _instances.Select( l => l.instance ).ToList();
			 }
		 }

		 /// <summary>
		 /// Shutdown and throw away all the current instances. After
		 /// this you can add new instances. This method does not change
		 /// the status of the LifeSupport (i.e. if it was started it will remain started)
		 /// </summary>
		 public virtual void Clear()
		 {
			 lock ( this )
			 {
				  foreach ( LifecycleInstance instance in _instances )
				  {
						instance.Shutdown();
				  }
				  _instances = new List<LifecycleInstance>();
			 }
		 }

		 public virtual LifecycleStatus Status
		 {
			 get
			 {
				  return _status;
			 }
		 }

		 public virtual void AddLifecycleListener( LifecycleListener listener )
		 {
			 lock ( this )
			 {
				  _listeners.Add( listener );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void bringToState(LifecycleInstance instance) throws LifecycleException
		 private void BringToState( LifecycleInstance instance )
		 {
					switch ( _status )
					{
					case Neo4Net.Kernel.Lifecycle.LifecycleStatus.Started:
						 instance.Start();
						 break;
					case Neo4Net.Kernel.Lifecycle.LifecycleStatus.Stopped:
						 instance.Init();
						 break;
					default:
						 break;
					}
		 }

		 private LifecycleStatus ChangedStatus( Lifecycle instance, LifecycleStatus oldStatus, LifecycleStatus newStatus )
		 {
			  foreach ( LifecycleListener listener in _listeners )
			  {
					listener.NotifyStatusChanged( instance, oldStatus, newStatus );
			  }

			  return newStatus;
		 }

		 public virtual bool Running
		 {
			 get
			 {
				  return _status == LifecycleStatus.Started;
			 }
		 }

		 public override string ToString()
		 {
			  StringBuilder sb = new StringBuilder();
			  ToString( 0, sb );
			  return sb.ToString();
		 }

		 private void ToString( int indent, StringBuilder sb )
		 {
			  for ( int i = 0; i < indent; i++ )
			  {
					sb.Append( ' ' );
			  }
			  sb.Append( "Lifecycle status:" + _status.name() ).Append('\n');
			  foreach ( LifecycleInstance instance in _instances )
			  {
					if ( instance.Instance is LifeSupport )
					{
						 ( ( LifeSupport ) instance.Instance ).ToString( indent + 3, sb );
					}
					else
					{
						 for ( int i = 0; i < indent + 3; i++ )
						 {
							  sb.Append( ' ' );
						 }
						 sb.Append( instance.ToString() ).Append('\n');

					}

			  }

		 }

		 private class LifecycleInstance : Lifecycle
		 {
			 private readonly LifeSupport _outerInstance;

			  internal Lifecycle Instance;
			  internal LifecycleStatus CurrentStatus = LifecycleStatus.None;
			  internal Exception AddedWhere;

			  internal LifecycleInstance( LifeSupport outerInstance, Lifecycle instance )
			  {
				  this._outerInstance = outerInstance;
					this.Instance = instance;
					Debug.Assert( TrackInstantiationStackTrace() );
			  }

			  internal virtual bool TrackInstantiationStackTrace()
			  {
					AddedWhere = new Exception();
					return true;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws LifecycleException
			  public override void Init()
			  {
					if ( CurrentStatus == LifecycleStatus.None )
					{
						 CurrentStatus = outerInstance.changedStatus( Instance, CurrentStatus, LifecycleStatus.Initializing );
						 try
						 {
							  Instance.init();
							  CurrentStatus = outerInstance.changedStatus( Instance, CurrentStatus, LifecycleStatus.Stopped );
						 }
						 catch ( Exception e )
						 {
							  CurrentStatus = outerInstance.changedStatus( Instance, CurrentStatus, LifecycleStatus.None );
							  try
							  {
									Instance.shutdown();
							  }
							  catch ( Exception se )
							  {
									LifecycleException lifecycleException = new LifecycleException( "Exception during graceful " + "attempt to shutdown partially initialized component. Please use non suppressed" + " exception to see original component failure.", se );
									e.addSuppressed( lifecycleException );
							  }
							  if ( e is LifecycleException )
							  {
									throw ( LifecycleException ) e;
							  }
							  throw new LifecycleException( Instance, LifecycleStatus.None, LifecycleStatus.Stopped, e );
						 }
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws LifecycleException
			  public override void Start()
			  {
					if ( CurrentStatus == LifecycleStatus.None )
					{
						 Init();
					}
					if ( CurrentStatus == LifecycleStatus.Stopped )
					{
						 CurrentStatus = outerInstance.changedStatus( Instance, CurrentStatus, LifecycleStatus.Starting );
						 try
						 {
							  Instance.start();
							  CurrentStatus = outerInstance.changedStatus( Instance, CurrentStatus, LifecycleStatus.Started );
						 }
						 catch ( Exception e )
						 {
							  CurrentStatus = outerInstance.changedStatus( Instance, CurrentStatus, LifecycleStatus.Stopped );
							  try
							  {
									Instance.stop();
							  }
							  catch ( Exception se )
							  {
									LifecycleException lifecycleException = new LifecycleException( "Exception during graceful " + "attempt to stop partially started component. Please use non suppressed" + " exception to see original component failure.", se );
									e.addSuppressed( lifecycleException );
							  }
							  if ( e is LifecycleException )
							  {
									throw ( LifecycleException ) e;
							  }
							  throw new LifecycleException( Instance, LifecycleStatus.Stopped, LifecycleStatus.Started, e );
						 }
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws LifecycleException
			  public override void Stop()
			  {
					if ( CurrentStatus == LifecycleStatus.Started )
					{
						 CurrentStatus = outerInstance.changedStatus( Instance, CurrentStatus, LifecycleStatus.Stopping );
						 try
						 {
							  Instance.stop();
						 }
						 catch ( LifecycleException e )
						 {
							  throw e;
						 }
						 catch ( Exception e )
						 {
							  throw new LifecycleException( Instance, LifecycleStatus.Started, LifecycleStatus.Stopped, e );
						 }
						 finally
						 {
							  CurrentStatus = outerInstance.changedStatus( Instance, CurrentStatus, LifecycleStatus.Stopped );
						 }
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws LifecycleException
			  public override void Shutdown()
			  {
					if ( CurrentStatus == LifecycleStatus.Started )
					{
						 Stop();
					}

					if ( CurrentStatus == LifecycleStatus.Stopped )
					{
						 CurrentStatus = outerInstance.changedStatus( Instance, CurrentStatus, LifecycleStatus.ShuttingDown );
						 try
						 {
							  Instance.shutdown();
						 }
						 catch ( LifecycleException e )
						 {
							  throw e;
						 }
						 catch ( Exception e )
						 {
							  throw new LifecycleException( Instance, LifecycleStatus.Stopped, LifecycleStatus.ShuttingDown, e );
						 }
						 finally
						 {
							  CurrentStatus = outerInstance.changedStatus( Instance, CurrentStatus, LifecycleStatus.Shutdown );
						 }
					}
			  }

			  public override string ToString()
			  {
					return Instance.ToString() + ": " + CurrentStatus.name();
			  }

			  public virtual bool IsInstance( Lifecycle instance )
			  {
					return this.Instance == instance;
			  }
		 }
	}

}