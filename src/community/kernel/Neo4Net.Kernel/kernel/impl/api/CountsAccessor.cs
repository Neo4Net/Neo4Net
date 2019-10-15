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
	using Register_DoubleLongRegister = Neo4Net.Register.Register_DoubleLongRegister;

	public interface CountsAccessor : CountsVisitor_Visitable
	{
		 /// <param name="target"> a register to store the read values in </param>
		 /// <returns> the input register for convenience </returns>
		 Register_DoubleLongRegister NodeCount( int labelId, Register_DoubleLongRegister target );

		 /// <param name="target"> a register to store the read values in </param>
		 /// <returns> the input register for convenience </returns>
		 Register_DoubleLongRegister RelationshipCount( int startLabelId, int typeId, int endLabelId, Register_DoubleLongRegister target );

		 /// <param name="target"> a register to store the read values in </param>
		 /// <returns> the input register for convenience </returns>
		 Register_DoubleLongRegister IndexUpdatesAndSize( long indexId, Register_DoubleLongRegister target );

		 /// <param name="target"> a register to store the read values in </param>
		 /// <returns> the input register for convenience </returns>
		 Register_DoubleLongRegister IndexSample( long indexId, Register_DoubleLongRegister target );
	}

	 public interface CountsAccessor_Updater : IDisposable
	 {
		  void IncrementNodeCount( long labelId, long delta );

		  void IncrementRelationshipCount( long startLabelId, int typeId, long endLabelId, long delta );

		  void Close();
	 }

	 public interface CountsAccessor_IndexStatsUpdater : IDisposable
	 {
		  void ReplaceIndexUpdateAndSize( long indexId, long updates, long size );

		  void ReplaceIndexSample( long indexId, long unique, long size );

		  void IncrementIndexUpdates( long indexId, long delta );

		  void Close();
	 }

	 public sealed class CountsAccessor_Initializer : CountsVisitor
	 {
		  internal readonly CountsAccessor_Updater Updater;
		  internal readonly CountsAccessor_IndexStatsUpdater Stats;

		  public CountsAccessor_Initializer( CountsAccessor_Updater updater, CountsAccessor_IndexStatsUpdater stats )
		  {
				this.Updater = updater;
				this.Stats = stats;
		  }

		  public override void VisitNodeCount( int labelId, long count )
		  {
				Updater.incrementNodeCount( labelId, count );
		  }

		  public override void VisitRelationshipCount( int startLabelId, int typeId, int endLabelId, long count )
		  {
				Updater.incrementRelationshipCount( startLabelId, typeId, endLabelId, count );
		  }

		  public override void VisitIndexStatistics( long indexId, long updates, long size )
		  {
				Stats.replaceIndexUpdateAndSize( indexId, updates, size );
		  }

		  public override void VisitIndexSample( long indexId, long unique, long size )
		  {
				Stats.replaceIndexSample( indexId, unique, size );
		  }
	 }

}