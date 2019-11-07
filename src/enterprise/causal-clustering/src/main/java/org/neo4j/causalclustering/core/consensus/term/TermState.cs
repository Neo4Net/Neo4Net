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
namespace Neo4Net.causalclustering.core.consensus.term
{

	using Neo4Net.causalclustering.core.state.storage;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

	public class TermState
	{
		 private volatile long _term;

		 public TermState()
		 {
		 }

		 private TermState( long term )
		 {
			  this._term = term;
		 }

		 public virtual long CurrentTerm()
		 {
			  return _term;
		 }

		 /// <summary>
		 /// Updates the term to a new value. This value is generally expected, but not required, to be persisted. Consecutive
		 /// calls to this method should always have monotonically increasing arguments, thus maintaining the raft invariant
		 /// that the term is always non-decreasing. <seealso cref="System.ArgumentException"/> can be thrown if an invalid value is
		 /// passed as argument.
		 /// </summary>
		 /// <param name="newTerm"> The new value. </param>
		 public virtual bool Update( long newTerm )
		 {
			  FailIfInvalid( newTerm );
			  bool changed = _term != newTerm;
			  _term = newTerm;
			  return changed;
		 }

		 /// <summary>
		 /// This method implements the invariant of this class, that term never transitions to lower values. If
		 /// newTerm is lower than the term already stored in this class, it will throw an
		 /// <seealso cref="System.ArgumentException"/>.
		 /// </summary>
		 private void FailIfInvalid( long newTerm )
		 {
			  if ( newTerm < _term )
			  {
					throw new System.ArgumentException( "Cannot move to a lower term" );
			  }
		 }

		 public class Marshal : SafeStateMarshal<TermState>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(TermState termState, Neo4Net.Kernel.Api.StorageEngine.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( TermState termState, WritableChannel channel )
			  {
					channel.PutLong( termState.CurrentTerm() );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected TermState unmarshal0(Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException
			  protected internal override TermState Unmarshal0( ReadableChannel channel )
			  {
					return new TermState( channel.Long );
			  }

			  public override TermState StartState()
			  {
					return new TermState();
			  }

			  public override long Ordinal( TermState state )
			  {
					return state.CurrentTerm();
			  }
		 }

		 public override string ToString()
		 {
			  return "TermState{" +
						"term=" + _term +
						'}';
		 }
	}

}