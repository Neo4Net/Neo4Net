﻿using System.Collections.Generic;
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
namespace Neo4Net.Kernel.impl.store
{

	using CountsAccessor = Neo4Net.Kernel.Impl.Api.CountsAccessor;
	using CountsRecordState = Neo4Net.Kernel.Impl.Api.CountsRecordState;
	using CountsVisitor = Neo4Net.Kernel.Impl.Api.CountsVisitor;
	using CountsTracker = Neo4Net.Kernel.impl.store.counts.CountsTracker;
	using Register = Neo4Net.Register.Register;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.register.Registers.newDoubleLongRegister;

	public class CountsOracle
	{
		 public class Node
		 {
			  internal readonly long[] Labels;

			  internal Node( long[] labels )
			  {
					this.Labels = labels;
			  }
		 }

		 private readonly CountsRecordState _state = new CountsRecordState();

		 public virtual Node Node( params long[] labels )
		 {
			  _state.addNode( labels );
			  return new Node( labels );
		 }

		 public virtual void Relationship( Node start, int type, Node end )
		 {
			  _state.addRelationship( start.Labels, type, end.Labels );
		 }

		 public virtual void IndexUpdatesAndSize( long indexId, long updates, long size )
		 {
			  _state.replaceIndexUpdateAndSize( indexId, updates, size );
		 }

		 public virtual void IndexSampling( long indexId, long unique, long size )
		 {
			  _state.replaceIndexSample( indexId, unique, size );
		 }

		 public virtual void Update( CountsTracker target, long txId )
		 {
			  using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater updater = target.Apply( txId ).get(), Neo4Net.Kernel.Impl.Api.CountsAccessor_IndexStatsUpdater stats = target.UpdateIndexCounts() )
			  {
					_state.accept( new Neo4Net.Kernel.Impl.Api.CountsAccessor_Initializer( updater, stats ) );
			  }
		 }

		 public virtual void Update( CountsOracle target )
		 {
			  _state.accept( new Neo4Net.Kernel.Impl.Api.CountsAccessor_Initializer( target._state, target._state ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public <Tracker extends org.neo4j.kernel.impl.api.CountsVisitor.Visitable & org.neo4j.kernel.impl.api.CountsAccessor> void verify(final Tracker tracker)
		 public virtual void Verify<Tracker>( Tracker tracker ) where Tracker : Neo4Net.Kernel.Impl.Api.CountsVisitor.Visitable, Neo4Net.Kernel.Impl.Api.CountsAccessor
		 {
			  CountsRecordState seenState = new CountsRecordState();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.api.CountsAccessor_Initializer initializer = new org.neo4j.kernel.impl.api.CountsAccessor_Initializer(seenState, seenState);
			  Neo4Net.Kernel.Impl.Api.CountsAccessor_Initializer initializer = new Neo4Net.Kernel.Impl.Api.CountsAccessor_Initializer( seenState, seenState );
			  IList<CountsRecordState.Difference> differences = _state.verify( verifier => tracker.accept( Neo4Net.Kernel.Impl.Api.CountsVisitor_Adapter.Multiplex( initializer, verifier ) ) );
			  seenState.Accept( new CountsVisitorAnonymousInnerClass( this, tracker ) );
			  if ( differences.Count > 0 )
			  {
					StringBuilder errors = ( new StringBuilder() ).Append("Counts differ in ").Append(differences.Count).Append(" places...");
					foreach ( CountsRecordState.Difference difference in differences )
					{
						 errors.Append( "\n\t" ).Append( difference );
					}
					throw new AssertionError( errors.ToString() );
			  }
		 }

		 private class CountsVisitorAnonymousInnerClass : CountsVisitor
		 {
			 private readonly CountsOracle _outerInstance;

			 private Neo4Net.Kernel.Impl.Api.CountsVisitor_Visitable _tracker;

			 public CountsVisitorAnonymousInnerClass( CountsOracle outerInstance, Neo4Net.Kernel.Impl.Api.CountsVisitor_Visitable tracker )
			 {
				 this.outerInstance = outerInstance;
				 this._tracker = tracker;
			 }

			 public void visitNodeCount( int labelId, long count )
			 {
				  long expected = _tracker.nodeCount( labelId, newDoubleLongRegister() ).readSecond();
				  assertEquals( "Should be able to read visited state.", expected, count );
			 }

			 public void visitRelationshipCount( int startLabelId, int typeId, int endLabelId, long count )
			 {
				  long expected = _tracker.relationshipCount( startLabelId, typeId, endLabelId, newDoubleLongRegister() ).readSecond();
				  assertEquals( "Should be able to read visited state.", expected, count );
			 }

			 public void visitIndexStatistics( long indexId, long updates, long size )
			 {
				  Neo4Net.Register.Register_DoubleLongRegister output = _tracker.indexUpdatesAndSize( indexId, newDoubleLongRegister() );
				  assertEquals( "Should be able to read visited state.", output.ReadFirst(), updates );
				  assertEquals( "Should be able to read visited state.", output.ReadSecond(), size );
			 }

			 public void visitIndexSample( long indexId, long unique, long size )
			 {
				  Neo4Net.Register.Register_DoubleLongRegister output = _tracker.indexSample( indexId, newDoubleLongRegister() );
				  assertEquals( "Should be able to read visited state.", output.ReadFirst(), unique );
				  assertEquals( "Should be able to read visited state.", output.ReadSecond(), size );
			 }
		 }
	}

}