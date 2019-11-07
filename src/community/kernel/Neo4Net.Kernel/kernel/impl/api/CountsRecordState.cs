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
namespace Neo4Net.Kernel.Impl.Api
{

	using RecordState = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordState;
	using CountsKey = Neo4Net.Kernel.impl.store.counts.keys.CountsKey;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using Register_DoubleLongRegister = Neo4Net.Register.Register_DoubleLongRegister;
	using Registers = Neo4Net.Register.Registers;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.StatementConstants.ANY_LABEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.StatementConstants.ANY_RELATIONSHIP_TYPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.counts.keys.CountsKeyFactory.indexSampleKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.counts.keys.CountsKeyFactory.indexStatisticsKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.counts.keys.CountsKeyFactory.nodeKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.counts.keys.CountsKeyFactory.relationshipKey;

	public class CountsRecordState : CountsAccessor, RecordState, CountsAccessor_Updater, CountsAccessor_IndexStatsUpdater
	{
		 private const long DEFAULT_FIRST_VALUE = 0;
		 private const long DEFAULT_SECOND_VALUE = 0;
		 private readonly IDictionary<CountsKey, Register_DoubleLongRegister> _counts = new Dictionary<CountsKey, Register_DoubleLongRegister>();

		 public override Register_DoubleLongRegister NodeCount( int labelId, Register_DoubleLongRegister target )
		 {
			  Counts( nodeKey( labelId ) ).copyTo( target );
			  return target;
		 }

		 public override void IncrementNodeCount( long labelId, long delta )
		 {
			  Counts( nodeKey( labelId ) ).increment( 0L, delta );
		 }

		 public override Register_DoubleLongRegister RelationshipCount( int startLabelId, int typeId, int endLabelId, Register_DoubleLongRegister target )
		 {
			  Counts( relationshipKey( startLabelId, typeId, endLabelId ) ).copyTo( target );
			  return target;
		 }

		 public override Register_DoubleLongRegister IndexSample( long indexId, Register_DoubleLongRegister target )
		 {
			  Counts( indexSampleKey( indexId ) ).copyTo( target );
			  return target;
		 }

		 public override void IncrementRelationshipCount( long startLabelId, int typeId, long endLabelId, long delta )
		 {
			  if ( delta != 0 )
			  {
					Counts( relationshipKey( startLabelId, typeId, endLabelId ) ).increment( 0L, delta );
			  }
		 }

		 public override Register_DoubleLongRegister IndexUpdatesAndSize( long indexId, Register_DoubleLongRegister target )
		 {
			  Counts( indexStatisticsKey( indexId ) ).copyTo( target );
			  return target;
		 }

		 public override void ReplaceIndexUpdateAndSize( long indexId, long updates, long size )
		 {
			  Counts( indexStatisticsKey( indexId ) ).write( updates, size );
		 }

		 public override void IncrementIndexUpdates( long indexId, long delta )
		 {
			  Counts( indexStatisticsKey( indexId ) ).increment( delta, 0L );
		 }

		 public override void ReplaceIndexSample( long indexId, long unique, long size )
		 {
			  Counts( indexSampleKey( indexId ) ).write( unique, size );
		 }

		 public override void Close()
		 {
			  // this is close() of CountsAccessor.Updater - do nothing.
		 }

		 public override void Accept( CountsVisitor visitor )
		 {
			  foreach ( KeyValuePair<CountsKey, Register_DoubleLongRegister> entry in _counts.SetOfKeyValuePairs() )
			  {
					Register_DoubleLongRegister register = entry.Value;
					entry.Key.accept( visitor, register.ReadFirst(), register.ReadSecond() );
			  }
		 }

		 public override void ExtractCommands( ICollection<StorageCommand> target )
		 {
			  Accept( new CommandCollector( target ) );
		 }

		 public virtual IList<Difference> Verify( CountsVisitor_Visitable visitable )
		 {
			  Verifier verifier = new Verifier( _counts );
			  visitable.Accept( verifier );
			  return verifier.Differences();
		 }

		 public override bool HasChanges()
		 {
			  return _counts.Count > 0;
		 }

		 public sealed class Difference
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly CountsKey KeyConflict;
			  internal readonly long ExpectedFirst;
			  internal readonly long ExpectedSecond;
			  internal readonly long ActualFirst;
			  internal readonly long ActualSecond;

			  public Difference( CountsKey key, long expectedFirst, long expectedSecond, long actualFirst, long actualSecond )
			  {
					this.ExpectedFirst = expectedFirst;
					this.ExpectedSecond = expectedSecond;
					this.ActualFirst = actualFirst;
					this.ActualSecond = actualSecond;
					this.KeyConflict = requireNonNull( key, "key" );
			  }

			  public override string ToString()
			  {
					return string.Format( "{0}[{1} expected={2:D}:{3:D}, actual={4:D}:{5:D}]", this.GetType().Name, KeyConflict, ExpectedFirst, ExpectedSecond, ActualFirst, ActualSecond );
			  }

			  public CountsKey Key()
			  {
					return KeyConflict;
			  }

			  public override bool Equals( object obj )
			  {
					if ( this == obj )
					{
						 return true;
					}
					if ( obj is Difference )
					{
						 Difference that = ( Difference ) obj;
						 return ActualFirst == that.ActualFirst && ExpectedFirst == that.ExpectedFirst && ActualSecond == that.ActualSecond && ExpectedSecond == that.ExpectedSecond && KeyConflict.Equals( that.KeyConflict );
					}
					return false;
			  }

			  public override int GetHashCode()
			  {
					int result = KeyConflict.GetHashCode();
					result = 31 * result + ( int )( ExpectedFirst ^ ( ( long )( ( ulong )ExpectedFirst >> 32 ) ) );
					result = 31 * result + ( int )( ExpectedSecond ^ ( ( long )( ( ulong )ExpectedSecond >> 32 ) ) );
					result = 31 * result + ( int )( ActualFirst ^ ( ( long )( ( ulong )ActualFirst >> 32 ) ) );
					result = 31 * result + ( int )( ActualSecond ^ ( ( long )( ( ulong )ActualSecond >> 32 ) ) );
					return result;
			  }
		 }

		 public virtual void AddNode( long[] labels )
		 {
			  IncrementNodeCount( ANY_LABEL, 1 );
			  foreach ( long label in labels )
			  {
					IncrementNodeCount( ( int ) label, 1 );
			  }
		 }

		 public virtual void AddRelationship( long[] startLabels, int type, long[] endLabels )
		 {
			  IncrementRelationshipCount( ANY_LABEL, ANY_RELATIONSHIP_TYPE, ANY_LABEL, 1 );
			  IncrementRelationshipCount( ANY_LABEL, type, ANY_LABEL, 1 );
			  foreach ( long startLabelId in startLabels )
			  {
					IncrementRelationshipCount( ( int ) startLabelId, ANY_RELATIONSHIP_TYPE, ANY_LABEL, 1 );
					IncrementRelationshipCount( ( int ) startLabelId, type, ANY_LABEL, 1 );
			  }
			  foreach ( long endLabelId in endLabels )
			  {
					IncrementRelationshipCount( ANY_LABEL, ANY_RELATIONSHIP_TYPE, ( int ) endLabelId, 1 );
					IncrementRelationshipCount( ANY_LABEL, type, ( int ) endLabelId, 1 );
			  }
		 }

		 private Register_DoubleLongRegister Counts( CountsKey key )
		 {
			  return _counts.computeIfAbsent( key, k => Registers.newDoubleLongRegister( DEFAULT_FIRST_VALUE, DEFAULT_SECOND_VALUE ) );
		 }

		 private class CommandCollector : CountsVisitor_Adapter
		 {
			  internal readonly ICollection<StorageCommand> Commands;

			  internal CommandCollector( ICollection<StorageCommand> commands )
			  {
					this.Commands = commands;
			  }

			  public override void VisitNodeCount( int labelId, long count )
			  {
					if ( count != 0 )
					{ // Only add commands for counts that actually change
						 Commands.Add( new Command.NodeCountsCommand( labelId, count ) );
					}
			  }

			  public override void VisitRelationshipCount( int startLabelId, int typeId, int endLabelId, long count )
			  {
					if ( count != 0 )
					{ // Only add commands for counts that actually change
						 Commands.Add( new Command.RelationshipCountsCommand( startLabelId, typeId, endLabelId, count ) );
					}
			  }
		 }

		 private class Verifier : CountsVisitor
		 {
			  internal readonly IDictionary<CountsKey, Register_DoubleLongRegister> Counts;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IList<Difference> DifferencesConflict = new List<Difference>();

			  internal Verifier( IDictionary<CountsKey, Register_DoubleLongRegister> counts )
			  {
					this.Counts = new Dictionary<CountsKey, Register_DoubleLongRegister>( counts );
			  }

			  public override void VisitNodeCount( int labelId, long count )
			  {
					Verify( nodeKey( labelId ), 0, count );
			  }

			  public override void VisitRelationshipCount( int startLabelId, int typeId, int endLabelId, long count )
			  {
					Verify( relationshipKey( startLabelId, typeId, endLabelId ), 0, count );
			  }
			  public override void VisitIndexStatistics( long indexId, long updates, long size )
			  {
					Verify( indexStatisticsKey( indexId ), updates, size );
			  }

			  public override void VisitIndexSample( long indexId, long unique, long size )
			  {
					Verify( indexSampleKey( indexId ), unique, size );
			  }

			  internal virtual void Verify( CountsKey key, long actualFirst, long actualSecond )
			  {
					Register_DoubleLongRegister expected = Counts.Remove( key );
					if ( expected == null )
					{
						 if ( actualFirst != 0 || actualSecond != 0 )
						 {
							  DifferencesConflict.Add( new Difference( key, 0, 0, actualFirst, actualSecond ) );
						 }
					}
					else
					{
						 long expectedFirst = expected.ReadFirst();
						 long expectedSecond = expected.ReadSecond();
						 if ( expectedFirst != actualFirst || expectedSecond != actualSecond )
						 {
							  DifferencesConflict.Add( new Difference( key, expectedFirst, expectedSecond, actualFirst, actualSecond ) );
						 }
					}
			  }

			  public virtual IList<Difference> Differences()
			  {
					foreach ( KeyValuePair<CountsKey, Register_DoubleLongRegister> entry in Counts.SetOfKeyValuePairs() )
					{
						 Register_DoubleLongRegister value = entry.Value;
						 DifferencesConflict.Add( new Difference( entry.Key, value.ReadFirst(), value.ReadSecond(), 0, 0 ) );
					}
					Counts.Clear();
					return DifferencesConflict;
			  }
		 }
	}

}