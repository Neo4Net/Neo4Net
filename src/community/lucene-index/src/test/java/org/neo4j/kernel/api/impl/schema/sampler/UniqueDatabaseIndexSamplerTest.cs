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
namespace Neo4Net.Kernel.Api.Impl.Schema.sampler
{
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Test = org.junit.jupiter.api.Test;
	using Mockito = org.mockito.Mockito;

	using TaskCoordinator = Neo4Net.Helpers.TaskCoordinator;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class UniqueDatabaseIndexSamplerTest
	{
		 private readonly IndexSearcher _indexSearcher = mock( typeof( IndexSearcher ), Mockito.RETURNS_DEEP_STUBS );
		 private readonly TaskCoordinator _taskControl = new TaskCoordinator( 0, TimeUnit.MILLISECONDS );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uniqueSamplingUseDocumentsNumber() throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void UniqueSamplingUseDocumentsNumber()
		 {
			  when( _indexSearcher.IndexReader.numDocs() ).thenReturn(17);

			  UniqueLuceneIndexSampler sampler = new UniqueLuceneIndexSampler( _indexSearcher, _taskControl.newInstance() );
			  IndexSample sample = sampler.SampleIndex();
			  assertEquals( 17, sample.IndexSize() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void uniqueSamplingCancel()
		 internal virtual void UniqueSamplingCancel()
		 {
			  when( _indexSearcher.IndexReader.numDocs() ).thenAnswer(invocation =>
			  {
				_taskControl.cancel();
				return 17;
			  });

			  UniqueLuceneIndexSampler sampler = new UniqueLuceneIndexSampler( _indexSearcher, _taskControl.newInstance() );
			  IndexNotFoundKernelException notFoundKernelException = assertThrows( typeof( IndexNotFoundKernelException ), sampler.sampleIndex );
			  assertEquals( notFoundKernelException.Message, "Index dropped while sampling." );
		 }

	}

}