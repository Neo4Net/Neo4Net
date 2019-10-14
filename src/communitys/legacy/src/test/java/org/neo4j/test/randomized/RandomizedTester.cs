using System.Collections.Generic;

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
namespace Neo4Net.Test.randomized
{

	/// <summary>
	/// Randomized tester, where it's given a target factory, i.e. a factory for creating the object
	/// participating in all actions. I.e. the subject of this randomized test. It's also given an action factory,
	/// i.e. a factory for the actions to execute on the target.
	/// <para>
	/// An <seealso cref="Action"/> can report if there an error happened, and if so the test halts indicating the order
	/// of the action that failed. From there a the <seealso cref="findMinimalReproducible() minimal reproducible list of actions"/>
	/// can be figured out and <seealso cref="testCaseWriter(string, Printable) test case"/> can be written.
	/// 
	/// </para>
	/// </summary>
	/// @param <T> type of target for the actions, i.e. the subject under test here. </param>
	/// @param <F> type of failure an <seealso cref="Action"/> may produce. </param>
	public class RandomizedTester<T, F> where T : TestResource
	{
		 private readonly IList<Action<T, F>> _givenActions = new List<Action<T, F>>();
		 private readonly TargetFactory<T> _targetFactory; // will be used later for finding a minimal test case
		 private readonly ActionFactory<T, F> _actionFactory;
		 private Action<T, F> _failingAction;
		 private F _failure;

		 public RandomizedTester( TargetFactory<T> targetFactory, ActionFactory<T, F> actionFactory ) : this( targetFactory, actionFactory, null, null )
		 {
		 }

		 private RandomizedTester( TargetFactory<T> targetFactory, ActionFactory<T, F> actionFactory, Action<T, F> failingAction, F failure )
		 {
			  this._targetFactory = targetFactory;
			  this._actionFactory = actionFactory;
			  this._failingAction = failingAction;
			  this._failure = failure;
		 }

		 /// <returns> the index of the failing action, or -1 if successful. </returns>
		 public virtual Result<T, F> Run( int numberOfActions )
		 {
			  using ( T target = _targetFactory.newInstance() )
			  {
					for ( int i = 0; i < numberOfActions; i++ )
					{
						 // Create a new action
						 Action<T, F> action = _actionFactory.apply( target );

						 // Have the effects of it applied to the target
						 F failure = action.Apply( target );
						 if ( failure != default( F ) )
						 { // Something went wrong.
							  this._failingAction = action;
							  this._failure = failure;
							  return new Result<T, F>( target, i, failure );
						 }

						 // Add it to the list of actions performed
						 _givenActions.Add( action );
					}

					// Full verification
					return new Result<T, F>( target, -1, _failure );
			  }
		 }

		 /// <summary>
		 /// Starts with the existing list of actions that were produced by <seealso cref="run(int)"/>, trying to prune actions
		 /// from that list, while still being able to reproduce the exact same failure. The result is a new
		 /// <seealso cref="RandomizedTester"/> instance. with a potentially reduced list of actions.
		 /// </summary>
		 /// <returns> a reduced list of actions to reproduce the failure. </returns>
		 public virtual RandomizedTester<T, F> FindMinimalReproducible()
		 {
			  RandomizedTester<T, F> minimal = this;
			  while ( true )
			  {
					RandomizedTester<T, F> candidate = minimal.ReduceOneAction();
					if ( candidate == minimal )
					{
						 return candidate;
					}
					minimal = candidate;
			  }
		 }

		 private RandomizedTester<T, F> ReduceOneAction()
		 {
			  int numberOfIterations = _givenActions.Count;
			  if ( numberOfIterations == 1 )
			  {
					return this;
			  }
			  for ( int actionToSkip = 0; actionToSkip < _givenActions.Count; actionToSkip++ )
			  {
					RandomizedTester<T, F> reducedActions = new RandomizedTester<T, F>( _targetFactory, ActionFactoryThatSkipsOneAction( _givenActions.GetEnumerator(), actionToSkip, _failingAction ), _failingAction, _failure );
					Result<T, F> result = reducedActions.Run( numberOfIterations - 1 );
					if ( result.Failure && result.Index == _givenActions.Count - 1 && result.Failure.Equals( _failure ) )
					{
						 return reducedActions;
					}
			  }
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private ActionFactory<T, F> actionFactoryThatSkipsOneAction(final java.util.Iterator<Action<T, F>> iterator, final int actionToSkip, final Action<T, F> failingAction)
		 private ActionFactory<T, F> ActionFactoryThatSkipsOneAction( IEnumerator<Action<T, F>> iterator, int actionToSkip, Action<T, F> failingAction )
		 {
			  return new ActionFactoryAnonymousInnerClass( this, iterator, actionToSkip, failingAction );
		 }

		 private class ActionFactoryAnonymousInnerClass : ActionFactory<T, F>
		 {
			 private readonly RandomizedTester<T, F> _outerInstance;

			 private IEnumerator<Action<T, F>> _iterator;
			 private int _actionToSkip;
			 private Neo4Net.Test.randomized.Action<T, F> _failingAction;

			 public ActionFactoryAnonymousInnerClass( RandomizedTester<T, F> outerInstance, IEnumerator<Action<T, F>> iterator, int actionToSkip, Neo4Net.Test.randomized.Action<T, F> failingAction )
			 {
				 this.outerInstance = outerInstance;
				 this._iterator = iterator;
				 this._actionToSkip = actionToSkip;
				 this._failingAction = failingAction;
			 }

			 private int index;
			 private bool failingActionReturned;

			 public Action<T, F> apply( T from )
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  if ( _iterator.hasNext() )
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						Action<T, F> action = _iterator.next();
						return index++ == _actionToSkip ? apply( from ) : action;
				  }

				  if ( failingActionReturned )
				  {
						throw new System.InvalidOperationException();
				  }
				  failingActionReturned = true;
				  return _failingAction;
			 }
		 }

		 public virtual TestCaseWriter<T, F> TestCaseWriter( string name, Printable given )
		 {
			  return new TestCaseWriter<T, F>( name, given, _targetFactory, _givenActions, _failingAction );
		 }

		 public virtual F Failure()
		 {
			  return _failure;
		 }

		 public interface TargetFactory<T>
		 {
			  T NewInstance();
		 }

		 public interface ActionFactory<T, F>
		 {
			  Action<T, F> Apply( T from );
		 }
	}

}