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
namespace Neo4Net.GraphDb.Traversal
{
	using Neo4Net.GraphDb;

	/// <summary>
	/// Factory for initial state of <seealso cref="TraversalBranch"/>es in a traversal.
	/// </summary>
	/// @param <STATE> type of initial state to produce. </param>
	public interface InitialBranchState<STATE>
	{
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 @@SuppressWarnings("rawtypes") InitialBranchState NO_STATE = new InitialBranchState()
	//	 {
	//		  @@Override public Object initialState(Path path)
	//		  {
	//				return null;
	//		  }
	//
	//		  @@Override public InitialBranchState reverse()
	//		  {
	//				return this;
	//		  }
	//	 };

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 InitialBranchState<double> DOUBLE_ZERO = new InitialBranchState<double>()
	//	 {
	//		  @@Override public System.Nullable<double> initialState(Path path)
	//		  {
	//				return 0d;
	//		  }
	//
	//		  @@Override public InitialBranchState<double> reverse()
	//		  {
	//				return this;
	//		  }
	//	 };

		 /// <summary>
		 /// Returns an initial state for a <seealso cref="Path"/>. All paths entering this method
		 /// are start paths(es) of a traversal. State is passed down along traversal
		 /// branches as the traversal progresses and can be changed at any point by a
		 /// <seealso cref="PathExpander"/> to becomes the new state from that point in that branch
		 /// and downwards.
		 /// </summary>
		 /// <param name="path"> the start branch to return the initial state for. </param>
		 /// <returns> an initial state for the traversal branch. </returns>
		 STATE InitialState( IPath path );

		 /// <summary>
		 /// Creates a version of this state factory which produces reversed initial state,
		 /// used in bidirectional traversals. </summary>
		 /// <returns> an instance which produces reversed initial state. </returns>
		 InitialBranchState<STATE> Reverse();

		 /// <summary>
		 /// Branch state evaluator for an initial state.
		 /// </summary>
	}

	 public abstract class InitialBranchState_Adapter<STATE> : InitialBranchState<STATE>
	 {
		 public abstract STATE InitialState( IPath path );
		  public override InitialBranchState<STATE> Reverse()
		  {
				return this;
		  }
	 }

	 public class InitialBranchState_State<STATE> : InitialBranchState_Adapter<STATE>
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly STATE InitialStateConflict;
		  internal readonly STATE ReversedInitialState;

		  public InitialBranchState_State( STATE initialState, STATE reversedInitialState )
		  {
				this.InitialStateConflict = initialState;
				this.ReversedInitialState = reversedInitialState;
		  }

		  public override InitialBranchState<STATE> Reverse()
		  {
				return new InitialBranchState_State<STATE>( ReversedInitialState, InitialStateConflict );
		  }

		  public override STATE InitialState( IPath path )
		  {
				return InitialStateConflict;
		  }
	 }

}