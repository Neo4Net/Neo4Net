using System;
using System.Collections.Generic;

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
namespace Neo4Net.ha.correctness
{

	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using Neo4Net.Helpers.Collection;
	using Neo4Net.Helpers.Collection;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.filter;

	/// <summary>
	/// A picture of the state of the cluster, including all messages waiting to get delivered.
	/// Important note: ClusterStates are equal on the states of the instances, not on the pending messages. Two states
	/// with the same cluster states but different pending messages will be considered equal.
	/// </summary>
	internal class ClusterState
	{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 private static readonly System.Predicate<ClusterInstance> _hasTimeouts = ClusterInstance::hasPendingTimeouts;
		 private readonly ISet<ClusterAction> _pendingActions;
		 private readonly IList<ClusterInstance> _instances = new List<ClusterInstance>();

		 internal ClusterState( IList<ClusterInstance> instances, ISet<ClusterAction> pendingActions )
		 {
			  this._pendingActions = pendingActions is LinkedHashSet ? pendingActions : new LinkedHashSet<ClusterAction>( pendingActions );
			  ( ( IList<ClusterInstance> )this._instances ).AddRange( instances );
		 }

		 public virtual void AddPendingActions( params ClusterAction[] actions )
		 {
			  _pendingActions.addAll( Arrays.asList( actions ) );
		 }

		 /// <summary>
		 /// All possible new cluster states that can be generated from this one. </summary>
		 public virtual IEnumerator<Pair<ClusterAction, ClusterState>> Transitions()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Iterator<ClusterAction> actions = pendingActions.iterator();
			  IEnumerator<ClusterAction> actions = _pendingActions.GetEnumerator();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Iterator<ClusterInstance> instancesWithTimeouts = filter(HAS_TIMEOUTS, instances).iterator();
			  IEnumerator<ClusterInstance> instancesWithTimeouts = filter( _hasTimeouts, _instances ).GetEnumerator();
			  return new PrefetchingIteratorAnonymousInnerClass( this, actions, instancesWithTimeouts );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<Pair<ClusterAction, ClusterState>>
		 {
			 private readonly ClusterState _outerInstance;

			 private IEnumerator<ClusterAction> _actions;
			 private IEnumerator<ClusterInstance> _instancesWithTimeouts;

			 public PrefetchingIteratorAnonymousInnerClass( ClusterState outerInstance, IEnumerator<ClusterAction> actions, IEnumerator<ClusterInstance> instancesWithTimeouts )
			 {
				 this.outerInstance = outerInstance;
				 this._actions = actions;
				 this._instancesWithTimeouts = instancesWithTimeouts;
			 }

			 protected internal override Pair<ClusterAction, ClusterState> fetchNextOrNull()
			 {
				  try
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						if ( _actions.hasNext() )
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 ClusterAction action = _actions.next();
							 return Pair.of( action, outerInstance.PerformAction( action ) );
						}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						else if ( _instancesWithTimeouts.hasNext() )
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 ClusterInstance instance = _instancesWithTimeouts.next();
							 return outerInstance.performNextTimeoutFrom( instance );
						}
						else
						{
							 return null;
						}
				  }
				  catch ( Exception e )
				  {
						throw new Exception( e );
				  }
			 }
		 }

		 /// <summary>
		 /// Managing timeouts is trickier than putting all of them in a long list, like regular message delivery.
		 ///  Timeouts are ordered and can be cancelled, so they need special treatment. Hence a separate method for
		 ///  managing timeouts triggering. 
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.helpers.collection.Pair<ClusterAction, ClusterState> performNextTimeoutFrom(ClusterInstance instance) throws Exception
		 private Pair<ClusterAction, ClusterState> PerformNextTimeoutFrom( ClusterInstance instance )
		 {
			  ClusterState newState = Snapshot();
			  ClusterAction clusterAction = newState.Instance( instance.Uri().toASCIIString() ).popTimeout();
			  clusterAction.Perform( newState );

			  return Pair.of( clusterAction, newState );
		 }

		 /// <summary>
		 /// Clone the state and perform the action with the provided index. Returns the new state and the action. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ClusterState performAction(ClusterAction action) throws Exception
		 internal virtual ClusterState PerformAction( ClusterAction action )
		 {
			  ClusterState newState = Snapshot();

			  // Remove the action from the list of things that can happen in the snapshot
			  newState._pendingActions.remove( action );

			  // Perform the action on the cloned state
			  IEnumerable<ClusterAction> newActions = action.Perform( newState );

			  // Include any outcome actions into the new state snapshot
			  newState._pendingActions.addAll( Iterables.asCollection( newActions ) );

			  return newState;
		 }

		 public virtual ClusterState Snapshot()
		 {
			  ISet<ClusterAction> newPendingActions = new LinkedHashSet<ClusterAction>( _pendingActions );

			  // Clone the current state & perform the action on it to change it
			  IList<ClusterInstance> cloneInstances = new List<ClusterInstance>();
			  foreach ( ClusterInstance clusterInstance in _instances )
			  {
					cloneInstances.Add( clusterInstance.NewCopy() );
			  }

			  return new ClusterState( cloneInstances, newPendingActions );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ClusterInstance instance(String to) throws java.net.URISyntaxException
		 public virtual ClusterInstance Instance( string to )
		 {
			  URI uri = new URI( to );
			  foreach ( ClusterInstance clusterInstance in _instances )
			  {
					URI instanceUri = clusterInstance.Uri();
					if ( instanceUri.Host.Equals( uri.Host ) && instanceUri.Port == uri.Port )
					{
						 return clusterInstance;
					}
			  }

			  throw new System.ArgumentException( "No instance in cluster at address: " + to );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  ClusterState that = ( ClusterState ) o;

//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: return instances.equals(that.instances) && pendingActions.equals(that.pendingActions);
			  return _instances.SequenceEqual( that._instances ) && _pendingActions.SetEquals( that._pendingActions );
		 }

		 public override int GetHashCode()
		 {
			  int result = _instances.GetHashCode();
			  result = 31 * result + _pendingActions.GetHashCode();
			  return result;
		 }

		 public override string ToString()
		 {
			  return "Cluster[" + Iterables.ToString( _instances, ", " ) + "]";
		 }

		 public virtual bool DeadEnd
		 {
			 get
			 {
				  if ( _pendingActions.Count > 0 )
				  {
						return false;
				  }
   
				  foreach ( ClusterInstance instance in _instances )
				  {
						if ( instance.HasPendingTimeouts() )
						{
							 return false;
						}
				  }
   
				  return true;
			 }
		 }
	}

}