/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.builtinprocs
{
	using Test = org.junit.Test;

	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using IndexSamplingMode = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class ResampleOutdatedIndexesProcedureTest
	{
		private bool InstanceFieldsInitialized = false;

		public ResampleOutdatedIndexesProcedureTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_procedure = new IndexProcedures( new StubKernelTransaction(), _indexingService );
		}

		 private readonly IndexingService _indexingService = mock( typeof( IndexingService ) );
		 private IndexProcedures _procedure;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTriggerResampling()
		 public virtual void ShouldTriggerResampling()
		 {
			  _procedure.resampleOutdatedIndexes();

			  verify( _indexingService ).triggerIndexSampling( IndexSamplingMode.TRIGGER_REBUILD_UPDATED );
		 }
	}

}